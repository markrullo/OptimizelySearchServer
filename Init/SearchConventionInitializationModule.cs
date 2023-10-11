using EPiServer;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Cms.Conventions;
using EPiServer.Find.Framework;
using EPiServer.Find.UnifiedSearch;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using System.Collections.Generic;

namespace Project.Cms.Business.Initialization
{
	[InitializableModule]
	[ModuleDependency(typeof(EPiServer.Find.Cms.Module.IndexingModule))]
	public class SearchConventionInitializationModule : IInitializableModule
	{
		public void Initialize(InitializationEngine context)
		{
			ContentIndexer.Instance.Conventions.ShouldIndexInContentAreaConvention = new AlwaysIndexConvention();

			var setup = new CmsUnifiedSearchSetUp();
			var reg = SearchClient.Instance.Conventions.UnifiedSearchRegistry;


			//This allowed us to include the categories and created date in the search index
			reg.Add<PageData>()
				.ProjectMetaDataFrom(page => new Dictionary<string, EPiServer.Find.IndexValue> {
					{ "Categories", page.Category.ToString() } ,
					{ "CreatedDate", page.Created.ToString() }
				})
				.CustomizeIndexProjection(setup.CustomizeIndexProjectionForPageData)
				;
		}

		public void Uninitialize(InitializationEngine context)
		{
		}



	}
	public class AlwaysIndexConvention : IShouldIndexInContentAreaConvention
	{
		public bool? ShouldIndexInContentArea(IContent content)
		{
			return true;
		}
	}
}
