//using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using VirtualAdvocate.Common;
using VirtualAdvocate.DAL;
using VirtualAdvocate.Models;
using Ionic.Zip;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using System.Xml.Linq;
using Microsoft.Office.Interop.Word;

namespace VirtualAdvocate.Controllers
{
    public class MultipleDocumentDownloadController : BaseController
    {
        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();
        private VirtualAdvocateDocumentData objData = new VirtualAdvocateDocumentData();

        public  int userID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
        public  int orgId = Convert.ToInt32(System.Web.HttpContext.Current.Session["OrgId"]);
        public  int deptID = Convert.ToInt32(System.Web.HttpContext.Current.Session["DepartmentID"]);
        public  int roleId = Convert.ToInt32(System.Web.HttpContext.Current.Session["RoleId"]);

        // GET: MultipleDocumentDownload
        public ActionResult MultipleDownload(int? id)
        {
            Session.Remove("Displayorder");
         
            Session["AssociateCount"] = 0;
            DocumentTemplateListModel objTempList = new DocumentTemplateListModel();
            GetCustomerNameList();
            try
            {
                int roleID = Convert.ToInt32(Session["RoleId"]);
                int department = Convert.ToInt32(Session["DepartmentID"]);

                var objTemplates = (from ut in db.DocumentTemplates
                                    join dc in db.DocumentCategories on ut.DocumentCategory equals dc.DocumentCategoryId
                                    where ut.IsEnabled == true && dc.ServiceId==orgId
                                    && (((roleID != 6) && (roleID != 5)) || ((roleID == 6 && ut.DepartmentID == department) || (roleID == 5 && ut.DepartmentID == department) || roleID == 3))

                                    select new DocumentTemplateListModel { TemplateName = ut.DocumentTitle, TemplateId = ut.TemplateId, DocumentFileName = ut.TemplateFileName, DocumentCategory = dc.DocumentCategoryName, Cost = ut.TemplateCost, AssociatedDocumentId = ut.AssociateTemplateId, AssociatedDocument = null, ServiceId = dc.ServiceId, DocumentSubCategoryId = ut.DocumentSubCategory, DocumentSubSubCategoryId = ut.DocumentSubSubCategory, DocumentSubCategoryName = null, DocumentSubSubCategoryName = null }
                        );
            
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
            return View();
        }


        public ActionResult CreateDynamicForm(int? id)
        {
            try
            {
                string multiCustomers = string.Empty;
                var multipleCustomerids = new int[] { };
                int customer = 0;
                int CommonTempId;
                if (Session["TemplateId"] == null)
                {
                    Session["TemplateId"] = id;
                }
                CommonTempId = Convert.ToInt32(Session["TemplateId"]); // To maintain Parent Id for Associate Templates

                Session["CurrentTemplateId"] = id; // For FormConfirmation function

                string customerId = null;

                if (Request["CustomerId"] != null)
                {

                    customerId = Request["CustomerId"].ToString();
                    //Session["customerId"] = customerId;
                    string[] cusomers = customerId.ToString().TrimEnd(',').Split(',');

                    if (cusomers.Length > 0)
                    {

                        if (cusomers.Length > 1)
                        {
                            Session["MultipleCustomer"] = "Yes";

                            multipleCustomerids = new Int32[cusomers.Length];
                            string CustomerNames = string.Empty;
                            string CustomerIID = string.Empty;
                            for (int i = 0; i < cusomers.Length - 1; i++)
                            {
                                string[] customerDetails1 = cusomers[i].Split('|');

                                string[] emailAddress1 = customerDetails1[1].Split(':');

                                string email1 = emailAddress1[1].ToString().Replace(" ", "");
                                var customerID1 = db.CustomerDetails.Where(c => c.EmailAddress == email1).Select(c => c.CustomerId).FirstOrDefault();

                                CustomerNames = CustomerNames + db.CustomerDetails.Where(c => c.EmailAddress == email1).Select(c => c.CustomerName).FirstOrDefault() + ", ";

                                ViewBag.MultiName = CustomerNames.Trim(',');
                                Session["MultiName"] = ViewBag.MultiName;
                                multipleCustomerids.SetValue(customerID1, i);
                                multiCustomers = multiCustomers + customerID1.ToString() + ",";
                            }

                            Session["MultipleCustomerIDS"] = multiCustomers;

                        }
                        string[] customerDetails = cusomers[0].Split('|');

                        string[] emailAddress = customerDetails[1].Split(':');

                        string email = emailAddress[1].ToString().Replace(" ", "");
                        var customerID = db.CustomerDetails.Where(c => c.EmailAddress == email).Select(c => c.CustomerId).FirstOrDefault();

                        customer = customerID;
                        //  var customerDetails=
                        Session["customerId"] = customerID;


                    }



                }
                else
                {
                    ViewBag.MultiName = Session["MultiName"];
                    multiCustomers = (Session["MultipleCustomerIDS"]).ToString();
                }


                string AssociateName = "";
                int? associateId = null;
                int userId = Convert.ToInt32(Session["UserId"]);

                var objCurrentTemplate = db.DocumentTemplates.Find(id); // To get Parent Template Name

                AssociateName = objCurrentTemplate.DocumentTitle;
                //ViewBag.Head = "Form  " + objCurrentTemplate.DocumentTitle;



                // Checking associate template
                var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == CommonTempId && c.IsEnabled == true).OrderBy(c => c.DisplayOrder);

                if (objAssociateIds != null && objAssociateIds.Count() >= 1)
                {
                    if (Session["ATCount"] == null)
                        Session["ATCount"] = objAssociateIds.Count();    // Total Associate Count

                    //if (Session["AssociateCount"] != null && Convert.ToInt32(Session["AssociateCount"]) > 1)
                    //{
                    //    DisplayOrder = Convert.ToInt32(Session["AssociateCount"]);
                    //}

                    //var objAssociateid = objAssociateIds.Where(d => d.DisplayOrder == DisplayOrder).ToList();
                    if (objAssociateIds != null)
                    {
                        AssociateName = "Associated Documents >> ";
                        foreach (AssociateTemplateDetail item in objAssociateIds)
                        {
                            var objAssociate = db.DocumentTemplates.Find(item.AssociateTemplateId);
                            if (objAssociate != null)
                            {
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
                                //+ objAssociate.DocumentTitle;
                                //if (Convert.ToInt32(Session["AssociateCount"]) == 0)
                                //{
                                //    Session["AssociateCount"] = DisplayOrder;
                                //}


                            }
                        }

                        AssociateName = AssociateName.Remove(AssociateName.Length - 2);
                    }


                }


                // Get Filled Details For This Template if Already exist
                // bool CurrentData = false;
                string keyval = null;


                //Dynamic form rows binding
                StringBuilder str = new StringBuilder();
                str.Append("<h4 style = 'background-color: gold;padding: 8px;border-radius: 5px;margin-top: 0px'>" + objCurrentTemplate.DocumentTitle + " </h4>");
                if (Convert.ToInt32(Session["AssociateCount"]) > 0)
                {
                    str.Append(DynamicFormStepCount(Convert.ToInt32(Session["AssociateCount"]), AssociateName));
                }
                else
                {
                    str.Append(DynamicFormName(AssociateName));
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
                                          c.CategoryOrder
                                      }).Distinct().OrderBy(x => x.CategoryOrder
                                     );


                foreach (var category in objkeyCategory)
                {

                    str.Append("<div class=col-lg-12> <legend>" + category.CategoryName + "</legend></div><div class=col-lg-6>");

                    var objkey = (from c in db.KeyCategories
                                  join k in db.TemplateKeywords on c.Id equals k.TemplateKeyCategory
                                  join p in db.TemplateKeysPointers on k.TemplateKeyId equals p.TemplateKeyId
                                  where p.TemplateId == id && c.CategoryName == category.CategoryName
                                  select new
                                  {
                                      p.TemplateKeyId,
                                      p.IsEnabled,
                                      p.TemplateId,
                                      p.TemplateKeyRowId
                                  }).ToList();


                    //fetching all the keys for current template
                    var lst1 = objkey.GroupBy(p => p.TemplateKeyId)
                         .Select(grp => grp.First()).ToList();
                    int keycount = 0;
                    int tempkeycount = 0;
                    keycount = lst1.Count / 2;

                    foreach (var li in lst1)
                    {
                        var TempKeyobj = objData.getKeyDetails(li.TemplateKeyId); // Fetch Keyword Details 
                        if (TempKeyobj != null)
                        {
                            if (keycount == tempkeycount && lst1.Count != 1) // Spiliting columns for two fields per row
                            {
                                str.Append(" </div><div class=col-lg-6>");
                            }

                            var existkeyval = db.BulkTemplateValues.Where(b => b.TemplateKey == TempKeyobj.TemplateKeyValue.Trim() && b.CustomerId == multiCustomers && b.IsEnabled == true).OrderByDescending(x => x.RowId).FirstOrDefault();

                            if (existkeyval != null) // Checking for same key value Already Exists
                            {
                                keyval = existkeyval.UserInputs;
                            }
                            else { keyval = null; }
                            str.Append(BuildDynamicForm(TempKeyobj.TemplateKeyValue.Trim(), TempKeyobj.TemplateKeyLabels, keyval, multipleCustomerids)); // Building textbox based on the key values

                        }
                        tempkeycount = tempkeycount + 1;
                    }

                    if (objkeyCategory.Count() > 1)
                    {
                        str.Append("</div>");
                    }

                }
                str.Append(DynamicFormBottom(customer));

                str.Append(BuildSubmitButton(id, Convert.ToInt32(Session["OrgId"]), associateId));
                ViewBag.Dynamic = str;


                //var objkey = db.TemplateKeysPointers.Where(m => m.TemplateId == id).ToList(); //fetching all the keys for current template
                //lst = objkey.GroupBy(p => p.TemplateKeyId)
                //    .Select(grp => grp.First()).ToList();
                //int keycount = 0;
                //int tempkeycount = 0;
                //keycount = lst.Count / 2;

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            // }




            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            return View();

        }

