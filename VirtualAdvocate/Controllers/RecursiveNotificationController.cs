using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using VirtualAdvocate.Models;

namespace VirtualAdvocate.Controllers
{
    public class RecursiveNotificationController : BaseController
    {
        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();
        public int userID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
        public int orgId = Convert.ToInt32(System.Web.HttpContext.Current.Session["OrgId"]);
        public int deptID = Convert.ToInt32(System.Web.HttpContext.Current.Session["DepartmentID"]);
        public int roleId = Convert.ToInt32(System.Web.HttpContext.Current.Session["RoleId"]);

        // GET: RecursiveNotification
        public ActionResult Index()
        {
            var recursiveNotificationDetails = db.RecursiveNotificationDetails.Where(m => m.Status && m.OrgId == orgId).Include(r => r.OrganizationDetail).ToList();
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

        // GET: RecursiveNotification/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecursiveNotificationDetail recursiveNotificationDetail = db.RecursiveNotificationDetails.Find(id);
            if (recursiveNotificationDetail == null)
            {
                return HttpNotFound();
            }
            return View(recursiveNotificationDetail);
        }

        // GET: RecursiveNotification/Create
        public ActionResult Create()
        {
            ViewBag.OrgId = new SelectList(db.OrganizationDetails, "OrganizationId", "OrgName");
            return View();
        }

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
                db.RecursiveNotificationDetails.Add(recursiveDetail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            return View(recursiveNotificationModel);
        }

        // GET: RecursiveNotification/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecursiveNotificationDetail recursiveNotificationDetail = db.RecursiveNotificationDetails.Find(id);
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

        // POST: RecursiveNotification/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,OrgId,RecurrsBeforeDays,RecurrsAfterDays")] RecursiveNotificationModel recursiveNotificationModel)
        {
            if (ModelState.IsValid)
            {
                var recursive = db.RecursiveNotificationDetails.FirstOrDefault(m => m.Id == recursiveNotificationModel.Id);
                
                recursive.RecurrsAfterDays = recursiveNotificationModel.RecurrsAfterDays;
                recursive.RecurrsBeforeDays = recursiveNotificationModel.RecurrsBeforeDays;
                recursive.ModifiedDate = DateTime.Now;
                
                db.Entry(recursive).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(recursiveNotificationModel);
        }

        // GET: InsuranceTracker/Delete/5
        public JsonResult Delete(int id)
        {
            try
            {
                if (id != 0)
                {
                    RecursiveNotificationDetail recursive = db.RecursiveNotificationDetails.Find(id);
                    recursive.Status = false;
                    db.Entry(recursive).State = EntityState.Modified;
                    db.SaveChanges();
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

        // POST: RecursiveNotification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            RecursiveNotificationDetail recursiveNotificationDetail = db.RecursiveNotificationDetails.Find(id);
            db.RecursiveNotificationDetails.Remove(recursiveNotificationDetail);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
