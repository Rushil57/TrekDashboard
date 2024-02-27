using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SalesAnalytics.Web.Dashboard
{
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{userInfo}",
            //    defaults: new { controller = "Dashboard", action = "Accounts", userInfo = "sitdW1bx0LZm0HuPctdcNuKCZtws0MLjLdwmxkEbDxqkXdMws0Z9kJcQ4KFUQWV3" }
            //);
        }
    }
}