        public string DynamicFormName(string AssociateDocName)
        {
            string row = "";
            row = "<h5 padding: 8px;border-radius: 5px;margin-top: 0px'>" + AssociateDocName + "</h5>";

            //row = "<h4 style='background-color: gold;padding: 8px;border-radius: 5px;margin-top: 0px'> Form " + AssociateDocName + "</h4>";
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
            row = "</div><div class=row><div class=col-lg-12><div class=col-md-2><input type=hidden value=" + associateId + " name=AssociateTemplateId /><input type=hidden value=" + id + " name=TemplateId /> <input class='btn btn-default' id=btnSubmit type=submit value=Submit /></div><div><button type=button value=Cancel class='btn btn-cancel'  onclick=location.href='" + Url.Action("SearchTemplate", "DocumentManagement", new { id = orgId }) + "'>Cancel</button></div></div></div></ form>";
            return row;
        }

        public string BuildDataList(string KeyValue, int[] CustomerId)
        {
            string row = "";
            string rowval = "";

            var objkeyInputs = db.TemplateDynamicFormValues.Where(m => m.TemplateKey == KeyValue && CustomerId.Contains(m.CustomerId)).ToList();
            if (objkeyInputs != null && objkeyInputs.Count != 0)
            {
                var lst = objkeyInputs.Select(p => p.UserInputs).Distinct().ToList();
                foreach (var item in lst)
                {
                    rowval = rowval + "<option value='" + item + "'>" + item + "</option>";
                }
                row = "<datalist  id='" + KeyValue + "'>" + rowval + "</datalist> ";
            }

            return row;
        }

        public string BuildDynamicForm(string field, string label, string value, int[] customerId)
        {
            string row = "";
            string datalist = "";
            datalist = BuildDataList(field, customerId);

            row = "<div class=form-group id=div_" + field + "><label class=col-lg-4 control-label>" + label + "</label><div class=col-lg-6><input class=form-control name='" + field + "' placeholder='" + label + "' type=text value='" + value + "'  list='" + field + "'>" + datalist + "</div></div>";

            return row;
        }

