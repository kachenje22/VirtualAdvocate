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
using Microsoft.Office.Interop.Word;
using VirtualAdvocate.BLL;
using System.Configuration;

namespace VirtualAdvocate.Controllers
{
    public class DueDiligenceController : BaseController
    {
        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();
        private VirtualAdvocateData objData = new VirtualAdvocateData();
        // GET: DueDiligence
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Enquiry()
        {
            DueDiligenceUserEnquiryViewModel obj = new DueDiligenceUserEnquiryViewModel();
            obj.getAllEnquiryType = objData.getAllEnquiryType();

            return View(obj);
        }

        [HttpPost]
        public ActionResult Enquiry(DueDiligenceUserEnquiryViewModel obj)
        {
            int userid = Convert.ToInt32(Session["UserId"]);
            string Field = "";
            string newFilename = "";
            decimal vat = 0;
            newFilename = RandomName(userid.ToString());
            if (ModelState.IsValid)
            {
                try
                {
                    DueDiligenceEnquiry objEnquiry = new DueDiligenceEnquiry();
                    if (obj.EnquiryTypeId == 1) // Business Details
                    {
                        objEnquiry.BusinessName = obj.BusinessName;
                        objEnquiry.BusinessRegistrationNumber = obj.BusinessRegistrationNumber;
                        Field = "Business Name :"+ obj.BusinessName;
                        Field = Field + "<br />Business Registration Number :" + obj.BusinessRegistrationNumber;
                    }
                    else if (obj.EnquiryTypeId == 2) // Company Details
                    {
                        objEnquiry.CompanyName = obj.CompanyName;
                        objEnquiry.CompanyRegName = obj.CompanyRegName;
                        Field = "Company Name :" + obj.CompanyName;
                        Field = Field + "<br />Company Incorporation Number    :" + obj.CompanyRegName;
                    }
                    else  //Land Details
                    {
                        objEnquiry.Area = obj.Area;
                        objEnquiry.BlockNumber = obj.BlockNumber;
                        objEnquiry.PlotNumber = obj.PlotNumber;
                        objEnquiry.Region = obj.Region;
                        objEnquiry.Municipality = obj.Municipality;
                        objEnquiry.CertificateTitleNo = obj.CertificateTitleNo;

                        Field = "Certificate Title No :" + obj.CertificateTitleNo;
                        Field = Field + "<br />Plot Number :" + obj.PlotNumber;
                        Field = Field + "<br />Block Number :" + obj.BlockNumber;
                        Field = Field + "<br />Area :" + obj.Area;
                        Field = Field + "<br />Municipality :" + obj.Municipality;
                        Field = Field + "<br />Region :" + obj.Region;

                    }
                    objEnquiry.EnquiryType = obj.EnquiryTypeId;
                    objEnquiry.IsEnabled = true;
                    objEnquiry.UserId = userid;
                    objEnquiry.CreatedDate = DateTime.Now;
                    db.DueDiligenceEnquiries.Add(objEnquiry);
                    db.SaveChanges();
                  
                    string FullName = "";
                    string AdminName = "";
                    string EmailAddress = "";
                    string EnqType = "";
                    int days;
                    //var objSysConfig = db.SystemConfigs.Find(1);
                    var appConfig = db.ApplicationConfigurations.Where(x => x.KeyName == "DueDays").FirstOrDefault();
                    if(appConfig != null)
                    { days = Convert.ToInt32(appConfig.KeyValue); }
                    else
                    {
                        days = 5;
                    }
                   

                    var objadmin = db.UserProfiles.Where(c => c.RoleId == 1 && c.IsEnabled == true).FirstOrDefault();
                    if(objadmin!=null)
                    {
                        var objAdminUP = db.UserAddressDetails.Find(objadmin.UserID);
                        if(objAdminUP!=null)
                        {
                            AdminName = objAdminUP.FirstName + " " + objAdminUP.LastName;
                        }
                    }

                    // Getting Amount 
                    var objCost = db.DueDiligenceCosts.Where(m => m.DueDiligenceType == obj.EnquiryTypeId.ToString()).FirstOrDefault();
                    decimal Cost = Convert.ToDecimal(objCost.Cost);

                    // Getting Enquiry Type
                    var objEnqType = db.DueDiligenceEnquiryTypes.Find(obj.EnquiryTypeId);
                    EnqType = objEnqType.EnquiryType;

                    // Getting Address Details
                    var objAddress = db.UserAddressDetails.Where(m=>m.UserId==userid).FirstOrDefault();                   
                    var objUP = db.UserProfiles.Find(userid);
                    if (objAddress != null && objUP!=null)
                    {
                        FullName = objAddress.FirstName + " " + objAddress.LastName;
                        EmailAddress = objUP.EmailAddress;
                    }

                    // Creating Invoice 


                   
                    string invoicepath = Path.Combine(Server.MapPath("~/DueInvoiceFiles/" + newFilename)); // New File Path with File Name
                    var path = Path.Combine(Server.MapPath("~/Resources/DueInvoiceTemplate.docx")); // Getting Original File For Create a new one with filled details
                    if (System.IO.File.Exists(invoicepath))
                    {
                        System.IO.File.Delete(invoicepath);
                    }
                    System.IO.File.Copy(path, invoicepath);
                    var appConfigVat = db.ApplicationConfigurations.Where(x => x.KeyName == "VAT").FirstOrDefault();
                    if (appConfigVat != null)
                    {
                        vat =Convert.ToDecimal(appConfigVat.KeyValue);
                    }
                    else
                    {
                        vat = 18;
                    }
                        vat = Cost * vat / 100;
                    decimal FinalAmount = vat + Cost;

                    //Invoice Word File Create 
                    DueDiligenceInvoice(invoicepath, objAddress, EnqType, Cost.ToString(),  vat, FinalAmount,1,Cost);

                    // Convert to pdf file
                    string dueFilename = newFilename;
                    dueFilename = dueFilename.Replace(".docx", ".pdf");
                    ConvertToPdfFile(invoicepath);

                    // Update Invoice Document 
                    DueDiligenceEnquiry objdue;
                    objdue = db.DueDiligenceEnquiries.Find(objEnquiry.EnquiryId);
                    objdue.InvoiceDocument= dueFilename;
                    objdue.Cost = FinalAmount;
                    db.SaveChanges();

                    // Mail Notification For Admin and Due User
                    MailSend objMail = new MailSend();
                    try
                    {
                        // Mail Notification For Admin 
                        objMail.EnquiryNotificationEmailForAdmin(AdminName, FullName, EnqType, Field, ConfigurationManager.AppSettings["ApplicationTitle"].ToString(), Common.Helper.GetBaseUrl());

                        // Mail Notification For Due Diligence User 
                        objMail.EnquiryNotificationEmailForDueUser(FullName, EmailAddress, days, ConfigurationManager.AppSettings["ApplicationTitle"].ToString(), Common.Helper.GetBaseUrl(), dueFilename);
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.LogThisError(ex);
                    }

                }
                catch(Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                }


            }else
            {
                return RedirectToAction("Enquiry", "DueDiligence");
            }

            return RedirectToAction("InquiryList", "DueDiligence");
        }

