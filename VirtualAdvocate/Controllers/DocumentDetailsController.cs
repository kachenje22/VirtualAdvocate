#region NameSpaces
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using VirtualAdvocate.Models;
#endregion
#region VirtualAdvocate.Controllers
namespace VirtualAdvocate.Controllers
{
    #region DocumentDetailsController
    public class DocumentDetailsController : BaseController
    {
        #region Global Variables
        public int userID = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserId"]);
        public int orgId = Convert.ToInt32(System.Web.HttpContext.Current.Session["OrgId"]);
        public int deptID = Convert.ToInt32(System.Web.HttpContext.Current.Session["DepartmentID"]);
        public int roleId = Convert.ToInt32(System.Web.HttpContext.Current.Session["RoleId"]);

        #endregion

        #region Index
        // GET: DocumentDetails
        public ActionResult Index(int? flagForNotification)
        {
            var docs = VAEDB.DocumentDetails.Where(m => m.Status).Include(d => d.DocumentDetailsStatu).Include(d => d.FilledTemplateDetail).ToList();
            var customers = VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId && m.Department == deptID).Select(s => s.CustomerId).ToList();
            var documentDetails = docs.Where(m => (roleId == 5 && m.UserId == userID) || (roleId == 6 && customers.Exists(e => e == m.FilledTemplateDetail.CustomerId)));
            
            List<Status> statuses = new List<Status>();

            statuses = (from DocumentDetailStatus d in Enum.GetValues(typeof(DocumentDetailStatus))
                        select new Status { Id = (int)d, Name = d.ToString() }).ToList();

            List<DocumentDetailsViewModel> dc = new List<DocumentDetailsViewModel>();
            foreach (var d in documentDetails)
            {
                DocumentDetailsViewModel detailsViewModel = new DocumentDetailsViewModel();
                detailsViewModel.Id = d.Id;
                detailsViewModel.Documentname = d.FilledTemplateDetail.FilledTemplateName;
                detailsViewModel.Name = VAEDB.CustomerDetails.FirstOrDefault(m => m.CustomerId == d.FilledTemplateDetail.CustomerId).CustomerName;
                //Name = VAEDB.CustomerDetails.Where(y => y.CustomerId == VAEDB.FilledTemplateDetails.Where(x => x.RowId == d.DocumentId).Select(s => s.CustomerId).FirstOrDefault()).Select(s => s.CustomerName).FirstOrDefault().ToString(),
                detailsViewModel.Vendor = d.Vendor;
                detailsViewModel.Purpose = d.Purpose;
                detailsViewModel.DateHanded = d.DateHanded.ToString("dd-MM-yyyy");
                detailsViewModel.DateToBeSubmitted = d.DateToBeSubmitted.ToString("dd-MM-yyyy");
                detailsViewModel.DelayedBy = (DateTime.Now - d.DateToBeSubmitted).Days > 0 ? (DateTime.Now - d.DateToBeSubmitted).Days : 0;
                //detailsViewModel.DocumentStatusId = d.DocumentStatus;

                if (roleId == 5)
                {
                    var statusesForUser = statuses.Where(m => m.Id == 1 || m.Id == 3).ToList();
                    if (d.DocumentStatus == (int)DocumentDetailStatus.PendingApproval)
                    {
                        statusesForUser.Add(new Status { Id = d.DocumentStatus, Name = DocumentDetailStatus.PendingApproval.ToString() });
                        detailsViewModel.DocumentStatus = new SelectList(statusesForUser, "Id", "Name", d.DocumentStatus);
                    }
                    else if (d.DocumentStatus == (int)DocumentDetailStatus.Reject)
                    {
                        var statusesForReject = statuses.Where(m => m.Id == 3).ToList();
                        statusesForReject.Add(new Status { Id = d.DocumentStatus, Name = DocumentDetailStatus.Reject.ToString() });
                        detailsViewModel.DocumentStatus = new SelectList(statusesForReject, "Id", "Name", d.DocumentStatus);
                    }
                    else
                    {
                        detailsViewModel.DocumentStatus = new SelectList(statusesForUser, "Id", "Name", d.DocumentStatus);
                    }

                }
                else if (roleId == 6)
                {
                    if (d.DocumentStatus == (int)DocumentDetailStatus.PendingApproval)
                    {
                        var changeRequests = VAEDB.DocumentStatusChangeRequests.FirstOrDefault(m => m.DocumentDetailId == d.Id && !m.Status);
                        if (changeRequests != null)
                        {
                            var statusesForAdmin = statuses.Where(m => m.Id == (int)DocumentDetailStatus.PendingApproval || m.Id == (int)DocumentDetailStatus.Reject || m.Id == (int)DocumentDetailStatus.Accept);
                            detailsViewModel.DocumentStatus = new SelectList(statusesForAdmin, "Id", "Name", d.DocumentStatus);
                        }
                    }
                    else
                    {
                        var statusesForAdmin = statuses.Where(m => m.Id == d.DocumentStatus);
                        detailsViewModel.DocumentStatus = new SelectList(statusesForAdmin, "Id", "Name", d.DocumentStatus);
                    }

                }
                else
                {
                    detailsViewModel.DocumentStatus = new SelectList(statuses, "Id", "Name", d.DocumentStatus);
                }

                detailsViewModel.RejectionReason = d.RejectionReason;
                detailsViewModel.CreatedDate = DateTime.Now;

                dc.Add(detailsViewModel);

                //dc.Add(new DocumentDetailsViewModel
                //{
                //    Id = d.Id,
                //    Documentname = d.FilledTemplateDetail.FilledTemplateName,
                //    Name = VAEDB.CustomerDetails.FirstOrDefault(m => m.CustomerId == d.FilledTemplateDetail.CustomerId).CustomerName,
                //    //Name = VAEDB.CustomerDetails.Where(y => y.CustomerId == VAEDB.FilledTemplateDetails.Where(x => x.RowId == d.DocumentId).Select(s => s.CustomerId).FirstOrDefault()).Select(s => s.CustomerName).FirstOrDefault().ToString(),
                //    Vendor = d.Vendor,
                //    Purpose = d.Purpose,
                //    DateHanded = d.DateHanded,
                //    DateToBeSubmitted = d.DateToBeSubmitted,
                //    DelayedBy = (DateTime.Now - d.DateToBeSubmitted).Days > 0 ? (DateTime.Now - d.DateToBeSubmitted).Days : 0,
                //    DocumentStatus = new SelectList(statuses, "Id", "Name", d.DocumentStatus),
                //    RejectionReason = d.RejectionReason
                //});

            }

