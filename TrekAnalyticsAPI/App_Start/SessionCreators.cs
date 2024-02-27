//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Http.WebHost;
//using System.Web.Routing;
//using System.Web.SessionState;

//namespace AnalyticsAPI.App_Start
//{
//    public class SessionControllerHandler : HttpControllerHandler, IRequiresSessionState
//    {
//        public SessionControllerHandler(RouteData routeData)
//            : base(routeData)
//        { }
//    }

//    public class SessionHttpControllerRouteHandler : HttpControllerRouteHandler
//    {
//        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
//        {
//            return new SessionControllerHandler(requestContext.RouteData);
//        }
//    }


//    public class MyHttpControllerHandler
//  : HttpControllerHandler, IRequiresSessionState
//    {
//        public MyHttpControllerHandler(RouteData routeData) : base(routeData)
//        { }
//    }

//    public class MyHttpControllerRouteHandler : HttpControllerRouteHandler
//    {
//        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
//        {
//            return new MyHttpControllerHandler(requestContext.RouteData);
//        }
//    }
//}