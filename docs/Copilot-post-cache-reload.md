Direct SQL upserts into BlogEngine tables do not update the in-memory post cache used by `Post.Posts` and `Post.ApplicablePosts`.

The admin "Clear blog cache" action does not rebuild post visibility state, so it does not make newly upserted posts appear in `/api/posts`, the admin post list, or slug routes.

`Post.Reload()` is required to rebuild the post cache for the target blog instance.

The BlogAI application-side fix is:

`POST /api/posts/reload/{blogId}`

The SQL upsert script does not live in this repository. It is owned by the `vs-mcp-bridge` repository, which contains the repo-backed blog publishing scripts and source content.

Required publish sequence:

1. Run the SQL upsert from the `vs-mcp-bridge` publishing script.
2. Call `POST /api/posts/reload/{blogId}`.
3. Verify the post via `/api/posts` and the BlogEngine admin UI.
