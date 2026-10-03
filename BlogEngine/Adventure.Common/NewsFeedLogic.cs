using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using BlogEngine.Core;
using BlogEngine.Core.Data.ViewModels;

namespace Adventure.Common
{
    public class NewsFeedLogic
    {
        public List<BlogRollItem> ReadCurrentBlogList()
        {
            return new BlogRollVM().BlogRolls;
        }

        public SyndicationFeed ToSyndicationFeed(IEnumerable<BlogRollItem> blogList)
        {
            var items = new List<SyndicationItem>();

            foreach (var blog in blogList ?? Enumerable.Empty<BlogRollItem>())
            {
                if (blog == null || blog.FeedUrl == null)
                {
                    continue;
                }

                try
                {
                    using (var reader = XmlReader.Create(blog.FeedUrl.ToString()))
                    {
                        var remoteFeed = SyndicationFeed.Load(reader);
                        if (remoteFeed == null)
                        {
                            continue;
                        }

                        foreach (var item in remoteFeed.Items.Take(5))
                        {
                            items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.Log("Dashboard news feed", ex);
                }
            }

            var latest = items
                .OrderByDescending(item => item.PublishDate)
                .Take(10)
                .ToList();

            var siteUri = blogList != null
                ? blogList
                    .Where(blog => blog != null && blog.BlogUrl != null)
                    .Select(blog => blog.BlogUrl)
                    .FirstOrDefault()
                : null;

            var feed = new SyndicationFeed
            {
                Title = new TextSyndicationContent("Current Blog Feed"),
                Description = new TextSyndicationContent("Current blog list converted to a syndicated feed."),
                Items = latest
            };

            if (siteUri != null)
            {
                feed.Links.Add(new SyndicationLink(siteUri));
            }

            return feed;
        }
    }
}
