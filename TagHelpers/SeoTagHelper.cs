using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWinners.Models;
using Umbraco.Cms.Web.Common;

namespace SmartWinners.TagHelpers;

[HtmlTargetElement("seo")]
public class SeoTagHelper(IUmbracoHelperAccessor umbracoHelperAccessor, IHttpContextAccessor httpContextAccessor) : TagHelper
{
	public SEOViewModel? Model { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "";
		output.Content.Clear();

		var umbraco = umbracoHelperAccessor.TryGetUmbracoHelper(out var helper) ? helper : null;

		string defaultTitle = umbraco?.GetDictionaryValue("SEO Default Title")
			?? "Social Casino Slots & Live Social Games | PlayerClub365";
		string defaultDescription = umbraco?.GetDictionaryValue("SEO Default Description")
			?? "Experience the ultimate social gaming platform at PlayerClub365. Enjoy free social slots, virtual table games, and live social entertainment. Play for fun and collect daily virtual rewards. No real money gambling.";
		string defaultOgDescription = umbraco?.GetDictionaryValue("SEO Default Og Description")
			?? "Join the world's premier social gaming community. Enjoy Vegas-style slots and social games for entertainment purposes only. Claim your virtual welcome pack today!";
		string defaultKeywords = umbraco?.GetDictionaryValue("SEO Default Keywords")
			?? "social casino, free slots, social gaming, virtual rewards, slot simulators, live social games, play for fun, PlayerClub365";

		string title = string.IsNullOrWhiteSpace(Model?.Title) ? defaultTitle : Model.Title;
		string description = string.IsNullOrWhiteSpace(Model?.Description) ? defaultDescription : Model.Description;

		string keywords = string.IsNullOrWhiteSpace(Model?.Keywords) ? defaultKeywords : Model.Keywords;

		string ogTitle = string.IsNullOrWhiteSpace(Model?.OgTitle) ? defaultTitle : Model.OgTitle;
		string ogDescription = string.IsNullOrWhiteSpace(Model?.OgDescription) ? defaultOgDescription : Model.OgDescription;
		string ogImage = string.IsNullOrWhiteSpace(Model?.OgImage)
			? "https://www.playerclub365.com/assets/img/social-preview.jpg"
			: Model.OgImage;

		string twitterTitle = string.IsNullOrWhiteSpace(Model?.TwitterTitle) ? defaultTitle : Model.TwitterTitle;
		string twitterDescription = string.IsNullOrWhiteSpace(Model?.TwitterDescription) ? defaultDescription : Model.TwitterDescription;
		string twitterImage = string.IsNullOrWhiteSpace(Model?.TwitterImage)
			? "https://www.playerclub365.com/assets/img/social-preview.jpg"
			: Model.TwitterImage;

		string robots = string.IsNullOrWhiteSpace(Model?.MetaRobots) ? "" : $"<meta name=\"robots\" content=\"{Model.MetaRobots}\" />";

		var request = httpContextAccessor.HttpContext?.Request;
		string currentUrl = request != null
			// Force https in generated canonical/OG URLs regardless of incoming scheme
			? $"https://{request.Host}{request.Path}{request.QueryString}"
			: "https://www.playerclub365.com/";

		string siteName = "PlayerClub365";
		string twitterHandle = "@playerclub365";

		output.Content.AppendHtml($@"
				<title>{title}</title>
				<meta name=""description"" content=""{description}"" />
				<meta name=""keywords"" content=""{keywords}"" />

				<meta property=""og:title"" content=""{ogTitle}"" />
				<meta property=""og:description"" content=""{ogDescription}"" />
				<meta property=""og:type"" content=""website"" />
				<meta property=""og:url"" content=""{currentUrl}"" />
				<meta property=""og:image"" content=""{ogImage}"" />
				<meta property=""og:image:width"" content=""1200"" />
				<meta property=""og:image:height"" content=""630"" />
				<meta property=""og:site_name"" content=""{siteName}"" />

				<meta name=""twitter:card"" content=""summary_large_image"" />
				<meta name=""twitter:site"" content=""{twitterHandle}"" />
				<meta name=""twitter:title"" content=""{twitterTitle}"" />
				<meta name=""twitter:description"" content=""{twitterDescription}"" />
				<meta name=""twitter:image"" content=""{twitterImage}"" />

				{robots}
			");
	}
}
