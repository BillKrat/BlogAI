using BlogEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;

namespace Adventure.Common
{
    public class NewsFeedLogic
    {
        /// <summary>
        /// Returns the posts that should populate the dashboard news/help feed.
        ///
        /// If the tenant admin has designated a "help" blog via
        /// <see cref="BlogSettings.HelpFeedBlogId"/> (Settings &gt; Feed), that blog
        /// instance's own posts are returned instead -- this is how a tenant can
        /// point every org/blog's dashboard at one central help/tutorials blog.
        /// Falls back to the current blog instance's own posts when unset, not
        /// found, or pointing at the current blog itself.
        ///
        /// Sourced entirely from in-process Post/Blog data (no external XML
        /// fetch), so this intentionally no longer touches remote blogroll feed
        /// URLs or their DTD/XML parsing concerns.
        /// </summary>
        public List<Post> ReadCurrentBlogList()
        {
            var helpBlog = GetHelpFeedBlog();

            if (helpBlog == null || helpBlog.Id == Blog.CurrentInstance.Id)
            {
                return ApplicablePublicPosts();
            }

            var previousOverride = Blog.InstanceIdOverride;
            try
            {
                Blog.InstanceIdOverride = helpBlog.Id;
                return ApplicablePublicPosts();
            }
            finally
            {
                Blog.InstanceIdOverride = previousOverride;
            }
        }

        /// <summary>
        /// Resolves the tenant-designated help blog from <see cref="BlogSettings.HelpFeedBlogId"/>,
        /// or null when unset or no longer a valid/active blog instance.
        /// </summary>
        private Blog GetHelpFeedBlog()
        {
            var blogId = BlogSettings.Instance.HelpFeedBlogId;
            if (string.IsNullOrWhiteSpace(blogId))
            {
                return null;
            }

            Guid id;
            if (!Guid.TryParse(blogId, out id))
            {
                return null;
            }

            var blog = Blog.GetBlog(id);
            return (blog != null && blog.IsActive) ? blog : null;
        }

        private static List<Post> ApplicablePublicPosts()
        {
            return Post.ApplicablePosts
                .Where(post => post != null && post.IsVisibleToPublic)
                .ToList();
        }

        public SyndicationFeed ToSyndicationFeed(IEnumerable<Post> postList)
        {
            var posts = (postList ?? Enumerable.Empty<Post>())
                .Where(post => post != null)
                .OrderByDescending(post => post.DateCreated)
                .Take(10)
                .ToList();

            var items = posts.Select(post => new SyndicationItem(
                post.Title,
                post.Description,
                post.AbsoluteLink,
                post.Id.ToString(),
                post.DateModified)
            {
                PublishDate = post.DateCreated,
                Authors = { new SyndicationPerson { Name = post.Author } }
            }).ToList();

            // The feed's own blog instance may differ from Blog.CurrentInstance when
            // sourced from a tenant-designated help blog (see ReadCurrentBlogList),
            // so derive the feed-level title/link/description from the posts' owning
            // blog instance rather than assuming the current one.
            var blog = posts.Count > 0 ? (Blog.GetBlog(posts[0].BlogId) ?? Blog.CurrentInstance) : Blog.CurrentInstance;

            var feed = new SyndicationFeed
            {
                Title = new TextSyndicationContent(blog.Name ?? "Current Blog Feed"),
                Description = new TextSyndicationContent(BlogSettings.Instance.Description ?? "Current blog posts converted to a syndicated feed."),
                Items = items
            };

            feed.Links.Add(new SyndicationLink(blog.AbsoluteWebRoot));

            return feed;
        }
    }
}
