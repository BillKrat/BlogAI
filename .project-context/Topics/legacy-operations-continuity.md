# Legacy Operations Continuity

status: active
last-updated: 2026-08-14

## Scope
Keep the legacy BlogAI/BlogEngine solution buildable and deployable with minimal change while rewrite planning continues.

## Current State
- Repository is now hosted at `https://github.com/BillKrat/BlogAI` (private).
- Solution build is currently successful on the new machine after dependency restore.
- `Web.config` is excluded from git for local secret handling.
- `Web.config.example` exists as a baseline XML-provider template.
- Session-context workflow is now initialized via `.project-context/` and `AGENTS.md` routing.

## Deployment Direction
- Target deployment flow: GitHub-driven deployment to SmarterASP on pushes to `master`.
- Next setup step is to add CI/CD workflow and host credentials via GitHub secrets.

## Next Objective
Implement and validate GitHub-to-SmarterASP deployment automation for `www.adventuresOnTheEdge.net` on `master` pushes.
