using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VirtualAdvocate.Common;
using VirtualAdvocate.Models;

namespace VirtualAdvocate.Controllers
{
    public class DepartmentController : BaseController
    {


        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();

        // GET: Department
        public ActionResult Index()
        {
            DepartmentModel depObj = new DepartmentModel();

            return View("AddDepartment",depObj);
        }

        public JsonResult CheckDepartment(string Department)
        {
            var chkExisting = db.Departments.Where(a => a.Name == Department.Trim()).FirstOrDefault();

            if (chkExisting != null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddDepartment(DepartmentModel obj)
        {
            try
            {
                Department objDepartment = new Department();
                objDepartment.IsEnabled = true;
                objDepartment.Name = obj.Department;
                objDepartment.Description = obj.Description;

                db.Departments.Add(objDepartment);


                db.SaveChanges();
                Int64 result = objDepartment.Id;


               
                //Log Insert
                DepartmentLog objLog = new DepartmentLog();
                objLog.IsEnabled = true;
                objLog.Name = objDepartment.Name;
                objLog.LogId = objDepartment.Id;
                objLog.Action = "Insert";
                objLog.Description = objDepartment.Description;
                objLog.ModifiedDate = DateTime.Now;
                db.DepartmentLogs.Add(objLog);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            ViewBag.Enable = true;
            return RedirectToAction("DepartmentList", "Department");

        }

        public ActionResult EditDepartment(int id)
        {

            if (id != 0)
            {
                var data = db.Departments.Where(i => i.Id == id).FirstOrDefault();

                DepartmentModel obj = new DepartmentModel();

                obj.Department = data.Name;
                obj.Description = data.Description;
              
                obj.Id = id;

                return View("EditDepartment", obj);
            }
            else
            {
                ViewBag.Enable = true;
                return View("DepartmentList", "Department");
            }
        }


        public ActionResult DepartmentList(string enable)
        {
            bool active;
            if (string.IsNullOrEmpty(enable))
            {
                active = true;
                enable = "Active";
            }
            else
            {
                if (enable == "Active")
                    active = true;
                else
                    active = false;
            }

            ViewBag.Enable = enable;

            List<Department> objDepartment = new List<Department>();
            objDepartment = db.Departments.Where(c => c.IsEnabled == active).ToList();

            return View(objDepartment);
        }


        [HttpPost]
        public JsonResult ActivateDepartment(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            //Log Insert
            DepartmentLog objLog = new DepartmentLog();
            try
            {
                var obj = db.Departments.Find(id);
                if (obj != null)
                {
                    if (obj.IsEnabled == true)
                    {
                        objLog.Action = "Inactive";
                        obj.IsEnabled = false;
                        objLog.IsEnabled = false;
                        message = "Department Deactivated Successfully";
                    }
                    else
                    {
                        objLog.Action = "Active";
                        obj.IsEnabled = true;
                        objLog.IsEnabled = true;
                        message = "Department Activated Successfully";
                    }
                }
               
                objLog.Name = obj.Name;
                objLog.Description = obj.Description;
                objLog.DepartmentID = obj.Id;
                objLog.ModifiedDate = DateTime.Now;
                db.DepartmentLogs.Add(objLog);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex.InnerException);
                ErrorLog.LogThisError(ex);
                message = "An error occured while processing the request. Try again later";
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            }

            return Json(new { message = message }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult GetDepartmentDetails(int? id)
        {
            DepartmentModel obj = new DepartmentModel();
            var department = db.Clice.Where(i => i.Id == id).FirstOrDefault();
            obj.Department = department.Clouse1;
            obj.Description = department.Description;

            obj.Id = id.Value;
            
            return View("EditClouse", obj);

        }


        public ActionResult UpdateDepartment(DepartmentModel obj)
        {
            try
            {
                var objDepartment = db.Departments.Find(obj.Id);
                objDepartment.Name = obj.Department;
                objDepartment.Description = obj.Description;

              

                       
                //Log Insert
                DepartmentLog objLog = new DepartmentLog();
                objLog.IsEnabled = true;
                objLog.Name =obj.Department;
                objLog.DepartmentID = obj.Id;
                objLog.Action = "Update";
                objLog.Description = obj.Description;
                objLog.ModifiedDate = DateTime.Now;
                db.DepartmentLogs.Add(objLog);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            ViewBag.Enable = true;
            return RedirectToAction("DepartmentList", "Department");

        }

        public ActionResult CheckDepartmentData(int id)
        {
         int count= db.UserProfiles.Where(s => s.Department == id  && s.IsEnabled==true).Count();
            if (count > 0)
                return Json(false,JsonRequestBehavior.AllowGet);
            else
            {
                count = db.DocumentTemplates.Where(s => s.DepartmentID == id && s.IsEnabled == true).Count();
                if (count > 0)
                    return Json(false, JsonRequestBehavior.AllowGet);
                else
                    count = db.SelectedDepartments.Where(s => s.DepartmentID == id ).Count();
                if (count > 0)
                    return Json(false, JsonRequestBehavior.AllowGet);
                else
                    return Json(true, JsonRequestBehavior.AllowGet);
            }
        }
    }
}