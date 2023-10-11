using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Find;
using EPiServer.Find.Api;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework;
using EPiServer.Find.Framework.Statistics;
using EPiServer.Find.Helpers;
using EPiServer.Find.Statistics;
using EPiServer.Find.Statistics.Api;
using EPiServer.Find.UnifiedSearch;
using EPiServer.GoogleAnalytics.Models.MetricImplementations;
using EPiServer.Shell.Search;
using EPiServer.Web.Routing;
using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Project.Cms.Pages
{

	public class SearchApiController : Controller
	{
		protected IUrlResolver _resolver;
		protected IStatisticTagsHelper _statsHelper;
		private CategoryRepository _catRepo;
		private IContentRepository _contentRepo;

		public SearchApiController(IUrlResolver resolver, IStatisticTagsHelper statsHelper, CategoryRepository catRepo, IContentRepository contentRepo)
		{

			_resolver = resolver;
			_statsHelper = statsHelper;
			_catRepo = catRepo;
			_contentRepo = contentRepo;
		}



		[HttpGet]
		public ActionResult GetUnifiedSearch(string searchFor = "", int blockId = 0)
		{
			SearchClient.Instance.Conventions.UnifiedSearchRegistry.Remove<ImageFile>();
			SearchClient.Instance.Conventions.UnifiedSearchRegistry.Remove<GenericMedia>();

			var query = SearchClient.Instance.UnifiedSearch(); //Language.English

			int maxResults = 300;
			List<string> pageTypeFilters = new List<string>();

			//If the blockId was passed in, look up the settings;
			if (blockId > 0)
			{
				var blockReference = new ContentReference(blockId);
				SearchBlock block;
				try
				{
					block = _contentRepo.Get<SearchBlock>(blockReference);
				}
				catch
				{
					//We can't find the block, so just return an empty result
					block = null;
				}


				if (block != null)
				{
					if (block.MaxApiResults > 0)
					{
						maxResults = block.MaxApiResults;
					}
					if (block.ServerPageTypeFilters != null)
					{
						pageTypeFilters = block.ServerPageTypeFilters.ToList();
					}
				}
			}


			var searchTags = _statsHelper.GetTags(false);

			var hitSpec = new HitSpecification();
			hitSpec.HighlightExcerpt = true;

			var searchQuery = query.For(searchFor)
						//.UsingUnifiedWeights(unifiedWeights)
						.InStandardFields()
						.UsingSynonyms()

						/*
						 These are producing a casting error Unable to cast object of type 'EPiServer.Find.Api.Querying.Queries.BoolQuery' to type 		'EPiServer.Find.Api.Querying.Queries.QueryStringQuery'
						//.UsingSynonymsImproved() 
						//.UsingRelevanceImproved()
						*/
						.WithAndAsDefaultOperator()
						.BoostMatching(x => x.MatchType(typeof(DPLMLabTestPage)), .1) //x.SearchHitTypeName.AnyWordBeginsWith("DPLM")
						.BoostMatching(content => content.SearchHitTypeName.AnyWordBeginsWith("Article"), .8)
						.Filter(x => !x.MatchType(typeof(GenericMedia)))
						.Filter(x => !x.MatchType(typeof(ImageFile)))
						.Filter(x => !x.MatchType(typeof(VideoFileData)))
						.Filter(x => !x.MatchType(typeof(SVGFile)))
						.Filter(x => !x.MatchType(typeof(LinkPage)))
						.Filter(x => !x.MatchType(typeof(ContainerPage)));


			//Filter the results based on the block's type limitations.
			if (pageTypeFilters != null && pageTypeFilters.Count() > 0)
			{
				List<Type> typeFilters = new List<Type>();
				foreach (var filter in pageTypeFilters)
				{
					try
					{
						var filterAsType = Type.GetType(filter);
						if (filterAsType != null)
						{
							typeFilters.Add(filterAsType);
						}
					}
					catch
					{
						//We're not going to add this to the list as it's malformed.
					}
				}

				if (typeFilters.Count() > 0)
				{
					searchQuery = searchQuery.FilterByExactTypes(typeFilters);
				}
			}

			var searchResults = searchQuery
						.ApplyBestBets(1000)
						.Take(maxResults)
						.Track(searchTags)
						.StaticallyCacheFor(TimeSpan.FromMinutes(30)).GetResult(hitSpec, true);

			//TrackQueryResult stats = new TrackQueryResult();

			//if (searchResults?.TotalMatching > 0)
			//{
			//	stats = SearchClient.Instance.Statistics().TrackQuery(searchFor, cmd =>
			//	{
			//		cmd.Id = new TrackContext().Id;
			//		cmd.Tags = searchTags;
			//		cmd.Query.Hits = searchResults.TotalMatching;
			//	});
			//}


			var searchConventions = SearchClient.Instance.Conventions;
			int increment = 1;
			var results = new
			{
				TrackingStats = searchResults.ProcessingInfo,
				Facets = searchResults.Facets,
				SearchedFor = searchFor,
				Tags = searchTags,
				HitCount = searchResults.Hits.Count(),
				Hits = searchResults.Select(sh => new
				{
					//Guid = sh.ContentGuid,	
					HitNumber = increment++,
					//Hit = sh,
					Name = sh.Title,
					IsBestBet = sh.IsBestBet(),
					HasBestBetStyle = sh.HasBestBetStyle(),
					MetaData = sh.MetaData,
					Excerpt = sh.Excerpt,
					Tags = sh.MetaData.ConvertTags(_catRepo),
					Link = sh.Url,
					//HitId = searchConventions.IdConvention.GetId(sh),
					HitType = sh.OriginalObjectType.Name,
					PublishDate = sh.PublishDate,
					UpdateDate = sh.UpdateDate
					//HitType = sh.HitTypeName
					//HitType = searchConventions.TypeNameConvention.GetTypeName(sh.GetType())
				})

			};


			var serialized = JsonConvert.SerializeObject(results);
			return Content(serialized, "application/json");
		}

		[HttpGet]
		public JsonResult GetCategories()
		{
			var facetTree = _catRepo.GetRoot();
			var simpleCats = Category_Dto.ConvertCategories(facetTree);
			//var serialized = JsonConvert.SerializeObject(simpleCats);
			//return Content(serialized, "application/json");			
			return Json(simpleCats, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult BlockDetails(int contentId)
		{

			var blockRef = new ContentReference(contentId);
			var block = _contentRepo.Get<Features.SearchBlock.SearchBlock>(blockRef);

			if (block == null)
			{

				return HttpNotFound("I did not find message goes here");
			}

			var blockData = new
			{
				BlockId = block.BlockId,
				Name = block.BlockName,
				PageTypeFilters = block.PageTypeFilters,
				Group1Name = block.Group1Name,
				Group2Name = block.Group2Name,
				Group3Name = block.Group3Name,
				Group1Cats = block.Group1Categories.ConvertTags(_catRepo),
				Group2Cats = block.Group2Categories.ConvertTags(_catRepo),
				Group3Cats = block.Group3Categories.ConvertTags(_catRepo),
			};


			var serialized = JsonConvert.SerializeObject(blockData);
			return Content(serialized, "application/json");

			//var facetTree = _catRepo.GetRoot();
			//var simpleCats = Category_Dto.ConvertCategories(facetTree);
			//var serialized = JsonConvert.SerializeObject(simpleCats);
			//return Content(serialized, "application/json");
		}


		[HttpGet]
		public JsonResult GetSortOptions()
		{
			var sortTypes = new SortTypes();
			return Json(sortTypes.GetSelections(), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		//This should not longer be needed as the link contains the tracking details and the system takes care of it for us.
		public JsonResult Track(string linkClicked)
		{
			/*
			Querystring breakdown
			\_t_id – TrackId, returned from client.Statistics().TrackQuery(...).
			\_t_q – The search query string.
			\_t_tags – Tags for categorization of the collected data. Normally contain site and language tags.
			\_t_hit.id – The expected format for a hit id. (hitId argument to StatisticsClient.TrackHit) is the type name used in the index, and the ID of the document in the index separated by a slash. Example: EPiServer_Templates_Alloy_Models_Pages_ProductPage/_cb1b190b-ec66-4426-83fb24546e24136c_en
			*/

			try
			{
				var url = new Url(linkClicked);
				string query = url.QueryCollection.Get("_t_q");
				string hitId = url.QueryCollection.Get("_t_hit.id");
				string trackId = url.QueryCollection.Get("_t_id");
				string hitPositionString = url.QueryCollection.Get("_t_hit.pos");
				int hitPosition = int.Parse(hitPositionString);

				var searchTags = _statsHelper.GetTags(false);
				var result = SearchClient.Instance.Statistics().TrackHit(linkClicked, hitId, cmd =>
				{
					cmd.Hit.Id = hitId;
					cmd.Id = trackId;
					cmd.Tags = searchTags;
					cmd.Hit.Position = hitPosition;
				});

				return Json(new { Success = true, msg = result.Status }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e)
			{
				HttpContext.Response.StatusCode = 500;
				return Json(new { Success = false, msg = e.Message }, JsonRequestBehavior.AllowGet);
			}
		}

	}

	//TODO:Move this to a better place
	public static class SearchCategoryHelpers
	{
		public static List<Tag_DTO> ConvertTags(this IDictionary<string, IndexValue> metaData, CategoryRepository catRepo)
		{
			var returnMe = new List<Tag_DTO>();

			if (metaData != null && catRepo != null && metaData.ContainsKey("Categories"))
			{
				var catString = metaData["Categories"].StringValue;
				var catList = catString.Split(',').ToList();

				foreach (var cat in catList)
				{
					int catId = 0;
					int.TryParse(cat, out catId);
					if (catId > 0)
					{
						string catName = catRepo.Get(catId)?.Name;
						if (catName != null)
						{
							returnMe.Add(new Tag_DTO()
							{
								Id = catId,
								Name = catName,
								Description = catRepo.Get(cat)?.Description
							});
						}
					}
				}

			}

			return returnMe;
		}

		public static List<Tag_DTO> ConvertTags(this CategoryList source, CategoryRepository catRepo)
		{
			var returnMe = new List<Tag_DTO>();

			if (source != null && catRepo != null && source.Count > 0)
			{
				foreach (var cat in source.ToList())
				{
					string catName = catRepo.Get(cat)?.Name;
					if (catName != null)
					{
						returnMe.Add(new Tag_DTO()
						{
							Id = cat,
							Name = catName,
							Description = catRepo.Get(cat)?.Description
						});
					}


				}
			}

			return returnMe;
		}
	}


	public class Tag_DTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		public static List<Tag_DTO> ConvertTags(CategoryList source, CategoryRepository catRepo)
		{
			var returnMe = new List<Tag_DTO>();

			if (source != null && catRepo != null && source.Count > 0)
			{
				foreach (var cat in source.ToList())
				{
					string catName = catRepo.Get(cat)?.Name;
					if (catName != null)
					{
						returnMe.Add(new Tag_DTO()
						{
							Id = cat,
							Name = catName,
							Description = catRepo.Get(cat)?.Description
						});
					}


				}
			}

			return returnMe;
		}
	}

	public class Category_Dto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int SortOrder { get; set; }
		public int Indent { get; set; }
		public bool Available { get; set; }
		public List<Category_Dto> Categories { get; set; }

		public static Category_Dto ConvertCategories(Category cat)
		{
			Category_Dto rtCat = new Category_Dto();

			if (cat != null)
			{
				rtCat.Id = cat.ID;
				rtCat.Name = cat.Name;
				rtCat.SortOrder = cat.SortOrder;
				rtCat.Indent = cat.Indent;
				rtCat.Available = cat.Available;
				rtCat.Categories = new List<Category_Dto>();

				if (cat.Categories != null && cat.Categories.Count() > 0)
				{
					foreach (var childCat in cat.Categories)
					{
						rtCat.Categories.Add(Category_Dto.ConvertCategories(childCat));
					}
				}
			}

			return rtCat;
		}
	}

	public static class SearchExtensions
	{
		public static ITypeSearch<T> AddSorting<T>(this ITypeSearch<T> search, string sortBy) where T : LayoutPageData
		{

			// set sort order (in this case, relevance = sort index order if no query applied)
			if (!String.IsNullOrEmpty(sortBy))
			{
				if (sortBy.Equals(SortTypes.DateSortAsc.Value as string))
				{
					search = search.OrderBy(x => x.Created);
				}
				else if (sortBy.Equals(SortTypes.DateSortDesc.Value as string))
				{
					search = search.OrderByDescending(x => x.Created);
				}
				else if (sortBy.Equals(SortTypes.TitleSortAsc.Value as string))
				{
					search = search.OrderBy(x => x.SearchTitle());
				}
				else if (sortBy.Equals(SortTypes.TitleSortDesc.Value as string))
				{
					search = search.OrderByDescending(x => x.SearchTitle());
				}
			}

			return search;
		}

	}
}