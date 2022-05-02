#region NameSpaces
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using VirtualAdvocate.Models;
#endregion
#region VirtualAdvocate.Controllers
namespace VirtualAdvocate.Controllers
{
    #region RecursiveNotificationController
    public class RecursiveNotificationController : BaseController
    {
        #region Global Variables
        public int userID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
        public int orgId = Convert.ToInt32(System.Web.HttpContext.Current.Session["OrgId"]);
        public int deptID = Convert.ToInt32(System.Web.HttpContext.Current.Session["DepartmentID"]);
        public int roleId = Convert.ToInt32(System.Web.HttpContext.Current.Session["RoleId"]);

        #endregion

        #region Index
        // GET: RecursiveNotification
        public ActionResult Index()
        {
            var recursiveNotificationDetails = VAEDB.RecursiveNotificationDetails.Where(m => m.Status && m.OrgId == orgId).Include(r => r.OrganizationDetail).ToList();
            var recursive = new List<RecursiveNotificationModel>();
            foreach (var item in recursiveNotificationDetails)
            {
                recursive.Add(new RecursiveNotificationModel
                {
                    Id = item.Id,
                    RecurrsAfterDays = item.RecurrsAfterDays,
                    RecurrsBeforeDays = item.RecurrsBeforeDays
                });
            }
            return View(recursive);
        }
        #endregion

        #region Details
        // GET: RecursiveNotification/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecursiveNotificationDetail recursiveNotificationDetail = VAEDB.RecursiveNotificationDetails.Find(id);
            if (recursiveNotificationDetail == null)
            {
                return HttpNotFound();
            }
            return View(recursiveNotificationDetail);
        }
        #endregion

        #region Create
        // GET: RecursiveNotification/Create
        public ActionResult Create()
        {
            ViewBag.OrgId = new SelectList(VAEDB.OrganizationDetails, "OrganizationId", "OrgName");
            return View();
        }
        #endregion

        #region Create
        // POST: RecursiveNotification/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,OrgId,RecurrsBeforeDays,RecurrsAfterDays")] RecursiveNotificationModel recursiveNotificationModel)
        {
            if (ModelState.IsValid)
            {
                var recursiveDetail = new RecursiveNotificationDetail
                {
                    RecurrsBeforeDays = recursiveNotificationModel.RecurrsBeforeDays,
                    RecurrsAfterDays = recursiveNotificationModel.RecurrsAfterDays,
                    OrgId = orgId,
                    CreatedDate = DateTime.Now,
                    Status = true,
                };
                VAEDB.RecursiveNotificationDetails.Add(recursiveDetail);
                VAEDB.SaveChanges();
                return RedirectToAction("Index");
            }


            return View(recursiveNotificationModel);
        }

        #endregion

        #region Edit
        // GET: RecursiveNotification/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecursiveNotificationDetail recursiveNotificationDetail = VAEDB.RecursiveNotificationDetails.Find(id);
            if (recursiveNotificationDetail == null)
            {
                return HttpNotFound();
            }
            var recursive = new RecursiveNotificationModel
            {
                RecurrsBeforeDays = recursiveNotificationDetail.RecurrsBeforeDays,
                Id = recursiveNotificationDetail.Id,
                RecurrsAfterDays = recursiveNotificationDetail.RecurrsAfterDays
            };
            return View(recursive);
        }
        #endregion

        #region Edit
        // POST: RecursiveNotification/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,OrgId,RecurrsBeforeDays,RecurrsAfterDays")] RecursiveNotificationModel recursiveNotificationModel)
        {
            if (ModelState.IsValid)
            {
                var recursive = VAEDB.RecursiveNotificationDetails.FirstOrDefault(m => m.Id == recursiveNotificationModel.Id);

                recursive.RecurrsAfterDays = recursiveNotificationModel.RecurrsAfterDays;
                recursive.RecurrsBeforeDays = recursiveNotificationModel.RecurrsBeforeDays;
                recursive.ModifiedDate = DateTime.Now;

                VAEDB.Entry(recursive).State = EntityState.Modified;
                VAEDB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(recursiveNotificationModel);
        }

        #endregion

        #region Delete
        // GET: InsuranceTracker/Delete/5
        public JsonResult Delete(int id)
        {
            try
            {
                if (id != 0)
                {
                    RecursiveNotificationDetail recursive = VAEDB.RecursiveNotificationDetails.Find(id);
                    recursive.Status = false;
                    VAEDB.Entry(recursive).State = EntityState.Modified;
                    VAEDB.SaveChanges();
                }
                else
                {
                    return Json(300, JsonRequestBehavior.AllowGet);
                }

                return Json(200, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(500, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region DeleteConfirmed
        // POST: RecursiveNotification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            RecursiveNotificationDetail recursiveNotificationDetail = VAEDB.RecursiveNotificationDetails.Find(id);
            VAEDB.RecursiveNotificationDetails.Remove(recursiveNotificationDetail);
            VAEDB.SaveChanges();
            return RedirectToAction("Index");
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                VAEDB.Dispose();
            }
            base.Dispose(disposing);
        } 
        #endregion
    } 
    #endregion
} 
#endregion
