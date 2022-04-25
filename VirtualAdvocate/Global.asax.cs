#region NameSpaces
using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
#endregion
#region VirtualAdvocate
namespace VirtualAdvocate
{
    #region MvcApplication
    public class MvcApplication : System.Web.HttpApplication
    {
        #region Application_Start
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //Enabling Bundling and Minification
            BundleTable.EnableOptimizations = false;
            if (string.Compare(ConfigurationManager.AppSettings["ProductionBuild"].ToString(), "true", true) == 0)
                Spire.License.LicenseProvider.SetLicenseFileName("license.elic.xml");
        }
        #endregion

        #region Application_Error
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            Server.ClearError();
        }
        #endregion
    }
    #endregion
}
#endregion
