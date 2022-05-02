#region NameSpaces
using System.Web.Mvc;
#endregion
#region VirtualAdvocate
namespace VirtualAdvocate
{
    #region FilterConfig
    public class FilterConfig
    {
        #region RegisterGlobalFilters
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        } 
        #endregion
    } 
    #endregion
} 
#endregion
