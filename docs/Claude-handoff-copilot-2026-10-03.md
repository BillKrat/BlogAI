# Handoff to Copilot: fix the findings from Claude's 2026-10-03 review

Source: [Claude-review-2026-10-copilot-page-wiki-helpfeed.md](Claude-review-2026-10-copilot-page-wiki-helpfeed.md). Branch: `claude-branch-cleanup-review`. The human authorized all items below ("have Copilot resolve"). Claude re-reviews afterwards.

## Ground rules

- Read `AGENTS.md` first. Commit each numbered item separately: subject `Copilot: ...`, body ends `Outside git: none` (or the real line) and the Co-authored-by line. Do not push; only Claude pushes.
- **Hard limits:** no NuGet or assembly upgrades. **No history rewrite, no force-push** (item 1 is untracking only). Do not touch the live site or deploy.
- Build the full solution after each code item (`BlogEngine.sln`). Report any failure plainly.
- When done, update your own AGENTS.md section (`Last worked on` / `Remaining`) and list below what you did and any item you skipped, with why.

## Items

Do 1 first, then 2 to 8 in any order.

### 1. Stop tracking local runtime and dev-blog data (secrets hygiene)
- Add to `.gitignore`: `BlogEngine/BlogEngine.NET/App_Data/logger.txt` and `BlogEngine/BlogEngine.NET/App_Data/blogs/bloghelp/`.
- `git rm --cached` those paths (files stay on disk). Do **not** delete them from disk and do **not** rewrite history.
- Check first whether `App_Data/blogs.xml` (the registry that now lists `bloghelp`) should stay tracked. If it must, add a one-line note to your research file that a fresh clone has a registry entry for a missing blog folder, and confirm a clean clone still starts (the app should tolerate it, or `EnsureBlogIsTracked`/startup should skip a missing folder). If it does not tolerate it, tell the human; do not guess.
- Acceptance: `git status` clean after the commit, `git ls-files | findstr bloghelp` empty, app still runs locally and the help blog still resolves.

### 2. `NewsFeedController`: stop returning exception detail
- In `BlogEngine.NET/AppCode/Api/NewsFeedController.cs`, replace `Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex)` with a generic message string (no exception object). Keep `Utils.Log("Dashboard news feed", ex)`.
- Acceptance: a forced failure returns a generic 500 body; the details appear only in the log.

### 3. Document the `Response.Redirect(url, false)` change
- Files: `BlogEngine.Core/Services/Security/Security.cs` (about line 197) and `BlogEngine.Core/Web/Controls/BlogBasePage.cs` (about line 222).
- Read the code after each call and confirm nothing after it runs that should not. If the change fixed a specific symptom (ThreadAbortException under the debugger?), say which in a new dated `docs/research/Copilot-2026-10-YY-response-redirect.md` and link it from `Copilot-research-index.md`. If you cannot justify it, revert only those two lines and say so.

### 4. `Utils.cs`: separate the real change from formatting noise
- Do not rewrite history. Add a short entry to a research file stating that the only functional change in `Helpers/Utils.cs` is the commented-out App_Code/App_SubCode assembly loading (about lines 459-472), and why it is safe (extensions still load; the wiki extension works).
- Going forward: formatting-only edits go in their own commit.

### 5. Delete the orphaned duplicate
- `git rm BlogEngine/BlogEngine.NET/AppCode/Logic/NewsFeedLogic.cs` (old remote-fetch version; not in the csproj; same class name as `Adventure.Common/NewsFeedLogic.cs`). Confirm with a grep that nothing references it and the solution builds.

### 6. Fix the broken wiki test
- `BlogEngine.Wiki.Test` `GwnWikiExtensionFixture.CanLoadExtension` fails because of the `TestResults` path bug in `UnitTestHelper.GetBlogEnginePath`. Fix the path resolution so it finds the BlogEngine.NET folder regardless of where the test runner's output directory is. Acceptance: the test passes. Do not weaken the assertion.

### 7. Remove the dead code in `Gallery.GetPackageExtras`
- In `BlogEngine.Core/Services/Packaging/Gallery.cs`, the body after `return null; // no longer supported` is unreachable (compiler warning CS0162). The human added the short-circuit; keep its behavior (returns null, no network call) but delete the dead body and the now-unused `LogGalleryFailure("GetPackageExtras", ...)` call path. Check callers (`Gallery.cs` line 25 and `PackageExtraController`) still handle null. Acceptance: build has no CS0162 for that file.

### 8. Make the docs index complete
- `Claude-check-context.ps1` flags your two new files as orphans: `docs/research/Copilot-2026-10-03-gallery-feed-silent-failure.md` and `docs/research/Copilot-2026-10-03-feed-settings-and-newsfeed.md`. Add a one-line row for each (and for any new research file from items 3 and 4) to the **Docs index** table in `AGENTS.md`. Keep `AGENTS.md` under 150 lines.
- Run `powershell -File ..\docs\Claude-check-context.ps1` and confirm no `[BlogAI]` lines fail. Ignore `[(root)]` and `[ai-research-blog]` failures; those are Claude's.

## Help feed in-browser check (human, not Copilot)

When the human is back: set Settings > Feed > Help feed to the `bloghelp` blog, load the dashboard, confirm the news list shows its posts; clear the setting and confirm it falls back to the current blog's posts. Copilot should not mark the feature verified until the human says so.
