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
- End each session with all intended work and AI context committed so the repository can always be restored to a known working state.
- BlogAI planning is architecture-first: build a decoupled, reusable ASP.NET Core platform with dependency injection, MEF plugin support, and a secure authentication-first MVP before feature replication.

## Session documentation model
- `AI_Start.md` is the durable entry point. Keep stable project purpose, approved architectural direction, non-negotiable working rules, key file locations, and safe resumption guidance here.
- `AI_Stop.md` is the latest session handoff. Use it to record what changed in the session, what was validated, what remains open, and the exact next step.
- Keep detailed design, architecture notes, diagrams, logs, and runbooks under `docs/` and related artifact folders rather than duplicating them in the root documents.
- Use Git commits as the authoritative checkpoint for completed work. Before ending any session, document the handoff, commit the intended state with a `Copilot:` commit message, and push it when the branch is ready so the session is preserved off-machine.
- When the user indicates a session is ending, prepare the closing documentation automatically using this split unless the user asks for a different format, then commit and push the intended session slice.

## Immediate goals / tasks
1. Preserve the approved BlogAI planning direction in the session documents and keep the next-session prompt aligned with that plan.
2. Plan a new ASP.NET Core application and reusable supporting framework that can run beside BlogEngine.NET without direct `System.Web` coupling.
3. Prioritize a secure authentication-capable website shell before feature replication.
4. Validate hosting options for `https://api.global-webnet.com` first, with `https://www.global-webnet.com/BlogAi` treated as a later path-based deployment option if hosting support is confirmed.
5. End each session slice with closing documentation, a `Copilot:` commit, and a push once the work is functional and documented.

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
Resume from `AI_Start.md`, then review `AI_Stop.md` and the linked runbooks/artifacts in `docs/` before making new changes. Avoid duplicating prior findings. For BlogAI planning, keep `AI_Start.md` focused on durable direction and let `AI_Stop.md` capture only the most recent checkpoint and next step.

## Approved BlogAI planning direction
- Build a new ASP.NET Core application as a side-by-side app, not as code embedded inside the legacy BlogEngine.NET process.
- Separate reusable framework concerns from BlogAI-specific host concerns so future applications can reuse the same platform.
- Use ASP.NET Core dependency injection for core composition and MEF for plugin discovery/extensibility seams.
- Treat security and authentication as the first deliverable; feature parity with BlogEngine.NET is explicitly deferred.
- Prefer `https://api.global-webnet.com` for the initial low-risk deployment target. Consider `https://www.global-webnet.com/BlogAi` later if SmarterASP child-application support is confirmed.
- Use explicit service boundaries for shared database and file access. Do not directly couple the new app to legacy `System.Web` runtime code.

## Contact/maintainer notes and environment-specific considerations
- Shared hosting environments such as SmarterASP.net may not support every ASP.NET secret-management feature; prefer external config files or control-panel-managed settings that stay out of source control.
- Treat production and local credentials as separate values.
- Do not commit publish profiles containing passwords or tokens.
- Push the end-of-session checkpoint after the implementation or planning slice is documented, committed, and ready to preserve off-machine.
