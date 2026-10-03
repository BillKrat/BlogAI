# Feed settings and the dashboard news feed — findings (2026-10-03)

## Question

Do the existing Settings > Feed fields (`Endorsement` / "bLink" and `AlternateFeedUrl`) already serve,
or could they be repurposed for, the new "help/tutorials blog feed" concept the human described: a
tenant admin designates a specific org/blog as the source of in-product help content, and
`NewsFeedController` on the dashboard pulls that blog's posts as a syndication feed.

## What `Endorsement` ("bLink") actually does

- Model: `BlogSettings.cs` line 956, `Data/Models/Settings.cs` line 119.
- Only consumer: `BlogEngine.Core/Services/Syndication/SyndicationGenerator.cs` lines 912-920 (RSS) and
  1084-1092 (Atom). When non-empty, it writes a `<blogChannel><blink>` element into **this blog's own
  outgoing** RSS/Atom feed XML — the ~2003-era "blogChannel" extension's way of saying "here is a blog
  I endorse/recommend." Pure outbound metadata, read by external RSS aggregators. BlogEngine itself
  never reads this URL back in.
- UI: `admin/app/settings/feedView.html` line 18-19, bound to `settings.Endorsement`.

## What `AlternateFeedUrl` actually does

- Model: `BlogSettings.cs` line 264, `Data/Models/Settings.cs` line 120.
- Consumer: `Helpers/Utils.cs` lines 124-132 (`Utils.FeedUrl`) — if set, `Utils.FeedUrl` returns this
  value instead of `{AbsoluteWebRoot}syndication.axd`. Used by `BlogBasePage.cs` (lines 159-167, the
  `<link rel="alternate">` header tags) and theme markup (`Custom/Themes/AllTuts/site.cshtml` line 84,
  the "Subscribe to our RSS Feed!" link) to point **visitors'** feed readers at a third-party feed host
  (classic FeedBurner-style redirect). Again outbound-only: it changes where *this blog's own* feed is
  advertised as living, it is not a source BlogEngine pulls content from.

## Verdict

Neither field is a "pull content from another blog" hook. Both only affect this blog's own outgoing
feed metadata/links. The human's new use case — a tenant-designated *source* blog whose posts populate
the `/api/newsfeed` dashboard panel — has no existing field to repurpose. Decision (human, 2026-10-03):
leave `Endorsement` and `AlternateFeedUrl` untouched; add a new setting on the same Settings > Feed tab.

## Dashboard `/api/newsfeed` wiring (the original "disconnect" question)

- `admin/app/dashboard/dashboardController.js` line 126 calls `$scope.loadNewsFeed()` on every
  dashboard load; line 154-164 does `dataService.getItems('/api/newsfeed', ...)` and copies the
  response into `$scope.news`.
- Routes via Web API convention to `NewsFeedController.Get()`
  (`AppCode/Api/NewsFeedController.cs`), which calls `Adventure.Common.NewsFeedLogic` (via its
  `using Adventure.Common;`) and serializes the resulting `SyndicationFeed` as **RSS/XML**
  (`application/rss+xml`), not JSON.
- `dashboardView.html` has no markup at all for `news` — only a spinner div (`#news-spinner`, lines
  133-135) that both the success and error handlers hide. No `ng-repeat` over `news` exists anywhere
  in the file. So even before the JSON/XML mismatch, there was never a visible list — the spinner just
  spins (briefly) and disappears.
- Two bugs stacked on top of each other here: (1) no view markup for `news` at all, (2) the controller
  returns XML while the Angular code expects to bind JSON fields directly (`item.title`, etc.) — the
  XML string would not even display sensibly if a naive `ng-repeat` were added.

## Orphaned duplicate file found

- `BlogEngine.NET/AppCode/Logic/NewsFeedLogic.cs` exists, declares `namespace Adventure.Common`, and is
  an byte-for-byte-equivalent *older* version of `Adventure.Common/NewsFeedLogic.cs` (still uses
  `BlogRollItem` + remote `XmlReader.Create(url)` — the version that originally threw the DTD
  exception). It is **not** included in `BlogEngine.NET.csproj` (`<Compile Include=...>` list has no
  reference to `AppCode\Logic\NewsFeedLogic.cs`), confirmed by grep against the csproj. It does not
  compile into the build and is dead weight. Flagged for the human/Claude to decide on deletion
  (rule 2 — this is Claude's/the human's call, not mine to delete unilaterally since it's outside my
  prior edit scope). **Status (2026-10-03, resolved for now): left untouched, still orphaned.**

## Implementation (2026-10-03) — resolved design + what was built

All three open questions from the first pass of this doc were resolved with the human via `ask_user`:

