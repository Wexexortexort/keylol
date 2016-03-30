﻿using System.Web.Http;
using CsQuery;
using CsQuery.Output;
using Ganss.XSS;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.ServiceBase;
using RabbitMQ.Client;

namespace Keylol.Controllers.Article
{
    /// <summary>
    ///     文章 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("article")]
    public partial class ArticleController : KeylolApiController
    {
        private readonly IModel _mqChannel;
        private readonly CouponProvider _coupon;

        /// <summary>
        /// 创建 ArticleController
        /// </summary>
        /// <param name="mqChannel">IModel</param>
        /// <param name="coupon">CouponProvider</param>
        public ArticleController(IModel mqChannel, CouponProvider coupon)
        {
            _mqChannel = mqChannel;
            _coupon = coupon;
        }

        private static void SanitizeArticle(Models.Article article, bool extractUnstyledContent)
        {
            Config.HtmlEncoder = new HtmlEncoderMinimum();
            var sanitizer =
                new HtmlSanitizer(
                    new[]
                    {
                        "br", "span", "a", "img", "b", "strong", "i", "strike", "u", "p", "blockquote", "h1", "hr",
                        "comment", "spoiler", "table", "colgroup", "col", "thead", "tr", "th", "tbody", "td"
                    },
                    null,
                    new[] {"src", "alt", "width", "height", "data-non-image", "href", "style"},
                    null,
                    new[] {"text-align"});
            var dom = CQ.Create(sanitizer.Sanitize(article.Content));
            article.ThumbnailImage = string.Empty;
            foreach (var img in dom["img"])
            {
                var url = string.Empty;
                if (string.IsNullOrEmpty(img.Attributes["src"]))
                {
                    img.Remove();
                }
                else
                {
                    var fileName = UpyunProvider.ExtractFileName(img.Attributes["src"]);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        url = img.Attributes["src"];
                    }
                    else
                    {
                        url = $"keylol://{fileName}";
                        img.Attributes["article-image-src"] = url;
                        img.RemoveAttribute("src");
                    }
                }
                if (string.IsNullOrEmpty(article.ThumbnailImage))
                    article.ThumbnailImage = url;
            }
            article.Content = dom.Render();
            if (extractUnstyledContent)
                article.UnstyledContent = dom.Render(OutputFormatters.PlainText);
        }
    }
}