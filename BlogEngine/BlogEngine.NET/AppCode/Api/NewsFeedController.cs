using Adventure.Common;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web.Http;
using System.Xml;

public class NewsFeedController : ApiController
{
    private readonly NewsFeedLogic _logic = new NewsFeedLogic();

    public HttpResponseMessage Get()
    {
        try
        {
            // Legacy external feed behavior retained only for reference.
            // var items = new List<SelectOption>();
            // string url = "https://blogengine.io/news.xml";

            var feed = _logic.ToSyndicationFeed(_logic.ReadCurrentBlogList());
            var output = new MemoryStream();

            using (var writer = XmlWriter.Create(output, new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false,
                Indent = false,
                NewLineHandling = NewLineHandling.None
            }))
            {
                var formatter = new Rss20FeedFormatter(feed);
                formatter.WriteTo(writer);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(output.ToArray())
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/rss+xml");
            return response;
        }
        catch (Exception ex)
        {
            BlogEngine.Core.Utils.Log("Dashboard news feed", ex);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
        }
    }
}