            ViewBag.RoleId = roleId;

            if (flagForNotification == 1)
            {
                dc = dc.OrderByDescending(m => m.DelayedBy).ToList();
            }
            else
            {
                dc = dc.OrderByDescending(m => m.CreatedDate).ToList();
            }

            return View(dc.ToList());
        }
        #endregion

        #region Details
        // GET: DocumentDetails/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentDetail documentDetail = VAEDB.DocumentDetails.Find(id);
            if (documentDetail == null)
            {
                return HttpNotFound();
            }
            return View(documentDetail);
        }
        #endregion

        #region Create
        // GET: DocumentDetails/Create
        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId && m.Department == deptID), "CustomerId", "CustomerName");
            ViewBag.DocumentStatus = new SelectList(new List<DocumentDetailsStatu>(), "Id", "Status");
            ViewBag.DocumentId = new SelectList(new List<FilledTemplateDetail>(), "RowId", "FilledTemplateName");
            return View();
        }
        #endregion

        #region Create
        // POST: DocumentDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DocumentDetailsViewModel documentDetail)
        {
            if (ModelState.IsValid)
            {
                if (documentDetail.DocumentId != 0)
                {
                    var document = VAEDB.DocumentDetails.Where(m => m.DocumentId == documentDetail.DocumentId && m.Status);
                    if (document.Count() > 0)
                    {
                        if (documentDetail.CustomerId != 0)
                            ViewBag.DocumentId = new SelectList(VAEDB.FilledTemplateDetails.Where(m => m.CustomerId == documentDetail.CustomerId), "RowId", "FilledTemplateName", documentDetail.DocumentId);
                        else
                            ViewBag.DocumentId = new SelectList(new List<FilledTemplateDetail>(), "RowId", "FilledTemplateName");

                        ViewBag.CustomerId = new SelectList(VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId && m.Department == deptID), "CustomerId", "CustomerName", documentDetail.CustomerId);

                        ModelState.AddModelError("PageError", "This document has already been added");

                        return View(documentDetail);
                    }
                }

                DocumentDetail D = new DocumentDetail()
                {
                    DateHanded = DateTime.ParseExact(documentDetail.DateHanded, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    DateToBeSubmitted = DateTime.ParseExact(documentDetail.DateToBeSubmitted, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    Vendor = documentDetail.Vendor,
                    Purpose = documentDetail.Purpose,
                    DocumentId = documentDetail.DocumentId,
                    DocumentStatus = (int)DocumentDetailStatus.Pending,
                    RejectionReason = string.Empty,
                    CreatedDate = DateTime.Now,
                    Status = true,
                    UserId = userID
                };
                VAEDB.DocumentDetails.Add(D);
                VAEDB.SaveChanges();
                return RedirectToAction("Index");
            }
            if (documentDetail.CustomerId != 0)
                ViewBag.DocumentId = new SelectList(VAEDB.FilledTemplateDetails.Where(m => m.CustomerId == documentDetail.CustomerId), "RowId", "FilledTemplateName", documentDetail.DocumentId);
            else
                ViewBag.DocumentId = new SelectList(new List<FilledTemplateDetail>(), "RowId", "FilledTemplateName");

            ViewBag.CustomerId = new SelectList(VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId && m.Department == deptID), "CustomerId", "CustomerName", documentDetail.CustomerId);
            return View(documentDetail);
        }
        #endregion

        #region Edit
        // GET: DocumentDetails/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentDetail documentDetail = VAEDB.DocumentDetails.Find(id);
            DocumentDetailsViewModel dc = new DocumentDetailsViewModel
            {
                Id = documentDetail.Id,
                DocumentId = documentDetail.DocumentId,
                DateHanded = documentDetail.DateHanded.ToString("dd-MM-yyyy"),
                DateToBeSubmitted = documentDetail.DateToBeSubmitted.ToString("dd-MM-yyyy"),
                Purpose = documentDetail.Purpose,
                Vendor = documentDetail.Vendor
            };

            ViewBag.DocumentId = new SelectList(VAEDB.FilledTemplateDetails, "RowId", "FilledTemplateName", documentDetail.DocumentId);
            ViewBag.CustomerId = new SelectList(VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId && m.Department == deptID), "CustomerId", "CustomerName",
                VAEDB.FilledTemplateDetails.FirstOrDefault(m => m.RowId == documentDetail.DocumentId).CustomerId);
            return View(dc);
        }
        #endregion

        #region Edit
        // POST: DocumentDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DocumentDetailsViewModel documentDetail)
        {

            if (ModelState.IsValid)
            {
                var dc = VAEDB.DocumentDetails.Find(documentDetail.Id);
                dc.Purpose = documentDetail.Purpose;
                dc.Vendor = documentDetail.Vendor;
                dc.DateHanded = DateTime.ParseExact(documentDetail.DateHanded, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                dc.DateToBeSubmitted = DateTime.ParseExact(documentDetail.DateToBeSubmitted, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                dc.ModifiedDate = DateTime.Now;

                VAEDB.Entry(dc).State = EntityState.Modified;
                VAEDB.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DocumentId = new SelectList(VAEDB.FilledTemplateDetails, "RowId", "FilledTemplateName", documentDetail.DocumentId);
            ViewBag.CustomerId = new SelectList(VAEDB.CustomerDetails.Where(m => m.OrganizationId == orgId && m.Department == deptID), "CustomerId", "CustomerName",
                VAEDB.FilledTemplateDetails.FirstOrDefault(m => m.RowId == documentDetail.DocumentId).CustomerId);

            return View(documentDetail);
        }
        #endregion

        #region GetDocumentsByCustomer
        public JsonResult GetDocumentsByCustomer(int id)
        {
            try
            {
                var templates = (from filled in VAEDB.FilledTemplateDetails
                                     //join template in VAEDB.DocumentTemplates
                                     //on filled.TemplateId equals template.TemplateId
                                 where filled.CustomerId == id
                                 select new { filled.RowId, filled.FilledTemplateName })
                                .Distinct();
                //var documents = VAEDB.FilledTemplateDetails.Where(m => m.CustomerId == id)
                //    .Select(s => new { s.RowId, s.FilledTemplateName })
                //    .ToList();

                return Json(templates, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region ChangeDocumentStatus
        public JsonResult ChangeDocumentStatus(ChangeStatusParam param)
        {
            try
            {
                if (roleId == 5)
                {
                    DocumentStatusChangeRequest request = new DocumentStatusChangeRequest
                    {
                        DocumentDetailId = param.DocumentId,
                        ChangeFrom = param.ChangeFrom,
                        ChangeTo = param.StatusId,
                        Status = false,
                        CreatedDate = DateTime.Now,
                        UserId = userID
                    };

                    VAEDB.DocumentStatusChangeRequests.Add(request);

                    var documentDetail = VAEDB.DocumentDetails.FirstOrDefault(m => m.Id == param.DocumentId);
                    documentDetail.DocumentStatus = (int)DocumentDetailStatus.PendingApproval;
                    documentDetail.ModifiedDate = DateTime.Now;

                    VAEDB.Entry(documentDetail).State = EntityState.Modified;

                    VAEDB.SaveChanges();

                    var mailParam = new SendMailParam
                    {
                        RoleId = roleId,
                        DocumentName = VAEDB.FilledTemplateDetails.FirstOrDefault(m => m.RowId == documentDetail.DocumentId).FilledTemplateName,
                        UserName = VAEDB.UserAddressDetails.FirstOrDefault(m => m.UserId == userID).FirstName,
                        ToUserName = "All",
                        ChangeFrom = ((DocumentDetailStatus)request.ChangeFrom).ToString(),
                        ChangeTo = ((DocumentDetailStatus)request.ChangeTo).ToString(),
                        CC = VAEDB.UserProfiles.Where(m => m.UserID == userID).Select(s => s.EmailAddress).ToList(),
                        To = VAEDB.UserProfiles.Where(m => m.RoleId == 6 && m.OrganizationId == orgId && m.Department == deptID).Select(s => s.EmailAddress).ToList()
                    };


                    Utility.SendMail(mailParam);

                    return Json(100, JsonRequestBehavior.AllowGet);
                }
                else if (roleId == 6)
                {
                    var documentDetails = VAEDB.DocumentDetails.FirstOrDefault(m => m.Id == param.DocumentId);

                    var changeRequest = VAEDB.DocumentStatusChangeRequests.FirstOrDefault(m => m.DocumentDetailId == param.DocumentId);

                    if (param.StatusId == (int)DocumentDetailStatus.Accept)
                    {
                        documentDetails.DocumentStatus = changeRequest.ChangeTo;
                        documentDetails.RejectionReason = string.Empty;
                    }
                    else
                    {
                        documentDetails.DocumentStatus = param.StatusId;
                        documentDetails.RejectionReason = param.RejectionReason;
                    }

                    VAEDB.Entry(documentDetails).State = EntityState.Modified;

                    var userid = changeRequest.UserId;
                    if (changeRequest != null)
                    {
                        VAEDB.Entry(changeRequest).State = EntityState.Deleted;
                    }

                    VAEDB.SaveChanges();

                    var mailParam = new SendMailParam
                    {
                        RoleId = roleId,
                        DocumentName = VAEDB.FilledTemplateDetails.FirstOrDefault(m => m.RowId == documentDetails.DocumentId).FilledTemplateName,
                        UserName = VAEDB.UserAddressDetails.FirstOrDefault(m => m.UserId == userid).FirstName,
                        ChangeFrom = ((DocumentDetailStatus)changeRequest.ChangeFrom).ToString(),
                        ChangeTo = ((DocumentDetailStatus)changeRequest.ChangeTo).ToString(),
                        CC = VAEDB.UserProfiles.Where(m => m.UserID == userID).Select(s => s.EmailAddress).ToList(),
                        To = VAEDB.UserProfiles.Where(m => m.UserID == userid).Select(s => s.EmailAddress).ToList(),
                        Status = (DocumentDetailStatus)param.StatusId,
                        RejectedReason = documentDetails.RejectionReason
                    };

                    Utility.SendMail(mailParam);

                    return Json(200, JsonRequestBehavior.AllowGet);
                }

                return Json(400, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(500, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Delete
        // GET: DocumentDetails/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentDetail documentDetail = VAEDB.DocumentDetails.Find(id);
            documentDetail.Status = false;
            VAEDB.Entry(documentDetail).State = EntityState.Modified;
            VAEDB.SaveChanges();
            if (documentDetail == null)
            {
                return HttpNotFound();
            }
            return Json(200, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region DeleteConfirmed
        // POST: DocumentDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DocumentDetail documentDetail = VAEDB.DocumentDetails.Find(id);
            VAEDB.DocumentDetails.Remove(documentDetail);
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
