# AI_Start

## Purpose
Provide the canonical starting point for AI and human sessions working on BlogAI.

## Project context
BlogAI is based on BlogEngine.NET and currently targets classic ASP.NET on .NET Framework 4.8. The active solution is `BlogEngine.sln`, and the primary web host is `BlogEngine/BlogEngine.NET`.

## Current state
- The web application reads sensitive values from external files instead of committing them in `BlogEngine/BlogEngine.NET/Web.Config`.
- For BlogAI work, never commit credentials, API keys, deployment secrets, or environment-specific tokens.
- Keep runtime secrets out of source control by using external config files or host-managed settings.
- The documentation trail starts in `README.md` and expands under `docs/`, with only `README.md`, `AI_Start.md`, and `AI_Stop.md` kept at the repository root for session continuity.
- End each session with all intended work committed so the repository can always be restored to a known working state.

## Immediate goals / tasks
1. Use the committed secret-management baseline and deployment documentation as the starting point for future work.
2. Gather deployment feedback after the documented SmarterASP steps are executed.
3. Plan how to begin the BlogAI rewrite of the current BlogEngine.NET application.
4. Push only after the code is functional and the related documentation is complete.

## How to run & test
- Open `BlogEngine.sln` in Visual Studio 2026 or later.
- Build the web project or the full solution in Debug.
- Required local secret files for the web app:
  - `BlogEngine/BlogEngine.NET/connectionStrings.config`
  - `BlogEngine/BlogEngine.NET/appSettings.secrets.config`
- Example local validation flow:
  1. Copy the sample secret config files.
  2. Add local-only values.
  3. Build the solution.
  4. Start `BlogEngine/BlogEngine.NET` and confirm the site loads and the admin area can authenticate.
- Deployment-specific secret instructions live in `docs/SmarterASP-Secrets.md`.
- Expected result: the web app starts without committed secrets in `Web.Config`, and the database-backed provider resolves settings from the external files.

## Important files & entry points
- `README.md` - lightweight index of project and documentation links.
- `AI_Start.md` - canonical session entry document.
- `AI_Stop.md` - prior-session handoff, validation record, and resumption notes.
- `docs/SmarterASP-Secrets.md` - SmarterASP deployment runbook for the external secret files.
- `docs/diagrams/smarterasp-secret-config-flow-20260518.mmd` - Mermaid diagram for the documented secret deployment flow.
- `docs/blog-drafts/20260518-blogai-security-hardening.md` - developer-facing summary of the security hardening work.
- `artifacts/20260518-build-validation.txt` - build validation summary for the current baseline.
- `BlogEngine/BlogEngine.NET/Web.Config` - main web application configuration.
- `BlogEngine/BlogEngine.NET/connectionStrings.config` - local/server-only database secrets file; do not commit.
- `BlogEngine/BlogEngine.NET/appSettings.secrets.config` - local/server-only sensitive app settings file; do not commit.
- `.gitignore` - Git protections for local secret files and publish artifacts.
- `docs/` - reusable documentation and follow-up artifacts.

## Resumption notes
Resume from `AI_Start.md`, then review `AI_Stop.md` and the linked runbooks/artifacts in `docs/` before making new changes. Avoid duplicating prior findings, and preserve the known-good state by committing all intended work at the end of each session.

## Contact/maintainer notes and environment-specific considerations
- Shared hosting environments such as SmarterASP.net may not support every ASP.NET secret-management feature; prefer external config files or control-panel-managed settings that stay out of source control.
- Treat production and local credentials as separate values.
- Do not commit publish profiles containing passwords or tokens.
- Push only when the implementation is functional and documented.
