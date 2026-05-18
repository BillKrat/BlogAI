# SmarterASP Secret Configuration

## Purpose
Document the BlogAI configuration steps required from this point forward for deploying the BlogEngine database connection string and `BlogEngine.ReloadEndpointKey` to SmarterASP.net without committing secrets to source control.

## Scope
This document assumes the developer has already completed the existing BlogEngine setup and deployment steps. It only covers the additional BlogAI secret-management pattern introduced in this repository.

## Files involved
The web application now reads sensitive values from external files located next to `BlogEngine/BlogEngine.NET/Web.Config`.

Required server-side files:
- `connectionStrings.config`
- `appSettings.secrets.config`

Sample templates in source control:
- `BlogEngine/BlogEngine.NET/connectionStrings.config.example`
- `BlogEngine/BlogEngine.NET/appSettings.secrets.config.example`

Do not commit the real secret files.

## Local preparation
1. Copy `BlogEngine/BlogEngine.NET/connectionStrings.config.example` to `BlogEngine/BlogEngine.NET/connectionStrings.config`.
2. Copy `BlogEngine/BlogEngine.NET/appSettings.secrets.config.example` to `BlogEngine/BlogEngine.NET/appSettings.secrets.config`.
3. Replace the placeholder values with local development secrets.
4. Build and run the site locally to verify the configuration is valid before deployment.

## connectionStrings.config format
Store the production SQL Server connection string in `connectionStrings.config`.

Example:

```xml
<?xml version="1.0" encoding="utf-8"?>
<connectionStrings>
  <clear />
  <add
	name="BlogEngine"
	connectionString="Data Source=YOUR_SQL_HOST;Initial Catalog=YOUR_DATABASE;User ID=YOUR_SQL_LOGIN;Password=YOUR_SECRET_PASSWORD;Encrypt=False;TrustServerCertificate=False;"
	providerName="System.Data.SqlClient" />
</connectionStrings>
```

Notes:
- Use the SQL Server host, database name, SQL login, and password provided by SmarterASP.
- Use a production-specific SQL login instead of sharing local credentials.
- Rotate the password immediately if it is ever exposed in source control, logs, screenshots, or email.

## appSettings.secrets.config format
Store the reload endpoint key in `appSettings.secrets.config`.

Example:

```xml
<?xml version="1.0" encoding="utf-8"?>
<appSettings>
  <add key="BlogEngine.ReloadEndpointKey" value="REPLACE_WITH_A_LONG_RANDOM_SECRET" />
</appSettings>
```

Key guidance:
- Generate a long random value.
- Do not reuse database passwords, admin passwords, or other shared secrets.
- Treat this as a production secret and rotate it if exposure is suspected.

## Deploying to SmarterASP.net
Use the external config file pattern as the primary deployment method.

1. Publish or copy the application to SmarterASP as usual.
2. In the deployed application folder, locate the same directory that contains `Web.Config`.
3. Upload `connectionStrings.config` with the production database connection string.
4. Upload `appSettings.secrets.config` with the production `BlogEngine.ReloadEndpointKey`.
5. Confirm both files remain on the server and are not included in Git commits.
6. Restart the application from the SmarterASP control panel if needed, or recycle the application by touching `Web.Config` only if a restart mechanism is unavailable.
7. Browse the site and validate that the home page and admin login both work.

## Finding the SmarterASP database values
The exact SmarterASP control panel wording can change, but the required values are typically available in the hosting portal under the SQL Server or database management area.

Collect these values:
- SQL Server host or server name
- Database name
- SQL login username
- SQL login password

Place those values into `connectionStrings.config`.

## Rotation procedure
If a secret changes or is suspected to be exposed:
1. Generate a new SQL password or reset the SQL login password in SmarterASP.
2. Update the server copy of `connectionStrings.config`.
3. Generate a new long random `BlogEngine.ReloadEndpointKey`.
4. Update the server copy of `appSettings.secrets.config`.
5. Restart the application and verify the site.
6. If the old secret ever existed outside a protected local file, treat the rotation as mandatory.

## Validation checklist
After deployment, verify:
- The site loads without configuration errors.
- Admin login works.
- Database-backed content loads correctly.
- The external secret files exist on the server next to `Web.Config`.
- No real passwords or keys were added to commits, publish profiles, or documentation.

## Repository rules
- Commit only the `.example` files.
- Keep the real `connectionStrings.config` and `appSettings.secrets.config` local or server-only.
- End each work session with all intended changes committed so the repository can be restored to a known working state.
- Push only after the code is functional and the related documentation is complete.