        public string RandomName(string filename)
        {
            Random rnd = new Random();

            if (filename == "" || filename == null)
            {
                filename = rnd.Next(1, 999999999).ToString();
            }
            filename = "U" + filename + "DI" + rnd.Next(1, 999999999) + "dueinvoice.docx"; // Create New File with unique name
            return filename;
        }
        public static void ConvertToPdfFile(string path)
        {
            try
            {

                Logger("pdf creation:" +path);
                Application appWord = new Application();
                Document wordDocument = new Document();
                wordDocument = appWord.Documents.Open(path);
                wordDocument.ExportAsFixedFormat(path.Replace(".docx", ".pdf"), WdExportFormat.wdExportFormatPDF);
            }
            catch (Exception ex)
            {
                Logger(ex.Message);
                ErrorLog.LogThisError(ex);
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
        public static void DueDiligenceInvoice(string filepath, UserAddressDetail objcustomer, string ServiceType, string total, decimal vat, decimal FinalAmount, int quantity,decimal cost)
        {
            try
            {

                // Create the Word application and declare a document
                Application word = new Application();
                Document doc = new Document();
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
                    string[] coverKeys = new string[] { "date","Name", "Street", "Build", "Plot", "Block", "Region", "LandMark", "VAT", "TotalAmount", "FinalAmount" };
                    string[] CustomerDetail = new string[11];// { "date", "Name and address of Bank", "name of Bank", "CUSTOMER NAME" };
                    CustomerDetail[0] = DateTime.Now.ToShortDateString();
                    CustomerDetail[1] = objcustomer.FirstName+" "+objcustomer.LastName;
                    CustomerDetail[2] = objcustomer.StreetName;
                    CustomerDetail[3] = objcustomer.BuildingName;
                    CustomerDetail[4] = objcustomer.PlotNumber;
                    CustomerDetail[5] = objcustomer.BlockNumber;
                    CustomerDetail[6] = objcustomer.Region;
                    CustomerDetail[7] = objcustomer.LandMark;
                    CustomerDetail[9] = Decimal.Round(Convert.ToDecimal(total), 2).ToString();
                    CustomerDetail[8] = Decimal.Round(Convert.ToDecimal(vat), 2).ToString();
                    CustomerDetail[10] = Decimal.Round(Convert.ToDecimal(FinalAmount), 2).ToString();
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

                    int tempcount = 1;
                    Tables tables = doc.Tables;
                    if (tables.Count > 0)
                    {
                        Table table = tables[2];
                        //tempcount = objDocumentTemp.Count + 1;
                        int rowsCount = table.Rows.Count;
                        int coulmnsCount = table.Columns.Count;
                        int DocCount = 1;
                        for (int i = 0; i < DocCount; i++)
                        {                            
                            object beforeRow = tables[2].Rows[2];
                            Row row = table.Rows.Add(ref beforeRow);
                            // Row row = table.Rows.Add(ref missing);

                            for (int j = 1; j <= coulmnsCount; j++)
                            {
                                if (j == 1)
                                {
                                    row.Cells[j].Range.Text = tempcount.ToString();
                                }
                                else if (j == 2)
                                {
                                    row.Cells[j].Range.Text = ServiceType;
                                }
                                else if (j == 3)
                                {
                                    row.Cells[j].Range.Text = cost.ToString();
                                }
                                else if (j == 4)
                                {
                                    row.Cells[j].Range.Text = quantity.ToString();
                                }
                                else if (j == 5)
                                {
                                    row.Cells[j].Range.Text = (quantity * cost).ToString();
                                }

                                row.Cells[j].WordWrap = true;
                                row.Cells[j].Range.Underline = WdUnderline.wdUnderlineNone;
                                row.Cells[j].Range.Bold = 0;
                            }
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
        public ActionResult InquiryList()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
           List<DueDiligenceEnquiryListViewModel> objDueList = new List<DueDiligenceEnquiryListViewModel>();
            try
            {
                var objDueFormsList = (from objDueEnquiry in db.DueDiligenceEnquiries
                                       join objEnqType in db.DueDiligenceEnquiryTypes on objDueEnquiry.EnquiryType equals objEnqType.EnquiryTypeId
                                       join up in db.UserAddressDetails on objDueEnquiry.UserId equals up.UserId
                                       // where objDueEnquiry.UserId== userId
                                       select new DueDiligenceEnquiryListViewModel { EnquiryType = objEnqType.EnquiryType, CreatedDate = objDueEnquiry.CreatedDate, IsEnabled = objDueEnquiry.IsEnabled,UserId=objDueEnquiry.UserId,EnquiryId=objDueEnquiry.EnquiryId, ReplyStatus=objDueEnquiry.ReplyStatus, ReportDocument=objDueEnquiry.ReportDocument,Name=up.FirstName,InvoiceDocument=objDueEnquiry.InvoiceDocument }
               ).OrderByDescending(x=>x.CreatedDate).ToList();
                objDueList = objDueFormsList.OrderByDescending(c=>c.CreatedDate).ToList();
                if (Convert.ToInt32(Session["RoleId"])!=1)
                {
                    var result = (from enquiryResult in objDueFormsList
                                  where enquiryResult.UserId == userId
                                  select enquiryResult);
                    result = result.OrderByDescending(x => x.CreatedDate).ToList();
                    return View(result);
                }               
               
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
                     

            return View(objDueList);
        }

        public ActionResult InquiryReply(int? id)
        {
            if(id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DueDiligenceEnquiryViewModel obj = new DueDiligenceEnquiryViewModel();
            obj.getAllEnquiryType = objData.getAllEnquiryType();
            var objresult = db.DueDiligenceEnquiries.Find(id);
            var objCost = db.DueDiligenceCosts.Where(m => m.DueDiligenceType == objresult.EnquiryType.ToString()).FirstOrDefault();
            if (objresult != null)
            {
                obj.CreatedDate = objresult.CreatedDate;

                if (objresult.EnquiryType == 1) // Business Details
                {
                    obj.BusinessName = objresult.BusinessName;
                    obj.BusinessRegistrationNumber = objresult.BusinessRegistrationNumber;
                }
                else if (objresult.EnquiryType == 2) // Company Details
                {
                    obj.CompanyName = objresult.CompanyName;
                    obj.CompanyRegName = objresult.CompanyRegName;

                }
                else  //Land Details
                {
                    obj.Area = objresult.Area;
                    obj.BlockNumber = objresult.BlockNumber;
                    obj.PlotNumber = objresult.PlotNumber;
                    obj.Region = objresult.Region;
                    obj.Municipality = objresult.Municipality;
                    obj.CertificateTitleNo = objresult.CertificateTitleNo;
                }
                obj.EnquiryTypeId = objresult.EnquiryType;
                obj.EnquiryId = objresult.EnquiryId;

                obj.TimeLine = objresult.TimeLine;
                obj.Cost = objresult.Cost;
                obj.ReplyStatus = objresult.ReplyStatus;
            }
            if (objCost!=null)
            {
                obj.Cost = objCost.Cost;
            }           
            

            return View(obj);
        }

        [HttpPost]
        public ActionResult InquiryReply(DueDiligenceEnquiryViewModel obj)
        {
            try
            {
                string FullName = "";
                string EmailAddress = "";
                DueDiligenceEnquiry objdue;
                objdue = db.DueDiligenceEnquiries.Find(obj.EnquiryId);               
                var objCost = db.DueDiligenceCosts.Where(m => m.DueDiligenceType == objdue.EnquiryType.ToString()).FirstOrDefault();
                //objdue.Cost = objCost.Cost;
                objdue.TimeLine = obj.TimeLine;
                objdue.ReplyStatus = true;
                db.SaveChanges();
                MailSend objMail = new MailSend();
                var objAd = db.UserAddressDetails.Where(m=>m.UserId==objdue.UserId).FirstOrDefault();
                var objUP = db.UserProfiles.Find(objdue.UserId);
                FullName = objAd.FirstName + " " + objAd.LastName;
                EmailAddress = objUP.EmailAddress;
                // Mail Notification For Due Diligence User 
                objMail.EnquiryReplyNotificationForDueUser(FullName, EmailAddress,  obj.TimeLine.ToString(), obj.Cost.ToString(), ConfigurationManager.AppSettings["ApplicationTitle"].ToString(), Common.Helper.GetBaseUrl());

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return RedirectToAction("InquiryList", "DueDiligence");
        }

        public ActionResult AttachReport(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DueDiligenceAttachViewModel obj = new DueDiligenceAttachViewModel();
            obj.getAllEnquiryType = objData.getAllEnquiryType();
            var objresult = db.DueDiligenceEnquiries.Find(id);
            if (objresult != null)
            {
                obj.CreatedDate = objresult.CreatedDate;

                if (objresult.EnquiryType == 1) // Business Details
                {
                    obj.BusinessName = objresult.BusinessName;
                    obj.BusinessRegistrationNumber = objresult.BusinessRegistrationNumber;
                }
                else if (objresult.EnquiryType == 2) // Company Details
                {
                    obj.CompanyName = objresult.CompanyName;
                    obj.CompanyRegName = objresult.CompanyRegName;

                }
                else  //Land Details
                {
                    obj.Area = objresult.Area;
                    obj.BlockNumber = objresult.BlockNumber;
                    obj.PlotNumber = objresult.PlotNumber;
                    obj.Region = objresult.Region;
                    obj.Municipality = objresult.Municipality;
                    obj.CertificateTitleNo = objresult.CertificateTitleNo;
                }
                obj.EnquiryTypeId = objresult.EnquiryType;
                obj.EnquiryId = objresult.EnquiryId;

                obj.TimeLine = objresult.TimeLine;
                obj.Cost = objresult.Cost;
                obj.ReplyStatus = objresult.ReplyStatus;
                obj.ReportDocument = objresult.ReportDocument;
            }

            return View(obj);           
        }
        [HttpPost]
        public ActionResult AttachReport(DueDiligenceAttachViewModel obj)
        {
            var newFilename = "";
            var path = "";           
            if (ModelState.IsValid)
            {
                try
                {
                    DueDiligenceEnquiry objdue;
                    objdue = db.DueDiligenceEnquiries.Find(obj.EnquiryId);
                 
                    bool exists = System.IO.Directory.Exists(Server.MapPath("~/DueReports"));

                    if (!exists)
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/DueReports"));

                    if (obj.AttachFile != null && obj.AttachFile.ContentLength > 0)
                    {
                        obj.ReportDocument = Path.GetFileName(obj.AttachFile.FileName);
                        Random rnd = new Random();
                        newFilename = "U" +obj.UserId + "T" + rnd.Next(1, 999999999) + obj.ReportDocument; // Create New File with unique name
                         path = Path.Combine(Server.MapPath("~/DueReports"), newFilename);
                        if (System.IO.File.Exists(path))
                        {
                            newFilename= newFilename = "U" + obj.UserId + "T" + rnd.Next(1, 999999999) +"E"+ obj.ReportDocument;
                            path = Path.Combine(Server.MapPath("~/DueReports"), newFilename);
                        }
                        else
                        {
                            obj.AttachFile.SaveAs(path); // Report document saved into Reports Folder
                            
                        }
                        objdue.ReportDocument = newFilename;
                        db.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);                   
                }

            }
            return RedirectToAction("InquiryList", "DueDiligence");
        }

    }
}