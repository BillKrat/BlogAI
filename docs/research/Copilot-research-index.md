# Copilot research index — BlogAI → ai-research-blog migration

Scratchpad-turned-reference for Copilot's investigation work during the migration. Each entry below is a
dated findings note: what was investigated, what the code actually does (not assumed), and open
questions for the human. Keep entries append-only; if a finding is later superseded, add a new entry
and note the supersession rather than editing history.

Format for a new entry file: `docs/research/Copilot-YYYY-MM-DD-<topic>.md`. One topic per file. Link it
here with a one-line summary the moment it's created (no orphans, per the repo's core guardrail 3).

## Entries

| File | Summary |
|---|---|
| [Copilot-2026-10-03-feed-settings-and-newsfeed.md](Copilot-2026-10-03-feed-settings-and-newsfeed.md) | How `Endorsement`/`AlternateFeedUrl` actually work (outbound-only, narrow legacy purposes); dashboard `/api/newsfeed` wiring and the disconnect between controller output and the (missing) view markup; an orphaned, non-compiled duplicate `NewsFeedLogic.cs` |
| [Copilot-2026-10-03-gallery-feed-silent-failure.md](Copilot-2026-10-03-gallery-feed-silent-failure.md) | `Gallery.GetPackageExtra[s]` silently swallowed a dead `dnbe.net` gallery domain's JSON failure; how `logger.txt`/`DashboardVM.Logs` actually wire to the dashboard log modal (two separator-parsing methods are dead code); the rate-limited log-and-bypass fix applied |
| [Copilot-2026-10-03-response-redirect.md](Copilot-2026-10-03-response-redirect.md) | Why `Response.Redirect(url, false)` is kept, the `Utils.cs` assembly-loading note, and the safe `bloghelp` registry note for the local dev blog |
