#region NameSpaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VirtualAdvocate.Common;
using VirtualAdvocate.Models;
#endregion
#region VirtualAdvocate.Controllers
namespace VirtualAdvocate.Controllers
{
    #region DepartmentController
    public class DepartmentController : BaseController
    {
        #region Index
        // GET: Department
        public ActionResult Index()
        {
            DepartmentModel depObj = new DepartmentModel();

            return View("AddDepartment", depObj);
        }
        #endregion

        #region CheckDepartment
        public JsonResult CheckDepartment(string Department)
        {
            var chkExisting = VAEDB.Departments
                .Where(a => a.Name == Department.Trim()).FirstOrDefault();

            if (chkExisting != null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region AddDepartment
        [HttpPost]
        public ActionResult AddDepartment(DepartmentModel obj)
        {
            try
            {
                Department objDepartment = new Department();
                objDepartment.IsEnabled = true;
                objDepartment.Name = obj.Department;
                objDepartment.Description = obj.Description;
                VAEDB.Departments.Add(objDepartment);
                VAEDB.SaveChanges();
                Int64 result = objDepartment.Id;

                //Log Insert
                DepartmentLog objLog = new DepartmentLog();
                objLog.IsEnabled = true;
                objLog.Name = objDepartment.Name;
                objLog.LogId = objDepartment.Id;
                objLog.Action = "Insert";
                objLog.Description = objDepartment.Description;
                objLog.ModifiedDate = DateTime.Now;
                VAEDB.DepartmentLogs.Add(objLog);
                VAEDB.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            ViewBag.Enable = true;
            return RedirectToAction("DepartmentList", "Department");

        }
        #endregion

        #region EditDepartment
        public ActionResult EditDepartment(int id)
        {

            if (id != 0)
            {
                var data = VAEDB.Departments.Where(i => i.Id == id).FirstOrDefault();

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
        #endregion

        #region DepartmentList
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
            objDepartment = VAEDB.Departments.Where(c => c.IsEnabled == active).ToList();

            return View(objDepartment);
        }
        #endregion

        #region ActivateDepartment
        [HttpPost]
        public JsonResult ActivateDepartment(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            //Log Insert
            DepartmentLog objLog = new DepartmentLog();
            try
            {
                var obj = VAEDB.Departments.Find(id);
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
                VAEDB.DepartmentLogs.Add(objLog);
                VAEDB.SaveChanges();

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
        #endregion

        #region GetDepartmentDetails
        [HttpPost]
        public ActionResult GetDepartmentDetails(int? id)
        {
            DepartmentModel obj = new DepartmentModel();
            var department = VAEDB.Clice.Where(i => i.Id == id).FirstOrDefault();
            obj.Department = department.Clouse1;
            obj.Description = department.Description;
            obj.Id = id.Value;
            return View("EditClouse", obj);
        }
        #endregion

        #region UpdateDepartment
        public ActionResult UpdateDepartment(DepartmentModel obj)
        {
            try
            {
                var objDepartment = VAEDB.Departments.Find(obj.Id);
                objDepartment.Name = obj.Department;
                objDepartment.Description = obj.Description;

                //Log Insert
                DepartmentLog objLog = new DepartmentLog();
                objLog.IsEnabled = true;
                objLog.Name = obj.Department;
                objLog.DepartmentID = obj.Id;
                objLog.Action = "Update";
                objLog.Description = obj.Description;
                objLog.ModifiedDate = DateTime.Now;
                VAEDB.DepartmentLogs.Add(objLog);
                VAEDB.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            ViewBag.Enable = true;
            return RedirectToAction("DepartmentList", "Department");

        }
        #endregion

        #region CheckDepartmentData
        public ActionResult CheckDepartmentData(int id)
        {
            int count = VAEDB.UserProfiles
                .Where(s => s.Department == id && s.IsEnabled == true).Count();
            if (count > 0)
                return Json(false, JsonRequestBehavior.AllowGet);
            else
            {
                count = VAEDB.DocumentTemplates.Where(s => s.DepartmentID == id && s.IsEnabled == true).Count();
                if (count > 0)
                    return Json(false, JsonRequestBehavior.AllowGet);
                else
                    count = VAEDB.SelectedDepartments.Where(s => s.DepartmentID == id).Count();
                if (count > 0)
                    return Json(false, JsonRequestBehavior.AllowGet);
                else
                    return Json(true, JsonRequestBehavior.AllowGet);
            }
        } 
        #endregion
    }
    #endregion
} 
#endregion