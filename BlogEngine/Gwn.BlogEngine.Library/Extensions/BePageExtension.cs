using System;
using BlogEngine.Core;

namespace Gwn.BlogEngine.Library.Extensions
{
    /// <summary>
    /// Page extension
    /// </summary>
    public static class BePageExtension
    {
        /// <summary>
        /// Gets the id link.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public static Tuple<string, string> GetIdLink(this Page page)
        {
            // Page has no PermaLink property (unlike Post) - mirror the same
            // page.aspx?id={id} pattern already used by page.aspx.cs and UrlRules.cs
            var permaLink = $"{page.Blog.AbsoluteWebRoot}page.aspx?id={page.Id}";
            return new Tuple<string, string>(page.Id.ToString(), permaLink);
        }
    }
}