1. **Field name/placement**: `HelpFeedBlogId` (not `HelpFeedUrl` — renamed once the next decision made
   clear it is an internal reference, not a URL), added to the existing Settings > Feed tab.
2. **Fallback when unset**: falls back to the current blog's own posts (the post-DTD-fix behavior),
   not an empty feed.
3. **URL vs. internal blog reference** (the one still open at the end of the last session): human chose
   **internal blog reference** — a `BlogID` (Guid, stored as string) pointing at another blog instance
   within the *same* multi-tenant BlogEngine install, resolved directly via `Blog.GetBlog(id)` /
   `Post.ApplicablePosts` (using the existing `Blog.InstanceIdOverride` mechanism already used by
   `PostsController.Reload`), not an HTTP fetch of a remote syndication endpoint. Simpler, no network
   round-trip, no XML parsing — matches today's single-install reality and the tenant/org data model
   already baked into `Blog`/`BlogSettings` (`Blog.Blogs`, one BlogSettings instance per Blog).

Settings plumbing added (same pattern as `AlternateFeedUrl` throughout):
- `BlogSettings.HelpFeedBlogId` (string; the designated help blog's `Guid` as text, or empty/null).
- `Settings.HelpFeedBlogId` (API DTO) + `SettingsRepository.Get()`/`Update()` mapping (the "// feed"
  section, both directions).
- UI: `admin/app/settings/feedView.html` — a `<select>` bound to a new `vm.BlogList` lookup (not a free
  text field, since the value is a BlogID now) with a "None" option prepended in
  `settingController.js`'s `$scope.loadSettings()` to let the tenant admin clear it back to the
  current-blog fallback. Dropped the url-format validation rule in `settingController.js` that an
  earlier draft of this change had added (no longer applicable once it became a select).
- New lookup: `Lookups.BlogList` / `LookupsRepository.LoadBlogList()` — every active `Blog` instance
  (`Name` + `Id`), following the existing `LoadThemes()`/`LoadPages()` pattern.
- Label resource: `helpFeedUrl` key in `labels.resx` (kept this key name for the Settings field label
  despite the property rename, to minimize resx churn — the label text itself says "Help feed URL" and
  reads fine next to a blog picker) plus `needHelp` for the dashboard panel title. Both also added to
  `BlogCulture.cs`'s JS-fallback `AddJavaScriptResources()` list and `App_Data/labels.txt` (the primary
  source `BlogCulture` reads first — missing either file silently drops the key from `BlogAdmin.i18n`
  with no build error, so both must stay in sync with `labels.resx` going forward).

`Adventure.Common/NewsFeedLogic.cs` rewritten again: `ReadCurrentBlogList()` now resolves
`BlogSettings.Instance.HelpFeedBlogId` → `Blog.GetBlog(id)`; if found, active, and different from the
current blog, temporarily sets `Blog.InstanceIdOverride` to read that blog's `Post.ApplicablePosts`
(restored in a `finally`), otherwise falls back to the current blog's own posts exactly as before.
`ToSyndicationFeed` now derives feed-level title/link from the *posts' owning blog* (via
`Blog.GetBlog(post.BlogId)`) rather than always assuming `Blog.CurrentInstance`, since the posts may
now come from a different blog instance than the one serving the dashboard request.

`NewsFeedController.cs` rewritten to return **JSON** (`Title`/`Link`/`Description`/`PublishDate` per
item) instead of RSS/XML, matching every other admin API endpoint's shape and what
`dataService.getItems` + a plain `ng-repeat` expects — this was the second bug in the original
"disconnect," now fixed alongside the first.

`dashboardView.html`: added the missing `ng-repeat` over `news` (title + external link), replacing the
previously-flagged hardcoded "Click here to view tutorials" block (static `adventuresontheedge.net`
image/link) entirely — the panel is now titled via the new `needHelp` label and is empty-state aware
(`{{lbl.empty}}` when `news.length == 0`, matching the pattern used by the other dashboard panels on
this same page, e.g. `vm.Comments`/`vm.DraftPosts`).

## Still open

- The orphaned `AppCode/Logic/NewsFeedLogic.cs` — still not deleted; still the human/Claude's call.
- This feature currently only supports pointing at another blog *within the same BlogEngine.NET
  install*. The human's longer-term MEF/tenant vision describes multiple orgs/blogs under one tenant,
  which this install model already represents (`Blog.Blogs` = every blog instance), so no further
  design gap is known here — but if a future tenant spans multiple physically separate installs, this
  mechanism (internal `BlogID` lookup) would not reach across installs and the URL-fetch alternative
  would need revisiting then.
- Not yet validated at the browser/UI level (admin login, exercising the new Settings dropdown, and
  viewing the dashboard panel) — only `run_build` (full solution) was used to verify compilation.

