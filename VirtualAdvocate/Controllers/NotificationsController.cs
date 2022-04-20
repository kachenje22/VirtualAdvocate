using System;
using System.Linq;
using System.Web.Mvc;
using VirtualAdvocate.Models;

namespace VirtualAdvocate.Controllers
{
    public class NotificationsController : BaseController
    {
        public int userID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
        public int orgId = Convert.ToInt32(System.Web.HttpContext.Current.Session["OrgId"]);
        public int deptID = Convert.ToInt32(System.Web.HttpContext.Current.Session["DepartmentID"]);
        public int roleId = Convert.ToInt32(System.Web.HttpContext.Current.Session["RoleId"]);

        // GET: Notifications
        public ActionResult Index()
        {
            //var notificationDetails = new DashBoardModel().GetNotificationDetails(userID)
            //    .Select(s => new NotificationViewModel
            //    {
            //        Title = s.TemplateType,
            //        Description = (s.PriorToExpiry > 0 ? (s.PriorToExpiry + " " + s.TemplateType + "(s) are being expired. ") : string.Empty)
            //                        + (s.OnExpiry > 0 ? (s.OnExpiry + " " + s.TemplateType + "(s) expires today. ") : string.Empty)
            //                        + (s.AfterExpiry > 0 ? (s.AfterExpiry + " " + s.TemplateType + "(s) has been expired.") : string.Empty),
            //        Url = s.TemplateType == TemplateType.Insurance ? "/InsuranceTracker/Index?flagForNotification=1" : "/ProbationTracker/Index?flagForNotification=1"
            //    });

            var notificationDetails = new DashBoardModel().GetNotificationDetails(new NotificationModel
            {
                DepartmentId = deptID,
                OrganizationId = orgId,
                FlatForNotification = 1,
                RoleId = roleId,
                UserId = userID
            })
                .Where(m => m.AfterExpiry > 0)
                .Select(s => new NotificationViewModel
                {
                    Title = s.TemplateType,
                    AfterExpiry = s.AfterExpiry,
                    OnExpiry = s.OnExpiry,
                    PriorToExpiry = s.PriorToExpiry,
                    Url = s.TemplateType == TemplateType.Insurance ?
                                            "/InsuranceTracker/Index?flagForNotification=1" :
                                            s.TemplateType == TemplateType.Probation ?
                                            "/ProbationTracker/Index?flagForNotification=1" :
                                            "/DocumentDetails/Index?flagForNotification=1"
                });

            return View(notificationDetails);
        }

        public int GetNotificationCount()
        {
            int count;
            try
            {
                count = new DashBoardModel().GetNotificationDetails(new NotificationModel
                {
                    DepartmentId = deptID,
                    OrganizationId = orgId,
                    FlatForNotification = 1,
                    RoleId = roleId,
                    UserId = userID
                }).Where(m => m.AfterExpiry > 0).Count();
            }
            catch(Exception ex)
            {
                count = 0;
            }
            return count;
        }
    }
}