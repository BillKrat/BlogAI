using Adventure.Common;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Web.Http;

public class NewsFeedController : ApiController
{
    private readonly NewsFeedLogic _logic = new NewsFeedLogic();

    /// <summary>
    /// Returns the dashboard news/help feed items as JSON (title, link, description,
    /// publish date), matching the shape every other admin API endpoint returns so
    /// Angular's dataService.getItems can bind directly to it without XML parsing.
    /// </summary>
    public HttpResponseMessage Get()
    {
        try
        {
            var feed = _logic.ToSyndicationFeed(_logic.ReadCurrentBlogList());

            var items = feed.Items.Select(item => new
            {
                Title = item.Title != null ? item.Title.Text : string.Empty,
                Link = item.Links.Count > 0 ? item.Links[0].Uri.ToString() : string.Empty,
                Description = item.Content is TextSyndicationContent ? ((TextSyndicationContent)item.Content).Text : string.Empty,
                PublishDate = item.PublishDate.UtcDateTime
            }).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }
        catch (Exception ex)
        {
            BlogEngine.Core.Utils.Log("Dashboard news feed", ex);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
        }
    }
}

