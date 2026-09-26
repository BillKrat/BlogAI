# Live database schema vs a clean build (2026-09-26)

Structure only, no data. Method: `docs/Claude-sql-schema.ps1` in the workspace root (read-only catalog queries, password from the credential vault, never on a command line). Compared the live SmarterASP database behind the `Adventures` site (`mssql-699378`) with a clean local build: `Setup.sql` + `BillKrat-Upgrade.2026.09.26.sql`. Re-run the tool any time to refresh; no dump file is kept in git.

## Verdict
The 28 stock BlogEngine tables (`be_*`) match the clean build column for column. The only differences are the fork additions already understood, plus tables that belong to other apps sharing the same database.

- **Server:** SQL Server 2016 (13.0). No `STRING_AGG` (2017+). `FOR JSON` (used by `DbMembershipProvider`) is available.
- **Fork additions present live, absent from stock `Setup.sql`:**
  - `be_Users.Comment` is `varchar(max)` (the upgrade script now creates it as `varchar(max)` to match).
  - `Contact` (41 columns) matches `BillKrat-Upgrade.2018.12.30.sql`.
- **Other apps sharing this database (not BlogEngine's):** ASP.NET Identity (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims`), IdentityServer-style `PersistedGrants` and `DeviceCodes`, `__EFMigrationsHistory`, `Triples` and `TriplesCrossRef`, and `sysdiagrams`. Never drop or alter them from BlogAI scripts.
- **Minor drift:** `be_PackageFiles` has no primary key live, only a non-PK index named `PK_be_PackageFiles`. Harmless to the blog.
- **Also live:** 8 stored procedures, 0 views, 0 triggers, 13 foreign keys.

## What it means
- Deploying the new `Web.Config` only moves the connection string out of the file. It does not touch the schema, so the schema is not a deployment risk.
- A new SQL install needs three scripts in order: `Setup.sql`, `BillKrat-Upgrade.2018.12.30.sql`, `BillKrat-Upgrade.2026.09.26.sql`. Your local `BlogAI_Dev` had not yet run the 2018 script when compared.
