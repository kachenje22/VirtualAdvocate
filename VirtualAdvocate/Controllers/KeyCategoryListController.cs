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
    public class KeyCategoryListController : Controller
    {

        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();
        private VirtualAdvocateDocumentData objData = new VirtualAdvocateDocumentData();
        // GET: KeyCategoryList
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult KeyCategoryList(string enable)
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

            List<KeyCategoryModel> objCat = new List<KeyCategoryModel>();
            objCat = (from k  in db.KeyCategories where k.IsEnabled==active select  new KeyCategoryModel {ID=k.Id,CategoryName=k.CategoryName,CategoryDescription=k.CategoryDescription,IsEnabled=k.IsEnabled,Order=k.CategoryOrder }).ToList();
            return View(objCat);
        }

        public ActionResult AddkeyCategory()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddKeyCategory(KeyCategoryModel objCM)
        {
            try
            {
                KeyCategory obj = new KeyCategory();
                obj.IsEnabled = true;
                obj.CategoryName = objCM.CategoryName;
                obj.CategoryDescription = objCM.CategoryDescription;
                obj.CategoryOrder = objCM.Order;
                obj.CanAddInsurance = objCM.CanAddInsurance;
                db.KeyCategories.Add(obj);
                db.SaveChanges();
                int result = obj.Id;

                // Log Insert
                LogKeywordCategory objLog = new LogKeywordCategory();
                objLog.IsEnabled = true;
                objLog.ModifiedDate = DateTime.Now;
                objLog.Description = objCM.CategoryDescription;
                objLog.KeywordCategoryId = result;
                objLog.Action = "Insert";
                objLog.Name = objCM.CategoryName;
                db.LogKeywordCategories.Add(objLog);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("KeyCategoryList", "KeyCategoryList");
        }

        [HttpGet]
        public JsonResult CheckKeyCategory(string CategoryName)
        {
            var chkExisting = db.KeyCategories.Where(a => a.CategoryName == CategoryName.Trim()).FirstOrDefault();

            if (chkExisting != null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CheckKeyCategoryName(string CategoryName)
        {
            var chkexisting = db.KeyCategories.Where(a => a.CategoryName == CategoryName.Trim()).FirstOrDefault();
            bool result = false;
            if (chkexisting != null)
            {
                result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult EditKeyCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KeyCategory dc = new KeyCategory();
            var cat = db.KeyCategories.Where(d => d.Id == id).FirstOrDefault();
            EditKeyCategoryModel obj = new EditKeyCategoryModel();
            obj.CategoryName = cat.CategoryName;
            obj.CategoryDescription = cat.CategoryDescription;
            obj.Order = cat.CategoryOrder;
            obj.CanAddInsurance = cat.CanAddInsurance != null ? cat.CanAddInsurance.Value : false;
            obj.ID = cat.Id;
            return View(obj);
        }

        [HttpPost]
        public ActionResult EditKeyCategory(EditKeyCategoryModel obj)
        {
            try
            {
                KeyCategory dc = new KeyCategory();
                var cat = db.KeyCategories.Where(d => d.Id == obj.ID).FirstOrDefault();

                cat.CategoryName = obj.CategoryName;
                cat.CategoryDescription = obj.CategoryDescription;
                cat.CategoryOrder = obj.Order;
                cat.CanAddInsurance = obj.CanAddInsurance;
                db.SaveChanges();

                ////Log Insert
                LogKeywordCategory objLog = new LogKeywordCategory();
                objLog.IsEnabled = true;
                objLog.ModifiedDate = DateTime.Now;
                objLog.Description = obj.CategoryDescription;
                objLog.Action = "Update";
                objLog.LogId = obj.ID;
                objLog.Name = obj.CategoryName;
                db.LogKeywordCategories.Add(objLog);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("KeyCategoryList", "KeyCategoryList");
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult ActivateKeyCategory(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            //Log Insert
            LogKeywordCategory objLog = new LogKeywordCategory();
            try
            {
                var obj = db.KeyCategories.Find(id);
                if (obj != null)
                {
                    if (obj.IsEnabled == true)
                    {
                        objLog.Action = "Inactive";
                        obj.IsEnabled = false;
                        objLog.IsEnabled = false;
                        message = "Document Category Deactivated Successfully";
                    }
                    else
                    {
                        objLog.Action = "Active";
                        obj.IsEnabled = true;
                        objLog.IsEnabled = true;
                        message = "Document Category Activated Successfully";
                    }
                }



                objLog.Name = obj.CategoryName;
                objLog.Description = obj.CategoryDescription;
                objLog.LogId = obj.Id;
                objLog.ModifiedDate = DateTime.Now;
                db.LogKeywordCategories.Add(objLog);
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

    }
}