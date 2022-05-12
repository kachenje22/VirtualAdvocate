#region NameSpaces
using Code7248.word_reader;
using DocumentFormat.OpenXml.Packaging;
using EntityFramework.Extensions;
using Microsoft.Office.Interop.Word;
using OpenXmlPowerTools;
using SelectPdf;
using Spire.Doc;
using Spire.Doc.Documents;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using VirtualAdvocate.Common;
using VirtualAdvocate.DAL;
using VirtualAdvocate.Helpers;
using VirtualAdvocate.Models;
using Document = Spire.Doc.Document;
using Paragraph = Spire.Doc.Documents.Paragraph;
using Section = Spire.Doc.Section;
using Word = Microsoft.Office.Interop.Word;
#endregion
#region VirtualAdvocate.Controllers
namespace VirtualAdvocate.Controllers
{
    #region DocumentManagementBackupController
    public class DocumentManagementBackupController : BaseController
    {
        #region NameSpaces
        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();
        private VirtualAdvocateDocumentData objData = new VirtualAdvocateDocumentData();
        public int userID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
        public int orgId = Convert.ToInt32(System.Web.HttpContext.Current.Session["OrgId"]);
        public int deptID = Convert.ToInt32(System.Web.HttpContext.Current.Session["DepartmentID"]);
        public int roleId = Convert.ToInt32(System.Web.HttpContext.Current.Session["RoleId"]);

        #endregion

        #region Category

        #region Index
        // GET: DocumentManagement
        public ActionResult Index(string enable)
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
            List<DocumentCategoryModel> objCat = new List<DocumentCategoryModel>();
            objCat = (from d in db.DocumentCategories
                      where d.IsEnabled == active
                      select new DocumentCategoryModel
                      {
                          ServiceId = d.ServiceId,
                          IsEnabled = d.IsEnabled,
                          DocumentCategoryName = d.DocumentCategoryName,
                          DocumentCategoryId = d.DocumentCategoryId,
                          DocumentCategoryDescription = d.DocumentCategoryDescription

                      }).ToList();
            return View(objCat);
        }
        #endregion

        #region AddCategory
        [HttpGet]
        public ActionResult AddCategory()
        {
            DocumentCategoryModel obj = new DocumentCategoryModel();
            obj.getAllServices = objData.getAllServices();
            return View(obj);
        }
        #endregion

        #region AddCategory
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult AddCategory(DocumentCategoryModel objCM,
         HttpPostedFileBase uploadFile)
        {
            try
            {
                DocumentCategory obj = new DocumentCategory();
                obj.IsEnabled = true;
                obj.DocumentCategoryName = objCM.DocumentCategoryName;
                obj.DocumentCategoryDescription = objCM.DocumentCategoryDescription;
                obj.ServiceId = objCM.ServiceId;
                if (uploadFile != null)
                {
                    if (uploadFile.ContentLength > 0)
                    {
                        string relativePath = "~/Images/Category/" + DateTime.Now.ToString("yyyyMMdd-HHMMss") + Path.GetFileName(uploadFile.FileName);
                        string physicalPath = Server.MapPath(relativePath);
                        uploadFile.SaveAs(physicalPath);
                        obj.ImagePath = relativePath.Replace("~", ConfigurationManager.AppSettings["PublishName"].ToString());
                    }
                }

                db.DocumentCategories.Add(obj);
                db.SaveChanges();
                int result = obj.DocumentCategoryId;

                //Log Insert
                LogDocumentCategory objLog = new LogDocumentCategory();
                objLog.IsEnabled = true;
                objLog.DocumentCategoryName = objCM.DocumentCategoryName;
                objLog.DocumentCategoryDescription = objCM.DocumentCategoryDescription;
                objLog.ServiceId = objCM.ServiceId;
                objLog.Action = "Insert";
                objLog.ModifiedDate = DateTime.Now;
                objLog.DocumentCategoryId = result;
                db.LogDocumentCategories.Add(objLog);
                db.SaveChanges();


            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("Index", "DocumentManagement");
        }
        #endregion

        #region EditCategory
        [HttpGet]
        public ActionResult EditCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentCategory dc = new DocumentCategory();
            dc = db.DocumentCategories.Find(id);
            DocumentCategoryModel obj = new DocumentCategoryModel();
            obj.DocumentCategoryName = dc.DocumentCategoryName;
            obj.DocumentCategoryDescription = dc.DocumentCategoryDescription;
            obj.ServiceId = dc.ServiceId;
            obj.ImagePath = dc.ImagePath;
            obj.DocumentCategoryId = dc.DocumentCategoryId;
            obj.getAllServices = objData.getAllServices();
            return View(obj);
        }
        #endregion

        #region EditCategory
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult EditCategory(DocumentCategoryModel obj,
        HttpPostedFileBase uploadFile)
        {
            try
            {
                DocumentCategory dc = new DocumentCategory();
                dc = db.DocumentCategories.Find(obj.DocumentCategoryId);
                dc.DocumentCategoryName = obj.DocumentCategoryName;
                dc.DocumentCategoryDescription = obj.DocumentCategoryDescription;
                dc.ServiceId = obj.ServiceId;
                if (uploadFile != null)
                {
                    if (uploadFile.ContentLength > 0)
                    {
                        string relativePath = "~/Images/Category/" + DateTime.Now.ToString("yyyyMMdd-HHMMss") + Path.GetFileName(uploadFile.FileName);
                        string physicalPath = Server.MapPath(relativePath);
                        uploadFile.SaveAs(physicalPath);
                        dc.ImagePath = relativePath.Replace("~", ConfigurationManager.AppSettings["PublishName"].ToString());
                    }
                }
                db.SaveChanges();

                //Log Insert
                LogDocumentCategory objLog = new LogDocumentCategory();
                objLog.IsEnabled = true;
                objLog.DocumentCategoryName = obj.DocumentCategoryName;
                objLog.DocumentCategoryDescription = obj.DocumentCategoryDescription;
                objLog.ServiceId = obj.ServiceId;
                objLog.Action = "Update";
                objLog.ModifiedDate = DateTime.Now;
                objLog.DocumentCategoryId = obj.DocumentCategoryId;
                db.LogDocumentCategories.Add(objLog);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("Index", "DocumentManagement");
        }
        #endregion

        #region ActivateCategory
        [AllowAnonymous]
        [HttpPost]
        public JsonResult ActivateCategory(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            //Log Insert
            LogDocumentCategory objLog = new LogDocumentCategory();
            try
            {
                var obj = db.DocumentCategories.Find(id);
                if (obj != null)
                {
                    if (obj.IsEnabled == true)
                    {
                        objLog.Action = "Inactive";
                        obj.IsEnabled = false;
                        objLog.IsEnabled = false;

                        message = "Document Category Deactivated Successfully";

                        var subCategoryobj = db.DocumentSubCategories.Where(s => s.DocumentCategoryId == id).ToList();


                        subCategoryobj.ForEach((a) =>
                        {
                            a.IsEnabled = false;
                        });

                        foreach (DocumentSubCategory subObj in subCategoryobj)
                        {
                            var subsubCategoryobj = db.DocumentSubSubCategories.Where(s => s.DocumentSubCategoryId == subObj.DocumentSubCategoryId).ToList();

                            subsubCategoryobj.ForEach((a) =>
                            {
                                a.IsEnabled = false;
                            });

                            var SubDocObj = db.DocumentTemplates.Where(d => d.DocumentSubCategory == subObj.DocumentSubCategoryId).ToList();

                            SubDocObj.ForEach((a) =>
                            {
                                a.IsEnabled = false;
                            });
                            foreach (DocumentSubSubCategory subSubObj in subsubCategoryobj)
                            {
                                var subSubDocObj = db.DocumentTemplates.Where(d => d.DocumentSubSubCategory == subSubObj.DocumentSubSubCategoryId).ToList();

                                subSubDocObj.ForEach((a) =>
                                {
                                    a.IsEnabled = false;
                                });
                            }
                        }

                        var cateuserObj = db.DocumentTemplates.Where(d => d.DocumentCategory == id).ToList();

                        cateuserObj.ForEach((a) =>
                        {
                            a.IsEnabled = false;
                        });
                    }
                    else
                    {
                        objLog.Action = "Active";
                        obj.IsEnabled = true;
                        objLog.IsEnabled = true;
                        message = "Document Category Activated Successfully";
                    }
                }

                objLog.DocumentCategoryName = obj.DocumentCategoryName;
                objLog.DocumentCategoryDescription = obj.DocumentCategoryDescription;
                objLog.ServiceId = obj.ServiceId;
                objLog.ModifiedDate = DateTime.Now;
                objLog.DocumentCategoryId = obj.DocumentCategoryId;
                db.LogDocumentCategories.Add(objLog);
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
        #endregion

        #region DeleteCategory
        [HttpPost]
        public ActionResult DeleteCategory(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            //Log Insert
            LogDocumentCategory objLog = new LogDocumentCategory();
            try
            {
                var obj = db.DocumentCategories.Find(id);
                if (obj != null)
                {
                    db.ClouseandCategoryMapings.Where(d => d.categoryID == id).Delete();
                    db.SaveChanges();

                    var subCategoryobj = db.DocumentSubCategories.Where(s => s.DocumentCategoryId == id).ToList();


                    foreach (DocumentSubCategory subObj in subCategoryobj)
                    {
                        var subsubCategoryobj = db.DocumentSubSubCategories.Where(s => s.DocumentSubCategoryId == subObj.DocumentSubCategoryId).ToList();

                        db.DocumentSubSubCategories.Where(s => s.DocumentSubCategoryId == subObj.DocumentSubCategoryId).Delete();

                        var SubDocObj = db.DocumentTemplates.Where(d => d.DocumentSubCategory == subObj.DocumentSubCategoryId).ToList();
                        db.DocumentTemplates.Where(d => d.DocumentSubCategory == subObj.DocumentSubCategoryId).Delete();
                        db.SaveChanges();
                        foreach (DocumentSubSubCategory subSubObj in subsubCategoryobj)
                        {
                            var subSubDocObj = db.DocumentTemplates.Where(d => d.DocumentSubSubCategory == subSubObj.DocumentSubSubCategoryId).ToList();

                            db.DocumentTemplates.Where(d => d.DocumentSubSubCategory == subSubObj.DocumentSubSubCategoryId).Delete();
                            db.SaveChanges();
                        }

                    }
                    db.SaveChanges();
                    db.DocumentSubCategories.Where(s => s.DocumentCategoryId == id).Delete();
                    db.SaveChanges();

                    var cateuserObj = db.DocumentTemplates.Where(d => d.DocumentCategory == id).ToList();
                    db.DocumentTemplates.Where(d => d.DocumentCategory == id).Delete();

                    db.DocumentCategories.Where(d => d.DocumentCategoryId == id).Delete();
                    objLog.Action = "Delete";
                    message = "Document Category Deleted Successfully";
                }
                objLog.DocumentCategoryName = obj.DocumentCategoryName;
                objLog.DocumentCategoryDescription = obj.DocumentCategoryDescription;
                objLog.ServiceId = obj.ServiceId;
                objLog.ModifiedDate = DateTime.Now;
                objLog.DocumentCategoryId = obj.DocumentCategoryId;
                db.LogDocumentCategories.Add(objLog);
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
        #endregion

        #endregion

        #region Sub Category

        #region SubCategoryList
        public ActionResult SubCategoryList(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<DocumentSubCategory> objCat = new List<DocumentSubCategory>();
            objCat = objData.GetDocumentSubCategories().Where(m => m.DocumentCategoryId == id).ToList();
            DocumentCategory objPCat = db.DocumentCategories.Find(id);
            ViewBag.Title = "Category  >> " + objPCat.DocumentCategoryName;
            TempData["SubCategoryId"] = id;
            return View(objCat);
        }
        #endregion

        #region AddSubCategory
        [HttpGet]
        public ActionResult AddSubCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentSubCategoryModel dc = new DocumentSubCategoryModel();
            dc.DocumentCategoryId = id.Value;
            dc.getAllCategory = objData.getCategoryOptionsList();
            DocumentCategory objPCat = db.DocumentCategories.Find(id);
            ViewBag.Title = "Category  >> " + objPCat.DocumentCategoryName;
            return View(dc);
        }
        #endregion

        #region AddSubCategory
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult AddSubCategory(DocumentSubCategoryModel obj,
        HttpPostedFileBase uploadFile)
        {
            try
            {
                DocumentSubCategory dsc = new DocumentSubCategory();
                dsc.DocumentCategoryId = obj.DocumentCategoryId;
                dsc.DocumentSubCategoryName = obj.DocumentSubCategoryName;
                dsc.SubCategoryDescription = obj.DocumentSubCategoryDescription;
                dsc.IsEnabled = true;
                if (uploadFile != null)
                {
                    if (uploadFile.ContentLength > 0)
                    {
                        string relativePath = "~/Images/SubCategory/" + DateTime.Now.ToString("yyyyMMdd-HHMMss") + Path.GetFileName(uploadFile.FileName);
                        string physicalPath = Server.MapPath(relativePath);
                        uploadFile.SaveAs(physicalPath);
                        dsc.ImagePath = relativePath.Replace("~", ConfigurationManager.AppSettings["PublishName"].ToString());
                    }
                }
                db.DocumentSubCategories.Add(dsc);
                db.SaveChanges();
                int result = dsc.DocumentSubCategoryId;

                //Log Insert
                LogDocumentSubCategory objLog = new LogDocumentSubCategory();
                objLog.IsEnabled = true;
                objLog.DocumentCategoryId = obj.DocumentCategoryId;
                objLog.DocumentSubCategoryName = obj.DocumentSubCategoryName;
                objLog.SubCategoryDescription = obj.DocumentSubCategoryDescription;
                objLog.Action = "Insert";
                objLog.ModifiedDate = DateTime.Now;
                objLog.DocumentSubCategoryId = result;
                db.LogDocumentSubCategories.Add(objLog);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("SubCategoryList",
            "DocumentManagement", new { id = obj.DocumentCategoryId });
        }
        #endregion

        #region EditSubCategory
        [HttpGet]
        public ActionResult EditSubCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentSubCategory dc = new DocumentSubCategory();
            dc = db.DocumentSubCategories.Find(id);

            DocumentCategory objPCat = db.DocumentCategories.Find(dc.DocumentCategoryId);
            ViewBag.Title = "Category  >> " + objPCat.DocumentCategoryName;

            DocumentSubCategoryModel dsc = new DocumentSubCategoryModel();
            dsc.DocumentSubCategoryId = id.Value;
            dsc.DocumentCategoryId = dc.DocumentCategoryId;
            dsc.DocumentSubCategoryName = dc.DocumentSubCategoryName;
            dsc.DocumentSubCategoryDescription = dc.SubCategoryDescription;
            dsc.ImagePath = dc.ImagePath;
            dsc.getAllCategory = objData.getCategoryOptionsList();
            return View(dsc);
        }
        #endregion

        #region EditSubCategory
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult EditSubCategory(DocumentSubCategoryModel obj,
        HttpPostedFileBase uploadFile)
        {
            try
            {
                DocumentSubCategory dc = new DocumentSubCategory();
                dc = db.DocumentSubCategories.Find(obj.DocumentSubCategoryId);
                dc.DocumentSubCategoryName = obj.DocumentSubCategoryName;
                dc.SubCategoryDescription = obj.DocumentSubCategoryDescription;
                dc.DocumentCategoryId = obj.DocumentCategoryId;

                if (uploadFile != null)
                {
                    if (uploadFile.ContentLength > 0)
                    {
                        string relativePath = "~/Images/SubCategory/" + DateTime.Now.ToString("yyyyMMdd-HHMMss") + Path.GetFileName(uploadFile.FileName);
                        string physicalPath = Server.MapPath(relativePath);
                        uploadFile.SaveAs(physicalPath);
                        dc.ImagePath = relativePath.Replace("~", ConfigurationManager.AppSettings["PublishName"].ToString());
                    }
                }
                db.SaveChanges();
                //Log Insert
                LogDocumentSubCategory objLog = new LogDocumentSubCategory();
                objLog.IsEnabled = true;
                objLog.DocumentCategoryId = obj.DocumentCategoryId;
                objLog.DocumentSubCategoryName = obj.DocumentSubCategoryName;
                objLog.SubCategoryDescription = obj.DocumentSubCategoryDescription;
                objLog.Action = "Update";
                objLog.ModifiedDate = DateTime.Now;
                objLog.DocumentSubCategoryId = obj.DocumentSubCategoryId;
                db.LogDocumentSubCategories.Add(objLog);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("SubCategoryList", "DocumentManagement", new { id = obj.DocumentCategoryId });
        }
        #endregion

        #region ActivateSubCategory
        [AllowAnonymous]
        [HttpPost]
        public JsonResult ActivateSubCategory(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            try
            {
                var obj = db.DocumentSubCategories.Find(id);
                if (obj != null)
                {
                    //Log Insert
                    LogDocumentSubCategory objLog = new LogDocumentSubCategory();

                    if (obj.IsEnabled == true)
                    {
                        objLog.Action = "Inactive";
                        obj.IsEnabled = false;
                        objLog.IsEnabled = false;
                        message = "Document Sub Category Deactivated Successfully";

                        var cateuserObj = db.DocumentTemplates.Where(d => d.DocumentSubCategory == id).ToList();

                        cateuserObj.ForEach((a) =>
                        {
                            a.IsEnabled = false;
                        });
                        var associateddocument = db.AssociateTemplateDetails.Where(s => s.AssociateTemplateId == id).ToList();
                        associateddocument.ForEach((a) =>
                        {
                            a.IsEnabled = false;
                        });
                        var subsubCategoryobj = db.DocumentSubSubCategories.Where(s => s.DocumentSubCategoryId == id).ToList();

                        subsubCategoryobj.ForEach((a) =>
                        {
                            a.IsEnabled = false;
                        });

                    }
                    else
                    {
                        objLog.Action = "Active";
                        obj.IsEnabled = true;
                        objLog.IsEnabled = true;
                        message = "Document Sub Category Activated Successfully";
                    }



                    objLog.DocumentCategoryId = obj.DocumentCategoryId;
                    objLog.DocumentSubCategoryName = obj.DocumentSubCategoryName;
                    objLog.SubCategoryDescription = obj.SubCategoryDescription;
                    objLog.ModifiedDate = DateTime.Now;
                    objLog.DocumentSubCategoryId = id;
                    db.LogDocumentSubCategories.Add(objLog);
                }


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
        #endregion

        #endregion

        #region Sub Sub Category

        #region SubSubCategoryList
        public ActionResult SubSubCategoryList(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<DocumentSubSubCategory> objCat = new List<DocumentSubSubCategory>();
            objCat = objData.GetDocumentSubSubCategories(id);
            TempData["SubCategoryId"] = id;
            DocumentSubCategory dsc = db.DocumentSubCategories.Find(id);
            TempData["Back"] = dsc.DocumentCategoryId;

            DocumentCategory objPCat = db.DocumentCategories.Find(dsc.DocumentCategoryId);
            ViewBag.Title = "Category  >> " + objPCat.DocumentCategoryName + " >> " + dsc.DocumentSubCategoryName;

            return View(objCat);
        }
        #endregion

        #region AddSubSubCategory
        [HttpGet]
        public ActionResult AddSubSubCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentSubSubCategoryModel dc = new DocumentSubSubCategoryModel();
            dc.DocumentSubCategoryId = id.Value;
            DocumentSubCategory dcs = db.DocumentSubCategories.Find(id);
            dc.getAllSubCategory = objData.getSubCategoryOptionsList(dcs.DocumentCategoryId);

            DocumentCategory objPCat = db.DocumentCategories.Find(dcs.DocumentCategoryId);
            ViewBag.Title = "Category  >> " + objPCat.DocumentCategoryName + " >> " + dcs.DocumentSubCategoryName;
            return View(dc);
        }
        #endregion

        #region AddSubSubCategory
        [HttpPost]
        public ActionResult AddSubSubCategory(DocumentSubSubCategoryModel obj,
       HttpPostedFileBase uploadFile)
        {
            try
            {
                DocumentSubSubCategory dsc = new DocumentSubSubCategory();
                dsc.DocumentSubCategoryId = obj.DocumentSubCategoryId;
                dsc.SubDocumentCategoryName = obj.DocumentSubCategoryName;
                dsc.SubSubCategoryDescription = obj.DocumentSubCategoryDescription;
                dsc.IsEnabled = true;
                if (uploadFile != null)
                {
                    if (uploadFile.ContentLength > 0)
                    {
                        string relativePath = "~/Images/SubSubCategory/" + DateTime.Now.ToString("yyyyMMdd-HHMMss") + Path.GetFileName(uploadFile.FileName);
                        string physicalPath = Server.MapPath(relativePath);
                        uploadFile.SaveAs(physicalPath);
                        dsc.ImagePath = relativePath.Replace("~", ConfigurationManager.AppSettings["PublishName"].ToString());
                        Logger(dsc.ImagePath);
                    }
                }
                db.DocumentSubSubCategories.Add(dsc);
                db.SaveChanges();

                int result = dsc.DocumentSubSubCategoryId;
                //Log Insert
                LogSubSubCategory objLog = new LogSubSubCategory();
                objLog.IsEnabled = true;
                objLog.DocumentSubSubCategoryId = result;
                objLog.DocumentSubCategoryId = obj.DocumentSubCategoryId;
                objLog.SubSubCategoryDescription = obj.DocumentSubCategoryDescription;
                objLog.SubDocumentCategoryName = obj.DocumentSubCategoryName;
                objLog.Action = "Insert";
                objLog.ModifiedDate = DateTime.Now;
                db.LogSubSubCategories.Add(objLog);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                Logger(ex.Message);
            }
            return RedirectToAction("SubSubCategoryList", "DocumentManagement", new { id = obj.DocumentSubCategoryId });
        }
        #endregion

        #region EditSubSubCategory
        [HttpGet]
        public ActionResult EditSubSubCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentSubSubCategoryModel dsc = new DocumentSubSubCategoryModel();
            try
            {
                DocumentSubSubCategory dc = new DocumentSubSubCategory();
                dc = db.DocumentSubSubCategories.Find(id);
                dsc.DocumentSubCategoryId = dc.DocumentSubCategoryId;
                dsc.DocumentSubSubCategoryId = dc.DocumentSubSubCategoryId;
                dsc.DocumentSubCategoryName = dc.SubDocumentCategoryName;
                dsc.DocumentSubCategoryDescription = dc.SubSubCategoryDescription;
                dsc.ImagePath = dc.ImagePath;
                DocumentSubCategory dcs = db.DocumentSubCategories.Find(dc.DocumentSubCategoryId);
                dsc.getAllSubCategory = objData.getSubCategoryOptionsList(dcs.DocumentCategoryId);
                DocumentCategory objPCat = db.DocumentCategories.Find(dcs.DocumentCategoryId);
                ViewBag.Title = "Category  >> " + objPCat.DocumentCategoryName + " >> " + dcs.DocumentSubCategoryName;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(dsc);
        }
        #endregion

        #region EditSubSubCategory
        [HttpPost]
        public ActionResult EditSubSubCategory(DocumentSubSubCategoryModel obj,
         HttpPostedFileBase uploadFile)
        {
            try
            {
                DocumentSubSubCategory dc = new DocumentSubSubCategory();
                dc = db.DocumentSubSubCategories.Find(obj.DocumentSubSubCategoryId);
                dc.SubDocumentCategoryName = obj.DocumentSubCategoryName;
                dc.SubSubCategoryDescription = obj.DocumentSubCategoryDescription;
                if (uploadFile != null)
                {
                    if (uploadFile.ContentLength > 0)
                    {
                        string relativePath = "~/Images/SubSubCategory/" + DateTime.Now.ToString("yyyyMMdd-HHMMss") + Path.GetFileName(uploadFile.FileName);
                        string physicalPath = Server.MapPath(relativePath);
                        uploadFile.SaveAs(physicalPath);
                        dc.ImagePath = relativePath.Replace("~", ConfigurationManager.AppSettings["PublishName"].ToString());
                    }
                }
                dc.DocumentSubCategoryId = obj.DocumentSubCategoryId;
                db.SaveChanges();

                //Log Insert
                LogSubSubCategory objLog = new LogSubSubCategory();
                objLog.IsEnabled = true;
                objLog.DocumentSubSubCategoryId = obj.DocumentSubSubCategoryId;
                objLog.DocumentSubCategoryId = obj.DocumentSubCategoryId;
                objLog.SubSubCategoryDescription = obj.DocumentSubCategoryDescription;
                objLog.SubDocumentCategoryName = obj.DocumentSubCategoryName;
                objLog.Action = "Update";
                objLog.ModifiedDate = DateTime.Now;
                db.LogSubSubCategories.Add(objLog);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("SubSubCategoryList", "DocumentManagement", new { id = obj.DocumentSubCategoryId });
        }
        #endregion

        #region ActivateSubSubCategory
        [AllowAnonymous]
        [HttpPost]
        public JsonResult ActivateSubSubCategory(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            try
            {
                var obj = db.DocumentSubSubCategories.Find(id);
                if (obj != null)
                {
                    //Log Insert
                    LogSubSubCategory objLog = new LogSubSubCategory();

                    if (obj.IsEnabled == true)
                    {
                        objLog.Action = "Inactive";
                        objLog.IsEnabled = false;
                        obj.IsEnabled = false;
                        message = "Document Sub Category Deactivated Successfully";

                        var cateuserObj = db.DocumentTemplates.Where(d => d.DocumentSubSubCategory == id).ToList();

                        cateuserObj.ForEach((a) =>
                        {
                            a.IsEnabled = false;
                        });
                        var associateddocument = db.AssociateTemplateDetails.Where(s => s.AssociateTemplateId == id).ToList();
                        associateddocument.ForEach((a) =>
                        {
                            a.IsEnabled = false;
                        });
                    }
                    else
                    {
                        objLog.IsEnabled = true;
                        obj.IsEnabled = true;
                        objLog.Action = "Active";
                        message = "Document Sub Category Activated Successfully";
                    }

                    objLog.DocumentSubSubCategoryId = obj.DocumentSubSubCategoryId;
                    objLog.DocumentSubCategoryId = obj.DocumentSubCategoryId;
                    objLog.SubSubCategoryDescription = obj.SubSubCategoryDescription;
                    objLog.SubDocumentCategoryName = obj.SubDocumentCategoryName;
                    objLog.ModifiedDate = DateTime.Now;
                    db.LogSubSubCategories.Add(objLog);

                }
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
        #endregion

        #endregion

        #region Document Upload

        #region Templates
        public ActionResult Templates(string enable)
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

            DocumentTemplateListModel objTemplate = new DocumentTemplateListModel();
            try
            {
                var objTemplates = (from ut in db.DocumentTemplates
                                    join dc in db.DocumentCategories on ut.DocumentCategory equals dc.DocumentCategoryId
                                    where ut.IsEnabled == active
                                    select new DocumentTemplateListModel { TemplateName = ut.DocumentTitle, TemplateId = ut.TemplateId, DocumentFileName = ut.TemplateFileName, DocumentCategory = dc.DocumentCategoryName, Cost = ut.TemplateCost, AssociatedDocumentId = ut.AssociateTemplateId, AssociatedDocument = null, IsEnabled = ut.IsEnabled }
                    );

                var query = objTemplates.Select(p => new DocumentTemplateListModel
                {
                    TemplateName = p.TemplateName,
                    TemplateId = p.TemplateId,
                    DocumentFileName = p.DocumentFileName,
                    DocumentCategory = p.DocumentCategory,
                    Cost = p.Cost,
                    AssociatedDocumentId = p.AssociatedDocumentId,
                    AssociatedDocument = (from utt in db.DocumentTemplates where utt.TemplateId == p.AssociatedDocumentId select utt.DocumentTitle).FirstOrDefault(),
                    IsEnabled = p.IsEnabled
                });

                return View(query);

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View(objTemplate);
        }
        #endregion

        #region getTemplateDetails
        private DocumentTemplate getTemplateDetails(EditDocumentUploadModel objTemplates)
        {
            DocumentTemplate obj = db.DocumentTemplates.Where(dc => dc.TemplateId == objTemplates.TemplateId).FirstOrDefault();
            obj.TemplateId = Convert.ToInt32(objTemplates.TemplateId);
            // obj.TemplateFileName = objTemplates.TemplateName;
            obj.DocumentType = objTemplates.DocumentType;
            obj.DocumentTitle = objTemplates.DocumentTitle;
            obj.DocumentCategory = obj.DocumentCategory;
            obj.DocumentSubCategory = obj.DocumentSubCategory;
            obj.DocumentSubSubCategory = obj.DocumentSubSubCategory;
            obj.DocumentDescription = objTemplates.DocumentDescription;
            obj.TemplateCost = Convert.ToDecimal(objTemplates.Cost);
            obj.AssociateTemplateId = objTemplates.AssociateTemplateId;
            obj.Mandatory = objTemplates.Mandatory;
            obj.ModifiedDate = DateTime.Now;
            return obj;
        }
        #endregion

        #region RestoreTempKeyValues
        public void RestoreTempKeyValues(List<TemplateKeysPointer> obj)
        {
            TemplateKeysPointer TempKeyObj = new TemplateKeysPointer();
            if (obj != null)
            {
                foreach (var dis in obj) // Restore the existing key values
                {
                    TempKeyObj.TemplateId = dis.TemplateId;
                    TempKeyObj.TemplateKeyId = dis.TemplateKeyId;
                    TempKeyObj.IsEnabled = dis.IsEnabled;
                    db.SaveChanges();
                }
            }

        }
        #endregion

        #region EditTemplates_old
        public ActionResult EditTemplates_old(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EditDocumentUploadModel obj = new EditDocumentUploadModel();
            try
            {
                DocumentTemplate objTemplates = db.DocumentTemplates.Where(dc => dc.TemplateId == id).FirstOrDefault();
                obj.TemplateId = id;
                obj.TemplateName = objTemplates.TemplateFileName;
                obj.DocumentType = objTemplates.DocumentType;
                obj.DocumentTitle = objTemplates.DocumentTitle;
                obj.DocumentCategoryId = objTemplates.DocumentCategory;
                obj.DocumentSubCategoryId = objTemplates.DocumentSubCategory;
                obj.DocumentSubSubCategoryId = objTemplates.DocumentSubSubCategory;
                obj.DocumentDescription = objTemplates.DocumentDescription;
                obj.Cost = Convert.ToDecimal(objTemplates.TemplateCost);
                obj.AssociateTemplateId = objTemplates.AssociateTemplateId;
                List<OptionsModel> objOptions = new List<OptionsModel>();
                obj.getAllCategory = objData.getCategoryOptionsList();
                if (objTemplates.DocumentCategory != 0)
                {
                    obj.getAllSubCategory = objData.getSubCategoryOptionsList(objTemplates.DocumentCategory);
                    obj.getDocumentList = objData.getTemplateList(objTemplates.DocumentCategory).Where(x => x.ID != id.Value);
                }
                else
                {
                    obj.getAllSubCategory = objOptions;
                    obj.getDocumentList = objOptions;
                }
                if (obj.DocumentSubCategoryId != null)
                {
                    obj.getAllSubSubCategory = objData.getSubSubCategoryOptionsList(obj.DocumentSubCategoryId);
                }
                else
                {
                    obj.getAllSubSubCategory = objOptions;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            return View(obj);
        }
        #endregion

        #region EditTemplates_old
        [HttpPost]
        public ActionResult EditTemplates_old(EditDocumentUploadModel objTemplates)
        {
            var err = 1;
            string filename = "";
            DocumentTemplate obj = new DocumentTemplate();
            try
            {
                obj = getTemplateDetails(objTemplates);
                filename = objTemplates.TemplateName;

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                return RedirectToAction("EditTemplates", "DocumentManagement", new { Id = objTemplates.TemplateId });
            }
            try
            {

                if (objTemplates.TemplateFile != null && objTemplates.TemplateFile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(Request.Files[0].FileName).ToLower();
                    //obj.TemplateFileName = Path.GetFileName(objTemplates.TemplateFile.FileName);
                    filename = Path.GetFileName(objTemplates.TemplateFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/TemplateFiles/"), Path.GetFileName(objTemplates.TemplateFile.FileName));
                    var existFilepath = Path.Combine(Server.MapPath("~/TemplateFiles/"), obj.TemplateFileName);
                    var path1 = Path.Combine(Server.MapPath("~/TemplateFiles/Archive"), Path.GetFileName(objTemplates.TemplateFile.FileName));
                    if (System.IO.File.Exists(path) && (obj.TemplateFileName != Path.GetFileName(objTemplates.TemplateFile.FileName)))
                    {
                        this.ModelState.AddModelError("TemplateFile", "File Name Already exists. Please Upload With Differnt File Name");
                        err = 0;
                    }
                    else if (extension != ".doc" && extension != ".docx")
                    {
                        ModelState.AddModelError("TemplateFile", "Invalid File.Supported file extensions: doc, docx");
                    }
                    else
                    {
                        if (System.IO.File.Exists(path1))
                        {
                            System.IO.File.Delete(path1); //Delete Old Archive File 
                        }
                        if (System.IO.File.Exists(existFilepath))
                        {
                            System.IO.File.Copy(existFilepath, path1, true); // Existing File copy to Archive Folder
                            System.IO.File.Delete(existFilepath); //Delete Old File From TemplateFiles Folder
                        }

                        obj.TemplateFileName = Path.GetFileName(objTemplates.TemplateFile.FileName);
                        objTemplates.TemplateFile.SaveAs(path); // New File saving to TemplateFiles Folder
                        err = 1;
                        List<string> lst = new List<string>();
                        lst = getKeyFields(obj.TemplateFileName); // Getting List of keywords from the document
                        TemplateKeysPointer objTKP = new TemplateKeysPointer();
                        objTKP.TemplateId = obj.TemplateId;
                        var objDisableTKP = db.TemplateKeysPointers.Where(d => d.TemplateId == obj.TemplateId); //Getting previous key value id's for roll back process

                        //ArchiveTemplateKeysPointer objATKP = new ArchiveTemplateKeysPointer();
                        ////Export key id's into archive table for exception Rollback
                        //foreach (var dis in objDisableTKP)
                        //{
                        //    objATKP.TemplateId = dis.TemplateId;
                        //    objATKP.TemplateKeyId = dis.TemplateKeyId;
                        //    objATKP.IsEnabled = dis.IsEnabled;
                        //    db.SaveChanges();
                        //}

                        //Delete previous key value id's
                        db.TemplateKeysPointers.Where(d => d.TemplateId == obj.TemplateId).Delete();//All Key id's deleted before update
                        int errCount = 0;
                        foreach (var li in lst)
                        {
                            var TempKeyobj = objData.getKeyFieldId(li.Trim(new Char[] { '<', '>' })); // Fetch Keyword Id and save it
                            if (TempKeyobj != null)
                            {
                                objTKP.TemplateKeyId = Convert.ToInt32(TempKeyobj.TemplateKeyId);
                                objTKP.IsEnabled = true;
                                db.TemplateKeysPointers.Add(objTKP);
                                db.SaveChanges(); // All the Key Id with Template Id 
                            }
                            else
                            {
                                errCount = errCount + 1;
                                if (errCount == 1)
                                    ModelState.AddModelError("", "The Following Keyword's doesn't exist. Please Create The Keyword Before Uploading This Template.");
                                ModelState.AddModelError("TemplateFile", li);
                                err = 0;
                            }
                        }
                        if (err == 0) // If any exceptions (Rollback) - Delete Created Template Details 
                        {
                            System.IO.File.Delete(path); //Delete New File
                            System.IO.File.Copy(path1, existFilepath, true); // Existing File Copy From Archive Folder                           
                            RestoreTempKeyValues(objDisableTKP.ToList()); // Restore existing key values
                        }
                    }

                }
                //else {err = 1; }

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                err = 0;
            }

            if (err == 1)
            {
                //int result = objData.EditTemplate(obj);

                LogTemplateUpload objLog = new LogTemplateUpload();
                objLog.Action = "Update";
                objLog.DocumentTitle = obj.DocumentTitle;
                objLog.DocumentDescription = obj.DocumentDescription;
                objLog.DocumentType = obj.DocumentType;
                objLog.TemplateCost = obj.TemplateCost;
                objLog.DocumentCategory = obj.DocumentCategory;
                objLog.DocumentSubCategory = obj.DocumentSubCategory;
                objLog.DocumentSubSubCategory = obj.DocumentSubSubCategory;
                objLog.AssociateTemplateId = obj.AssociateTemplateId;
                objLog.IsEnabled = true;
                objLog.Mandatory = obj.Mandatory;
                objLog.ModifiedDate = DateTime.Now;
                objLog.TemplateFileName = filename;
                objLog.TemplateId = obj.TemplateId;
                db.LogTemplateUploads.Add(objLog);
                db.SaveChanges();
                return RedirectToAction("Templates", "DocumentManagement");
            }
            else
            {
                try
                {

                    List<OptionsModel> objOptions = new List<OptionsModel>();
                    objTemplates.getAllCategory = objData.getCategoryOptionsList();
                    if (objTemplates.getAllCategory != null)
                    {
                        objTemplates.getAllSubCategory = objData.getSubCategoryOptionsList(objTemplates.DocumentCategoryId);
                        objTemplates.getDocumentList = objData.getTemplateList(objTemplates.DocumentCategoryId);
                    }
                    else
                    {
                        objTemplates.getAllSubCategory = objOptions;
                        objTemplates.getDocumentList = objOptions;
                    }
                    if (objTemplates.getAllSubCategory != null)
                    {
                        objTemplates.getAllSubSubCategory = objData.getSubSubCategoryOptionsList(objTemplates.DocumentSubCategoryId);
                    }
                    else
                    {
                        objTemplates.getAllSubSubCategory = objOptions;
                    }
                    return View(objTemplates);
                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                }
            }

            return RedirectToAction("Templates", "DocumentManagement", new { Id = objTemplates.TemplateId });
        }
        #endregion

        #region EditTemplates
        public ActionResult EditTemplates(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EditDocumentUploadModel obj = new EditDocumentUploadModel();
            try
            {
                DocumentTemplate objTemplates = db.DocumentTemplates.Where(dc => dc.TemplateId == id).FirstOrDefault();
                obj.TemplateId = id;
                obj.TemplateName = objTemplates.TemplateFileName;
                obj.DocumentType = objTemplates.DocumentType;
                obj.DocumentTitle = objTemplates.DocumentTitle;
                obj.DocumentCategoryId = objTemplates.DocumentCategory;
                obj.DocumentSubCategoryId = objTemplates.DocumentSubCategory;
                obj.DocumentSubSubCategoryId = objTemplates.DocumentSubSubCategory;
                obj.DocumentDescription = objTemplates.DocumentDescription;
                obj.Cost = Convert.ToDecimal(objTemplates.TemplateCost);
                obj.Mandatory = objTemplates.Mandatory.Value;
                List<OptionsModel> objOptions = new List<OptionsModel>();
                obj.getAllCategory = objData.getCategoryOptionsList();
                if (objTemplates.DocumentCategory != 0)
                {
                    obj.getAllSubCategory = objData.getSubCategoryOptionsList(objTemplates.DocumentCategory);
                    obj.getDocumentList = objData.getTemplateList(objTemplates.DocumentCategory).Where(x => x.ID != id.Value);

                    var objAssociate = objData.getTemplateList(objTemplates.DocumentCategory);
                    //var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == id && c.IsEnabled==true).OrderBy(d=>d.DisplayOrder);
                    //int[] ATemplateIds= { };
                    //if (objAssociateIds!=null)
                    //{
                    //    int i = 0;
                    //    int j = objAssociateIds.Count();
                    //    ATemplateIds = new int[j];
                    //    obj.SelectedOrderIds = new int[j];
                    //    //foreach (var ids in objAssociateIds)
                    //    //{                           
                    //    //    ATemplateIds[i] = ids.AssociateTemplateId;
                    //    //    obj.SelectedOrderIds[i] = ids.AssociateTemplateId;
                    //    //    if (i == 0)
                    //    //    {
                    //    //        obj.OrderIds = ids.AssociateTemplateId.ToString();
                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        obj.OrderIds= obj.OrderIds+","+ ids.AssociateTemplateId.ToString();
                    //    //    }

                    //    //    i = i + 1;
                    //    //}
                    //}
                    //obj.AssociateTemplateIds = ATemplateIds;
                    SelectListItem List = new SelectListItem();
                    // obj.AssociateTemplateList = new MultiSelectList(objAssociate.ToList(), "ID", "Name", obj.AssociateTemplateIds);
                    List<GetAssociatedDocuments_Result> objCat = new List<GetAssociatedDocuments_Result>(); ;

                    obj.associatedTemplate = db.GetAssociatedDocuments(objTemplates.DocumentCategory, id, objTemplates.DepartmentID).ToList();
                }
                else
                {
                    obj.getAllSubCategory = objOptions;
                    obj.getDocumentList = objOptions;
                }
                if (obj.DocumentSubCategoryId != null)
                {
                    obj.getAllSubSubCategory = objData.getSubSubCategoryOptionsList(obj.DocumentSubCategoryId);
                }
                else
                {
                    obj.getAllSubSubCategory = objOptions;
                }

                obj.getDepartmentlist = objData.getDepartmentOptionsList();
                obj.DepartmentID = objTemplates.DepartmentID.Value;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            return View(obj);
        }
        #endregion

        #region EditTemplates
        [HttpPost]
        public ActionResult EditTemplates(EditDocumentUploadModel objTemplates)
        {
            var err = 1;
            string filename = "";
            DocumentTemplate obj = new DocumentTemplate();
            try
            {
                obj = getTemplateDetails(objTemplates);
                filename = objTemplates.TemplateName;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                return RedirectToAction("EditTemplates", "DocumentManagement", new { Id = objTemplates.TemplateId });
            }
            try
            {

                if (objTemplates.TemplateFile != null && objTemplates.TemplateFile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(Request.Files[0].FileName).ToLower();
                    //obj.TemplateFileName = Path.GetFileName(objTemplates.TemplateFile.FileName);
                    filename = Path.GetFileName(objTemplates.TemplateFile.FileName.Replace(" ", ""));
                    var path = Path.Combine(Server.MapPath("~/TemplateFiles/"), Path.GetFileName(objTemplates.TemplateFile.FileName.Replace(" ", "")));
                    var existFilepath = Path.Combine(Server.MapPath("~/TemplateFiles/"), obj.TemplateFileName);
                    var path1 = Path.Combine(Server.MapPath("~/TemplateFiles/"), Path.GetFileName(objTemplates.TemplateFile.FileName.Replace(" ", "")));
                    if (System.IO.File.Exists(path) && (obj.TemplateFileName != Path.GetFileName(objTemplates.TemplateFile.FileName.Replace(" ", ""))))
                    {
                        this.ModelState.AddModelError("TemplateFile", "File Name Already exists. Please Upload With Differnt File Name");
                        err = 0;
                    }
                    else if (extension != ".doc" && extension != ".docx")
                    {
                        ModelState.AddModelError("TemplateFile", "Invalid File.Supported file extensions: doc, docx");
                    }
                    else
                    {
                        if (System.IO.File.Exists(path1))
                        {
                            System.IO.File.Delete(path1); //Delete Old Archive File 
                        }
                        if (System.IO.File.Exists(existFilepath))
                        {
                            System.IO.File.Copy(existFilepath, path1, true); // Existing File copy to Archive Folder
                            System.IO.File.Delete(existFilepath); //Delete Old File From TemplateFiles Folder
                        }

                        obj.TemplateFileName = Path.GetFileName(objTemplates.TemplateFile.FileName.Replace(" ", ""));
                        objTemplates.TemplateFile.SaveAs(path); // New File saving to TemplateFiles Folder

                        err = 1;
                        List<string> lst = new List<string>();
                        lst = getKeyFields(obj.TemplateFileName); // Getting List of keywords from the document
                        TemplateKeysPointer objTKP = new TemplateKeysPointer();
                        objTKP.TemplateId = obj.TemplateId;
                        var objDisableTKP = db.TemplateKeysPointers.Where(d => d.TemplateId == obj.TemplateId); //Getting previous key value id's for roll back process

                        var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == objTemplates.TemplateId);

                        //// Associate Template id Fetching and inserting
                        if (objAssociateIds != null)
                        {
                            db.AssociateTemplateDetails.RemoveRange(db.AssociateTemplateDetails.Where(c => c.TemplateId == objTemplates.TemplateId));
                            db.SaveChanges();
                        }
                        // Associate Template id Fetching and inserting
                        //if (objTemplates.AssociateTemplateIds != null)
                        //{
                        //    int ordervalue = 0;
                        //    foreach (var associateid in objTemplates.AssociateTemplateIds)
                        //    {
                        //        ordervalue = ordervalue + 1;
                        //        objData.insertAssociateTemplate(objTemplates.TemplateId, associateid, ordervalue); 
                        //    }                           
                        //}

                        if (objTemplates.associatedTemplate != null && objTemplates.associatedTemplate.Count() > 0)
                        {
                            foreach (GetAssociatedDocuments_Result item in objTemplates.associatedTemplate)
                            {
                                if (item.Selected)
                                {
                                    objData.insertAssociateTemplate(objTemplates.TemplateId, item.TemplateId, item.DisplayOrder, item.Mandatory);
                                    db.SaveChanges();
                                }

                            }

                        }

                        //Delete previous key value id's
                        db.TemplateKeysPointers.RemoveRange(db.TemplateKeysPointers.Where(c => c.TemplateId == objTemplates.TemplateId));
                        db.SaveChanges();
                        //  db.TemplateKeysPointers.Where(d => d.TemplateId == obj.TemplateId).Delete();//All Key id's deleted before update
                        int errCount = 0;
                        foreach (var li in lst)
                        {
                            var TempKeyobj = objData.getKeyFieldId(li.Trim(new Char[] { '<', '>' })); // Fetch Keyword Id and save it

                            string keyName = li.Trim(new Char[] { '<', '>' });
                            var groupObj = db.AssociatedKeyGroups.Where(g => g.GroupName == keyName && g.TemplateID == objTemplates.TemplateId).FirstOrDefault();



                            if (TempKeyobj != null)
                            {
                                objTKP.TemplateKeyId = Convert.ToInt32(TempKeyobj.TemplateKeyId);
                                objTKP.IsEnabled = true;
                                db.TemplateKeysPointers.Add(objTKP);
                                db.SaveChanges();
                            }
                            else if (groupObj != null)
                            {
                                var groupKeys = db.AssociatedKeyGroups.Where(k => k.GroupName == groupObj.GroupName && k.TemplateID == objTemplates.TemplateId).ToList();
                                foreach (var g in groupKeys)
                                {
                                    objTKP.TemplateId = objTemplates.TemplateId.Value;
                                    objTKP.TemplateKeyId = Convert.ToInt32(g.KeyID); // Fetch Keyword Id & assign it
                                    objTKP.IsEnabled = true;
                                    db.TemplateKeysPointers.Add(objTKP);
                                    db.SaveChanges(); // All the Key Id with Template Id 
                                }
                            }

                            else
                            {
                                errCount = errCount + 1;
                                if (errCount == 1)
                                    ModelState.AddModelError("", "The Following Keyword's doesn't exist. Please Create The Keyword Before Uploading This Template.");
                                ModelState.AddModelError("TemplateFile", li);
                                err = 0;
                            }
                        }
                        if (err == 0) // If any exceptions (Rollback) - Delete Created Template Details 
                        {
                            System.IO.File.Delete(path); //Delete New File
                            System.IO.File.Copy(path1, existFilepath, true); // Existing File Copy From Archive Folder                           
                            RestoreTempKeyValues(objDisableTKP.ToList()); // Restore existing key values
                        }
                    }
                    db.SaveChanges();
                }
                //else {err = 1; }

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                err = 0;
            }

            if (err == 1)
            {
                LogTemplateUpload objLog = new LogTemplateUpload();
                objLog.Action = "Update";
                objLog.DocumentTitle = obj.DocumentTitle;
                objLog.DocumentDescription = obj.DocumentDescription;
                objLog.DocumentType = obj.DocumentType;
                objLog.TemplateCost = obj.TemplateCost;
                objLog.DocumentCategory = obj.DocumentCategory;
                objLog.DocumentSubCategory = obj.DocumentSubCategory;
                objLog.DocumentSubSubCategory = obj.DocumentSubSubCategory;
                //objLog.AssociateTemplateId = obj.AssociateTemplateId;
                objLog.IsEnabled = true;
                objLog.Mandatory = obj.Mandatory;
                objLog.ModifiedDate = DateTime.Now;
                objLog.TemplateFileName = filename;
                objLog.TemplateId = obj.TemplateId;
                db.LogTemplateUploads.Add(objLog);

                var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == objTemplates.TemplateId);

                if (objAssociateIds != null)
                {
                    db.AssociateTemplateDetails.RemoveRange(db.AssociateTemplateDetails.Where(c => c.TemplateId == objTemplates.TemplateId));
                    db.SaveChanges();
                }

                if (objTemplates.associatedTemplate != null && objTemplates.associatedTemplate.Count() > 0)
                {
                    foreach (GetAssociatedDocuments_Result item in objTemplates.associatedTemplate)
                    {
                        if (item.Selected)
                        {
                            objData.insertAssociateTemplate(objTemplates.TemplateId, item.TemplateId, item.DisplayOrder, item.Mandatory);
                            db.SaveChanges();
                        }

                    }

                }
                db.SaveChanges();

                return RedirectToAction("Templates", "DocumentManagement");
            }
            else
            {
                try
                {

                    List<OptionsModel> objOptions = new List<OptionsModel>();
                    objTemplates.getAllCategory = objData.getCategoryOptionsList();
                    if (objTemplates.getAllCategory != null)
                    {
                        objTemplates.getAllSubCategory = objData.getSubCategoryOptionsList(objTemplates.DocumentCategoryId);
                        objTemplates.getDocumentList = objData.getTemplateList(objTemplates.DocumentCategoryId);
                    }
                    else
                    {
                        objTemplates.getAllSubCategory = objOptions;
                        objTemplates.getDocumentList = objOptions;
                    }
                    if (objTemplates.getAllSubCategory != null)
                    {
                        objTemplates.getAllSubSubCategory = objData.getSubSubCategoryOptionsList(objTemplates.DocumentSubCategoryId);
                    }
                    else
                    {
                        objTemplates.getAllSubSubCategory = objOptions;
                    }
                    return View(objTemplates);
                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                }
            }

            return RedirectToAction("Templates", "DocumentManagement", new { Id = objTemplates.TemplateId });
        }
        #endregion

        #region UploadAssociateDocument
        public ActionResult UploadAssociateDocument()
        {
            DocumentUploadModel obj = new DocumentUploadModel();
            try
            {
                List<OptionsModel> objOptions = new List<OptionsModel>();
                obj.getAllCategory = objData.getCategoryOptionsList();
                obj.getAllSubCategory = objOptions;
                obj.getAllSubSubCategory = objOptions;
                MultiSelectList items = new MultiSelectList(objOptions);
                obj.AssociateTemplateList = items;
                //obj.getDocumentList = objOptions;
                //var objAssociate = objData.getTemplateList(1);
                //// MultiSelectList items = new MultiSelectList(objAssociate.ToList(), "ID", "Name", obj.AssociateTemplateIds.ToArray());
                //obj.AssociateTemplateList = new MultiSelectList(objAssociate.ToList(), "ID", "Name");
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(obj);
        }
        #endregion

        #region UploadAssociateDocument
        [HttpPost]
        public ActionResult UploadAssociateDocument(DocumentUploadModel obj)
        {
            var err = 1;
            int? templateId = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    AssociateTemplateDetail objAssociate = new AssociateTemplateDetail();
                    DocumentTemplate objUpload = new DocumentTemplate();
                    objUpload.DocumentTitle = obj.DocumentTitle;
                    objUpload.DocumentDescription = obj.DocumentDescription;
                    objUpload.DocumentType = obj.DocumentType;
                    objUpload.TemplateCost = obj.Cost;
                    objUpload.DocumentCategory = obj.DocumentCategoryId;
                    objUpload.DocumentSubCategory = obj.DocumentSubCategoryId;
                    objUpload.DocumentSubSubCategory = obj.DocumentSubSubCategoryId;
                    //objUpload.AssociateTemplateId = obj.AssociateTemplateId;
                    objUpload.IsEnabled = true;
                    objUpload.Mandatory = obj.Mandatory;
                    objUpload.CreatedDate = DateTime.Now;


                    bool exists = System.IO.Directory.Exists(Server.MapPath("~/TemplateFiles"));

                    if (!exists)
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/TemplateFiles"));

                    if (obj.TemplateFile != null && obj.TemplateFile.ContentLength > 0)
                    {
                        objUpload.TemplateFileName = Path.GetFileName(obj.TemplateFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/TemplateFiles/"), objUpload.TemplateFileName);
                        if (System.IO.File.Exists(path))
                        {
                            this.ModelState.AddModelError("TemplateFile", "File Name Already exists. Please Upload With Differnt File Name");
                            err = 0;
                        }
                        else
                        {

                            obj.TemplateFile.SaveAs(path); // Template document saved into TemplateFiles Folder

                            int result = objData.AddTemplate(objUpload); //Insert Template details

                            // Associate Template id Fetching and inserting
                            //if (obj.AssociateTemplateIds != null)
                            //{
                            //    int ordervalue = 0;
                            //    foreach (var associateid in obj.AssociateTemplateIds)
                            //    {
                            //        ordervalue = ordervalue + 1;
                            //        objData.insertAssociateTemplate(result, associateid, ordervalue); //Insert Associate Template Id                                 
                            //    }
                            //}


                            List<string> lst = new List<string>();
                            lst = getKeyFields(objUpload.TemplateFileName); // Getting List of keywords from the document
                            TemplateKeysPointer objTKP = new TemplateKeysPointer();
                            objTKP.TemplateId = result; // Assign TemplateId
                            templateId = result;
                            int errCount = 0;
                            foreach (var li in lst)
                            {
                                var TempKeyobj = objData.getKeyFieldId(li.Trim(new Char[] { '<', '>' })); // Fetch Keyword  
                                if (TempKeyobj != null)
                                {
                                    objTKP.TemplateKeyId = Convert.ToInt32(TempKeyobj.TemplateKeyId); // Fetch Keyword Id & assign it
                                    objTKP.IsEnabled = true;
                                    db.TemplateKeysPointers.Add(objTKP);
                                    db.SaveChanges(); // All the Key Id with Template Id 

                                }
                                else
                                {
                                    errCount = errCount + 1;
                                    if (errCount == 1)
                                        ModelState.AddModelError("", "The Following Keyword's doesn't exist. Please Create The Keyword Before Uploading This Template.");
                                    ModelState.AddModelError("TemplateFile", li);
                                    err = 0;
                                }
                            }
                            if (err == 0) // Rollback- Delete Created Template Details
                            {
                                System.IO.File.Delete(path);//Delete Uploaded file
                                db.TemplateKeysPointers.Where(d => d.TemplateId == result).Delete();//All Key id delete
                                db.DocumentTemplates.Where(d => d.TemplateId == result).Delete();//Delete Created template
                                db.AssociateTemplateDetails.Where(d => d.TemplateId == result).Delete();// Delete Associate Template Details
                            }
                        }

                    }
                    db.SaveChanges();

                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                    err = 0;
                }

            }
            else
            {
                err = 0;
            }
            if (err == 1)
            {
                LogTemplateUpload objLog = new LogTemplateUpload();

                objLog.Action = "Insert";
                objLog.DocumentTitle = obj.DocumentTitle;
                objLog.DocumentDescription = obj.DocumentDescription;
                objLog.DocumentType = obj.DocumentType;
                objLog.TemplateCost = obj.Cost;
                objLog.DocumentCategory = obj.DocumentCategoryId;
                objLog.DocumentSubCategory = obj.DocumentSubCategoryId;
                objLog.DocumentSubSubCategory = obj.DocumentSubSubCategoryId;
                objLog.AssociateTemplateId = obj.AssociateTemplateId;
                objLog.IsEnabled = true;
                objLog.Mandatory = obj.Mandatory;
                objLog.ModifiedDate = DateTime.Now;
                objLog.TemplateFileName = Path.GetFileName(obj.TemplateFile.FileName);
                objLog.TemplateId = templateId;
                db.LogTemplateUploads.Add(objLog);
                db.SaveChanges();
                return RedirectToAction("Templates", "DocumentManagement");
            }
            else
            {
                try
                {
                    var errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
                    List<OptionsModel> objOptions = new List<OptionsModel>();
                    obj.getAllCategory = objData.getCategoryOptionsList();
                    if (obj.getAllCategory != null)
                    {
                        obj.getAllSubCategory = objData.getSubCategoryOptionsList(obj.DocumentCategoryId);
                        List<SelectListItem> objOptions2 = new List<SelectListItem>();
                        var objAssociate = objData.getTemplateList(obj.DocumentCategoryId);
                        // MultiSelectList items = new MultiSelectList(objAssociate.ToList(), "ID", "Name", obj.AssociateTemplateIds.ToArray());
                        obj.AssociateTemplateList = new MultiSelectList(objAssociate.ToList(), "ID", "Name", obj.AssociateTemplateIds);
                    }
                    else
                    {
                        obj.getAllSubCategory = objOptions;
                    }
                    if (obj.getAllSubCategory != null)
                    {
                        obj.getAllSubSubCategory = objData.getSubSubCategoryOptionsList(obj.DocumentSubCategoryId);
                    }
                    else
                    {
                        obj.getAllSubSubCategory = objOptions;
                    }

                    obj.getDocumentList = objOptions;

                    return View(obj);
                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                }
                return RedirectToAction("UploadAssociateDocument", "DocumentManagement");

            }
        }
        #endregion

        #region UploadDocument
        public ActionResult UploadDocument()
        {
            DocumentUploadModel obj = new DocumentUploadModel();
            try
            {
                List<OptionsModel> objOptions = new List<OptionsModel>();
                obj.getAllCategory = objData.getCategoryOptionsList();
                obj.getDepartmentlist = objData.getDepartmentOptionsList();
                obj.getAllSubCategory = objOptions;
                obj.getAllSubSubCategory = objOptions;
                MultiSelectList items = new MultiSelectList(objOptions);
                obj.AssociateTemplateList = items;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(obj);
        }
        #endregion

        #region UploadDocument
        [HttpPost]
        public ActionResult UploadDocument(DocumentUploadModel obj)
        {
            var err = 1;
            int? templateId = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    AssociateTemplateDetail objAssociate = new AssociateTemplateDetail();
                    DocumentTemplate objUpload = new DocumentTemplate();
                    objUpload.DocumentTitle = obj.DocumentTitle;
                    objUpload.DocumentDescription = obj.DocumentDescription;
                    objUpload.DocumentType = obj.DocumentType;
                    objUpload.TemplateCost = obj.Cost;
                    objUpload.DocumentCategory = obj.DocumentCategoryId;
                    objUpload.DocumentSubCategory = obj.DocumentSubCategoryId;
                    objUpload.DocumentSubSubCategory = obj.DocumentSubSubCategoryId;
                    // objUpload.AssociateTemplateId = obj.AssociateTemplateId;
                    objUpload.IsEnabled = true;
                    objUpload.Mandatory = obj.Mandatory;
                    objUpload.CreatedDate = DateTime.Now;
                    objUpload.DepartmentID = obj.DepartmentID;


                    bool exists = System.IO.Directory.Exists(Server.MapPath("~/TemplateFiles"));

                    if (!exists)
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/TemplateFiles"));

                    if (obj.TemplateFile != null && obj.TemplateFile.ContentLength > 0)
                    {
                        objUpload.TemplateFileName = Path.GetFileName(obj.TemplateFile.FileName.Replace(" ", ""));
                        var path = Path.Combine(Server.MapPath("~/TemplateFiles/"), objUpload.TemplateFileName);
                        if (System.IO.File.Exists(path))
                        {
                            this.ModelState.AddModelError("TemplateFile", "File Name Already exists. Please Upload With Differnt File Name");
                            err = 0;
                        }
                        else
                        {

                            obj.TemplateFile.SaveAs(path); // Template document saved into TemplateFiles Folder

                            int result = objData.AddTemplate(objUpload); //Insert Template details
                            List<string> lst = new List<string>();
                            lst = getKeyFields(objUpload.TemplateFileName); // Getting List of keywords from the document
                            TemplateKeysPointer objTKP = new TemplateKeysPointer();
                            objTKP.TemplateId = result; // Assign TemplateId
                            templateId = result;
                            int errCount = 0;

                            if (obj.associatedTemplate != null && obj.associatedTemplate.Count() > 0)
                            {
                                foreach (GetAssociatedDocuments_Result item in obj.associatedTemplate)
                                {
                                    if (item.Selected)
                                    {
                                        objData.insertAssociateTemplate(templateId, item.TemplateId, item.DisplayOrder, item.Mandatory);
                                        db.SaveChanges();
                                    }

                                }

                            }
                            int keyError = 0;
                            int caseError = 0;
                            foreach (var li in lst)
                            {
                                var TempKeyobj = objData.getKeyFieldId(li.Trim(new Char[] { '<', '>' })); // Fetch Keyword  


                                if (TempKeyobj != null && TempKeyobj.TemplateKeyValue == li.Trim(new Char[] { '<', '>' }))
                                {
                                    objTKP.TemplateKeyId = Convert.ToInt32(TempKeyobj.TemplateKeyId); // Fetch Keyword Id & assign it
                                    objTKP.IsEnabled = true;
                                    db.TemplateKeysPointers.Add(objTKP);
                                    db.SaveChanges(); // All the Key Id with Template Id 

                                }
                                else
                                {
                                    errCount = errCount + 1;

                                    if (TempKeyobj != null && TempKeyobj.TemplateKeyValue != li.Trim(new Char[] { '<', '>' }))
                                    {
                                        caseError++;
                                        if (caseError == 1)
                                        {
                                            Logger("Case is not matching with existing key value. Please correct it.");
                                            this.ModelState.AddModelError("", "Case is not matching with existing key value. Please correct it.");
                                        }

                                    }
                                    else
                                    {
                                        keyError++;
                                        if (keyError == 1)
                                        {
                                            Logger("The Following Keyword's doesn't exist. Please Create The Keyword Before Uploading This Template.");
                                            this.ModelState.AddModelError("", "The Following Keyword's doesn't exist. Please Create The Keyword Before Uploading This Template.");
                                        }

                                        Logger("The Following Keyword's doesn't exist. Please Create The Keyword Before Uploading This Template.1");
                                    }
                                    err = 0;
                                    this.ModelState.AddModelError("TemplateFile", li);

                                }
                            }
                            if (err == 0) // Rollback- Delete Created Template Details
                            {
                                db.TemplateKeysPointers.Where(d => d.TemplateId == result).Delete();//All Key id delete
                                db.DocumentTemplates.Where(d => d.TemplateId == result).Delete();//Delete Created template
                                db.AssociateTemplateDetails.Where(d => d.TemplateId == result).Delete();// Delete Associate Template Details
                                System.IO.File.Delete(path);//Delete Uploaded file

                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("PageError", "Catch1: " + ex.Message);
                    ErrorLog.LogThisError(ex);
                    this.ModelState.AddModelError("TemplateFile", ex.StackTrace);
                    err = 0;
                }

            }
            else
            {
                err = 0;
            }
            if (err == 1)
            {

                LogTemplateUpload objLog = new LogTemplateUpload();

                objLog.Action = "Insert";
                objLog.DocumentTitle = obj.DocumentTitle;
                objLog.DocumentDescription = obj.DocumentDescription;
                objLog.DocumentType = obj.DocumentType;
                objLog.TemplateCost = obj.Cost;
                objLog.DocumentCategory = obj.DocumentCategoryId;
                objLog.DocumentSubCategory = obj.DocumentSubCategoryId;
                objLog.DocumentSubSubCategory = obj.DocumentSubSubCategoryId;
                objLog.AssociateTemplateId = obj.AssociateTemplateId;
                objLog.IsEnabled = true;
                objLog.Mandatory = obj.Mandatory;
                objLog.ModifiedDate = DateTime.Now;
                objLog.TemplateFileName = Path.GetFileName(obj.TemplateFile.FileName);
                objLog.TemplateId = templateId;
                db.LogTemplateUploads.Add(objLog);
                db.SaveChanges();
                return RedirectToAction("Templates", "DocumentManagement");
            }
            else
            {
                try
                {
                    var errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
                    List<OptionsModel> objOptions = new List<OptionsModel>();
                    obj.getAllCategory = objData.getCategoryOptionsList();
                    if (obj.getAllCategory != null)
                    {
                        obj.getAllSubCategory = objData.getSubCategoryOptionsList(obj.DocumentCategoryId);
                        List<SelectListItem> objOptions2 = new List<SelectListItem>();
                        var objAssociate = objData.getTemplateList(obj.DocumentCategoryId);
                        // MultiSelectList items = new MultiSelectList(objAssociate.ToList(), "ID", "Name", obj.AssociateTemplateIds.ToArray());
                        obj.AssociateTemplateList = new MultiSelectList(objAssociate.ToList(), "ID", "Name", obj.AssociateTemplateIds);
                    }
                    else
                    {
                        obj.getAllSubCategory = objOptions;
                    }
                    if (obj.getAllSubCategory != null)
                    {
                        obj.getAllSubSubCategory = objData.getSubSubCategoryOptionsList(obj.DocumentSubCategoryId);
                    }
                    else
                    {
                        obj.getAllSubSubCategory = objOptions;
                    }

                    obj.getDocumentList = objOptions;
                    obj.getDepartmentlist = objData.getDepartmentOptionsList();

                    return View(obj);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("PageError", "Catch2: " + ex.Message);
                    ErrorLog.LogThisError(ex);
                }

                return RedirectToAction("UploadDocument", "DocumentManagement");

            }
        }
        #endregion

        #region ActivateTemplate
        [HttpPost]
        public JsonResult ActivateTemplate(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            LogTemplateUpload objLog = new LogTemplateUpload();
            try
            {
                var obj = db.DocumentTemplates.Find(id);
                if (obj != null)
                {
                    if (obj.IsEnabled == true)
                    {
                        objLog.IsEnabled = false;
                        objLog.Action = "Inactive";
                        obj.IsEnabled = false;
                        message = "Document Template Deactivated Successfully";

                        var associateddocument = db.AssociateTemplateDetails.Where(s => s.AssociateTemplateId == id).ToList();
                        associateddocument.ForEach((a) =>
                        {
                            a.IsEnabled = false;
                        });

                    }
                    else
                    {
                        objLog.IsEnabled = true;
                        objLog.Action = "Active";
                        obj.IsEnabled = true;
                        message = "Document Template Activated Successfully";
                    }
                }
                objLog.DocumentTitle = obj.DocumentTitle;
                objLog.DocumentDescription = obj.DocumentDescription;
                objLog.DocumentType = obj.DocumentType;
                objLog.TemplateCost = obj.TemplateCost;
                objLog.DocumentCategory = obj.DocumentCategory;
                objLog.DocumentSubCategory = obj.DocumentSubCategory;
                objLog.DocumentSubSubCategory = obj.DocumentSubSubCategory;
                objLog.AssociateTemplateId = obj.AssociateTemplateId;

                objLog.Mandatory = obj.Mandatory;
                objLog.ModifiedDate = DateTime.Now;
                objLog.TemplateFileName = obj.TemplateFileName;
                objLog.TemplateId = obj.TemplateId;
                db.LogTemplateUploads.Add(objLog);
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
        #endregion

        #region getKeyFields
        public List<string> getKeyFields(string filename)
        {
            Logger(filename);
            List<string> lst = new List<string>();
            try
            {
                string path = Path.Combine(Server.MapPath("~/TemplateFiles/" + filename));

                TextExtractor extractor = new TextExtractor(path);

                string text = extractor.ExtractText();
                //Application word = new Application();
                //object miss = System.Reflection.Missing.Value;
                ////object path = @"C:\DOC\myDocument.docx";
                //object readOnly = true;
                //Microsoft.Office.Interop.Word.Document docs = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss);
                //string totaltext = "";
                //for (int i = 0; i < docs.Paragraphs.Count; i++)
                //{
                //    Logger(docs.Paragraphs[i + 1].Range.Text);
                //    totaltext += " \r\n " + docs.Paragraphs[i + 1].Range.Text.ToString();
                //}

                MatchCollection mcol = Regex.Matches(text, @"<\b\S+?\b>");
                foreach (Match m in mcol)
                {
                    lst.Add(m.ToString());
                }



                // Console.WriteLine(totaltext);
                //docs.Close();
                //word.Quit();


            }
            catch (Exception ex)
            {
                //Logger(ex.InnerException.StackTrace);
                Logger(ex.Message);
                ErrorLog.LogThisError(ex);
            }

            finally { }
            return lst;
        }
        #endregion

        #region IsFileinUse
        protected virtual bool IsFileinUse(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
        #endregion

        #region GetSubCategoryListById
        /// <summary>
        /// Getting Sub Category list 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetSubCategoryListById(int? id)
        {
            List<OptionsModel> objOptions = new List<OptionsModel>();
            try
            {
                objOptions = objData.getSubCategoryOptionsList(id);

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return Json(new { DocumentSubCategory = objOptions }, JsonRequestBehavior.AllowGet);
            //return Json(objOptions);

        }
        #endregion

        #region GetSubCategoryById
        /// <summary>
        /// Getting Sub Category list and Category List Templates (for associate template)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetSubCategoryById(int? id, int TemplateId)
        {
            List<OptionsModel> objOptions = new List<OptionsModel>();
            List<OptionsModel> objOptions2 = new List<OptionsModel>();
            try
            {
                objOptions = objData.getSubCategoryOptionsList(id);
                objOptions2 = objData.getTemplateList(id).Where(x => x.ID != TemplateId).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return Json(new { DocumentSubCategory = objOptions, TemplateList = objOptions2 }, JsonRequestBehavior.AllowGet);
            //return Json(objOptions);

        }
        #endregion

        #region GetSubSubCategoryById
        /// <summary>
        /// Getting sub sub category list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetSubSubCategoryById(int? id)
        {
            List<OptionsModel> objOptions = new List<OptionsModel>();
            try
            {
                objOptions = objData.getSubSubCategoryOptionsList(id);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return Json(objOptions);

        }
        #endregion

        //public List<DocumentCategory> GetSubCategories(List<DocumentCategory> Cats)
        //{
        //    string count = "-";
        //    List<DocumentCategory> MainCats = new List<DocumentCategory>();
        //    foreach (var item in Cats)
        //    {
        //        // Add the first one and see if it has child categories
        //        if (item.DocumentCategoryId == true)
        //            item.CategoryName = "[" + item.DocumentCategoryName + "]";
        //        MainCats.Add(item);
        //        var children = db.GetAllCategories().Where(c => c.ParentCategoryID == item.CategoryID);
        //        if (children.Count() > 0)
        //        {
        //            //Html.RenderPartial("ShowSubCategories", children);
        //            foreach (var subcats in GetSubCategories(children.ToList()))
        //            {
        //                subcats.CategoryName = count + subcats.CategoryName;
        //                MainCats.Add(subcats);
        //                count += "-";
        //            }
        //        }
        //        count = "-";
        //    }
        //    return MainCats;
        //}

        #endregion

        #region Template Key Value
        public ActionResult AddkeyValue()
        {
            TemplateKeywordModel obj = new TemplateKeywordModel();
            obj.getTemplateKeyCategory = getTemplateKeyCategory(null);
            obj.getTemplateKeys = getTemplateKey();
            return View(obj);
        }



        public List<OptionsModel> getTemplateKeyCategory(int? id)
        {
            List<OptionsModel> objOptions = new List<OptionsModel>();
            try
            {
                objOptions = objData.getTemplateKeyCategoryList(id);

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return objOptions;
        }

        public List<OptionsModel> getTemplateKey()
        {
            List<OptionsModel> objOptions = new List<OptionsModel>();
            try
            {
                objOptions = objData.getTemplateKeyList();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return objOptions;
        }



        [HttpPost]
        public ActionResult AddKeyValue(TemplateKeywordModel obj)
        {
            try
            {
                TemplateKeyword objkey = new TemplateKeyword();
                objkey.TemplateKeyValue = obj.TemplateKeyValue;
                objkey.TemplateKeyLabels = obj.TemplateKeyLabels;
                objkey.TemplateKeyDescription = obj.TemplateKeyDescription;
                objkey.IsEnabled = true;
                objkey.MultipleKeys = obj.MultipleKeys;
                objkey.TemplateKeyCategory = obj.TemplateKeyCategory;
                objkey.SecurityAlert = obj.SecurityCheck;
                objkey.TextArea = obj.TextArea;
                objkey.BigTextArea = obj.BigTextArea;
                objkey.ClonedFrom = obj.ClonedFrom;
                if (db.KeyCategories.FirstOrDefault(m => m.Id == obj.TemplateKeyCategory).CanAddInsurance != null &&
                    db.KeyCategories.FirstOrDefault(m => m.Id == obj.TemplateKeyCategory).CanAddInsurance.Value)
                    objkey.IsAssetName = obj.IsAssetName;
                else
                    objkey.IsAssetName = false;
                if (obj.ClonedFrom != null)
                    objkey.Cloned = true;
                else
                    objkey.Cloned = false;

                var KeyName = db.KeyCategories.Where(p => p.Id == obj.TemplateKeyCategory).FirstOrDefault();
                objkey.KeyCategoryName = Convert.ToString(KeyName.CategoryName);
                db.TemplateKeywords.Add(objkey);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("KeywordIndex", "DocumentManagement");
        }

        public ActionResult EditKeyValue(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            EditTemplateKeywordModel objkey = new EditTemplateKeywordModel();
            try
            {
                var obj = db.TemplateKeywords.Find(id);
                objkey.TemplateKeyId = obj.TemplateKeyId;
                objkey.TemplateKeyValue = obj.TemplateKeyValue;
                objkey.TemplateKeyLabels = obj.TemplateKeyLabels;
                objkey.TemplateKeyDescription = obj.TemplateKeyDescription;
                objkey.getTemplateKeyCategory = getTemplateKeyCategory(null);
                objkey.SecurityCheck = obj.SecurityAlert == null ? false : obj.SecurityAlert.Value;
                objkey.TemplateKeyCategory = Convert.ToInt32(obj.TemplateKeyCategory);
                objkey.MultipleKeys = obj.MultipleKeys;
                objkey.BigTextArea = obj.BigTextArea;
                objkey.getTemplateKeys = getTemplateKey();
                objkey.ClonedFrom = obj.ClonedFrom != null ? obj.ClonedFrom.Value : 0;
                objkey.TextArea = obj.TextArea;
                objkey.IsAssetName = obj.IsAssetName != null ? obj.IsAssetName.Value : false;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objkey);
        }

        [HttpPost]
        public ActionResult EditKeyValue(EditTemplateKeywordModel obj)
        {
            try
            {
                TemplateKeyword objkey = new TemplateKeyword();
                objkey = db.TemplateKeywords.Find(obj.TemplateKeyId);
                objkey.TemplateKeyLabels = obj.TemplateKeyLabels;
                objkey.TemplateKeyDescription = obj.TemplateKeyDescription;
                objkey.MultipleKeys = obj.MultipleKeys;
                objkey.SecurityAlert = obj.SecurityCheck;
                objkey.TextArea = obj.TextArea;
                objkey.BigTextArea = obj.BigTextArea;
                objkey.TemplateKeyCategory = obj.TemplateKeyCategory;
                objkey.IsAssetName = obj.IsAssetName;
                var KeyName = db.KeyCategories.Where(p => p.Id == obj.TemplateKeyCategory).FirstOrDefault();
                objkey.KeyCategoryName = Convert.ToString(KeyName.CategoryName);

                if (obj.ClonedFrom != null)
                {
                    objkey.Cloned = true;
                    objkey.ClonedFrom = obj.ClonedFrom;
                }
                else
                {
                    objkey.Cloned = false;
                    objkey.ClonedFrom = 0;
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("KeywordIndex", "DocumentManagement");
        }

        [HttpGet]
        public JsonResult CheckKeyCategory(int id)
        {
            var key = db.TemplateKeywords.Where(m => m.TemplateKeyCategory == id && m.IsAssetName.Value);

            if (key.Count() > 0)
            {
                return Json(300, JsonRequestBehavior.AllowGet);
            }

            return Json(200, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CheckTemplateKey(string TemplateKeyValue)
        {
            var chkExisting = db.TemplateKeywords.Where(a => a.TemplateKeyValue == TemplateKeyValue.Trim()).FirstOrDefault();

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
        public JsonResult ActivateKeyValue(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            try
            {
                var obj = db.TemplateKeywords.Find(id);
                if (obj != null)
                {
                    if (obj.IsEnabled == true)
                    {
                        obj.IsEnabled = false;
                        message = "Template Key Deactivated Successfully";
                    }
                    else
                    {
                        obj.IsEnabled = true;
                        message = "Template Key Activated Successfully";
                    }
                }
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

        public ActionResult KeywordIndex()
        {
            List<TemplateKeyword> obj = new List<TemplateKeyword>();
            try
            {


                Int32 categoryId = 0;
                if (Session["keycategoryID"] != null)
                {
                    string enable = Session["keyenable"].ToString();
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

                    categoryId = Convert.ToInt32(Session["keycategoryID"].ToString());
                    obj = db.TemplateKeywords.Where(m => m.AddedByClient == false && m.IsEnabled == active && (categoryId == 0 || m.TemplateKeyCategory == categoryId)).ToList<TemplateKeyword>();
                    Session.Remove("keycategoryID");
                    ViewBag.Enable = enable;
                }
                else
                {
                    obj = db.TemplateKeywords.Where(m => m.AddedByClient == false && m.IsEnabled == true).ToList<TemplateKeyword>();
                    ViewBag.Enable = "Active";
                }
                var query = db.KeyCategories.OrderBy(o => o.CategoryOrder).Where(e => e.IsEnabled == true).Select(c => new { c.Id, c.CategoryName });
                ViewBag.categories = new SelectList(query.AsEnumerable(), "id", "categoryname", categoryId);

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            return View(obj);
        }

        public ActionResult KeywordIndexID(Int32 Id, string enable)
        {

            try
            {


                var query = db.KeyCategories.Where(e => e.IsEnabled == true).Select(c => new { c.Id, c.CategoryName });
                ViewBag.categories = new SelectList(query.AsEnumerable(), "id", "categoryname");
                Session["keycategoryID"] = Id;
                Session["keyenable"] = enable;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Dynamic Form Filling
        public ActionResult CreateDynamicForm(int? id)
        {
            //int userId = Convert.ToInt32(Session["UserId"]);
            //var service = db.SelectedAccountServices.Where(s => s.UserId == userId).FirstOrDefault();
            //Session["ServiceID"] = service.ServiceId;
            // var catogories = db.DocumentCategories.Where(d => d.ServiceId == service.ServiceId && d.IsEnabled == true).ToArray();
            // ViewBag.ClientID = service.ServiceId;

            if (Session["ExtraFiles"] != null)
            {
                id = Convert.ToInt32(Session["ExtraFiles"]);
                Session.Remove("ExtraFiles");
            }
            try
            {
                var modals = new List<KeyCategoryModal>();
                int CommonTempId;
                if (Session["TemplateId"] == null)
                {
                    Session["TemplateId"] = id;
                }
                CommonTempId = Convert.ToInt32(Session["TemplateId"]); // To maintain Parent Id for Associate Templates

                Session["CurrentTemplateId"] = id; // For FormConfirmation function

                int? customerId = null;
                if (Request["CustomerId"] != null)
                {
                    customerId = Convert.ToInt32(Request["CustomerId"]);
                    Session["customerId"] = customerId;
                }
                else
                {
                    customerId = Convert.ToInt32(Session["customerId"]);
                }

                ViewBag.customerID = customerId;
                string AssociateName = "";
                int? associateId = null;
                var objCurrentTemplate = db.DocumentTemplates.Find(id); // To get Parent Template Name

                var customer = db.CustomerDetails.FirstOrDefault(m => m.CustomerId == customerId);

                AssociateName = objCurrentTemplate.DocumentTitle;
                // Checking associate template
                var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == CommonTempId && c.IsEnabled == true).OrderBy(c => c.DisplayOrder);

                if (objAssociateIds != null && objAssociateIds.Count() >= 1)
                {
                    Session["ATCount"] = objAssociateIds.Count();    // Total Associate Count


                    if (objAssociateIds != null)
                    {
                        int i = 0;
                        foreach (AssociateTemplateDetail item in objAssociateIds)
                        {
                            var objAssociate = db.DocumentTemplates.Find(item.AssociateTemplateId);
                            if (objAssociate != null)
                            {
                                if (i == 0)
                                    AssociateName = "Associated Documents >> ";
                                //var Isassociate = catogories.Where(c => c.DocumentCategoryId == objAssociate.DocumentCategory).FirstOrDefault();

                                //if (Isassociate != null)
                                //{
                                string currentdoc = string.Empty;

                                if (item.Mandatory)
                                {
                                    if (objAssociate.DocumentTitle == objCurrentTemplate.DocumentTitle)
                                    {
                                        currentdoc = "<font color =red><b>" + objAssociate.DocumentTitle + "</b></font>";

                                    }
                                    else
                                    {
                                        currentdoc = "<font color =red>" + objAssociate.DocumentTitle + "</font>";
                                    }
                                }
                                else if (objAssociate.DocumentTitle == objCurrentTemplate.DocumentTitle)
                                {
                                    currentdoc = "<b>" + objAssociate.DocumentTitle + "</b>";
                                }
                                else
                                    currentdoc = objAssociate.DocumentTitle;
                                AssociateName = AssociateName + currentdoc + " - ";
                            }
                            i++;
                        }
                        //}

                        AssociateName = AssociateName.Remove(AssociateName.Length - 2);

                    }


                }


                // Get Filled Details For This Template if Already exist
                // bool CurrentData = false;
                string keyval = null;


                //Dynamic form rows binding
                StringBuilder str = new StringBuilder();
                str.Append("<h4 style = 'background-color: grey;padding: 8px;border-radius: 5px;margin-top: 0px'>" + objCurrentTemplate.DocumentTitle + " </h4>");
                if (Convert.ToInt32(Session["ATCount"]) > 0)
                {
                    str.Append(DynamicFormStepCount(Convert.ToInt32(Session["ATCount"]), AssociateName));
                }
                else
                {
                    //str.Append(DynamicFormName(AssociateName));
                }
                str.Append(DynamicFormTop());
                List<TemplateKeysPointer> lst = new List<TemplateKeysPointer>();
                var objkeyCategory = (from c in db.KeyCategories
                                      join k in db.TemplateKeywords on c.Id equals k.TemplateKeyCategory
                                      join p in db.TemplateKeysPointers on k.TemplateKeyId equals p.TemplateKeyId
                                      where p.TemplateId == id
                                      orderby c.CategoryOrder
                                      select new
                                      {
                                          c.CategoryName,
                                          c.CategoryOrder,
                                          c.CanAddInsurance,
                                          c.Id
                                      }).Distinct().OrderBy(x => x.CategoryOrder
                                     );


                foreach (var category in objkeyCategory)
                {
                    try
                    {

                        str.Append("<div class=col-lg-12> <legend>" + category.CategoryName + "</legend></div><div class=col-lg-6>");

                        var objkey = (from c in db.KeyCategories
                                      join k in db.TemplateKeywords on c.Id equals k.TemplateKeyCategory
                                      join p in db.TemplateKeysPointers on k.TemplateKeyId equals p.TemplateKeyId
                                      where p.TemplateId == id && c.CategoryName == category.CategoryName && k.BigTextArea == false && k.Cloned == false

                                      select new
                                      {
                                          p.TemplateKeyId,
                                          p.IsEnabled,
                                          p.TemplateId,
                                          p.TemplateKeyRowId,
                                          k.IsAssetName
                                      }).ToList();


                        //fetching all the keys for current template
                        var lst1 = objkey.GroupBy(p => p.TemplateKeyId)
                             .Select(grp => new { TemplateKeyId = grp.Key, grp.First().IsAssetName }).ToList();
                        int keycount = 0;
                        int tempkeycount = 0;
                        keycount = lst1.Count / 2;

                        string assetName = string.Empty;
                        int assetKeyId = 0;

                        foreach (var li in lst1)
                        {


                            var TempKeyobj = objData.getKeyDetails(li.TemplateKeyId); // Fetch Keyword Details 
                            if (TempKeyobj != null)
                            {
                                if (keycount == tempkeycount && lst1.Count != 1) // Spiliting columns for two fields per row
                                {
                                    str.Append(" </div><div class=col-lg-6>");
                                }

                                var existkeyval = db.TemplateDynamicFormValues.Where(b => b.CustomerId == customerId && b.TemplateKey == TempKeyobj.TemplateKeyValue.Trim()).OrderByDescending(x => x.RowId).FirstOrDefault();

                                if (existkeyval != null) // Checking for same key value Already Exists
                                {
                                    keyval = existkeyval.UserInputs;
                                }
                                else { keyval = null; }
                                str.Append(BuildDynamicForm(TempKeyobj.TemplateKeyValue.Trim(), TempKeyobj.TemplateKeyLabels, keyval, customerId.Value, TempKeyobj.MultipleKeys, Convert.ToInt32(id), (TempKeyobj.SecurityAlert != null ? TempKeyobj.SecurityAlert.Value : false), TempKeyobj.TextArea != null ? TempKeyobj.TextArea : false, category.Id, li.IsAssetName != null ? li.IsAssetName.Value : false)); // Building textbox based on the key values

                            }

                            if (li.IsAssetName != null && li.IsAssetName.Value)
                            {
                                if (!string.IsNullOrEmpty(keyval))
                                    assetName = keyval;
                                else
                                {
                                    if (li.TemplateKeyId == 1)
                                    {
                                        var cust = db.CustomerDetails.FirstOrDefault(m => m.CustomerId == customerId);
                                        assetName = cust != null ? cust.CustomerName : string.Empty;
                                    }
                                    else
                                    {
                                        assetName = string.Empty;
                                    }
                                }
                                assetKeyId = li.TemplateKeyId;
                            }

                            tempkeycount = tempkeycount + 1;
                        }

                        if (objkeyCategory.Count() > 1)
                        {
                            str.Append("</div>");
                        }

                        if (roleId == 5 || roleId == 6)
                        {
                            if (category.CanAddInsurance != null && category.CanAddInsurance.Value && assetKeyId != 0)
                            {
                                modals.Add(new KeyCategoryModal
                                {
                                    KeyCateogryId = category.Id,
                                    KeyCategoryName = category.CategoryName,
                                    AssetKeyId = assetKeyId,
                                    AssetName = assetName,
                                    CustomerName = customer.CustomerName
                                });
                                str.Append("<div class=\"col-lg-12 btn-insurance\"><button class=\"btn btn-default\" id=\"btnInsurance-" + category.Id
                                    + "\" data-toggle=\"modal\" data-target=\"#mdlInsurance-" + category.Id + "\">Add Insurance</button></div>");
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Logger(ex.Message);
                    }
                }

                var objbigkey = (
                              from
                              k in db.TemplateKeywords
                              join p in db.TemplateKeysPointers on k.TemplateKeyId equals p.TemplateKeyId
                              where p.TemplateId == id && k.BigTextArea == true && k.Cloned == false

                              select new
                              {
                                  p.TemplateKeyId,
                                  p.IsEnabled,
                                  p.TemplateId,
                                  p.TemplateKeyRowId
                              }).ToList();

                if (objbigkey.Count > 0)
                {
                    //fetching all the keys for current template
                    var keyLst = objbigkey.GroupBy(p => p.TemplateKeyId)
                         .Select(grp => grp.First()).ToList();

                    foreach (var li in keyLst)
                    {

                        var TempKeyobj = objData.getKeyDetails(li.TemplateKeyId); // Fetch Keyword Details 
                        if (TempKeyobj != null)
                        {
                            var existkeyval = db.TemplateDynamicFormValues.Where(b => b.CustomerId == customerId && b.TemplateKey == TempKeyobj.TemplateKeyValue.Trim()).OrderByDescending(x => x.RowId).FirstOrDefault();

                            str.Append("<div class='row'><div class=col-lg-12 id=div_" + TempKeyobj.TemplateKeyValue + "><label class='col-lg-2'>" + TempKeyobj.TemplateKeyLabels + "</label><div class='col-lg-10'><textarea rows='9'  maxlength='250' class='form-control' name='" + TempKeyobj.TemplateKeyValue + "' placeholder='" + TempKeyobj.TemplateKeyLabels + "'>" + existkeyval.UserInputs + "</textarea></div></div></div>");

                        }

                    }

                }

                str.Append(DynamicFormBottom(customerId.Value));

                str.Append(BuildSubmitButton(id, Convert.ToInt32(Session["OrgId"]), associateId));

                foreach (var item in modals)
                {
                    string modal = Utility.RenderPartialViewToString(this, "AddInsuranceModal", item);
                    str.Append(modal);
                }

                ViewBag.Dynamic = str;

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            return View();

        }

        private void CreateTemplate(int id, string customerDetails)
        {
        }

        public string BuildDynamicForm(string field, string label, string value, int customerId, bool isMultiple, int templateID, bool securityCheck, bool textArea, int keyCategoryId, bool isAssetName)
        {
            try
            {
                string row = "";
                string datalist = "";
                datalist = BuildDataList(field, customerId);

                string customerField = field.Replace("_", " ");
                var customerData = db.CustomerTemplateDetails.Where(c => c.CustID == customerId && c.FieldName.Contains(customerField)).FirstOrDefault();
                if (customerData != null)
                { value = customerData.FieldValue; }

                if (isMultiple)
                {

                    if (securityCheck)
                        if (textArea)
                            row = "<div class='jQTextArea form-group' id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "' maxlength='250' placeholder='" + label + "'>" + value + "</textarea></div> <div class=col-lg-1><button type=button class=glyphicon-plus" + "  id=add_" + field + " onclick=addFunctionNew(this.id,'polo') ></button> </div></div>";
                        else
                            row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=form-control name='" + field + "' id='" + field + "'  maxlength='250' placeholder='" + label + "' onblur=checkData(this.id) type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div> <div class=col-lg-1><button type=button class=glyphicon-plus" + "  id=add_" + field + " onclick=addFunction(this.id) ></button> </div></div>";

                    else
                    {
                        if (textArea)
                            row = "<div class='jQTextArea form-group' id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'  maxlength='250' placeholder='" + label + "'>" + value + "</textarea></div><div class=col-lg-1><button type=button class=glyphicon-plus" + "  id=add_" + field + " onclick=addFunctionNew(this.id,'polo') ></button> </div></div>";
                        else
                            row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=form-control name='" + field + "' id='" + field + "' maxlength='250' placeholder='" + label + "' type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div> <div class=col-lg-1><button type=button class=glyphicon-plus" + "  id=add_" + field + " onclick=addFunction(this.id) ></button> </div></div>";

                    }
                    var keysaddedbycustomer = db.TemplateDynamicFormValues.Where(d => d.ParentkeyId == field && d.CustomerId == customerId && d.IsEnabled == true && d.TemplateId == templateID).ToList();

                    if (keysaddedbycustomer == null && keysaddedbycustomer.Count == 0)
                    {
                        keysaddedbycustomer = db.TemplateDynamicFormValues.Where(d => d.ParentkeyId == field && d.CustomerId == customerId).ToList();
                    }

                    if (keysaddedbycustomer != null && keysaddedbycustomer.Count > 0)
                    {
                        for (int i = 0; i < keysaddedbycustomer.Count; i++)
                        {
                            datalist = BuildDataList(field, customerId);

                            if (textArea)
                                row = row + "<div class='jQTextArea form-group' id=div_" + keysaddedbycustomer[i].TemplateKey + "><label class=col-lg-4 control-label></label><div class=col-lg-6><textarea rows='4' cols='35' name='" + keysaddedbycustomer[i].TemplateKey + "'  maxlength='250' placeholder='" + label + "'>" + keysaddedbycustomer[i].UserInputs + "</textarea></div><div class=col-lg-1><button type=button class=glyphicon-minus" + "  id=remove_" + keysaddedbycustomer[i].TemplateKey + " onclick=removeFunction(this.id) ></button> </div></div>";
                            else
                                //row = row + "<div class=form-group id=div_" + keysaddedbycustomer[i].TemplateKey + "><label class=col-lg-4 control-label></label><div class=col-lg-6><input class=form-control name='" + keysaddedbycustomer[i].TemplateKey + "' placeholder='" + label + "' type=text  list='" + keysaddedbycustomer[i].TemplateKey + "'>" + datalist + "</div><div class=col-lg-1><button type=button class=glyphicon-minus" + "  id=remove_" + keysaddedbycustomer[i].TemplateKey + " onclick=removeFunction(this.id) ></button> </div></div>";
                                row = row + "<div class=form-group id=div_" + keysaddedbycustomer[i].TemplateKey + "><label class=col-lg-4 control-label></label><div class=col-lg-6><input class=form-control id='" + keysaddedbycustomer[i].TemplateKey + "' name='" + keysaddedbycustomer[i].TemplateKey + "' maxlength='250' onblur=checkData(this.id)  placeholder='" + label + "' type=text value='" + keysaddedbycustomer[i].UserInputs + "' list='" + keysaddedbycustomer[i].TemplateKey + "'>" + datalist + "</div><div class=col-lg-1><button type=button class=glyphicon-minus" + "  id=remove_" + keysaddedbycustomer[i].TemplateKey + " onclick=removeFunction(this.id) ></button> </div></div>";
                        }
                    }
                }
                else
                {
                    if (isAssetName)
                    {
                        if (securityCheck)
                            if (textArea)
                                row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'   maxlength='250' placeholder='" + label + "'>" + value + "</textarea> </div></div>";
                            else
                                row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=\"form-control asset-name\" keyCategoryId=" + keyCategoryId + " id='" + field + "' name='" + field + "'   maxlength='250' placeholder='" + label + "' onblur=checkData(this.id) type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div></div>";

                        else
                        {
                            if (textArea)
                                row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'    maxlength='250' placeholder='" + label + "'>" + value + "</textarea></div></div>";
                            else
                                row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=\"form-control asset-name\" keyCategoryId=" + keyCategoryId + " id='" + field + "' name='" + field + "'  maxlength='250' placeholder='" + label + "' type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div></div>";

                        }
                    }
                    else
                    {
                        if (securityCheck)
                            if (textArea)
                                row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'   maxlength='250' placeholder='" + label + "'>" + value + "</textarea> </div></div>";
                            else
                                row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=form-control id='" + field + "' name='" + field + "'   maxlength='250' placeholder='" + label + "' onblur=checkData(this.id) type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div></div>";

                        else
                        {
                            if (textArea)
                                row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'    maxlength='250' placeholder='" + label + "'>" + value + "</textarea></div></div>";
                            else
                                row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=form-control id='" + field + "' name='" + field + "'  maxlength='250' placeholder='" + label + "' type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div></div>";

                        }
                    }
                }


                return row;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                return string.Empty;
            }
        }

        //public string BuildDynamicForm(string field, string label, string value, int customerId, bool isMultiple, int templateID, bool securityCheck, bool textArea, int keyCategoryId, bool isAssetName)
        //{
        //    try
        //    {
        //        string row = "";
        //        string datalist = "";
        //        datalist = BuildDataList(field, customerId);

        //        string customerField = field.Replace("_", " ");
        //        var customerData = db.CustomerTemplateDetails.Where(c => c.CustID == customerId && c.FieldName.Contains(customerField)).FirstOrDefault();
        //        if (customerData != null)
        //        { value = customerData.FieldValue; }

        //        if (isMultiple)
        //        {

        //            if (securityCheck)
        //                if (textArea)
        //                    row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "' maxlength='250' placeholder='" + label + "'>" + value + "</textarea></div> <div class=col-lg-1><button type=button class=glyphicon-plus" + "  id=add_" + field + " onclick=addFunction(this.id) ></button> </div></div>";
        //                else
        //                    row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=form-control name='" + field + "' id='" + field + "'  maxlength='250' placeholder='" + label + "' onblur=checkData(this.id) type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div> <div class=col-lg-1><button type=button class=glyphicon-plus" + "  id=add_" + field + " onclick=addFunction(this.id) ></button> </div></div>";

        //            else
        //            {
        //                if (textArea)
        //                    row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'  maxlength='250' placeholder='" + label + "'>" + value + "</textarea></div><div class=col-lg-1><button type=button class=glyphicon-plus" + "  id=add_" + field + " onclick=addFunction(this.id) ></button> </div></div>";
        //                else
        //                    row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=form-control name='" + field + "' id='" + field + "' maxlength='250' placeholder='" + label + "' type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div> <div class=col-lg-1><button type=button class=glyphicon-plus" + "  id=add_" + field + " onclick=addFunction(this.id) ></button> </div></div>";

        //            }
        //            var keysaddedbycustomer = db.TemplateDynamicFormValues.Where(d => d.ParentkeyId == field && d.CustomerId == customerId && d.IsEnabled == true && d.TemplateId == templateID).ToList();

        //            if (keysaddedbycustomer == null && keysaddedbycustomer.Count == 0)
        //            {
        //                keysaddedbycustomer = db.TemplateDynamicFormValues.Where(d => d.ParentkeyId == field && d.CustomerId == customerId).ToList();
        //            }

        //            if (keysaddedbycustomer != null && keysaddedbycustomer.Count > 0)
        //            {
        //                for (int i = 0; i < keysaddedbycustomer.Count; i++)
        //                {
        //                    datalist = BuildDataList(field, customerId);

        //                    if (textArea)
        //                        row = row + "<div class=form-group id=div_" + keysaddedbycustomer[i].TemplateKey + "><label class=col-lg-4 control-label></label><div class=col-lg-6><textarea rows='4' cols='35' name='" + keysaddedbycustomer[i].TemplateKey + "'  maxlength='250' placeholder='" + label + "'>" + keysaddedbycustomer[i].UserInputs + "</textarea></div><div class=col-lg-1><button type=button class=glyphicon-minus" + "  id=remove_" + keysaddedbycustomer[i].TemplateKey + " onclick=removeFunction(this.id) ></button> </div></div>";
        //                    else
        //                        //row = row + "<div class=form-group id=div_" + keysaddedbycustomer[i].TemplateKey + "><label class=col-lg-4 control-label></label><div class=col-lg-6><input class=form-control name='" + keysaddedbycustomer[i].TemplateKey + "' placeholder='" + label + "' type=text  list='" + keysaddedbycustomer[i].TemplateKey + "'>" + datalist + "</div><div class=col-lg-1><button type=button class=glyphicon-minus" + "  id=remove_" + keysaddedbycustomer[i].TemplateKey + " onclick=removeFunction(this.id) ></button> </div></div>";
        //                        row = row + "<div class=form-group id=div_" + keysaddedbycustomer[i].TemplateKey + "><label class=col-lg-4 control-label></label><div class=col-lg-6><input class=form-control id='" + keysaddedbycustomer[i].TemplateKey + "' name='" + keysaddedbycustomer[i].TemplateKey + "' maxlength='250' onblur=checkData(this.id)  placeholder='" + label + "' type=text value='" + keysaddedbycustomer[i].UserInputs + "' list='" + keysaddedbycustomer[i].TemplateKey + "'>" + datalist + "</div><div class=col-lg-1><button type=button class=glyphicon-minus" + "  id=remove_" + keysaddedbycustomer[i].TemplateKey + " onclick=removeFunction(this.id) ></button> </div></div>";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (isAssetName)
        //            {
        //                if (securityCheck)
        //                    if (textArea)
        //                        row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'   maxlength='250' placeholder='" + label + "'>" + value + "</textarea> </div></div>";
        //                    else
        //                        row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=\"form-control asset-name\" keyCategoryId=" + keyCategoryId + " id='" + field + "' name='" + field + "'   maxlength='250' placeholder='" + label + "' onblur=checkData(this.id) type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div></div>";

        //                else
        //                {
        //                    if (textArea)
        //                        row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'    maxlength='250' placeholder='" + label + "'>" + value + "</textarea></div></div>";
        //                    else
        //                        row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=\"form-control asset-name\" keyCategoryId=" + keyCategoryId + " id='" + field + "' name='" + field + "'  maxlength='250' placeholder='" + label + "' type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div></div>";

        //                }
        //            }
        //            else
        //            {
        //                if (securityCheck)
        //                    if (textArea)
        //                        row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'   maxlength='250' placeholder='" + label + "'>" + value + "</textarea> </div></div>";
        //                    else
        //                        row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=form-control id='" + field + "' name='" + field + "'   maxlength='250' placeholder='" + label + "' onblur=checkData(this.id) type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div></div>";

        //                else
        //                {
        //                    if (textArea)
        //                        row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><textarea rows='4' cols='35' name='" + field + "'    maxlength='250' placeholder='" + label + "'>" + value + "</textarea></div></div>";
        //                    else
        //                        row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=form-control id='" + field + "' name='" + field + "'  maxlength='250' placeholder='" + label + "' type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div></div>";

        //                }
        //            }
        //        }


        //        return row;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //        return string.Empty;
        //    }
        //}

        public string DynamicFormTop()
        {
            string row = "";
            row = "<form class=form-horizontal  method=post action='" + Url.Content("~/DocumentManagement/FillDynamicForm/") + "'><div class=row><div class=col-lg-12><div class=well bs-component><fieldset><div class=row>";
            return row;
        }

        public string DynamicFormBottom(int customerId)
        {
            string row = "";
            row = "</ div></ div></ div></ fieldset ></ div><input type=hidden name='customerId' value=" + customerId + "></ div>";
            return row;
        }

        public string BuildSubmitButton(int? id, int orgId, int? associateId)
        {
            string row = "";
            row = "</div><div class=row><div class=col-lg-12><div class=col-md-2><input type=hidden value=" + associateId + " name=AssociateTemplateId /><input type=hidden value=" + id + " name=TemplateId /> <input class='btn btn-default' id=btnSubmit type=submit value=Submit /></div><div><button type=button value=Cancel class='btn btn-cancel'  onclick=location.href='" + Url.Action("SearchCategory", "DocumentManagement", new { id = orgId }) + "'>Cancel</button></div></div></div></ form>";
            return row;
        }

        public string BuildDataList(string KeyValue, int CustomerId)
        {
            string row = "";
            string rowval = "";
            //string customerIDS =string.Empty;
            //if (Session["MultipleCustomer"] != null && !string.IsNullOrEmpty(Session["MultipleCustomer"].ToString()))
            //{
            //    customerIDS = Session["MultipleCustomerIDS"].ToString();
            //}

            var customerData = db.CustomerTemplateDetails.Where(c => c.CustID == CustomerId && c.FieldName.Contains(KeyValue)).FirstOrDefault();


            var objkeyInputs = db.TemplateDynamicFormValues.Where(m => m.CustomerId == CustomerId && m.TemplateKey == KeyValue).ToList();
            if (objkeyInputs != null && objkeyInputs.Count != 0)
            {
                var lst = objkeyInputs.Select(p => p.UserInputs).Distinct().ToList();
                foreach (var item in lst)
                {
                    if (item != null)
                        rowval = rowval + "<option value='" + item.Replace("'", " ") + "'>" + item.Replace("'", " ") + "</option>";
                }

                if (customerData != null)
                {
                    rowval = rowval + "<option value='" + customerData.FieldValue.Replace("'", " ") + "'>" + customerData.FieldValue.Replace("'", " ") + "</option>";

                }
                row = "<datalist  id='" + KeyValue + "'>" + rowval + "</datalist> ";
            }

            return row;
        }

        public string BuildSubmitButtonOld(int? id, string BtnValue, string url, int? associateId)
        {
            string row = "";
            row = "</div><div class=row><div class=col-lg-12><div class=col-md-2><input type=hidden value=" + associateId + " name=AssociateTemplateId /><input type=hidden value=" + id + " name=TemplateId /> <input class='btn btn-primary' type=submit value=" + BtnValue + " /></div><div><button type=button value=Cancel class=btn btn-cancel  onclick=location.href='/DocumentManagement/" + url + "'>Cancel</button></div></div></div></ form>";
            return row;
        }

        public string DynamicFormStepCount(int stepCount, string AssociateDocName)
        {
            string row = "";
            row = "<h5 padding: 8px;border-radius: 5px;margin-top: 0px'>" + AssociateDocName + "</h5>";

            //row = "<h4 style='background-color: gold;padding: 8px;border-radius: 5px;margin-top: 0px'>STEP " + stepCount + " :  " + AssociateDocName + "</h4>";
            return row;
        }
        public string DynamicFormName(string AssociateDocName)
        {
            string row = "";
            row = "<h5 padding: 8px;border-radius: 5px;margin-top: 0px'>" + AssociateDocName + "</h5>";

            //row = "<h4 style='background-color: gold;padding: 8px;border-radius: 5px;margin-top: 0px'> Form " + AssociateDocName + "</h4>";
            return row;
        }

        //public ActionResult FillDynamicForm(FormCollection obj)
        //{
        //    TempData["FormCollection"] = obj;
        //    string[] etrxakeys = Array.FindAll(obj.AllKeys, x => x.Contains("add_"));
        //    List<TemplateKeysPointer> lst = new List<TemplateKeysPointer>();
        //    int id = 0;
        //    int? associateId = null;
        //    try
        //    {
        //        int customerId = Convert.ToInt32(Session["customerId"]);

        //        if (Request.Form["TemplateId"] != null)
        //            id = Convert.ToInt32(Request.Form["TemplateId"]);
        //        InsertKeysaddedBycustomer(Convert.ToString(id), etrxakeys, customerId.ToString());
        //        if (Request.Form["AssociateTemplateId"] != null)
        //            associateId = Convert.ToInt32(Request.Form["AssociateTemplateId"]);



        //        // Get Already Filled Details For This Template
        //        bool ExistsData = false;
        //        var objAlreadyFilled = db.TemplateDynamicFormValues.Where(a => a.TemplateId == id && a.UserId == userID && a.IsEnabled == true && a.CustomerId == customerId && a.ParentkeyId == null);
        //        if (objAlreadyFilled != null && objAlreadyFilled.Count() > 0)
        //        {
        //            ExistsData = true;
        //        }



        //        var objkey = (from a in db.TemplateKeysPointers
        //                      join b in db.TemplateKeywords on a.TemplateKeyId equals b.TemplateKeyId
        //                      where b.Cloned != true && a.TemplateId == id
        //                      select a).ToList();

        //        //  db.TemplateKeysPointers.Where(m => m.TemplateId == id).ToList();
        //        lst = objkey.GroupBy(p => p.TemplateKeyId)
        //            .Select(grp => grp.First()).ToList();

        //        var addedkyes = db.TemplateDynamicFormValues.Where(a => a.TemplateId == id && a.CustomerId == customerId && a.ParentkeyId != null).ToList();

        //        foreach (var key in addedkyes)
        //        {
        //            if (Array.Exists(obj.AllKeys, element => element.Contains(key.TemplateKey)) && !Array.Exists(etrxakeys, element => element.Contains(key.TemplateKey)))
        //            {
        //                TemplateDynamicFormValue extraobj = db.TemplateDynamicFormValues.Where(a => a.TemplateId == id && a.CustomerId == customerId && a.ParentkeyId != null && a.TemplateKey == key.TemplateKey).FirstOrDefault();

        //                //var duplicates = db.TemplateKeywords.Where(x => x.ClonedFrom == labelName.TemplateKeyId && x.IsEnabled == true).ToList();
        //                extraobj.IsEnabled = true;
        //                extraobj.UserInputs = Request.Form[key.TemplateKey];
        //                db.SaveChanges();
        //            }
        //        }


        //        TemplateDynamicFormValue objDynamicForm = new TemplateDynamicFormValue();
        //        foreach (var li in lst)
        //        {
        //            var TempKeyobj = objData.getKeyDetails(li.TemplateKeyId); // Fetch Keyword Details 
        //            var duplicates = db.TemplateKeywords.Where(x => x.ClonedFrom == TempKeyobj.TemplateKeyId && x.IsEnabled == true).ToList();
        //            if (TempKeyobj != null)
        //            {

        //                // Update or insert dynamic data
        //                if (ExistsData)
        //                {
        //                    if (duplicates != null)
        //                    {
        //                        foreach (var d in duplicates)
        //                        {
        //                            objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userID && b.TemplateKey == d.TemplateKeyValue && b.CustomerId == customerId).FirstOrDefault();
        //                            objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
        //                            objDynamicForm.IsEnabled = true;
        //                            db.SaveChanges();
        //                        }
        //                    }

        //                    objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userID && b.TemplateKey == TempKeyobj.TemplateKeyValue && b.CustomerId == customerId).FirstOrDefault();
        //                    objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
        //                    objDynamicForm.IsEnabled = true;

        //                }
        //                else
        //                {
        //                    if (duplicates != null)
        //                    {
        //                        foreach (var d in duplicates)
        //                        {
        //                            objDynamicForm.TemplateId = id;
        //                            objDynamicForm.TemplateKey = d.TemplateKeyValue;
        //                            objDynamicForm.UserId = userID;
        //                            objDynamicForm.IsEnabled = true;
        //                            objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
        //                            objDynamicForm.CreatedDate = DateTime.Now;
        //                            objDynamicForm.CustomerId = customerId;
        //                            db.TemplateDynamicFormValues.Add(objDynamicForm);
        //                            db.SaveChanges();
        //                        }
        //                    }

        //                    objDynamicForm.TemplateId = id;
        //                    objDynamicForm.TemplateKey = TempKeyobj.TemplateKeyValue;
        //                    objDynamicForm.UserId = userID;
        //                    objDynamicForm.IsEnabled = true;
        //                    objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
        //                    objDynamicForm.CreatedDate = DateTime.Now;
        //                    objDynamicForm.CustomerId = customerId;
        //                    db.TemplateDynamicFormValues.Add(objDynamicForm);
        //                }
        //                db.SaveChanges();
        //            }

        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }


        //    //return RedirectToAction("PreviewDocument", "DocumentManagement", new { id = id });
        //    return RedirectToAction("PrevDocument", "DocumentManagement", new { id = id });
        //}

        //public ActionResult PreviewDocument(int? id)
        //{
        //    string wordContent = "";
        //    ViewBag.WordContent = "";
        //    if (id == null || id == 0)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    ViewBag.TemplateId = id;

        //    // Checking associate template
        //    var objAssociate = db.DocumentTemplates.Find(id);
        //    if (objAssociate != null)
        //    {
        //        ViewBag.Title = "Preview Filled Form  " + objAssociate.DocumentTitle;
        //        if (objAssociate.AssociateTemplateId != null)
        //        {
        //            ViewBag.NxtBtnValue = "Next";
        //            ViewBag.AssociateId = objAssociate.AssociateTemplateId;
        //        }
        //    }
        //    ViewBag.TemplateId = id;
        //    int userId = Convert.ToInt32(Session["UserId"]);
        //    try
        //    {
        //        var objTemplate = db.DocumentTemplates.Find(id);
        //        wordContent = getWordContent(objTemplate.TemplateFileName);
        //        var objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true).ToList();
        //        foreach (var temp in objDynamicForm)
        //        {
        //            string regstr = "&lt;" + temp.TemplateKey + "&gt;";
        //            string replacestr = "<b><i>" + temp.UserInputs + "</i></b>";
        //            Regex regexText = new Regex(regstr);
        //            wordContent = regexText.Replace(wordContent, replacestr);
        //           // Regex regexText1 = new Regex("\n");
        //           // wordContent = regexText1.Replace(wordContent, "<br></br>");
        //            // wordContent = wordContent.Replace("<" + temp.TemplateKey + ">", "<b><i>" + temp.UserInputs + "</i></b>");
        //            // wordContent = wordContent.Replace("\n", "<br></br>");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }
        //    ViewBag.WordContent = wordContent;
        //    return View();
        //}

        //public ActionResult PreviewDocument(int? id)
        //{
        //    var template = db.DocumentTemplates.Where(c => c.TemplateId == id).FirstOrDefault();
        //    int categoryID = template.DocumentCategory;

        //    var clauselist = (from obj in db.ClouseandCategoryMapings

        //                      join c in db.Clice on obj.clouseID equals c.Id into g
        //                      from subset in g.DefaultIfEmpty()
        //                      where obj.categoryID == categoryID && subset.IsEnabled == true
        //                      select new PreviewClauses { Clause = subset.Clouse1, ClauseID = obj.clouseID }
        //            );

        //    string wordContent = "";
        //    ViewBag.WordContent = "";
        //    if (id == null || id == 0)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    ViewBag.TemplateId = id;
        //    Session["CurrentTempAId"] = id;
        //    Session["CurrentEditId"] = id;
        //    // Checking associate template
        //    var objAssociate = db.DocumentTemplates.Find(id);
        //    if (objAssociate != null)
        //    {
        //        ViewBag.Title = "Preview Filled Form  " + objAssociate.DocumentTitle;
        //        if (objAssociate.AssociateTemplateId != null)
        //        {
        //            ViewBag.NxtBtnValue = "Next";
        //            ViewBag.AssociateId = objAssociate.AssociateTemplateId;
        //        }
        //    }
        //    ViewBag.TemplateId = id;
        //    int userId = Convert.ToInt32(Session["UserId"]);
        //    int customerID = 0;
        //    if (Session["customerId"] != null)
        //    {
        //        customerID = Convert.ToInt32(Session["customerId"]);
        //    }
        //    try
        //    {
        //        var objTemplate = db.DocumentTemplates.Find(id);
        //        wordContent = getWordContent(objTemplate.TemplateFileName);

        //        var objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true && b.ParentkeyId == null && b.CustomerId == customerID).ToList().OrderByDescending(x => x.RowId);

        //        var maxlength = db.GetMaxlengthOfUserInputs(customerID, id).ToList();

        //        var statementKeys = db.AssociatedKeyGroups.Where(t => t.TemplateID == id && t.DesignType == "Statement").Select(k => k.KeyID).ToList();

        //        char[] alpha = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        //        foreach (var temp in objDynamicForm)
        //        {
        //            var inputs = db.TemplateDynamicFormValues.Where(w => w.ParentkeyId == temp.TemplateKey && w.TemplateId == temp.TemplateId && w.CustomerId == customerID && w.IsEnabled == true).ToList();

        //            var keyID = db.TemplateKeywords.Where(k => k.TemplateKeyValue == temp.TemplateKey && k.Cloned != true).FirstOrDefault();
        //            if (keyID != null)
        //            {
        //                if (!statementKeys.Contains(keyID.TemplateKeyId))
        //                {
        //                    if (inputs != null && inputs.Count > 0)
        //                    {
        //                        bool serialNumber = false;
        //                        int p = 1;
        //                        // temp.UserInputs = alpha[0] + ". " + temp.UserInputs;
        //                        var hasAssociatedGroup = (from a in db.AssociatedKeyGroups
        //                                                  join k in db.TemplateKeywords on a.KeyID equals k.TemplateKeyId

        //                                                  where k.TemplateKeyValue == temp.TemplateKey && a.TemplateID == temp.TemplateId

        //                                                  select new { groupName = a.GroupName, firstColumn = a.FirstColumn }).ToList();
        //                        if (hasAssociatedGroup != null && hasAssociatedGroup.Count() > 0)
        //                        {
        //                            string groupName = hasAssociatedGroup.FirstOrDefault().groupName;


        //                            for (int u = 0; u < inputs.Count; u++)
        //                            {
        //                                if (hasAssociatedGroup.Count() > 0)
        //                                {
        //                                    if (hasAssociatedGroup.FirstOrDefault().firstColumn == "First")
        //                                    {
        //                                        var isFirstkey = db.AssociatedKeyGroups.Where(k => k.KeyID == keyID.TemplateKeyId && k.TemplateID == temp.TemplateId && k.KeyOrder == 1).FirstOrDefault();
        //                                        if (isFirstkey != null)
        //                                        {
        //                                            serialNumber = true;
        //                                            if (p == 1)
        //                                                temp.UserInputs = alpha[0] + ". " + temp.UserInputs;
        //                                        }

        //                                    }
        //                                    else
        //                                    {
        //                                        serialNumber = true;
        //                                        if (p == 1)
        //                                            temp.UserInputs = alpha[0] + ". " + temp.UserInputs;
        //                                    }
        //                                    var length = maxlength.Where(g => g.groupname == groupName).FirstOrDefault().lettercount;
        //                                    int maximumlength = length != null ? Convert.ToInt32(length) : 0;
        //                                    //long length = maxlength.FirstOrDefault().Value;

        //                                    if (p == 1 && temp.UserInputs.Length < length)
        //                                    {
        //                                        StringBuilder appendText1 = new StringBuilder();
        //                                        appendText1.Append("".PadLeft((maximumlength - temp.UserInputs.Length), ' ').Replace(" ", " "));
        //                                        temp.UserInputs = temp.UserInputs + " " + appendText1;
        //                                    }
        //                                    if (inputs[u].UserInputs.Length < length)
        //                                    {
        //                                        Int32 tobeadded = maximumlength - inputs[u].UserInputs.Length;
        //                                        StringBuilder appendText = new StringBuilder();
        //                                        appendText.Append("".PadRight(tobeadded, ' ').Replace(" ", " "));
        //                                        if (serialNumber)
        //                                            temp.UserInputs = temp.UserInputs + "<br/> " + alpha[p] + ". " + inputs[u].UserInputs + "  " + appendText;
        //                                        //.PadRight(Convert.ToInt32(tobeadded + 2)) + "\t";
        //                                        else
        //                                            temp.UserInputs = temp.UserInputs + "<br/> " + inputs[u].UserInputs + "  " + appendText;
        //                                    }
        //                                    else
        //                                    {
        //                                        if (serialNumber)
        //                                            temp.UserInputs = temp.UserInputs + "<br/> " + alpha[p] + ". " + inputs[u].UserInputs;
        //                                        else
        //                                            temp.UserInputs = temp.UserInputs + "<br/> " + inputs[u].UserInputs;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (p == 1)
        //                                        temp.UserInputs = alpha[0] + ". " + temp.UserInputs;
        //                                    temp.UserInputs = temp.UserInputs + "<br/> " + alpha[p] + ". " + inputs[u].UserInputs;
        //                                }
        //                                p++;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            int q = 1;
        //                            for (int u = 0; u < inputs.Count; u++)
        //                            {

        //                                temp.UserInputs = temp.UserInputs + "<br/> " + alpha[q] + ". " + inputs[u].UserInputs;
        //                                q++;
        //                            }
        //                        }
        //                    }



        //                    // wordContent = wordContent.Replace("\n", "<br></br>");
        //                }

        //                //var duplicateKeys = db.TemplateKeywords.Where(k => k.Cloned == true && k.ClonedFrom == keyID.TemplateKeyId).ToList();

        //                //foreach (var cloned in duplicateKeys)
        //                //{
        //                //    wordContent = wordContent.Replace("&lt;" + cloned.TemplateKeyValue + "&gt;", "<i>" + temp.UserInputs + "</i>");
        //                //}

        //                wordContent = wordContent.Replace("&lt;" + temp.TemplateKey + "&gt;", "<i>" + temp.UserInputs + "</i>");
        //            }
        //        }
        //        var keys = (from k in db.AssociatedKeyGroups
        //                    join t in db.TemplateKeywords on k.KeyID equals t.TemplateKeyId
        //                    where k.TemplateID == id && k.Statement == true
        //                    select new
        //                    {
        //                        keyID = t.TemplateKeyId,
        //                        keyValue = t.TemplateKeyValue,
        //                        order = k.KeyOrder,
        //                        Group = k.GroupName
        //                    }).ToList();
        //        if (keys != null && keys.Count > 0)
        //        {
        //            var groups = keys.GroupBy(x => x.Group).Select(g => g.First());

        //            string groupName = string.Empty;
        //            foreach (var g in groups)
        //            {
        //                string statement = string.Empty;
        //                bool isNumeric = false;
        //                int startsFrom = 0;
        //                var groupAutoNo = db.AssociatedKeyGroups.Where(gp => gp.GroupName == g.Group && gp.AutoNumberStartsFrom != null).FirstOrDefault();
        //                int AsciicharforAutoNumber = 97;
        //                if (groupAutoNo.AutoNumberStartsFrom != null)
        //                {
        //                    AsciicharforAutoNumber = Convert.ToInt32(groupAutoNo.AutoNumberStartsFrom);
        //                }
        //                groupName = g.Group;
        //                var groupKeys = keys.Where(k => k.Group == g.Group).OrderBy(o => o.order).ToList();
        //                int key1 = groupKeys.FirstOrDefault().keyID;
        //                string key1Value = groupKeys.FirstOrDefault().keyValue;
        //                foreach (var p in groupKeys)
        //                {
        //                    var inputs = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.TemplateKey == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).FirstOrDefault();
        //                    if (inputs != null)
        //                        statement = statement + " " + inputs.UserInputs;

        //                }

        //                var multivalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.ParentkeyId == key1Value && w.CustomerId == customerID && w.IsEnabled == true).ToList();

        //                if (multivalues != null && multivalues.Count() > 0)
        //                {

        //                    char asciivalue = Convert.ToChar(AsciicharforAutoNumber);
        //                    //   statement = AsciicharforAutoNumber + ". " + statement + "<br/><" + groupName + "_1>";
        //                    statement = asciivalue + ". " + statement + "<br/><" + groupName + "_1>";
        //                    AsciicharforAutoNumber++;
        //                }
        //                //var duplicateKeys = db.TemplateKeywords.Where(k => k.Cloned == true && k.ClonedFrom == key1).ToList();

        //                //foreach (var cloned in duplicateKeys)
        //                //{
        //                //    wordContent = wordContent.Replace("&lt;" + groupName + "&gt;", "<i>" + statement + "</i>");
        //                //}

        //                wordContent = wordContent.Replace("&lt;" + groupName + "&gt;", "<i>" + statement + "</i>");
        //                //SearchAndReplace(groupName, statement, filepath);

        //                groupName = groupName + "_1";

        //                foreach (var m in multivalues)
        //                {

        //                    char asciivaluenext = Convert.ToChar(AsciicharforAutoNumber);
        //                    statement = asciivaluenext + ". ";
        //                    foreach (var p in groupKeys)
        //                    {
        //                        var multiKeyvalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.ParentkeyId == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).FirstOrDefault();
        //                        if (multiKeyvalues != null)
        //                        {
        //                            //foreach (var m1 in multiKeyvalues)
        //                            //{
        //                            statement = statement + " " + multiKeyvalues.UserInputs;
        //                            //}
        //                        }
        //                    }
        //                    statement = statement + "<br/><" + groupName + "_1>";

        //                    wordContent = wordContent.Replace("&lt;" + groupName + "&gt;", "<i>" + statement + "</i>");
        //                    //statement = statement + "<br/><" + groupName + "_"+"_2";
        //                    //var duplicateKeys1 = db.TemplateKeywords.Where(k => k.Cloned == true && k.ClonedFrom == key1).ToList();
        //                    //foreach (var cloned in duplicateKeys)
        //                    //{
        //                    //    wordContent = wordContent.Replace("&lt;" + groupName + "&gt;", "<i>" + statement + "</i>");
        //                    //}
        //                    wordContent = wordContent.Replace("<" + groupName + ">", "<i>" + statement + "</i>");
        //                    //SearchAndReplace(groupName, statement, filepath);
        //                    groupName = groupName + "_";
        //                    AsciicharforAutoNumber++;
        //                }
        //                groupName = g.Group;

        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }
        //    ViewBag.WordContent = wordContent;
        //    ViewBag.ClientID = Session["ServiceID"];
        //    ViewBag.customerID = customerID;
        //    TempData.Keep();
        //    return View(clauselist.ToList());
        //}

        //public ActionResult FillDynamicForm(FormCollection obj)
        //{
        //    TempData["FormCollection"] = obj;
        //    //string[] etrxakeys = Array.FindAll(obj.AllKeys, x => x.Contains("add_"));
        //    string[] etrxakeys = Array.FindAll(obj.AllKeys, x => x.Contains("polo")
        //    && x.Trim().Length > 4);
        //    List<TemplateKeysPointer> lst = new List<TemplateKeysPointer>();
        //    int id = 0;
        //    int? associateId = null;
        //    try
        //    {
        //        int customerId = Convert.ToInt32(Session["customerId"]);
        //        if (Request.Form["TemplateId"] != null)
        //            id = Convert.ToInt32(Request.Form["TemplateId"]);
        //        InsertKeysaddedBycustomer(Convert.ToString(id), etrxakeys, customerId.ToString());
        //        if (Request.Form["AssociateTemplateId"] != null)
        //            associateId = Convert.ToInt32(Request.Form["AssociateTemplateId"]);




        //        // Get Already Filled Details For This Template
        //        bool ExistsData = false;
        //        var objAlreadyFilled = db.TemplateDynamicFormValues.Where(a => a.TemplateId == id && a.UserId == userID && a.IsEnabled == true && a.CustomerId == customerId && a.ParentkeyId == null);
        //        if (objAlreadyFilled != null && objAlreadyFilled.Count() > 0)
        //        {
        //            ExistsData = true;
        //        }

        //        var objkey = (from a in db.TemplateKeysPointers
        //                      join b in db.TemplateKeywords on a.TemplateKeyId equals b.TemplateKeyId
        //                      where b.Cloned != true && a.TemplateId == id
        //                      select a).ToList();

        //        //  db.TemplateKeysPointers.Where(m => m.TemplateId == id).ToList();
        //        lst = objkey.GroupBy(p => p.TemplateKeyId)
        //            .Select(grp => grp.First()).ToList();

        //        var addedkyes = db.TemplateDynamicFormValues.Where(a => a.TemplateId == id && a.CustomerId == customerId && a.ParentkeyId != null).ToList();

        //        foreach (var key in addedkyes)
        //        {
        //            if (Array.Exists(obj.AllKeys, element => element.Contains(key.TemplateKey)) && !Array.Exists(etrxakeys, element => element.Contains(key.TemplateKey)))
        //            {
        //                TemplateDynamicFormValue extraobj = db.TemplateDynamicFormValues.Where(a => a.TemplateId == id && a.CustomerId == customerId && a.ParentkeyId != null && a.TemplateKey == key.TemplateKey).FirstOrDefault();

        //                //var duplicates = db.TemplateKeywords.Where(x => x.ClonedFrom == labelName.TemplateKeyId && x.IsEnabled == true).ToList();
        //                extraobj.IsEnabled = true;
        //                extraobj.UserInputs = Request.Form[key.TemplateKey];
        //                db.SaveChanges();
        //            }
        //        }


        //        TemplateDynamicFormValue objDynamicForm = new TemplateDynamicFormValue();
        //        foreach (var li in lst)
        //        {
        //            var TempKeyobj = objData.getKeyDetails(li.TemplateKeyId); // Fetch Keyword Details 
        //            var duplicates = db.TemplateKeywords.Where(x => x.ClonedFrom == TempKeyobj.TemplateKeyId && x.IsEnabled == true).ToList();
        //            if (TempKeyobj != null)
        //            {

        //                // Update or insert dynamic data
        //                if (ExistsData)
        //                {
        //                    if (duplicates != null)
        //                    {
        //                        foreach (var d in duplicates)
        //                        {
        //                            objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userID && b.TemplateKey == d.TemplateKeyValue && b.CustomerId == customerId).FirstOrDefault();
        //                            if (objDynamicForm != null)
        //                            {
        //                                objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
        //                                objDynamicForm.IsEnabled = true;
        //                                db.SaveChanges();
        //                            }
        //                        }
        //                    }

        //                    objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userID && b.TemplateKey == TempKeyobj.TemplateKeyValue && b.CustomerId == customerId).FirstOrDefault();
        //                    if (objDynamicForm != null)
        //                    {
        //                        objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
        //                        objDynamicForm.IsEnabled = true;
        //                    }

        //                }
        //                else
        //                {
        //                    if (duplicates != null)
        //                    {
        //                        foreach (var d in duplicates)
        //                        {
        //                            objDynamicForm.TemplateId = id;
        //                            objDynamicForm.TemplateKey = d.TemplateKeyValue;
        //                            objDynamicForm.UserId = userID;
        //                            objDynamicForm.IsEnabled = true;
        //                            objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
        //                            objDynamicForm.CreatedDate = DateTime.Now;
        //                            objDynamicForm.CustomerId = customerId;
        //                            db.TemplateDynamicFormValues.Add(objDynamicForm);
        //                            db.SaveChanges();
        //                        }
        //                    }

        //                    objDynamicForm.TemplateId = id;
        //                    objDynamicForm.TemplateKey = TempKeyobj.TemplateKeyValue;
        //                    objDynamicForm.UserId = userID;
        //                    objDynamicForm.IsEnabled = true;
        //                    objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
        //                    objDynamicForm.CreatedDate = DateTime.Now;
        //                    objDynamicForm.CustomerId = customerId;
        //                    db.TemplateDynamicFormValues.Add(objDynamicForm);
        //                }
        //                db.SaveChanges();
        //            }

        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }


        //    //return RedirectToAction("PreviewDocument", "DocumentManagement", new { id = id });
        //    return RedirectToAction("PrevDocument", "DocumentManagement", new { id = id });
        //}


        #region FillDynamicForm
        public ActionResult FillDynamicForm(FormCollection obj)
        {
            int TemplateId = 0;
            int customerId = Convert.ToInt32(Session["customerId"]);
            if (Request.Form["TemplateId"] != null)
                TemplateId = Convert.ToInt32(Request.Form["TemplateId"]);

            db.TemplateDynamicFormValues.Where(a => a.TemplateId == TemplateId &&
               a.UserId == userID && a.CustomerId == customerId).ToList()
               .ForEach(t => t.IsEnabled = false);
            db.SaveChanges();

            foreach (var key in obj.AllKeys)
            {
                if (!new string[] { "customerid", "templateid" }.Contains(key.ToLower()))
                {
                    TemplateDynamicFormValue templateDynamicFormValue =
                    db.TemplateDynamicFormValues.Where(a => a.TemplateId == TemplateId &&
                    a.UserId == userID && a.CustomerId == customerId &&
                    string.Compare(a.TemplateKey, key, true) == 0).FirstOrDefault();
                    if (templateDynamicFormValue != null)
                    {
                        templateDynamicFormValue.UserInputs = Request.Form[key];
                        templateDynamicFormValue.IsEnabled = true;
                        db.Entry(templateDynamicFormValue).State = EntityState.Modified;
                    }
                    else
                    {
                        templateDynamicFormValue = new TemplateDynamicFormValue();
                        templateDynamicFormValue.TemplateId = TemplateId;
                        templateDynamicFormValue.TemplateKey = key;
                        templateDynamicFormValue.UserId = userID;
                        templateDynamicFormValue.IsEnabled = true;
                        templateDynamicFormValue.UserInputs = Request.Form[key];
                        templateDynamicFormValue.CreatedDate = DateTime.Now;
                        if (key.Contains("polo") && key.Trim().Length > 4)
                            templateDynamicFormValue.ParentkeyId = "polo";
                        templateDynamicFormValue.CustomerId = customerId;
                        db.TemplateDynamicFormValues.Add(templateDynamicFormValue);
                    }
                    db.SaveChanges();
                }
            }

            //TempData["FormCollection"] = obj;
            ////string[] etrxakeys = Array.FindAll(obj.AllKeys, x => x.Contains("add_"));
            //string[] etrxakeys = Array.FindAll(obj.AllKeys, x => x.Contains("polo")
            //&& x.Trim().Length > 4);
            //List<TemplateKeysPointer> lst = new List<TemplateKeysPointer>();
            //int id = 0;
            ////int? associateId = null;
            //try
            //{
            //    int customerId = Convert.ToInt32(Session["customerId"]);
            //    if (Request.Form["TemplateId"] != null)
            //        id = Convert.ToInt32(Request.Form["TemplateId"]);
            //    InsertKeysaddedBycustomer(Convert.ToString(id), etrxakeys, customerId.ToString());
            //    if (Request.Form["AssociateTemplateId"] != null)
            //        associateId = Convert.ToInt32(Request.Form["AssociateTemplateId"]);




            //    // Get Already Filled Details For This Template
            //    bool ExistsData = false;
            //    var objAlreadyFilled = db.TemplateDynamicFormValues.Where(a => a.TemplateId == id && a.UserId == userID && a.IsEnabled == true && a.CustomerId == customerId && a.ParentkeyId == null);
            //    if (objAlreadyFilled != null && objAlreadyFilled.Count() > 0)
            //    {
            //        ExistsData = true;
            //    }

            //    var objkey = (from a in db.TemplateKeysPointers
            //                  join b in db.TemplateKeywords on a.TemplateKeyId equals b.TemplateKeyId
            //                  where b.Cloned != true && a.TemplateId == id
            //                  select a).ToList();

            //    //  db.TemplateKeysPointers.Where(m => m.TemplateId == id).ToList();
            //    lst = objkey.GroupBy(p => p.TemplateKeyId)
            //        .Select(grp => grp.First()).ToList();

            //    var addedkyes = db.TemplateDynamicFormValues.Where(a => a.TemplateId == id && a.CustomerId == customerId && a.ParentkeyId != null).ToList();

            //    foreach (var key in addedkyes)
            //    {
            //        if (Array.Exists(obj.AllKeys, element => element.Contains(key.TemplateKey)) && !Array.Exists(etrxakeys, element => element.Contains(key.TemplateKey)))
            //        {
            //            TemplateDynamicFormValue extraobj = db.TemplateDynamicFormValues.Where(a => a.TemplateId == id && a.CustomerId == customerId && a.ParentkeyId != null && a.TemplateKey == key.TemplateKey).FirstOrDefault();

            //            //var duplicates = db.TemplateKeywords.Where(x => x.ClonedFrom == labelName.TemplateKeyId && x.IsEnabled == true).ToList();
            //            extraobj.IsEnabled = true;
            //            extraobj.UserInputs = Request.Form[key.TemplateKey];
            //            db.SaveChanges();
            //        }
            //    }


            //    TemplateDynamicFormValue objDynamicForm = new TemplateDynamicFormValue();
            //    foreach (var li in lst)
            //    {
            //        var TempKeyobj = objData.getKeyDetails(li.TemplateKeyId); // Fetch Keyword Details 
            //        var duplicates = db.TemplateKeywords.Where(x => x.ClonedFrom == TempKeyobj.TemplateKeyId && x.IsEnabled == true).ToList();
            //        if (TempKeyobj != null)
            //        {

            //            // Update or insert dynamic data
            //            if (ExistsData)
            //            {
            //                if (duplicates != null)
            //                {
            //                    foreach (var d in duplicates)
            //                    {
            //                        objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userID && b.TemplateKey == d.TemplateKeyValue && b.CustomerId == customerId).FirstOrDefault();
            //                        if (objDynamicForm != null)
            //                        {
            //                            objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
            //                            objDynamicForm.IsEnabled = true;
            //                            db.SaveChanges();
            //                        }
            //                    }
            //                }

            //                objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userID && b.TemplateKey == TempKeyobj.TemplateKeyValue && b.CustomerId == customerId).FirstOrDefault();
            //                if (objDynamicForm != null)
            //                {
            //                    objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
            //                    objDynamicForm.IsEnabled = true;
            //                }

            //            }
            //            else
            //            {
            //                if (duplicates != null)
            //                {
            //                    foreach (var d in duplicates)
            //                    {
            //                        objDynamicForm.TemplateId = id;
            //                        objDynamicForm.TemplateKey = d.TemplateKeyValue;
            //                        objDynamicForm.UserId = userID;
            //                        objDynamicForm.IsEnabled = true;
            //                        objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
            //                        objDynamicForm.CreatedDate = DateTime.Now;
            //                        objDynamicForm.CustomerId = customerId;
            //                        db.TemplateDynamicFormValues.Add(objDynamicForm);
            //                        db.SaveChanges();
            //                    }
            //                }

            //                objDynamicForm.TemplateId = id;
            //                objDynamicForm.TemplateKey = TempKeyobj.TemplateKeyValue;
            //                objDynamicForm.UserId = userID;
            //                objDynamicForm.IsEnabled = true;
            //                objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
            //                objDynamicForm.CreatedDate = DateTime.Now;
            //                objDynamicForm.CustomerId = customerId;
            //                db.TemplateDynamicFormValues.Add(objDynamicForm);
            //            }
            //            db.SaveChanges();
            //        }

            //        db.SaveChanges();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ErrorLog.LogThisError(ex);
            //}


            //return RedirectToAction("PreviewDocument", "DocumentManagement", new { id = id });
            return RedirectToAction("PrevDocument", "DocumentManagement", new { id = TemplateId });
        }
        #endregion


        public ActionResult PreviewDocument(int? id)
        {
            var template = db.DocumentTemplates.Where(c => c.TemplateId == id).FirstOrDefault();
            int categoryID = template.DocumentCategory;

            var clauselist = (from obj in db.ClouseandCategoryMapings

                              join c in db.Clice on obj.clouseID equals c.Id into g
                              from subset in g.DefaultIfEmpty()
                              where obj.categoryID == categoryID && subset.IsEnabled == true
                              select new PreviewClauses { Clause = subset.Clouse1, ClauseID = obj.clouseID }
                    );

            string wordContent = "";
            ViewBag.WordContent = "";
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.TemplateId = id;
            Session["CurrentTempAId"] = id;
            Session["CurrentEditId"] = id;
            // Checking associate template
            var objAssociate = db.DocumentTemplates.Find(id);
            if (objAssociate != null)
            {
                ViewBag.Title = "Preview Filled Form  " + objAssociate.DocumentTitle;
                if (objAssociate.AssociateTemplateId != null)
                {
                    ViewBag.NxtBtnValue = "Next";
                    ViewBag.AssociateId = objAssociate.AssociateTemplateId;
                }
            }
            ViewBag.TemplateId = id;
            int userId = Convert.ToInt32(Session["UserId"]);
            int customerID = 0;
            if (Session["customerId"] != null)
            {
                customerID = Convert.ToInt32(Session["customerId"]);
            }
            try
            {
                var objTemplate = db.DocumentTemplates.Find(id);
                wordContent = getWordContent(objTemplate.TemplateFileName);
                char[] alpha = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
                ////Code for Table
                var keystable = (from k in db.AssociatedKeyGroups
                                 join t in db.TemplateKeywords on k.KeyID equals t.TemplateKeyId
                                 where k.TemplateID == id && k.Statement == false
                                 select new
                                 {
                                     keyID = t.TemplateKeyId,
                                     keyValue = t.TemplateKeyValue,
                                     order = k.KeyOrder,
                                     Group = k.GroupName
                                 }).ToList();

                //for statement 
                var keys = (from k in db.AssociatedKeyGroups
                            join t in db.TemplateKeywords on k.KeyID equals t.TemplateKeyId
                            where k.TemplateID == id && k.Statement == true
                            select new
                            {
                                keyID = t.TemplateKeyId,
                                keyValue = t.TemplateKeyValue,
                                order = k.KeyOrder,
                                Group = k.GroupName
                            }).ToList();
                //logic for table
                if (keystable != null && keystable.Count > 0)
                {
                    var groups = keystable.GroupBy(x => x.Group)
                  .Select(g => g.First());
                    string groupName = string.Empty;
                    StringBuilder htmlTable = new StringBuilder();
                    htmlTable.Append("<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'>");
                    int chkloop = 0;
                    foreach (var g in groups)
                    {
                        string statement = string.Empty;
                        bool isNumeric = false;
                        int startsFrom = 0;
                        groupName = g.Group;
                        var groupKeys = keystable.Where(k => k.Group == g.Group).OrderBy(o => o.order).ToList();
                        int key1 = groupKeys.FirstOrDefault().keyID;
                        string key1Value = groupKeys.FirstOrDefault().keyValue;
                        int count = 0;
                        int countm = 0;
                        foreach (var p in groupKeys)
                        {
                            if (chkloop == 0)
                            {
                                for (int th = 0; th < groupKeys.Count; th++)
                                {
                                    string d = groupKeys[th].keyValue;
                                    var keyID2 = db.TemplateKeywords.Where(k => k.TemplateKeyValue == d && k.Cloned != true).FirstOrDefault();
                                    if (keyID2 != null)
                                    {
                                        htmlTable.Append("<th style='border: 1px solid #ccc'>" + keyID2.TemplateKeyLabels + "</th>");

                                    }
                                    //else
                                    //{
                                    //    var keyIDcloned = db.TemplateKeywords.Where(k => k.TemplateKeyValue == d).FirstOrDefault();
                                    //    htmlTable.Append("<th style='border: 1px solid #ccc'>" + keyIDcloned.TemplateKeyLabels + "</th>");
                                    //}
                                    chkloop = 1;
                                }
                            }
                            var inputs = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.TemplateKey == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).FirstOrDefault();
                            if (inputs != null)
                            {
                                if (count == 0)
                                    htmlTable.Append("<tr>");
                                htmlTable.Append("<td style='width:100px;border: 1px solid #ccc'>" + inputs.UserInputs + "</td>");
                                count++;
                                if (count == groupKeys.Count)
                                {
                                    htmlTable.Append("</tr>");
                                    count = 0;
                                }

                            }



                        }

                        var multivalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.ParentkeyId == key1Value && w.CustomerId == customerID && w.IsEnabled == true).ToList();
                        //var multivalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.ParentkeyId != null && w.CustomerId == customerID && w.IsEnabled == true).GroupBy(w=>w.ParentkeyId).ToList();
                        //to check keys total count to make no of rows
                        var chknull = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.CustomerId == customerID && w.IsEnabled == true && w.ParentkeyId != null).FirstOrDefault();
                        int countkeystotal = 0;
                        if (chknull != null)
                        {
                            var countkey = db.TemplateDynamicFormValues.
                              Where(w => w.TemplateId == id && w.CustomerId == customerID && w.IsEnabled == true && w.ParentkeyId != null).
                              GroupBy(w => w.ParentkeyId).OrderByDescending(t => t.Count()).FirstOrDefault();
                            countkeystotal = countkey.Count();
                            if (multivalues != null && multivalues.Count() > 0)
                            {
                                List<PreviewRowID> Rowids = new List<PreviewRowID>();
                                int f = 2;
                                int j = 1;
                                // foreach (var m in multivalues)
                                for (int cnt = 0; cnt <= countkeystotal - 1; cnt++)
                                {
                                    htmlTable.Append("<tr>");
                                    foreach (var p in groupKeys)
                                    {
                                        List<PreviewKeyValue> keyvalues = new List<PreviewKeyValue>();
                                        var multiKeyvalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.ParentkeyId == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).ToList();

                                        for (int i = 0; i <= multiKeyvalues.Count - 1; i++)
                                        {

                                            if (keyvalues.Count > 0)
                                            {

                                                if (Rowids.All(x => x.RowId.ToString() != multiKeyvalues[i].RowId.ToString()))
                                                {
                                                    if (keyvalues.All(x => x.ToString() != multiKeyvalues[i].TemplateKey))
                                                    {
                                                        keyvalues.Add(new PreviewKeyValue { TemplateKey = multiKeyvalues[i].TemplateKey, RowId = multiKeyvalues[i].RowId });
                                                        //  keyvalues.Add(.ToString());
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                if (Rowids.All(x => x.RowId.ToString() != multiKeyvalues[i].RowId.ToString()))
                                                {
                                                    //htmlTable = htmlTable.Append("<tr>");

                                                    keyvalues.Add(new PreviewKeyValue { TemplateKey = multiKeyvalues[i].TemplateKey, RowId = multiKeyvalues[i].RowId });
                                                }
                                            }
                                        }
                                        if (keyvalues.Count == 0)
                                        {
                                            htmlTable.Append("<td style='width:100px;border: 1px solid #ccc'>" + "" + "</td>");
                                        }
                                        else
                                        {
                                            foreach (var t in keyvalues)
                                            {
                                                Rowids.Add(new PreviewRowID { RowId = t.RowId });
                                                var uservalue = db.TemplateDynamicFormValues.Where(x => x.CustomerId == customerID && x.IsEnabled == true && x.TemplateKey == t.TemplateKey).FirstOrDefault();
                                                // statement = statement + " " + uservalue.UserInputs;
                                                htmlTable.Append("<td style='width:100px;border: 1px solid #ccc'>" + uservalue.UserInputs + "</td>");
                                                break;
                                            }
                                        }
                                        //htmlTable.Append("</tr>");
                                        //keyvalues.Remove(t);

                                    }
                                    htmlTable.Append("</tr>");
                                    // if (isNumeric ? j - 1 != multivalues.Count() : j != multivalues.Count())
                                    if (isNumeric ? j - 1 != cnt : j != cnt)
                                        //htmlTable = htmlTable.Append(+ groupName.ToString() + "_" + f + ">");
                                        j++;
                                }
                                htmlTable.Append("</table>");
                                wordContent = wordContent.Replace("&lt;" + groupName + "&gt;", "<i>" + htmlTable.ToString() + "</i>");

                            }
                        }
                        else
                        {
                            // htmlTable.Append("<tr>");
                            //..statement = alpha[0] + ". " + statement;
                            htmlTable.Append("</table>");
                            wordContent = wordContent.Replace("&lt;" + groupName + "&gt;", "<i>" + htmlTable.ToString() + "</i>");
                        }




                    }
                    //cloning
                    //for cloning table
                    StringBuilder htmlTablecloning = new StringBuilder();
                    htmlTablecloning.Append("<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'>");

                    var objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true && b.ParentkeyId == null && b.CustomerId == customerID).ToList().OrderByDescending(x => x.RowId);
                    var objDynamicFormsingle = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true && b.ParentkeyId == null && b.CustomerId == customerID).FirstOrDefault();
                    htmlTablecloning.Append("<tr>");
                    foreach (var temp in objDynamicForm)
                    {
                        //for cloned templatekey
                        var keyID = db.TemplateKeywords.Where(k => k.TemplateKeyValue == temp.TemplateKey && k.Cloned != true).FirstOrDefault();
                        if (keyID != null)
                        {
                            var duplicateKeys = db.TemplateKeywords.Where(k => k.Cloned == true && k.ClonedFrom == keyID.TemplateKeyId).ToList();
                            if (duplicateKeys.Count != 0)
                            {
                                foreach (var cloned in duplicateKeys)
                                {
                                    htmlTablecloning.Append("<td style='width:100px;border: 1px solid #ccc'>" + temp.UserInputs + "</td>");
                                }
                            }
                        }
                    }
                    htmlTablecloning.Append("<tr>");
                    htmlTablecloning.Append("</table>");
                    wordContent = wordContent.Replace(objDynamicFormsingle.TemplateKey, "<i>" + htmlTablecloning.ToString() + "</i>");
                    wordContent = wordContent.Replace(objDynamicFormsingle.TemplateKey, string.Empty);

                    //end cloning
                }

                //logic for Statement


                else if (keys != null && keys.Count > 0)
                {
                    var groups = keys.GroupBy(x => x.Group)

                    .Select(g => g.First());

                    string groupName = string.Empty;
                    foreach (var g in groups)
                    {
                        string statement = string.Empty;
                        Roman r = new Roman();
                        bool isNumeric = false;
                        bool isRoman = false;
                        string roman = "I";
                        int startsFrom = 0;
                        var groupAutoNo = db.AssociatedKeyGroups.Where(gp => gp.GroupName == g.Group && gp.AutoNumberStartsFrom != null).FirstOrDefault();
                        if (groupAutoNo != null)
                        {
                            if (groupAutoNo.AutoNumberStartsFrom != null)
                            {
                                var chkRoman = groupAutoNo.AutoNumberStartsFrom;
                                if (chkRoman == "#R")
                                    isRoman = true;
                                else
                                {
                                    isNumeric = int.TryParse(groupAutoNo.AutoNumberStartsFrom, out startsFrom);
                                    if (!isNumeric)
                                    {
                                        string alphabets = "abcdefghijklmnopqrstuvwxyz";
                                        string ext = "";
                                        if (alphabets.Contains(groupAutoNo.AutoNumberStartsFrom))
                                        {
                                            ext = alphabets.Substring(alphabets.IndexOf(groupAutoNo.AutoNumberStartsFrom));//, alphabets.Length - 1);
                                        }
                                        else
                                        {
                                            alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                                            ext = alphabets.Substring(alphabets.IndexOf(groupAutoNo.AutoNumberStartsFrom));//, alphabets.Length - 1);
                                        }
                                        alpha = ext.ToCharArray();
                                    }
                                }
                            }
                        }
                        else
                        {
                            isNumeric = false;
                            alpha = "abcdefghijklmnopqrstuvwxyz".ToArray();
                        }

                        groupName = g.keyValue;
                        var groupKeys = keys.Where(k => k.Group == g.Group).OrderBy(o => o.order).ToList();
                        int key1 = groupKeys.FirstOrDefault().keyID;
                        string key1Value = groupKeys.FirstOrDefault().keyValue;
                        foreach (var p in groupKeys)
                        {
                            var inputs = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.TemplateKey == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).FirstOrDefault();
                            if (inputs != null)
                                statement = statement + " " + inputs.UserInputs;

                            //for cloning
                            var objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true && b.ParentkeyId == null && b.CustomerId == customerID).ToList().OrderByDescending(x => x.RowId);
                            foreach (var temp in objDynamicForm)
                            {
                                //for uncloned templatekey
                                var keyID = db.TemplateKeywords.Where(k => k.TemplateKeyValue == temp.TemplateKey && k.Cloned != true).FirstOrDefault();
                                if (keyID != null)
                                {
                                    var duplicateKeys = db.TemplateKeywords.Where(k => k.Cloned == true && k.ClonedFrom == keyID.TemplateKeyId).ToList();
                                    if (duplicateKeys.Count != 0)
                                    {
                                        foreach (var cloned in duplicateKeys)
                                        {
                                            //wordContent = wordContent.Replace("&lt;" + cloned.TemplateKey + "&gt;", "<i>" + inputs.UserInputs + "</i>");
                                            wordContent = wordContent.Replace("&lt;" + cloned.TemplateKeyValue + "&gt;", "<i>" + temp.UserInputs + "</i>");
                                        }
                                    }
                                }
                            }
                        }

                        var multivalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.CustomerId == customerID && w.IsEnabled == true).ToList();
                        //to check keys total count to make no of rows
                        var chknull = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.CustomerId == customerID && w.IsEnabled == true && w.ParentkeyId != null).FirstOrDefault();
                        int countkeystotal = 0;
                        if (chknull != null)
                        {
                            {
                                var keyvalues = groupKeys.Select(x => x.keyValue);
                                var countkey = db.TemplateDynamicFormValues.
                              Where(w => w.TemplateId == id && w.CustomerId == customerID && keyvalues.Contains(w.ParentkeyId) && w.IsEnabled == true && w.ParentkeyId != null)
                              .GroupBy(w => w.ParentkeyId).OrderByDescending(t => t.Count()).FirstOrDefault();
                                countkeystotal = countkey.Count();
                            }
                            if (multivalues != null && multivalues.Count() > 0)
                                if (isNumeric)
                                    statement = "<p dir='ltr' class='css-000004' style='margin: 0 0 0px;position:relative;font-family:\"Times New Roman\", \"serif\";font-size: 12pt;margin-left: 0.18in;text-indent: -0.18in;'><span class='css-000005'> " + startsFrom + ".</span>  <span class='css-DefaultParagraphFont-000000'><i class='dynamic-form-clause'>" + statement + "</i> </span></p><" + groupName + "_1>";
                                else if (isRoman)

                                {
                                    statement = "<p dir='ltr' class='css-000004' style='margin: 0 0 0px;position:relative;font-family:\"Times New Roman\", \"serif\";font-size: 12pt;margin-left: 0.18in;text-indent: -0.18in;'><span class='css-000005'> " + r.ToRomanNumber(1) + ".</span>  <span class='css-DefaultParagraphFont-000000'><i class='dynamic-form-clause'>" + statement + "</i> </span></p><" + groupName + "_1>";
                                }
                                else
                                    statement = "<p dir='ltr' class='css-000004' style='margin: 0 0 0px;position:relative;font-family:\"Times New Roman\", \"serif\";font-size: 12pt;margin-left: 0.18in;text-indent: -0.18in;'><span class='css-000005'> " + alpha[0] + ".</span>  <span class='css-DefaultParagraphFont-000000'><i class='dynamic-form-clause'>" + statement + "</i> </span></p><" + groupName + "_1>";

                            wordContent = wordContent.Replace("&lt;" + groupName + "&gt;", "" + statement + "");

                            //SearchAndReplace(groupName, statement, filepath);
                            List<PreviewRowID> Rowids = new List<PreviewRowID>();
                            groupName = groupName + "_1";
                            int f = 2;
                            int j = 1;
                            if (isNumeric)

                                j = startsFrom + 1;
                            else if (isRoman)
                            {
                                j = 2;
                            }
                            else
                                j = 1;
                            //foreach (var m in multivalues)
                            for (int cn = 0; cn <= countkeystotal - 1; cn++)
                            {
                                if (isNumeric)
                                {
                                    statement = "<p dir='ltr' class='css-000004' style='margin: 0 0 0px;position:relative;font-family:\"Times New Roman\", \"serif\";font-size: 12pt;margin-left: 0.18in;text-indent: -0.18in;'><span class='css-000005'> " + j + ".</span> ";
                                }
                                else if (isRoman)
                                {
                                    statement = "<p dir='ltr' class='css-000004' style='margin: 0 0 0px;position:relative;font-family:\"Times New Roman\", \"serif\";font-size: 12pt;margin-left: 0.18in;text-indent: -0.18in;'><span class='css-000005'> " + r.ToRomanNumber(j) + ".</span> ";
                                }
                                else
                                    statement = "<p dir='ltr' class='css-000004' style='margin: 0 0 0px;position:relative;font-family:\"Times New Roman\", \"serif\";font-size: 12pt;margin-left: 0.18in;text-indent: -0.18in;'><span class='css-000005'> " + alpha[j] + ".</span> ";

                                foreach (var p in groupKeys)
                                {
                                    List<PreviewKeyValue> keyvalues = new List<PreviewKeyValue>();
                                    var multiKeyvalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.ParentkeyId == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).ToList();

                                    for (int i = 0; i <= multiKeyvalues.Count - 1; i++)
                                    {

                                        if (keyvalues.Count > 0)
                                        {

                                            if (Rowids.All(x => x.RowId.ToString() != multiKeyvalues[i].RowId.ToString()))
                                            {
                                                if (keyvalues.All(x => x.ToString() != multiKeyvalues[i].TemplateKey))
                                                {
                                                    keyvalues.Add(new PreviewKeyValue { TemplateKey = multiKeyvalues[i].TemplateKey, RowId = multiKeyvalues[i].RowId });
                                                    //  keyvalues.Add(.ToString());
                                                }
                                            }

                                        }
                                        else
                                        {
                                            if (Rowids.All(x => x.RowId.ToString() != multiKeyvalues[i].RowId.ToString()))
                                            {
                                                keyvalues.Add(new PreviewKeyValue { TemplateKey = multiKeyvalues[i].TemplateKey, RowId = multiKeyvalues[i].RowId });
                                            }
                                        }
                                    }
                                    foreach (var t in keyvalues)
                                    {
                                        Rowids.Add(new PreviewRowID { RowId = t.RowId });
                                        var uservalue = db.TemplateDynamicFormValues.Where(x => x.CustomerId == customerID && x.IsEnabled == true && x.TemplateKey == t.TemplateKey).FirstOrDefault();

                                        statement = statement + " <span class='css-DefaultParagraphFont-000000'><i class='dynamic-form-clause'>" + uservalue.UserInputs + "</i></span></p>";
                                        //keyvalues.Remove(t);
                                        break;
                                    }
                                }
                                //if (isNumeric ? j - 1 != multivalues.Count() : j != multivalues.Count())
                                if (isNumeric ? j - 1 != cn : j != cn)
                                    statement = statement + "  <" + groupName + "_" + f + ">";
                                wordContent = wordContent.Replace("<" + groupName + ">", statement);
                                groupName = groupName + "_" + f;
                                j++;
                                //AsciicharforAutoNumber++;
                            }
                            //code for replace uncloned template
                            var keyvalues1 = groupKeys.Select(x => x.keyValue);
                            var UnclonedTemplate = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && !keyvalues1.Contains(b.ParentkeyId) && b.UserId == userId && b.IsEnabled == true && b.ParentkeyId == null && b.CustomerId == customerID && !keyvalues1.Contains(b.TemplateKey)).ToList().OrderByDescending(x => x.RowId);
                            int resetfirstkey = 0;
                            foreach (var temp in UnclonedTemplate)
                            {
                                //var keyID = db.TemplateKeywords.Where(k => k.TemplateKeyValue == temp.TemplateKey && k.Cloned != true).ToList();
                                List<PreviewKeyValue> keyvalues = new List<PreviewKeyValue>();
                                var inputsofParent = db.TemplateDynamicFormValues.Where(w => w.ParentkeyId == null && w.TemplateId == temp.TemplateId && w.CustomerId == customerID && w.IsEnabled == true).ToList();
                                //  wordContent = source.Replace("<" + oldValue + ">", "<i>" + oldValue + "</i>");
                                if (resetfirstkey == 0)
                                {
                                    //wordContent = wordContent.ReplacewithIndex(temp.TemplateKey, temp.UserInputs);
                                    //resetfirstkey = 0; 
                                    resetfirstkey = 1;
                                }

                                var inputs = db.TemplateDynamicFormValues.Where(w => w.ParentkeyId == temp.TemplateKey && w.TemplateId == temp.TemplateId && w.CustomerId == customerID && w.IsEnabled == true).ToList();
                                var keyID = db.TemplateKeywords.Where(k => k.TemplateKeyValue == temp.TemplateKey && k.Cloned != true).FirstOrDefault();
                                if (keyID != null)
                                {
                                    for (int u = 0; u < inputs.Count; u++)
                                    {
                                        wordContent = wordContent.ReplacewithIndex(temp.TemplateKey, inputs[u].UserInputs);
                                    }
                                }
                                //to remove <template> which having no input
                                break;
                            }
                            foreach (var tempkey in UnclonedTemplate)
                            {
                                wordContent = wordContent.ReplacewithIndex(tempkey.TemplateKey, string.Empty);
                            }
                            //end uncloned
                            groupName = g.Group;
                        }
                        else
                        {

                            statement = "<p dir='ltr' class='css-000004' style='margin: 0 0 0px;position:relative;font-family:\"Times New Roman\", \"serif\";font-size: 12pt;margin-left: 0.18in;text-indent: -0.18in;'><span class='css-000005'> " + alpha[0] + ".</span>  <span class='css-DefaultParagraphFont-000000'><i class='dynamic-form-clause'>" + statement + "</i> </span></p>";

                            wordContent = wordContent.Replace("&lt;" + groupName + "&gt;", "<i>" + statement + "</i>");
                        }

                    }
                }
                //code for not groupassociated template

                else

                {
                    var objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true && b.ParentkeyId == null && b.CustomerId == customerID).ToList().OrderByDescending(x => x.RowId);

                    var maxlength = db.GetMaxlengthOfUserInputs(customerID, id).ToList();

                    var statementKeys = db.AssociatedKeyGroups.Where(t => t.TemplateID == id && t.DesignType == "Statement").Select(k => k.KeyID).ToList();

                    //char[] alpha = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

                    foreach (var temp in objDynamicForm)
                    {
                        var inputs = db.TemplateDynamicFormValues.Where(w => w.ParentkeyId == temp.TemplateKey && w.TemplateId == temp.TemplateId && w.CustomerId == customerID && w.IsEnabled == true).ToList();

                        var keyID = db.TemplateKeywords.Where(k => k.TemplateKeyValue == temp.TemplateKey && k.Cloned != true).FirstOrDefault();
                        if (keyID != null)
                        {
                            if (!statementKeys.Contains(keyID.TemplateKeyId))
                            {
                                if (inputs != null && inputs.Count > 0)
                                {
                                    bool serialNumber = false;
                                    int p = 1;
                                    // temp.UserInputs = alpha[0] + ". " + temp.UserInputs;
                                    var hasAssociatedGroup = (from a in db.AssociatedKeyGroups
                                                              join k in db.TemplateKeywords on a.KeyID equals k.TemplateKeyId
                                                              where k.TemplateKeyValue == temp.TemplateKey && a.TemplateID == temp.TemplateId
                                                              select new { groupName = a.GroupName, firstColumn = a.FirstColumn }).ToList();
                                    if (hasAssociatedGroup != null && hasAssociatedGroup.Count() > 0)
                                    {
                                        string groupName = hasAssociatedGroup.FirstOrDefault().groupName;


                                        for (int u = 0; u < inputs.Count; u++)
                                        {
                                            if (hasAssociatedGroup.Count() > 0)
                                            {
                                                if (hasAssociatedGroup.FirstOrDefault().firstColumn == "First")
                                                {
                                                    var isFirstkey = db.AssociatedKeyGroups.Where(k => k.KeyID == keyID.TemplateKeyId && k.TemplateID == temp.TemplateId && k.KeyOrder == 1).FirstOrDefault();
                                                    if (isFirstkey != null)
                                                    {
                                                        serialNumber = true;
                                                        if (p == 1)
                                                            temp.UserInputs = alpha[0] + ". " + temp.UserInputs;
                                                    }

                                                }
                                                else
                                                {
                                                    serialNumber = true;
                                                    if (p == 1)
                                                        temp.UserInputs = alpha[0] + ". " + temp.UserInputs;
                                                }
                                                var length = maxlength.Where(g => g.groupname == groupName).FirstOrDefault().lettercount;
                                                int maximumlength = length != null ? Convert.ToInt32(length) : 0;
                                                //long length = maxlength.FirstOrDefault().Value;

                                                if (p == 1 && temp.UserInputs.Length < length)
                                                {
                                                    StringBuilder appendText1 = new StringBuilder();
                                                    appendText1.Append("".PadLeft((maximumlength - temp.UserInputs.Length), ' ').Replace(" ", " "));
                                                    temp.UserInputs = temp.UserInputs + " " + appendText1;
                                                }
                                                if (inputs[u].UserInputs.Length < length)
                                                {
                                                    Int32 tobeadded = maximumlength - inputs[u].UserInputs.Length;
                                                    StringBuilder appendText = new StringBuilder();
                                                    appendText.Append("".PadRight(tobeadded, ' ').Replace(" ", " "));
                                                    if (serialNumber)
                                                        temp.UserInputs = temp.UserInputs + "<br/> " + alpha[p] + ". " + inputs[u].UserInputs + "  " + appendText;
                                                    //.PadRight(Convert.ToInt32(tobeadded + 2)) + "\t";
                                                    else
                                                        temp.UserInputs = temp.UserInputs + "<br/> " + inputs[u].UserInputs + "  " + appendText;
                                                }
                                                else
                                                {
                                                    if (serialNumber)
                                                        temp.UserInputs = temp.UserInputs + "<br/> " + alpha[p] + ". " + inputs[u].UserInputs;
                                                    else
                                                        temp.UserInputs = temp.UserInputs + "<br/> " + inputs[u].UserInputs;
                                                }
                                            }
                                            else
                                            {
                                                if (p == 1)
                                                    temp.UserInputs = alpha[0] + ". " + temp.UserInputs;
                                                temp.UserInputs = temp.UserInputs + "<br/> " + alpha[p] + ". " + inputs[u].UserInputs;
                                            }
                                            p++;
                                        }
                                    }
                                    else
                                    {
                                        int q = 1;
                                        for (int u = 0; u < inputs.Count; u++)
                                        {

                                            temp.UserInputs = temp.UserInputs + "<br/> " + alpha[q] + ". " + inputs[u].UserInputs;
                                            q++;
                                        }
                                    }
                                }
                                wordContent = wordContent.Replace("&lt;" + temp.TemplateKey + "&gt;", "<i>" + temp.UserInputs + "</i>");
                                // wordContent = wordContent.Replace("\n", "<br></br>");
                            }
                            //changed by vai
                            //var duplicateKeys = db.TemplateKeywords.Where(k => k.Cloned == true && k.ClonedFrom == keyID.TemplateKeyId).ToList();

                            //foreach (var cloned in duplicateKeys)
                            //{
                            //    wordContent = wordContent.Replace("&lt;" + cloned.TemplateKeyValue + "&gt;", "<i>" + temp.UserInputs + "</i>");
                            //}
                            // end changed by vaishali
                            //wordContent = wordContent.Replace("&lt;" + temp.TemplateKey + "&gt;", "<i>" + temp.UserInputs + "</i>");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            ViewBag.WordContent = wordContent;
            ViewBag.ClientID = Session["ServiceID"];
            ViewBag.customerID = customerID;
            TempData.Keep();
            return View(clauselist.ToList());
        }

        public ActionResult PrevDocument(int? id)
        {
            string newFileName = string.Empty;
            string filePath = string.Empty;
            int customerID = 0;
            DocumentTemplate template =
            db.DocumentTemplates.Where(c => c.TemplateId == id).FirstOrDefault();

            if (id == null || id == 0 || template == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.WordContent = "";
            ViewBag.TemplateId = id;
            Session["CurrentTempAId"] = id;
            Session["CurrentEditId"] = id;
            if (template != null)
            {
                ViewBag.Title = "Preview Filled Form  " + template.DocumentTitle;
                if (template.AssociateTemplateId != null)
                {
                    ViewBag.NxtBtnValue = "Next";
                    ViewBag.AssociateId = template.AssociateTemplateId;
                }
                ViewBag.TemplateId = id;
                int userId = Convert.ToInt32(Session["UserId"]);

                if (Session["customerId"] != null)
                {
                    customerID = Convert.ToInt32(Session["customerId"]);
                }
                try
                {
                    CustomerDetail customer =
                    db.CustomerDetails.Where(s => s.CustomerId == customerID).FromCacheFirstOrDefault();
                    if (customer != null)
                    {
                        newFileName = customer.CustomerName + "-" + template.DocumentTitle.Replace(" ", "") + "." + template.TemplateFileName.Split('.')[1];
                        filePath = Path.Combine(Server.MapPath("~/FilledTemplateFiles/" + newFileName));
                        var TemplateFilePath = Path.Combine(Server.MapPath("~/TemplateFiles/"),
                            template.TemplateFileName);
                        if (System.IO.File.Exists(TemplateFilePath))
                        {
                            System.IO.File.Copy(TemplateFilePath, filePath, true);
                        }
                        if (System.IO.File.Exists(filePath))
                        {
                            Document pdfDocument = new Document();
                            //Document htmlDocument = new Document();
                            pdfDocument.
                            LoadFromFile(filePath);
                            //htmlDocument.
                            //LoadFromFile(filePath);
                            List<TemplateDynamicFormValue> lst
                            = db.TemplateDynamicFormValues.Where
                            (
                                t => t.TemplateId == template.TemplateId &&
                                    t.CustomerId == customerID && t.IsEnabled
                            ).ToList();

                            List<TemplateDynamicFormValue> Pololst =
                            new List<TemplateDynamicFormValue>();

                            foreach (TemplateDynamicFormValue templateDynamicFormValue in lst)
                            {
                                if (string.Compare(templateDynamicFormValue.TemplateKey, "polo", true) == 0 ||
                                    string.Compare((templateDynamicFormValue.ParentkeyId ?? "").ToString(), "polo", true) == 0)
                                {
                                    if ((templateDynamicFormValue.UserInputs ?? "").Trim().Length > 0)
                                    {
                                        var cnt = Pololst
                                                  .Where(c =>
                                                  string.Compare(c.TemplateKey,
                                                  templateDynamicFormValue.TemplateKey, true) == 0).Count();
                                        if (cnt == 0)
                                            Pololst.Add(templateDynamicFormValue);
                                    }
                                }
                                else
                                {
                                    pdfDocument.Replace("<" + templateDynamicFormValue.TemplateKey + ">",
                                    templateDynamicFormValue.UserInputs, false, false);

                                    //htmlDocument.Replace("<" + templateDynamicFormValue.TemplateKey + ">",
                                    //templateDynamicFormValue.UserInputs, false, false);
                                }
                            }
                            if (Pololst.Count > 0)
                            {
                                Pololst = Pololst.OrderBy(p => p.ParentkeyId).ThenBy(p => p.RowId).ToList();
                                Section section = pdfDocument.Sections[0];
                                Paragraph poloParagraph = new Paragraph(pdfDocument);
                                Paragraph previousNotNullParagraph = new Paragraph(pdfDocument);
                                int i = 0;
                                foreach (Paragraph paragraph in section.Paragraphs)
                                {
                                    if (new string[] { "<polo>", "<addclause>" }.Contains(paragraph.Text.ToLower()))
                                    {
                                        int previousSiblingCharCount = 0;
                                        previousNotNullParagraph = (Paragraph)(paragraph.PreviousSibling);
                                        previousSiblingCharCount = ((Paragraph)(previousNotNullParagraph)).CharCount;

                                        while (previousSiblingCharCount == 0 && i <= 100)
                                        {
                                            if (previousNotNullParagraph.PreviousSibling != null)
                                                previousNotNullParagraph = (Paragraph)previousNotNullParagraph.PreviousSibling;
                                            previousSiblingCharCount = ((Paragraph)(previousNotNullParagraph)).CharCount;
                                            i++;
                                        }
                                        poloParagraph = (Paragraph)(previousNotNullParagraph.Clone());
                                        break;
                                    }
                                }

                                IList<Paragraph> replacement = new List<Paragraph>();
                                foreach (TemplateDynamicFormValue templateDynamicFormValue in Pololst)
                                {
                                    Paragraph pololistParagraph = (Paragraph)(poloParagraph).Clone();
                                    pololistParagraph.Text = templateDynamicFormValue.UserInputs.Replace("\r\n", "");
                                    //if (i > 0)
                                    //    pololistParagraph.Format.BeforeSpacing =
                                    //    pololistParagraph.Format.AfterSpacing = 15;
                                    replacement.Add(pololistParagraph);
                                }

                                TextSelection[] selections = pdfDocument.FindAllString("<polo>", false, true);
                                if (selections == null)
                                    selections = pdfDocument.FindAllString("<addclause>", false, true);
                                if (selections != null)
                                {
                                    List<TextRangeLocation> locations = new List<TextRangeLocation>();
                                    foreach (TextSelection selection in selections)
                                    {
                                        locations.Add(new TextRangeLocation(selection.GetAsOneRange()));
                                    }
                                    locations.Sort();
                                    foreach (TextRangeLocation location in locations)
                                    {
                                        ReplaceSpireDocText(location, replacement);
                                    }
                                }
                                //ParagraphStyle ItalicStyle = new ParagraphStyle(htmlDocument);
                                //ItalicStyle.Name = "ItalicStyle1";
                                //ItalicStyle.CharacterFormat.Italic = true;
                                //htmlDocument.Styles.Add(ItalicStyle);

                                //IList<Paragraph> replacement1 = new List<Paragraph>();
                                //foreach (TemplateDynamicFormValue templateDynamicFormValue in Pololst)
                                //{
                                //    Paragraph pololistParagraph1 = (Paragraph)(poloParagraph).Clone();
                                //    pololistParagraph1.Text = templateDynamicFormValue.UserInputs.Replace("\r\n", "");
                                //    pololistParagraph1.ApplyStyle(ItalicStyle.Name);
                                //    replacement1.Add(pololistParagraph1);
                                //}

                                //TextSelection[] selections1 = htmlDocument.FindAllString("<polo>", false, true);
                                //List<TextRangeLocation> locations1 = new List<TextRangeLocation>();
                                //foreach (TextSelection selection in selections1)
                                //{
                                //    locations1.Add(new TextRangeLocation(selection.GetAsOneRange()));
                                //}
                                //locations1.Sort();
                                //foreach (TextRangeLocation location in locations1)
                                //{
                                //    ReplaceSpireDocText(location, replacement1);
                                //}

                            }
                            pdfDocument.SaveToFile(filePath.Replace(".docx", ".html"), FileFormat.Html);
                            pdfDocument.SaveToFile(filePath.Replace(".docx", ".pdf"), FileFormat.PDF);
                            ViewBag.WordContent = filePath.Replace(".docx", ".html");
                        }
                    }
                    ViewBag.customerID = customerID;
                    if (Session["ServiceID"] != null)
                        ViewBag.ClientID = Session["ServiceID"];
                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                }
            }
            //ViewBag.customerID = customerID;
            //TempData.Keep();
            return View();
        }

        public ActionResult SearchTemplate(int? id)
        {
            Session.Remove("ATCount");
            Session.Remove("TemplateId");
            Session.Remove("Displayorder");
            int userId = Convert.ToInt32(Session["UserId"]);
            Session["AssociateCount"] = 0;



            if (Session["ScategoryID"] != null && !string.IsNullOrEmpty(Session["ScategoryID"].ToString()))
            { }


            DocumentTemplateListModel objTempList = new DocumentTemplateListModel();
            try
            {
                //var objServiceId = (from SAS in db.SelectedAccountServices.Where(m => m.UserId == userId)
                //                    select SAS.ServiceId
                //              );

                var objTemplates = (from ut in db.DocumentTemplates
                                    join dc in db.DocumentCategories on ut.DocumentCategory equals dc.DocumentCategoryId
                                    where ut.IsEnabled == true && ut.IsEnabled == true

                                    select new DocumentTemplateListModel { TemplateName = ut.DocumentTitle, TemplateId = ut.TemplateId, DocumentFileName = ut.TemplateFileName, DocumentCategory = dc.DocumentCategoryName, Cost = ut.TemplateCost, AssociatedDocumentId = ut.AssociateTemplateId, AssociatedDocument = null, ServiceId = dc.ServiceId, DocumentSubCategoryId = ut.DocumentSubCategory, DocumentSubSubCategoryId = ut.DocumentSubSubCategory, DocumentSubCategoryName = null, DocumentSubSubCategoryName = null }
                        );
                //var objFilteredTemplate = (from FilTem in objTemplates
                //                           where objServiceId.Contains(FilTem.ServiceId)
                //                           select FilTem);
                var query = objTemplates.Select(p => new DocumentTemplateListModel
                {
                    TemplateName = p.TemplateName,
                    TemplateId = p.TemplateId,
                    DocumentFileName = p.DocumentFileName,
                    DocumentCategory = p.DocumentCategory,
                    Cost = p.Cost,
                    AssociatedDocumentId = p.AssociatedDocumentId,
                    AssociatedDocument = "", //(from utt in db.DocumentTemplates where utt.TemplateId == p.AssociatedDocumentId select utt.DocumentTitle).FirstOrDefault(),
                    ServiceId = p.ServiceId,
                    DocumentSubCategoryId = p.DocumentSubCategoryId,
                    DocumentSubSubCategoryId = p.DocumentSubSubCategoryId,
                    DocumentSubCategoryName = (from dsc in db.DocumentSubCategories where dsc.DocumentSubCategoryId == p.DocumentSubCategoryId select dsc.DocumentSubCategoryName).FirstOrDefault(),
                    DocumentSubSubCategoryName = (from dssc in db.DocumentSubSubCategories where dssc.DocumentSubSubCategoryId == p.DocumentSubSubCategoryId select dssc.SubDocumentCategoryName).FirstOrDefault()

                });

                return View(query);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objTempList);
        }

        //public string getWordContent(string filename)
        //{
        //    string totaltext = "";
        //    try
        //    {
        //        object path = Path.Combine(Server.MapPath("~/TemplateFiles/" + filename));
        //        Application word = new Application();
        //        object miss = System.Reflection.Missing.Value;
        //        object readOnly = true;
        //        Document docs = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss);

        //        try
        //        {
        //            for (int i = 0; i < docs.Paragraphs.Count; i++)
        //            {
        //                totaltext += " \r\n " + docs.Paragraphs[i + 1].Range.Text.ToString() +"<div id = assigned_attributes class=sortable></div>";
        //            }
        //           // totaltext = totaltext.Replace("</p>", "</p>);
        //        }
        //        catch (Exception ex)
        //        {
        //            ErrorLog.LogThisError(ex);
        //            docs.Close();
        //            word.Quit();
        //        }
        //        docs.Close();
        //        word.Quit();
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }


        //    return totaltext;
        //}
        public string getWordContent(string filename)
        {
            string path1 = Path.Combine(Server.MapPath("~/TemplateFiles/" + filename));

            byte[] byteArray = System.IO.File.ReadAllBytes(path1);
            int imageCounter = 0;


            // string html = WordToHTMLSautin(path1);

            FileInfo fileInfo = new FileInfo(path1);
            string imageDirectoryName = path1 + "_files";
            DirectoryInfo localDirInfo = new DirectoryInfo(path1 + "_files");
            if (!localDirInfo.Exists)
            {
                localDirInfo.Create();
            }


            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArray, 0, byteArray.Length);
                using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                {
                    HtmlConverterSettings convSettings = new HtmlConverterSettings()
                    {

                        FabricateCssClasses = true,
                        CssClassPrefix = "css-",
                        RestrictToSupportedLanguages = false,
                        RestrictToSupportedNumberingFormats = false,
                        ImageHandler = imageInfo =>
                        {
                            //DirectoryInfo localDirInfo = new DirectoryInfo(imageDirectoryName);
                            //if (!localDirInfo.Exists)
                            //{
                            //    localDirInfo.Create();
                            //}

                            ++imageCounter;
                            string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                            ImageFormat imageFormat = null;
                            if (extension == "png")
                            {
                                extension = "jpeg";
                                imageFormat = ImageFormat.Jpeg;
                            }
                            else if (extension == "bmp")
                            {
                                imageFormat = ImageFormat.Bmp;
                            }
                            else if (extension == "jpeg")
                            {
                                imageFormat = ImageFormat.Jpeg;
                            }
                            else if (extension == "tiff")
                            {
                                imageFormat = ImageFormat.Tiff;
                            }
                            else if (extension == "x-wmf")
                            {
                                extension = "wmf";
                                imageFormat = ImageFormat.Wmf;
                            }

                            // If the image format is not one that you expect, ignore it,
                            // and do not return markup for the link.
                            if (imageFormat == null)
                            {
                                return null;
                            }

                            string imageFileName = imageDirectoryName + "/image" + imageCounter.ToString() + "." + extension;

                            try
                            {
                                imageInfo.Bitmap.Save(imageFileName, imageFormat);
                            }
                            catch (System.Runtime.InteropServices.ExternalException)
                            {
                                return null;
                            }

                            XElement img = new XElement(Xhtml.img, new XAttribute(NoNamespace.src, imageFileName),
                                imageInfo.ImgStyleAttribute, imageInfo.AltText != null ? new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                            return img;
                        }

                    };

                    XElement html = OpenXmlPowerTools.HtmlConverter.ConvertToHtml(doc, convSettings);
                    string totaltext = html.ToStringNewLineOnAttributes();
                    // string totaltext = html;
                    totaltext = totaltext.Replace("</p>", "</p><div id=assigned_attributes class=sortable></div>");
                    // totaltext = totaltext.Replace("margin-top", "margin top");
                    //totaltext = totaltext.Replace("margin-bottom:", "margin bottom");
                    totaltext = totaltext.Replace("pt-DefaultParagraphFont", " ");
                    totaltext = totaltext.Replace("span { white - space: pre - wrap; }", " ");
                    totaltext = totaltext.Replace("span { white-space: pre-wrap; }", " ");
                    totaltext = totaltext.Replace("span {", "test {");
                    totaltext = totaltext.Replace("width: 0", "r");

                    totaltext = totaltext.Replace(ConfigurationManager.AppSettings["FolderPath"].ToString(), ConfigurationManager.AppSettings["PublishName"].ToString());

                    return totaltext;

                }
            }
        }

        //public string getWordContent(string filename)
        //{
        //    string totaltext = "";
        //    try
        //    {
        //        string path1 = Path.Combine(Server.MapPath("~/TemplateFiles/" + filename));



        //        byte[] byteArray = System.IO.File.ReadAllBytes(path1);
        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            memoryStream.Write(byteArray, 0, byteArray.Length);
        //            using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
        //            {
        //                HtmlConverterSettings settings = new HtmlConverterSettings()
        //                {
        //                    // PageTitle = "My Page Title"
        //                };

        //                // HtmlConverter htmlConverter = new HtmlConverter(

        //                XElement html = OpenXmlPowerTools.HtmlConverter.ConvertToHtml(doc, settings);
        //                ////  System.IO.File.WriteAllText(path1, html.ToStringNewLineOnAttributes());

        //                totaltext = html.ToStringNewLineOnAttributes();
        //                totaltext = totaltext.Replace("</p>", "</p><div id=assigned_attributes class=sortable></div>");
        //                // totaltext = totaltext.Replace("margin-top", "margin top");
        //                //totaltext = totaltext.Replace("margin-bottom:", "margin bottom");
        //                totaltext = totaltext.Replace("pt-DefaultParagraphFont", " ");
        //                totaltext = totaltext.Replace("span { white - space: pre - wrap; }", " ");
        //                totaltext = totaltext.Replace("span { white-space: pre-wrap; }", " ");
        //                totaltext = totaltext.Replace("span {", "test {");

        //            }
        //        }



        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }


        //    return totaltext;
        //}


        public ActionResult FormsConfirmation(string html, bool clause)
        {
            string html1 = HttpUtility.UrlDecode(html, System.Text.Encoding.Default);

            //var result = Uglify.HtmlToText(html1);
            html1 = html1.Replace("min-height: 30px;", "");
            html1 = html1.Replace("min-width:", "r");
            html1 = html1.Replace("class='' style=''", "");
            int? id = null;
            //code by vaishali
            if (Session["CurrentTemplateId"] != null)
            {
                id = Convert.ToInt32(Session["CurrentTemplateId"]);
                // userId = Convert.ToInt32(Session["UserId"]);
            }
            var category = db.DocumentTemplates.Where(c => c.TemplateId == id).Select(c => c.DocumentCategory).FirstOrDefault();

            var clauses = (from c in db.ClouseandCategoryMapings
                           join cust in db.Clice on c.clouseID equals cust.Id
                           where c.categoryID == category
                           select new ClouseModel { Clouse1 = cust.Clouse1, Id = c.clouseID });
            foreach (var clousemap in clauses)
            {

                var ClauseDescription = from cd in db.Clice
                                        where cd.Id == clousemap.Id && cd.IsEnabled == true
                                        select cd.Description;
                foreach (var clousetext in ClauseDescription)
                {
                    string clouse = clousemap.Clouse1;
                    string chkk = html1.Contains(clouse).ToString();
                    if (html1.Contains(clouse))
                    {
                        html1 = html1.Replace("{{" + clousemap.Clouse1 + "}}", clousetext.ToString());
                    }
                }
            }
            //end code by vaishali
            //string htmlData = result.Code;
            string newFilename = "";
            string path = "";
            string newpath = "";
            int customerId = 0;
            int GroupId = 1;

            try
            {

                int? userId = null;


                if (Session["customerId"] != null)
                {
                    customerId = Convert.ToInt32(Session["customerId"]);
                }

                if (Session["CurrentTemplateId"] != null)
                {
                    id = Convert.ToInt32(Session["CurrentTemplateId"]);
                    userId = Convert.ToInt32(Session["UserId"]);
                }

                // Updating status for create document
                if (id != null)
                {
                    var objDocumentTemplate = db.DocumentTemplates.Find(id);
                    //  wordContent = getWordContent(objDocumentTemplate.TemplateFileName,html);
                    string customerName = db.CustomerDetails.Single(s => s.CustomerId == customerId).CustomerName;
                    List<TemplateDynamicFormValue> objDynamicForm = new List<TemplateDynamicFormValue>();
                    objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true && b.ParentkeyId == null && b.CustomerId == customerId).ToList();
                    //Replace Keyvalues from word Document

                    newFilename = customerName + "-" + objDocumentTemplate.DocumentTitle.Replace(" ", "") + "." + objDocumentTemplate.TemplateFileName.Split('.')[1];
                    //newFilename = customerName + DateTime.Now.Ticks + objDocumentTemplate.TemplateFileName.Replace(" ", ""); // Create New File with unique name
                    path = Path.Combine(Server.MapPath("~/TemplateFiles/" + objDocumentTemplate.TemplateFileName.Replace(" ", ""))); // Getting Original File For Create a new one with filled details
                    newpath = Path.Combine(Server.MapPath("~/FilledTemplateFiles/" + newFilename)); // New File Path with File Name
                                                                                                    //comment by vaishali                                                                               //System.IO.File.Copy(path, newpath);
                    if (!clause)
                    {
                        if (System.IO.File.Exists(newpath))
                        {
                            System.IO.DirectoryInfo directory = new DirectoryInfo(Path.Combine(Server.MapPath("~/FilledTemplateFiles/")));
                            var files = directory.GetFiles(customerName + "-" + objDocumentTemplate.DocumentTitle.Replace(" ", "").Split('.')[0] + "*" + ".docx");

                            newFilename = files.LastOrDefault().Name.Split('.')[0] + "-" + files.Count().ToString() + files[0].Extension;
                            newpath = Path.Combine(Server.MapPath("~/FilledTemplateFiles/" + newFilename));
                        }

                        System.IO.File.Copy(path, newpath);

                        Document document = new Document();
                        document.LoadFromFile(newpath);

                        foreach (TemplateDynamicFormValue tem in objDynamicForm)
                        {
                            string input = string.Empty;
                            //  var statementKeys = db.AssociatedKeyGroups.Where(t => t.TemplateID == templateID && t.Statement).Select(k => k.KeyID).ToList();
                            var keyID = db.TemplateKeywords.Where(k => k.TemplateKeyValue == tem.TemplateKey).First().TemplateKeyId;
                            // if (!statementKeys.Contains(keyID))
                            {
                                var inputs = db.TemplateDynamicFormValues.Where(w => w.ParentkeyId == tem.TemplateKey && w.TemplateId == tem.TemplateId && w.CustomerId == tem.CustomerId && w.IsEnabled == true).ToList();
                                input = tem.UserInputs;
                                foreach (TemplateDynamicFormValue values in inputs)
                                {
                                    if (inputs.Count > 0)
                                        //{
                                        //    var hasAssociatedGroup = (from a in db.AssociatedKeyGroups
                                        //                              join k in db.TemplateKeywords on a.KeyID equals k.TemplateKeyId

                                        //                              where k.TemplateKeyValue == tem.TemplateKey && a.TemplateID == tem.TemplateId && a.Statement == false

                                        //                              select new { groupName = a.GroupName, firstColumn = a.FirstColumn }).ToList();
                                        //    if (hasAssociatedGroup != null && hasAssociatedGroup.Count() > 0)
                                        //    {
                                        //        string groupName = hasAssociatedGroup.FirstOrDefault().groupName;
                                        //        if (hasAssociatedGroup.Count() > 0)
                                        //        {
                                        values.IsEnabled = false;
                                    //}
                                    db.SaveChanges();
                                    //  }
                                    //}
                                }

                            }

                            document.Replace("<" + tem.TemplateKey + ">", tem.UserInputs, false, false);
                        }
                        document.SaveToFile((newpath).Replace(".docx", ".html"), FileFormat.Html);
                        document.SaveToFile((newpath).Replace(".docx", ".pdf"), FileFormat.PDF);

                        //CreateDocumentFromHiQpdf(html1, newpath);
                        // DoSearchAndReplaceInWord(newpath, objDynamicForm, id.Value);// Replace process

                        //   ConvertToPdfFile(newpath); // Convert to pdf file
                    }
                    else
                    {

                        CreateDocumentFromHiQpdf(html1, newpath);
                    }
                    //Update the status for creating new word document
                    foreach (var frmList in objDynamicForm)
                    {
                        frmList.IsEnabled = false;
                    }
                    db.SaveChanges();
                    Session["newFilename"] = newFilename;
                    CreateCoverLetteronHold(newFilename);
                    var objFilledForm = db.FilledTemplateDetails.Where(c => c.UserId == userId);
                    if (Session["Displayorder"] != null && Convert.ToInt32(Session["Displayorder"]) > 0)
                    {
                        GroupId = Convert.ToInt32(Session["GroupId"]);
                    }
                    else
                    {
                        var GroupForm = objFilledForm.OrderByDescending(d => d.GroupId).FirstOrDefault();

                        // Assign Group Id
                        if (GroupForm != null)
                        {
                            GroupId = GroupForm.GroupId + 1;
                            Session["GroupId"] = GroupId;
                        }
                        if (Session["GroupId"] != null && Convert.ToInt32(Session["GroupId"]) != 0)
                        {
                            if (Convert.ToInt32(Session["AssociateCount"]) >= 1)
                            {
                                // Holding same Group Id
                                GroupId = Convert.ToInt32(Session["GroupId"]);
                            }
                        }
                    }

                    using (var context = new VirtualAdvocateEntities())
                    {
                        using (var dbContextTransaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                // Insert Filled Form Details
                                FilledTemplateDetail objFilledTemp = new FilledTemplateDetail();
                                objFilledTemp.GroupId = GroupId;
                                objFilledTemp.PaidStatus = false;
                                objFilledTemp.UserId = userId.Value;
                                objFilledTemp.TemplateId = id.Value;
                                objFilledTemp.FilledTemplateName = newFilename;
                                objFilledTemp.Amount = objDocumentTemplate.TemplateCost;
                                objFilledTemp.CreatedDate = DateTime.Now;
                                objFilledTemp.CustomerId = customerId;
                                objFilledTemp.OrgId = Convert.ToInt32(Session["OrgId"]);
                                context.FilledTemplateDetails.Add(objFilledTemp);
                                context.SaveChanges();

                                FormCollection formCollection = TempData["FormCollection"] as FormCollection;

                                if (formCollection != null && (roleId == 5 || roleId == 6))
                                {
                                    var objkeyCategory = (from c in context.KeyCategories
                                                          join k in context.TemplateKeywords on c.Id equals k.TemplateKeyCategory
                                                          join p in context.TemplateKeysPointers on k.TemplateKeyId equals p.TemplateKeyId
                                                          where p.TemplateId == id
                                                          orderby c.CategoryOrder
                                                          select new
                                                          {
                                                              c.CategoryName,
                                                              c.CategoryOrder,
                                                              c.CanAddInsurance,
                                                              c.Id
                                                          }).Distinct().OrderBy(x => x.CategoryOrder
                                                    );

                                    foreach (var item in objkeyCategory)
                                    {
                                        var property = new Property();
                                        if (item.CanAddInsurance != null && item.CanAddInsurance.Value)
                                        {
                                            var assetInsured = formCollection["InsuranceAssetInsured-" + item.Id]?.ToLower().Trim();
                                            if (!string.IsNullOrEmpty(assetInsured))
                                            {
                                                var prop = context.Properties
                                                    .Include("FilledTemplateDetail")
                                                    .Where(m => m.FilledTemplateDetail.TemplateId == objFilledTemp.TemplateId
                                                    && m.FilledTemplateDetail.CustomerId == objFilledTemp.CustomerId
                                                    && m.PropertyName.ToLower() == assetInsured && m.Status);
                                                //var prop = context.Properties.Where(m => m.DocumentId == objFilledTemp.RowId && m.PropertyName.ToLower() == assetInsured);

                                                if (prop != null && prop.Count() > 0)
                                                {
                                                    //property already added
                                                    property = prop.FirstOrDefault();
                                                }
                                                else
                                                {
                                                    property = new Property
                                                    {
                                                        CreatedDate = DateTime.Now,
                                                        PropertyName = formCollection["InsuranceAssetInsured-" + item.Id].Trim(),
                                                        Status = true,
                                                        DocumentId = objFilledTemp.RowId,
                                                    };

                                                    context.Properties.Add(property);
                                                    context.SaveChanges();
                                                }

                                                if (property.Id != 0)
                                                {
                                                    if (formCollection["InsuranceWantToAddInsurance-" + item.Id] != null && Convert.ToInt32(formCollection["InsuranceWantToAddInsurance-" + item.Id]) == 1)
                                                    {
                                                        var insuranceExists = db.Insurances.Where(m => m.PropertyId == property.Id && m.Status).Count();

                                                        if (insuranceExists > 0)
                                                        {
                                                            //insurance already added
                                                        }
                                                        else
                                                        {
                                                            var insuranceDetail = new Insurance
                                                            {
                                                                PropertyId = property.Id,
                                                                Currency = formCollection.GetValue("InsuranceCurrency-" + item.Id).AttemptedValue.Trim(),
                                                                Insurer = formCollection.GetValue("InsuranceInsurer-" + item.Id).AttemptedValue.Trim(),
                                                                AmountInsured = formCollection.GetValue("InsuranceAmountInsured-" + item.Id).AttemptedValue.Trim(),
                                                                DateOfInsurance = DateTime.ParseExact(formCollection.GetValue("InsuranceDateOfInsrurance-" + item.Id).AttemptedValue.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                                                                DateOfExpiry = DateTime.ParseExact(formCollection.GetValue("InsuranceDateOfExpiry-" + item.Id).AttemptedValue.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                                                                CreatedDate = DateTime.Now,
                                                                UserId = userId,
                                                                Status = true
                                                            };

                                                            context.Insurances.Add(insuranceDetail);
                                                            context.SaveChanges();
                                                        }
                                                    }
                                                }

                                            }

                                        }
                                    }


                                }
                            }
                            catch (Exception ex)
                            {
                                //Log, handle or absorbe I don't care ^_^
                            }

                            dbContextTransaction.Commit();
                        }
                    }

                }

                int templateId = Convert.ToInt32(Session["TemplateId"]);
                int displayOrder = 0;
                if (Session["Displayorder"] != null)
                {
                    displayOrder = Convert.ToInt32(Session["Displayorder"]);
                }

                var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == templateId && c.IsEnabled == true && (c.DisplayOrder == null || c.DisplayOrder > displayOrder)).OrderBy(c => c.DisplayOrder).FirstOrDefault();

                if (objAssociateIds != null)
                {
                    // var service = db.SelectedAccountServices.Where(s => s.UserId == userId).FirstOrDefault();
                    var catogories = db.DocumentCategories.Where(d => d.ServiceId == 0 && d.IsEnabled == true).ToArray();

                    Session["Displayorder"] = objAssociateIds.DisplayOrder;
                    displayOrder = Convert.ToInt32(objAssociateIds.DisplayOrder);
                    var lastdoc = db.AssociateTemplateDetails.Where(c => c.TemplateId == templateId && c.IsEnabled == true && (c.DisplayOrder == null || c.DisplayOrder > displayOrder)).OrderBy(c => c.DisplayOrder + 1).FirstOrDefault();

                    if (lastdoc == null)
                    {
                        ViewBag.lastdoc = "true";
                    }
                    else
                    {
                        ViewBag.lastdoc = "false";
                    }

                    if (objAssociateIds.Mandatory)
                    {
                        Session["ExtraFiles"] = objAssociateIds.AssociateTemplateId;
                        return Json("CreateDynamicForm", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Session["ExtraFiles"] = objAssociateIds.AssociateTemplateId;
                        return Json("CoverLetterConfirm", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Session.Remove("Displayorder");
                    Session["AssociateCount"] = 0;
                    Session["GroupId"] = 0;
                    Session["customerId"] = null;
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                //return Json(true, JsonRequestBehavior.AllowGet);
                return Json(ex.StackTrace + "\n" + ex.InnerException, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult FormsConfirmation1(string html, bool clause)
        {
            //string html1 = HttpUtility.UrlDecode(html, System.Text.Encoding.Default);

            ////var result = Uglify.HtmlToText(html1);
            //html1 = html1.Replace("min-height: 30px;", "");
            //html1 = html1.Replace("min-width:", "r");
            //html1 = html1.Replace("class='' style=''", "");
            int? id = null;
            //code by vaishali
            if (Session["CurrentTemplateId"] != null)
            {
                id = Convert.ToInt32(Session["CurrentTemplateId"]);
                // userId = Convert.ToInt32(Session["UserId"]);
            }
            var category = db.DocumentTemplates.Where(c => c.TemplateId == id).Select(c => c.DocumentCategory).FirstOrDefault();

            var clauses = (from c in db.ClouseandCategoryMapings
                           join cust in db.Clice on c.clouseID equals cust.Id
                           where c.categoryID == category
                           select new ClouseModel { Clouse1 = cust.Clouse1, Id = c.clouseID });

            string newFilename = "";
            string path = "";
            string newpath = "";
            int customerId = 0;
            int GroupId = 1;

            try
            {

                int? userId = null;

                if (Session["customerId"] != null)
                {
                    customerId = Convert.ToInt32(Session["customerId"]);
                }

                if (Session["CurrentTemplateId"] != null)
                {
                    id = Convert.ToInt32(Session["CurrentTemplateId"]);
                    userId = Convert.ToInt32(Session["UserId"]);
                }

                // Updating status for create document
                if (id != null)
                {
                    var objDocumentTemplate = db.DocumentTemplates.Find(id);
                    string customerName = db.CustomerDetails.Single(s => s.CustomerId == customerId).CustomerName;
                    List<TemplateDynamicFormValue> objDynamicForm = new List<TemplateDynamicFormValue>();
                    objDynamicForm = db.TemplateDynamicFormValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true && b.ParentkeyId == null && b.CustomerId == customerId).ToList();
                    newFilename = customerName + "-" + objDocumentTemplate.DocumentTitle.Replace(" ", "") + "." + objDocumentTemplate.TemplateFileName.Split('.')[1];
                    path = Path.Combine(Server.MapPath("~/TemplateFiles/" + objDocumentTemplate.TemplateFileName.Replace(" ", ""))); // Getting Original File For Create a new one with filled details
                    newpath = Path.Combine(Server.MapPath("~/FilledTemplateFiles/" + newFilename)); // New File Path with File Name

                    foreach (TemplateDynamicFormValue tem in objDynamicForm)
                    {
                        string input = string.Empty;
                        var keyID = db.TemplateKeywords
                        .Where(k => k.TemplateKeyValue == tem.TemplateKey)
                        .First().TemplateKeyId;
                        {
                            var inputs = db.TemplateDynamicFormValues.Where(w => w.ParentkeyId == tem.TemplateKey && w.TemplateId == tem.TemplateId && w.CustomerId == tem.CustomerId && w.IsEnabled == true).ToList();
                            input = tem.UserInputs;
                            foreach (TemplateDynamicFormValue values in inputs)
                            {
                                if (inputs.Count > 0)
                                    values.IsEnabled = false;
                                db.SaveChanges();
                            }

                        }
                    }
                    //Update the status for creating new word document
                    foreach (var frmList in objDynamicForm)
                    {
                        frmList.IsEnabled = false;
                    }
                    db.SaveChanges();
                    Session["newFilename"] = newFilename;
                    CreateCoverLetteronHold(newFilename);
                    var objFilledForm = db.FilledTemplateDetails.Where(c => c.UserId == userId);
                    if (Session["Displayorder"] != null && Convert.ToInt32(Session["Displayorder"]) > 0)
                    {
                        GroupId = Convert.ToInt32(Session["GroupId"]);
                    }
                    else
                    {
                        var GroupForm = objFilledForm.OrderByDescending(d => d.GroupId).FirstOrDefault();

                        // Assign Group Id
                        if (GroupForm != null)
                        {
                            GroupId = GroupForm.GroupId + 1;
                            Session["GroupId"] = GroupId;
                        }
                        if (Session["GroupId"] != null && Convert.ToInt32(Session["GroupId"]) != 0)
                        {
                            if (Convert.ToInt32(Session["AssociateCount"]) >= 1)
                            {
                                // Holding same Group Id
                                GroupId = Convert.ToInt32(Session["GroupId"]);
                            }
                        }
                    }

                    using (var context = new VirtualAdvocateEntities())
                    {
                        using (var dbContextTransaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                // Insert Filled Form Details
                                FilledTemplateDetail objFilledTemp = new FilledTemplateDetail();
                                objFilledTemp.GroupId = GroupId;
                                objFilledTemp.PaidStatus = false;
                                objFilledTemp.UserId = userId.Value;
                                objFilledTemp.TemplateId = id.Value;
                                objFilledTemp.FilledTemplateName = newFilename;
                                objFilledTemp.Amount = objDocumentTemplate.TemplateCost;
                                objFilledTemp.CreatedDate = DateTime.Now;
                                objFilledTemp.CustomerId = customerId;
                                objFilledTemp.OrgId = Convert.ToInt32(Session["OrgId"]);
                                context.FilledTemplateDetails.Add(objFilledTemp);
                                context.SaveChanges();

                                FormCollection formCollection = TempData["FormCollection"] as FormCollection;

                                if (formCollection != null && (roleId == 5 || roleId == 6))
                                {
                                    var objkeyCategory = (from c in context.KeyCategories
                                                          join k in context.TemplateKeywords on c.Id equals k.TemplateKeyCategory
                                                          join p in context.TemplateKeysPointers on k.TemplateKeyId equals p.TemplateKeyId
                                                          where p.TemplateId == id
                                                          orderby c.CategoryOrder
                                                          select new
                                                          {
                                                              c.CategoryName,
                                                              c.CategoryOrder,
                                                              c.CanAddInsurance,
                                                              c.Id
                                                          }).Distinct().OrderBy(x => x.CategoryOrder
                                                    );

                                    foreach (var item in objkeyCategory)
                                    {
                                        var property = new Property();
                                        if (item.CanAddInsurance != null && item.CanAddInsurance.Value)
                                        {
                                            var assetInsured = formCollection["InsuranceAssetInsured-" + item.Id]?.ToLower().Trim();
                                            if (!string.IsNullOrEmpty(assetInsured))
                                            {
                                                var prop = context.Properties
                                                    .Include("FilledTemplateDetail")
                                                    .Where(m => m.FilledTemplateDetail.TemplateId == objFilledTemp.TemplateId
                                                    && m.FilledTemplateDetail.CustomerId == objFilledTemp.CustomerId
                                                    && m.PropertyName.ToLower() == assetInsured && m.Status);
                                                //var prop = context.Properties.Where(m => m.DocumentId == objFilledTemp.RowId && m.PropertyName.ToLower() == assetInsured);

                                                if (prop != null && prop.Count() > 0)
                                                {
                                                    //property already added
                                                    property = prop.FirstOrDefault();
                                                }
                                                else
                                                {
                                                    property = new Property
                                                    {
                                                        CreatedDate = DateTime.Now,
                                                        PropertyName = formCollection["InsuranceAssetInsured-" + item.Id].Trim(),
                                                        Status = true,
                                                        DocumentId = objFilledTemp.RowId,
                                                    };

                                                    context.Properties.Add(property);
                                                    context.SaveChanges();
                                                }

                                                if (property.Id != 0)
                                                {
                                                    if (formCollection["InsuranceWantToAddInsurance-" + item.Id] != null && Convert.ToInt32(formCollection["InsuranceWantToAddInsurance-" + item.Id]) == 1)
                                                    {
                                                        var insuranceExists = db.Insurances.Where(m => m.PropertyId == property.Id && m.Status).Count();

                                                        if (insuranceExists > 0)
                                                        {
                                                            //insurance already added
                                                        }
                                                        else
                                                        {
                                                            var insuranceDetail = new Insurance
                                                            {
                                                                PropertyId = property.Id,
                                                                Currency = formCollection.GetValue("InsuranceCurrency-" + item.Id).AttemptedValue.Trim(),
                                                                Insurer = formCollection.GetValue("InsuranceInsurer-" + item.Id).AttemptedValue.Trim(),
                                                                AmountInsured = formCollection.GetValue("InsuranceAmountInsured-" + item.Id).AttemptedValue.Trim(),
                                                                DateOfInsurance = DateTime.ParseExact(formCollection.GetValue("InsuranceDateOfInsrurance-" + item.Id).AttemptedValue.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                                                                DateOfExpiry = DateTime.ParseExact(formCollection.GetValue("InsuranceDateOfExpiry-" + item.Id).AttemptedValue.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                                                                CreatedDate = DateTime.Now,
                                                                UserId = userId,
                                                                Status = true
                                                            };

                                                            context.Insurances.Add(insuranceDetail);
                                                            context.SaveChanges();
                                                        }
                                                    }
                                                }

                                            }

                                        }
                                    }


                                }
                            }
                            catch (Exception ex)
                            {
                                //Log, handle or absorbe I don't care ^_^
                            }

                            dbContextTransaction.Commit();
                        }
                    }

                }

                int templateId = Convert.ToInt32(Session["TemplateId"]);
                int displayOrder = 0;
                if (Session["Displayorder"] != null)
                {
                    displayOrder = Convert.ToInt32(Session["Displayorder"]);
                }

                var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == templateId && c.IsEnabled == true && (c.DisplayOrder == null || c.DisplayOrder > displayOrder)).OrderBy(c => c.DisplayOrder).FirstOrDefault();

                if (objAssociateIds != null)
                {
                    var catogories = db.DocumentCategories.Where(d => d.ServiceId == 0 && d.IsEnabled == true).ToArray();
                    Session["Displayorder"] = objAssociateIds.DisplayOrder;
                    displayOrder = Convert.ToInt32(objAssociateIds.DisplayOrder);
                    var lastdoc = db.AssociateTemplateDetails.Where(c => c.TemplateId == templateId && c.IsEnabled == true && (c.DisplayOrder == null || c.DisplayOrder > displayOrder)).OrderBy(c => c.DisplayOrder + 1).FirstOrDefault();

                    if (lastdoc == null)
                    {
                        ViewBag.lastdoc = "true";
                    }
                    else
                    {
                        ViewBag.lastdoc = "false";
                    }

                    if (objAssociateIds.Mandatory)
                    {
                        Session["ExtraFiles"] = objAssociateIds.AssociateTemplateId;
                        return Json("CreateDynamicForm", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Session["ExtraFiles"] = objAssociateIds.AssociateTemplateId;
                        return Json("CoverLetterConfirm", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Session.Remove("Displayorder");
                    Session["AssociateCount"] = 0;
                    Session["GroupId"] = 0;
                    Session["customerId"] = null;
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                //return Json(true, JsonRequestBehavior.AllowGet);
                return Json(ex.StackTrace + "\n" + ex.InnerException, JsonRequestBehavior.AllowGet);
            }
        }

        //CoverLetter Process

        // string coverLetterpath = Path.Combine(Server.MapPath("~/CoverLetter/" + newFilename)); // New File Path with File Name
        // path = Path.Combine(Server.MapPath("~/CoverLetter/coverletter.docx")); // Getting Original File For Create a new one with filled details
        // System.IO.File.Copy(path, coverLetterpath);
        // var objDT = db.FilledTemplateDetails.Where(dc => dc.CustomerId == customerId && dc.GroupId == GroupId).ToList();

        // CustomerDetail objCD = db.CustomerDetails.Find(customerId);

        // List<DocumentTemplate> objDocumentTemp=new List<DocumentTemplate>();
        // if(objDT != null && objDT.Count()>0)
        // {               
        //     foreach (FilledTemplateDetail objFilled in objDT)
        //     {
        //         var objdc = db.DocumentTemplates.Find(objFilled.TemplateId);
        //         objDocumentTemp.Add(new DocumentTemplate { DocumentTitle = objdc.DocumentTitle });

        //     }
        // }

        // string docList = DocumentListForCoverLetter(objDocumentTemp);
        //CoverLetterInWord(coverLetterpath, objCD, docList);//CoverLetter Create 
        //ConvertToPdfFile(coverLetterpath); // Convert to pdf file



        public ActionResult Skip()
        {
            int templateId = Convert.ToInt32(Session["TemplateId"]);
            int displayOrder = 0;
            if (Session["Displayorder"] != null)
            {
                displayOrder = Convert.ToInt32(Session["Displayorder"]);
            }
            var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == templateId && c.IsEnabled == true && (c.DisplayOrder == null || c.DisplayOrder > displayOrder)).OrderBy(c => c.DisplayOrder).FirstOrDefault();

            if (objAssociateIds != null)
            {
                var lastdoc = db.AssociateTemplateDetails.Where(c => c.TemplateId == templateId && c.IsEnabled == true && (c.DisplayOrder == null || c.DisplayOrder > displayOrder)).OrderBy(c => c.DisplayOrder + 1).FirstOrDefault();
                Session["Displayorder"] = displayOrder + 1;
                if (lastdoc == null)
                    ViewBag.lastdoc = "true";
                else
                    ViewBag.lastdoc = "false";

                if (objAssociateIds.Mandatory)
                    return RedirectToAction("CreateDynamicForm", "DocumentManagement", new { id = objAssociateIds.AssociateTemplateId });
                else
                    return RedirectToAction("CoverLetterConfirm", "DocumentManagement", new { id = objAssociateIds.AssociateTemplateId });


            }
            return RedirectToAction("FormsHistory", "DocumentManagement");
        }

        //CoverLetter Process
        public void CreateCoverLetteronHold(string newFilename)
        {
            try
            {
                if (newFilename == null)
                {
                    newFilename = Session["newFilename"].ToString();
                }
                int customerId = Convert.ToInt32(Session["customerId"]);
                int GroupId = Convert.ToInt32(Session["GroupId"]);
                string coverLetterpath = Path.Combine(Server.MapPath("~/CoverLetter/" + newFilename)); // New File Path with File Name
                var path = Path.Combine(Server.MapPath("~/Resources/coverletter.docx")); // Getting Original File For Create a new one with filled details
                if (System.IO.File.Exists(coverLetterpath))
                {
                    System.IO.File.Delete(coverLetterpath);
                }
                System.IO.File.Copy(path, coverLetterpath);
                var objDT = db.FilledTemplateDetails.Where(dc => dc.CustomerId == customerId && dc.GroupId == GroupId).ToList();

                CustomerDetail objCD = db.CustomerDetails.Find(customerId);

                List<DocumentTemplate> objDocumentTemp = new List<DocumentTemplate>();
                if (objDT != null && objDT.Count() > 0)
                {
                    foreach (FilledTemplateDetail objFilled in objDT)
                    {
                        var objdc = db.DocumentTemplates.Find(objFilled.TemplateId);
                        objDocumentTemp.Add(new DocumentTemplate { DocumentTitle = objdc.DocumentTitle });

                    }
                }

                string docList = DocumentListForCoverLetter(objDocumentTemp);
                CoverLetterInWord(coverLetterpath, objCD, docList);//CoverLetter Create 
                ConvertToPdfFile(coverLetterpath); // Convert to pdf file

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

        }

        public ActionResult CoverLetterConfirm(int? id)
        {
            if (id == null)
            {
                ViewBag.AssociateId = Session["ExtraFiles"];
                Session.Remove("ExtraFiles");
                // return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                ViewBag.AssociateId = id;
            }
            return View();
        }

        //Manually creating CoverLetter
        public ActionResult CoverLetterConfirmed()
        {
            CreateCoverLetteronHold(Session["newFilename"].ToString());
            Session["AssociateCount"] = 0;
            Session["GroupId"] = 0;
            Session["customerId"] = null;
            return RedirectToAction("FormsHistory", "DocumentManagement");
        }



        public static void DoSearchAndReplaceInWord(string filepath, List<TemplateDynamicFormValue> obj, Int32 templateID)
        {
            StringBuilder stringBuilder = new StringBuilder();

            VirtualAdvocateEntities db = new VirtualAdvocateEntities();
            Application word = new Application();
            Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();
            //Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();
            // Define an object to pass to the API for missing parameters
            object missing = System.Type.Missing;
            try
            {
                // Everything that goes to the interop must be an object
                object fileName = filepath;
                doc = word.Documents.Open(ref fileName,
                    ref missing, ref missing, ref missing);//, ref missing,
                                                           //ref missing, ref missing, ref missing, ref missing,
                                                           //ref missing, ref missing, ref missing, ref missing,
                                                           //ref missing, ref missing, ref missing);
                                                           // Activate the document
                doc.Activate();


                var category = db.DocumentTemplates.Where(c => c.TemplateId == templateID).Select(c => c.DocumentCategory).FirstOrDefault();
                var statementKeys = db.AssociatedKeyGroups.Where(t => t.TemplateID == templateID && t.Statement).Select(k => k.KeyID).ToList();
                var maxlength = db.GetMaxlengthOfUserInputs(obj.FirstOrDefault().CustomerId, templateID).ToList();
                char[] alpha = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
                int customerID = 0;
                customerID = obj.FirstOrDefault().CustomerId;

                // Close the doc and exit the app
                doc.Close(ref missing, ref missing, ref missing);
                word.Application.Quit(ref missing, ref missing, ref missing);

                //Construct statement Level string

                var keys = (from k in db.AssociatedKeyGroups
                            join t in db.TemplateKeywords on k.KeyID equals t.TemplateKeyId
                            where k.TemplateID == templateID && k.Statement == true
                            select new
                            {
                                keyID = t.TemplateKeyId,
                                keyValue = t.TemplateKeyValue,
                                order = k.KeyOrder,
                                Group = k.GroupName
                            }).ToList();
                if (keys != null && keys.Count > 0)
                {
                    var groups = keys.GroupBy(x => x.Group)
                 .Select(g => g.First());
                    string statement = string.Empty;
                    string groupName = string.Empty;
                    foreach (var g in groups)
                    {
                        bool isNumeric = false;
                        int startsFrom = 0;
                        var groupAutoNo = db.AssociatedKeyGroups.Where(gp => gp.GroupName == g.Group && gp.AutoNumberStartsFrom != null).FirstOrDefault();

                        if (groupAutoNo != null)
                        {
                            if (groupAutoNo.AutoNumberStartsFrom != null)
                            {

                                isNumeric = int.TryParse(groupAutoNo.AutoNumberStartsFrom, out startsFrom);

                                if (!isNumeric)
                                {
                                    string alphabets = "abcdefghijklmnopqrstuvwxyz";

                                    string ext = alphabets.Substring(alphabets.IndexOf(groupAutoNo.AutoNumberStartsFrom));//, alphabets.Length - 1);
                                    alpha = ext.ToCharArray();
                                }

                            }
                        }
                        else
                        {
                            isNumeric = false;
                            alpha = "abcdefghijklmnopqrstuvwxyz".ToArray();
                        }
                        groupName = g.Group;
                        var groupKeys = keys.Where(k => k.Group == g.Group).OrderBy(o => o.order).ToList();
                        int key1 = groupKeys.FirstOrDefault().keyID;
                        string key1Value = groupKeys.FirstOrDefault().keyValue;
                        foreach (var p in groupKeys)
                        {
                            var inputs = db.TemplateDynamicFormValues.Where(w => w.TemplateId == templateID && w.TemplateKey == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).FirstOrDefault();
                            statement = statement + " " + inputs.UserInputs;
                            inputs.IsEnabled = false;
                            db.SaveChanges();
                        }

                        var multivalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == templateID && w.ParentkeyId == key1Value && w.CustomerId == customerID && w.IsEnabled == true).ToList();

                        if (multivalues != null && multivalues.Count() > 0)
                            if (isNumeric)
                                statement = startsFrom + ". " + statement + "\v<" + groupName + "_1>";
                            else
                                statement = alpha[0] + ". " + statement + "\v<" + groupName + "_1>";

                        //comment for tablev SearchAndReplace(groupName, statement, filepath);

                        groupName = groupName + "_1";
                        int f = 2;
                        int j = 1;
                        if (isNumeric)
                            j = startsFrom + 1;
                        else
                            j = 1;
                        foreach (var m in multivalues)
                        {
                            if (isNumeric)
                            {
                                statement = j + ". ";
                            }
                            else
                                statement = alpha[j] + ". ";
                            foreach (var p in groupKeys)
                            {

                                var multiKeyvalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == templateID && w.ParentkeyId == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).FirstOrDefault();
                                statement = statement + " " + multiKeyvalues.UserInputs;
                                multiKeyvalues.IsEnabled = false;
                                db.SaveChanges();

                            }
                            if (j != multivalues.Count())
                                statement = statement + "\v<" + groupName + "_" + f + ">";

                            //comment for tablev SearchAndReplace(groupName, statement, filepath);

                            groupName = groupName + "_" + f;
                            j++;
                        }
                        groupName = g.Group;
                    }
                }
                //code for table  code by vaishalit
                else
                {
                    string wordContent = "";
                    //wordContent = getWordContent(filepath);
                    var keystable = (from k in db.AssociatedKeyGroups
                                     join t in db.TemplateKeywords on k.KeyID equals t.TemplateKeyId
                                     where k.TemplateID == templateID && k.Statement == false
                                     select new
                                     {
                                         keyID = t.TemplateKeyId,
                                         keyValue = t.TemplateKeyValue,
                                         order = k.KeyOrder,
                                         Group = k.GroupName
                                     }).ToList();
                    if (keystable != null && keystable.Count > 0)
                    {
                        var groups = keystable.GroupBy(x => x.Group)
                      .Select(g => g.First());
                        string groupName = string.Empty;
                        StringBuilder htmlTable = new StringBuilder();
                        htmlTable.Append("<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'>");
                        int chkloop = 0;

                        //tbl.Columns.Add("col1");
                        foreach (var g in groups)
                        {
                            string statement = string.Empty;
                            bool isNumeric = false;
                            int startsFrom = 0;
                            groupName = g.Group;
                            var groupKeys = keystable.Where(k => k.Group == g.Group).OrderBy(o => o.order).ToList();
                            int key1 = groupKeys.FirstOrDefault().keyID;
                            string key1Value = groupKeys.FirstOrDefault().keyValue;
                            int count = 0;
                            int countm = 0;
                            foreach (var p in groupKeys)
                            {
                                var inputs = db.TemplateDynamicFormValues.Where(w => w.TemplateId == templateID && w.TemplateKey == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).FirstOrDefault();
                                // statement = statement + " " + inputs.UserInputs;
                                inputs.IsEnabled = false;
                                db.SaveChanges();
                            }

                            foreach (var p in groupKeys)
                            {

                                if (chkloop == 0)
                                {
                                    for (int th = 0; th < groupKeys.Count; th++)
                                    {
                                        string d = groupKeys[th].keyValue;
                                        var keyID2 = db.TemplateKeywords.Where(k => k.TemplateKeyValue == d && k.Cloned != true).FirstOrDefault();
                                        if (keyID2 != null)
                                        {

                                            htmlTable.Append("<th style='border: 1px solid #ccc'>" + keyID2.TemplateKeyLabels + "</th>");

                                        }
                                        else
                                        {
                                            var keyIDcloned = db.TemplateKeywords.Where(k => k.TemplateKeyValue == d).FirstOrDefault();
                                            htmlTable.Append("<th style='border: 1px solid #ccc'>" + keyIDcloned.TemplateKeyLabels + "</th>");
                                        }
                                        chkloop = 1;
                                    }
                                }
                                var inputs = db.TemplateDynamicFormValues.Where(w => w.TemplateId == templateID && w.TemplateKey == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).FirstOrDefault();
                                // inputs.IsEnabled = false;
                                //db.SaveChanges();
                                if (inputs != null)
                                {
                                    if (count == 0)
                                        htmlTable.Append("<tr>");
                                    htmlTable.Append("<td style='width:100px;border: 1px solid #ccc'>" + inputs.UserInputs + "</td>");
                                    count++;
                                    if (count == groupKeys.Count)
                                    {
                                        htmlTable.Append("</tr>");
                                        count = 0;
                                    }
                                }
                            }

                            var multivalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == templateID && w.ParentkeyId == key1Value && w.CustomerId == customerID && w.IsEnabled == true).ToList();
                            //var multivalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == id && w.ParentkeyId != null && w.CustomerId == customerID && w.IsEnabled == true).GroupBy(w=>w.ParentkeyId).ToList();
                            //  var countkey = db.TemplateDynamicFormValues.
                            //     Where(w => w.TemplateId == templateID && w.CustomerId == customerID && w.IsEnabled == true && w.ParentkeyId != null).
                            //    GroupBy(w => w.ParentkeyId).OrderByDescending(t => t.Count()).First();

                            // int countkeystotal = countkey.Count();
                            if (multivalues != null && multivalues.Count() > 0)
                            {
                                List<PreviewRowID> Rowids = new List<PreviewRowID>();
                                // groupName = groupName + "_1";
                                int f = 2;
                                int j = 1;
                                //for (int cnt = 0; cnt <= multivalues; cnt++)
                                foreach (var m in multivalues)
                                {

                                    htmlTable.Append("<tr>");
                                    foreach (var p in groupKeys)
                                    {
                                        List<PreviewKeyValue> keyvalues = new List<PreviewKeyValue>();
                                        var multiKeyvalues = db.TemplateDynamicFormValues.Where(w => w.TemplateId == templateID && w.ParentkeyId == p.keyValue && w.CustomerId == customerID && w.IsEnabled == true).ToList();
                                        for (int i = 0; i <= multiKeyvalues.Count - 1; i++)
                                        {
                                            if (keyvalues.Count > 0)
                                            {
                                                if (Rowids.All(x => x.RowId.ToString() != multiKeyvalues[i].RowId.ToString()))
                                                {
                                                    if (keyvalues.All(x => x.ToString() != multiKeyvalues[i].TemplateKey))
                                                    {
                                                        keyvalues.Add(new PreviewKeyValue { TemplateKey = multiKeyvalues[i].TemplateKey, RowId = multiKeyvalues[i].RowId });
                                                        //  keyvalues.Add(.ToString());
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                if (Rowids.All(x => x.RowId.ToString() != multiKeyvalues[i].RowId.ToString()))
                                                {
                                                    //htmlTable = htmlTable.Append("<tr>");
                                                    keyvalues.Add(new PreviewKeyValue { TemplateKey = multiKeyvalues[i].TemplateKey, RowId = multiKeyvalues[i].RowId });
                                                }
                                            }
                                        }
                                        if (keyvalues.Count == 0)
                                        {
                                            htmlTable.Append("<td style='width:100px;border: 1px solid #ccc'>" + "" + "</td>");
                                        }
                                        else
                                        {
                                            foreach (var t in keyvalues)
                                            {
                                                Rowids.Add(new PreviewRowID { RowId = t.RowId });
                                                var uservalue = db.TemplateDynamicFormValues.Where(x => x.CustomerId == customerID && x.IsEnabled == true && x.TemplateKey == t.TemplateKey).FirstOrDefault();
                                                // statement = statement + " " + uservalue.UserInputs;
                                                htmlTable.Append("<td style='width:100px;border: 1px solid #ccc'>" + uservalue.UserInputs + "</td>");
                                                break;
                                            }
                                        }
                                        //htmlTable.Append("</tr>");
                                        //keyvalues.Remove(t);

                                    }
                                    htmlTable.Append("</tr>");
                                    if (isNumeric ? j - 1 != multivalues.Count() : j != multivalues.Count())
                                        //if (isNumeric ? j - 1 != cnt : j != cnt)
                                        //htmlTable = htmlTable.Append(+ groupName.ToString() + "_" + f + ">");
                                        j++;
                                }
                                htmlTable.Append("</table>");
                                // Paragraph paragraph= doc.Sections[0].p;
                                // SearchAndReplace(groupName, htmlTable.ToString(), filepath);
                                SearchAndReplace(groupName, htmlTable.ToString(), filepath);
                            }
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                doc.Close(ref missing, ref missing, ref missing);
                word.Application.Quit(ref missing, ref missing, ref missing);
            }
        }


        public static void SearchAndReplace(string keyName, string value, string filePath)
        {
            Application word = new Application();
            Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();
            // HtmlToPdfConverter converter = new HtmlToPdfConverter();
            // Define an object to pass to the API for missing parameters
            object missing = System.Type.Missing;
            VirtualAdvocateEntities db = new VirtualAdvocateEntities();
            try
            {

                // Everything that goes to the interop must be an object
                object fileName = filePath;
                doc = word.Documents.Open(ref fileName,
                    ref missing, ref missing, ref missing);//, ref missing,
                doc.Activate();
                // doc.Paragraphs.
                // Loop through the StoryRanges (sections of the Word doc)
                foreach (Range tmpRange in doc.StoryRanges)
                {
                    Tables tables = doc.Tables;
                    //  tmpRange.Tables[0] = value;
                    // Set the text to find and replace
                    tmpRange.Find.Text = "<" + keyName + ">";
                    tmpRange.Find.Replacement.Text = value;

                    //tmpRange.Paragraphs.
                    tmpRange.Find.Wrap = WdFindWrap.wdFindContinue;
                    object replaceAll = WdReplace.wdReplaceAll;
                    tmpRange.Find.Execute(ref missing, ref missing, ref missing,
                        ref missing, ref missing, ref missing, ref missing,
                        ref missing, ref missing, ref missing, ref replaceAll,
                        ref missing, ref missing, ref missing, ref missing);
                }
                // Save the changes
                doc.Save();
                // Close the doc and exit the app
                doc.Close(ref missing, ref missing, ref missing);
                word.Application.Quit(ref missing, ref missing, ref missing);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                doc.Close(ref missing, ref missing, ref missing);
                word.Application.Quit(ref missing, ref missing, ref missing);
            }

        }

        private static Stream WordStream(string body)
        {
            var ms = new MemoryStream();

            byte[] byteInfo = Encoding.ASCII.GetBytes(body);
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            return ms;
        }

        public string DocumentListForCoverLetter(List<DocumentTemplate> objDocument)
        {
            string docList = "";
            int i = 0;
            foreach (DocumentTemplate dclist in objDocument)
            {

                docList = docList + i + ". Copy of form " + dclist.DocumentTitle + " \r\n";
                i++;
            }

            return docList;
        }

        public void CreateLetterInWordUsingXML(string filepath, CustomerDetail objcustomer, string docList)
        {
            string totaltext = "";
            try
            {
                string path1 = Path.Combine(Server.MapPath("~/Resources/coverletter.docx"));

                byte[] byteArray = System.IO.File.ReadAllBytes(path1);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                    {
                        HtmlConverterSettings settings = new HtmlConverterSettings()
                        {
                        };

                        XElement html = OpenXmlPowerTools.HtmlConverter.ConvertToHtml(doc, settings);

                        totaltext = html.ToStringNewLineOnAttributes();

                    }
                }


                string[] coverKeys = new string[] { "date", "NameandaddressofBank", "nameofBank", "CUSTOMERNAME", "DocumentNameList" };
                string[] CustomerDetail = new string[5];// { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME" };
                CustomerDetail[0] = DateTime.Now.ToShortDateString();
                CustomerDetail[1] = objcustomer.Address;
                CustomerDetail[2] = objcustomer.BankName;
                CustomerDetail[3] = objcustomer.CustomerName;
                CustomerDetail[4] = docList;
                int k = 0;
                foreach (string tem in coverKeys)
                {
                    totaltext = totaltext.Replace("# " + tem + " #", CustomerDetail[k]);
                    totaltext = totaltext.Replace("#" + tem + "#", CustomerDetail[k]);
                    k++;
                }

                filepath = filepath.Replace(".docx", ".pdf");
                CreateDocumentFromHiQpdf(totaltext, filepath);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

        }




        public static void CoverLetterInWord(string filepath, CustomerDetail objcustomer, string docList)
        {
            try
            {
                // Create the Word application and declare a document
                Application word = new Application();
                Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();
                // Define an object to pass to the API for missing parameters
                object missing = System.Type.Missing;

                try
                {

                    // Everything that goes to the interop must be an object
                    object fileName = filepath;

                    // Open the Word document.
                    // Pass the "missing" object defined above to all optional
                    // parameters.  All parameters must be of type object,
                    // and passed by reference.
                    doc = word.Documents.Open(ref fileName,
                        ref missing, ref missing, ref missing);//, ref missing,
                                                               //ref missing, ref missing, ref missing, ref missing,
                                                               //ref missing, ref missing, ref missing, ref missing,
                                                               //ref missing, ref missing, ref missing);

                    // Activate the document
                    doc.Activate();
                    string[] coverKeys = new string[] { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME", "DocumentNameList" };
                    string[] CustomerDetail = new string[5];// { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME" };
                    CustomerDetail[0] = DateTime.Now.ToShortDateString();
                    CustomerDetail[1] = objcustomer.Address;
                    CustomerDetail[2] = objcustomer.BankName;
                    CustomerDetail[3] = objcustomer.CustomerName;
                    CustomerDetail[4] = docList;
                    int k = 0;
                    foreach (string tem in coverKeys)
                    {

                        // Loop through the StoryRanges (sections of the Word doc)
                        foreach (Range tmpRange in doc.StoryRanges)
                        {
                            // Set the text to find and replace
                            tmpRange.Find.Text = "#" + tem + "#";
                            tmpRange.Find.Replacement.Text = CustomerDetail[k];
                            // Set the Find.Wrap property to continue (so it doesn't
                            // prompt the user or stop when it hits the end of
                            // the section)
                            tmpRange.Find.Wrap = WdFindWrap.wdFindContinue;

                            // Declare an object to pass as a parameter that sets
                            // the Replace parameter to the "wdReplaceAll" enum
                            object replaceAll = WdReplace.wdReplaceAll;

                            // Execute the Find and Replace -- notice that the
                            // 11th parameter is the "replaceAll" enum object
                            tmpRange.Find.Execute(ref missing, ref missing, ref missing,
                                ref missing, ref missing, ref missing, ref missing,
                                ref missing, ref missing, ref missing, ref replaceAll,
                                ref missing, ref missing, ref missing, ref missing);
                        }
                        k++;

                    }



                    // Save the changes
                    doc.Save();

                    // Close the doc and exit the app
                    doc.Close(ref missing, ref missing, ref missing);
                    word.Application.Quit(ref missing, ref missing, ref missing);
                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                    doc.Close(ref missing, ref missing, ref missing);
                    word.Application.Quit(ref missing, ref missing, ref missing);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

        }


        #endregion

        #region Filled Form History

        /// <summary>
        /// Filled Document List Based on Logged in User
        /// </summary>
        /// <returns></returns>
        public ActionResult FormsHistory()
        {
            List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                var customer = (from user in db.UserProfiles.Where(u => u.UserID == userId) select user.OrganizationId).FirstOrDefault();

                int roleID = Convert.ToInt32(Session["RoleId"] ?? 0);
                var department = db.UserProfiles.Where(d => d.UserID == userId).FirstOrDefault().Department;
                var objFilledTemp = (from obj in db.FilledTemplateDetails

                                     join cust in db.CustomerDetails on obj.CustomerId equals cust.CustomerId
                                     join doc in db.DocumentTemplates on obj.TemplateId equals doc.TemplateId into g
                                     from subset in g.DefaultIfEmpty()
                                     where obj.OrgId == customer &&
                                     (obj.ArchiveStatus == null || obj.ArchiveStatus == false) &&
                                     (obj.CoverLetter == null || obj.CoverLetter == true) &&
                                     ((roleID != 6 && roleID != 5) || (roleID == 6 && subset.DepartmentID == department) || (roleID == 5 && obj.UserId == userId))

                                     select new FilledFormDetailModel { DocumentTitle = (subset == null ? "Template Deleted" : subset.DocumentTitle), Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, RowId = obj.RowId, CustomerName = cust.CustomerName }
                    );
                objForm = objFilledTemp.OrderByDescending(x => x.GroupId).ThenBy(o => o.RowId).OrderByDescending(y => y.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objForm);
        }

        /// <summary>
        /// Filled Document List - For Super Admin
        /// </summary>
        /// <returns></returns>
        public ActionResult AllFilledFormsList()
        {
            List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                int orgid = Convert.ToInt32(Session["OrgId"]);
                int roleID = Convert.ToInt32(Session["RoleId"]);
                var department = db.UserProfiles.Where(d => d.UserID == userId).FirstOrDefault().Department;
                var objFilledTemp = (from obj in db.FilledTemplateDetails
                                     join doc in db.DocumentTemplates on obj.TemplateId equals doc.TemplateId into g
                                     from subset in g.DefaultIfEmpty()
                                     where (obj.CoverLetter == null || obj.CoverLetter == true)
     && (roleID != 6 || (roleID == 6 && subset.DepartmentID == department))
                                     select new FilledFormDetailModel { DocumentTitle = (subset == null ? "Template Deleted" : subset.DocumentTitle), Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, UserId = obj.UserId, OrgId = obj.OrgId }
                    ).OrderBy(x => x.UserId);

                objForm = objFilledTemp.OrderByDescending(m => m.CreatedDate).ToList();
                if (orgid != 0)
                {
                    objForm = objForm.Where(org => org.OrgId == orgid).ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objForm);
        }
        #endregion

        #region Archive Documents
        public ActionResult ArchiveDocument()
        {
            List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                //  var docName = db.FilledTemplateDetails.Select(t=>t.TemplateId).Distinct().Select().ToList();

                if (Convert.ToInt32(Session["RoleId"]) == 1)  // Super Admin
                {
                    var objFilledTemp = (from obj in db.FilledTemplateDetails
                                         join doc in db.DocumentTemplates on obj.TemplateId equals doc.TemplateId
                                         where (obj.ArchiveStatus == null || obj.ArchiveStatus == false)
                                         // join d in docName on obj.RowId equals d.RowId
                                         select new FilledFormDetailModel { DocumentTitle = doc.DocumentTitle, Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, UserId = obj.UserId, RowId = obj.RowId }
                   ).OrderBy(x => x.UserId);
                    objForm = objFilledTemp.OrderByDescending(m => m.CreatedDate).Distinct().ToList();
                }
                else  // Other Users
                {
                    int roleID = Convert.ToInt32(Session["RoleId"]);
                    var department = db.UserProfiles.Where(d => d.UserID == userId).FirstOrDefault().Department;

                    var objFilledTemp = (from obj in db.FilledTemplateDetails
                                         join doc in db.DocumentTemplates on obj.TemplateId equals doc.TemplateId
                                         //   join d in docName on obj.RowId equals d.RowId
                                         where obj.UserId == userId
                                         //&&
                                         //docName.Contains(obj.TemplateId)
                                         && (obj.ArchiveStatus == null || obj.ArchiveStatus == false)
                                    && (((roleID != 6) && (roleID != 5)) || ((roleID == 6 && doc.DepartmentID == department) || (roleID == 5 && doc.DepartmentID == department) || roleID == 3))
                                         select new FilledFormDetailModel { DocumentTitle = doc.DocumentTitle, Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, RowId = obj.RowId }
                   );

                    objForm = objFilledTemp.OrderByDescending(m => m.CreatedDate).Distinct().ToList();
                }


            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objForm);
        }

        [HttpPost]
        public ActionResult ArchiveDocument(int[] ArchiveId)
        {
            try
            {
                if (ArchiveId != null && ArchiveId.Length > 0)
                {
                    for (int i = 0; i < ArchiveId.Length; i++)
                    {
                        var obj = db.FilledTemplateDetails.Find(ArchiveId[i]);

                        var existFilepath = Path.Combine(Server.MapPath("~/TemplateFiles/"), obj.FilledTemplateName);
                        var path1 = Path.Combine(Server.MapPath("~/ArchiveDocuments/"), Path.GetFileName(obj.FilledTemplateName));
                        if (System.IO.File.Exists(existFilepath))
                        {
                            System.IO.File.Copy(existFilepath, path1, true); // Existing File copy to Archive Folder
                            System.IO.File.Delete(existFilepath); //Delete Old File From TemplateFiles Folder
                        }

                        obj.ArchiveStatus = true;
                        db.SaveChanges();

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("ArchiveDocument", "DocumentManagement");
        }
        #endregion

        #region Cover Letter
        public ActionResult CoverLetter()
        {
            return View();
        }

        public ActionResult CreateCoverLetter(CoverLetterModel obj)
        {
            try
            {
                CoverLetter objCover = new CoverLetter();
                objCover.BankName = obj.BankName;
                objCover.BankAddress = obj.BankAddress;
                objCover.CustomerName = obj.CustomerName;
                objCover.CreatedDate = DateTime.Now;
                objCover.UserId = Convert.ToInt32(Session["UserId"]);
                objCover.GroupId = Convert.ToInt32(Session["GroupId"]);
                db.CoverLetters.Add(objCover);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View();
        }
        #endregion

        #region Bootbox Test
        public ActionResult Test() // Create Folders
        {
            try
            {
                DirectoryInfo di = Directory.CreateDirectory("~/TemplateFiles");
                DirectoryInfo dj = Directory.CreateDirectory("~/ArchiveDocuments");
                DirectoryInfo dk = Directory.CreateDirectory("~/TemplateFiles/Archive");
                DirectoryInfo dd = Directory.CreateDirectory("~/FilledTemplateFiles");
                DirectoryInfo dr = Directory.CreateDirectory("~/DueReports");
                DirectoryInfo dif = Directory.CreateDirectory("~/DueInvoiceFiles");
                DirectoryInfo dic = Directory.CreateDirectory("~/Invoices");
                Response.Write("Folder Created");
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View();
        }

        //public ActionResult Test1()
        //{
        //    //CoverLetter Process
        //    string newFilename = "test.docx";
        //    string path = "";
        //    string coverLetterpath = Path.Combine(Server.MapPath("~/CoverLetter/" + newFilename)); // New File Path with File Name
        //    path = Path.Combine(Server.MapPath("~/CoverLetter/coverletter.docx")); // Getting Original File For Create a new one with filled details
        //    System.IO.File.Copy(path, coverLetterpath);
        //    var objDT = db.FilledTemplateDetails.Where(dc => dc.CustomerId == 1).ToList();

        //    CustomerDetail objCD = db.CustomerDetails.Find(1);

        //    List<DocumentTemplate> objDocumentTemp = new List<DocumentTemplate>();
        //    if (objDT != null && objDT.Count() > 0)
        //    {
        //        foreach (FilledTemplateDetail objFilled in objDT)
        //        {
        //            var objdc = db.DocumentTemplates.Find(objFilled.TemplateId);
        //            objDocumentTemp.Add(new DocumentTemplate { DocumentTitle = objdc.DocumentTitle });

        //        }
        //    }

        //    string docList = DocumentListForCoverLetter(objDocumentTemp);


        //    try
        //    {
        //        // Create the Word application and declare a document
        //        Application word = new Application();
        //        Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();
        //        // Define an object to pass to the API for missing parameters
        //        object missing = System.Type.Missing;

        //        try
        //        {

        //            // Everything that goes to the interop must be an object
        //            object fileName = coverLetterpath;

        //            // Open the Word document.
        //            // Pass the "missing" object defined above to all optional
        //            // parameters.  All parameters must be of type object,
        //            // and passed by reference.
        //            doc = word.Documents.Open(ref fileName,
        //                ref missing, ref missing, ref missing);//, ref missing,
        //                                                       //ref missing, ref missing, ref missing, ref missing,
        //                                                       //ref missing, ref missing, ref missing, ref missing,
        //                                                       //ref missing, ref missing, ref missing);
        //            Response.Write(">>>");
        //            // Activate the document
        //            doc.Activate();
        //            Response.Write("<<<");
        //            string[] coverKeys = new string[] { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME", "DocumentNameList" };
        //            string[] CustomerDetail = new string[5];// { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME" };
        //            CustomerDetail[0] = DateTime.Now.ToShortDateString();
        //            CustomerDetail[1] = objCD.Address;
        //            CustomerDetail[2] = objCD.BankName;
        //            CustomerDetail[3] = objCD.CustomerName;
        //            CustomerDetail[4] = docList;
        //            int k = 0;
        //            foreach (string tem in coverKeys)
        //            {

        //                // Loop through the StoryRanges (sections of the Word doc)
        //                foreach (Range tmpRange in doc.StoryRanges)
        //                {
        //                    // Set the text to find and replace
        //                    tmpRange.Find.Text = "#" + tem + "#";
        //                    tmpRange.Find.Replacement.Text = CustomerDetail[k];
        //                    // Set the Find.Wrap property to continue (so it doesn't
        //                    // prompt the user or stop when it hits the end of
        //                    // the section)
        //                    tmpRange.Find.Wrap = WdFindWrap.wdFindContinue;

        //                    // Declare an object to pass as a parameter that sets
        //                    // the Replace parameter to the "wdReplaceAll" enum
        //                    object replaceAll = WdReplace.wdReplaceAll;

        //                    // Execute the Find and Replace -- notice that the
        //                    // 11th parameter is the "replaceAll" enum object
        //                    tmpRange.Find.Execute(ref missing, ref missing, ref missing,
        //                        ref missing, ref missing, ref missing, ref missing,
        //                        ref missing, ref missing, ref missing, ref replaceAll,
        //                        ref missing, ref missing, ref missing, ref missing);
        //                }
        //                k++;

        //            }



        //            // Save the changes
        //            doc.Save();

        //            // Close the doc and exit the app
        //            doc.Close(ref missing, ref missing, ref missing);
        //            word.Application.Quit(ref missing, ref missing, ref missing);
        //        }
        //        catch (Exception ex)
        //        {
        //            ErrorLog.LogThisError(ex);
        //            doc.Close(ref missing, ref missing, ref missing);
        //            word.Application.Quit(ref missing, ref missing, ref missing);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }


        //    //try
        //    //{
        //    //    CoverLetterInWord(coverLetterpath, objCD, docList); //CoverLetter Create 


        //    //    ViewBag.Status = "Word writing process success";
        //    //    //Response.Write("Word writing process success");
        //    // // ConvertToPdfFile(coverLetterpath); // Convert to pdf file
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    ErrorLog.LogThisError(ex);
        //    //    //ErrorLog.LogThisError(ex.InnerException);                
        //    //    ViewBag.Status = "Word writing process failed";
        //    //    Response.Write(ex.InnerException);
        //    //    Response.Write("Message: " + ex.Message);
        //    //    Response.Write("Source : " + ex.Source);
        //    //    Response.Write(" Data: " + ex.Data.Values);
        //    //    Response.Write(" Trace: " + ex.StackTrace);

        //    //}


        //    return View();
        //}
        #endregion

        #region Customers
        public ActionResult AddCustomer(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CustomerDetailsModel obj = new CustomerDetailsModel();
            obj.OrganizationId = id.Value;
            return View(obj);
        }

        [HttpPost]
        public ActionResult AddCustomer(CustomerDetailsModel obj)
        {
            CustomerDetail objCustomer = new CustomerDetail();
            try
            {
                objCustomer.CustomerName = obj.CustomerName;
                objCustomer.AccountNumber = obj.AccountNumber;
                objCustomer.Address = obj.Address;
                objCustomer.BankName = obj.BankName;
                objCustomer.EmailAddress = obj.EmailAddress;
                objCustomer.IsEnabled = true;
                objCustomer.OrganizationId = obj.OrganizationId;

                db.CustomerDetails.Add(objCustomer);
                db.SaveChanges();

                if (obj.extraFields.Count() > 0)
                {

                    foreach (CustomerTemplateDetail item in obj.extraFields)
                    {
                        CustomerTemplateDetail objExtra = new CustomerTemplateDetail();
                        objExtra.CustID = objCustomer.CustomerId;
                        objExtra.FieldName = item.FieldName.Replace(' ', '_');
                        objExtra.FieldValue = item.FieldValue;
                        objExtra.More = true;
                        db.CustomerTemplateDetails.Add(objExtra);
                        db.SaveChanges();
                    }

                }

                // Add customer Name
                CustomerTemplateDetail objCustomerName = new CustomerTemplateDetail();
                objCustomerName.CustID = objCustomer.CustomerId;
                objCustomerName.FieldName = "Customer_Name";
                objCustomerName.FieldValue = obj.CustomerName;
                objCustomerName.More = false;
                db.CustomerTemplateDetails.Add(objCustomerName);
                db.SaveChanges();

                //Add CustomerAddress
                CustomerTemplateDetail objAccountNumber = new CustomerTemplateDetail();
                objAccountNumber.CustID = objCustomer.CustomerId;
                objAccountNumber.FieldName = "Account_Number";
                objAccountNumber.FieldValue = obj.AccountNumber;
                objAccountNumber.More = false;
                db.CustomerTemplateDetails.Add(objAccountNumber);
                db.SaveChanges();

                //Add CustomerAddress
                CustomerTemplateDetail objBankName = new CustomerTemplateDetail();
                objBankName.CustID = objCustomer.CustomerId;
                objBankName.FieldName = "Bank_Name";
                objBankName.FieldValue = obj.BankName;
                objBankName.More = false;
                db.CustomerTemplateDetails.Add(objBankName);
                db.SaveChanges();


                //Add CustomerAddress
                CustomerTemplateDetail objAddress = new CustomerTemplateDetail();
                objAddress.CustID = objCustomer.CustomerId;
                objAddress.FieldName = "Address";
                objAddress.FieldValue = obj.Address;
                objAddress.More = false;
                db.CustomerTemplateDetails.Add(objAddress);
                db.SaveChanges();

                //Add CustomerAddress
                CustomerTemplateDetail objEmailAddress = new CustomerTemplateDetail();
                objEmailAddress.CustID = objCustomer.CustomerId;
                objEmailAddress.FieldName = "Email";
                objEmailAddress.FieldValue = obj.EmailAddress;
                objEmailAddress.More = false;
                db.CustomerTemplateDetails.Add(objEmailAddress);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return RedirectToAction("CustomerList", "DocumentManagement", new { id = objCustomer.OrganizationId });
        }
        public ActionResult EditCustomer(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerDetailsModel objCustomer = new CustomerDetailsModel();
            var obj = db.CustomerDetails.Find(id);
            try
            {
                objCustomer.CustomerName = obj.CustomerName;
                objCustomer.AccountNumber = obj.AccountNumber;
                objCustomer.Address = obj.Address;
                objCustomer.BankName = obj.BankName;
                objCustomer.EmailAddress = obj.EmailAddress;
                objCustomer.CustomerId = obj.CustomerId;
                objCustomer.OrganizationId = obj.OrganizationId;
                objCustomer.extraFields = db.CustomerTemplateDetails.Where(d => d.CustID == objCustomer.CustomerId && d.More == true).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View(objCustomer);
        }

        [HttpPost]
        public ActionResult EditCustomer(CustomerDetailsModel obj)
        {
            CustomerDetail objCustomer = new CustomerDetail();
            objCustomer = db.CustomerDetails.Find(obj.CustomerId);
            try
            {
                objCustomer.CustomerName = obj.CustomerName;
                objCustomer.AccountNumber = obj.AccountNumber;
                objCustomer.Address = obj.Address;
                objCustomer.BankName = obj.BankName;
                db.CustomerTemplateDetails.Where(d => d.CustID == objCustomer.CustomerId).Delete();

                db.SaveChanges();

                db.CustomerTemplateDetails.ToList().RemoveAll(c => c.CustID == objCustomer.CustomerId);
                db.SaveChanges();

                if (obj.extraFields.Count() > 0)
                {
                    foreach (CustomerTemplateDetail item in obj.extraFields)
                    {
                        CustomerTemplateDetail objExtra = new CustomerTemplateDetail();
                        objExtra.CustID = objCustomer.CustomerId;
                        objExtra.FieldName = item.FieldName;
                        objExtra.FieldValue = item.FieldValue;
                        objExtra.More = true;
                        db.CustomerTemplateDetails.Add(objExtra);
                        db.SaveChanges();
                    }

                }
                // Add customer Name
                CustomerTemplateDetail objCustomerName = new CustomerTemplateDetail();
                objCustomerName.CustID = objCustomer.CustomerId;
                objCustomerName.FieldName = "Customer Name";
                objCustomerName.FieldValue = obj.CustomerName;
                db.CustomerTemplateDetails.Add(objCustomerName);
                db.SaveChanges();

                //Add CustomerAddress
                CustomerTemplateDetail objAccountNumber = new CustomerTemplateDetail();
                objAccountNumber.CustID = objCustomer.CustomerId;
                objAccountNumber.FieldName = "Account Number";
                objAccountNumber.FieldValue = obj.AccountNumber;
                db.CustomerTemplateDetails.Add(objAccountNumber);
                db.SaveChanges();

                //Add CustomerAddress
                CustomerTemplateDetail objBankName = new CustomerTemplateDetail();
                objBankName.CustID = objCustomer.CustomerId;
                objBankName.FieldName = "Bank Name";
                objBankName.FieldValue = obj.BankName;
                db.CustomerTemplateDetails.Add(objBankName);
                db.SaveChanges();


                //Add CustomerAddress
                CustomerTemplateDetail objAddress = new CustomerTemplateDetail();
                objAddress.CustID = objCustomer.CustomerId;
                objAddress.FieldName = "Address";
                objAddress.FieldValue = obj.Address;
                db.CustomerTemplateDetails.Add(objAddress);
                db.SaveChanges();

                //Add CustomerAddress
                CustomerTemplateDetail objEmailAddress = new CustomerTemplateDetail();
                objEmailAddress.CustID = objCustomer.CustomerId;
                objEmailAddress.FieldName = "Email Address";
                objEmailAddress.FieldValue = obj.EmailAddress;
                db.CustomerTemplateDetails.Add(objEmailAddress);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return RedirectToAction("CustomerList", "DocumentManagement", new { id = objCustomer.OrganizationId });
        }
        public ActionResult CustomerList(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<CustomerDetailsModel> objCustomerList = new List<CustomerDetailsModel>();
            try
            {
                objCustomerList = (from Cus in db.CustomerDetails.Where(m => m.OrganizationId == id)
                                   select new CustomerDetailsModel { CustomerId = Cus.CustomerId, AccountNumber = Cus.AccountNumber, Address = Cus.Address, CustomerName = Cus.CustomerName, BankName = Cus.BankName, EmailAddress = Cus.EmailAddress, IsEnabled = Cus.IsEnabled, OrganizationId = Cus.OrganizationId }
                     ).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View(objCustomerList);
        }

        [HttpGet]
        public JsonResult CheckCustomerExist(string EmailAddress)
        {
            using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
            {
                var chkExisting = objContext.CustomerDetails.Where(a => a.EmailAddress == EmailAddress).FirstOrDefault();

                if (chkExisting != null)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

        }

        [HttpPost]
        public JsonResult ActivateCustomer(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            try
            {
                var obj = db.CustomerDetails.Find(id);
                if (obj != null)
                {
                    if (obj.IsEnabled == true)
                    {
                        obj.IsEnabled = false;
                        message = "Customer Deactivated Successfully";
                    }
                    else
                    {
                        obj.IsEnabled = true;
                        message = "Customer Activated Successfully";
                    }
                }
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
        public JsonResult TemplateSearch(string term)
        {
            try
            {
                //List<DocumentTemplate> objDoc;
                var objDoc = db.DocumentTemplates.Where(m => m.DocumentTitle.Contains(term) && m.IsEnabled).Select(s => new { DocumentTitle = s.DocumentTitle, TemplateId = s.TemplateId }).ToList();
                return Json(objDoc, JsonRequestBehavior.AllowGet);
                //return Json("true", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            { return this.Json(null, JsonRequestBehavior.AllowGet); }
        }

        public ActionResult AssociatedKeys()
        {
            return View();
        }

        public ActionResult AssociatedKeysGroup()
        {
            List<GetAssociatedKeysGroup_Result> obj = db.GetAssociatedKeysGroup().ToList();

            return View(obj.ToList());
        }

        [HttpPost]
        public PartialViewResult GetTemplateKeys(int templateID)
        {
            Session["TemplateID"] = templateID;
            var keys = (from t in db.TemplateKeysPointers
                        join k in db.TemplateKeywords on t.TemplateKeyId equals k.TemplateKeyId

                        where t.TemplateId == templateID && !(from a in db.AssociatedKeyGroups where a.TemplateID == templateID select a.KeyID).Contains(k.TemplateKeyId)

                        select new TemplateKeywordModel { TemplateKeyId = t.TemplateKeyId, TemplateKeyLabels = k.TemplateKeyLabels, TemplateKeyValue = k.TemplateKeyValue }).ToList();

            return PartialView("_TemplateKeys", keys);
        }
        public ActionResult EditAssociatedKeys(string GroupName, int TemplateID)
        {

            EditAssociatedKeygroupModel objGroup = new EditAssociatedKeygroupModel();
            List<GetAssociatedGroupKeys_Result> objList = new List<GetAssociatedGroupKeys_Result>();
            var keyData = db.AssociatedKeyGroups.Where(a => a.GroupName == GroupName && a.TemplateID == TemplateID).FirstOrDefault();
            if (keyData != null)
            {

                //int AsciicharforAutoNumber = (Convert.ToInt32(keyData.AutoNumberStartsFrom));
                //char asciivalue = Convert.ToChar(AsciicharforAutoNumber);

                objGroup.AutoNumberStartsFrom = keyData.AutoNumberStartsFrom;
                objGroup.DesignType = keyData.DesignType == "Table" ? "1" : "2";
                objGroup.DocumentTitle = db.DocumentTemplates.Where(t => t.TemplateId == TemplateID).FirstOrDefault().DocumentTitle;
                objGroup.FirstColumn = keyData.FirstColumn;
                objGroup.GroupName = keyData.GroupName;
                objList = db.GetAssociatedGroupKeys(TemplateID, GroupName).ToList();

                List<TemplateKeywordModel> objKeygroups = new List<TemplateKeywordModel>();

                if (objList != null && objList.Count > 0)
                {
                    foreach (var g in objList)
                    {
                        TemplateKeywordModel objKeyGroup = new TemplateKeywordModel();

                        objKeyGroup.Selected = g.Selected == 0 ? false : true;
                        objKeyGroup.TemplateKeyId = g.TemplateKeyId;
                        objKeyGroup.TemplateKeyLabels = g.TemplateKeyLabels;
                        objKeyGroup.TemplateKeyValue = g.TemplateKeyValue;
                        objKeyGroup.Order = g.keyorder != null ? g.keyorder.Value : 0;
                        objKeygroups.Add(objKeyGroup);
                    }
                }

                objGroup.templateKeyword = objKeygroups;
            }


            return View(objGroup);
        }
        //rakshitha
        public List<TemplateKeywordModel> getselectedValuesForAssociateTemplate(string GroupName, int TemplateID)
        {

            var keys = db.AssociatedKeyGroups.Where(u => u.GroupName == GroupName && u.TemplateID == TemplateID).ToList();
            var obj = (from t in db.TemplateKeysPointers
                       join k in db.TemplateKeywords on t.TemplateKeyId equals k.TemplateKeyId
                       where t.TemplateId == TemplateID
                       select new TemplateKeywordModel
                       {
                           TemplateKeyId = t.TemplateKeyId,
                           TemplateKeyLabels = k.TemplateKeyLabels,
                           TemplateKeyValue = k.TemplateKeyValue,
                           Selected = (keys.Where(k => k.KeyID == t.TemplateKeyId).FirstOrDefault() != null) ? true : false

                       }).ToList();

            return (obj);
        }
        //rakshitha


        [HttpPost]
        public ActionResult SaveAssociatedKeys(AssociatedKeygroupModel objTemplateKeyword)
        {
            int templateid = Convert.ToInt32(Session["TemplateID"]);
            var keys = db.AssociatedKeyGroups.Where(u => u.GroupName == objTemplateKeyword.GroupName && u.TemplateID == templateid).ToList();
            if (keys != null || keys.Count != 0)
            {
                foreach (var items in keys)
                {
                    db.AssociatedKeyGroups.Remove(items);
                }
                db.SaveChanges();
            }
            int CreatedBy = Convert.ToInt32(Session["UserId"]);
            //int templateid = Convert.ToInt32(Session["TemplateID"]);
            try
            {
                foreach (TemplateKeywordModel item in objTemplateKeyword.templateKeyword)
                {
                    if (item.Selected)
                    {
                        AssociatedKeyGroup obj = new AssociatedKeyGroup();
                        obj.TemplateID = templateid;
                        obj.GroupName = objTemplateKeyword.GroupName;
                        obj.KeyID = item.TemplateKeyId;
                        if (objTemplateKeyword.AutoNumberStartsFrom != null)
                        {
                            if (objTemplateKeyword.AutoNumberStartsFrom.Contains("."))
                            {
                                objTemplateKeyword.AutoNumberStartsFrom = objTemplateKeyword.AutoNumberStartsFrom.Replace(".", string.Empty);
                                // objTemplateKeyword.AutoNumberStartsFrom = objTemplateKeyword.AutoNumberStartsFrom.Remove(1, 1);
                            }
                            //else if (objTemplateKeyword.AutoNumberStartsFrom.Contains("#R"))
                            //{
                            //    objTemplateKeyword.AutoNumberStartsFrom = objTemplateKeyword.AutoNumberStartsFrom.Replace(".", string.Empty);
                            //    // objTemplateKeyword.AutoNumberStartsFrom = objTemplateKeyword.AutoNumberStartsFrom.Remove(1, 1);
                            //}
                        }
                        obj.AutoNumberStartsFrom = objTemplateKeyword.AutoNumberStartsFrom;
                        obj.DesignType = objTemplateKeyword.DesignType == "1" ? "Table" : "Statement";
                        obj.Statement = objTemplateKeyword.DesignType == "1" ? false : true;
                        //if (obj.DesignType == "1")
                        //    obj.Statement = true;
                        //else
                        //    obj.Statement = false;
                        obj.FirstColumn = objTemplateKeyword.FirstColumn;
                        obj.KeyLabel = item.TemplateKeyValue;
                        obj.CreatedDate = DateTime.Now;
                        obj.KeyOrder = item.Order;
                        obj.CreatedBy = CreatedBy;
                        db.AssociatedKeyGroups.Add(obj);
                        db.SaveChanges();

                    }

                }
                return RedirectToAction("AssociatedKeysGroup", "DocumentManagement");
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            //return RedirectToAction("Templates", "DocumentManagement", new { Id = objTemplates.TemplateId });
            return null;
        }
        public ActionResult DeleteKeyGroup(int id, string groupname)
        {
            var message = string.Empty;
            var keys = db.AssociatedKeyGroups.Where(u => u.GroupName == groupname && u.TemplateID == id).ToList();
            if (keys != null || keys.Count != 0)
            {
                foreach (var items in keys)
                {
                    db.AssociatedKeyGroups.Remove(items);
                }
                db.SaveChanges();
                message = "Success";
            }
            return Json(new { message = message }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult UpdateAssociatedKeys(EditAssociatedKeygroupModel objTemplateKeyword)
        {


            var keys = db.AssociatedKeyGroups.Where(u => u.GroupName == objTemplateKeyword.GroupName && u.TemplateID == objTemplateKeyword.TemplateID).ToList();
            if (keys != null || keys.Count != 0)
            {
                foreach (var items in keys)
                {
                    db.AssociatedKeyGroups.Remove(items);
                }
                db.SaveChanges();
            }
            int CreatedBy = Convert.ToInt32(Session["UserId"]);
            //int templateid = Convert.ToInt32(Session["TemplateID"]);
            try
            {
                foreach (TemplateKeywordModel item in objTemplateKeyword.templateKeyword)
                {
                    if (item.Selected)
                    {
                        AssociatedKeyGroup obj = new AssociatedKeyGroup();
                        obj.TemplateID = objTemplateKeyword.TemplateID;
                        obj.GroupName = objTemplateKeyword.GroupName;
                        obj.KeyID = item.TemplateKeyId;
                        if (objTemplateKeyword.AutoNumberStartsFrom != null)
                        {
                            //var asciibytes = Encoding.ASCII.GetBytes(objTemplateKeyword.AutoNumberStartsFrom);
                            // obj.AutoNumberStartsFrom = asciibytes[0].ToString();
                            if (objTemplateKeyword.AutoNumberStartsFrom.Contains("."))
                            {
                                obj.AutoNumberStartsFrom = objTemplateKeyword.AutoNumberStartsFrom.Replace(".", string.Empty);
                            }
                            else
                            {
                                obj.AutoNumberStartsFrom = objTemplateKeyword.AutoNumberStartsFrom;
                            }
                        }
                        obj.DesignType = objTemplateKeyword.DesignType == "1" ? "Table" : "Statement";
                        obj.Statement = objTemplateKeyword.DesignType == "1" ? false : true;
                        obj.FirstColumn = objTemplateKeyword.FirstColumn;
                        obj.KeyLabel = item.TemplateKeyValue;
                        obj.CreatedDate = DateTime.Now;
                        obj.KeyOrder = item.Order;
                        obj.CreatedBy = CreatedBy;
                        db.AssociatedKeyGroups.Add(obj);
                        db.SaveChanges();

                    }

                }
                return RedirectToAction("AssociatedKeysGroup", "DocumentManagement");
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            //return RedirectToAction("Templates", "DocumentManagement", new { Id = objTemplates.TemplateId });
            return null;
        }





        public JsonResult CustomerSearch(string term)
        {
            List<CustomerDetail> objCustomer;
            if (Session["CustHistoryID"] != null && !string.IsNullOrEmpty(Session["CustHistoryID"].ToString()))
            {
                int customerID = Convert.ToInt32(Session["CustHistoryID"]);
                Session.Remove("CustHistoryID");
                objCustomer = db.CustomerDetails.Where(m => m.CustomerId == customerID && m.IsEnabled == true).ToList<CustomerDetail>();
                return Json(objCustomer.Select(s => new { s.CustomerId, s.CustomerName }), JsonRequestBehavior.AllowGet);
            }
            else if (term != "testdataby")
            {
                int userId = Convert.ToInt32(Session["UserId"].ToString());
                // var service = db.SelectedAccountServices.Where(s => s.UserId == userId).FirstOrDefault();
                int OrgId = Convert.ToInt32(Session["OrgId"].ToString());
                // int serviceID =service.ServiceId;

                if (Convert.ToInt32(Session["RoleId"]) == 1)
                {
                    objCustomer = db.CustomerDetails.Where(m => m.EmailAddress.Contains(term) || m.CustomerName.Contains(term)).ToList<CustomerDetail>();
                }
                else
                {
                    objCustomer = db.CustomerDetails.Where(m => m.OrganizationId == OrgId && m.IsEnabled == true && (m.createdBy == userID || (roleId == 2) || (roleId == 5 && m.Department == deptID)) && (m.CustomerName.Contains(term))).ToList<CustomerDetail>();
                }
                Session.Remove("CustHistoryID");
                //objCustomer = db.CustomerDetails.Where(m => m.EmailAddress.Contains(term) && m.OrganizationId==4).ToList<CustomerDetail>();
                return Json(objCustomer.Select(s => new { s.CustomerId, s.CustomerName }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult GetCustomerDetails(int id)
        {
            CustomerDetail objCustomer;
            objCustomer = db.CustomerDetails.Where(m => m.CustomerId == id).FirstOrDefault();
            return Json(objCustomer, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Invoice

        public ActionResult InvoiceList()
        {

            List<InvoiceListModel> obj = new List<InvoiceListModel>();
            var objList = objData.GetAllInvoiceList().ToList();
            obj = (from list in objList
                   select new InvoiceListModel { PaidStatus = list.PaidStatus, groupid = list.groupid, CustomerId = list.CustomerId, CustomerName = list.CustomerName, CreatedDate = list.CreatedDate, DocumentTitle = list.DocumentTitle, InvoiceDocumentName = list.InvoiceDocumentName }).ToList();
            //List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();
            //try
            //{
            //    int userId = Convert.ToInt32(Session["UserId"]);
            //    var objFilledTemp = (from obj in db.FilledTemplateDetails
            //                         join doc in db.DocumentTemplates on obj.TemplateId equals doc.TemplateId
            //                         select new FilledFormDetailModel { DocumentTitle = doc.DocumentTitle, Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, RowId = obj.RowId , PaidStatus=obj.PaidStatus}
            //        );
            //    objForm = objFilledTemp.OrderByDescending(x => x.RowId).ToList();
            //}
            //catch (Exception ex)
            //{
            //    ErrorLog.LogThisError(ex);
            //}

            if (Convert.ToInt32(Session["RoleId"]) == 2)
            {
                int orgId = Convert.ToInt32(Session["Orgid"]);
                var objList1 = objData.getInvoiceListAcAdmin_sp(orgId, null).ToList();
                obj = (from list in objList1
                       select new InvoiceListModel { PaidStatus = list.PaidStatus, groupid = list.groupid, CustomerId = list.CustomerId, CustomerName = list.CustomerName, CreatedDate = list.CreatedDate, DocumentTitle = list.DocumentTitle, InvoiceDocumentName = list.InvoiceDocumentName }).ToList();

            }
            if (Convert.ToInt32(Session["RoleId"]) == 3)
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var objList2 = objData.getInvoiceListAcAdmin_sp(null, userId).ToList();
                obj = (from list in objList2
                       select new InvoiceListModel { PaidStatus = list.PaidStatus, groupid = list.groupid, CustomerId = list.CustomerId, CustomerName = list.CustomerName, CreatedDate = list.CreatedDate, DocumentTitle = list.DocumentTitle, InvoiceDocumentName = list.InvoiceDocumentName }).ToList();

            }

            return View(obj);
        }
        public string RandomName(string filename)
        {
            Random rnd = new Random();

            if (filename == "" || filename == null)
            {
                filename = rnd.Next(1, 999999999).ToString();
            }
            filename = "U" + filename + "T" + rnd.Next(1, 999999999) + "invoice.docx"; // Create New File with unique name
            return filename;
        }
        public List<DocumentTemplate> BindListofDocuments(int customerId, int groupId)
        {

            var objDT = db.FilledTemplateDetails.Where(dc => dc.CustomerId == customerId && dc.GroupId == groupId).ToList();


            List<DocumentTemplate> objDocumentTemp = new List<DocumentTemplate>();
            if (objDT != null && objDT.Count() > 0)
            {
                foreach (FilledTemplateDetail objFilled in objDT)
                {
                    var objdc = db.DocumentTemplates.Find(objFilled.TemplateId);
                    decimal roundvalue = Decimal.Round(Convert.ToDecimal(objdc.TemplateCost), 2);
                    objDocumentTemp.Add(new DocumentTemplate { DocumentTitle = objdc.DocumentTitle, TemplateCost = roundvalue, TemplateId = objdc.TemplateId });
                    roundvalue = 0;
                }
            }
            return objDocumentTemp;
        }
        public decimal TemplateTotalAmount(int customerId, int groupId)
        {

            var objDT = db.FilledTemplateDetails.Where(dc => dc.CustomerId == customerId && dc.GroupId == groupId).ToList();

            decimal total = 0;
            List<DocumentTemplate> objDocumentTemp = new List<DocumentTemplate>();
            if (objDT != null && objDT.Count() > 0)
            {
                foreach (FilledTemplateDetail objFilled in objDT)
                {
                    var objdc = db.DocumentTemplates.Find(objFilled.TemplateId);
                    total = total + decimal.Parse(objdc.TemplateCost.ToString());
                }
            }
            return total;
        }

        public JsonResult AutoGenerateInvoice(int? customerid, int? groupid)
        {
            try
            {
                decimal vat = 0;
                // FilledTemplateDetail obj = db.FilledTemplateDetails.Find(id);
                List<DocumentTemplate> objDocumentTemp = new List<DocumentTemplate>();
                CustomerDetail objCD = db.CustomerDetails.Find(customerid);
                objDocumentTemp = BindListofDocuments(customerid.Value, groupid.Value);
                string newFilename = "";
                newFilename = RandomName(customerid.ToString());

                string invoicepath = Path.Combine(Server.MapPath("~/Invoices/" + newFilename)); // New File Path with File Name
                var path = Path.Combine(Server.MapPath("~/Invoices/InvoiceTemplate.docx")); // Getting Original File For Create a new one with filled details
                if (System.IO.File.Exists(invoicepath))
                {
                    System.IO.File.Delete(invoicepath);
                }
                System.IO.File.Copy(path, invoicepath);
                //var sum = objDocumentTemp.Sum(t => t.TemplateCost ?? 0);
                decimal sum = TemplateTotalAmount(customerid.Value, groupid.Value);
                vat = sum * 18 / 100;
                decimal FinalAmount = vat + sum;
                string docList = DocumentListForInvoice(objDocumentTemp);
                //InvoiceInWord(invoicepath, objCD, docList, sum.ToString(), objDocumentTemp, vat, FinalAmount);//Invoice Create 
                //ConvertToPdfFile(invoicepath); // Convert to pdf file

                InvoiceDetail objInvoice = new InvoiceDetail();
                objInvoice.CreatedDate = DateTime.Now;
                objInvoice.CustomerId = customerid.Value;
                objInvoice.GroupId = groupid.Value;
                objInvoice.InvoiceDocumentName = newFilename;
                objInvoice.TotalAmount = FinalAmount;// Decimal.Round(sum, 2);

                db.InvoiceDetails.Add(objInvoice);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //public static void InvoiceInWord(string filepath, CustomerDetail objcustomer, string docList, string total, List<DocumentTemplate> objDocumentTemp, decimal vat, decimal FinalAmount)
        //{
        //    try
        //    {

        //        // Create the Word application and declare a document
        //        Application word = new Application();
        //        Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();
        //        // Define an object to pass to the API for missing parameters
        //        object missing = System.Type.Missing;

        //        try
        //        {

        //            // Everything that goes to the interop must be an object
        //            object fileName = filepath;

        //            // Open the Word document.
        //            // Pass the "missing" object defined above to all optional
        //            // parameters.  All parameters must be of type object,
        //            // and passed by reference.
        //            doc = word.Documents.Open(ref fileName,
        //                ref missing, ref missing, ref missing);//, ref missing,
        //                                                       //ref missing, ref missing, ref missing, ref missing,
        //                                                       //ref missing, ref missing, ref missing, ref missing,
        //                                                       //ref missing, ref missing, ref missing);

        //            // Activate the document
        //            doc.Activate();
        //            string[] coverKeys = new string[] { "date", "Address", "CUSTOMER NAME", "VAT", "TotalAmount", "FinalAmount" };
        //            string[] CustomerDetail = new string[6];// { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME" };
        //            CustomerDetail[0] = DateTime.Now.ToShortDateString();
        //            CustomerDetail[1] = objcustomer.Address;
        //            //CustomerDetail[2] = objcustomer.BankName;
        //            CustomerDetail[2] = objcustomer.CustomerName;
        //            //CustomerDetail[3] = docList;
        //            CustomerDetail[4] = Decimal.Round(Convert.ToDecimal(total), 2).ToString();
        //            CustomerDetail[3] = Decimal.Round(Convert.ToDecimal(vat), 2).ToString();
        //            CustomerDetail[5] = Decimal.Round(Convert.ToDecimal(FinalAmount), 2).ToString();
        //            int k = 0;
        //            foreach (string tem in coverKeys)
        //            {

        //                // Loop through the StoryRanges (sections of the Word doc)
        //                foreach (Range tmpRange in doc.StoryRanges)
        //                {
        //                    // Set the text to find and replace
        //                    tmpRange.Find.Text = "#" + tem + "#";
        //                    tmpRange.Find.Replacement.Text = CustomerDetail[k];
        //                    // Set the Find.Wrap property to continue (so it doesn't
        //                    // prompt the user or stop when it hits the end of
        //                    // the section)
        //                    tmpRange.Find.Wrap = WdFindWrap.wdFindContinue;

        //                    // Declare an object to pass as a parameter that sets
        //                    // the Replace parameter to the "wdReplaceAll" enum
        //                    object replaceAll = WdReplace.wdReplaceAll;

        //                    // Execute the Find and Replace -- notice that the
        //                    // 11th parameter is the "replaceAll" enum object
        //                    tmpRange.Find.Execute(ref missing, ref missing, ref missing,
        //                        ref missing, ref missing, ref missing, ref missing,
        //                        ref missing, ref missing, ref missing, ref replaceAll,
        //                        ref missing, ref missing, ref missing, ref missing);
        //                }
        //                k++;

        //            }

        //            int tempcount = 0;
        //            Tables tables = doc.Tables;
        //            if (tables.Count > 0)
        //            {
        //                Microsoft.Office.Interop.Word.Table table = tables[2];
        //                tempcount = objDocumentTemp.Count + 1;
        //                int rowsCount = table.Rows.Count;
        //                int coulmnsCount = table.Columns.Count;
        //                for (int i = 0; i < objDocumentTemp.Count; i++)
        //                {
        //                    tempcount = tempcount - 1;
        //                    object beforeRow = tables[2].Rows[2];
        //                    Microsoft.Office.Interop.Word.Row row = table.Rows.Add(ref beforeRow);
        //                    // Row row = table.Rows.Add(ref missing);

        //                    for (int j = 1; j <= coulmnsCount; j++)
        //                    {
        //                        if (j == 1)
        //                        {
        //                            row.Cells[j].Range.Text = tempcount.ToString();
        //                        }
        //                        else if (j == 2)
        //                        {
        //                            row.Cells[j].Range.Text = objDocumentTemp[i].DocumentTitle;
        //                        }
        //                        else if (j == 3)
        //                        {
        //                            row.Cells[j].Range.Text = objDocumentTemp[i].TemplateCost.ToString();
        //                        }
        //                        else if (j == 4)
        //                        {
        //                            row.Cells[j].Range.Text = "1";
        //                        }
        //                        else if (j == 5)
        //                        {
        //                            row.Cells[j].Range.Text = objDocumentTemp[i].TemplateCost.ToString();
        //                        }

        //                        row.Cells[j].WordWrap = true;
        //                        row.Cells[j].Range.Underline = WdUnderline.wdUnderlineNone;
        //                        row.Cells[j].Range.Bold = 0;
        //                    }
        //                }
        //            }



        //            // Save the changes
        //            doc.Save();

        //            // Close the doc and exit the app
        //            doc.Close(ref missing, ref missing, ref missing);
        //            word.Application.Quit(ref missing, ref missing, ref missing);
        //        }
        //        catch (Exception ex)
        //        {
        //            ErrorLog.LogThisError(ex);
        //            doc.Close(ref missing, ref missing, ref missing);
        //            word.Application.Quit(ref missing, ref missing, ref missing);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }

        //}

        public string DocumentListForInvoice(List<DocumentTemplate> objDocument)
        {
            string docList = "";
            int i = 0;
            foreach (DocumentTemplate dclist in objDocument)
            {
                i++;
                docList = docList + i + ". " + dclist.DocumentTitle + " - " + dclist.TemplateCost + " \r\n";
            }

            return docList;
        }

        public ActionResult ManualInvoice(int? id, int? groupId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //InvoiceDetail objInvoice = new InvoiceDetail();
            ManualInvoiceModel obj = new ManualInvoiceModel();
            //FilledTemplateDetail obj = db.FilledTemplateDetails.Find(id);
            List<DocumentTemplate> objDocumentTemp = new List<DocumentTemplate>();
            // CustomerDetail objCD = db.CustomerDetails.Find(obj.CustomerId);
            objDocumentTemp = BindListofDocuments(id.Value, groupId.Value);
            var sum = objDocumentTemp.Sum(t => t.TemplateCost ?? 0);
            List<ManualInvoiceListModel> objmanual = new List<ManualInvoiceListModel>();
            foreach (DocumentTemplate dcList in objDocumentTemp)
            {
                objmanual.Add(new ManualInvoiceListModel { DocumentTitle = dcList.DocumentTitle, DocumentCost = decimal.Parse(dcList.TemplateCost.ToString()) });
            }
            obj.getManualList = objmanual;
            obj.TotalAmount = decimal.Parse(sum.ToString());
            //string docList = DocumentListForInvoice(objDocumentTemp);
            //ViewBag.DocList = docList;
            //ViewBag.TotalAmount = sum;
            //objInvoice.CustomerId = id.Value;
            //objInvoice.GroupId = groupId.Value;
            obj.CustomerId = id.Value;
            obj.GroupId = groupId.Value;
            return View(obj);
        }
        public ActionResult GenerateInvoice(int? id, int? groupId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ManualInvoiceModel obj = new ManualInvoiceModel();

            List<DocumentTemplate> objDocumentTemp = new List<DocumentTemplate>();
            objDocumentTemp = BindListofDocuments(id.Value, groupId.Value);
            var sum = objDocumentTemp.Sum(t => t.TemplateCost ?? 0);
            List<ManualInvoiceListModel> objmanual = new List<ManualInvoiceListModel>();
            foreach (DocumentTemplate dcList in objDocumentTemp)
            {
                objmanual.Add(new ManualInvoiceListModel { DocumentTitle = dcList.DocumentTitle, Quantity = 1, TemplateId = dcList.TemplateId, DocumentCost = decimal.Parse(dcList.TemplateCost.ToString()) });
            }
            obj.getManualList = objmanual;
            obj.TotalAmount = decimal.Parse(sum.ToString());
            obj.CustomerId = id.Value;
            obj.GroupId = groupId.Value;
            return View(obj);
        }
        [HttpPost]
        public ActionResult GenerateInvoice(ManualInvoiceModel obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    FilledTemplateDetail objTem = db.FilledTemplateDetails.Where(m => m.CustomerId == obj.CustomerId && m.GroupId == obj.GroupId).FirstOrDefault();
                    List<DocumentTemplate> objDocumentTemp = new List<DocumentTemplate>();
                    List<ManualInvoiceListModel> objTempList = new List<ManualInvoiceListModel>();
                    objTempList = obj.getManualList;
                    CustomerDetail objCD = db.CustomerDetails.Find(obj.CustomerId);
                    //objDocumentTemp = BindListofDocuments(obj.CustomerId, obj.GroupId);
                    string newFilename = "";
                    decimal vat = 0;
                    newFilename = RandomName(obj.CustomerId.ToString());

                    string invoicepath = Path.Combine(Server.MapPath("~/Invoices/" + newFilename)); // New File Path with File Name
                    var path = Path.Combine(Server.MapPath("~/Resources/InvoiceTemplate.docx")); // Getting Original File For Create a new one with filled details
                    if (System.IO.File.Exists(invoicepath))
                    {
                        System.IO.File.Delete(invoicepath);
                    }
                    System.IO.File.Copy(path, invoicepath);
                    //var sum = objDocumentTemp.Sum(t => t.TemplateCost ?? 0);
                    decimal totalamt = 0;
                    decimal tempAmount = 0;
                    foreach (ManualInvoiceListModel objmanList in objTempList)
                    {
                        tempAmount = objmanList.Quantity * objmanList.DocumentCost;
                        totalamt = totalamt + tempAmount;
                    }
                    vat = totalamt * 18 / 100;
                    decimal FinalAmount = vat + totalamt;
                    string docList = DocumentListForInvoice(objDocumentTemp);
                    //InvoiceManualInWord(invoicepath, objCD, docList, totalamt.ToString(), objTempList, vat, FinalAmount);//Invoice Create 
                    //ConvertToPdfFile(invoicepath); // Convert to pdf file

                    int CreatedBy = Convert.ToInt32(Session["UserId"]);

                    InvoiceDetail objInvoice = new InvoiceDetail();

                    objInvoice.CreatedDate = DateTime.Now;
                    objInvoice.CustomerId = obj.CustomerId.Value;
                    objInvoice.GroupId = obj.GroupId.Value;
                    objInvoice.InvoiceDocumentName = newFilename;
                    objInvoice.TotalAmount = FinalAmount;// obj.TotalAmount;
                    objInvoice.CreatedBy = CreatedBy;
                    db.InvoiceDetails.Add(objInvoice);
                    db.SaveChanges();

                    try
                    {
                        foreach (ManualInvoiceListModel objmanList in objTempList)
                        {
                            //InvoicedList objDocList = new InvoicedList();
                            //objDocList.CreatedBy = CreatedBy;
                            //objDocList.CreatedDate = DateTime.Now;
                            //objDocList.InvoiceId = objInvoice.InvoiceId;
                            //objDocList.Quantity = objmanList.Quantity;
                            //objDocList.TemplateId = objmanList.TemplateId;
                            //objDocList.Title = objmanList.DocumentTitle;
                            //objDocList.Amount = objmanList.DocumentCost;
                            //db.InvoicedLists.Add(objDocList);
                        }
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.LogThisError(ex);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                }
            }
            else
            {
                return RedirectToAction("InvoiceList", "DocumentManagement");
            }

            return RedirectToAction("InvoiceList", "DocumentManagement");
        }
        //public static void InvoiceManualInWord(string filepath, CustomerDetail objcustomer, string docList, string total, List<ManualInvoiceListModel> objDocumentTemp, decimal vat, decimal FinalAmount)
        //{
        //    try
        //    {

        //        // Create the Word application and declare a document
        //        Application word = new Application();
        //        Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();
        //        // Define an object to pass to the API for missing parameters
        //        object missing = System.Type.Missing;

        //        try
        //        {

        //            // Everything that goes to the interop must be an object
        //            object fileName = filepath;

        //            // Open the Word document.
        //            // Pass the "missing" object defined above to all optional
        //            // parameters.  All parameters must be of type object,
        //            // and passed by reference.
        //            doc = word.Documents.Open(ref fileName,
        //                ref missing, ref missing, ref missing);//, ref missing,
        //                                                       //ref missing, ref missing, ref missing, ref missing,
        //                                                       //ref missing, ref missing, ref missing, ref missing,
        //                                                       //ref missing, ref missing, ref missing);

        //            // Activate the document
        //            doc.Activate();
        //            string[] coverKeys = new string[] { "date", "Address", "CUSTOMER NAME", "VAT", "TotalAmount", "FinalAmount" };
        //            string[] CustomerDetail = new string[6];// { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME" };
        //            CustomerDetail[0] = DateTime.Now.ToShortDateString();
        //            CustomerDetail[1] = objcustomer.Address;
        //            //CustomerDetail[2] = objcustomer.BankName;
        //            CustomerDetail[2] = objcustomer.CustomerName;
        //            //CustomerDetail[3] = docList;
        //            CustomerDetail[4] = Decimal.Round(Convert.ToDecimal(total), 2).ToString();
        //            CustomerDetail[3] = Decimal.Round(Convert.ToDecimal(vat), 2).ToString();
        //            CustomerDetail[5] = Decimal.Round(Convert.ToDecimal(FinalAmount), 2).ToString();
        //            int k = 0;
        //            foreach (string tem in coverKeys)
        //            {

        //                // Loop through the StoryRanges (sections of the Word doc)
        //                foreach (Range tmpRange in doc.StoryRanges)
        //                {
        //                    // Set the text to find and replace
        //                    tmpRange.Find.Text = "#" + tem + "#";
        //                    tmpRange.Find.Replacement.Text = CustomerDetail[k];
        //                    // Set the Find.Wrap property to continue (so it doesn't
        //                    // prompt the user or stop when it hits the end of
        //                    // the section)
        //                    tmpRange.Find.Wrap = WdFindWrap.wdFindContinue;

        //                    // Declare an object to pass as a parameter that sets
        //                    // the Replace parameter to the "wdReplaceAll" enum
        //                    object replaceAll = WdReplace.wdReplaceAll;

        //                    // Execute the Find and Replace -- notice that the
        //                    // 11th parameter is the "replaceAll" enum object
        //                    tmpRange.Find.Execute(ref missing, ref missing, ref missing,
        //                        ref missing, ref missing, ref missing, ref missing,
        //                        ref missing, ref missing, ref missing, ref replaceAll,
        //                        ref missing, ref missing, ref missing, ref missing);
        //                }
        //                k++;

        //            }

        //            int tempcount = 0;
        //            Tables tables = doc.Tables;
        //            if (tables.Count > 0)
        //            {
        //                Microsoft.Office.Interop.Word.Table table = tables[2];
        //                tempcount = objDocumentTemp.Count + 1;
        //                int rowsCount = table.Rows.Count;
        //                int coulmnsCount = table.Columns.Count;
        //                for (int i = 0; i < objDocumentTemp.Count; i++)
        //                {
        //                    tempcount = tempcount - 1;
        //                    object beforeRow = tables[2].Rows[2];
        //                    Microsoft.Office.Interop.Word.Row row = table.Rows.Add(ref beforeRow);
        //                    // Row row = table.Rows.Add(ref missing);

        //                    for (int j = 1; j <= coulmnsCount; j++)
        //                    {
        //                        if (j == 1)
        //                        {
        //                            row.Cells[j].Range.Text = tempcount.ToString();
        //                        }
        //                        else if (j == 2)
        //                        {
        //                            row.Cells[j].Range.Text = objDocumentTemp[i].DocumentTitle;
        //                        }
        //                        else if (j == 3)
        //                        {
        //                            row.Cells[j].Range.Text = objDocumentTemp[i].DocumentCost.ToString();
        //                        }
        //                        else if (j == 4)
        //                        {
        //                            row.Cells[j].Range.Text = objDocumentTemp[i].Quantity.ToString();
        //                        }
        //                        else if (j == 5)
        //                        {
        //                            row.Cells[j].Range.Text = (objDocumentTemp[i].Quantity * objDocumentTemp[i].DocumentCost).ToString();
        //                        }

        //                        row.Cells[j].WordWrap = true;
        //                        row.Cells[j].Range.Underline = WdUnderline.wdUnderlineNone;
        //                        row.Cells[j].Range.Bold = 0;
        //                    }
        //                }
        //            }



        //            // Save the changes
        //            doc.Save();

        //            // Close the doc and exit the app
        //            doc.Close(ref missing, ref missing, ref missing);
        //            word.Application.Quit(ref missing, ref missing, ref missing);
        //        }
        //        catch (Exception ex)
        //        {
        //            ErrorLog.LogThisError(ex);
        //            doc.Close(ref missing, ref missing, ref missing);
        //            word.Application.Quit(ref missing, ref missing, ref missing);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }

        //}

        [HttpPost]
        public ActionResult ManualInvoice(InvoiceModelDetail obj)
        {

            FilledTemplateDetail objTem = db.FilledTemplateDetails.Where(m => m.CustomerId == obj.CustomerId && m.GroupId == obj.GroupId).FirstOrDefault();
            List<DocumentTemplate> objDocumentTemp = new List<DocumentTemplate>();
            CustomerDetail objCD = db.CustomerDetails.Find(obj.CustomerId);
            objDocumentTemp = BindListofDocuments(obj.CustomerId, obj.GroupId);
            string newFilename = "";
            decimal vat = 0;
            newFilename = RandomName(obj.CustomerId.ToString());

            string invoicepath = Path.Combine(Server.MapPath("~/Invoices/" + newFilename)); // New File Path with File Name
            var path = Path.Combine(Server.MapPath("~/Invoices/InvoiceTemplate.docx")); // Getting Original File For Create a new one with filled details
            if (System.IO.File.Exists(invoicepath))
            {
                System.IO.File.Delete(invoicepath);
            }
            System.IO.File.Copy(path, invoicepath);
            //var sum = objDocumentTemp.Sum(t => t.TemplateCost ?? 0);
            decimal totalamt = 0;
            totalamt = decimal.Parse(obj.TotalAmount.ToString());
            vat = totalamt * 18 / 100;
            decimal FinalAmount = vat + totalamt;
            string docList = DocumentListForInvoice(objDocumentTemp);
            //InvoiceInWord(invoicepath, objCD, docList, obj.TotalAmount.ToString(), objDocumentTemp, vat, FinalAmount);//Invoice Create 
            //ConvertToPdfFile(invoicepath); // Convert to pdf file



            InvoiceDetail objInvoice = new InvoiceDetail();

            objInvoice.CreatedDate = DateTime.Now;
            objInvoice.CustomerId = obj.CustomerId;
            objInvoice.GroupId = obj.GroupId;
            objInvoice.InvoiceDocumentName = newFilename;
            objInvoice.TotalAmount = FinalAmount;// obj.TotalAmount;
            db.InvoiceDetails.Add(objInvoice);
            db.SaveChanges();

            return RedirectToAction("InvoiceList", "DocumentManagement");
        }

        [HttpPost]
        public JsonResult PaymentStatus(int? customerid, int? groupid, bool status)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            try
            {
                InvoiceDetail obj = new InvoiceDetail();
                obj = db.InvoiceDetails.Where(m => m.CustomerId == customerid && m.GroupId == groupid).FirstOrDefault();
                if (obj != null)
                {
                    obj.PaidStatus = status;
                }
                db.SaveChanges();
                message = "success";
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                message = "An error occured while processing the request. Try again later";
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            }

            return Json(new { message = message }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Reports
        public ActionResult Reports()
        {

            ReportsListModel obj = new ReportsListModel();
            try
            {
                List<OptionsModel> objOptions = new List<OptionsModel>();
                List<OptionsModel> objOptions1 = new List<OptionsModel>();
                VirtualAdvocateData objDataMethods = new VirtualAdvocateData();

                obj.getAllReportType = objDataMethods.getAllReportsType();
                obj.getAllOrganization = objDataMethods.getAllCompany();

                if (Convert.ToInt32(Session["RoleId"]) != 1)
                {

                    objOptions1.Add(new OptionsModel() { ID = 2 });
                    if (Convert.ToInt32(Session["RoleId"]) == 3 || Convert.ToInt32(Session["RoleId"]) == 5)
                    {
                        objOptions1.Add(new OptionsModel() { ID = 3 });
                    }
                    obj.getAllReportType = obj.getAllReportType.Where(p => !objOptions1.Any(p2 => p2.ID == p.ID));

                    obj.getAllCategory = objData.getDocumentCategoryFilteredByUser(Convert.ToInt32(Session["UserId"]));
                }
                else
                {
                    obj.getAllCategory = objData.getCategoryOptionsList();

                }

                obj.CurrentOrgId = Convert.ToInt32(Session["OrgId"]);

                if (Convert.ToInt32(Session["RoleId"]) == 2)
                {
                    obj.CurrentOrgId = Convert.ToInt32(Session["OrgId"]);
                    obj.getAllOrgUsers1 = objData.getUsersByOrganization(obj.CurrentOrgId);
                }
                else if (Convert.ToInt32(Session["RoleId"]) == 6)
                {
                    obj.getAllOrgUsers1 = objData.getUsersByDepartment(deptID, orgId); ;
                }
                else
                    obj.getAllOrgUsers1 = objOptions1;
                //obj.getSingleUserCompanyList = objDataMethods.getSingleUserCompanyList();
                obj.getSingleUserCompanyList = objData.getIndividualusrs(Convert.ToInt32(Session["UserId"]), Convert.ToInt32(Session["RoleId"]), Convert.ToInt32(Session["DepartmentID"]), Convert.ToInt32(Session["OrgId"]));
                obj.getAllOrgUsers = objOptions;

                obj.getAllSubCategory = objOptions;
                obj.getAllSubSubCategory = objOptions;
                List<GetReportData_Result> objReport = new List<GetReportData_Result>();
                obj.getReportDetails = objReport;
                obj.FromDate = DateTime.Now.AddDays(-1).Date.ToString();
                obj.ToDate = DateTime.Today.Date.ToString();
                obj.ExcelExportStatus = 0;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            return View(obj);
        }

        [HttpPost]
        public ActionResult Reports(ReportsListModel objReportPosted)
        {
            ReportsListModel obj = new ReportsListModel();
            int organizationId = 0;
            int selectedUser = 0;
            List<GetReportData_Result> data = new List<GetReportData_Result>();

            if (objReportPosted.OrgId != 0)
            {
                organizationId = objReportPosted.OrgId;
            }
            else
            {
                if (objReportPosted.CurrentOrgId != null && objReportPosted.CurrentOrgId != 0)
                    organizationId = objReportPosted.CurrentOrgId.Value;
            }

            if (organizationId != 0)
            {
                if (Convert.ToInt32(Session["RoleId"]) == 2 || Convert.ToInt32(Session["RoleId"]) == 6)
                    selectedUser = objReportPosted.OrgUserId != null ? objReportPosted.OrgUserId.Value : 0;
                else
                    selectedUser = objReportPosted.UserId != null ? objReportPosted.UserId.Value : 0;
            }
            else if (objReportPosted.IndividualUserId != 0)
            {
                selectedUser = objReportPosted.IndividualUserId;
            }
            else
            {
                if (objReportPosted.CurrentOrgId != null)
                    selectedUser = objReportPosted.CurrentOrgId.Value;
            }

            //if (objReportPosted.CurrentOrgId == null)
            //{
            //    objReportPosted.CurrentOrgId = orgId;
            //    if (roleId != 1 && objReportPosted.OrgUserId != null)
            //        selectedUser = objReportPosted.OrgUserId.Value;
            //    else
            //        selectedUser = objReportPosted.IndividualUserId;

            //}
            //else
            //{
            //    selectedUser = objReportPosted.UserId.Value;

            //}

            var from = (!string.IsNullOrEmpty(objReportPosted.FromDate)) ? DateTime.ParseExact(objReportPosted.FromDate, new string[] { "MM-dd-yyyy", "MM/dd/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None) : (Nullable<DateTime>)null;
            var to = (!string.IsNullOrEmpty(objReportPosted.ToDate)) ? DateTime.ParseExact(objReportPosted.ToDate, new string[] { "MM-dd-yyyy", "MM/dd/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None) : (Nullable<DateTime>)null;

            data = db.GetReportData(null, from, to, selectedUser, objReportPosted.DocumentCategoryId, objReportPosted.DocumentSubCategoryId, objReportPosted.DocumentSubSubCategoryId, organizationId, deptID, roleId).ToList();

            //else if ( roleId == 2)
            //    data = db.GetReportData(null, objReportPosted.FromDate, objReportPosted.ToDate,null, null, null, null, orgId, null, null).ToList();
            //else if (objReportPosted.ReportTypeId == 1 && (roleId == 3|| roleId == 5))
            //    data = db.GetReportData(null, objReportPosted.FromDate, objReportPosted.ToDate,userID, null, null, null, orgId, null, null).ToList();
            //else if (objReportPosted.ReportTypeId == 1 && roleId == 6)
            //    data = db.GetReportData(null, objReportPosted.FromDate, objReportPosted.ToDate, userID, null, null, null, orgId, deptID, null).ToList();
            //else if(objReportPosted.ReportTypeId == 2)
            //    data = db.GetReportData(null, null, null, objReportPosted.UserId, null, null, null, objReportPosted.OrgId, deptID, null).ToList();

            //else if (objReportPosted.ReportTypeId == 3  && roleId==1)
            //    data = db.GetReportData(null, null, null, objReportPosted.IndividualUserId, null, null, null, null, deptID, null).ToList();
            //else if (objReportPosted.ReportTypeId == 3 && roleId == 2)
            //    data = db.GetReportData(null, null, null, objReportPosted.OrgUserId, null, null, null, orgId, null,  null).ToList();
            //else if (objReportPosted.ReportTypeId == 3 && roleId == 6)
            //    data = db.GetReportData(null, null, null, objReportPosted.OrgUserId, null, null, null, orgId, deptID, null).ToList();
            //else if (objReportPosted.ReportTypeId == 4 && roleId == 1)
            //    data = db.GetReportData(null, null, null,null, objReportPosted.DocumentCategoryId, objReportPosted.DocumentSubCategoryId, objReportPosted.DocumentSubSubCategoryId,null, null, null).ToList();
            //else if (objReportPosted.ReportTypeId == 4 && roleId != 1)
            //    data = db.GetReportData(null, null, null, userID, objReportPosted.DocumentCategoryId, objReportPosted.DocumentSubCategoryId, objReportPosted.DocumentSubSubCategoryId,orgId, deptID, null).ToList();

            obj.getReportDetails = data;
            VirtualAdvocateData objDataMethods = new VirtualAdvocateData();

            List<GenerateReport_Result> ObjReport = new List<GenerateReport_Result>();
            List<OptionsModel> objOptions = new List<OptionsModel>();
            List<OptionsModel> objOptions1 = new List<OptionsModel>();

            obj.getAllReportType = objDataMethods.getAllReportsType(); // All Report Types
            obj.getAllOrganization = objDataMethods.getAllCompany();   // All Organization List - Super Admin                  
                                                                       //obj.getSingleUserCompanyList = objDataMethods.getSingleUserCompanyList();
            if (Convert.ToInt32(Session["RoleId"]) == 2)
            {
                obj.CurrentOrgId = Convert.ToInt32(Session["OrgId"]);
                obj.getAllOrgUsers1 = objData.getUsersByOrganization(obj.CurrentOrgId);
            }
            else if (Convert.ToInt32(Session["RoleId"]) == 6)
            {
                obj.getAllOrgUsers1 = objData.getUsersByDepartment(deptID, orgId); ;
            }
            else
                obj.getAllOrgUsers1 = objOptions1;
            //obj.getSingleUserCompanyList = objDataMethods.getSingleUserCompanyList();
            obj.getSingleUserCompanyList = objData.getIndividualusrs(Convert.ToInt32(Session["UserId"]), Convert.ToInt32(Session["RoleId"]), Convert.ToInt32(Session["DepartmentID"]), Convert.ToInt32(Session["OrgId"]));
            obj.getAllOrgUsers = objOptions;

            obj.getAllCategory = objData.getCategoryOptionsList(); // All Document Category - Super Admin


            obj.getAllSubCategory = objOptions;
            obj.getAllSubSubCategory = objOptions;

            obj.getAllOrgUsers = objOptions; // Company Users Empty List - Super Admin


            if (Convert.ToInt32(Session["RoleId"]) != 1) // Other Users
            {

                objOptions1.Add(new OptionsModel() { ID = 2 });
                if (Convert.ToInt32(Session["RoleId"]) == 3 || Convert.ToInt32(Session["RoleId"]) == 5)
                {
                    objOptions1.Add(new OptionsModel() { ID = 3 });
                }
                obj.getAllReportType = obj.getAllReportType.Where(p => !objOptions1.Any(p2 => p2.ID == p.ID));

                obj.getAllCategory = objData.getDocumentCategoryFilteredByUser(Convert.ToInt32(Session["UserId"]));
            }

            int reportType;
            obj.ExcelExportStatus = objReportPosted.ExcelExportStatus;
            reportType = objReportPosted.ReportTypeId;
            obj.ReportTypeId = reportType;
            // ObjReport = FilteredReportList(objReportPosted);



            obj.OrgId = objReportPosted.OrgId;
            obj.UserId = objReportPosted.UserId;
            obj.getAllOrgUsers = objData.getUsersByOrganization(objReportPosted.OrgId);


            if (Convert.ToInt32(Session["RoleId"]) == 2) // Company User List
            {
                obj.CurrentOrgId = objReportPosted.CurrentOrgId;
                obj.OrgUserId = objReportPosted.OrgUserId;
            }
            else if (Convert.ToInt32(Session["RoleId"]) == 1 || Convert.ToInt32(Session["RoleId"]) == 6) //  individual
            {
                obj.IndividualUserId = objReportPosted.IndividualUserId;
                obj.OrgUserId = objReportPosted.OrgUserId;
            }
            else
            {
                obj.UserId = objReportPosted.UserId;
            }




            if (Convert.ToInt32(Session["RoleId"]) == 6)
            {
                obj.getAllOrgUsers1 = objData.getUsersByDepartment(deptID, orgId); ;
            }
            else { }
            //obj.getAllOrgUsers1 = objOptions1;

            obj.IndividualUserId = objReportPosted.IndividualUserId;

            //obj.TemplateId = objReportPosted.TemplateId;
            obj.DocumentCategoryId = objReportPosted.DocumentCategoryId;
            obj.DocumentSubCategoryId = objReportPosted.DocumentSubCategoryId;
            obj.DocumentSubSubCategoryId = objReportPosted.DocumentSubSubCategoryId;

            obj.getAllSubCategory = objData.getSubCategoryOptionsList(objReportPosted.DocumentCategoryId);
            obj.getAllSubSubCategory = objData.getSubSubCategoryOptionsList(objReportPosted.DocumentSubCategoryId);


            obj.FromDate = from != null ? from.Value.Date.ToString() : string.Empty;
            obj.ToDate = to != null ? to.Value.Date.ToString() : string.Empty;
            obj.CurrentOrgId = objReportPosted.CurrentOrgId;


            ModelState.Clear();
            return View(obj);
        }

        //public ActionResult ReportsByDate(int reportType, DateTime FromDate, DateTime ToDate)
        //{
        //    List<ReportsGenerate_Result> ObjReport = new List<ReportsGenerate_Result>();
        //    try
        //    {
        //        ObjReport = objData.GenerateReportsByFilter(reportType, FromDate, ToDate, null, null,null);
        //        ExportExcelData(ObjReport);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }
        //    return Json(new { success = true });
        //}
        //public JsonResult ReportsByOrganization(int orgid, int reportType)
        //{
        //    List<ReportsGenerate_Result> ObjReport = new List<ReportsGenerate_Result>();
        //    try
        //    {
        //        ObjReport = objData.GenerateReportsByFilter(reportType, null, null, orgid, null,null);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }
        //    return Json(ObjReport, JsonRequestBehavior.AllowGet);

        //}
        //public JsonResult ReportsByDocumentType(int TemplateId, int reportType)
        //{
        //    List<ReportsGenerate_Result> ObjReport = new List<ReportsGenerate_Result>();
        //    try
        //    {
        //        ObjReport = objData.GenerateReportsByFilter(reportType, null, null, null, TemplateId,null);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }
        //    return Json(ObjReport, JsonRequestBehavior.AllowGet);

        //}

        //[HttpPost]
        //public string ReportsByOrg(int orgid, int reportType)
        //{
        //    List<ReportsGenerate_Result> ObjReport = new List<ReportsGenerate_Result>();
        //    try
        //    {
        //        ObjReport = objData.GenerateReportsByFilter(reportType, null, null, orgid, null,null);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }

        //    string strTable = "";
        //    strTable = strTable + "<thead><tr><th>#</th><th> Document Title </th><th> Document Type </th><th> CustomerName </th><th> Organization Name </th>";
        //    strTable = strTable + "<th> Date </th></tr></thead><tbody> ";

        //    int i = 0;
        //    foreach (ReportsGenerate_Result item in ObjReport)
        //    {
        //        i = i + 1;
        //        strTable = strTable + "<tr>";
        //        strTable = strTable + "<td>" + i + "</td>";
        //        strTable = strTable + "<td>" + item.DocumentTitle + "</td>";
        //        strTable = strTable + "<td>" + item.DocumentType + "</td>";
        //        strTable = strTable + "<td>" + item.OrgName + "</td>";
        //        strTable = strTable + "<td>" + item.CustomerName + "</td>";
        //        strTable = strTable + "<td>" + item.CreatedDate + "</td>";
        //        strTable = strTable + "</tr>";
        //    }
        //    strTable = strTable + "</tbody></table>";
        //    return strTable;
        //}

        public List<GenerateReport_Result> FilteredReportList(ReportsListModel objReportPosted)
        {
            int RoleId = Convert.ToInt32(Session["RoleId"]);
            int orgId;
            //if (RoleId == 2)
            //{
            orgId = Convert.ToInt32(Session["OrgId"]);
            //}
            //else
            //{
            //    orgId = objReportPosted.OrgId;
            //}

            ReportsListModel obj = new ReportsListModel();
            List<GenerateReport_Result> ObjReport = new List<GenerateReport_Result>();
            int reportType;
            reportType = objReportPosted.ReportTypeId;
            obj.ReportTypeId = reportType;
            obj.RoleId = RoleId;
            int userId;
            try
            {
                if (reportType == 1) //Date
                {
                    ObjReport = objData.GenerateReportsByFilter(RoleId, reportType, Convert.ToDateTime(objReportPosted.FromDate), Convert.ToDateTime(objReportPosted.ToDate), orgId, Convert.ToInt32(Session["UserId"]), null, null, null, Convert.ToInt32(Session["DepartmentID"]));
                }
                else if (reportType == 2) //Company
                {
                    if (objReportPosted.UserId != null)
                        ObjReport = objData.GenerateReportsByFilter(RoleId, reportType, null, null, orgId, objReportPosted.UserId.Value, null, null, null, Convert.ToInt32(Session["DepartmentID"]));

                    else

                        ObjReport = objData.GenerateReportsByFilter(RoleId, reportType, null, null, orgId, null, null, null, null, Convert.ToInt32(Session["DepartmentID"]));
                }
                else if (reportType == 3) //Individual User
                {
                    if (RoleId == 2)
                    {
                        userId = objReportPosted.OrgUserId.Value;
                    }
                    else { userId = objReportPosted.IndividualUserId; }
                    ObjReport = objData.GenerateReportsByFilter(RoleId, reportType, null, null, null, userId, null, null, null, Convert.ToInt32(Session["DepartmentID"]));
                }
                else if (reportType == 4)// Document Type
                {
                    ObjReport = objData.GenerateReportsByFilter(RoleId, reportType, null, null, orgId, Convert.ToInt32(Session["UserId"]), objReportPosted.DocumentCategoryId, objReportPosted.DocumentSubCategoryId, objReportPosted.DocumentSubSubCategoryId, Convert.ToInt32(Session["DepartmentID"]));
                }
                else
                {
                    ObjReport = objData.GenerateReportsByFilter(RoleId, reportType, null, null, orgId, Convert.ToInt32(Session["UserId"]), objReportPosted.DocumentCategoryId, objReportPosted.DocumentSubCategoryId, objReportPosted.DocumentSubSubCategoryId, Convert.ToInt32(Session["DepartmentID"]));

                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return ObjReport;

        }

        //public ActionResult ReportsExportToExcel(ReportsListModel objReportPosted)
        //{
        //    List<ReportsGenerate_Result> ObjReport = new List<ReportsGenerate_Result>();
        //    ObjReport = FilteredReportList(objReportPosted);
        //    ExportExcelData(ObjReport);
        //    return Json(new { success = true });

        //}

        //public void ExportExcelData(List<ReportsGenerate_Result> obj)
        //{

        //    StringWriter sw = new StringWriter();

        //    sw.WriteLine("\"Document Title\",\"Document Type\",\"Organization Name\",\"Customer Name\",\"Date\"");

        //    Response.ClearContent();
        //    Random rnd = new Random();

        //    string filename = "Reports" + rnd.Next(1, 999999999) + ".csv";
        //    Response.AddHeader("content-disposition", "attachment;filename="+ filename);
        //    Response.ContentType = "text/csv";

        //    foreach (ReportsGenerate_Result line in obj)
        //    {
        //        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
        //                                   line.DocumentTitle,
        //                                   line.DocumentType,
        //                                   line.OrgName,
        //                                   line.CustomerName,
        //                                   line.CreatedDate.ToString("MM/dd/yyyy")));
        //    }

        //    Response.Write(sw.ToString());
        //    Response.End();
        //}


        public ActionResult DownloadExcel(string path)
        {
            return File(path, "text/csv", path.Substring(path.LastIndexOf('\\') + 1));
        }

        public JsonResult ExcelExport(int reportTypeId, DateTime? fromDate, DateTime? toDate, int? OrguserId, int? documentCategory, int? documentsubCategory, int? documentSubSubCategory, int? companyID, int? indUser, int? companyUser)
        {

            int selectedUser;
            if (companyID == null)
            {
                companyID = orgId;
                if (roleId != 1)
                {
                    if (OrguserId != null)
                        selectedUser = OrguserId != null ? OrguserId.Value : 0;
                    else
                        selectedUser = 0;
                }
                else
                    selectedUser = indUser != null ? indUser.Value : 0;

            }
            else
            {
                selectedUser = companyUser != null ? companyUser.Value : 0;

            }
            ReportsListModel obj = new ReportsListModel();
            List<GenerateReport_Result> ObjReport = new List<GenerateReport_Result>();
            int reportType;
            reportType = reportTypeId;
            obj.ReportTypeId = reportTypeId;
            obj.RoleId = roleId;
            List<GetReportDetails_Result> data = new List<GetReportDetails_Result>();

            try
            {
                //if (reportType==1&& roleId == 1)
                //    data = db.GetReportData(null,fromDate,toDate,null,null,null,null,null,null,null).ToList();
                //else if (reportType == 1 && roleId == 2)
                //    data = db.GetReportData(null, fromDate,toDate,null, null, null, null, orgId, null, null).ToList();
                //else if (reportType == 1 && (roleId == 3|| roleId == 5))
                //    data = db.GetReportData(null, fromDate, toDate,userID, null, null, null, orgId, null, null).ToList();
                //else if (reportType == 1 && roleId == 6)
                //    data = db.GetReportData(null, fromDate, toDate, userID, null, null, null, orgId, deptID, null).ToList();
                //else if(reportType == 2)
                //    data = db.GetReportData(null, null, null,companyUser , null, null, null,companyID, deptID, null).ToList();

                //else if (reportType == 3  && roleId==1)
                //    data = db.GetReportData(null, null, null, indUser, null, null, null, null, deptID, null).ToList();
                //else if (reportType == 3 && roleId == 2)
                //    data = db.GetReportData(null, null, null, companyUser, null, null, null, orgId, null,  null).ToList();
                //else if (reportType == 3 && roleId == 6)
                //    data = db.GetReportData(null, null, null, OrguserId, null, null, null, orgId, deptID, null).ToList();
                //else if (reportType == 4 && roleId == 1)
                //    data = db.GetReportData(null, null, null,null,documentCategory, documentsubCategory, documentSubSubCategory,null, null, null).ToList();
                //else if (reportType == 4 && roleId != 1)
                data = db.GetReportDetails(null, fromDate, toDate, selectedUser, documentCategory, documentsubCategory, documentSubSubCategory, orgId, deptID, roleId).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            ListToDataTable objTable = new ListToDataTable();
            System.Data.DataTable dt = objTable.ToDataTable(data);


            dt.Columns.Remove("CustomerId");
            // dt.Columns.RemoveAt(3);
            dt.Columns.Remove("groupid");
            // dt.Columns.RemoveAt(4);
            dt.Columns.Remove("UserId");
            //dt.Columns.RemoveAt(7);
            Random rnd = new Random();
            string Filename = Server.MapPath("~/Reports/") + "Reports" + rnd.Next(1, 999999999) + ".csv";
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            System.IO.File.WriteAllText(Filename, sb.ToString());

            return Json(Filename, JsonRequestBehavior.AllowGet);



        }

        static System.Data.DataTable ConvertListToDataTable(List<string[]> list)
        {
            // New table.
            System.Data.DataTable table = new System.Data.DataTable();

            // Get max columns.
            int columns = 0;
            foreach (var array in list)
            {
                if (array.Length > columns)
                {
                    columns = array.Length;
                }
            }

            // Add columns.
            for (int i = 0; i < columns; i++)
            {
                table.Columns.Add();
            }

            // Add rows.
            foreach (var array in list)
            {
                table.Rows.Add(array);
            }

            return table;
        }

        public ActionResult GetOrganizationUsersList(int id)
        {
            List<OptionsModel> objOptions = new List<OptionsModel>();
            try
            {
                objOptions = objData.getUsersByOrganization(id);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return Json(objOptions);

        }

        public ActionResult DashBoard(int? flagForNotification)
        {
            DashBoardModel obj = new DashBoardModel();

            try
            {
                int roleId = CurrentUser.RoleId;

                List<GetTotalDocumentCountByOrganization_sp_Result> objOrgTotal = new List<GetTotalDocumentCountByOrganization_sp_Result>();
                int? userId = null;
                int? orgid = null;
                if (Convert.ToInt32(Session["RoleId"]) == 2)
                {
                    orgid = Convert.ToInt32(Session["OrgId"]);
                }
                if (Convert.ToInt32(Session["RoleId"]) != 2 && Convert.ToInt32(Session["RoleId"]) != 1)
                {
                    userId = Convert.ToInt32(Session["UserId"]);
                }
                orgid = Convert.ToInt32(Session["OrgId"]);

                obj.getDocumentCountPerOrg = objOrgTotal;
                if (Convert.ToInt32(Session["RoleId"]) == 1)
                {
                    obj.UserCount = db.UserProfiles.Where(u => u.IsEnabled == true && u.RoleId != 1).Count();
                    obj.IndividualCount = db.UserProfiles.Where(m => m.IsEnabled == true && m.HasActivated == true && m.RoleId == 3).Count();
                    //code by vaishali
                    obj.CompanyUserCount = db.OrganizationDetails.Where(m => m.IsEnabled == true && m.UserAccountsType != null).Count();
                    //obj.CompanyUserCount = db.UserProfiles.Where(m => m.IsEnabled == true && m.HasActivated == true && m.RoleId == 5).Count();
                    //end code by vaishali
                    obj.getDocumentCountPerOrg = objData.GetTotalDocumentCountByOrganization_sp(); // Total Count For Each Organization

                    obj.getOrganizationCountCategorywise = objData.GetOrganizationCountByCategoy();
                    obj.OrgCategoryChart = GetChart(obj.getOrganizationCountCategorywise.ToList<GetOrganizationCountByCategoy_Result>());

                    //obj.getCategoriesTotalCount = objData.GetCategoriesTotalCount(1);
                    //obj.getSubCategoriesTotalCount = objData.GetCategoriesTotalCount(2);
                    //obj.getSubSubCategoriesTotalCount = objData.GetCategoriesTotalCount(3);

                    obj.getCategoriesCurrnetMonthTotalCount = objData.getCategoriesCurrnetMonthTotalCount(1);
                    obj.getSubCatCurrnetMonthTotalCount = objData.getCategoriesCurrnetMonthTotalCount(2);
                    obj.getSubSubCatCurrnetMonthTotalCount = objData.getCategoriesCurrnetMonthTotalCount(3);

                    // Super Admin Get Documents Created Total count 
                    obj.getCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(1, null, null, null);
                    obj.getSubCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(2, null, null, null);
                    obj.getSubSubCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(3, null, null, null);

                    // Due Diligence Enquiries
                    obj.DueEnquiryMonthCount = objData.getNewEnquiriesDueDiligence(1);
                    obj.DueEnquiryCount = objData.getNewEnquiriesDueDiligence(null);
                    obj.displayInvoiceThisMonth = objData.getDueInvoiceThisMonth();

                    //Invoice Total Amount Categorywise
                    obj.displayCategoryInvoiceTotalAmount_Result = objData.getCategoryInvoiceTotalAmount(null);
                    obj.displayInvoiceTotalAmount = objData.getInvoiceTotalAmount(null);


                }
                if (Convert.ToInt32(Session["RoleId"]) == 2)
                {
                    obj.UserCount = db.UserProfiles.Where(m => m.OrganizationId == orgid).Count();
                    obj.MonthlyDocumentCount = objData.GetCurrentMonthCount(orgid, null, null).Value;
                    obj.TotalDocumentCountOrg = objData.GetTotalDocumentCountOrgUser_sp(orgid, null).Value;

                    // Account Admin Get Documents Created Total count 
                    obj.getCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(1, orgid, Convert.ToInt32(Session["UserId"]), null);
                    obj.getSubCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(2, orgid, Convert.ToInt32(Session["UserId"]), null);
                    obj.getSubSubCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(3, orgid, Convert.ToInt32(Session["UserId"]), null);

                    //Invoice Total Amount Categorywise
                    obj.displayCategoryInvoiceTotalAmount_Result = objData.getCategoryInvoiceTotalAmount(Convert.ToInt32(Session["UserId"]));
                    obj.displayInvoiceTotalAmount = objData.getInvoiceTotalAmount(Convert.ToInt32(Session["UserId"]));

                }

                if (Convert.ToInt32(Session["RoleId"]) == 6)
                {
                    var department = db.UserProfiles.Where(c => c.UserID == userId).FirstOrDefault().Department;
                    obj.UserCount = db.UserProfiles.Where(m => m.OrganizationId == orgid && m.Department == department).Count();
                    obj.MonthlyDocumentCount = objData.GetCurrentMonthCount(orgid, null, department).Value;
                    obj.TotalDocumentCountOrg = objData.GetTotalDocumentCountOrgUser_sp(orgid, null).Value;

                    // Account Admin Get Documents Created Total count 
                    obj.getCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(1, orgid, Convert.ToInt32(Session["UserId"]), department);
                    obj.getSubCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(2, orgid, Convert.ToInt32(Session["UserId"]), department);
                    obj.getSubSubCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(3, orgid, Convert.ToInt32(Session["UserId"]), department);

                    //Invoice Total Amount Categorywise
                    obj.displayCategoryInvoiceTotalAmount_Result = objData.getCategoryInvoiceTotalAmount(Convert.ToInt32(Session["UserId"]));
                    obj.displayInvoiceTotalAmount = objData.getInvoiceTotalAmount(Convert.ToInt32(Session["UserId"]));

                }

                if (Convert.ToInt32(Session["RoleId"]) == 3 || Convert.ToInt32(Session["RoleId"]) == 5)
                {
                    obj.MonthlyDocumentCount = objData.GetCurrentMonthCount(null, userId, null).Value;
                    obj.TotalDocumentCountOrg = objData.GetTotalDocumentCountOrgUser_sp(null, Convert.ToInt32(Session["UserId"])).Value;
                }
                if (Convert.ToInt32(Session["RoleId"]) == 3)
                {
                    //Individual User Created Document Total Count
                    obj.getCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(1, orgid, Convert.ToInt32(Session["UserId"]), null);
                    obj.getSubCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(2, orgid, Convert.ToInt32(Session["UserId"]), null);
                    obj.getSubSubCategoriesOrgUsersTotalCount = objData.getCategoriesOrgUsersTotalCount(3, orgid, Convert.ToInt32(Session["UserId"]), null);

                    //Invoice Total Amount Categorywise
                    obj.displayCategoryInvoiceTotalAmount_Result = objData.getCategoryInvoiceTotalAmount(Convert.ToInt32(Session["UserId"]));
                    obj.displayInvoiceTotalAmount = objData.getInvoiceTotalAmount(Convert.ToInt32(Session["UserId"]));
                }
                if (Convert.ToInt32(Session["RoleId"]) != 7)
                {
                    try
                    {
                        obj.getCategoriesOrgUsersCurrnetMonthTotalCount = objData.getCategoriesOrgUsersCurrnetMonthTotalCount(1, orgid, Convert.ToInt32(Session["UserId"]));
                        obj.getSubCatOrgUsersCurrnetMonthTotalCount = objData.getCategoriesOrgUsersCurrnetMonthTotalCount(2, orgid, Convert.ToInt32(Session["UserId"]));
                        obj.getSubSubCatOrgUsersCurrnetMonthTotalCount = objData.getCategoriesOrgUsersCurrnetMonthTotalCount(3, orgid, Convert.ToInt32(Session["UserId"]));
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.LogThisError(ex);
                    }
                }

                if (flagForNotification != null && flagForNotification == 1 && (roleId == 5 || roleId == 6))
                {
                    obj.Notifications = obj.GetNotificationDetails(new NotificationModel
                    {
                        DepartmentId = deptID,
                        FlatForNotification = 0,
                        RoleId = roleId,
                        OrganizationId = orgid.Value,
                        UserId = userID
                    });
                }
                else
                {
                    obj.Notifications = new List<Notification>();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View(obj);
        }

        //public ActionResult GetChart()
        //{
        //    return new System.Web.Helpers.Chart(200, 150, System.Web.Helpers.ChartTheme.Blue)
        //        .AddTitle("Number of website readers")
        //        .AddLegend()
        //        .AddSeries(
        //            name: "WebSite",
        //            chartType: "Bar",
        //            xValue: new[] { "Digg", "DZone", "DotNetKicks", "StumbleUpon" },
        //            yValues: new[] { "150000", "180000", "120000", "250000" });

        //}


        public System.Web.Helpers.Chart GetChart(List<GetOrganizationCountByCategoy_Result> obj)
        {
            List<GetOrganizationCountByCategoy_Result> objOrg = new List<GetOrganizationCountByCategoy_Result>();
            objOrg = obj;

            int i = 0;
            int totalcount = 0;
            totalcount = obj.Count;
            int[] Count = new int[totalcount];
            string[] CompanyType = new string[totalcount];
            foreach (GetOrganizationCountByCategoy_Result items in obj)
            {
                Count[i] = items.NewCount.Value;
                CompanyType[i] = items.OrganizationType;
                i++;
            }
            var key = new System.Web.Helpers.Chart(width: 600, height: 400)
                .AddSeries(name: "Organiztion Type", chartType: "column",
                xValue: CompanyType,
                yValues: Count
                );
            return key;
        }

        //public string GetOrgBarChart()
        //{

        //    List<GetOrganizationCountByCategoy_Result> objOrg = new List<GetOrganizationCountByCategoy_Result>();
        //    objOrg = objData.GetOrganizationCountByCategoy();

        //    //int i = 0;
        //    //int totalcount = 0;
        //    //totalcount = obj.Count;
        //    //int[] Count = new int[totalcount];
        //    //string[] CompanyType = new string[totalcount];
        //    List<GraphData> objGraph = new List<GraphData>();
        //    foreach (GetOrganizationCountByCategoy_Result items in objOrg)
        //    {
        //       GraphData objGr = new GraphData();
        //        objGr.label = items.OrganizationType;
        //        objGr.value= items.NewCount.Value;
        //        //Count[i] = items.NewCount.Value;
        //        //CompanyType[i] = items.OrganizationType;
        //        //i++;
        //        objGraph.Add(objGr);
        //    }

        //    var jsonSerializer = new JavaScriptSerializer();
        //    string data = jsonSerializer.Serialize(objGraph);
        //    return data;

        //    //string data = "[{y: '2006',a:100}, {y: '2007',a:75}, {y: '2008',a:50}, {y: '2009',a:75}]";
        //    //return Json(data);
        //}

        public JsonResult GetOrgBarChart()
        {
            List<GetOrganizationCountByCategoy_Result> objOrg = new List<GetOrganizationCountByCategoy_Result>();
            objOrg = objData.GetOrganizationCountByCategoy();
            var obj = objOrg;
            return Json(obj.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCompanyUsersCount()
        {
            List<getGraphMonthlyCompanyRegister_Result> objCountCompUsers = new List<getGraphMonthlyCompanyRegister_Result>();
            objCountCompUsers = objData.getGraphMonthlyCompanyRegister();
            var obj = objCountCompUsers;
            return Json(obj.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetIndividualUsersCount()
        {
            List<GraphMonthlyIndividualRegister_Result> objCountIndividualUsers = new List<GraphMonthlyIndividualRegister_Result>();
            //Individual User Count for each month 
            objCountIndividualUsers = objData.getGraphMonthlyIndividualRegister();
            var obj = objCountIndividualUsers;
            return Json(obj.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInvoiceLastFiveMonths(int? userId)
        {
            List<GraphInvoiceTotalAmount_Result> objInvoice = new List<GraphInvoiceTotalAmount_Result>();
            //Last Five Months variation of invoice
            objInvoice = objData.getGraphInvoiceTotalAmount(userId);
            var obj = objInvoice;
            return Json(obj.ToList(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Log

        public ActionResult RegistrationLog()
        {
            List<LogRegistrationList_Result> obj = new List<LogRegistrationList_Result>();
            obj = objData.LogRegistration().ToList<LogRegistrationList_Result>();
            return View(obj);
        }
        public ActionResult ViewRegistrationLog(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<ViewLogRegistration_Result1> objLog = new List<ViewLogRegistration_Result1>();
            try
            {
                objLog = objData.ViewLogRegistration(id);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objLog);
        }

        public ActionResult DueRegistrationLog()
        {
            List<LogRegistrationList_Result> obj = new List<LogRegistrationList_Result>();
            obj = objData.LogRegistration().ToList<LogRegistrationList_Result>();
            return View(obj);
        }
        public ActionResult ViewDueRegistrationLog(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<ViewLogRegistration_Result> objLog = new List<ViewLogRegistration_Result>();
            try
            {
                //   objLog = objData.ViewLogRegistration(id);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objLog);
        }

        public ActionResult CategoryLog()
        {
            List<LogDocumentCategory> obj = new List<LogDocumentCategory>();
            try
            {
                var objlog = db.LogDocumentCategories.ToList();
                obj = objlog.OrderByDescending(x => x.ModifiedDate).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View(obj);
        }

        public ActionResult subCategoryLog()
        {
            List<LogDocumentSubCategory> obj = new List<LogDocumentSubCategory>();
            try
            {
                var objlog = db.LogDocumentSubCategories.ToList();
                obj = objlog.OrderByDescending(x => x.ModifiedDate).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View(obj);
        }
        public ActionResult SubSubCategoryLog()
        {
            List<LogSubSubCategory> obj = new List<LogSubSubCategory>();
            try
            {
                var objlog = db.LogSubSubCategories.ToList();
                obj = objlog.OrderByDescending(x => x.ModifiedDate).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View(obj);
        }


        public ActionResult TemplateLog()
        {
            List<LogTemplateUpload> obj = new List<LogTemplateUpload>();
            try
            {
                var objlog = db.LogTemplateUploads.ToList();
                obj = objlog.OrderByDescending(x => x.ModifiedDate).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View(obj);
        }
        public ActionResult ViewTemplateLog(int? id, int? templateid)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<ViewLogTemplateUpload_Result> objLog = new List<ViewLogTemplateUpload_Result>();
            try
            {
                objLog = objData.ViewLogTemplateUpload(id, templateid);
                objLog = objLog.OrderByDescending(x => x.ModifiedDate).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objLog);
        }
        public ActionResult ViewLogCategory(int? id, int category)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<ViewLogCategory_Result> objLog = new List<ViewLogCategory_Result>();
            try
            {
                objLog = objData.ViewLogCategory(id, category);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objLog);
        }

        public ActionResult ViewLogSubCategory(int? id, int subcategoryid)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<ViewLogSubCategory_Result> objLog = new List<ViewLogSubCategory_Result>();
            try
            {
                objLog = objData.ViewLogSubCategory(id, subcategoryid).OrderByDescending(x => x.ModifiedDate).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objLog);
        }
        public ActionResult ViewLogSubSubCategory(int? id, int categoryid)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<ViewLogSubSubCategory_Result> objLog = new List<ViewLogSubSubCategory_Result>();
            try
            {
                objLog = objData.ViewLogSubSubCategory(id, categoryid).OrderByDescending(x => x.ModifiedDate).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objLog);
        }

        #endregion

        #region Navigation

        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Terms()
        {
            return View();
        }
        public ActionResult Policy()
        {
            return View();
        }
        #endregion

        [HttpPost]
        public JsonResult DeleteDocument(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            DeleteSingleDocument(id);
            return Json(new { message = message }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult DeleteKeyword(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            LogTemplateUpload objLog = new LogTemplateUpload();
            try
            {
                var obj = db.TemplateKeywords.Find(id);
                if (obj != null)
                {
                    db.TemplateKeywords.Remove(obj);

                }

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

        //[HttpGet]
        //public ActionResult AccountServiceList(string enable)
        //{
        //    bool active;
        //    if (string.IsNullOrEmpty(enable))
        //    {
        //        active = true;
        //        enable = "Active";
        //    }
        //    else
        //    {
        //        if (enable == "Active")
        //            active = true;
        //        else
        //            active = false;
        //    }

        //    ViewBag.Enable = enable;
        //    List<ServiceModel> objCat = new List<ServiceModel>();
        //    objCat = (from a in db.AccountServices where a.IsEnabled == active select new ServiceModel { ID = a.ServiceId, Service = a.Service, ServiceDescription = a.ServicesDescription, IsEnabled = a.IsEnabled }).ToList();
        //    return View(objCat);
        //}

        public ActionResult AddService()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult AddService(ServiceModel objCM)
        //{
        //    try
        //    {
        //        AccountService obj = new AccountService();
        //        obj.IsEnabled = true;
        //        obj.Service = objCM.Service;
        //        obj.ServicesDescription = objCM.ServiceDescription;
        //        db.AccountServices.Add(obj);
        //        db.SaveChanges();
        //        int result = obj.ServiceId;

        //        // Log Insert
        //        LogAccountService objLog = new LogAccountService();
        //        objLog.IsEnabled = true;
        //        objLog.ModifiedDate = DateTime.Now;
        //        objLog.ServiceDescription = objCM.ServiceDescription;
        //        objLog.ServiceId = result;
        //        objLog.Action = "Insert";
        //        objLog.ServiceName = objCM.Service;
        //        db.LogAccountServices.Add(objLog);
        //        db.SaveChanges();

        //        if (objCM.extraFields.Count() > 0)
        //        {

        //            foreach (ClientWiseCustomerTemplate item in objCM.extraFields)
        //            {
        //                var key = db.ClientWiseCustomerTemplates.Where(c => c.ClientID == result && c.KeyName == item.KeyName).FirstOrDefault();
        //                var customerKey = db.TemplateKeywords.Where(t => t.TemplateKeyValue == item.KeyName.Replace(" ", "_")).FirstOrDefault();
        //                if (key == null)
        //                {
        //                    ClientWiseCustomerTemplate objExtra = new ClientWiseCustomerTemplate();
        //                    objExtra.ClientID = result;
        //                    objExtra.KeyName = item.KeyName;
        //                    objExtra.Show = item.Show;

        //                    db.ClientWiseCustomerTemplates.Add(objExtra);

        //                }
        //                if (customerKey == null)
        //                {
        //                    TemplateKeyword keyObj = new TemplateKeyword();

        //                    keyObj.TemplateKeyValue = item.KeyName.Replace(" ", "_");
        //                    keyObj.TemplateKeyLabels = item.KeyName;
        //                    keyObj.MultipleKeys = false;
        //                    keyObj.IsEnabled = true;
        //                    keyObj.TemplateKeyCategory = 1;
        //                    db.TemplateKeywords.Add(keyObj);
        //                }
        //                db.SaveChanges();
        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }
        //    return RedirectToAction("AccountServiceList", "DocumentManagement");
        //}

        //[HttpGet]
        //public JsonResult CheckService(string Service)
        //{
        //    var chkExisting = db.AccountServices.Where(a => a.Service == Service.Trim()).FirstOrDefault();

        //    if (chkExisting != null)
        //    {
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpGet]
        //public ActionResult CheckServiceName(string Service)
        //{
        //    var chkExisting = db.AccountServices.Where(a => a.Service == Service.Trim()).FirstOrDefault();
        //    bool result = false;
        //    if (chkExisting != null)
        //    {
        //        result = true;
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {

        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpGet]
        //public ActionResult EditService(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AccountService dc = new AccountService();
        //    var service = db.AccountServices.Where(d => d.ServiceId == id).FirstOrDefault();
        //    EditServiceModel obj = new EditServiceModel();
        //    obj.Service = service.Service;
        //    obj.ServiceDescription = service.ServicesDescription;
        //    obj.ID = service.ServiceId;
        //    obj.extraFields = db.ClientWiseCustomerTemplates.Where(d => d.ClientID == id).ToList();
        //    return View(obj);


        //}

        //[HttpPost]
        //public ActionResult EditService(EditServiceModel obj)
        //{
        //    try
        //    {
        //        AccountService dc = new AccountService();
        //        var service = db.AccountServices.Where(d => d.ServiceId == obj.ID).FirstOrDefault();

        //        service.Service = obj.Service;
        //        service.ServicesDescription = obj.ServiceDescription;
        //        db.SaveChanges();

        //        //Log Insert
        //        LogAccountService objLog = new LogAccountService();
        //        objLog.IsEnabled = true;
        //        objLog.ModifiedDate = DateTime.Now;
        //        objLog.ServiceDescription = obj.ServiceDescription;
        //        objLog.Action = "Update";
        //        objLog.ServiceId = obj.ID;
        //        objLog.ServiceName = obj.Service;
        //        db.LogAccountServices.Add(objLog);
        //        db.SaveChanges();

        //        if (obj != null)
        //        {
        //            if (obj.extraFields.Count() > 0)
        //            {

        //                db.ClientWiseCustomerTemplates.Where(r => r.ClientID == obj.ID)
        //       .ToList().ForEach(p => db.ClientWiseCustomerTemplates.Remove(p));
        //                db.SaveChanges();

        //                foreach (ClientWiseCustomerTemplate item in obj.extraFields)
        //                {
        //                    var customerKey = db.TemplateKeywords.Where(t => t.TemplateKeyValue == item.KeyName.Replace(" ", "_")).FirstOrDefault();

        //                    var key = db.ClientWiseCustomerTemplates.Where(c => c.ClientID == obj.ID && c.KeyName == item.KeyName).FirstOrDefault();
        //                    if (key == null)
        //                    {
        //                        ClientWiseCustomerTemplate objExtra = new ClientWiseCustomerTemplate();
        //                        objExtra.ClientID = obj.ID;
        //                        objExtra.KeyName = item.KeyName;
        //                        objExtra.Show = item.Show;
        //                        db.ClientWiseCustomerTemplates.Add(objExtra);
        //                    }

        //                    if (customerKey == null)
        //                    {
        //                        TemplateKeyword keyObj = new TemplateKeyword();

        //                        keyObj.TemplateKeyValue = item.KeyName.Replace(" ", "_");
        //                        keyObj.TemplateKeyLabels = item.KeyName;
        //                        keyObj.MultipleKeys = false;
        //                        keyObj.IsEnabled = true;
        //                        keyObj.TemplateKeyCategory = 1;

        //                        db.TemplateKeywords.Add(keyObj);
        //                    }
        //                    db.SaveChanges();
        //                }

        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }
        //    return RedirectToAction("AccountServiceList", "DocumentManagement");
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //public JsonResult ActivateService(int? id)
        //{
        //    HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        //    var message = string.Empty;
        //    //Log Insert
        //    LogAccountService objLog = new LogAccountService();
        //    try
        //    {
        //        var obj = db.AccountServices.Find(id);
        //        if (obj != null)
        //        {
        //            if (obj.IsEnabled == true)
        //            {
        //                objLog.Action = "Inactive";
        //                obj.IsEnabled = false;
        //                objLog.IsEnabled = false;
        //                message = "Document Category Deactivated Successfully";

        //                var category = db.DocumentCategories.Where(d => d.ServiceId == id).ToList();

        //                category.ForEach(d => d.IsEnabled = false);


        //                foreach (DocumentCategory catObj in category)
        //                {
        //                    var subCategoryobj = db.DocumentSubCategories.Where(s => s.DocumentCategoryId == catObj.DocumentCategoryId).ToList();

        //                    subCategoryobj.ForEach((a) =>
        //                    {
        //                        a.IsEnabled = false;
        //                    });

        //                    var cateuserObj = db.DocumentTemplates.Where(d => d.DocumentCategory == catObj.DocumentCategoryId).ToList();

        //                    cateuserObj.ForEach((a) =>
        //                    {
        //                        a.IsEnabled = false;
        //                    });

        //                    var associateddocument = db.AssociateTemplateDetails.Where(s => s.AssociateTemplateId == id).ToList();
        //                    associateddocument.ForEach((a) =>
        //                    {
        //                        a.IsEnabled = false;
        //                    });

        //                    foreach (DocumentSubCategory catsubObj in subCategoryobj)
        //                    {

        //                        var subsubCategoryobj = db.DocumentSubSubCategories.Where(s => s.DocumentSubCategoryId == catsubObj.DocumentSubCategoryId).ToList();

        //                        subsubCategoryobj.ForEach((a) =>
        //                        {
        //                            a.IsEnabled = false;
        //                        });


        //                        var subuserObj = db.DocumentTemplates.Where(d => d.DocumentSubCategory == catsubObj.DocumentSubCategoryId).ToList();

        //                        subuserObj.ForEach((a) =>
        //                        {
        //                            a.IsEnabled = false;
        //                        });
        //                    }
        //                }

        //               // var userObj = db.SelectedAccountServices.Where(d => d.ServiceId == id).ToList();

        //                foreach (SelectedAccountService usrIDObj in userObj)
        //                {
        //                    var multipleservice = db.SelectedAccountServices.Where(d => d.UserId == usrIDObj.UserId).ToList();

        //                    if (multipleservice.Count == 1)
        //                    {
        //                        var usr = db.UserProfiles.Where(u => u.UserID == usrIDObj.UserId).ToList();

        //                        usr.ForEach((a) =>
        //                        {
        //                            a.IsEnabled = false;
        //                        });
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                objLog.Action = "Active";
        //                obj.IsEnabled = true;
        //                objLog.IsEnabled = true;
        //                message = "Document Category Activated Successfully";
        //            }
        //        }



        //        objLog.ServiceName = obj.Service;
        //        objLog.ServiceDescription = obj.ServicesDescription;
        //        objLog.ServiceId = obj.ServiceId;
        //        objLog.ModifiedDate = DateTime.Now;
        //        db.LogAccountServices.Add(objLog);
        //        db.SaveChanges();

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //        message = "An error occured while processing the request. Try again later";
        //        HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        //    }

        //    return Json(new { message = message }, JsonRequestBehavior.AllowGet);

        //}

        //public ActionResult ViewServiceLog(int? id, int? serviceID)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    List<ViewLogService_Result> objLog = new List<ViewLogService_Result>();
        //    try
        //    {
        //        objLog = objData.ViewLogService(id, serviceID);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }
        //    return View(objLog);
        //}

        //public ActionResult ServiceLog()
        //{
        //    List<LogAccountService> obj = new List<LogAccountService>();
        //    try
        //    {
        //        var objlog = db.LogAccountServices.ToList();
        //        obj = objlog.OrderByDescending(x => x.ModifiedDate).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }

        //    return View(obj);
        //}


        private void InsertKeysaddedBycustomer(string templateID, string[] keys,
            string customerId)
        {
            int CustId = Convert.ToInt32(customerId);
            int TemplateId = Convert.ToInt32(templateID);
            for (int i = 0; i < keys.Length; i++)
            {
                string keyword = keys[i].Replace("add_", "");
                TemplateKeyword obj = new TemplateKeyword();

                string keywordwithoutnumbers = Regex.Match(keyword, "[^0-9]+").Value;

                var labelName = db.TemplateKeywords.Where(x => keywordwithoutnumbers == x.TemplateKeyValue).FirstOrDefault();
                if (labelName != null)
                {
                    var duplicates = db.TemplateKeywords.Where(x => x.ClonedFrom == labelName.TemplateKeyId && x.IsEnabled == true).ToList();

                    if (duplicates != null)
                    {
                        foreach (var d in duplicates)
                        {
                            obj.IsEnabled = true;
                            obj.MultipleKeys = false;
                            obj.TemplateKeyLabels = d.TemplateKeyLabels;
                            obj.TemplateKeyDescription = d.TemplateKeyLabels;
                            obj.TemplateKeyValue = keyword;
                            obj.AddedByClient = true;
                            obj.Cloned = true;
                            db.TemplateKeywords.Add(obj);



                            TemplateDynamicFormValue dfcValue = db.TemplateDynamicFormValues.
                                Where(t => t.CustomerId == CustId &&
                                t.TemplateId == TemplateId &&
                                string.Compare(t.TemplateKey, keyword, true) == 0).FirstOrDefault();

                            if (dfcValue != null)
                            {
                                dfcValue.UserInputs = Request.Form[keyword];
                                dfcValue.ParentkeyId = d.TemplateKeyValue;
                                db.Entry(dfcValue).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                TemplateDynamicFormValue objDynamicFormClone = new TemplateDynamicFormValue();
                                objDynamicFormClone.TemplateId = Convert.ToInt32(templateID);
                                objDynamicFormClone.TemplateKey = keyword;
                                objDynamicFormClone.UserId = Convert.ToInt32(Session["UserId"]);
                                objDynamicFormClone.IsEnabled = true;
                                //objDynamicFormClone.UserInputs = Request.Form["add_" + keyword];
                                objDynamicFormClone.UserInputs = Request.Form[keyword];
                                objDynamicFormClone.CreatedDate = DateTime.Now;
                                objDynamicFormClone.CustomerId = Convert.ToInt32(customerId);
                                objDynamicFormClone.ParentkeyId = d.TemplateKeyValue;
                                db.TemplateDynamicFormValues.Add(objDynamicFormClone);
                                db.SaveChanges();
                            }
                        }
                    }

                    obj.IsEnabled = true;
                    obj.MultipleKeys = false;
                    obj.TemplateKeyLabels = labelName.TemplateKeyLabels;
                    obj.TemplateKeyDescription = labelName.TemplateKeyLabels;
                    obj.TemplateKeyValue = keyword;
                    obj.AddedByClient = true;
                    db.TemplateKeywords.Add(obj);

                    TemplateDynamicFormValue dfc = db.TemplateDynamicFormValues.
                                Where(t => t.CustomerId == CustId &&
                                t.TemplateId == TemplateId &&
                                string.Compare(t.TemplateKey, keyword, true) == 0)
                      .FirstOrDefault();

                    if (dfc != null)
                    {
                        dfc.UserInputs = Request.Form[keyword];
                        dfc.ParentkeyId = labelName.TemplateKeyValue;
                        db.Entry(dfc).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        TemplateDynamicFormValue objDynamicForm = new TemplateDynamicFormValue();
                        objDynamicForm.TemplateId = Convert.ToInt32(templateID);
                        objDynamicForm.TemplateKey = keyword;
                        objDynamicForm.UserId = Convert.ToInt32(Session["UserId"]);
                        objDynamicForm.IsEnabled = true;
                        //objDynamicForm.UserInputs = Request.Form["add_" + keyword];
                        objDynamicForm.UserInputs = Request.Form[keyword];
                        objDynamicForm.CreatedDate = DateTime.Now;
                        objDynamicForm.CustomerId = Convert.ToInt32(customerId);
                        objDynamicForm.ParentkeyId = labelName.TemplateKeyValue;
                        db.TemplateDynamicFormValues.Add(objDynamicForm);
                        db.SaveChanges();
                    }
                }
            }
        }


        public ActionResult Disablekey(string key)
        {
            int CommonTempId;

            CommonTempId = Convert.ToInt32(Session["CurrentTemplateId"]); // To maintain Parent Id for Associate Templates
            int? customerId = null;
            if (Request["CustomerId"] != null)
            {
                customerId = Convert.ToInt32(Request["CustomerId"]);
                Session["customerId"] = customerId;
            }
            else
            {
                customerId = Convert.ToInt32(Session["customerId"]);
            }

            TemplateDynamicFormValue obj = db.TemplateDynamicFormValues.Where(d => d.CustomerId == customerId && d.IsEnabled == true && d.TemplateId == CommonTempId && d.TemplateKey == key).FirstOrDefault();
            if (obj != null)
            {
                obj.IsEnabled = false;
                db.SaveChanges();
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult keyDetails(string key)
        {

            var keyObj = db.TemplateKeywords.Where(k => k.TemplateKeyValue == key.Replace("add_", "")).FirstOrDefault();

            if (keyObj != null && keyObj.TextArea)
                return Json(true, JsonRequestBehavior.AllowGet);
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }

        //public bool CheckService(int id)
        //{
        //   // return db.AccountServices.Where(s => s.ServiceId == id).FirstOrDefault().IsEnabled;
        //}

        public bool CheckCategory(int id)
        {
            return db.DocumentCategories.Where(s => s.DocumentCategoryId == id).FirstOrDefault().IsEnabled;
        }

        public bool CheckSubcategory(int id)
        {
            return db.DocumentSubCategories.Where(s => s.DocumentSubCategoryId == id).FirstOrDefault().IsEnabled;
        }

        public bool CheckSubSubcategory(int id)
        {
            return db.DocumentSubSubCategories.Where(s => s.DocumentSubSubCategoryId == id).FirstOrDefault().IsEnabled;
        }

        public ActionResult DocumentCategoryActivation(Int32 Id)
        {
            var serviceID = db.DocumentCategories.Where(d => d.DocumentCategoryId == Id).FirstOrDefault();


            return Json(true, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DocumentSubCategoryActivation(Int32 Id)
        {
            var categoryID = db.DocumentSubCategories.Where(d => d.DocumentSubCategoryId == Id).FirstOrDefault();

            if (CheckCategory(categoryID.DocumentCategoryId))
                return Json(true, JsonRequestBehavior.AllowGet);
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DocumentSubSubCategoryActivation(Int32 Id)
        {
            var categoryID = db.DocumentSubSubCategories.Where(d => d.DocumentSubSubCategoryId == Id).FirstOrDefault();

            if (CheckSubcategory(categoryID.DocumentSubCategoryId))
                return Json(true, JsonRequestBehavior.AllowGet);
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TemplateActivation(Int32 Id)
        {
            var template = db.DocumentTemplates.Where(d => d.TemplateId == Id).FirstOrDefault();

            if (CheckCategory(template.DocumentCategory))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else if (template.DocumentSubCategory != null && CheckSubcategory(Convert.ToInt32(template.DocumentSubCategory)))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else if (template.DocumentSubSubCategory != null && CheckSubSubcategory(Convert.ToInt32(template.DocumentSubSubCategory)))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public PartialViewResult GetPartialview(int? id, int Templateid, /* drop down value */int? DepartmentID)
        {

            List<GetAssociatedDocuments_Result> objCat = new List<GetAssociatedDocuments_Result>(); ;

            objCat = db.GetAssociatedDocuments(id, Templateid, DepartmentID).ToList();
            return PartialView("_associatedDoc", objCat);
        }

        [HttpPost]
        public JsonResult GetAssociatedDocuments(int? id, int Templateid /* drop down value */, int? DepartmentID)
        {

            List<GetAssociatedDocuments_Result> objCat = new List<GetAssociatedDocuments_Result>(); ;

            objCat = db.GetAssociatedDocuments(id, Templateid, DepartmentID).ToList();
            return Json(objCat, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SearchCategory()
        {
            List<DocumentCategory> objCat = new List<DocumentCategory>();
            objCat = db.DocumentCategories.Where(s => s.ServiceId == orgId && s.IsEnabled == true).ToList();
            return View(objCat);
        }

        public ActionResult SearchSubCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<DocumentSubCategory> objCat = new List<DocumentSubCategory>();
            objCat = objData.GetDocumentSubCategories().Where(m => m.DocumentCategoryId == id && m.IsEnabled == true).ToList();
            DocumentCategory objPCat = db.DocumentCategories.Find(id);

            if (objCat.Count > 0)
                return View(objCat);
            else
            {
                List<DocumentTemplateListModel> obj = new List<DocumentTemplateListModel>();
                obj = FillTemplate(0, id.Value, 0, 0);
                return View("SearchTemplate", obj);
            }
        }

        public ActionResult SearchSubSubCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<DocumentSubSubCategory> objCat = new List<DocumentSubSubCategory>();
            objCat = objData.GetDocumentSubSubCategories(id);

            if (objCat.Count > 0)
                return View(objCat);
            else
            {
                List<DocumentTemplateListModel> obj = new List<DocumentTemplateListModel>();
                obj = FillTemplate(0, 0, id.Value, 0);
                return View("SearchTemplate", obj);
            }
        }

        private List<DocumentTemplateListModel> FillTemplate(int orgId, int categoryId, int subCategoryID, int subsubCategoryID)
        {

            Session.Remove("ATCount");
            Session.Remove("TemplateId");
            orgId = Convert.ToInt32(Session["Orgid"]);
            Session.Remove("Displayorder");
            int userId = Convert.ToInt32(Session["UserId"]);
            Session["AssociateCount"] = 0;
            DocumentTemplateListModel objTempList = new DocumentTemplateListModel();
            try
            {
                //var objServiceId = (from SAS in db.SelectedAccountServices.Where(m => m.UserId == userId)
                //                    select SAS.ServiceId
                //              );
                int roleID = Convert.ToInt32(Session["RoleId"]);
                var department = db.UserProfiles.Where(d => d.UserID == userId).FirstOrDefault().Department;

                var objTemplates = (from ut in db.DocumentTemplates

                                    join dc in db.DocumentCategories on ut.DocumentCategory equals dc.DocumentCategoryId
                                    where ut.IsEnabled == true && ut.IsEnabled == true
                                    && (subCategoryID == 0 || ut.DocumentSubCategory == subCategoryID)
                                    && (subsubCategoryID == 0 || ut.DocumentSubSubCategory == subsubCategoryID)
                                    && (categoryId == 0 || ut.DocumentCategory == categoryId)
                                    && (((roleID != 6) && (roleID != 5)) || ((roleID == 6 && ut.DepartmentID == department) || (roleID == 5 && ut.DepartmentID == department) || roleID == 3))


                                    select new DocumentTemplateListModel { TemplateName = ut.DocumentTitle, TemplateId = ut.TemplateId, DocumentFileName = ut.TemplateFileName, DocumentCategory = dc.DocumentCategoryName, Cost = ut.TemplateCost, AssociatedDocumentId = ut.AssociateTemplateId, AssociatedDocument = null, ServiceId = dc.ServiceId, DocumentSubCategoryId = ut.DocumentSubCategory, DocumentSubSubCategoryId = ut.DocumentSubSubCategory, DocumentSubCategoryName = null, DocumentSubSubCategoryName = null }
                        );

                var test = objTemplates.ToList();
                //var objFilteredTemplate = (from FilTem in objTemplates
                //                           where objServiceId.Contains(FilTem.ServiceId)
                //                           select FilTem);
                var query = objTemplates.Select(p => new DocumentTemplateListModel
                {
                    TemplateName = p.TemplateName,
                    TemplateId = p.TemplateId,
                    DocumentFileName = p.DocumentFileName,
                    DocumentCategory = p.DocumentCategory,
                    Cost = p.Cost,
                    AssociatedDocumentId = p.AssociatedDocumentId,
                    AssociatedDocument = "", //(from utt in db.DocumentTemplates where utt.TemplateId == p.AssociatedDocumentId select utt.DocumentTitle).FirstOrDefault(),
                    ServiceId = p.ServiceId,
                    DocumentSubCategoryId = p.DocumentSubCategoryId,
                    DocumentSubSubCategoryId = p.DocumentSubSubCategoryId,
                    DocumentSubCategoryName = (from dsc in db.DocumentSubCategories where dsc.DocumentSubCategoryId == p.DocumentSubCategoryId select dsc.DocumentSubCategoryName).FirstOrDefault(),
                    DocumentSubSubCategoryName = (from dssc in db.DocumentSubSubCategories where dssc.DocumentSubSubCategoryId == p.DocumentSubSubCategoryId select dssc.SubDocumentCategoryName).FirstOrDefault()

                });
                return query.ToList();

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                return null;
            }
        }

        public ActionResult FillTemplateAction(int orgId, int categoryId, int subCategoryID, int subsubCategoryID)
        {
            orgId = Convert.ToInt32(Session["Orgid"]);
            Session.Remove("Displayorder");
            Session.Remove("TemplateId");
            int userId = Convert.ToInt32(Session["UserId"]);
            Session["AssociateCount"] = 0;
            DocumentTemplateListModel objTempList = new DocumentTemplateListModel();
            try
            {
                //var objServiceId = (from SAS in db.SelectedAccountServices.Where(m => m.UserId == userId)
                //                    select SAS.ServiceId
                //              );

                int roleID = Convert.ToInt32(Session["RoleId"]);
                var department = db.UserProfiles.Where(d => d.UserID == userId).FirstOrDefault().Department;

                var objTemplates = (from ut in db.DocumentTemplates
                                    join dc in db.DocumentCategories on ut.DocumentCategory equals dc.DocumentCategoryId
                                    where ut.IsEnabled == true && ut.IsEnabled == true
                                    && dc.ServiceId == orgId
                                    && (subCategoryID == 0 || ut.DocumentSubCategory == subCategoryID)
                                    && (subsubCategoryID == 0 || ut.DocumentSubSubCategory == subsubCategoryID)
                                    && (categoryId == 0 || ut.DocumentCategory == categoryId)
                                     && (((roleID != 6) && (roleID != 5)) || ((roleID == 6 && ut.DepartmentID == department) || (roleID == 5 && ut.DepartmentID == department)))
                                    select new DocumentTemplateListModel { TemplateName = ut.DocumentTitle, TemplateId = ut.TemplateId, DocumentFileName = ut.TemplateFileName, DocumentCategory = dc.DocumentCategoryName, Cost = ut.TemplateCost, AssociatedDocumentId = ut.AssociateTemplateId, AssociatedDocument = null, ServiceId = dc.ServiceId, DocumentSubCategoryId = ut.DocumentSubCategory, DocumentSubSubCategoryId = ut.DocumentSubSubCategory, DocumentSubCategoryName = null, DocumentSubSubCategoryName = null }
                        );
                //var objFilteredTemplate = (from FilTem in objTemplates
                //                           where objServiceId.Contains(FilTem.ServiceId)
                //                           select FilTem);
                var query = objTemplates.Select(p => new DocumentTemplateListModel
                {
                    TemplateName = p.TemplateName,
                    TemplateId = p.TemplateId,
                    DocumentFileName = p.DocumentFileName,
                    DocumentCategory = p.DocumentCategory,
                    Cost = p.Cost,
                    AssociatedDocumentId = p.AssociatedDocumentId,
                    AssociatedDocument = "", //(from utt in db.DocumentTemplates where utt.TemplateId == p.AssociatedDocumentId select utt.DocumentTitle).FirstOrDefault(),
                    ServiceId = p.ServiceId,
                    DocumentSubCategoryId = p.DocumentSubCategoryId,
                    DocumentSubSubCategoryId = p.DocumentSubSubCategoryId,
                    DocumentSubCategoryName = (from dsc in db.DocumentSubCategories where dsc.DocumentSubCategoryId == p.DocumentSubCategoryId select dsc.DocumentSubCategoryName).FirstOrDefault(),
                    DocumentSubSubCategoryName = (from dssc in db.DocumentSubSubCategories where dssc.DocumentSubSubCategoryId == p.DocumentSubSubCategoryId select dssc.SubDocumentCategoryName).FirstOrDefault()

                });
                return View("SearchTemplate", query);

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                return null;
            }
        }

        #region Archive Documents
        public ActionResult ArchivedDocument()
        {
            List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                if (Convert.ToInt32(Session["RoleId"]) == 1)  // Super Admin
                {
                    var objFilledTemp = (from obj in db.FilledTemplateDetails
                                         join doc in db.DocumentTemplates on obj.TemplateId equals doc.TemplateId
                                         where obj.ArchiveStatus == true
                                         select new FilledFormDetailModel { DocumentTitle = doc.DocumentTitle, Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, UserId = obj.UserId, RowId = obj.RowId }
                   ).OrderBy(x => x.UserId);
                    objForm = objFilledTemp.OrderByDescending(m => m.CreatedDate).ToList();
                }
                else  // Other Users
                {
                    var objFilledTemp = (from obj in db.FilledTemplateDetails
                                         join doc in db.DocumentTemplates on obj.TemplateId equals doc.TemplateId
                                         where obj.UserId == userId
                                         && obj.ArchiveStatus == true
                                         select new FilledFormDetailModel { DocumentTitle = doc.DocumentTitle, Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, RowId = obj.RowId }
                   );

                    objForm = objFilledTemp.OrderByDescending(m => m.CreatedDate).ToList();
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objForm);
        }


        [HttpPost]
        public ActionResult UnArchiveDocument(int[] ArchiveId)
        {
            try
            {
                if (ArchiveId != null && ArchiveId.Length > 0)
                {
                    for (int i = 0; i < ArchiveId.Length; i++)
                    {
                        var obj = db.FilledTemplateDetails.Find(ArchiveId[i]);

                        var existFilepath = Path.Combine(Server.MapPath("~/ArchiveDocuments/"), obj.FilledTemplateName);
                        var path1 = Path.Combine(Server.MapPath("~/TemplateFiles/"), Path.GetFileName(obj.FilledTemplateName));
                        if (System.IO.File.Exists(existFilepath))
                        {
                            System.IO.File.Copy(existFilepath, path1, true); // Existing File copy to Archive Folder
                            System.IO.File.Delete(existFilepath); //Delete Old File From TemplateFiles Folder
                        }

                        obj.ArchiveStatus = false;
                        db.SaveChanges();

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("ArchivedDocument", "DocumentManagement");
        }

        #endregion

        [HttpPost]
        public PartialViewResult AddMoreFields(int id)
        {
            CustomerTemplateDetail nwObj = new CustomerTemplateDetail();
            nwObj.id = id;

            return PartialView("_AddCustomerExtraKeys", nwObj);
        }

        [HttpPost]
        public ActionResult deleteMultipleDocuments(IEnumerable<int> docID)
        {
            if (docID != null)
            {
                foreach (var id in docID)
                {
                    DeleteSingleDocument(id);
                }
                db.SaveChanges();


            }
            else
            {
                TempData["Error"] = "Template not selected!";
            }
            return RedirectToAction("Templates");
        }

        private void DeleteSingleDocument(int? id)
        {
            LogTemplateUpload objLog = new LogTemplateUpload();
            try
            {
                var obj = db.DocumentTemplates.Find(id);
                if (obj != null)
                {
                    db.DocumentTemplates.Remove(obj);

                }
                var path = Path.Combine(Server.MapPath("~/TemplateFiles/"), obj.TemplateFileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                objLog.DocumentTitle = obj.DocumentTitle;
                objLog.DocumentDescription = obj.DocumentDescription;
                objLog.DocumentType = obj.DocumentType;
                objLog.TemplateCost = obj.TemplateCost;
                objLog.DocumentCategory = obj.DocumentCategory;
                objLog.DocumentSubCategory = obj.DocumentSubCategory;
                objLog.DocumentSubSubCategory = obj.DocumentSubSubCategory;
                objLog.AssociateTemplateId = obj.AssociateTemplateId;

                objLog.Mandatory = obj.Mandatory;
                objLog.ModifiedDate = DateTime.Now;
                objLog.TemplateFileName = obj.TemplateFileName;
                objLog.TemplateId = obj.TemplateId;
                db.LogTemplateUploads.Add(objLog);

                db.AssociateTemplateDetails.RemoveRange(db.AssociateTemplateDetails.Where(x => x.AssociateTemplateId == id));
                db.SaveChanges();


            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        public static void Logger(string msg)
        {
            string fileName = System.Configuration.ConfigurationManager.AppSettings["logPath"];

            //Checks for folder if not it will create new folder 
            if (!Directory.Exists(fileName))
                System.IO.Directory.CreateDirectory(fileName);

            //File stream
            FileStream file = new FileStream(fileName + "\\" + DateTime.Now.ToString("dd_MMMM_yyyy") + ".txt", FileMode.Append, FileAccess.Write);
            StreamWriter writer = new StreamWriter(file);

            //Writes log message
            writer.WriteLine(DateTime.Now + " " + msg);
            writer.Close();
            file.Close();
            file.Dispose();
            writer.Dispose();

        }

        //public void CreateDocument(string html, string FilePath)
        //{
        //    try
        //    {
        //        Logger("Document Creating:" + FilePath);

        //        string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
        //        html = Regex.Replace(html, re, "");
        //        html = html.Replace("<label name=", "<br><label name=");
        //        //string html = Properties.Resources.DemoHtml;

        //        ////// instantiate the html to pdf converter 
        //       // HtmlToPdf converter = new HtmlToPdf();

        //        ////// convert the url to pdf 
        //        PdfDocument doc = converter.ConvertHtmlString(html);



        //        ////// save pdf document 
        //        doc.Save(FilePath);
        //        Logger("Document Created:" + FilePath);
        //        ////// close pdf document 
        //        ////doc.Close();



        //        //using (MemoryStream generatedDocument = new MemoryStream())
        //        //{
        //        //    using (WordprocessingDocument package = WordprocessingDocument.Create(generatedDocument, WordprocessingDocumentType.Document))
        //        //    {
        //        //        MainDocumentPart mainPart = package.MainDocumentPart;
        //        //        if (mainPart == null)
        //        //        {
        //        //            mainPart = package.AddMainDocumentPart();
        //        //            new DocumentFormat.OpenXml.Wordprocessing.Document(new DocumentFormat.OpenXml.Wordprocessing.Body()).Save(mainPart);
        //        //        }

        //        //       // NotesFor.HtmlToOpenXml.HtmlConverter converter = new NotesFor.HtmlToOpenXml.HtmlConverter(mainPart);
        //        //        converter.ParseHtml(html);

        //        //        mainPart.Document.Save();
        //        //    }

        //        //    System.IO.File.WriteAllBytes(FilePath, generatedDocument.ToArray());
        //        //}

        //        //System.Diagnostics.Process.Start(FilePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger(ex.Message);
        //        Logger(ex.InnerException.Message);

        //        Logger(ex.StackTrace);

        //    }
        //}


        public PartialViewResult GetCustomerPartialView(int index = 0)
        {
            ClientWiseCustomerTemplate model = new ClientWiseCustomerTemplate();
            model.KeyID = index;
            return PartialView("_addmultiplekeyvalue", model);
        }

        public PartialViewResult GetCustomerTempltePartialView(int clientID)
        {
            var customerTemplate = db.ClientWiseCustomerTemplates.Where(c => c.ClientID == clientID).ToList();
            return PartialView("_EditCustomerTemplate", customerTemplate);
        }

        #region Dynamic Form Filling
        public ActionResult CreateDynamicCustomerForm(Int32 id)
        {

            try
            {
                //Dynamic form rows binding
                StringBuilder str = new StringBuilder();

                str.Append(DynamicCustomerFormTop(0));
                List<TemplateKeysPointer> lst = new List<TemplateKeysPointer>();
                var objcustomerkey = (from c in db.ClientWiseCustomerTemplates
                                      where c.ClientID == orgId
                                      orderby c.KeyID
                                      select new
                                      {
                                          c.KeyName,
                                          c.KeyID
                                      }).Distinct().OrderBy(x => x.KeyID
                                     );

                int keycount = 0;
                int tempkeycount = 0;
                keycount = objcustomerkey.Count() / 2;
                str.Append("<div class=col-lg-6>");
                foreach (var li in objcustomerkey)
                {
                    if (keycount == tempkeycount && objcustomerkey.Count() != 1) // Spiliting columns for two fields per row
                    {
                        str.Append("</div><div class=col-lg-6>");
                    }
                    tempkeycount = tempkeycount + 1;
                    str.Append(BuildCustomerDynamicForm(li.KeyName, li.KeyName, 0)); // Building textbox based on the key values
                }
                str.Append(BuildCustomerSubmit());
                ViewBag.Dynamic = str;
                ViewBag.Title = "Add Customer";
            }

            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View("CreateCustomerTemplate");

        }
        #endregion

        public string DynamicCustomerFormTop(Int32 custID)
        {
            string row = "";
            row = "<form class=form-horizontal  method=post action='" + Url.Content("~/DocumentManagement/FillCustomerForm?custID=" + custID) + "'><div class=row><div class=col-lg-12><div class=well bs-component>";
            return row;
        }


        public string BuildCustomerDynamicForm(string field, string label, int? customerId)
        {
            string row = "";


            //datalist = BuildCustomerDataList(field, customerId);
            var customerData = db.CustomerTemplateDetails.Where(c => c.CustID == customerId && c.FieldName == (field)).FirstOrDefault();

            if (customerData != null)
            {
                if (label == "Name")
                    row = "<div class=form-group><label>" + label + "</label><input class='form-control' id='" + field + "' name='" + field + "'required placeholder='" + label + "' type=text value='" + customerData.FieldValue + "'  list='" + field + "'></div>";
                else
                    row = "<div class=form-group><label>" + label + "</label><input class='form-control' id='" + field + "' name='" + field + "'placeholder='" + label + "' type=text value='" + customerData.FieldValue + "'  list='" + field + "'></div>";

            }
            else
            {
                if (label == "Name")
                    row = "<div class=form-group><label>" + label + "</label><input class='form-control' id='" + field + "'  name='" + field + "'required placeholder='" + label + "' type=text value='" + "" + "'  list='" + field + "'></div>";
                else
                    row = "<div class=form-group><label>" + label + "</label><input class='form-control' id='" + field + "'  name='" + field + "' placeholder='" + label + "' type=text value='" + "" + "'  list='" + field + "'></div>";

            }


            return row;
        }

        public string BuildCustomerDataList(string KeyValue, int? CustomerId)
        {
            string row = "";
            string rowval = "";
            if (CustomerId > 0)
            {
                var customerData = db.CustomerTemplateDetails.Where(c => c.CustID == CustomerId && c.FieldName.Contains(KeyValue)).FirstOrDefault();

                if (customerData != null)
                {
                    rowval = rowval + "<option value='" + customerData.FieldValue + "'>" + customerData.FieldValue + "</option>";

                }
            }
            row = "<datalist  id='" + KeyValue + "'>" + rowval + "</datalist> ";


            return row;
        }

        public string BuildCustomerSubmit()
        {
            string row = "";
            if (Session["RoleId"] != null && Session["RoleId"].ToString() == "5")
                row = "</div><div class=row><div class=col-lg-12><div class=col-md-12></div><div><button type=button value=Back class='btn btn-cancel pull-left'  onclick=location.href='" + Url.Action("GetCustomerList", "DocumentManagement", new { id = 0, enable = "Active" }) + "'>Back</button></div></div></div></ form>";
            else

                row = "</div><div class=row><div class=col-lg-12><div class=col-md-2><input class='btn btn-default' id=btnSubmit type=submit value=Submit /></div><div><button type=button value=Cancel class='btn btn-cancel'  onclick=location.href='" + Url.Action("GetCustomerList", "DocumentManagement", new { id = 0, enable = "Active" }) + "'>Cancel</button></div></div></div></ form>";
            return row;
        }


        public ActionResult FillCustomerForm(Int32 custID, FormCollection obj)
        {
            List<ClientWiseCustomerTemplate> lst = new List<ClientWiseCustomerTemplate>();

            lst = db.ClientWiseCustomerTemplates.Where(c => c.ClientID == orgId).ToList();

            try
            {
                CustomerTemplateDetail objDynamicForm = new CustomerTemplateDetail();

                if (custID > 0)
                {
                    foreach (var li in lst)
                    {

                        var existData = db.CustomerTemplateDetails.Where(c => c.CustID == custID && c.FieldName == li.KeyName).FirstOrDefault();
                        if (existData != null)
                        {
                            existData.FieldValue = Request.Form[li.KeyName];
                            existData.ClientID = orgId;
                        }
                        else
                        {
                            objDynamicForm.CustID = custID;
                            objDynamicForm.FieldID = li.KeyID;
                            objDynamicForm.FieldName = li.KeyName;
                            objDynamicForm.FieldValue = Request.Form[li.KeyName];
                            objDynamicForm.ClientID = orgId;
                            db.CustomerTemplateDetails.Add(objDynamicForm);
                        }
                        db.SaveChanges();

                    }

                    var custDetails = db.CustomerDetails.Where(c => c.CustomerId == custID).FirstOrDefault();
                    if (custDetails != null)
                    {
                        custDetails.CustomerName = Request.Form["Name"];
                        custDetails.ModifiedBy = userID;
                        custDetails.ModifiedOn = DateTime.UtcNow;
                        custDetails.Department = deptID;
                    }
                    else
                    {
                        CustomerDetail cust = new CustomerDetail();
                        cust.CustomerName = Request.Form["Name"];
                        cust.CustomerId = custID;
                        cust.IsEnabled = true;
                        cust.OrganizationId = orgId;
                        cust.CreatedOn = DateTime.UtcNow;
                        cust.createdBy = userID;
                        custDetails.Department = deptID;
                        db.CustomerDetails.Add(cust);
                    }
                    db.SaveChanges();

                }
                else
                {
                    var ID = db.CustomerTemplateDetails.Max(c => c.CustID);

                    if (ID == null)
                        ID = 1;
                    else
                        ID = ID + 1;
                    foreach (var li in lst)
                    {
                        objDynamicForm.CustID = ID;
                        objDynamicForm.FieldID = li.KeyID;
                        objDynamicForm.FieldName = li.KeyName;
                        objDynamicForm.FieldValue = Request.Form[li.KeyName];
                        objDynamicForm.ClientID = orgId;
                        db.CustomerTemplateDetails.Add(objDynamicForm);
                        db.SaveChanges();
                    }

                    CustomerDetail cust = new CustomerDetail();
                    cust.CustomerName = Request.Form["Name"];
                    cust.CustomerId = ID.Value;
                    cust.IsEnabled = true;
                    cust.CreatedOn = DateTime.UtcNow;
                    cust.createdBy = userID;
                    cust.OrganizationId = orgId;
                    cust.Department = deptID;
                    db.CustomerDetails.Add(cust);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            return RedirectToAction("GetCustomerList", "DocumentManagement");
        }


        public ActionResult GetCustomerData()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            //var service = db.SelectedAccountServices.Where(s => s.UserId == userId).FirstOrDefault();

            return View();
        }


        public ActionResult GetCustomerList(string enable)
        {
            System.Data.DataTable table = new System.Data.DataTable();

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

            var customers = db.CustomerDetails.Where(c => c.IsEnabled == active && (c.createdBy == userID || (roleId == 2) || (c.Department == deptID && roleId == 5)) && c.OrganizationId == orgId).Select(c => c.CustomerId).ToArray();


            var customerTemplate = db.ClientWiseCustomerTemplates.Where(c => c.ClientID == orgId).OrderBy(k => k.KeyID).Select(f => f.KeyName).Take(5);
            ViewBag.Enable = enable;
            foreach (var fieldName in customerTemplate)
            {
                try
                {
                    table.Columns.Add(fieldName); // columns for each language
                }
                catch { }
            }

            table.Columns.Add("CustID");  // first column

            var query = (from r in db.CustomerTemplateDetails
                         join c in db.CustomerDetails on r.CustID equals c.CustomerId
                         where r.ClientID == orgId && c.IsEnabled == active
                         && c.OrganizationId == orgId
                         && (c.createdBy == userID || (roleId == 2) || (roleId == 5 && c.Department == deptID))
                         group r by r.CustID into nameGroup
                         select new
                         {
                             Name = nameGroup.Key,
                             Values = from lang in customerTemplate
                                      join ng in nameGroup
                                           on lang equals ng.FieldName into languageGroup
                                      select new
                                      {
                                          Column = lang,
                                          Value = languageGroup.Any() ?
                                                  languageGroup.FirstOrDefault().FieldValue : null
                                      }
                         }

                        ).ToList();


            foreach (var key in query)
            {
                var row = table.NewRow();
                var items = key.Values.ToList().Select(c => c.Value); // data for columns
                                                                      //items.Insert(0,key.Name);
                                                                      // data for first column

                row.ItemArray = items.ToArray();
                row.SetField(items.Count(), key.Name);
                table.Rows.Add(row);

            }

            ViewData["CustomerData"] = table;
            if (Session["RoleId"] != null && Session["RoleId"].ToString() == "5")
            {
                return View("CudtomerDetails");
            }
            else
            {
                ViewBag.Title = "Add Customer";
                return View("CustomerTemplateData");
            }
        }

        public ActionResult EditDynamicCustomerForm(Int32 custId)
        {

            try
            {
                //Dynamic form rows binding
                StringBuilder str = new StringBuilder();

                str.Append(DynamicCustomerFormTop(custId));
                List<TemplateKeysPointer> lst = new List<TemplateKeysPointer>();
                var objcustomerkey = (from c in db.ClientWiseCustomerTemplates
                                      where c.ClientID == orgId
                                      orderby c.KeyID
                                      select new
                                      {
                                          c.KeyName,
                                          c.KeyID
                                      }).Distinct().OrderBy(x => x.KeyID
                                     );


                int keycount = 0;
                int tempkeycount = 0;
                keycount = objcustomerkey.Count() / 2;
                str.Append("<div class=col-lg-6>");
                foreach (var li in objcustomerkey)
                {
                    if (keycount == tempkeycount && objcustomerkey.Count() != 1) // Spiliting columns for two fields per row
                    {
                        str.Append("</div><div class=col-lg-6>");
                    }
                    tempkeycount = tempkeycount + 1;
                    str.Append(BuildCustomerDynamicForm(li.KeyName, li.KeyName, custId)); // Building textbox based on the key values
                }


                str.Append(BuildCustomerSubmit());
                ViewBag.Dynamic = str;
                ViewBag.Title = "Edit Customer";
            }

            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View("CreateCustomerTemplate");

        }

        public void CreateDocumentFromHiQpdf(string html, string FilePath)
        {
            try
            {
                Logger("Document Creating:" + FilePath);

                string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
                html = Regex.Replace(html, re, "");
                html = html.Replace("<label name=", "<br><label name=");

                // the base URL to resolve relative images and css
                // String thisPageUrl = this.ControllerContext.HttpContext.Request.Url.AbsoluteUri;
                // String baseUrl = thisPageUrl.Substring(0, thisPageUrl.Length -
                //     "Home/ConvertThisPageToPdf".Length);

                // // instantiate the HiQPdf HTML to PDF converter
                //HiQPdf.HtmlToPdf htmlToPdfConverter = new HiQPdf.HtmlToPdf();

                // // hide the button in the created PDF
                // //htmlToPdfConverter.HiddenHtmlElements = new string[]
                // //           { "#convertThisPageButtonDiv" };

                // // render the HTML code as PDF in memory
                // byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, baseUrl);
                byte[] bytedata = PdfSharpConvert(html);

                // send the PDF file to browsers
                System.IO.File.WriteAllBytes(FilePath.Replace(".docx", ".pdf"), bytedata);

                //ConvertTWordFile(FilePath);

            }
            catch (Exception ex)
            {
                Logger(ex.Message);
                Logger(ex.InnerException.Message);

                Logger(ex.StackTrace);

            }
        }

        public void CreateSelecHtmlPdf(string html, string filePath)
        {
            string pdf_page_size = "A4";
            int webPageWidth = 1024;
            int webPageHeight = 0;
            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize), pdf_page_size, true);
            string pdf_orientation = "Portrait";
            PdfPageOrientation pdfOrientation = (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation), pdf_orientation, true);
            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();
            // set converter options
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.WebPageWidth = webPageWidth;
            converter.Options.WebPageHeight = webPageHeight;
            converter.Options.MarginTop = converter.Options.MarginLeft = converter.Options.MarginRight = converter.Options.MarginBottom = 35;
            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertHtmlString(html, "");

            // save pdf document
            doc.Save(filePath);
            // close pdf document
            doc.Close();
        }

        public void CreateAsposeHtmlPdf(string html, string filePath)
        {
            // For complete examples and data files, please go to https://github.com/aspose-pdf/Aspose.PDF-for-.NET
            // The path to the documents directory.
            // Initialize HTMLLoadSave Options
            Aspose.Words.HtmlLoadOptions options = new Aspose.Words.HtmlLoadOptions();
            // Set Render to single page property
            //options.IsRenderToSinglePage = true;
            // Load document
            Aspose.Words.Document pdfDocument = new Aspose.Words.Document(html, options);
            // Save
            pdfDocument.Save(filePath + "RenderContentToSamePage.pdf");
        }

        public void CreateHtmlToWordFromAPI(string html, string FilePath)
        {
            string baseUrl = "http://localhost:61469";
            String attachApiPath = baseUrl + "/HtmlToWord";
            string urlParameters = "?FilePath=" + FilePath + "&html=" + "";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(attachApiPath);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                //// Parse the response body. Blocking!
                //var dataObjects = response.Content.ReadAsAsync<IEnumerable<DataObject>>().Result;
                //foreach (var d in dataObjects)
                //{
                //    Console.WriteLine("{0}", d.Name);
                //}
            }

        }


        public static void ConvertDocToHtml(object Sourcepath, object TargetPath)
        {

            Microsoft.Office.Interop.Word._Application newApp = new Microsoft.Office.Interop.Word.Application();
            Microsoft.Office.Interop.Word.Documents d = newApp.Documents;
            object Unknown = Type.Missing;
            Word.Document od = d.Open(ref Sourcepath, ref Unknown,
                                     ref Unknown, ref Unknown, ref Unknown,
                                     ref Unknown, ref Unknown, ref Unknown,
                                     ref Unknown, ref Unknown, ref Unknown,
                                     ref Unknown, ref Unknown, ref Unknown, ref Unknown);
            object format = Word.WdSaveFormat.wdFormatDocumentDefault;



            newApp.ActiveDocument.SaveAs(ref TargetPath, ref format,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown);

            newApp.Documents.Close(Word.WdSaveOptions.wdDoNotSaveChanges);


        }

        //public void CreateHtmlToWordFrom(string html, string FilePath)
        //{
        //    using (MemoryStream generatedDocument = new MemoryStream())
        //    {
        //        using (WordprocessingDocument package = WordprocessingDocument.Create(generatedDocument, WordprocessingDocumentType.Document))
        //        {
        //            MainDocumentPart mainPart = package.MainDocumentPart;
        //            if (mainPart == null)
        //            {
        //                mainPart = package.AddMainDocumentPart();
        //                new DocumentFormat.OpenXml.Wordprocessing.Document(new Body()).Save(mainPart);
        //            }

        //            HtmlConverter converter = new HtmlConverter(mainPart);
        //            Body body = mainPart.Document.Body;

        //            var paragraphs = converter.Parse(html);
        //            for (int i = 0; i < paragraphs.Count; i++)
        //            {
        //                body.Append(paragraphs[i]);
        //            }

        //            mainPart.Document.Save();
        //        }


        //        System.IO.File.WriteAllBytes(FilePath, generatedDocument.ToArray());


        //    }

        public static Byte[] PdfSharpConvert(String html)
        {
            Byte[] res = null;
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);
            //    pdf.Save(ms);
            //    res = ms.ToArray();
            //}
            //return res;

            return res = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(html);
        }

        public PartialViewResult GetPartialCustomerDetails(int customerID)
        {

            var custObj = db.ClientWiseCustomerTemplates.Where(c => c.ClientID == orgId && c.Show == true).Take(6).ToList();
            string keyName = string.Empty;
            if (custObj != null)
            {
                if (custObj.Count > 0)
                {
                    ViewBag.Item1 = custObj[0].KeyName + " :";
                    keyName = custObj[0].KeyName;
                    var value1 = db.CustomerTemplateDetails.Where(c => c.FieldName == keyName && c.CustID == customerID).FirstOrDefault();
                    ViewBag.Value1 = value1 == null ? "" : value1.FieldValue;
                    try
                    {
                        ViewBag.Item2 = custObj[1].KeyName + " :";
                        keyName = custObj[1].KeyName;
                        var value2 = db.CustomerTemplateDetails.Where(c => c.FieldName == keyName && c.CustID == customerID).FirstOrDefault();
                        ViewBag.Value2 = value2 == null ? "" : value2.FieldValue;
                    }
                    catch { }
                    try
                    {
                        ViewBag.Item3 = custObj[2].KeyName + " :";
                        keyName = custObj[2].KeyName;
                        var value3 = db.CustomerTemplateDetails.Where(c => c.FieldName == keyName && c.CustID == customerID).FirstOrDefault();
                        ViewBag.Value3 = value3 == null ? "" : value3.FieldValue;
                    }
                    catch { }
                    try
                    {
                        ViewBag.Item4 = custObj[3].KeyName + " :";
                        keyName = custObj[3].KeyName;
                        var value4 = db.CustomerTemplateDetails.Where(c => c.FieldName == keyName && c.ClientID == orgId && c.CustID == customerID).FirstOrDefault();
                        ViewBag.Value4 = value4 == null ? "" : value4.FieldValue;
                    }
                    catch { }
                    try
                    {
                        ViewBag.Item5 = custObj[4].KeyName + " :";
                        keyName = custObj[4].KeyName;
                        var value5 = db.CustomerTemplateDetails.Where(c => c.FieldName == keyName && c.ClientID == orgId && c.CustID == customerID).FirstOrDefault();
                        ViewBag.Value5 = value5 == null ? "" : value5.FieldValue;
                    }
                    catch { }
                    try
                    {
                        ViewBag.Item6 = custObj[5].KeyName + " :";
                        keyName = custObj[5].KeyName;
                        var value6 = db.CustomerTemplateDetails.Where(c => c.FieldName == keyName && c.ClientID == orgId && c.CustID == customerID).FirstOrDefault();
                        ViewBag.Value6 = value6 == null ? "" : value6.FieldValue;
                    }
                    catch { }
                }
            }
            return PartialView("_CustomerDetails");

        }

        #region BulkUpload
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost]
        public ActionResult BulkUpload()
        {
            string fname = string.Empty;
            int insertedCount = 0;
            int updatedCount = 0;
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }
                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/FileCabinet/BulkUpload/"), DateTime.Now.ToString("yyyyMMdd-HHMMss") + fname);
                        file.SaveAs(fname);
                        DataSet ds = new DataSet();
                        //A 32-bit provider which enables the use of
                        string ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fname + ";Extended Properties=Excel 12.0;";
                        using (OleDbConnection conn = new System.Data.OleDb.OleDbConnection(ConnectionString))
                        {
                            conn.Open();
                            using (System.Data.DataTable dtExcelSchema = conn.GetSchema("Tables"))
                            {
                                string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                                string query = "SELECT * FROM [" + sheetName + "]";
                                OleDbDataAdapter adapter = new OleDbDataAdapter(query, conn);
                                adapter.Fill(ds, "Customers");
                                if (ds.Tables.Count > 0)
                                {
                                    System.Data.DataTable dataTable = ds.Tables[0];
                                    dataTable = dataTable.Rows.Cast<DataRow>()
                                    .Where(row => !row.ItemArray.All(field => field is DBNull ||
                                                                   string.IsNullOrWhiteSpace(field as string)))
                                     .CopyToDataTable();
                                    string excelCustomerName = string.Empty;
                                    string excelColumnName = string.Empty;
                                    string excelColumnValue = string.Empty;
                                    int custId = db.CustomerDetails
                                                .Select(c => c.CustomerId).Max();

                                    foreach (DataRow dr in dataTable.Rows)
                                    {
                                        CustomerDetail customerDetail = null;

                                        if (dataTable.Columns.Contains("Name"))
                                        {
                                            excelCustomerName = dr["Name"].ToString().Trim();
                                            customerDetail = db.CustomerDetails.Where(c =>
                                             string.Compare(c.CustomerName.ToString(), excelCustomerName,
                                             true) == 0).FirstOrDefault();
                                        }
                                        if (customerDetail != null)
                                            updatedCount++;

                                        if (customerDetail == null &&
                                            !string.IsNullOrEmpty(excelCustomerName))
                                        {
                                            if (Session["OrgId"] != null)
                                            {
                                                customerDetail = new CustomerDetail();
                                                customerDetail.CustomerName = excelCustomerName;
                                                customerDetail.OrganizationId = Session["OrgId"].ToString().ToInteger();
                                                customerDetail.IsEnabled = true;
                                                customerDetail.CustomerId = ++custId;
                                                customerDetail.createdBy = Session["UserId"].ToString().ToInteger();
                                                customerDetail.CreatedOn = DateTime.UtcNow;
                                                insertedCount++;
                                                db.CustomerDetails.Add(customerDetail);
                                                db.SaveChanges();
                                            }
                                        }
                                        if (customerDetail != null)
                                        {
                                            foreach (DataColumn col in dataTable.Columns)
                                            {
                                                excelColumnName = col.ColumnName.Trim();
                                                excelColumnValue = dr[col.ColumnName].ToString().Trim().Replace("00:00:00", "");
                                                if (excelColumnValue.Length > 0)
                                                {
                                                    ClientWiseCustomerTemplate clientWiseCustomerTemplate
                                                    = db.ClientWiseCustomerTemplates.Where
                                                    (c => c.ClientID == customerDetail.OrganizationId &&
                                                    string.Compare(c.KeyName, excelColumnName, true) == 0)
                                                    .FirstOrDefault();
                                                    if (clientWiseCustomerTemplate == null)
                                                    {
                                                        clientWiseCustomerTemplate = new ClientWiseCustomerTemplate();
                                                        clientWiseCustomerTemplate.ClientID = customerDetail.OrganizationId;
                                                        clientWiseCustomerTemplate.KeyName = col.ColumnName.ToString().Trim();
                                                        clientWiseCustomerTemplate.Show = true;
                                                        db.ClientWiseCustomerTemplates.Add(clientWiseCustomerTemplate);
                                                        db.SaveChanges();
                                                    }
                                                    CustomerTemplateDetail customerTemplateDetail
                                                    = db.CustomerTemplateDetails.Where(c =>
                                                        c.CustID == customerDetail.CustomerId &&
                                                        c.ClientID == customerDetail.OrganizationId &&
                                                        string.Compare(c.FieldName, excelColumnName, true) == 0
                                                       ).FirstOrDefault();
                                                    if (customerTemplateDetail == null)
                                                    {
                                                        customerTemplateDetail = new CustomerTemplateDetail();
                                                        customerTemplateDetail.CustID = customerDetail.CustomerId;
                                                        customerTemplateDetail.ClientID = customerDetail.OrganizationId;
                                                        customerTemplateDetail.FieldName = col.ColumnName.ToString().Trim();
                                                        customerTemplateDetail.Show = true;
                                                        customerTemplateDetail.FieldValue = excelColumnValue;
                                                        db.CustomerTemplateDetails.Add(customerTemplateDetail);
                                                    }
                                                    else
                                                    {
                                                        customerTemplateDetail.Show = true;
                                                        customerTemplateDetail.FieldValue = excelColumnValue;
                                                        db.Entry(customerTemplateDetail).State = EntityState.Modified;
                                                    }

                                                    TemplateKeyword templateKeyword
                                                    = db.TemplateKeywords.Where
                                                    (t => string.Compare(t.TemplateKeyValue, excelColumnName, true) == 0)
                                                    .FirstOrDefault();
                                                    if (templateKeyword == null)
                                                    {
                                                        templateKeyword = new TemplateKeyword();
                                                        templateKeyword.TemplateKeyValue = excelColumnName;
                                                        templateKeyword.TemplateKeyLabels = excelColumnName;
                                                        templateKeyword.IsEnabled = true;
                                                        templateKeyword.TemplateKeyCategory = 1;
                                                        db.TemplateKeywords.Add(templateKeyword);
                                                    }

                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return Json(new
                    {
                        id = 999,
                        Message = string.Format("No of rows Inserted : <b>{0}</b><br><br>" +
                        "No of rows Updated : <b>{1}</b><br><br>", insertedCount, updatedCount)
                    }); ;
                }
                catch (Exception ex)
                {
                    return Json(new { id = 998, Message = "Error occurred. Error details: " + ex.Message });
                }
                finally
                {
                    if (System.IO.File.Exists(fname))
                    {
                        System.IO.File.Delete(fname);
                    }
                }
            }
            else
            {
                return Json(new { id = 0, Message = "No files selected." });
            }
        }
        #endregion

        #region ExcelConnection
        private string ExcelConnection(string fileName)
        {
            return @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                   @"Data Source=" + fileName + ";" +
                   @"Extended Properties=" + Convert.ToChar(34).ToString() +
                   @"Excel 8.0" + Convert.ToChar(34).ToString() + ";";
        }
        #endregion

        public JsonResult CheckCustomer(string id)
        {
            try
            {
                var cust = db.CustomerDetails.Where(n => n.CustomerName == id && n.OrganizationId == orgId && n.IsEnabled == true).FirstOrDefault();
                if (cust != null)
                    return Json("true", JsonRequestBehavior.AllowGet);
                else

                    return Json("false", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("false", JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult CheckkeyDetails(string key, string keyID)
        {
            var data = db.TemplateDynamicFormValues.Where(k => k.TemplateKey == keyID && k.UserInputs == key).FirstOrDefault();

            if (data != null)
                return Json("true", JsonRequestBehavior.AllowGet);
            else
                return Json("false", JsonRequestBehavior.AllowGet);
        }

        private string HTMLToWordSautin(string htmlContent, string outputFilePath)
        {
            try
            {
                SautinSoft.UseOffice u = new SautinSoft.UseOffice();

                //Prepare UseOffice .Net, loads MS Word in memory 
                int ret = u.InitWord();

                //Return values: 
                //0 - Loading successfully 
                //1 - Can't load MS Word® library in memory  

                //if (ret == 1)
                //    return;

                //Converting 
                ret = u.ConvertFile(htmlContent, outputFilePath, SautinSoft.UseOffice.eDirection.HTML_to_DOCX);

                //Release MS Word from memory 
                u.CloseWord();

                //0 - Converting successfully 
                //1 - Can't open input file. Check that you are using full local path to input file, URL and relative path are not supported 
                //2 - Can't create output file. Please check that you have permissions to write by this path or probably this path already used by another application 
                //3 - Converting failed, please contact with our Support Team 
                //4 - MS Office isn't installed. The component requires that any of these versions of MS Office should be installed: 2000, XP, 2003, 2007 or 2010 


                return outputFilePath;
            }
            catch (Exception ex)
            {
                Logger(ex.Message);
                Logger(ex.StackTrace);
                return string.Empty;

            }



        }

        private string WordToHTMLSautin(string path)
        {
            try
            {
                SautinSoft.UseOffice u = new SautinSoft.UseOffice();

                //Path to any local file 
                string inputFilePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
                //Path to output resulted file 
                string outputFilePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path.Replace(".docx", ".html")));

                //Prepare UseOffice .Net, loads MS Word in memory 
                int ret = u.InitWord();

                //Return values: 
                //0 - Loading successfully 
                //1 - Can't load MS Word® library in memory  

                //if (ret == 1)
                //    return;

                //Converting 
                ret = u.ConvertFile(inputFilePath, outputFilePath, SautinSoft.UseOffice.eDirection.DOC_to_HTML);

                //Release MS Word from memory 
                u.CloseWord();

                //0 - Converting successfully 
                //1 - Can't open input file. Check that you are using full local path to input file, URL and relative path are not supported 
                //2 - Can't create output file. Please check that you have permissions to write by this path or probably this path already used by another application 
                //3 - Converting failed, please contact with our Support Team 
                //4 - MS Office isn't installed. The component requires that any of these versions of MS Office should be installed: 2000, XP, 2003, 2007 or 2010 

                if (ret == 0)
                {
                    //Show produced file 
                    System.Diagnostics.Process.Start(outputFilePath);
                }

                string html = System.IO.File.ReadAllText(outputFilePath);

                return html;
            }
            catch (Exception ex)
            {
                Logger(ex.Message);
                Logger(ex.StackTrace);
                return string.Empty;

            }



        }
        public Microsoft.Office.Interop.Word.Document wordDocument { get; set; }
        public object allS { get; private set; }

        private void ConvertPdfToDoc(string filePath)
        {
            Microsoft.Office.Interop.Word.Application appWord = new Microsoft.Office.Interop.Word.Application();
            wordDocument = appWord.Documents.Open(filePath);
            wordDocument.ExportAsFixedFormat(filePath.Replace("pdf", "docx"), WdExportFormat.wdExportFormatPDF);
        }


        public static void ConvertToPdfFile(string path)
        {
            try
            {
                Application appWord = new Application();
                Microsoft.Office.Interop.Word.Document wordDocument = new Microsoft.Office.Interop.Word.Document();
                wordDocument = appWord.Documents.Open(path);
                wordDocument.ExportAsFixedFormat(path.Replace(".docx", ".pdf"), WdExportFormat.wdExportFormatPDF);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


        }

        public static void ConvertTWordFile(string path)
        {
            try
            {
                Application appWord = new Application();
                Microsoft.Office.Interop.Word.Document wordDocument = new Microsoft.Office.Interop.Word.Document();

                var wordDoc = appWord.Documents.Open(path);
                wordDoc.SaveAs2(FileName: path.Replace(".pdf", ".docx"), FileFormat: WdSaveFormat.wdFormatXMLDocument);

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
        }

        [HttpPost]
        public PartialViewResult GetReportData(ReportsListModel objReportPosted)
        {
            return PartialView("_ReportsPartial", objReportPosted);
        }

        public class FileData
        {
            public string FilePath { get; set; }
            public string html { get; set; }

        }
    }
    #endregion


}
#endregion