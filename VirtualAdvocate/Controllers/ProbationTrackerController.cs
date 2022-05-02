#region NameSpaces
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
#endregion
#region VirtualAdvocate.Controllers
namespace VirtualAdvocate.Controllers
{
    #region ProbationTrackerController
    public class ProbationTrackerController : BaseController
    {
        #region Global Variables
        public int userID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
        public int orgId = Convert.ToInt32(System.Web.HttpContext.Current.Session["OrgId"]);
        public int deptID = Convert.ToInt32(System.Web.HttpContext.Current.Session["DepartmentID"]);
        public int roleId = Convert.ToInt32(System.Web.HttpContext.Current.Session["RoleId"]);

        #endregion

        #region Index
        // GET: ProbationTracker
        public ActionResult Index(int? flagForNotification)
        {
            var probations = new List<ProbationViewModel>();
            foreach (var item in VAEDB.ProbationDetails.Include("CustomerDetail").Where(m => m.Status && m.CustomerDetail.OrganizationId == orgId && m.CustomerDetail.Department == deptID))
            {
                probations.Add(new ProbationViewModel
                {
                    Name = item.CustomerDetail.CustomerName,
                    Id = item.Id,
                    DateOfJoining = item.DateOfJoining.ToString("dd-MM-yyyy"),
                    ProbationPeriod = item.ProbationPeriod,
                    CreatedDate = item.CreatedDate,
                    ProbationPeriodExpiredOn = item.DateOfExpiry,
                    NoOfDaysExpired = (DateTime.Today - item.DateOfExpiry).Days > 0 ? (DateTime.Today - item.DateOfExpiry).Days : 0,
                    Status = ((DateTime.Now - item.DateOfExpiry).Days > 0) ? InsuranceStatus.Expired : InsuranceStatus.Valid,

                });
            }

            if (flagForNotification != null && flagForNotification == 1)
            {
                probations = probations.OrderByDescending(m => m.NoOfDaysExpired).ToList();
            }
            else
            {
                probations = probations.OrderByDescending(m => m.CreatedDate).ToList();
            }

            List<Month> months = new List<Month>();

            for (int i = 1; i <= Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ExtendExpiryLimit"]); i++)
            {
                months.Add(new Month { Label = i });
            }
            ViewBag.ExtendedMonths = new SelectList(months, "Label", "Label");

            return View(probations);
        }
        #endregion

        #region Details
        // GET: ProbationTracker/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProbationDetail probationDetail = VAEDB.ProbationDetails.Find(id);
            if (probationDetail == null)
            {
                return HttpNotFound();
            }
            return View(probationDetail);
        }
        #endregion