        public string DynamicFormStepCount(int stepCount, string AssociateDocName)
        {
            string row = "";
            row = "<h5 padding: 8px;border-radius: 5px;margin-top: 0px'>" + AssociateDocName + "</h5>";

            //row = "<h4 style='background-color: gold;padding: 8px;border-radius: 5px;margin-top: 0px'>STEP " + stepCount + " :  " + AssociateDocName + "</h4>";
            return row;
        }
        public string DynamicFormTop()
        {
            string row = "";
            row = "<form class=form-horizontal  method=post action='" + Url.Content("~/MultipleDocumentDownload/FillDynamicForm/") + "'><div class=row><div class=col-lg-12><div class=well bs-component><fieldset><div class=row>";
            return row;
        }

        public ActionResult FillDynamicForm(FormCollection obj)
        {
            List<TemplateKeysPointer> lst = new List<TemplateKeysPointer>();
            string customerid;

            customerid = Session["MultipleCustomerIDS"].ToString();
            int id = 0;
            int? associateId = null;
            try
            {


                if (Request.Form["TemplateId"] != null)
                    id = Convert.ToInt32(Request.Form["TemplateId"]);

                if (Request.Form["AssociateTemplateId"] != null)
                    associateId = Convert.ToInt32(Request.Form["AssociateTemplateId"]);

                int userId = Convert.ToInt32(Session["UserId"]);

                // Get Already Filled Details For This Template
                bool ExistsData = false;
                var objAlreadyFilled = db.BulkTemplateValues.Where(a => a.TemplateId == id && a.UserId == userId && a.IsEnabled == true && a.CustomerId == customerid);
                if (objAlreadyFilled != null && objAlreadyFilled.Count() > 0)
                {
                    ExistsData = true;
                }

                var objkey = db.TemplateKeysPointers.Where(m => m.TemplateId == id).ToList();
                lst = objkey.GroupBy(p => p.TemplateKeyId)
                    .Select(grp => grp.First()).ToList();

                BulkTemplateValue objDynamicForm = new BulkTemplateValue();
                foreach (var li in lst)
                {
                    var TempKeyobj = objData.getKeyDetails(li.TemplateKeyId); // Fetch Keyword Details 

                    // Update or insert dynamic data
                    if (ExistsData)
                    {
                        objDynamicForm = db.BulkTemplateValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true && b.TemplateKey == TempKeyobj.TemplateKeyValue && b.CustomerId == customerid).FirstOrDefault();
                        objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
                    }
                    else
                    {
                        objDynamicForm.TemplateId = id;
                        objDynamicForm.TemplateKey = TempKeyobj.TemplateKeyValue;
                        objDynamicForm.UserId = Convert.ToInt32(Session["UserId"]);
                        objDynamicForm.IsEnabled = true;
                        objDynamicForm.UserInputs = Request.Form[TempKeyobj.TemplateKeyValue];
                        objDynamicForm.CreatedDate = DateTime.Now;
                        objDynamicForm.CustomerId = customerid;
                        db.BulkTemplateValues.Add(objDynamicForm);
                    }

                    db.SaveChanges();
                }


            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            return RedirectToAction("PreviewDocument", "MultipleDocumentDownload", new { id = id });
        }

