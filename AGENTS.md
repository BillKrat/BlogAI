# Workspace Instructions

## Skill Routing

- When the user message matches an installed skill name or a known
  skill trigger phrase, follow that skill's workflow before improvising
  an ad hoc one.
- For session-start requests such as "let's code," "let's get
  started," "pick up where we left off," or "where were we," route to
  `lets-code`.
- For skill-discovery requests such as "help," "what skills are
  available," "list skills," or "which skill should I use," route to
  `help-skills`.
- For new context-bootstrap requests such as "initialize context," "set
  up project context," "bootstrap context," or "prepare this repo for
  lets-code/update-context," route to `initialize-context`.
- For session-end requests such as "update context," "save the
  session," "checkpoint this," or "wrap up," route to
  `update-context`.
- For skill-backup requests such as "sync skills," "backup skills," or
  "restore skills," route to `sync-skills`.

## Shared Skill Behavior

- Follow the standardized repo-context workflow used by the skills:
  prefer `.project-context/`, fall back to legacy root-level
  `INDEX.md` + `Sessions/` + `Topics/`, and only use lightweight git
  orientation when no context structure exists.
- Keep skills repo-agnostic. Do not hardcode machine-specific paths,
  repo names, GitHub owners, or one project's folder layout unless the
  task explicitly requires it.
- For skill backup and restore, derive the namespace from the directory
  that owns `.claude/skills/` rather than from a fixed path.
- When instructions and a skill cover the same behavior, let the skill
  define the detailed procedure and keep the instruction file limited to
  routing and consistency.

## Reuse Standard

- Changes in this repo should stay reusable across `ai-skills`,
  `architecture-rosetta-stone`, `BlogAI`, and `vs-mcp-bridge`.
- Prefer updating the shared convention over adding per-repo special
  cases.

## BlogAI-Specific Note (added 2026-07-19)

This repo has neither `.project-context/` nor the legacy `INDEX.md` +
`Sessions/` + `Topics/` layout the shared skills above expect — `lets-code`/
`update-context` will correctly report "context not initialized" here per
their own documented fallback. Unlike `vs-mcp-bridge`, there is no
clearly established alternative convention to defer to instead: `docs/
session-handoffs/` has only one old entry (2026-04-19), and this repo's
own `README.md` says the project is "on hold." Before running
`initialize-context` here, decide explicitly whether to build on the
existing `session-handoffs/` precedent or adopt the shared
`.project-context/` convention — don't default to either silently.