        #region Create
        // GET: ProbationTracker/Create
        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId && m.Department == deptID), "CustomerId", "CustomerName");
            List<Month> months = new List<Month>();

            for (int i = 1; i <= Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ExtendExpiryLimit"]); i++)
            {
                months.Add(new Month { Label = i });
            }
            ViewBag.ProbationPeriod = new SelectList(months, "Label", "Label");
            return View();
        }
        #endregion

        #region Create
        // POST: ProbationTracker/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProbationViewModel probationDetail)
        {
            List<Month> months = new List<Month>();
            if (ModelState.IsValid)
            {
                var probationData = VAEDB.ProbationDetails.Where(m => m.CustomerId == probationDetail.CustomerId && m.Status);
                if (probationData.Count() > 0)
                {
                    ViewBag.CustomerId = new SelectList(VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId), "CustomerId", "CustomerName");


                    for (int i = 1; i <= Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ExtendExpiryLimit"]); i++)
                    {
                        months.Add(new Month { Label = i });
                    }
                    ViewBag.ProbationPeriod = new SelectList(months, "Label", "Label");
                    ModelState.AddModelError("PageError", "Probation is already been added for this customer.");

                    return View(probationDetail);
                }

                var doj = DateTime.ParseExact(probationDetail.DateOfJoining, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                ProbationDetail probation = new ProbationDetail
                {
                    CustomerId = probationDetail.CustomerId,
                    DateOfExpiry = doj.AddMonths(probationDetail.ProbationPeriod),
                    DateOfJoining = doj,
                    ProbationPeriod = probationDetail.ProbationPeriod,
                    CreatedDate = DateTime.Now,
                    Status = true,
                    UserId = userID
                };
                VAEDB.ProbationDetails.Add(probation);
                VAEDB.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId), "CustomerId", "CustomerName");
            //List<Month> months = new List<Month>();

            for (int i = 1; i <= Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ExtendExpiryLimit"]); i++)
            {
                months.Add(new Month { Label = i });
            }
            ViewBag.ProbationPeriod = new SelectList(months, "Label", "Label");

            return View(probationDetail);
        }
        #endregion

        #region Edit
        // GET: ProbationTracker/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var probationDetail = VAEDB.ProbationDetails.Find(id);
            ProbationViewModel probation = new ProbationViewModel()
            {
                Id = probationDetail.Id,
                CustomerId = probationDetail.CustomerId,
                DateOfExpiry = probationDetail.DateOfExpiry,
                DateOfJoining = probationDetail.DateOfJoining.ToString("dd-MM-yyyy"),
                ProbationPeriod = probationDetail.ProbationPeriod

            };

            if (probationDetail == null)
            {
                return HttpNotFound();
            }
            List<Month> months = new List<Month>();

            for (int i = 1; i <= Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ExtendExpiryLimit"]); i++)
            {
                months.Add(new Month { Label = i });
            }
            ViewBag.ProbationPeriod = new SelectList(months, "Label", "Label", probation.ProbationPeriod);
            ViewBag.ExtendExpiry = new SelectList(months, "Label", "Label");
            ViewBag.CustomerId = new SelectList(VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId), "CustomerId", "CustomerName", probation.CustomerId);

            return View(probation);
        }
        #endregion

        #region Edit
        // POST: ProbationTracker/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProbationViewModel probationModel)
        {
            ModelState.Remove("CustomerId");
            ModelState.Remove("ExtendExpiry");

            if (ModelState.IsValid)
            {
                var probationDetail = VAEDB.ProbationDetails.FirstOrDefault(x => x.Id == probationModel.Id);
                probationDetail.DateOfJoining = DateTime.ParseExact(probationModel.DateOfJoining, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                probationDetail.DateOfExpiry = probationDetail.DateOfExpiry.AddMonths(probationModel.ExtendExpiry);

                VAEDB.Entry(probationDetail).State = EntityState.Modified;
                VAEDB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(probationModel);
        }
        #endregion

        #region ExtendExpiry
        [HttpPost]
        public JsonResult ExtendExpiry(IEnumerable<Extend> months)
        {
            try
            {
                if (months.Count() > 0)
                {
                    foreach (var item in months)
                    {
                        var probation = VAEDB.ProbationDetails.FirstOrDefault(m => m.Id == item.Id);
                        probation.DateOfExpiry = probation.DateOfExpiry.AddMonths(item.Month);
                        VAEDB.Entry(probation).State = EntityState.Modified;

                    }
                    VAEDB.SaveChanges();
                }

                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region BulkProbationUpload
        public ActionResult BulkProbationUpload()
        {
            return View();
        }
        #endregion

        #region PostBulkProbationUpload
        [HttpPost]
        public JsonResult PostBulkProbationUpload()
        {
            try
            {
                int i = 1;
                BulkInsuranceJsonResponse bulkInsuranceJsonResponse = new BulkInsuranceJsonResponse();
                HttpFileCollectionBase files = Request.Files;
                HttpPostedFileBase file = files[0];
                var fileData = Utility.ReadExcelFile(file);
                string joiningDate = string.Empty;
                DateTime date = DateTime.Now;
                double res;

                bulkInsuranceJsonResponse.TotalRecords = fileData.Rows.Count;

                foreach (DataRow dr in fileData.Rows)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(dr[1].ToString()))
                        {
                            try
                            {
                                if (!double.TryParse(dr[1].ToString(), out res))
                                {
                                    var dateTime = DateTime.ParseExact(dr[1].ToString(), new string[] { "dd-MM-yyyy", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
                                    var timeZoneDt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                                    joiningDate = timeZoneDt.ToString("dd-MM-yyyy");

                                }
                                else
                                {
                                    try
                                    {
                                        var convertedDate = DateTime.FromOADate(Convert.ToDouble(dr[1].ToString().Trim()));
                                        joiningDate = DateTime.SpecifyKind(convertedDate, DateTimeKind.Utc).ToString("dd-MM-yyyy");
                                    }
                                    catch (Exception ex)
                                    {
                                        joiningDate = string.Empty;
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                joiningDate = string.Empty;
                            }
                        }
                        //if (!string.IsNullOrEmpty(dr[1].ToString()))
                        //{
                        //    try
                        //    {
                        //        if(DateTime.TryParseExact(dr[1].ToString().Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        //        {
                        //            joiningDate = Convert.ToDateTime(dr[1].ToString().Trim()).ToString("dd-MM-yyyy");
                        //        }
                        //        else
                        //        {
                        //            try
                        //            {
                        //                joiningDate = DateTime.FromOADate(Convert.ToDouble(dr[1].ToString().Trim())).ToString("dd-MM-yyyy");
                        //            }
                        //            catch(Exception ex)
                        //            {
                        //                joiningDate = string.Empty;
                        //            }
                        //        }

                        //    }
                        //    catch(Exception ex)
                        //    {
                        //        joiningDate = string.Empty;
                        //    }
                        //}

                        if (string.IsNullOrEmpty(dr[0].ToString()) || string.IsNullOrEmpty(dr[1].ToString()) || string.IsNullOrEmpty(dr[2].ToString()))
                        {
                            //all fields are mandatory
                            bulkInsuranceJsonResponse.Failure++;
                            bulkInsuranceJsonResponse.Errors.Add(new Error
                            {
                                RecordNumber = i,
                                Description = "All fields are mandatory.",
                                Name = dr[0].ToString().Trim(),
                                DateOfJoining = DateTime.TryParse(dr[1].ToString(), out date) ?
                                DateTime.FromOADate(Convert.ToDouble(dr[1].ToString().Trim())).ToString("dd-MM-yyyy") : string.Empty,

                                ProbationPeriod = dr[2].ToString().Trim()
                            });
                        }
                        else if (string.IsNullOrEmpty(joiningDate))
                        {
                            //date is not in correct format
                            bulkInsuranceJsonResponse.Failure++;
                            bulkInsuranceJsonResponse.Errors.Add(new Error
                            {
                                RecordNumber = i,
                                Description = "Date is not in correct format.",
                                Name = dr[0].ToString().Trim(),
                                DateOfJoining = DateTime.TryParse(dr[1].ToString(), out date) ? joiningDate : string.Empty,
                                ProbationPeriod = dr[2].ToString().Trim()
                            });
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                var custName = dr[0].ToString().Trim().ToLower();
                                var customer = VAEDB.CustomerDetails.Where(m => m.CustomerName.ToLower() == custName && m.OrganizationId == orgId && m.Department == deptID);

                                if (customer != null && customer.Count() > 1)
                                {
                                    //multiple customers found with the same name
                                    bulkInsuranceJsonResponse.Failure++;
                                    bulkInsuranceJsonResponse.Errors.Add(new Error
                                    {
                                        RecordNumber = i,
                                        Description = "Multiple customers found with the same name",
                                        Name = dr[0].ToString().Trim(),
                                        DateOfJoining = joiningDate,
                                        ProbationPeriod = dr[2].ToString().Trim()
                                    });
                                }
                                else if (customer == null || (customer != null && customer.Count() == 0))
                                {
                                    //customer not found
                                    bulkInsuranceJsonResponse.Failure++;
                                    bulkInsuranceJsonResponse.Errors.Add(new Error
                                    {
                                        RecordNumber = i,
                                        Description = "Customer Not Found",
                                        Name = dr[0].ToString().Trim(),
                                        DateOfJoining = joiningDate,
                                        ProbationPeriod = dr[2].ToString().Trim()
                                    });
                                }
                                else
                                {
                                    if (customer != null)
                                    {
                                        var dateOfJoining = DateTime.ParseExact(joiningDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                        var isAlreadyExist = VAEDB.ProbationDetails
                                            .Include("CustomerDetail")
                                            .Where(m => m.CustomerDetail.CustomerName.ToLower() == custName && m.DateOfJoining == dateOfJoining && m.Status).Count() > 0 ? true : false;

                                        if (isAlreadyExist)
                                        {
                                            //probation already added for this customer on this date
                                            bulkInsuranceJsonResponse.Failure++;
                                            bulkInsuranceJsonResponse.Errors.Add(new Error
                                            {
                                                RecordNumber = i,
                                                Description = "Probation already added for this customer on the given date.",
                                                Name = dr[0].ToString().Trim(),
                                                DateOfJoining = joiningDate,
                                                ProbationPeriod = dr[2].ToString().Trim()
                                            });
                                        }
                                        else
                                        {
                                            var probationPeriod = Convert.ToInt32(dr[2].ToString().Trim());
                                            var existingProbation = VAEDB.ProbationDetails
                                            .Include("CustomerDetail")
                                            .FirstOrDefault(m => m.CustomerId == customer.FirstOrDefault().CustomerId && m.Status);

                                            if (existingProbation != null)
                                            {
                                                existingProbation.ModifiedDate = DateTime.Now;
                                                existingProbation.DateOfJoining = dateOfJoining;
                                                existingProbation.DateOfExpiry = dateOfJoining.AddMonths(probationPeriod);
                                                existingProbation.ProbationPeriod = probationPeriod;

                                                VAEDB.Entry(existingProbation).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                var probation = new ProbationDetail
                                                {
                                                    CreatedDate = DateTime.Now,
                                                    CustomerId = customer.FirstOrDefault().CustomerId,
                                                    DateOfExpiry = dateOfJoining.AddMonths(probationPeriod),
                                                    DateOfJoining = dateOfJoining,
                                                    ProbationPeriod = probationPeriod,
                                                    Status = true,
                                                    UserId = userID,
                                                };

                                                VAEDB.ProbationDetails.Add(probation);

                                            }
                                            bulkInsuranceJsonResponse.Success++;
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
                                            DateOfJoining = joiningDate,
                                            ProbationPeriod = dr[2].ToString().Trim()
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
                                    DateOfJoining = joiningDate,
                                    ProbationPeriod = dr[2].ToString().Trim()
                                });
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        //something went wrong
                        bulkInsuranceJsonResponse.Failure++;
                        bulkInsuranceJsonResponse.Errors.Add(new Error
                        {
                            RecordNumber = i,
                            Description = "Something went wrong.",
                            Name = dr[0].ToString().Trim(),
                            DateOfJoining = joiningDate,
                            ProbationPeriod = dr[2].ToString().Trim()
                        });
                    }
                    i++;
                }

                VAEDB.SaveChanges();

                return Json(bulkInsuranceJsonResponse);
            }
            catch (Exception ex)
            {
                return Json("error");
            }
        }
        #endregion

        #region Delete
        // GET: ProbationTracker/Delete/5
        public JsonResult Delete(int id)
        {
            try
            {
                if (id != 0)
                {
                    ProbationDetail probation = VAEDB.ProbationDetails.Find(id);
                    probation.Status = false;
                    VAEDB.Entry(probation).State = EntityState.Modified;
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
        // POST: ProbationTracker/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProbationDetail probationDetail = VAEDB.ProbationDetails.Find(id);
            VAEDB.ProbationDetails.Remove(probationDetail);
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
