# BlogAI

BlogAI is a fork of the open-source [BlogEngine.NET](https://github.com/BlogEngine/BlogEngine.NET)
platform, a lightweight, extensible blogging engine built on ASP.NET (.NET Framework 4.8),
with supporting components targeting .NET Core 2.1. It provides the core blogging
features you'd expect — posts, pages, comments, themes, widgets, and a plugin/extension
model — along with pluggable data providers (XML, SQL Server, MySQL, SQLite, SQL CE) so
it can run against a variety of storage backends.

This repo is set up for local development and experimentation. See the setup
instructions under `BlogEngine/BlogEngine.NET/setup/` for provider-specific configuration,
and review the security notes below before deploying anywhere beyond your local machine.

## Security & Public Release Notes

This repository contains sample/template configuration and data files used for local
development and setup. Before deploying or using this project in any environment
beyond local testing, take the following steps:

- **Never commit `App_Data` or runtime config with real credentials.** The files under
  `BlogEngine/BlogEngine.NET/App_Data/**` and `BlogEngine/BlogEngine.NET/Web.config`
  are excluded via `.gitignore` and should only ever contain sanitized placeholder
  values in source control.
- **Regenerate the ASP.NET `machineKey`** (`validationKey` / `decryptionKey`) in your
  own `Web.config` before deployment. The values shipped in `Web.config.example`,
  `Web.Config.SQL`, and the files under `setup/*` are placeholders
  (`CHANGE_ME`) and must never be used in a real deployment.
- **Rotate admin credentials before production use.** The default admin user/password
  hash and profile data in `App_Data/users.xml` and `App_Data/profiles/admin.xml` are
  placeholders and must be replaced with your own values before running the site.
- **Purge secrets from git history before making a private repository public.** If any
  real secrets, credentials, or machine keys were ever committed to this repository's
  history, they must be removed from history (e.g., using `git filter-repo` or the BFG
  Repo-Cleaner) prior to publishing, since sanitizing the current file contents alone
  does not remove values from prior commits.

## Links

- [AGENTS.md](AGENTS.md) - AI context index: repo state, per-AI sections, docs index.
- [SmarterASP secret configuration](docs/Copilot-smarterasp-secrets.md) - configure and deploy `connectionStrings.config` and `appSettings.secrets.config` for SmarterASP.net.
- [BlogEngine website](https://blogengine.io/)
- [BlogEngine getting started docs](https://blogengine.io/support/get-started/)
- [BlogEngine themes](https://blogengine.io/themes/)
- [BlogEngine custom design theme](https://blogengine.io/themes/custom/)
- [BlogEngine support](https://blogengine.io/support/)
