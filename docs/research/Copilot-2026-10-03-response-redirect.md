# Response.Redirect(false) and the `Utils.cs` assembly-loading note

## Redirect behavior and why `false` stays

The change in `BlogEngine.Core/Services/Security/Security.cs` and `BlogEngine.Core/Web/Controls/BlogBasePage.cs` keeps the explicit `Response.Redirect(url, false)` form instead of the default `Response.Redirect(url)`.

This is backed by the ASP.NET behavior behind the debugger symptom: the overload with the second parameter set to `true`/default triggers an `End`-style request termination, which under the debugger often surfaces as a `ThreadAbortException` while the response is being torn down. `Response.Redirect(url, false)` still sends a redirect response, but it does not immediately abort the request. That makes it the right choice for a redirect that is part of an already-completing request flow.

Both call sites are safe to keep this way:

- `Security.cs` redirects to the login page or return URL after authentication succeeds; nothing meaningful runs afterward.
- `BlogBasePage.cs` redirects after a post-delete flow; no further page logic should run once the redirect is issued.

The result is a cleaner response without the debugger-only abort exception, while preserving the redirect semantics.

## `Utils.cs` change tracking note

This is the exact review note for item 4: the only functional change in `BlogEngine.Core/Helpers/Utils.cs` is the commented-out `App_Code` / `App_SubCode` assembly-loading block around lines 459-472. It is safe to keep the code commented out because the runtime still loads the compiled extension assemblies through `GetCompiledExtensions()`, and the wiki extension continues to load and work as expected. This code path is obsolete in .NET 4.0+, so leaving it disabled removes dead/unsupported behavior without harming the current extension model.

## Blog registry note for the `bloghelp` dev blog

`App_Data/blogs.xml` still contains the `BlogHelp` registry entry. That is intentional and safe: `XmlBlogProvider.FillBlogs()` loads the registry entries from `blogs.xml` without validating that each `storageContainerName` folder exists on disk. Startup therefore tolerates a stale or missing blog folder entry; the app does not crash just because a registry record points to a folder that has not been created yet.
