# BlogAI

BlogAI is a fork of [BlogEngine.NET](https://github.com/BlogEngine/BlogEngine.NET) 3.3.6, a blogging engine on ASP.NET (.NET Framework 4.8): posts, pages, comments, themes, widgets, and an extension model, with pluggable storage (XML, SQL Server, and others under `BlogEngine/BlogEngine.NET/setup/`). It runs live at https://adventuresontheedge.net on SQL Server.

**Status: legacy.** It is maintained only until its replacement, [`ai-research-blog`](https://github.com/BillKrat/ai-research-blog), is ready. Expect fixes, not features.

## Quick start (no SQL, no secrets)

The checked-in `BlogEngine/BlogEngine.NET/Web.Config` uses the XML provider, so a fresh clone runs with no database and no credential files.

1. Open `BlogEngine.sln` in Visual Studio and run `BlogEngine.NET` (IIS Express), or build with MSBuild and point IIS at `BlogEngine/BlogEngine.NET`.
2. Browse to the site. Content lives in `BlogEngine/BlogEngine.NET/App_Data` (XML files).
3. Sign in with the stock BlogEngine admin account from `App_Data/users.xml` and **change its password immediately**; it is a well-known default.

If you launch IIS Express by hand, give `/path:` as a native Windows path (`M:\...`), from PowerShell or cmd. Git Bash rewrites it and every request returns 404.

## SQL Server storage

1. Create an empty database and run, in order, `BlogEngine/BlogEngine.NET/setup/SQLServer/Setup.sql` and then `BlogEngine/BlogEngine.NET/setup/BillKrat-Upgrade.2026.09.26.sql` (adds the `be_Users.Comment` column this fork's code needs; idempotent). For a local test database, LocalDB works: `sqlcmd -S "(localdb)\MSSQLLocalDB" -d <db> -i <script>` (no `-f` option; the scripts are read as-is). `BillKrat-Upgrade.2018.12.30.sql` is the optional GwnWiki extension.
2. Copy `BlogEngine/BlogEngine.NET/Web.Config.SQL` over `Web.Config`.
3. Create these git-ignored files next to it from the `.example` templates:
   - `connectionStrings.config`: your connection string (LocalDB: `Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=<db>;Integrated Security=True;`).
   - `appSettings.secrets.config` (optional): `BlogEngine.ReloadEndpointKey`, which protects `POST /api/posts/reload/{blogId}` (see [docs/Copilot-post-cache-reload.md](docs/Copilot-post-cache-reload.md)).
4. Sign in with the stock `admin` / `admin` and change it immediately.

`Web.Config.SQL` contains no credentials and no `machineKey`. Do not commit a `Web.Config` that holds real values (`git update-index --skip-worktree` on `Web.Config` while testing keeps it out of commits).

**Windows on ARM:** run the site under the 32-bit (x86) IIS Express. The csproj sets `Use64BitIISExpress` to `false`; keep it that way (the Visual Studio toggle Tools > Options > Projects and Solutions > Web Projects rewrites it). SQL Server LocalDB ships x86 and x64 client libraries but no ARM64 one, so an ARM64 IIS Express fails with `Unable to load the SQLUserInstance.dll` / `%1 is not a valid Win32 application`. SmarterASP also runs this site in a 32-bit pool.

## Security notes

- **Secrets stay out of git.** Only the `.example` templates are committed. Never commit real connection strings, keys, or publish profiles with passwords.
- **`machineKey`:** the repo config has none, so ASP.NET generates one per app. If you set your own, keep it on the host only. Two keys for the live site were committed in May 2026 and remain in public history; they are treated as compromised and must not be reused (details: [docs/Claude-smarterasp-secrets.md](docs/Claude-smarterasp-secrets.md)).
- **Admin credentials:** replace the default admin account before any use beyond local testing.
- **Errors:** `customErrors` is `RemoteOnly`; do not set it to `Off` in production.
- `App_Data` holds the sample XML site content; do not put real user data or credentials there and commit it.

## Deployment

Deploying to SmarterASP.net is documented in [docs/Claude-smarterasp-secrets.md](docs/Claude-smarterasp-secrets.md), driven through the SmarterASP MCP tools.

## Links

- [AGENTS.md](AGENTS.md): AI context index (repo state, per-AI sections, docs index).
- [BlogEngine website](https://blogengine.io/) · [getting started](https://blogengine.io/support/get-started/) · [themes](https://blogengine.io/themes/) · [support](https://blogengine.io/support/)
