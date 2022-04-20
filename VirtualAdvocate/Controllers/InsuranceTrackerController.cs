using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VirtualAdvocate.Models;

namespace VirtualAdvocate.Controllers
{
    public class InsuranceTrackerController : BaseController
    {
        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();
        public int userID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
        public int orgId = Convert.ToInt32(System.Web.HttpContext.Current.Session["OrgId"]);
        public int deptID = Convert.ToInt32(System.Web.HttpContext.Current.Session["DepartmentID"]);
        public int roleId = Convert.ToInt32(System.Web.HttpContext.Current.Session["RoleId"]);

        // GET: InsuranceTracker
        public ActionResult Index(int? flagForNotification)
        {
            var insurances = new List<InsuranceViewModel>();
            var users = db.UserProfiles.Where(m => m.OrganizationId == orgId && m.Department == deptID).ToList();
            var insuranceDetails = db.Insurances
                .Include("Property")
                .Include("Property.FilledTemplateDetail")
                .ToList()
                .Where(m => m.Status && users.Exists(e => e.UserID == m.Property.FilledTemplateDetail.UserId));
            foreach (var item in insuranceDetails)
            {
                var docTitle = db.DocumentTemplates.FirstOrDefault(m => m.TemplateId == item.Property.FilledTemplateDetail.TemplateId).DocumentTitle;
                insurances.Add(new InsuranceViewModel
                {
                    Id = item.Id,
                    DocumentTitle = docTitle,
                    CustomerName = db.CustomerDetails.FirstOrDefault(m => m.CustomerId == item.Property.FilledTemplateDetail.CustomerId).CustomerName,
                    AssetInsured = db.Properties.FirstOrDefault(m => m.Id == item.PropertyId).PropertyName,
                    Insurer = item.Insurer,
                    AmountInsured = item.AmountInsured,
                    DateOfExpiry = item.DateOfExpiry.ToString("dd-MM-yyyy"),
                    DateOfInsurance = item.DateOfInsurance.ToString("dd-MM-yyyy"),
                    Status = ((DateTime.Now - item.DateOfExpiry).Days > 0) ? InsuranceStatus.Expired : InsuranceStatus.Valid,
                    NoOfDaysExpired = ((DateTime.Now - item.DateOfExpiry).Days > 0) ? (DateTime.Now - item.DateOfExpiry).Days : 0,
                    CreatedDate = item.CreatedDate,
                    Currency = item.Currency
                });
            }

            if (flagForNotification != null && flagForNotification == 1)
            {
                insurances = insurances.OrderByDescending(m => m.NoOfDaysExpired).ToList();
            }
            else
            {
                insurances = insurances.OrderByDescending(m => m.CreatedDate).ToList();
            }

            List<Month> months = new List<Month>();

            for (int i = 1; i <= Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ExtendExpiryLimit"]); i++)
            {
                months.Add(new Month { Label = i });
            }
            ViewBag.ExtendedMonths = new SelectList(months, "Label", "Label");

            return View(insurances);
        }

        // GET: InsuranceTracker/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Insurance insurance = db.Insurances.Find(id);
            if (insurance == null)
            {
                return HttpNotFound();
            }
            return View(insurance);
        }

