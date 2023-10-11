using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Labs.LinkItemProperty;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using Newtonsoft.Json;
using Project.Cms.Business.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Project.Cms.Constants;

namespace Project.Cms.Features.SearchBlock
{
    [ContentType(DisplayName = "Search Block", GroupName = Constants.GroupNames.Content,
        Description = "Search Block using front end search with Angular",
        GUID = "E434E7D8-6993-41EE-A882-AE00AFC77B9B")]
	[SiteImageUrl(Constants.PageIconsPath + "search - icon.svg")]
	public class SearchBlock : SiteBlockData, AllowedTypes.IMainContentBlock, AllowedTypes.IArticleContentBlock
    {

		[Display(Name = "Page Type Filters - Server",
			Description = "Allows the page types to be filtered on the server before getting to the client. Allows for directory pages.",
			GroupName = Constants.GroupNames.Content,
			Order = 140)]
		public virtual IList<string> ServerPageTypeFilters { get; set; }

		[Display(Name = "Page Type Filters - Client",
			Description = "Allows the page type filters to show on the client side and configures them. This is a Javascript string array with the first element being the display name on the client.",
			GroupName = Constants.GroupNames.Content,
			Order = 150)]
		public virtual IList<string> PageTypeFilters { get; set; }

		[Display(Name = "Max Results",
			Description = "Limits the number of results returned to the client.",
			GroupName = Constants.GroupNames.Content,
			Order = 160)]
		public virtual int MaxApiResults { get; set; }

		[Display(Name = "Group 1 Name",
		GroupName = SystemTabNames.Content,
		Order = 200)]
		[CultureSpecific]
		public virtual string Group1Name { get; set; }

		[Display(Name = "Group 1 categories available",
			GroupName = Constants.GroupNames.Content,
			Order = 210)]
		public virtual CategoryList Group1Categories { get; set; }


		[Display(Name = "Group 2 Name",
		GroupName = SystemTabNames.Content,
		Order = 220)]
		[CultureSpecific]
		public virtual string Group2Name { get; set; }

		[Display(Name = "Group 2 categories available",
			GroupName = Constants.GroupNames.Content,
			Order = 230)]
		public virtual CategoryList Group2Categories { get; set; }


		[Display(Name = "Group 3 Name",
		GroupName = SystemTabNames.Content,
		Order = 240)]
		[CultureSpecific]
		public virtual string Group3Name { get; set; }

		[Display(Name = "Group 3 categories available",
			GroupName = Constants.GroupNames.Content,
			Order = 250)]
		public virtual CategoryList Group3Categories { get; set; }

		
	}
}
