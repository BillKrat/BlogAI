# Gallery/extras remote feed: silent failure, dead domain, and dashboard log wiring

Triage of a `Newtonsoft.Json.JsonReaderException` ("Unexpected character encountered while parsing
value: <") seen breaking in the debugger inside `BlogEngine.Core.Packaging.Gallery.GetPackageExtras()`.

## Root cause

`BlogConfig.GalleryFeedUrl` (`BlogEngine.Core/BlogConfig.cs`) defaults to `http://dnbe.net/v01/nuget`
unless overridden by the `BlogEngine.GalleryFeedUrl` app setting. `Gallery.GetPackageExtras()` and
`Gallery.GetPackageExtra(id)` (`BlogEngine.Core/Services/Packaging/Gallery.cs`) each call
`WebClient.DownloadString` against `.../api/extras[...]` on that domain and `JsonConvert.DeserializeObject`
the result. The domain currently returns an HTML page (observed: a Portuguese gambling/SEO page), not
JSON — the legacy BlogEngine.NET gallery/extras service appears dead or the domain has been repurposed.
This is a remote-dependency failure, not a local bug; it cannot be fixed from this repo.

`Gallery.Load()` (used to populate the Packages/Gallery admin screen) already tolerates a null/empty
`extras` result (`if (extras != null && extras.Any())`), so the feature degrades gracefully functionally
— packages just show without download counts/ratings. The problem was total silence: both methods had
`catch (Exception) { return null; }` with no `Utils.Log` call, so admins had zero visibility that the
extras/ratings feature was non-functional (the exception only surfaced in the debugger with "break on
all exceptions" enabled).

`FileSystem.cs`'s `LoadThemes()`/`LoadWidgets()`/`LoadExtensions()` also call `Gallery.GetPackageExtra`
per installed item — so a single Packages page load can call it many times.

## Existing logging convention (precedent already in the same file)

`Gallery.RatePackage()` in the same file already does this correctly:
`catch (Exception ex) { Utils.Log("Error rating package", ex); return ex.Message; }`.
`Utils.Log(...)` (`BlogEngine.Core/Helpers/Utils.cs`) fires a static `OnLog` event; the only subscriber
anywhere in the codebase is `BlogEngine.NET/Custom/Extensions/Logger.cs`, which appends a line to a
per-blog `logger.txt` (gated by the `Logger` extension being enabled — it is, by default, per
`setup/SQLServer/Setup.sql`).

## How this reaches the dashboard (the "existing feature" referenced by the human)

`DashboardVM.Logs` (`BlogEngine.Core/Data/ViewModels/DashboardVM.cs`) is a plain property that returns
`GetLogFile()` — a full `ReadToEnd()` of `logger.txt`. This is what `dashboardController.js` binds to
(`vm.Logs`) and `dashboardView.html` displays in the `#modal-log-file` modal (opened via the warning-icon
button, visible whenever `vm.Logs.length > 0`).

Important nuance found during this triage: there are **two** separator-based parsers that split
`logger.txt` into discrete `SelectOption` entries on the literal string
`"*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*"` — `DashboardVM.GetLogs()` and `LogsController.GetLogFile()` (used
by `GET /api/logs/file`). Neither is actually wired to the dashboard's `vm.Logs`/modal — `DashboardVM.Logs`
uses the raw-text `GetLogFile()` instead, and nothing in `dashboardController.js` calls `/api/logs/file`.
Those two separator-parsing methods currently look like dead/orphaned code (no caller found), and the
separator is never written by `Logger.cs`'s writer. **Conclusion: no special format is needed** — any
`Utils.Log(...)` call reaches the dashboard modal as-is, appended as another line of raw text.

## Fix applied

Added a rate-limited (15 minutes per method, via a `ConcurrentDictionary<string, DateTime>`) private
`LogGalleryFailure` helper in `Gallery.cs` and call it from both `GetPackageExtra`'s and
`GetPackageExtras`'s catch blocks, logging the method name, the URL that failed, and the exception
message. Rate limiting avoids flooding `logger.txt` given `GetPackageExtra` can be called once per
installed theme/widget/extension on a single page load. `RatePackage`'s existing (unthrottled, user-
triggered, low frequency) logging was left as-is.

## Open items / not in scope for this fix

- The `dnbe.net` domain itself being dead/repurposed is outside this repo's control; `BlogConfig.GalleryFeedUrl`
  could in principle be repointed via the `BlogEngine.GalleryFeedUrl` app setting if a replacement
  gallery feed exists, but no such replacement is known at this time.
- `GetNugetPackages()` (`PackageRepositoryFactory.Default.CreateRepository(BlogConfig.GalleryFeedUrl).GetPackages()`,
  used by `Gallery.Load()`) was not confirmed to be failing the same way — only the two JSON `/api/extras`
  methods were examined and fixed this round. If the entire gallery is dead, `Load()`'s outer try/catch
  (already logs via `Utils.Log("BlogEngine.Core.Packaging.Load", ex)`) would catch it, so that path
  already has visibility.
- The two separator-parsing, apparently-unused methods (`DashboardVM.GetLogs()`,
  `LogsController.GetLogFile()`) were left untouched — they look like dead code from an earlier UI
  design, but removing dead code was out of scope for this triage.
