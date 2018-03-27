using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MedicinSkema
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute(
                "Custom",
                "{s}/{h}",
                new { controller = "Home", action = "Index"},
                new { s = @"\d+", h = @"\d+" }
            );

            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            );

            //routes.MapRoute(
            //    "Custom2",
            //    "{s}/{h}/{m}",
            //    new { controller = "Home", action = "Index", s = UrlParameter.Optional, h = UrlParameter.Optional, m = 1 }
            //);

            //// Default MVC route (fallback)
            //routes.MapRoute(
            //    "Default", // Route name
            //    "{controller}/{action}/{id}", // URL with parameters
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            //);
        }
    }
}
