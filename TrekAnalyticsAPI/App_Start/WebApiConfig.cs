using AnalyticsAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using System.Web.Http.Cors;

namespace AnalyticsAPI
{
    public static class WebApiConfig
    {
        public static string UrlPrefix { get { return "api"; } }
        public static string UrlPrefixRelative { get { return "~/api"; } }
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            config.Formatters.RemoveAt(0);
            config.Formatters.Insert(0, new JilFormatter());
        }
    }
}
