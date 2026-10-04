using System;
using System.Text;
using BlogEngine.Core;
using BlogEngine.Core.Web.Controls;
using Resources;
using Page = BlogEngine.Core.Page;

/// <summary>
/// The page.
/// </summary>
public partial class page : BlogBasePage
{
    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event to initialize the page.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
    protected override void OnInit(EventArgs e)
    {
        var queryString = Request.QueryString;
        var qsDeletePage = queryString["deletepage"];
        if (qsDeletePage != null && qsDeletePage.Length == 36)
        {
            DeletePage(new Guid(qsDeletePage));
        }

		Guid id = GetPageId();
        if (id != Guid.Empty)
        {
            ServePage(id);
            AddMetaTags();
        }
        else if (!IsCallback)
        {
            Response.Redirect(Utils.RelativeWebRoot);
        }

        base.OnInit(e);
    }

	protected Guid GetPageId()
	{
		string id = Request.QueryString["id"];
		Guid result;
		return id != null && Guid.TryParse(id, out result) ? result : Guid.Empty;
	}

    /// <summary>
    /// Serves the page to the containing DIV tag on the page.
    /// </summary>
    /// <param name="id">
    /// The id of the page to serve.
    /// </param>
    private void ServePage(Guid id)
    {
		var pg = this.Page;

        if (pg == null || (!pg.IsVisible))
        {
            // If page is not found (could be security issue) then go to root 
            this.Response.Redirect($"~/{Utils.RelativeWebRoot}", true);
            return; // WLF: ReSharper is stupid and doesn't know that redirect returns this method.... or does it not...?
        }

        var encodedTitle = System.Web.HttpContext.Current.Server.HtmlEncode(pg.Title);
        if (UsePostStyleHeader)
        {
            // same title markup a post uses, so the theme's post styling applies
            this.h1Title.Attributes["class"] = "post-title";
            this.h1Title.InnerHtml = "<a href=\"" + PermaLink + "\" class=\"taggedlink\">" + encodedTitle + "</a>";
        }
        else
        {
            this.h1Title.InnerHtml = encodedTitle;
        }

        var arg = new ServingEventArgs(pg.Content, ServingLocation.SinglePage);
        BlogEngine.Core.Page.OnServing(pg, arg);

        if (arg.Cancel)
        {
            this.Response.Redirect("error404.aspx", true);
        }

        if (arg.Body.Contains("[usercontrol", StringComparison.OrdinalIgnoreCase))
        {
            Utils.InjectUserControls(this.divText, arg.Body);
           // this.InjectUserControls(arg.Body);
        }
        else
        {
            this.divText.InnerHtml = arg.Body;
        }
    }

    /// <summary>
    /// Adds the meta tags and title to the HTML header.
    /// </summary>
    private void AddMetaTags()
    {
        if (Page == null)
            return;

        Title = Server.HtmlEncode(Page.Title);
        AddMetaTag("keywords", Server.HtmlEncode(Page.Keywords));

        var desc = BlogSettings.Instance.Name + " - " + BlogSettings.Instance.Description + " - " + Page.Description;
        AddMetaTag("description", Server.HtmlEncode(desc));
    }

    /// <summary>
    /// Deletes the page.
    /// </summary>
    /// <param name="id">
    /// The page id.
    /// </param>
    private void DeletePage(Guid id)
    {
        var page = BlogEngine.Core.Page.GetPage(id);
        if (page == null)
        {
            return;
        }
        if (!page.CanUserDelete)
        {
            Response.Redirect(Utils.RelativeWebRoot);
            return;
        }
        if (page.HasChildPages)
        {
            return;
        }
        page.Delete();
        page.Save();
        this.Response.Redirect(Utils.RelativeWebRoot, true);
    }

	private Page _page;
	private bool _pageLoaded;
    /// <summary>
    ///     The Page instance to render on the page.
    /// </summary>
	public new Page Page
	{
		get
		{
			if (!_pageLoaded)
			{
				_pageLoaded = true;
				Guid id = GetPageId();
				if (id != Guid.Empty)
				{
					_page = Page.GetPage(id);
				}
			}

			return _page;
		}
	}

    /// <summary>
    /// True when the active theme styles posts with the "post-title"/"post-info"
    /// classes (ContactManager), so a page can show the same header as a post.
    /// Other themes keep the original page header.
    /// </summary>
    protected bool UsePostStyleHeader
    {
        get
        {
            return string.Equals(BlogSettings.Instance.Theme, "ContactManager", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Post-style "date, author, admin links" line of the page header (no comments for pages).
    /// </summary>
    public string PostInfoHtml
    {
        get
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<span class=\"post-date\">{0} <span class=\"separator\"></span></span>",
                this.Page.DateCreated.ToString("d. MMMM yyyy"));

            if (!string.IsNullOrWhiteSpace(this.Page.Author))
            {
                var profile = AuthorProfile.GetProfile(this.Page.Author);
                var name = profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.DisplayName : this.Page.Author;
                sb.AppendFormat("<span class=\"post-author\"><a href=\"{0}author/{1}{2}\">{3}</a> <span class=\"separator\"></span></span>",
                    Utils.AbsoluteWebRoot,
                    Utils.RemoveIllegalCharacters(this.Page.Author),
                    BlogConfig.FileExtension,
                    System.Web.HttpContext.Current.Server.HtmlEncode(name));
            }

            sb.Append("&nbsp;");
            sb.Append(BuildAdminLinks());
            return sb.ToString();
        }
    }

    /// <summary>
    ///     Gets the admin links to edit and delete a page.
    /// </summary>
    /// <value>The admin links.</value>
    public string AdminLinks
    {
        get
        {
            var links = BuildAdminLinks();
            return links.Length == 0 ? string.Empty : "<div id=\"admin\">" + links + "</div>";
        }
    }

    private string BuildAdminLinks()
    {
        if (!Security.IsAuthenticated)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        if (this.Page.CanUserEdit)
        {
            if (sb.Length > 0) { sb.Append(" | "); }

            sb.AppendFormat(
                "<a href=\"{0}admin/app/editor/editpage.cshtml\">{1}</a>",
                Utils.RelativeWebRoot,
                labels.add);

            sb.Append(" | ");

            sb.AppendFormat(
                "<a href=\"{0}admin/app/editor/editpage.cshtml?id={1}\">{2}</a>",
                Utils.RelativeWebRoot,
                this.Page.Id,
                labels.edit);
        }

        if (this.Page.CanUserDelete && !this.Page.HasChildPages)
        {
            if (sb.Length > 0) { sb.Append(" | "); }

            sb.AppendFormat(
                String.Concat("<a href=\"javascript:void(0);\" onclick=\"if (confirm('", labels.areYouSureDeletePage, "')) location.href='?deletepage={0}'\">{1}</a>"),
                this.Page.Id,
                labels.delete);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets PermaLink.
    /// </summary>
    public string PermaLink
    {
        get
        {
            return $"{Utils.AbsoluteWebRoot}page.aspx?id={Page.Id}";
        }
    }
}