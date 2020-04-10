using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PermissionManagement.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute("GetTransfer", "GetTransfer", new { controller = "Remitly", action = "GetTransfer" });
            routes.MapRoute("UpdateTransfer", "UpdateTransfer", new { controller = "Remitly", action = "UpdateTransfer" });
            routes.MapRoute("LogIn", "LogIn", new { controller = "Home", action = "LogIn" });
            routes.MapRoute("LogOut", "LogOut", new { controller = "Home", action = "LogOut" });
            routes.MapRoute("ResetPassword", "ResetPassword", new { controller = "Home", action = "ResetPassword" });
            routes.MapRoute("AccountActivate", "AccountActivate", new { controller = "Home", action = "AccountActivate" });
            routes.MapRoute("Error", "Error", new { controller = "Home", action = "Error" });
            routes.MapRoute("PageNotFound", "PageNotFound", new { controller = "Home", action = "PageNotFound" });
            routes.MapRoute("SessionTimeOut", "SessionTimeOut", new { controller = "Home", action = "SessionTimeOut" });


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.Add("UnknownRoute", new Route("{*UnknownRoute}", new UnknownRouteHandler()));
        }
    }
}
