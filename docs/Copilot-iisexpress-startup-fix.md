# IIS Express startup fixes (BlogEngine.NET)

Two issues prevented `BlogEngine.NET` from running under IIS Express in VS 2026 Insiders on this machine.

## 1. Module-not-found (hr=8007007e)
`AspNetCoreModuleV2` was registered in the machine-local IIS Express config but only present in the 64-bit
install, so the 32-bit IIS Express failed to load it. The first attempted fix was `Use64BitIISExpress=true`.

**Corrected after review (2026-09-27):** do not use that. On Windows on ARM the "64-bit" IIS Express
(`C:\Program Files\IIS Express`) is a native ARM64 build (PE machine 0xAA64). SQL Server LocalDB ships x64 and x86
client libraries only, so the ARM64 process fails on the first database call with `%1 is not a valid Win32
application` / `Unable to load the SQLUserInstance.dll`. Tested against the cleaned `applicationhost.config`
(section 2): x86 IIS Express returns 200 with the SQL provider; the 64-bit one returns that 500. Once the
`AspNetCoreModuleV2` entries are removed (section 2) the x86 build starts fine, so the 64-bit switch is not needed.
The project keeps `Use64BitIISExpress` = `false` in `BlogEngine.NET.csproj` (also in the local `.csproj.user`).
## 2. IIS Express exiting immediately (exit code 0, "site can't be reached")
Root cause: the shared `.vs/BlogEngine/config/applicationhost.config` had `AspNetCoreModuleV2`
registered as a **global module** (loaded once for the whole IIS Express process at startup) and
wired into the default site `<modules>` collection — applied even to `BlogEngine.NET`, a classic
.NET Framework 4.8 app with no ASP.NET Core component. This likely came from config merging with
the solution's .NET Core 2.1 projects sharing the same IIS Express instance. Loading the ASP.NET
Core hosting module for a non-Core app pool crashed the whole IIS Express process before any site
could bind, with no exception surfaced to Visual Studio.

Fix: removed the `AspNetCoreModuleV2` entries (both the `<globalModules>` registration and the
per-site `<modules>` reference) from `applicationhost.config`.

Secondary: `BlogEngine.NET.csproj.user` had `AlwaysStartWebServerOnDebug=False` and an empty
`StartPageUrl`/`StartAction=CurrentPage`, which could stop the web server right after launch.
Set `StartPageUrl` to `http://localhost:64080/`, `StartAction=SpecificPage`, and
`AlwaysStartWebServerOnDebug=True`.

`.vs/` is machine-local and not committed; if this recurs on another machine or after a `.vs`
reset, re-apply the `applicationhost.config` module removal above.
