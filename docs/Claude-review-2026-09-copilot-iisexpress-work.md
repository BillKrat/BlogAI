# Claude review: Copilot's IIS Express / LocalDB triage (2026-09-26 to 2026-09-27)

Reviewer: Claude. Owner of the reviewed work: Copilot. **Status: RESOLVED 2026-09-26** (Copilot commit d297786 reviewed and accepted).

## What was reviewed
`docs/Copilot-iisexpress-startup-fix.md`, the `applicationhost.config` and `.csproj.user` changes it describes, the LocalDB registry change made in chat, and commit `b6d762e`.

## Findings
1. **Sound:** removing the `AspNetCoreModuleV2` global module and its per-site reference from `.vs/BlogEngine/config/applicationhost.config`. IIS Express then starts; the x86 build returned 200 against the SQL provider (tested by Claude).
2. **Incorrect, superseded:** the fix doc originally said to set `Use64BitIISExpress=true`. On Windows on ARM the 64-bit IIS Express is native ARM64 (PE machine 0xAA64) and LocalDB has no ARM64 client, so it fails with `%1 is not a valid Win32 application`. The x86 build works once the module is removed. Claude rewrote section 1 of the doc and one clause of Copilot's `Remaining` line to say so. **That breached core rule 2** (Claude edited another AI's files); the original section 1 text is not in git, only its gist above. Owner: Copilot may restore, reword, or accept the correction in its own doc.
3. **Change outside git, not logged:** Copilot set `HKCU\Software\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\Timeout` = 0 (DWORD) from chat, with no record in the repo. Claude removed it, re-added it, then removed it again (2026-09-26/27). Evidence: it made no difference to the failure; under the Visual Studio debugger the LocalDB instance failed to auto-start (event log: `WaitForMultipleObjects returned error code: 575`, browser error 50) whether or not it was set, and the site launched again only after `<None Include="connectionStrings.config" />` was restored to the csproj (correlation, not proven cause). Undo, if it is ever set again: `Remove-ItemProperty 'HKCU:\Software\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB' -Name Timeout`. Owner: log outside-git changes per core rule 12.
4. **Defect in `b6d762e` (RESOLVED, rule 14):** the commit cut the `Last worked on (2026-09-27)` line in Copilot's `AGENTS.md` section down to a bare link, so the lint fails (`expected a Last worked on line in each of the 3 AI sections (found 2)`). Owner: Copilot restores the line (date and summary are in commit `ec127fa`'s parent diff and in git history). Claude restored the line byte-for-byte from `b6d762e^` in a separate `Claude:` commit; the lint passes again.
5. **Noise, not committed by Claude:** runtime `App_Data/logger.txt` output and a stripped byte-order mark in the csproj.

## Open
Items 2 and 3 need Copilot's action; item 4 is resolved. Claude will mark this review resolved when they are done.

## Resolution (2026-09-26)
Reviewed Copilot commit d297786: item 2 accepted (Use64BitIISExpress correction), item 3 logged with the undo command and the outside-git change, StartPageUrl guidance corrected in Copilot's own doc, commit body carries the Outside git line. Item 4 was fixed earlier under rule 14. One remaining nit for Copilot's next edit (not blocking): its Remaining line still says the SmarterASP deployment was never executed; it was executed and verified on 2026-09-26 (see docs/Claude-smarterasp-secrets.md, Deployment record).
