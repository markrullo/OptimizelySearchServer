using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Project.Cms.Business.Initialization
{
	[InitializableModule]
	[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
	public class AddSearchApiRoutingInitialization : IInitializableModule
	{
		private bool initialized = false;
		public void Initialize(InitializationEngine context)
		{
			//Add initialization logic, this method is called once after CMS has been initialized
			if (!initialized)
			{
				RouteTable.Routes.MapRoute(
				name: "SearchApi",
				url: "api/SearchApi/{action}",
				defaults: new
				{
					controller = "SearchApi"
				});
				initialized = true;
			}
		}

		public void Uninitialize(InitializationEngine context)
		{
			//Add uninitialization logic			
		}
	}
}