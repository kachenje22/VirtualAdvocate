
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using VirtualAdvocate.Common;
using VirtualAdvocate.DAL;
using VirtualAdvocate.Models;
using EntityFramework.Extensions;
using System.Data;
using ClosedXML.Excel;
using System.Web.Script.Serialization;



namespace VirtualAdvocate.Controllers
{
    public class ClouseManagementController : BaseController
    {
        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();

        public ActionResult Index()
        {
            ClouseModel obj = new ClouseModel();
            int[] i = new int[] { 0};

            var categories = db.DocumentCategories.Where(s => s.IsEnabled == true).ToList();
            obj.SelectedGroups =i;
            obj.getAllCategory = categories;
           
            return View("AddClouse",obj);
        }



        [HttpPost]
        public ActionResult AddClouse(ClouseModel obj)
        {
            try
            {
                Clouse objClouse = new Clouse();
                objClouse.IsEnabled = true;
                objClouse.Clouse1 = obj.Clouse1;
                objClouse.Description = obj.Description;
              
                db.Clice.Add(objClouse);

                
                db.SaveChanges();
                Int64 result = objClouse.Id;

               
                for(int i= 0;i<obj.SelectedGroups.Count();i++)
                {
                    ClouseandCategoryMaping objMaping = new ClouseandCategoryMaping();
                    objMaping.categoryID = obj.SelectedGroups[i];
                    objMaping.clouseID = objClouse.Id;
                    db.ClouseandCategoryMapings.Add(objMaping);
                    db.SaveChanges();
                }
             
                //Log Insert
                ClouseLog objLog = new ClouseLog();
                objLog.IsEnabled = true;
                objLog.ClouseName = objClouse.Clouse1;
                objLog.ClouseId = objClouse.Id;
                objLog.Action = "Insert";
                objLog.ClouseDescription = objClouse.Description;
                objLog.ModifiedDate = DateTime.Now;
                objLog.ModifiedBy ="" ;
                db.ClouseLogs.Add(objLog);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            ViewBag.Enable = true;
            return RedirectToAction("ClouseList", "ClouseManagement");

        }

  

        public ActionResult EditClouse(int id)
        {

            if (id != 0)
            {
                var data = db.Clice.Where(i => i.Id == id).FirstOrDefault();

                ClouseModel obj = new ClouseModel();

                obj.Clouse1 = data.Clouse1;
                obj.Description = data.Description;
                var categories = db.DocumentCategories.Where(s => s.IsEnabled == true).ToList();
                var selectedCategories = db.ClouseandCategoryMapings.Where(s => s.clouseID == id).Select(i => i.categoryID).ToArray();
                obj.SelectedGroups = selectedCategories;
                obj.getAllCategory = categories;
                obj.Id = id;

                return View("EditClouse", obj);
            }
            else
            {
                ViewBag.Enable = true;
                return View("ClouseList", "ClouseManagement");
            }
        }


        public ActionResult ClouseList(string enable)
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

            List<Clouse> objClouse = new List<Clouse>();
            objClouse = db.Clice.Where(c=>c.IsEnabled==active).ToList();
        
            return View(objClouse);
        }


        [HttpPost]
        public JsonResult ActivateClouse(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            //Log Insert
            ClouseLog objLog = new ClouseLog();
            try
            {
                var obj = db.Clice.Find(id);
                if (obj != null)
                {
                    if (obj.IsEnabled == true)
                    {
                        objLog.Action = "Inactive";
                        obj.IsEnabled = false;
                        objLog.IsEnabled = false;
                        message = "Clouse Deactivated Successfully";                     
                    }
                    else
                    {
                        objLog.Action = "Active";
                        obj.IsEnabled = true;
                        objLog.IsEnabled = true;
                        message = "Clouse Activated Successfully";
                    }
                }
                objLog.ClouseDescription = obj.Description;
                objLog.ClouseId = obj.Id;
                objLog.ModifiedDate = DateTime.Now;
                db.ClouseLogs.Add(objLog);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                message = "An error occured while processing the request. Try again later";
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            }

            return Json(new { message = message }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult GetCloseDetails(int? id)
        {
            ClouseModel obj = new ClouseModel();
            var clouse = db.Clice.Where(i => i.Id == id).FirstOrDefault();
            obj.Clouse1 = clouse.Clouse1;
            obj.Description = clouse.Description;
            
            obj.Id = id.Value;
            var categories = db.DocumentCategories.Where(s => s.IsEnabled==true).ToList();
            var selectedCategories = db.ClouseandCategoryMapings.Where(c => c.clouseID == id.Value).Select(c=>c.categoryID).ToArray();
            obj.SelectedGroups = selectedCategories;
            obj.getAllCategory = categories;

            return View("EditClouse", obj);

        }

    
        public ActionResult UpdateClouse(ClouseModel obj)
        {
            try
            {
                var objClouse = db.Clice.Find(obj.Id);
                objClouse.Clouse1 = obj.Clouse1;
                objClouse.Description = obj.Description;

                var selectedCategory = db.ClouseandCategoryMapings.Where(c => c.clouseID == obj.Id).ToList();

                foreach (var ClouseandCategoryMapings in selectedCategory)
                {
                    db.ClouseandCategoryMapings.Remove(ClouseandCategoryMapings);
                    db.SaveChanges();
                }
          

                var clouse = db.Clice.Where(c => c.Id == obj.Id).FirstOrDefault();

                clouse.Clouse1 = obj.Clouse1; ;
                clouse.Description = obj.Description;


             
                for (int i = 0; i < obj.SelectedGroups.Count(); i++)
                {
                    ClouseandCategoryMaping objMaping = new ClouseandCategoryMaping();
                    objMaping.categoryID = obj.SelectedGroups[i];
                    objMaping.clouseID = obj.Id;
                    db.ClouseandCategoryMapings.Add(objMaping);
                    db.SaveChanges();
                }            

                //Log Insert
                ClouseLog objLog = new ClouseLog();
                objLog.IsEnabled = true;
                objLog.ClouseName = objClouse.Clouse1;
                objLog.ClouseId = objClouse.Id;
                objLog.Action = "Update";
                objLog.ClouseDescription = objClouse.Description;
                objLog.ModifiedDate = DateTime.Now;
                objLog.ModifiedBy = "";
                db.ClouseLogs.Add(objLog);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            ViewBag.Enable = true;
            return RedirectToAction("ClouseList", "ClouseManagement");

        }

    }
}