        // GET: InsuranceTracker/Create
        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.CustomerDetails.Where(m => m.OrganizationId == orgId && m.Department == deptID), "CustomerId", "CustomerName");
            ViewBag.DocumentId = new SelectList(new List<FilledTemplateDetail>(), "RowId", "FilledTemplateName");
            ViewBag.Asset = new SelectList(new List<Property>(), "Id", "PropertyName");
            return View();
        }

        // POST: InsuranceTracker/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,PropertyId,Insurer,AmountInsured,DateOfInsurance,DateOfExpiry,Asset,CustomerId,DocumentId,Currency")] InsuranceViewModel insuranceViewModel)
        {
            try
            {
                ModelState.Remove("DocumentId");
                ModelState.Remove("CustomerId");
                if (ModelState.IsValid)
                {
                    var data = db.Insurances.Where(m => m.PropertyId == insuranceViewModel.Asset && m.Status);

                    if (data != null && data.Count() > 0)
                    {
                        //Insurance already added for the property
                        ModelState.AddModelError("PageError", "This property is already been insured.");
                    }
                    else
                    {
                        var insurance = new Insurance
                        {
                            PropertyId = insuranceViewModel.Asset,
                            Insurer = insuranceViewModel.Insurer,
                            AmountInsured = insuranceViewModel.AmountInsured,
                            DateOfInsurance = DateTime.ParseExact(insuranceViewModel.DateOfInsurance, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                            DateOfExpiry = DateTime.ParseExact(insuranceViewModel.DateOfExpiry, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                            CreatedDate = DateTime.Now,
                            Status = true,
                            UserId = userID,
                            Currency = insuranceViewModel.Currency
                        };
                        db.Insurances.Add(insurance);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }

                }
            }
            catch (Exception ex)
            {

            }

            ViewBag.CustomerId = new SelectList(db.CustomerDetails.Where(m => m.OrganizationId == orgId && m.Department == deptID),
                "CustomerId",
                "CustomerName",
                insuranceViewModel.CustomerId == 0 ? "" : insuranceViewModel.CustomerId.ToString());

            if (insuranceViewModel.CustomerId != 0)
            {
                var templates = from document in db.FilledTemplateDetails.Where(m => m.CustomerId == insuranceViewModel.CustomerId)
                                join template in db.DocumentTemplates
                                on document.TemplateId equals template.TemplateId
                                select new { template.TemplateId, template.DocumentTitle };

                ViewBag.DocumentId = new SelectList(templates,
                    "TemplateId",
                    "DocumentTitle",
                    insuranceViewModel.DocumentId == 0 ? "" : insuranceViewModel.DocumentId.ToString());
            }
            else
            {
                ViewBag.DocumentId = new SelectList(new List<DocumentTemplate>(), "TemplateId", "DocumentTitle");
            }

            if (insuranceViewModel.DocumentId != 0 && insuranceViewModel.CustomerId != 0)
            {
                var properties = from document in db.FilledTemplateDetails.Where(m => m.TemplateId == insuranceViewModel.DocumentId)
                                 join property in db.Properties
                                 on document.RowId equals property.DocumentId
                                 where document.CustomerId == insuranceViewModel.CustomerId
                                 select new { property.Id, property.PropertyName };

                ViewBag.Asset = new SelectList(properties,
                    "Id",
                    "PropertyName",
                    insuranceViewModel.Asset == 0 ? "" : insuranceViewModel.Asset.ToString());
            }
            else
            {
                ViewBag.Asset = new SelectList(new List<Property>(), "Id", "PropertyName");
            }

            return View(insuranceViewModel);
        }

        // GET: InsuranceTracker/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Insurance insurance = db.Insurances.Include(m => m.Property).Include(m => m.Property.FilledTemplateDetail).FirstOrDefault(m => m.Id == id);
            var template = db.DocumentTemplates.FirstOrDefault(m => m.TemplateId == insurance.Property.FilledTemplateDetail.TemplateId);
            var insuranceViewModel = new InsuranceViewModel
            {
                Id = insurance.Id,
                PropertyId = insurance.PropertyId,
                DocumentTitle = template.DocumentTitle,
                AmountInsured = insurance.AmountInsured,
                AssetInsured = insurance.Property.PropertyName,
                CustomerName = db.CustomerDetails.FirstOrDefault(m => m.CustomerId == insurance.Property.FilledTemplateDetail.CustomerId).CustomerName,
                DateOfExpiry = insurance.DateOfExpiry.ToString("dd-MM-yyyy"),
                DateOfInsurance = insurance.DateOfInsurance.ToString("dd-MM-yyyy"),
                Insurer = insurance.Insurer,
                Currency = insurance.Currency
            };
            if (insurance == null)
            {
                return HttpNotFound();
            }

            List<Month> months = new List<Month>();

            for (int i = 1; i <= Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ExtendExpiryLimit"]); i++)
            {
                months.Add(new Month { Label = i });
            }
            ViewBag.ExtendedMonths = new SelectList(months, "Label", "Label");
            ViewBag.PropertyId = new SelectList(db.Properties, "Id", "PropertyName", insurance.PropertyId);
            return View(insuranceViewModel);
        }

        // POST: InsuranceTracker/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PropertyId,Insurer,AmountInsured,DateOfInsurance,DateOfExpiry,Status,ExtendedMonths,Currency")] InsuranceViewModel insuranceViewModel)
        {
            ModelState.Remove("DateOfExpiry");
            ModelState.Remove("ExtendedMonths");
            if (ModelState.IsValid)
            {
                var insurance = db.Insurances.FirstOrDefault(m => m.Id == insuranceViewModel.Id);
                insurance.Insurer = insuranceViewModel.Insurer;
                insurance.AmountInsured = insuranceViewModel.AmountInsured;
                insurance.DateOfInsurance = DateTime.ParseExact(insuranceViewModel.DateOfInsurance, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                insurance.Currency = insuranceViewModel.Currency;

                if (insuranceViewModel.ExtendedMonths != 0)
                {
                    insurance.DateOfExpiry = insurance.DateOfExpiry.AddMonths(insuranceViewModel.ExtendedMonths);
                }

                db.Entry(insurance).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.PropertyId = new SelectList(db.Properties, "Id", "PropertyName", insurance.PropertyId);
            List<Month> months = new List<Month>();

            for (int i = 1; i <= Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ExtendExpiryLimit"]); i++)
            {
                months.Add(new Month { Label = i });
            }
            ViewBag.ExtendedMonths = new SelectList(months, "Label", "Label");
            return View();
        }

        public ActionResult BulkInsuranceUpload()
        {
            return View();
        }

        [HttpPost]
        public JsonResult PostBulkInsuranceUpload()
        {
            try
            {
                int i = 1;
                BulkInsuranceJsonResponse bulkInsuranceJsonResponse = new BulkInsuranceJsonResponse();
                HttpFileCollectionBase files = Request.Files;
                HttpPostedFileBase file = files[0];
                var fileData = Utility.ReadExcelFile(file);


                bulkInsuranceJsonResponse.TotalRecords = fileData.Rows.Count;

                Insurance insuranceDetail = new Insurance();
                DateTime date = DateTime.Now;
                double res;
                //var user = db.UserProfiles.FirstOrDefault(m => m.UserID == userID);
                //var users = db.UserProfiles.Where(m => m.Department == deptID && m.OrganizationId == orgId)
                //    .Select(s => new { s.UserID, s.OrganizationId, s.Department });

                foreach (DataRow dr in fileData.Rows)
                {
                    string doi = string.Empty;
                    string doe = string.Empty;

                    if (!string.IsNullOrEmpty(dr[5].ToString()) && !string.IsNullOrEmpty(dr[6].ToString()))
                    {
                        try
                        {
                            doi = Utility.GetDate(dr[5].ToString());
                            doe = Utility.GetDate(dr[6].ToString());
                        }
                        catch(Exception ex)
                        {
                            doi = string.Empty;
                            doe = string.Empty;
                        }
                    }

                    //if (!string.IsNullOrEmpty(dr[5].ToString()) && !string.IsNullOrEmpty(dr[6].ToString()))
                    //{
                    //    try
                    //    {
                    //        if (!double.TryParse(dr[5].ToString(), out res))
                    //        {
                    //            var dateTimeDoi = DateTime.ParseExact(dr[5].ToString(), new string[] { "dd-MM-yyyy", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
                    //            var timeZoneDt = DateTime.SpecifyKind(dateTimeDoi, DateTimeKind.Utc);

                    //            doi = timeZoneDt.ToString("dd-MM-yyyy");

                    //        }
                    //        else if(double.TryParse(dr[5].ToString(), out res))
                    //        {
                    //            try
                    //            {
                    //                var convertedDate = DateTime.FromOADate(Convert.ToDouble(dr[5].ToString().Trim()));
                    //                doi = DateTime.SpecifyKind(convertedDate, DateTimeKind.Utc).ToString("dd-MM-yyyy");
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                doi = string.Empty;
                    //            }
                    //        }

                    //        if (!double.TryParse(dr[6].ToString(), out res))
                    //        {
                    //            var dateTimeDoe = DateTime.ParseExact(dr[6].ToString(), new string[] { "dd-MM-yyyy", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
                    //            var timeZoneDt = DateTime.SpecifyKind(dateTimeDoe, DateTimeKind.Utc);

                    //            doe = timeZoneDt.ToString("dd-MM-yyyy");

                    //        }
                    //        else if(double.TryParse(dr[6].ToString(), out res))
                    //        {
                    //            try
                    //            {
                    //                var convertedDate = DateTime.FromOADate(Convert.ToDouble(dr[6].ToString().Trim()));
                    //                doe = DateTime.SpecifyKind(convertedDate, DateTimeKind.Utc).ToString("dd-MM-yyyy");
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                doe = string.Empty;
                    //            }
                    //        }

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        doi = string.Empty;
                    //        doe = string.Empty;
                    //    }
                    //}

                    //if (!string.IsNullOrEmpty(dr[5].ToString()) && !string.IsNullOrEmpty(dr[6].ToString()))
                    //{
                    //    try
                    //    {
                    //        if (DateTime.TryParseExact(dr[5].ToString().Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) 
                    //            && DateTime.TryParseExact(dr[6].ToString().Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    //        {
                    //            doi = Convert.ToDateTime(dr[5].ToString().Trim()).ToString("dd-MM-yyyy");
                    //            doe = Convert.ToDateTime(dr[6].ToString().Trim()).ToString("dd-MM-yyyy");
                    //        }
                    //        else
                    //        {
                    //            try
                    //            {
                    //                doi = DateTime.FromOADate(Convert.ToDouble(dr[5].ToString().Trim())).ToString("dd-MM-yyyy");
                    //                doe = DateTime.FromOADate(Convert.ToDouble(dr[6].ToString().Trim())).ToString("dd-MM-yyyy");
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                doi = string.Empty;
                    //                doe = string.Empty;
                    //            }
                    //        }

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        doi = string.Empty;
                    //        doe = string.Empty;
                    //    }
                    //}

                    if (string.IsNullOrEmpty(dr[0].ToString()) || string.IsNullOrEmpty(dr[2].ToString()) || string.IsNullOrEmpty(dr[3].ToString())
                    || string.IsNullOrEmpty(dr[4].ToString()) || string.IsNullOrEmpty(dr[5].ToString()) || string.IsNullOrEmpty(dr[6].ToString()))
                    {
                        //fields are mandatory
                        bulkInsuranceJsonResponse.Failure++;
                        bulkInsuranceJsonResponse.Errors.Add(new Error
                        {
                            RecordNumber = i,
                            Description = "Name, Asset Insured, Insurer, Amount Insured, Date of insurance, Date of Expiry fields are mandatory.",
                            Name = dr[0].ToString().Trim(),
                            AssetInsured = dr[2].ToString().Trim(),
                            Document = dr[1].ToString().Trim()
                        });
                    }
                    else if (string.IsNullOrEmpty(doi) || string.IsNullOrEmpty(doe))
                    {
                        //date of expiry should be greater than date of insurance
                        bulkInsuranceJsonResponse.Failure++;
                        bulkInsuranceJsonResponse.Errors.Add(new Error
                        {
                            RecordNumber = i,
                            Description = "Date is not in correct format.",
                            Name = dr[0].ToString().Trim(),
                            AssetInsured = dr[2].ToString().Trim(),
                            Document = dr[1].ToString().Trim()
                        });
                    }
                    //else if (Convert.ToDateTime(doi) > Convert.ToDateTime(doe))
                    else if (DateTime.ParseExact(doi, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None) > DateTime.ParseExact(doe, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None))
                    {
                        //date of expiry should be greater than date of insurance
                        bulkInsuranceJsonResponse.Failure++;
                        bulkInsuranceJsonResponse.Errors.Add(new Error
                        {
                            RecordNumber = i,
                            Description = "Date of expiry should be greater than date of insurance.",
                            Name = dr[0].ToString().Trim(),
                            AssetInsured = dr[2].ToString().Trim(),
                            Document = dr[1].ToString().Trim()
                        });
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(dr[0].ToString()))
                        {
                            var custName = dr[0].ToString().ToLower().Trim();
                            var customer = db.CustomerDetails.FirstOrDefault(m => m.CustomerName.ToLower() == custName && m.OrganizationId == orgId && m.Department == deptID);
                            var propertyName = dr[2].ToString().Trim().ToLower();
                            if (customer != null)
                            {
                                if (!string.IsNullOrEmpty(dr[1].ToString()))
                                {
                                    var docName = dr[1].ToString().Trim().ToLower();
                                    var templates = db.DocumentTemplates.Where(m => m.DocumentTitle.ToLower() == docName && m.DepartmentID == deptID).FirstOrDefault();
                                    List<FilledTemplateDetail> documents = new List<FilledTemplateDetail>();
                                    var property = new Property();
                                    int docCount = 0;
                                    if (templates != null)
                                    {
                                        documents = db.FilledTemplateDetails.Where(m => m.TemplateId == templates.TemplateId && m.CustomerId == customer.CustomerId).ToList();
                                    }

                                    if (documents != null && documents.Count() > 0)
                                    {
                                        foreach (var doc in documents)
                                        {
                                            var tempProperty = db.Properties.FirstOrDefault(m => m.DocumentId == doc.RowId && m.PropertyName.ToLower() == propertyName);
                                            if (tempProperty != null)
                                            {
                                                property = tempProperty;
                                                docCount++;
                                            }
                                        }

                                        if (docCount > 1)
                                        {
                                            //multiple document found
                                            bulkInsuranceJsonResponse.Failure++;
                                            bulkInsuranceJsonResponse.Errors.Add(new Error
                                            {
                                                RecordNumber = i,
                                                Description = "Multiple document found.",
                                                Name = dr[0].ToString().Trim(),
                                                AssetInsured = dr[2].ToString().Trim(),
                                                Document = dr[1].ToString().Trim()
                                            });
                                        }
                                        else if (docCount == 1)
                                        {
                                            //document found

                                            if (property != null)
                                            {
                                                var customerName = dr[0].ToString().Trim();
                                                //var customer = db.CustomerDetails.FirstOrDefault(m => m.CustomerName == customerName);

                                                var insurance = db.Insurances.Where(m => m.PropertyId == property.Id && m.Status);

                                                if (insurance != null && insurance.Count() > 0)
                                                {
                                                    //insurance already added
                                                    bulkInsuranceJsonResponse.Failure++;
                                                    bulkInsuranceJsonResponse.Errors.Add(new Error
                                                    {
                                                        RecordNumber = i,
                                                        Description = "Insurance already added.",
                                                        Name = dr[0].ToString().Trim(),
                                                        AssetInsured = dr[2].ToString().Trim(),
                                                        Document = dr[1].ToString().Trim()
                                                    });
                                                }
                                                else
                                                {
                                                    insuranceDetail.PropertyId = property.Id;
                                                    insuranceDetail.Insurer = dr[3].ToString().Trim();
                                                    insuranceDetail.AmountInsured = dr[4].ToString().Trim();
                                                    insuranceDetail.DateOfInsurance = DateTime.ParseExact(doi, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                                    insuranceDetail.DateOfExpiry = DateTime.ParseExact(doe, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                                    insuranceDetail.CreatedDate = DateTime.Now;
                                                    insuranceDetail.ModifiedDate = DateTime.Now;
                                                    insuranceDetail.Status = true;
                                                    insuranceDetail.UserId = userID;

                                                    db.Insurances.Add(insuranceDetail);
                                                    db.SaveChanges();
                                                    bulkInsuranceJsonResponse.Success++;
                                                }
                                            }
                                            else
                                            {
                                                //property not found
                                                bulkInsuranceJsonResponse.Failure++;
                                                bulkInsuranceJsonResponse.Errors.Add(new Error
                                                {
                                                    RecordNumber = i,
                                                    Description = "Property not found.",
                                                    Name = dr[0].ToString().Trim(),
                                                    AssetInsured = dr[2].ToString().Trim(),
                                                    Document = dr[1].ToString().Trim()
                                                });
                                            }
                                        }
                                        else
                                        {
                                            //document not found
                                            bulkInsuranceJsonResponse.Failure++;
                                            bulkInsuranceJsonResponse.Errors.Add(new Error
                                            {
                                                RecordNumber = i,
                                                Description = "Property not found.",
                                                Name = dr[0].ToString().Trim(),
                                                AssetInsured = dr[2].ToString().Trim(),
                                                Document = dr[1].ToString().Trim()
                                            });
                                        }
                                    }
                                    else
                                    {
                                        //document not found
                                        bulkInsuranceJsonResponse.Failure++;
                                        bulkInsuranceJsonResponse.Errors.Add(new Error
                                        {
                                            RecordNumber = i,
                                            Description = "Document not found.",
                                            Name = dr[0].ToString().Trim(),
                                            AssetInsured = dr[2].ToString().Trim(),
                                            Document = dr[1].ToString().Trim()
                                        });
                                    }

                                }
                                else
                                {
                                    //var propertyName = dr[2].ToString().ToLower();
                                    Property property = null;
                                    int docCount = 0;
                                    var documents = db.FilledTemplateDetails.Where(m => m.CustomerId == customer.CustomerId);

                                    if (documents != null && documents.Count() > 0)
                                    {
                                        foreach (var doc in documents)
                                        {
                                            var tempProperty = db.Properties.FirstOrDefault(m => m.DocumentId == doc.RowId && m.PropertyName.ToLower() == propertyName);
                                            if (tempProperty != null)
                                            {
                                                property = tempProperty;
                                                docCount++;
                                            }
                                        }

                                        if (docCount > 1)
                                        {
                                            //multiple document found
                                            bulkInsuranceJsonResponse.Failure++;
                                            bulkInsuranceJsonResponse.Errors.Add(new Error
                                            {
                                                RecordNumber = i,
                                                Description = "Multiple document found.",
                                                Name = dr[0].ToString().Trim(),
                                                AssetInsured = dr[2].ToString().Trim(),
                                                Document = dr[1].ToString().Trim()
                                            });
                                        }
                                        else if (docCount == 1)
                                        {
                                            //document found

                                            if (property != null)
                                            {
                                                var customerName = dr[0].ToString().Trim();
                                                //var customer = db.CustomerDetails.FirstOrDefault(m => m.CustomerName == customerName);

                                                var insurance = db.Insurances.Where(m => m.PropertyId == property.Id && m.Status);

                                                if (insurance != null && insurance.Count() > 0)
                                                {
                                                    //insurance already added
                                                    bulkInsuranceJsonResponse.Failure++;
                                                    bulkInsuranceJsonResponse.Errors.Add(new Error
                                                    {
                                                        RecordNumber = i,
                                                        Description = "Insurance already added.",
                                                        Name = dr[0].ToString().Trim(),
                                                        AssetInsured = dr[2].ToString().Trim(),
                                                        Document = dr[1].ToString().Trim()
                                                    });
                                                }
                                                else
                                                {
                                                    insuranceDetail.PropertyId = property.Id;
                                                    insuranceDetail.Insurer = dr[3].ToString().Trim();
                                                    insuranceDetail.AmountInsured = dr[4].ToString().Trim();
                                                    insuranceDetail.DateOfInsurance = DateTime.FromOADate(Convert.ToDouble(dr[5].ToString().Trim()));
                                                    insuranceDetail.DateOfExpiry = DateTime.FromOADate(Convert.ToDouble(dr[6].ToString().Trim()));
                                                    insuranceDetail.CreatedDate = DateTime.Now;
                                                    insuranceDetail.ModifiedDate = DateTime.Now;
                                                    insuranceDetail.Status = true;
                                                    insuranceDetail.UserId = userID;

                                                    db.Insurances.Add(insuranceDetail);
                                                    db.SaveChanges();
                                                    bulkInsuranceJsonResponse.Success++;
                                                }
                                            }
                                            else
                                            {
                                                //property not found
                                                bulkInsuranceJsonResponse.Failure++;
                                                bulkInsuranceJsonResponse.Errors.Add(new Error
                                                {
                                                    RecordNumber = i,
                                                    Description = "Property not found.",
                                                    Name = dr[0].ToString().Trim(),
                                                    AssetInsured = dr[2].ToString().Trim(),
                                                    Document = dr[1].ToString().Trim()
                                                });
                                            }
                                        }
                                        else
                                        {
                                            //document not found
                                            bulkInsuranceJsonResponse.Failure++;
                                            bulkInsuranceJsonResponse.Errors.Add(new Error
                                            {
                                                RecordNumber = i,
                                                Description = "Document not found.",
                                                Name = dr[0].ToString().Trim(),
                                                AssetInsured = dr[2].ToString().Trim(),
                                                Document = dr[1].ToString().Trim()
                                            });
                                        }
                                    }
                                    else
                                    {
                                        //document not found
                                        bulkInsuranceJsonResponse.Failure++;
                                        bulkInsuranceJsonResponse.Errors.Add(new Error
                                        {
                                            RecordNumber = i,
                                            Description = "Document not found.",
                                            Name = dr[0].ToString().Trim(),
                                            AssetInsured = dr[2].ToString().Trim(),
                                            Document = dr[1].ToString().Trim()
                                        });
                                    }
                                }
                            }
                            else
                            {
                                //customer not found
                                bulkInsuranceJsonResponse.Failure++;
                                bulkInsuranceJsonResponse.Errors.Add(new Error
                                {
                                    RecordNumber = i,
                                    Description = "Customer not found.",
                                    Name = dr[0].ToString().Trim(),
                                    AssetInsured = dr[2].ToString().Trim(),
                                    Document = dr[1].ToString().Trim()
                                });
                            }
                        }
                        else
                        {
                            //customer not found
                            bulkInsuranceJsonResponse.Failure++;
                            bulkInsuranceJsonResponse.Errors.Add(new Error
                            {
                                RecordNumber = i,
                                Description = "Customer not found.",
                                Name = dr[0].ToString().Trim(),
                                AssetInsured = dr[2].ToString().Trim(),
                                Document = dr[1].ToString().Trim()
                            });
                        }

                    }
                    i++;
                }
                return Json(bulkInsuranceJsonResponse);
            }
            catch (Exception ex)
            {
                return Json("Message: " + ex.Message + "\nStack Trace: " + ex.StackTrace);
            }
        }


        public JsonResult GetDocumentsByCustomer(int id)
        {
            try
            {
                var templates = (from filled in db.FilledTemplateDetails
                                 join template in db.DocumentTemplates
                                 on filled.TemplateId equals template.TemplateId
                                 where filled.CustomerId == id
                                 select new { template.TemplateId, template.DocumentTitle })
                                .Distinct();
                //var documents = db.FilledTemplateDetails.Where(m => m.CustomerId == id)
                //    .Select(s => new { s.RowId, s.FilledTemplateName })
                //    .ToList();

                return Json(templates, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAssetsByDocument(AssetParam param)
        {
            try
            {
                var assets = from doc in db.FilledTemplateDetails.Where(m => m.TemplateId == param.Id)
                             join property in db.Properties
                             on doc.RowId equals property.DocumentId
                             where doc.CustomerId == param.CustomerId
                             select new { property.Id, property.PropertyName };

                //var documents = db.FilledTemplateDetails.Where(m => m.TemplateId == id);
                //var assets = db.Properties.Where(m => m.DocumentId == id)
                //    .Select(s => new { s.Id, s.PropertyName })
                //    .ToList();

                return Json(assets, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ExtendExpiry(IEnumerable<Extend> months)
        {
            try
            {
                if (months.Count() > 0)
                {
                    foreach (var item in months)
                    {
                        var insurance = db.Insurances.FirstOrDefault(m => m.Id == item.Id);
                        insurance.DateOfExpiry = insurance.DateOfExpiry.AddMonths(item.Month);
                        db.Entry(insurance).State = EntityState.Modified;

                    }
                    db.SaveChanges();
                }

                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }

        }

        // GET: InsuranceTracker/Delete/5
        public JsonResult Delete(int id)
        {
            try
            {
                if (id != 0)
                {
                    Insurance insurance = db.Insurances.Find(id);
                    insurance.Status = false;
                    db.Entry(insurance).State = EntityState.Modified;
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

        // POST: InsuranceTracker/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Insurance insurance = db.Insurances.Find(id);
            db.Insurances.Remove(insurance);
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
