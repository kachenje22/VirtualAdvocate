using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;
using VirtualAdvocate.BLL;
using VirtualAdvocate.Common;
using VirtualAdvocate.DAL;
using VirtualAdvocate.Models;

namespace VirtualAdvocate.Controllers
{
    public class UsersManagementController : BaseController
    {
        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();
        private VirtualAdvocateDocumentData objData = new VirtualAdvocateDocumentData();
        // GET: UsersManagement
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserList(string enable)
        {
            Session["re"] = "Yes";
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
            int myRoleID = Convert.ToInt32(Session["RoleId"]);
            // Only individual and due diligence user
            if (Convert.ToInt32(Session["RoleId"]) == 1)
            {
                if (active)
                {
                    var obj = (from ua in db.UserAddressDetails
                               join up in db.UserProfiles on ua.UserId equals up.UserID
                               join rol in db.Roles on up.RoleId equals rol.RoleId
                               where up.IsEnabled == active && up.UnusedUser != active

                               select new AllUserList { EmailAddress = up.EmailAddress, FirstName = ua.FirstName, LastName = ua.LastName, RoleDescription = rol.RoleDescription, UserID = up.UserID, IsEnabled = up.IsEnabled, CreatedDate = up.CreatedDate, IsLocked = up.UnusedUser }).ToList().OrderByDescending(y => y.CreatedDate).OrderBy(x => x.IsEnabled);
                    return View(obj.ToList());
                }
                else
                {
                    var obj = (from ua in db.UserAddressDetails
                               join up in db.UserProfiles on ua.UserId equals up.UserID
                               join rol in db.Roles on up.RoleId equals rol.RoleId
                               where up.IsEnabled == active || up.UnusedUser != active

                               select new AllUserList { EmailAddress = up.EmailAddress, FirstName = ua.FirstName, LastName = ua.LastName, RoleDescription = rol.RoleDescription, UserID = up.UserID, IsEnabled = up.IsEnabled, CreatedDate = up.CreatedDate, IsLocked = up.UnusedUser }).ToList().OrderByDescending(y => y.CreatedDate).OrderBy(x => x.IsEnabled);
                    return View(obj.ToList());
                }

            }
            else if (Convert.ToInt32(Session["RoleId"]) == 6) //company users
            {
                int Department = Convert.ToInt32(Session["DepartmentID"]);
                int orgID = Convert.ToInt32(Session["OrgId"]);
                var obj = (from ua in db.UserAddressDetails
                           join up in db.UserProfiles on ua.UserId equals up.UserID
                           join rol in db.Roles on up.RoleId equals rol.RoleId
                           where rol.RoleId != 1 && up.OrganizationId == orgID && (Department == 0 || (Department != 0 && up.Department == Department)) && up.IsEnabled == active &&
                          ((myRoleID == 6 && up.RoleId == 5)
                          ||
                           (myRoleID == 2 && up.RoleId != 2) || myRoleID == 1)
                           select new AllUserList { EmailAddress = up.EmailAddress, FirstName = ua.FirstName, LastName = ua.LastName, RoleDescription = rol.RoleDescription, UserID = up.UserID, IsEnabled = up.IsEnabled, IsLocked = up.UnusedUser }).ToList();
                return View(obj.ToList());
            }
            else
            {
                int userId;
                userId = Convert.ToInt32(Session["UserId"]);
                var obj = (from ua in db.UserAddressDetails
                           join up in db.UserProfiles on ua.UserId equals up.UserID
                           join rol in db.Roles on up.RoleId equals rol.RoleId
                           join org in db.OrganizationDetails on up.OrganizationId equals org.OrganizationId
                           where up.UserID == userId && up.IsEnabled == active && up.UnusedUser != active
                           select new AllUserList { EmailAddress = up.EmailAddress, FirstName = ua.FirstName, LastName = ua.LastName, RoleDescription = rol.RoleDescription, UserID = up.UserID, IsEnabled = up.IsEnabled, CreatedDate = up.CreatedDate, IsLocked = up.UnusedUser }).ToList().OrderByDescending(y => y.CreatedDate).OrderBy(x => x.IsEnabled);
                return View(obj.ToList());
            }


        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult ActivateProfile(int? id)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            var test = 0;
            bool LogSts = false;
            try
            {
                VirtualAdvocateData objData = new VirtualAdvocateData();
                LogRegistration objLog = new LogRegistration();
                var obj = db.UserProfiles.Where(ua => ua.UserID == id).FirstOrDefault();
                if (obj != null)
                {

                    if (obj.IsEnabled == true && obj.HasActivated == true && obj.UnusedUser == false)
                    {
                        obj.IsEnabled = false;
                        obj.HasActivated = false;

                        message = "User profile deactivated successfully";
                        objLog.Action = "Inactive";
                        objLog.IsEnabled = false;
                        LogSts = false;
                    }
                    else
                    {
                        LoginHistory objLoginLog = new LoginHistory();

                        objLoginLog.LoginDate = DateTime.Now;
                        objLoginLog.Status = 1;
                        objLoginLog.UserId = obj.UserID;

                        db.LoginHistories.Add(objLoginLog);

                        LogSts = true;
                        objLog.Action = "Active";
                        objLog.IsEnabled = true;
                        obj.IsEnabled = true;
                        obj.UnusedUser = false;
                        obj.HasActivated = true;
                        message = "User profile activated successfully";
                        var objAddress = db.UserAddressDetails.Where(x => x.UserId == id).FirstOrDefault();
                        MailSend objMail = new MailSend();
                        objMail.ActivationNotificationEmail(objAddress.FirstName + " " + objAddress.LastName, obj.EmailAddress, ConfigurationManager.AppSettings["ApplicationTitle"].ToString(), Common.Helper.GetBaseUrl());
                    }
                    var objUserRegistration = db.UserAddressDetails.Where(ua => ua.UserId == id).FirstOrDefault();

                    // Assign Values for Log
                    objLog.UserId = id.Value;
                    objLog.FirstName = objUserRegistration.FirstName;
                    objLog.LastName = objUserRegistration.LastName;
                    objLog.EmailAddress = obj.EmailAddress;
                    objLog.PhoneNumber = objUserRegistration.PhoneNumber;
                    objLog.StreetName = objUserRegistration.StreetName;
                    objLog.PlotNumber = objUserRegistration.PlotNumber;
                    objLog.Region = objUserRegistration.Region;
                    objLog.BuildingName = objUserRegistration.BuildingName;
                    objLog.LandMark = objUserRegistration.LandMark;
                    objLog.BlockNumber = objUserRegistration.BlockNumber;
                    objLog.ModifierId = Convert.ToInt32(Session["UserId"]);
                    objLog.RoleId = obj.RoleId;
                    objLog.OrgId = obj.OrganizationId;
                    objLog.ModifiedDate = DateTime.Now;
                    if (obj.RoleId == 7)
                    {

                        objData.LogDueRegistrations(id.Value, LogSts, Convert.ToInt32(Session["UserId"]));
                    }
                    else
                    {
                        objData.LogRegistrations(objLog);
                    }


                }
                db.SaveChanges();
                test = 1;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                message = "An error occured while processing the request. Try again later";
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            }
            if (test == 1)
                return Json(new { message = message }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { ErrorMessage = "not saved" }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult ActivateProfile(int? id)
        //{
        //    var message = string.Empty;
        //    try
        //    {
        //        var obj = db.UserProfiles.Where(ua => ua.UserID == id).FirstOrDefault();
        //        if (obj != null)
        //        {
        //            if (obj.IsEnabled == true && obj.HasActivated == true)
        //            {
        //                obj.IsEnabled = false;
        //                obj.HasActivated = false;
        //                //message = "User profiles deactivated successfully";
        //            }
        //            else
        //            {
        //                obj.IsEnabled = true;
        //                obj.HasActivated = true;
        //                //message = "User profiles activated successfully";
        //            }

        //            db.SaveChanges();
        //        }
        //        else
        //        {

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //        message = "An error occured while processing the request. Try again later";
        //        HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        //    }
        //    return RedirectToAction("UserList", "UsersManagement");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Dispaly all the Organization list for CRUD
        /// </summary>
        /// <returns></returns>
        public ActionResult OrganizationList(string enable)
        {
            if (Convert.ToInt32(Session["RoleId"]) != 1)
            {
                return RedirectToAction("LogOff", "UsersRegistration");
            }
            else
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

                List<SelectListItem> listInfo = new List<SelectListItem>();

                //var services = db.AccountServices.Where(s => s.IsEnabled == true).ToList();

                //foreach (AccountService s in services)
                //{
                //    listInfo.Add(new SelectListItem() { Text = s.Service, Value = Convert.ToString(s.ServiceId) });
                //}

                MultiSelectList models = new MultiSelectList(listInfo, "Value", "Text", "");
                ViewBag.Services = models;


                var obj = (from ua in db.UserAddressDetails
                           join od in db.OrganizationDetails on ua.UserId equals od.UserId
                           join up in db.UserProfiles on od.UserId equals up.UserID

                           where od.UserAccountsType != null && od.IsEnabled == active
                           select new AllOrganizationList
                           {
                               EmailAddress = od.OrgEmail,
                               OrganizationName = od.OrgName,
                               OrganizationId = od.OrganizationId,
                               FirstName = ua.FirstName,
                               LastName = ua.LastName,
                               IsEnabled = od.IsEnabled,
                               userAccountType = od.UserAccountsType,
                               CreatedDate = od.CreatedDate,
                               IsLocked = up.UnusedUser


                           }).ToList().OrderByDescending(x => x.CreatedDate).OrderBy(y => y.IsEnabled);

                return View(obj.ToList());
            }
        }

        /// <summary>
        /// Enable or disable the Organization
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ActivateOrganization(int? id)
        {
            var message = string.Empty;
            try
            {
                var obj = db.OrganizationDetails.Where(ua => ua.OrganizationId == id).FirstOrDefault();
                if (obj != null)
                {
                    if (obj.IsEnabled == true)
                    {
                        obj.IsEnabled = false;

                        var category = db.DocumentCategories.Where(d => d.ServiceId == id).ToList();

                        category.ForEach(d => d.IsEnabled = false);

                        foreach (DocumentCategory catObj in category)
                        {
                            var subCategoryobj = db.DocumentSubCategories.Where(s => s.DocumentCategoryId == catObj.DocumentCategoryId).ToList();

                            subCategoryobj.ForEach((a) =>
                            {
                                a.IsEnabled = false;
                            });

                            var cateuserObj = db.DocumentTemplates.Where(d => d.DocumentCategory == catObj.DocumentCategoryId).ToList();

                            cateuserObj.ForEach((a) =>
                            {
                                a.IsEnabled = false;
                            });

                            var associateddocument = db.AssociateTemplateDetails.Where(s => s.AssociateTemplateId == id).ToList();
                            associateddocument.ForEach((a) =>
                            {
                                a.IsEnabled = false;
                            });

                            foreach (DocumentSubCategory catsubObj in subCategoryobj)
                            {

                                var subsubCategoryobj = db.DocumentSubSubCategories.Where(s => s.DocumentSubCategoryId == catsubObj.DocumentSubCategoryId).ToList();

                                subsubCategoryobj.ForEach((a) =>
                                {
                                    a.IsEnabled = false;
                                });


                                var subuserObj = db.DocumentTemplates.Where(d => d.DocumentSubCategory == catsubObj.DocumentSubCategoryId).ToList();

                                subuserObj.ForEach((a) =>
                                {
                                    a.IsEnabled = false;
                                });
                            }
                        }
                        var usr = db.UserProfiles.Where(u => u.OrganizationId == id).ToList();

                        usr.ForEach((a) =>
                        {
                            a.IsEnabled = false;
                        });


                    }
                    else
                    {
                        obj.IsEnabled = true;
                    }

                    db.SaveChanges();
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                message = "An error occured while processing the request. Try again later";
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            }
            return Json(new { message = message }, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("OrganizationList", "UsersManagement");
        }

        [HttpGet]
        public ActionResult ManageOrganization(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrganizationViewModel objOrg = new OrganizationViewModel();
            try
            {
                OrganizationDetail organizationDetail = db.OrganizationDetails.Find(id);
                objOrg.userAccountTypes = db.UserAccountTypes.ToList();
                VirtualAdvocateData objData = new VirtualAdvocateData();
                List<OptionsModel> objOrgType = new List<OptionsModel>();
                if (organizationDetail.OrganizationTypeId == null)
                {
                    organizationDetail.OrganizationTypeId = 0;
                }
                objOrgType = objData.getOrganizationTypesOptionsList();
                objOrg.getAllOrganizationTypes = objOrgType;
                if (organizationDetail == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    if (Convert.ToInt32(Session["RoleId"]) == 1 || Convert.ToInt32(Session["UserId"]) == organizationDetail.UserId)
                    {
                        objOrg.OrganizationId = organizationDetail.OrganizationId;
                        objOrg.OrgName = organizationDetail.OrgName;
                        objOrg.OrgPhoneNumber = organizationDetail.OrgPhoneNumber;
                        objOrg.OrgEmail = organizationDetail.OrgEmail;
                        objOrg.OrgStreetName = organizationDetail.OrgStreetName;
                        objOrg.OrgBuildingName = organizationDetail.OrgBuildingName;
                        objOrg.OrgBlockNo = organizationDetail.OrgBlockNo;
                        objOrg.OrgPlotNo = organizationDetail.OrgPlotNo;
                        objOrg.OrgRegion = organizationDetail.OrgRegion;
                        objOrg.OrgLandMark = organizationDetail.OrgLandMark;
                        objOrg.userId = Convert.ToInt32(organizationDetail.UserId);
                        objOrg.UserAccountsType = Convert.ToInt32(organizationDetail.UserAccountsType);
                        objOrg.OrganizationTypeId = Convert.ToInt32(organizationDetail.OrganizationTypeId);
                        var departments = from d in db.Departments where d.IsEnabled == true select new DepartmentModel { Department = d.Name, Id = d.Id };
                        objOrg.AvailableDepartment = departments;
                        var selectedDepartment = from s in db.SelectedDepartments join d in db.Departments on s.DepartmentID equals d.Id where (s.OrgID == organizationDetail.OrganizationId) select new DepartmentModel { Department = d.Name, Id = d.Id };
                        objOrg.SelectedDepartment = selectedDepartment;
                        objOrg.extraFields = db.ClientWiseCustomerTemplates.Where(d => d.ClientID == id).ToList();
                    }
                    else
                    {
                        return RedirectToAction("LogOff", "UsersRegistration");
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return View(objOrg);
        }

        [HttpPost]
        public ActionResult ManageOrganization(OrganizationViewModel organizationDetail)
        {
            try
            {
                OrganizationDetail objOrg = db.OrganizationDetails.Find(organizationDetail.OrganizationId);
                objOrg.OrganizationId = organizationDetail.OrganizationId;
                objOrg.OrgName = organizationDetail.OrgName;
                objOrg.OrgPhoneNumber = organizationDetail.OrgPhoneNumber;
                objOrg.OrgEmail = organizationDetail.OrgEmail;
                objOrg.OrgStreetName = organizationDetail.OrgStreetName;
                objOrg.OrgBuildingName = organizationDetail.OrgBuildingName;
                objOrg.OrgBlockNo = organizationDetail.OrgBlockNo;
                objOrg.OrgPlotNo = organizationDetail.OrgPlotNo;
                objOrg.OrgRegion = organizationDetail.OrgRegion;
                objOrg.OrgLandMark = organizationDetail.OrgLandMark;
                if (organizationDetail.UserAccountsType != 0)
                {
                    objOrg.UserAccountsType = organizationDetail.UserAccountsType;
                    UserProfile objUp = db.UserProfiles.Find(organizationDetail.userId);
                    if (organizationDetail.UserAccountsType == 2 && objUp.RoleId == 3)
                    {
                        objUp.RoleId = 2;
                    }
                }
                objOrg.OrganizationTypeId = organizationDetail.OrganizationTypeId;

                //Remove selected departmentsfor perticular organization

                db.SelectedDepartments.RemoveRange(db.SelectedDepartments.Where(c => c.OrgID == organizationDetail.OrganizationId));
                db.SaveChanges();

                //Adding customer Template
                if (organizationDetail.extraFields.Count() > 0)
                {

                    db.ClientWiseCustomerTemplates.Where(r => r.ClientID == organizationDetail.OrganizationId)
           .ToList().ForEach(p => db.ClientWiseCustomerTemplates.Remove(p));
                    db.SaveChanges();

                    foreach (ClientWiseCustomerTemplate item in organizationDetail.extraFields)
                    {
                        var customerKey = db.TemplateKeywords.Where(t => t.TemplateKeyValue == item.KeyName.Replace(" ", "_")).FirstOrDefault();

                        var key = db.ClientWiseCustomerTemplates.Where(c => c.ClientID == organizationDetail.OrganizationId && c.KeyName == item.KeyName).FirstOrDefault();
                        item.KeyName = item.KeyName == null ? "Name" : item.KeyName;
                        if (key == null)
                        {
                            ClientWiseCustomerTemplate objExtra = new ClientWiseCustomerTemplate();
                            objExtra.ClientID = organizationDetail.OrganizationId;
                            objExtra.KeyName = item.KeyName;
                            objExtra.Show = item.Show;
                            db.ClientWiseCustomerTemplates.Add(objExtra);
                        }

                        if (customerKey == null)
                        {
                            TemplateKeyword keyObj = new TemplateKeyword();

                            keyObj.TemplateKeyValue = item.KeyName.Replace(" ", "_");
                            keyObj.TemplateKeyLabels = item.KeyName;
                            keyObj.MultipleKeys = false;
                            keyObj.IsEnabled = true;
                            keyObj.TemplateKeyCategory = 1;

                            db.TemplateKeywords.Add(keyObj);
                        }
                        db.SaveChanges();
                    }


                    var selectedDepartments = new List<DepartmentModel>();
                    var postedDepartmentsIds = new string[0];
                    if (organizationDetail.PostedDepartment == null) organizationDetail.PostedDepartment = new PostedDepartment();
                    VirtualAdvocateData objData = new VirtualAdvocateData();

                    // if a view model array of posted payment
                    // and is not empty,save selected ids
                    if (organizationDetail.PostedDepartment.DepartmentIDs != null)
                    {
                        SelectedDepartment objDep = new SelectedDepartment();

                        postedDepartmentsIds = organizationDetail.PostedDepartment.DepartmentIDs;
                        for (int i = 0; i < postedDepartmentsIds.Length; i++)
                        {
                            objDep.DepartmentID = Convert.ToInt32(postedDepartmentsIds[i]);
                            objDep.OrgID = organizationDetail.OrganizationId;
                            objData.SaveSelectedDepartment(objDep);
                        }

                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            if (Convert.ToInt32(Session["RoleId"]) == 2)
            {
                int id = Convert.ToInt32(Session["OrgId"]);
                return RedirectToAction("ManageOrganization", "UsersManagement", new { @id = id });
            }
            return RedirectToAction("OrganizationList", "UsersManagement");
        }

        public ActionResult AddOrganization()
        {
            int? id = null;
            VirtualAdvocateData objData = new VirtualAdvocateData();
            UserRegistrationModel obj = objData.getDefaultRegistration(id);
            obj.getAllOrganizationTypes = objData.getOrganizationTypesOptionsList();
            var departments = from d in db.Departments where d.IsEnabled == true select new DepartmentModel { Department = d.Name, Id = d.Id };
            obj.AvailableDepartment = departments;
            return View(obj);
        }

        public ActionResult AddUser()
        {
            int? id = null;
            VirtualAdvocateData objData = new VirtualAdvocateData();
            UserRegistrationModel obj = objData.getDefaultRegistration(id);
            obj.getAllOrganizationTypes = objData.getOrganizationTypesOptionsList();
            return View(obj);
        }
        public ActionResult AddOrgUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VirtualAdvocateData objData = new VirtualAdvocateData();
            VirtualAdvocateDocumentData objData1 = new
                 VirtualAdvocateDocumentData();
            OrgUserViewModel obj = objData.getOrgServicesForNewUser(id);
            obj.getDepartmentList = objData1.getDepartmentOptionsList(id.Value);
            obj.getRoleList = objData1.getRoles(Convert.ToInt32(Session["RoleId"]));
            obj.Department = 0;
            obj.OrgId = id.Value;
            return View(obj);
        }
        [HttpPost]
        public ActionResult AddOrgUser(OrgUserViewModel objModel, FormCollection fc)
        {
            try
            {
                LogRegistration objLog = new LogRegistration();
                int userId = 0;
                var success = false;
                VirtualAdvocateData objData = new VirtualAdvocateData();
                UserProfile objUP = new UserProfile();

                objUP.RoleId = objModel.RoleID;
                objUP.EmailAddress = objModel.EmailAddress;
                if (Convert.ToInt32(Session["RoleId"]) == 1 || Convert.ToInt32(Session["RoleId"]) == 2 || Convert.ToInt32(Session["RoleId"]) == 6)
                {
                    objUP.IsEnabled = true;
                    objUP.HasActivated = true;
                }
                else
                {
                    objUP.IsEnabled = false;
                    objUP.HasActivated = false;
                }
                if (Convert.ToInt32(Session["RoleId"]) == 6)
                {
                    objUP.Department = Convert.ToInt32(Session["DepartmentID"]);
                }
                else
                {
                    objUP.Department = objModel.Department;
                }
                objUP.Password = Crypto.HashPassword(objModel.password);
                objUP.IsMailSent = true;
                objUP.CreatedDate = DateTime.Now;
                objUP.OrganizationId = objModel.OrgId;
                try
                {
                    userId = objData.SaveUserProfile(objUP);
                    success = true;
                }
                catch (Exception ex)
                {
                    success = false;
                    ErrorLog.LogThisError(ex);
                }

                if (userId != 0)
                {
                    UserAddressDetail objAddress = new UserAddressDetail();
                    objAddress.UserId = userId;
                    objAddress.FirstName = objModel.FirstName;
                    objAddress.LastName = objModel.LastName;
                    objAddress.Designation = objModel.Designation;
                    objAddress.CreatedDate = DateTime.Now;
                    try
                    {
                        objData.SaveUserPersonalDetails(objAddress);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        ErrorLog.LogThisError(ex);
                    }
                    string clientID;
                    if (fc["AvailableService"] != null)
                        clientID = fc["AvailableService"];
                    else
                    {
                        var AccountAdmin = db.UserProfiles.Where(u => u.OrganizationId == objModel.OrgId && u.RoleId == 2).FirstOrDefault().UserID;

                        //  int AccountAdmin = Convert.ToInt32(Session["UserId"]);
                        try
                        {
                            clientID = "0";
                            //Convert.ToString(db.SelectedAccountServices.Where(s => s.UserId == AccountAdmin).FirstOrDefault().ServiceId);
                        }
                        catch (Exception ex)
                        {
                            clientID = "0";
                        }
                    }
                    //var selectedServices = new List<AccountServicesModel>();
                    //if (Convert.ToInt32(clientID) > 0)
                    //{
                    //    SelectedAccountService objAC = new SelectedAccountService();
                    //    objAC.ServiceId = Convert.ToInt32(clientID);
                    //    objAC.UserId = userId;
                    //    objData.SaveSelectedAccountServices(objAC);
                    //    db.SaveChanges();
                    //}
                    //var postedAccServicesIds = new string[0];
                    //if (objModel.PostedServices == null) objModel.PostedServices = new PostedServices();


                    //// if a view model posted services
                    //// and is not empty,save selected ids
                    //if (objModel.PostedServices.ServiceIds != null)
                    //{
                    //    SelectedAccountService objAC = new SelectedAccountService();
                    //    postedAccServicesIds = objModel.PostedServices.ServiceIds;
                    //    for (int i = 0; i < postedAccServicesIds.Length; i++)
                    //    {
                    //        objAC.ServiceId = Convert.ToInt32(postedAccServicesIds[i]);
                    //        objAC.UserId = userId;
                    //        objData.SaveSelectedAccountServices(objAC);
                    //    }
                    //}


                }
                if (success == true)
                {
                    try
                    {
                        var companyName = "";
                        var orgname = objData.getOrganizationDetails(objModel.OrgId);
                        if (orgname != null)
                        {
                            companyName = orgname.OrgName;
                        }

                        objLog.Action = "Insert";
                        objLog.UserId = userId;
                        objLog.ModifierId = Convert.ToInt32(Session["UserId"]);
                        objLog.FirstName = objModel.FirstName;
                        objLog.LastName = objModel.LastName;
                        objLog.EmailAddress = objModel.EmailAddress;
                        objLog.Designation = objModel.Designation;
                        objLog.PhoneNumber = objModel.PhoneNumber;
                        objLog.StreetName = objModel.StreetName;
                        objLog.PlotNumber = objModel.PlotNo;
                        objLog.Region = objModel.Region;
                        objLog.BuildingName = objModel.BuildingName;
                        objLog.LandMark = objModel.LandMark;
                        objLog.BlockNumber = objModel.BlockNo;
                        objLog.RoleId = 5;
                        objLog.OrgId = objModel.OrgId;
                        objLog.IsEnabled = false;
                        objLog.ModifiedDate = DateTime.Now;

                        int result = objData.LogRegistrations(objLog); // Log insert process
                        var postedAccServicesIds = new string[0];
                        // if (objModel.PostedServices.ServiceIds != null) // Log Services 
                        //{
                        //    postedAccServicesIds = objModel.PostedServices.ServiceIds;
                        //    for (int i = 0; i < postedAccServicesIds.Length; i++)
                        //    {
                        //        //objData.LogAccountServices(result, userId, Convert.ToInt32(postedAccServicesIds[i]));
                        //    }
                        //}
                        db.SaveChanges();
                        MailSend objMail = new MailSend();
                        objMail.SendMailForUserCreation(objModel, "New Account Created", ConfigurationManager.AppSettings["ApplicationTitle"].ToString(), companyName, Convert.ToInt32(Session["RoleId"]));
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.LogThisError(ex);
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {

                ErrorLog.LogThisError(ex);
            }
            catch (InvalidOperationException ioe)
            {

                ErrorLog.LogThisError(ioe);
            }
            return RedirectToAction("OrgUserList", "UsersManagement", new { id = objModel.OrgId });
        }

        public ActionResult OrgUserList(string enable, int? id)
        {
            Session["re"] = "No";
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
            if (id == null)
                id = Convert.ToInt32(Session["OrgId"]);
            else
                Session["OrgId"] = id.Value;

            int myRoleID = Convert.ToInt32(Session["RoleId"]);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (Convert.ToInt32(Session["RoleId"]) != 1 && Convert.ToInt32(Session["RoleId"]) != 2 && Convert.ToInt32(Session["RoleId"]) != 6)
            {
                return RedirectToAction("LogOff", "UsersRegistration");
            }

            var orgDetails = db.OrganizationDetails.Where(o => o.OrganizationId == id).FirstOrDefault();

            ViewBag.AccountType = orgDetails.UserAccountsType;

            int Department = 0;
            if (Convert.ToInt32(Session["RoleId"]) == 6)
                Department = Convert.ToInt32(Session["DepartmentID"]);


            var obj = (from ua in db.UserAddressDetails
                       join up in db.UserProfiles on ua.UserId equals up.UserID
                       join rol in db.Roles on up.RoleId equals rol.RoleId
                       where rol.RoleId != 1 && up.OrganizationId == id && (Department == 0 || (Department != 0 && up.Department == Department)) && up.IsEnabled == active &&
                      ((myRoleID == 6 && up.RoleId == 5)
                      ||
                       (myRoleID == 2 && up.RoleId != 2) || myRoleID == 1)
                       select new OrganizationUserList { EmailAddress = up.EmailAddress, FirstName = ua.FirstName, LastName = ua.LastName, RoleDescription = rol.RoleDescription, UserID = up.UserID, IsEnabled = up.IsEnabled, IsLocked = up.UnusedUser }).ToList();
            return View(obj.ToList());


        }

        [HttpGet]
        public ActionResult ManageProfile(int? id)
        {
            int userid;
            if (id != null)
            {
                userid = Convert.ToInt32(id);
            }
            else
            {
                userid = Convert.ToInt32(Session["UserId"]);
            }
            UserProfile objUP = new UserProfile();
            UserAddressDetail objUser = new UserAddressDetail();
            PersonalDetailsViewModel obj = new PersonalDetailsViewModel();
            try
            {
                using (VirtualAdvocateEntities context = new VirtualAdvocateEntities())
                {
                    objUser = context.UserAddressDetails.Where(ua => ua.UserId == userid).FirstOrDefault();
                    objUP = context.UserProfiles.Find(userid);
                    if (objUser != null)
                    {
                        obj.UserId = objUser.UserId;
                        obj.FirstName = objUser.FirstName;
                        obj.LastName = objUser.LastName;
                        obj.EmailAddress = objUP.EmailAddress;
                        obj.StreetName = objUser.StreetName;
                        obj.BuildingName = objUser.BuildingName;
                        obj.BlockNo = objUser.BlockNumber;
                        obj.PlotNo = objUser.PlotNumber;
                        obj.Region = objUser.Region;
                        obj.LandMark = objUser.LandMark;
                        obj.roleID = objUP.RoleId;
                        Session["UserRoleId"] = objUP.RoleId;
                        if (objUP.RoleId != 7)
                        {
                            obj.Designation = objUser.Designation;
                        }
                        if (objUP.Department != null)
                            obj.Department = objUP.Department.Value;
                        //if (objUP.RoleId == 7)

                        else if (objUP.RoleId != 7)
                        {
                            obj.Department = 0;
                            obj.getDepartmentList = objData.getDepartmentOptionsList(objUP.OrganizationId.Value);
                            obj.getRoleList = objData.getRoles(Convert.ToInt32(Session["RoleId"]));

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            obj.UserId = objUP.UserID;
            obj.EmailAddress = objUP.EmailAddress;
            obj.roleID = objUP.RoleId;
            if (objUP.OrganizationId != null)
                obj.getDepartmentList = objData.getDepartmentOptionsList(objUP.OrganizationId.Value);
            return View(obj);
        }

        [HttpPost]
        public ActionResult ManageProfile(PersonalDetailsViewModel objUser)
        {
            int result = int.MinValue;
            int id = objUser.UserId;
            try
            {
                VirtualAdvocateData objData = new VirtualAdvocateData();
                result = objData.EditPersonalDetails(objUser);
                var obj = db.UserProfiles.Where(ua => ua.UserID == objUser.UserId).FirstOrDefault();
                if (objUser.Department != 0)
                    obj.Department = objUser.Department;
                db.SaveChanges();
                if (obj.RoleId == 4)
                {
                    objData.LogDuePersonalDetails(objUser, Convert.ToInt32(Session["UserId"]));//Log After Modification
                }
                else
                {
                    objData.LogPersonalDetails(objUser, Convert.ToInt32(Session["UserId"]), obj.RoleId, obj.OrganizationId.Value);//Log After Modification
                }

            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            if (Convert.ToInt32(Session["RoleId"]) == 1 || Convert.ToInt32(Session["RoleId"]) == 2)
            {
                return RedirectToAction("EditProfile", "UsersManagement", new { id = id });
            }
            else
                return RedirectToAction("Dashboard", "DocumentManagement");
        }

        public ActionResult EditOrganization(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrganizationDetail od = new OrganizationDetail();
            od = db.OrganizationDetails.Where(o => o.OrganizationId == id).FirstOrDefault();
            UserAccountType objAcType = new UserAccountType();
            if (od != null)
            {
                objAcType = db.UserAccountTypes.Where(at => at.UserAccountTypeId == od.UserAccountsType).FirstOrDefault();
            }
            return View();
        }
        public ActionResult EditProfile(int? id)
        {
            if (Session["re"] != null && Session["re"].ToString() == "Yes")
                ViewBag.user = "Yes";
            else
                ViewBag.user = "No";
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Session["AccountUser"] = 0;
            UserAddressDetail ua = new UserAddressDetail();
            ua = db.UserAddressDetails.Where(u => u.UserId == id).FirstOrDefault();
            UserProfile objUp = new UserProfile();
            objUp = db.UserProfiles.Where(up => up.UserID == id).FirstOrDefault();
            var department = (objUp != null) ? db.Departments.Where(d => d.Id == objUp.Department).FirstOrDefault() : null;
            ProfileViewModel objProfile = new ProfileViewModel();

            if (ua.FirstName != null) { objProfile.FirstName = ua.FirstName; } else { objProfile.FirstName = "not entered"; }
            if (ua.LastName != null) { objProfile.LastName = ua.LastName; } else { objProfile.LastName = "not entered"; }
            if (ua.PhoneNumber != null) { objProfile.PhoneNumber = ua.PhoneNumber; } else { objProfile.PhoneNumber = "not entered"; }
            if (objUp.EmailAddress != null) { objProfile.EmailAddress = objUp.EmailAddress; } else { objProfile.EmailAddress = "not entered"; }
            if (ua.StreetName != null) { objProfile.StreetName = ua.StreetName; } else { objProfile.StreetName = "not entered"; }
            if (ua.PlotNumber != null) { objProfile.PlotNo = ua.PlotNumber; } else { objProfile.PlotNo = "not entered"; }
            if (ua.Region != null) { objProfile.Region = ua.Region; } else { objProfile.Region = "not entered"; }
            if (ua.BuildingName != null) { objProfile.BuildingName = ua.BuildingName; } else { objProfile.BuildingName = "not entered"; }
            if (ua.LandMark != null) { objProfile.LandMark = ua.LandMark; } else { objProfile.LandMark = "not entered"; }
            if (ua.BlockNumber != null) { objProfile.BlockNo = ua.BlockNumber; } else { objProfile.BlockNo = "not entered"; }
            if (ua.Designation != null) { objProfile.Designation = ua.Designation; } else { objProfile.Designation = "not entered"; }
            if (department != null) { objProfile.Department = department.Name; } else { objProfile.Department = "not entered"; }
            Role objrole = db.Roles.Find(objUp.RoleId);
            objProfile.RoleDescription = objrole.RoleDescription;
            objProfile.RoleId = objUp.RoleId;

            objProfile.userId = ua.UserId;
            if (objUp.OrganizationId != null)
            {
                objProfile.OrganizationId = objUp.OrganizationId.Value;
            }
            else
            {
                objProfile.OrganizationId = 0;
            }
            //objProfile.getSelectedService

            //var objSelectedService = (from accser in db.AccountServices
            //                          join selser in db.SelectedAccountServices on accser.ServiceId equals selser.ServiceId
            //                          join usp in db.UserProfiles on selser.UserId equals usp.UserID
            //                          where usp.UserID == ua.UserId 
            //                          select accser
            //                        );

            //objProfile.getSelectedService = objSelectedService.ToList();
            if (objUp.RoleId == 5)
            {
                Session["AccountUser"] = 1;
            }
            var objSelectedPayment = (from pm in db.PaymentMethods
                                      join selpm in db.SelectedPaymentMethods on pm.PaymentTypeId equals selpm.PaymentTypeId
                                      join usrp in db.UserProfiles on selpm.UserId equals usrp.UserID
                                      where usrp.UserID == ua.UserId
                                      select pm
                );

            objProfile.getSeletedPayment = objSelectedPayment.ToList();

            //objProfile.FirstName = ua.FirstName;
            //objProfile.LastName = ua.LastName;
            //objProfile.PhoneNumber = ua.PhoneNumber;
            //objProfile.EmailAddress = ua.EmailAddress;
            //objProfile.StreetName = ua.StreetName;
            //objProfile.PlotNo = ua.PlotNumber;
            //objProfile.Region = ua.Region;
            //objProfile.BuildingName = ua.BuildingName;
            //objProfile.LandMark = ua.LandMark;
            //objProfile.BlockNo = ua.BlockNumber;



            return View(objProfile);
        }

        public ActionResult EditAcService(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            VirtualAdvocateData objData = new VirtualAdvocateData();
            AccountServiceModel obj = new AccountServiceModel();
            if (Convert.ToInt32(Session["RoleId"]) == 2 && Convert.ToInt32(Session["UserId"]) == id) // Account Admin
            {
                //obj.AvailableService = ServicesRepository.GetAllAccountServices().ToList();
                //obj.SelectedService = objData.getUserSelectedServices(id);
                foreach (AccountServicesModel a in obj.AvailableService)
                {
                    foreach (AccountServicesModel s in obj.SelectedService)
                    {
                        if (a.Id == s.Id)
                        {
                            a.IsSelected = true;
                        }
                    }

                }

            }
            else if (Convert.ToInt32(Session["RoleId"]) == 2)
            {
                obj = objData.getOrgServicesForEditUser(id);
            }
            else
            {
                //obj.AvailableService = ServicesRepository.GetAllAccountServices().ToList();
                //obj.SelectedService = objData.getUserSelectedServices(id);

                foreach (AccountServicesModel a in obj.AvailableService)
                {
                    foreach (AccountServicesModel s in obj.SelectedService)
                    {
                        if (a.Id == s.Id)
                        {
                            a.IsSelected = true;
                        }
                    }

                }
            }

            obj.userId = id.Value;
            return View(obj);
        }

        [HttpPost]
        public ActionResult EditAcService(AccountServiceModel objmodel, FormCollection fc)
        {

            VirtualAdvocateData objData = new VirtualAdvocateData();

            string clientID = fc["AvailableService"];

            var selectedServices = new List<AccountServicesModel>();

            var objtop = db.LogRegistrations
               .Where(m => m.UserId == objmodel.userId)
               .OrderByDescending(n => n.LogId)
               .FirstOrDefault();


            // int res = objData.DeleteSelectedAccountServices(objmodel.userId);

            //SelectedAccountService objAC = new SelectedAccountService();

            //objAC.ServiceId = Convert.ToInt32(clientID);
            //objAC.UserId = objmodel.userId;
            //objData.SaveSelectedAccountServices(objAC);
            try
            {
                //  objData.LogAccountServices(objtop.LogId, objmodel.userId, Convert.ToInt32(clientID));// Log Insert account services
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return RedirectToAction("EditProfile", "UsersManagement", new { id = objmodel.userId });
        }

        public ActionResult EditPayment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VirtualAdvocateData objData = new VirtualAdvocateData();
            EditPaymentMethodeModel obj = objData.getPayments(id);
            obj.userId = id.Value;
            return View(obj);
        }


        public ActionResult EditRole(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userRole = db.UserProfiles.Where(u => u.UserID == id).FirstOrDefault().RoleId;
            VirtualAdvocateDocumentData objData = new VirtualAdvocateDocumentData();
            EditUserRoleModel obj = new EditUserRoleModel();
            obj.userId = id.Value;
            obj.Roles = objData.getRoles(Convert.ToInt32(Session["RoleId"]));
            obj.UserRole = userRole;
            return View(obj);
        }

        [HttpPost]
        public ActionResult EditPayment(EditPaymentMethodeModel objmodel)
        {

            VirtualAdvocateData objData = new VirtualAdvocateData();

            var selectedServices = new List<PaymentMethodModel>();
            var postedPaymentTypeIds = new string[0];
            if (objmodel.PostedPaymentMethods == null) objmodel.PostedPaymentMethods = new PostedPaymentMethods();

            int res = objData.DeleteSelectedPaymentMethods(objmodel.userId);
            // save selected ids
            //var obj = (from logdue in db.LogDueDiligenceUsers
            //           where logdue.UserId == objmodel.userId
            //           orderby logdue.LogId
            //           select logdue).Take(1); //db.LogDueDiligenceUsers.OrderByDescending(m => m.UserId == objmodel.userId).FirstOrDefault();
            var objtop = db.LogDueDiligenceUsers
                .Where(m => m.UserId == objmodel.userId)
                .OrderByDescending(n => n.LogId)
                .FirstOrDefault();
            var objUP = db.UserProfiles.Find(objmodel.userId);
            if (objmodel.PostedPaymentMethods.PaymentTypeIds != null)
            {
                SelectedPaymentMethod objAC = new SelectedPaymentMethod();
                postedPaymentTypeIds = objmodel.PostedPaymentMethods.PaymentTypeIds;
                for (int i = 0; i < postedPaymentTypeIds.Length; i++)
                {
                    objAC.PaymentTypeId = Convert.ToInt32(postedPaymentTypeIds[i]);
                    objAC.UserId = objmodel.userId;
                    objData.SaveSelectedPaymentMethod(objAC);

                    try
                    {
                        if (objUP.RoleId == 7)
                        {
                            objData.LogDuePaymentType(objtop.LogId, objmodel.userId, Convert.ToInt32(postedPaymentTypeIds[i]));
                        }
                        else
                        {
                            objData.LogUserPaymentType(objtop.LogId, objmodel.userId, Convert.ToInt32(postedPaymentTypeIds[i]));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.LogThisError(ex);
                    }

                }
            }


            return RedirectToAction("EditProfile", "UsersManagement", new { id = objmodel.userId });
        }

        public ActionResult Development()
        {
            return View();
        }

        public class HandleExceptionAttribute : HandleErrorAttribute
        {
            public override void OnException(ExceptionContext filterContext)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.Exception != null)
                {
                    filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    filterContext.Result = new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            filterContext.Exception.Message,
                            filterContext.Exception.StackTrace
                        }
                    };
                    filterContext.ExceptionHandled = true;
                }
                else
                {
                    base.OnException(filterContext);
                }
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult EditUserRole(EditUserRoleModel usrObj)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            var obj = db.UserProfiles.Find(usrObj.userId);
            try
            {

                var objDisable = db.UserProfiles.Where(ua => ua.UserID == usrObj.userId).FirstOrDefault();
                if (objDisable != null)
                {
                    objDisable.RoleId = usrObj.UserRole;

                    db.SaveChanges();
                }
                //var objDisable = db.UserProfiles.Where(ua => ua.OrganizationId == obj.OrganizationId && ua.RoleId == 2).FirstOrDefault();
                //if (objDisable != null)
                //{
                //    objDisable.RoleId = 5;
                //    objDisable.IsEnabled = false;
                //    objDisable.HasActivated = false;
                //    db.SaveChanges();
                //}


                //obj.RoleId = 2;
                //db.SaveChanges();

                //message = "User role changed as admin";
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                message = "An error occured while processing the request. Try again later";
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            }

            return RedirectToAction("EditProfile", "UsersManagement", new { id = obj.UserID });
        }

        //public ActionResult Checkservicename(Int64 id)
        //{
        //    using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
        //    {
        //        var userid = objContext.UserProfiles.Where(o => o.OrganizationId == id).Select(u => u.UserID).FirstOrDefault();

        //        var services = objContext.SelectedAccountServices.Where(u => u.UserId == userid).ToList();

        //        if (services.Count > 0)
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }
        //        else

        //            return Json(false, JsonRequestBehavior.AllowGet);

        //    }
        //}


        //public ActionResult AssignService(Int32 id, string selectedservices)
        //{
        //    try
        //    {
        //        var userid = db.OrganizationDetails.Where(o => o.OrganizationId == id).FirstOrDefault();
        //        var obj = db.SelectedAccountServices.Where(u => u.UserId == userid.UserId).FirstOrDefault();

        //        string[] services = selectedservices.Split(',');

        //        if (obj == null)
        //        {
        //            SelectedAccountService addservice = new SelectedAccountService();

        //            for (int i = 0; i < services.Length; i++)
        //            {
        //                addservice.UserId =Convert.ToInt32( userid.UserId);
        //                addservice.ServiceId = Convert.ToInt32(services[i]);

        //                db.SelectedAccountServices.Add(addservice);
        //            }
        //            db.SaveChanges();
        //        }

        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }

        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult UserActivation(Int32 Id)
        {
            // var userID = db.SelectedAccountServices.Where(d => d.UserId == Id).ToList();
            var user = db.UserProfiles.Where(u => u.UserID == Id).FirstOrDefault();
            var org = db.OrganizationDetails.Where(o => o.OrganizationId == user.OrganizationId).FirstOrDefault();

            if ((user.RoleId != 7 && user.RoleId != 3) && !org.IsEnabled)
            {
                return Json("Org", JsonRequestBehavior.AllowGet);
            }
            else if ((user.RoleId != 7 && user.RoleId != 3) && user.Department == null || user.Department == 0)
            {
                return Json("Department", JsonRequestBehavior.AllowGet);
            }
            else
            { return Json("true", JsonRequestBehavior.AllowGet); }
        }


        public ActionResult OrgActivation(Int32 Id)
        {
            var org = db.SelectedDepartments.Where(o => o.OrgID == Id).FirstOrDefault();

            if (org == null)
            {
                return Json("Department", JsonRequestBehavior.AllowGet);
            }
            else
            { return Json("true", JsonRequestBehavior.AllowGet); }
        }

    }
}