        public ActionResult PreviewDocument(int? id,bool associated, int[] customers)
        {
            int customer = customers[0];
            string wordContent = "";
            ViewBag.WordContent = "";
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.TemplateId = id;
        
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

            try
            {
                var objTemplate = db.DocumentTemplates.Find(id);
                wordContent = getWordContent(objTemplate.TemplateFileName);
                var inputs = db.CustomerTemplateDetails.Where(w => w.CustID == customer).ToList();

                foreach (CustomerTemplateDetail tem in inputs)
                {
                    wordContent = wordContent.Replace("&lt;" + tem.FieldName.Replace(" ", "_") + "&gt;", tem.FieldValue);
                    wordContent = wordContent.Replace("<" + tem.FieldName.Replace(" ", "_") + ">", tem.FieldValue);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            ViewBag.WordContent = wordContent;
            
            return View();
        }

        public ActionResult htmlEditor()
        {
            ViewBag.WordContent = "Test";
            return View("HTMLEditor");
        }


        public string getWordContent(string filename)
        {
            string totaltext = "";
            try
            {
                string path = Path.Combine(Server.MapPath("~/TemplateFiles/" + filename));
                byte[] byteArray = System.IO.File.ReadAllBytes(path);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                    {
                        HtmlConverterSettings settings = new HtmlConverterSettings()
                        {
                            // PageTitle = "My Page Title"
                        };

                        // HtmlConverter htmlConverter = new HtmlConverter(

                        XElement html = HtmlConverter.ConvertToHtml(doc, settings);
                        ////  System.IO.File.WriteAllText(path1, html.ToStringNewLineOnAttributes());

                        totaltext = html.ToStringNewLineOnAttributes();
                        totaltext = totaltext.Replace("</p>", "</p><div id=assigned_attributes class=sortable></div>");
                        // totaltext = totaltext.Replace("margin-top", "margin top");
                        //totaltext = totaltext.Replace("margin-bottom:", "margin bottom");
                        totaltext = totaltext.Replace("pt-DefaultParagraphFont", " ");
                        totaltext = totaltext.Replace("span { white - space: pre - wrap; }", " ");
                        totaltext = totaltext.Replace("span { white-space: pre-wrap; }", " ");
                        totaltext = totaltext.Replace("span {", "test {");

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


            return totaltext;
        }


     //   public ActionResult FormsConfirmation()
     //   {
     //       string newFilename = "";
     //       string path = "";
     //       string newpath = "";
     //       int customerId = 0;
     //       int GroupId = 1;
     //       bool associatedDocument = false;
     //       int templateId = Convert.ToInt32(Session["TemplateId"]);
     //       int displayOrder = 0;
     //       bool Mandatory = false;
     //       int associatedTemplateID = 0;


     //       try
     //       {
     //           int? id = null;
     //           int? userId = null;
     //           string wordContent = "";

     //           if (Session["CurrentTemplateId"] != null)
     //           {
     //               id = Convert.ToInt32(Session["CurrentTemplateId"]);
     //               userId = Convert.ToInt32(Session["UserId"]);
     //           }

     //           // Updating status for create document
     //           if (id != null)
     //           {
     //               string multicustomerid = Session["MultipleCustomerIDS"].ToString();

     //               string[] customerIds = multicustomerid.Split(',');
     //               for (int i = 0; i < customerIds.Length - 1; i++)
     //               {
     //                   customerId = Convert.ToInt32(customerIds[i]);
     //                   var objDocumentTemplate = db.DocumentTemplates.Find(id);
     //                   wordContent = getWordContent(objDocumentTemplate.TemplateFileName);

     //                   List<BulkTemplateValue> objDynamicForm = new List<BulkTemplateValue>();
     //                   objDynamicForm = db.BulkTemplateValues.Where(b => b.TemplateId == id && b.UserId == userId && b.IsEnabled == true && b.CustomerId == multicustomerid).ToList();
     //                   //Replace Keyvalues from word Document

     //                   Random rnd = new Random();
     //                   newFilename = "U" + userId + "T" + rnd.Next(1, 999999999) + objDocumentTemplate.TemplateFileName; // Create New File with unique name
     //                   path = Path.Combine(Server.MapPath("~/TemplateFiles/" + objDocumentTemplate.TemplateFileName)); // Getting Original File For Create a new one with filled details
     //                   newpath = Path.Combine(Server.MapPath("~/FilledTemplateFiles/" + newFilename)); // New File Path with File Name
     //                   System.IO.File.Copy(path, newpath);

     //                   DoSearchAndReplaceInWord(newpath, objDynamicForm);// Replace process


     //                   if (customerIds.Length - 2 == i)
     //                   {
     //                       //Update the status for creating new word document
     //                       foreach (var frmList in objDynamicForm)
     //                       {
     //                           frmList.IsEnabled = false;
     //                       }
     //                       db.SaveChanges();
     //                   }
     //                   ConvertToPdfFile(newpath); // Convert to pdf file
     //                   Session["newFilename"] = newFilename;
     //                   // Insert Filled Form Details For Billing
     //                   var objFilledForm = db.FilledTemplateDetails.Where(c => c.UserId == userId);

     //                   if (Session["Displayorder"] != null && Convert.ToInt32(Session["Displayorder"]) > 0)
     //                   {
     //                       GroupId = Convert.ToInt32(Session["GroupId"]);

     //                   }
     //                   else
     //                   {
     //                       if (Session["GroupId"] != null && Convert.ToInt32(Session["GroupId"]) != 0)
     //                       {
     //                       }
     //                       else
     //                       {
     //                           var GroupForm = objFilledForm.OrderByDescending(d => d.GroupId).FirstOrDefault();

     //                           // Assign Group Id
     //                           if (GroupForm != null)
     //                           {
     //                               GroupId = GroupForm.GroupId + 1;
     //                               Session["GroupId"] = GroupId;
     //                           }
     //                           if (Session["GroupId"] != null && Convert.ToInt32(Session["GroupId"]) != 0)
     //                           {
     //                               if (Convert.ToInt32(Session["AssociateCount"]) >= 1)
     //                               {
     //                                   // Holding same Group Id
     //                                   GroupId = Convert.ToInt32(Session["GroupId"]);
     //                               }
     //                           }
     //                       }
     //                   }




     //                   // Insert Filled Form Details
     //                   FilledTemplateDetail objFilledTemp = new FilledTemplateDetail();
     //                   objFilledTemp.GroupId = (Session["GroupId"] != null) ? Convert.ToInt32(Session["GroupId"]) : GroupId;
     //                   objFilledTemp.PaidStatus = false;
     //                   objFilledTemp.UserId = userId.Value;
     //                   objFilledTemp.TemplateId = id.Value;
     //                   objFilledTemp.FilledTemplateName = newFilename;
     //                   objFilledTemp.Amount = objDocumentTemplate.TemplateCost;
     //                   objFilledTemp.CreatedDate = DateTime.Now;
     //                   objFilledTemp.CustomerId = customerId;
     //                   objFilledTemp.OrgId = Convert.ToInt32(Session["OrgId"]);
     //                   db.FilledTemplateDetails.Add(objFilledTemp);
     //                   db.SaveChanges();

     //                   if (Convert.ToInt32(Session["ATCount"]) >= 1 && customerIds.Length - 2 == i)
     //                   {
     //                       if (Session["Displayorder"] != null)
     //                       {
     //                           displayOrder = Convert.ToInt32(Session["Displayorder"]);
     //                       }
     //                       var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == templateId && c.IsEnabled == true && (c.DisplayOrder == null || c.DisplayOrder > displayOrder)).OrderBy(c => c.DisplayOrder).FirstOrDefault();

     //                       if (objAssociateIds != null)
     //                       {
     //                           Session["Displayorder"] = objAssociateIds.DisplayOrder;
     //                           displayOrder = Convert.ToInt32(objAssociateIds.DisplayOrder);
     //                           associatedDocument = true;
     //                           Mandatory = objAssociateIds.Mandatory;
     //                           associatedTemplateID = objAssociateIds.AssociateTemplateId;
     //                       }
     //                       else
     //                       {
     //                           associatedDocument = false;
     //                       }
     //                   }
     //                   if (Convert.ToInt32(Session["ATCount"]) == 0 && customerIds.Length - 2 == i)
     //                   {

     //                       Session.Remove("Displayorder");

     //                       int groupId = (Session["GroupId"] != null) ? Convert.ToInt32(Session["GroupId"]) : GroupId;

     //                       var filledDocs = db.FilledTemplateDetails.Where(g => g.GroupId == groupId).GroupBy(s => s.CustomerId).ToList().Select(g => g.First())
     //.ToList();

     //                       foreach (FilledTemplateDetail temp in filledDocs)
     //                       {
     //                           int Group = 0;
     //                           var GroupForm = objFilledForm.OrderByDescending(d => d.GroupId).FirstOrDefault();
     //                           if (GroupForm != null)
     //                           {
     //                               Group = GroupForm.GroupId + 1;
     //                           }
     //                           db.FilledTemplateDetails.Where(g => g.GroupId == groupId && g.CustomerId == temp.CustomerId).ToList().ForEach(g => g.GroupId = Group);
     //                           db.SaveChanges();
     //                           Session["GroupId"] = Group;

     //                           var templateName = db.FilledTemplateDetails.Where(t => t.TemplateId == templateId && t.GroupId == Group && t.CustomerId == temp.CustomerId).FirstOrDefault();

     //                           CreateCoverLetteronHold(templateName.FilledTemplateName, temp.CustomerId);
     //                       }
     //                       Session.Remove("ATCount");
     //                       Session.Remove("AssociateCount");
     //                       Session.Remove("GroupId");
     //                       Session.Remove("customerId");
     //                       Session.Remove("newFilename");
     //                       Session.Remove("MultipleCustomerIDS");
     //                       Session.Remove("TemplateId");
     //                       Session.Remove("CurrentTemplateId");
     //                       Session.Remove("MultipleCustomer");
     //                   }

     //               }

     //               if (associatedDocument)
     //               {

     //                   var lastdoc = db.AssociateTemplateDetails.Where(c => c.TemplateId == templateId && c.IsEnabled == true && (c.DisplayOrder == null || c.DisplayOrder > displayOrder)).OrderBy(c => c.DisplayOrder + 1).FirstOrDefault();

     //                   if (lastdoc == null)
     //                       ViewBag.lastdoc = "true";
     //                   else
     //                       ViewBag.lastdoc = "false";
     //                   int i = Convert.ToInt32(Session["ATCount"]);
     //                   if (Mandatory)
     //                   {

     //                       Session["ATCount"] = i - 1;
     //                       return RedirectToAction("CreateDynamicForm", "MultipleDocumentDownload", new { id = associatedTemplateID });
     //                   }
     //                   else
     //                   {
     //                       Session["ATCount"] = i - 1;
     //                       return RedirectToAction("CoverLetterConfirm", "MultipleDocumentDownload", new { id = associatedTemplateID });
     //                   }

     //               }
     //           }

     //       }
     //       catch (Exception ex)
     //       {
     //           ErrorLog.LogThisError(ex);
     //       }

     //       return RedirectToAction("FormsHistory", "DocumentManagement");
     //   }

        public void CreateCoverLetteronHold(string newFilename, int customerID)
        {
            int customerId = customerID;
            try
            {
                if (newFilename == null)
                {
                    newFilename = Session["newFilename"].ToString();
                }

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
                //CoverLetterInWord(coverLetterpath, objCD, docList);//CoverLetter Create 
               // ConvertToPdfFile(coverLetterpath); // Convert to pdf file

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


        }

        public string DocumentListForCoverLetter(List<DocumentTemplate> objDocument)
        {
            string docList = "";
            int i = 0;
            foreach (DocumentTemplate dclist in objDocument)
            {
                i++;
                docList = docList + i + ". Copy of form " + dclist.DocumentTitle + " \r\n";
            }

            return docList;
        }

        //public static void CoverLetterInWord(string filepath, CustomerDetail objcustomer, string docList)
        //{
        //    try
        //    {
        //        // Create the Word application and declare a document
        //        Application word = new Application();
        //        Document doc = new Document();
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
        //            string[] coverKeys = new string[] { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME", "DocumentNameList" };
        //            string[] CustomerDetail = new string[5];// { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME" };
        //            CustomerDetail[0] = DateTime.Now.ToShortDateString();
        //            CustomerDetail[1] = objcustomer.Address;
        //            CustomerDetail[2] = objcustomer.BankName;
        //            CustomerDetail[3] = objcustomer.CustomerName;
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

        //}

        public static void ConvertToPdfFile(string path)
        {
            try
            {
                Application appWord = new Application();
                Document wordDocument = new Document();
                wordDocument = appWord.Documents.Open(path);
                wordDocument.ExportAsFixedFormat(path.Replace(".docx", ".pdf"), WdExportFormat.wdExportFormatPDF);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }


        }


        public static void DoSearchAndReplaceInWord(string filepath, int customerID,DateTime? date)
        {
            try
            { 
            //string[] customerIds = obj[0].CustomerId.Split(',');
            //for (int i = 0; i < customerIds.Length; i++)
            //{

            // Create the Word application and declare a document
            Application word = new Application();
                Microsoft.Office.Interop.Word.Document doc = word.Documents.Add();
               // Document doc = new Document();

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



                string input = string.Empty;
                VirtualAdvocateEntities db = new VirtualAdvocateEntities();
                var inputs = db.CustomerTemplateDetails.Where(w => w.CustID == customerID).ToList();
                    CustomerTemplateDetail obj = new CustomerTemplateDetail();
                    obj.FieldName = "Date";
                    if(date!=null)
                    obj.FieldValue = date.Value.ToString("dd-MMM-yyyy");
                    inputs.Add(obj);
                foreach (CustomerTemplateDetail tem in inputs)
                {
                    // Loop through the StoryRanges (sections of the Word doc)
                    foreach (Range tmpRange in doc.StoryRanges)
                    {
                        // Set the text to find and replace
                        tmpRange.Find.Text = "<" + tem.FieldName.Replace(" ", "_") + ">";
                        tmpRange.Find.Replacement.Text = tem.FieldValue;
                        //tmpRange.Find.Text = "<Date>";
                        //tmpRange.Find.Replacement.Text = date.Value.ToString("dd-MMM-yyyy");
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



                }

                // Save the changes
                doc.Save();

                // Close the doc and exit the app
                doc.Close(ref missing, ref missing, ref missing);
                word.Application.Quit(ref missing, ref missing, ref missing);
            }

            catch (Exception ex)
            {
                Logger(ex.Message);
                ErrorLog.LogThisError(ex);
                doc.Close(ref missing, ref missing, ref missing);
                word.Application.Quit(ref missing, ref missing, ref missing);
            }
            }
            catch (Exception ex)
            {
                Logger(ex.Message);
               
            }
        }

        public ActionResult CoverLetterConfirm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.MultiName = Session["MultiName"];
            ViewBag.AssociateId = id;
            return View();
        }

        //Manually creating CoverLetter
        public ActionResult CoverLetterConfirmed()
        {
            string multicustomerid = Session["MultipleCustomerIDS"].ToString();

            int GroupId = 1;
            Int32 userId = Convert.ToInt32(Session["UserId"]);
            int templateId = Convert.ToInt32(Session["TemplateId"]);
            var objFilledForm = db.FilledTemplateDetails.Where(c => c.UserId == userId);


            if (Session["GroupId"] != null && Convert.ToInt32(Session["GroupId"]) != 0)
            {
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
            string[] customerIds = multicustomerid.Split(',');
            for (int i = 0; i < customerIds.Length - 1; i++)
            {


                Session.Remove("Displayorder");

                int groupId = (Session["GroupId"] != null) ? Convert.ToInt32(Session["GroupId"]) : GroupId;

                var filledDocs = db.FilledTemplateDetails.Where(g => g.GroupId == groupId).GroupBy(s => s.CustomerId).ToList().Select(g => g.First())
.ToList();

                foreach (FilledTemplateDetail temp in filledDocs)
                {
                    int Group = 0;
                    var GroupForm = objFilledForm.OrderByDescending(d => d.GroupId).FirstOrDefault();
                    if (GroupForm != null)
                    {
                        Group = GroupForm.GroupId + 1;
                    }
                    db.FilledTemplateDetails.Where(g => g.GroupId == groupId && g.CustomerId == temp.CustomerId).ToList().ForEach(g => g.GroupId = Group);
                    db.SaveChanges();
                    Session["GroupId"] = Group;

                    var templateName = db.FilledTemplateDetails.Where(t => t.TemplateId == templateId && t.GroupId == Group && t.CustomerId == temp.CustomerId).FirstOrDefault();

                    CreateCoverLetteronHold(templateName.FilledTemplateName, temp.CustomerId);
                }

            }
            Session.Remove("ATCount");
            Session.Remove("AssociateCount");
            Session.Remove("GroupId");
            Session.Remove("customerId");
            Session.Remove("newFilename");
            Session.Remove("MultipleCustomerIDS");
            Session.Remove("TemplateId");
            Session.Remove("CurrentTemplateId");
            Session.Remove("MultipleCustomer");

            //    CreateCoverLetteronHold(Session["newFilename"].ToString(),Convert.ToInt32( Session["customerId"]));
            //Session["AssociateCount"] = 0;
            //Session["GroupId"] = 0;
            //Session["customerId"] = null;
            return RedirectToAction("FormsHistory", "DocumentManagement");
        }

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
                Session["ATCount"] = Convert.ToInt32(Session["ATCount"]) - 1;
                if (objAssociateIds.Mandatory)

                    return RedirectToAction("CreateDynamicForm", "MultipleDocumentDownload", new { id = objAssociateIds.AssociateTemplateId });

                else
                    return RedirectToAction("CoverLetterConfirm", "MultipleDocumentDownload", new { id = objAssociateIds.AssociateTemplateId });


            }
            else
            {

                string multicustomerid = Session["MultipleCustomerIDS"].ToString();

                int GroupId = 1;
                Int32 userId = Convert.ToInt32(Session["UserId"]);
                var objFilledForm = db.FilledTemplateDetails.Where(c => c.UserId == userId);


                if (Session["GroupId"] != null && Convert.ToInt32(Session["GroupId"]) != 0)
                {
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
                string[] customerIds = multicustomerid.Split(',');
                for (int i = 0; i < customerIds.Length - 1; i++)
                {


                    Session.Remove("Displayorder");

                    int groupId = (Session["GroupId"] != null) ? Convert.ToInt32(Session["GroupId"]) : GroupId;

                    var filledDocs = db.FilledTemplateDetails.Where(g => g.GroupId == groupId).GroupBy(s => s.CustomerId).ToList().Select(g => g.First())
    .ToList();

                    foreach (FilledTemplateDetail temp in filledDocs)
                    {
                        int Group = 0;
                        var GroupForm = objFilledForm.OrderByDescending(d => d.GroupId).FirstOrDefault();
                        if (GroupForm != null)
                        {
                            Group = GroupForm.GroupId + 1;
                        }
                        db.FilledTemplateDetails.Where(g => g.GroupId == groupId && g.CustomerId == temp.CustomerId).ToList().ForEach(g => g.GroupId = Group);
                        db.SaveChanges();
                        Session["GroupId"] = Group;

                        var templateName = db.FilledTemplateDetails.Where(t => t.TemplateId == templateId && t.GroupId == Group && t.CustomerId == temp.CustomerId).FirstOrDefault();

                        CreateCoverLetteronHold(templateName.FilledTemplateName, temp.CustomerId);
                    }

                }
                Session.Remove("ATCount");
                Session.Remove("AssociateCount");
                Session.Remove("GroupId");
                Session.Remove("customerId");
                Session.Remove("newFilename");
                Session.Remove("MultipleCustomerIDS");
                Session.Remove("TemplateId");
                Session.Remove("CurrentTemplateId");
                Session.Remove("MultipleCustomer");


                return RedirectToAction("FormsHistory", "DocumentManagement");
            }


        }


        public string CreateDocument(Int32 id,Int32 custID,Int32 groupID,Int32 bulkID,DateTime Date)
        {
            string newFilename = "";
            string path = "";
            string newpath = "";
            int userId = Convert.ToInt32(Session["UserId"]);
           // string wordContent = string.Empty;

            var objDocumentTemplate = db.DocumentTemplates.Find(id);
            //  wordContent = getWordContent(objDocumentTemplate.TemplateFileName);
            string customerName = db.CustomerDetails.Single(s => s.CustomerId == custID).CustomerName;
            newFilename = customerName + DateTime.Now.Ticks + objDocumentTemplate.TemplateFileName.Replace(" ", ""); // Create New File with unique name
            path = Path.Combine(Server.MapPath("~/TemplateFiles/" + objDocumentTemplate.TemplateFileName.Replace(" ", ""))); // Getting Original File For Create a new one with filled details
            newpath = Path.Combine(Server.MapPath("~/FilledTemplateFiles/" + newFilename.Replace(" ","_"))); // New File Path with File Name
            System.IO.File.Copy(path, newpath);

           // CreateDocument(newpath, custID,Date);



            DoSearchAndReplaceInWord(newpath, custID,Date);// Replace process                       
            ConvertToPdfFile(newpath); // Convert to pdf file

            // Insert Filled Form Details For Billing
            var objFilledForm = db.FilledTemplateDetails.Where(c => c.UserId == userId);

            // Insert Filled Form Details
            FilledTemplateDetail objFilledTemp = new FilledTemplateDetail();
            objFilledTemp.GroupId = groupID;
            objFilledTemp.PaidStatus = false;
            objFilledTemp.UserId = userId;
            objFilledTemp.TemplateId = id;
            objFilledTemp.FilledTemplateName = newFilename;
            objFilledTemp.Amount = objDocumentTemplate.TemplateCost;
            objFilledTemp.CreatedDate = DateTime.Now;
            objFilledTemp.CustomerId = custID;
            objFilledTemp.OrgId = Convert.ToInt32(Session["OrgId"]);
            objFilledTemp.CoverLetter = false;
            objFilledTemp.BulkTemplateID = bulkID;

            db.FilledTemplateDetails.Add(objFilledTemp);
            db.SaveChanges();

            return newFilename;

        }


        public void CreateDocument(string filepath, int customerID,DateTime Date)
        {
            string totaltext = "";
            try
            {
                //DoSearchAndReplaceInWord(filepath,customerID,Date);// Replace process
                //ConvertToPdfFile(filepath); // Convert to pdf file

                byte[] byteArray = System.IO.File.ReadAllBytes(filepath);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                    {
                        HtmlConverterSettings settings = new HtmlConverterSettings()
                        {
                        };

                        XElement html = HtmlConverter.ConvertToHtml(doc, settings);

                        totaltext = html.ToStringNewLineOnAttributes();
                        totaltext = totaltext.Replace("pt-DefaultParagraphFont", " ");
                        totaltext = totaltext.Replace("span { white - space: pre - wrap; }", " ");
                        totaltext = totaltext.Replace("span { white-space: pre-wrap; }", " ");
                        totaltext = totaltext.Replace("span {", "test {");
                    }
                }
                string input = string.Empty;
                VirtualAdvocateEntities db = new VirtualAdvocateEntities();
                var inputs = db.CustomerTemplateDetails.Where(w => w.CustID == customerID).ToList();

                foreach (CustomerTemplateDetail tem in inputs)
                {
                    totaltext = totaltext.Replace("&lt;" + tem.FieldName.Replace(" ", "_") + "&gt;", tem.FieldValue);
                    totaltext = totaltext.Replace("<" + tem.FieldName.Replace(" ", "_") + ">", tem.FieldValue);
                }
                totaltext = totaltext.Replace("&lt;" + "Date" + "&gt;", DateTime.UtcNow.ToString());
                totaltext = totaltext.Replace("<" + "Date" + ">", Date.ToString("dd-MMM-yyyy"));
                Logger(totaltext);
                filepath = filepath.Replace(".docx", ".pdf");
                CreateDocumentFromHiQpdf(totaltext, filepath);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

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
                System.IO.File.WriteAllBytes(FilePath, bytedata);

            }
            catch (Exception ex)
            {
                Logger(ex.Message);
                Logger(ex.InnerException.Message);

                Logger(ex.StackTrace);

            }
        }

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


      
        public ActionResult Download(int? id,bool associated,int[] customers,DateTime? Date)
        {
           
            string customerIds = string.Empty;
                int userId = Convert.ToInt32(Session["UserId"]);
            if (Date == null)
                Date = DateTime.UtcNow.Date;
            BulkTemplateLog objLog = new BulkTemplateLog();
            objLog.CreatedBy = userID;
            objLog.TemplateID = id.Value;
            objLog.CreatedOn = DateTime.UtcNow;

            db.BulkTemplateLogs.Add(objLog);

            db.SaveChanges();
            try
                {

                ///Insert into bulk template log table

               
                int customerId = 0;
                    
                    for (int i = 0; i < customers.Length; i++)
                    {
                    int groupID = 0;
                        var GroupForm = db.FilledTemplateDetails.OrderByDescending(d => d.GroupId).FirstOrDefault();
                    if (GroupForm == null)
                        groupID = 1;
                    else
                        groupID = GroupForm.GroupId + 1;
                  customerId = Convert.ToInt32(customers[i]);
                        var customerID = db.CustomerDetails.Where(c => c.CustomerId == customerId).Select(c => c.CustomerId).FirstOrDefault();

                        customerId = customerID;
                        string filename=  CreateDocument(id.Value, customerId,groupID,objLog.ID,Date.Value);

                        if (associated)
                        {
                            var objAssociateIds = db.AssociateTemplateDetails.Where(c => c.TemplateId == id.Value && c.IsEnabled == true).ToList();

                            if (objAssociateIds != null && objAssociateIds.Count > 0)
                            {
                                foreach (AssociateTemplateDetail objAss in objAssociateIds)
                                {
                                    CreateDocument(objAss.AssociateTemplateId, customerId, GroupForm.GroupId + 1, objLog.ID,Date.Value);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                }
            return Json(objLog.ID, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckAssociatedDocs(int? id)
        {
           var assObj= db.AssociateTemplateDetails.Where(a => a.TemplateId == id.Value).FirstOrDefault();

            if (assObj != null)
                return Json(true, JsonRequestBehavior.AllowGet);
            else
                return Json(false, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public FileResult DownloadMultipleDocuments(IEnumerable<string> FilledTemplateName)
        {
            if (FilledTemplateName != null)
            {
                // Create file on disk
                MemoryStream ms = new MemoryStream();
                // byte[] zipContent = null;
                using (ZipFile zip = new ZipFile())
                {
                    foreach (var filename in FilledTemplateName)
                    {
                        string filepath = Server.MapPath("~/FilledTemplateFiles/") + filename;
                        Logger(filepath);
                        if (System.IO.File.Exists(filepath))
                        {

                            zip.AddFile(filepath, "Files");
                        }



                    }
                    zip.Save(ms);
                    ms.Position = 0;
                    return File(ms, "application/zip", "Documents_" + DateTime.Now.ToString("ddMMyyyyhhss") + ".zip");
                }
            }
            else

            return (null);
        }

        /// <summary>
        /// Filled Document List Based on Logged in User
        /// </summary>
        /// <returns></returns>
        public ActionResult BulkDownload()
        {
            GetCustomerNameList();
            List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                var customer = (from user in db.UserProfiles.Where(u => u.UserID == userId) select user.OrganizationId).FirstOrDefault();

                var objFilledTemp = (from obj in db.FilledTemplateDetails

                                     join cust in db.CustomerDetails on obj.CustomerId equals cust.CustomerId
                                     join doc in db.DocumentTemplates on obj.TemplateId equals doc.TemplateId into g
                                     from subset in g.DefaultIfEmpty()
                                     where obj.OrgId == customer && (obj.ArchiveStatus == null || obj.ArchiveStatus == false) && obj.CoverLetter==false
                                     select new FilledFormDetailModel { DocumentTitle = (subset == null ? "Template Deleted" : subset.DocumentTitle), Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, RowId = obj.RowId, CustomerName = cust.CustomerName }
                    );
                objForm = objFilledTemp.OrderByDescending(x => x.GroupId).ThenBy(o => o.RowId).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View(objForm);
        }

        public static void Logger(string msg)
        {
            string fileName = System.Configuration.ConfigurationManager.AppSettings["logPath"] ;

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

        private void GetCustomerNameList()
        {
            int orgid = 0;
            if (Convert.ToInt32(Session["Orgid"]) != 0 && Session["Orgid"] != null)
            {
                orgid = Convert.ToInt32(Session["Orgid"]);
            }

            if (Session["CustHistoryID"] != null && !string.IsNullOrEmpty(Session["CustHistoryID"].ToString()))
            {
                int customerID = Convert.ToInt32(Session["CustHistoryID"]);
                Session.Remove("CustHistoryID");

                var customers = db.CustomerDetails.Where(m => m.IsEnabled == true && m.OrganizationId == orgid &&(roleId==6 || m.createdBy==userID)).Select(c => new {
                    customerID = c.CustomerId,
                    customerName = c.CustomerName
                }).ToList();

                ViewBag.Customers = new MultiSelectList(customers, "customerID", "customerName");
            }
            else
            {
               
                if (orgid == 0 && Convert.ToInt32(Session["RoleId"]) == 1)
                {
                    var customers = db.CustomerDetails.Where(c => c.IsEnabled == true).Select(c => new {
                        customerID = c.CustomerId,
                        customerName = c.CustomerName
                    }).ToList();

                    ViewBag.Customers = new MultiSelectList(customers, "customerID", "customerName");
                }
                else
                {
                    var customers = db.CustomerDetails.Where(m => m.OrganizationId == orgid && m.IsEnabled == true && orgid != 0  && (m.createdBy == userID || (roleId == 2) || (m.Department == deptID && roleId == 5))).Select(c => new {
                        customerID = c.CustomerId,
                        customerName = c.CustomerName
                    }).ToList();

                    ViewBag.Customers = new MultiSelectList(customers, "customerID", "customerName");

                }
                Session.Remove("CustHistoryID");

            }

        }

        public ActionResult BulkDocuments()
        {
            try
            {
                int roleID = Convert.ToInt32(Session["RoleId"]);
                int department = Convert.ToInt32(Session["DepartmentID"]);

                var objTemplates = (from ut in db.DocumentTemplates
                                    join dc in db.DocumentCategories on ut.DocumentCategory equals dc.DocumentCategoryId
                                    join bu in db.BulkTemplateLogs  on ut.TemplateId equals  bu.TemplateID
                                    where ut.IsEnabled == true && ut.IsEnabled == true && dc.ServiceId == orgId
                                                                        && (((roleID != 6) && (roleID != 5)) || ((roleID == 6 && ut.DepartmentID == department) || (roleID == 5 && ut.DepartmentID == department))) orderby bu.CreatedOn descending
                                    

                                    select new BulkDocumentTemplateListModel { TemplateName = ut.DocumentTitle, TemplateId = ut.TemplateId, BulkTemplateID = bu.ID, DocumentCategory = dc.DocumentCategoryName, Cost = ut.TemplateCost,CreatedOn=bu.CreatedOn

                                    }
                        ).OrderByDescending(s => s.CreatedOn);

                return View(objTemplates.ToList());
            }

            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                return View();
            }

        }

        public ActionResult GetDocuments(int id)
        {
            GetCustomerNameList();
            List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();
            try
            {
                
                var objFilledTemp = (from obj in db.FilledTemplateDetails

                                     join cust in db.CustomerDetails on obj.CustomerId equals cust.CustomerId
                                     join doc in db.DocumentTemplates on obj.TemplateId equals doc.TemplateId into g
                                     from subset in g.DefaultIfEmpty()
                                     where obj.OrgId == orgId && (obj.ArchiveStatus == null || obj.ArchiveStatus == false) && obj.CoverLetter == false && obj.BulkTemplateID==id
                                     select new FilledFormDetailModel { DocumentTitle = (subset == null ? "Template Deleted" : subset.DocumentTitle), Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, RowId = obj.RowId, CustomerName = cust.CustomerName }
                    );
                objForm = objFilledTemp.OrderByDescending(x => x.GroupId).ThenBy(o => o.RowId).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return View("BulkDownload", objForm);
        }

        [HttpGet]
        public FileResult BulkDocumentDownload(int id)
        {
            List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();

            var objFilledTemp = (from obj in db.FilledTemplateDetails

                                 where obj.OrgId == orgId && (obj.ArchiveStatus == null || obj.ArchiveStatus == false) && obj.CoverLetter == false && obj.BulkTemplateID == id
                                 select new FilledFormDetailModel {FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, RowId = obj.RowId }
                 );
            objForm = objFilledTemp.OrderByDescending(x => x.GroupId).ThenBy(o => o.RowId).ToList();

            if (objForm != null)
            {
                // Create file on disk
                MemoryStream ms = new MemoryStream();
                // byte[] zipContent = null;
                using (ZipFile zip = new ZipFile())
                {
                    foreach (FilledFormDetailModel filename in objForm)
                    {
                        string filepath = Server.MapPath("~/FilledTemplateFiles/") + filename.FilledTemplateName.Replace("docx","pdf");
                        Logger(filepath);
                        if (System.IO.File.Exists(filepath))
                        {

                            zip.AddFile(filepath, "Files");
                        }



                    }
                    zip.Save(ms);
                    ms.Position = 0;
                    return File(ms, "application/zip", "Documents_" + DateTime.Now.ToString("ddMMyyyyhhss") + ".zip");
                }
            }
            else

                return (null);
        }
    }
    }