# SmarterASP.net deployment runbook (MCP-centric)

BlogAI is legacy. It is supported only until `ai-research-blog` replaces it, so this runbook uses the **SmarterASP MCP tools** wherever they exist and adds no other tooling. Site: **Adventures**, `siteId` `site-749862`, https://adventuresontheedge.net (Windows, ASP.NET 4.x, integrated pipeline, **32-bit** pool). Database `mssql-699378` (`db_a2cb58_adventure_1`). Verified via MCP 2026-09-26.

## What the MCP can and cannot do

| Need | MCP tool | Notes |
|---|---|---|
| Find the site, runtime, pool bitness | `websites_list`, `website_get_application_pool_settings` | read-only |
| Deploy state and recent errors | `deployment_get_status` | shows WebDeploy on/off and recent deploy errors |
| Web Deploy target (never the password) | `deployment_get_webdeploy_settings` | secret labels only: `SMARTHOST_WEBDEPLOY_USERNAME/PASSWORD` |
| Database host, name, login, connection-string template | `database_get_connection_settings` | never returns the password; template uses `${DB_PASSWORD}` |
| Restart after a config change | `website_restart` | needs an idempotency key |
| Health | `project_health_check` | site, DNS, SSL, runtime state |
| Follow a write to completion | `operation_get_status` | |
| Set or rotate a secret value | `website_set_environment_secret`, `database_rotate_credential`, `deployment_rotate_webdeploy_credential` | The value is a tool parameter, so the AI would see it. Instead the human copies it from the vault (`Claude-secret.ps1 clip <name>`, clipboard clears after 30 s) and pastes it into the SmarterASP panel field the AI names. The vault is the source of truth; SmarterASP mirrors it. |
| Upload files (config, app) | none | **No MCP file upload exists.** Substitute: `docs/Claude-ftp.ps1` in the workspace root (FTPS, dry run by default, auto-backup of the remote file, refuses a `Web.Config` holding inline secrets). The AI runs it through the vault, so the FTP password never reaches it. Uses the ROOT FTP user `billkrat-001` (path `/`) on `win8166.site4now.net:21`, explicit TLS, so remote paths start with the site folder (`Adventures/...`); the tool refuses folders other than `Adventures`, `api-global`, `global`, `mcp` unless `-AnyPath`. A secret can be rendered into a config template in memory (`-Also` on the vault `run`, `-TemplateEnv` on `put`), so it never touches disk. |

Some read tools that touch secrets (for example `website_list_environment_secrets`) can be refused by the session's auto-mode classifier. Do not work around a refusal; ask the human.

## Configuration model

- `Web.Config` (repo default): XML storage, no secrets. Runs from a clean clone.
- `Web.Config.SQL`: the SQL Server variant, checked in with **no credentials**. It reads `connectionStrings.config` and `appSettings.secrets.config` from next to itself. It has no `machineKey` (auto-generated per app on the host) and `customErrors` is `RemoteOnly`, `debug` is `false`.
- The deployed server `Web.Config` differs from `Web.Config.SQL` only by an inline connection string, an inline `machineKey`, and `customErrors="Off"` (compared 2026-09-26). Everything else is identical, so `Web.Config.SQL` is a faithful, credential-free equivalent.
- Real secret files are git-ignored and never committed: `connectionStrings.config`, `appSettings.secrets.config`. Templates: the `.example` files beside them.

## Procedure

1. **Verify the target (MCP, read-only).** `websites_list`, `deployment_get_status`, `website_get_application_pool_settings`, `database_get_connection_settings` (`mssql-699378`), `project_health_check`. Expect: site Active, WebDeploy enabled, `aspnet-4`, integrated, 32-bit. Investigate any recent `deploy Error` entries first (two were logged 2026-08-15).
2. **Store the secrets in the vault (human, once).** `Claude-secret.ps1 set ftp/win8166/billkrat-adventures -User billkrat-001` (holds the root FTP credential; rename to `ftp/win8166/billkrat-001` when convenient) and `set mssql/db_a2cb58_adventure_1/db_a2cb58_adventure_1_admin` (hidden prompt, or `-FromClipboard`). Then build `connectionStrings.config` locally from the `.example` and the MCP template, filling the password without the AI seeing it (`Claude-secret.ps1 usersecret`-style stdin write, or the human pastes it). Generate `BlogEngine.ReloadEndpointKey` as a long random value.
3. **Test locally (either).** Copy `Web.Config.SQL` over `Web.Config`, run under IIS Express (from PowerShell, or Visual Studio), confirm home page and admin login. Do not commit `Web.Config` while it holds real values (the repo default must stay the XML one).
4. **Deploy `Web.Config` safely (AI, via `Claude-ftp.ps1`).** Because the deployed file is the credential-free `Web.Config.SQL` content, there is no server-only `Web.Config` to protect and no round trip of pulling the server copy and undoing it. `put Web.Config.SQL -Remote Adventures/Web.Config` dry-runs, then with `-Apply` backs up the current server file to `M:\Dev\Websites\Adventures\_ftp-backup\<timestamp>\` and uploads. Upload `connectionStrings.config` the same way (first time only; it is never overwritten by a normal publish because it is not in the project). Application files: Visual Studio Web Deploy or the same tool. Never commit publish profiles with passwords.
5. **Restart and verify (MCP).** `website_restart`, then `operation_get_status`, `deployment_get_status`, `project_health_check`. Browse the home page and admin login; confirm database-backed posts render.
6. **Rotation (vault first).** New value into the vault (human), update the server file by redeploy, restart via MCP, verify. If a value was exposed, rotation is mandatory.

## Committed machineKey (resolved 2026-09-26)
A `machineKey` for this site was committed in May 2026 and sat in public history. Treat that key pair as burned. The repo no longer contains one. To rotate on the host: deploy a `Web.Config` without an inline `machineKey` (auto-generate), or a fresh key that exists only on the host. Users are signed out once. History is not rewritten; rotation makes the old key worthless.

## Validation checklist
- Site loads, admin login works, posts load from SQL Server.
- `connectionStrings.config` and `appSettings.secrets.config` exist on the server next to `Web.Config`.
- Server `Web.Config` has no inline connection string and no committed `machineKey`.
- `git status` shows no real values staged; `deployment_get_status` shows no new errors.
