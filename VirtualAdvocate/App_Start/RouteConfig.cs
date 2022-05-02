#region NameSpaces
using System.Web.Mvc;
using System.Web.Routing;
#endregion
#region VirtualAdvocate
namespace VirtualAdvocate
{
    #region RouteConfig
    public class RouteConfig
    {
        #region RegisterRoutes
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            //routes.MapRoute(
            //   name: "Default",
            //   url: "{controller}/{action}/{id}",
            // defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            // );
            routes.MapRoute(
                name: "RegisterOrg",
                url: "{controller}/{action}/{id}",
              defaults: new { controller = "UsersRegistration", action = "Registration", id = UrlParameter.Optional }
            );
        } 
        #endregion
    } 
    #endregion
} 
#endregion
