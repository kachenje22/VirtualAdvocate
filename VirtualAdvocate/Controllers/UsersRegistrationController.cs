#region NameSpaces
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using VirtualAdvocate.BLL;
using VirtualAdvocate.Common;
using VirtualAdvocate.DAL;
using VirtualAdvocate.Models;
#endregion
#region VirtualAdvocate.Controllers
namespace VirtualAdvocate.Controllers
{
    #region UsersRegistrationController
    public class UsersRegistrationController : Controller
    {
        #region Global Variables
        private VirtualAdvocateDocumentData objData =
        new VirtualAdvocateDocumentData();
        #endregion

        #region Index
        // GET: UsersRegistration
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Registration
        [HttpGet]
        public ActionResult Registration(int? id)
        {
            VirtualAdvocateData objData = new VirtualAdvocateData();
            UserRegistrationModel obj = objData.getDefaultRegistration(id);
            List<OptionsModel> objOrgType = new List<OptionsModel>();
            objOrgType = objData.getOrganizationTypesOptionsList();
            obj.getAllOrganizationTypes = objOrgType;
            return View(obj);
        }
        #endregion

        #region Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration
        (UserRegistrationModel objUserRegistration, FormCollection fc)
        {
            int userId = 0;
            int newOrgId = 0;
            LogRegistration objLog = new LogRegistration();
            using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
            {
                try
                {
                    var success = false;
                    VirtualAdvocateData objData = new VirtualAdvocateData();
                    UserProfile objUP = new UserProfile();
                    if (objUserRegistration.UserAccountType == 1)
                    {
                        objUP.RoleId = 3;
                        objLog.RoleId = 3;
                    }
                    else
                    {
                        objUP.RoleId = 2;
                        objLog.RoleId = 2;
                    }
                    objUP.EmailAddress = objUserRegistration.EmailAddress;
                    if (Session["RoleId"] != null && Session["RoleId"].ToString() == "1")
                    {
                        objUP.IsEnabled = true;
                        objUP.HasActivated = true;
                    }
                    else
                    {
                        objUP.IsEnabled = false;
                        objUP.HasActivated = false;
                    }
                    objUP.Password = Crypto.HashPassword(objUserRegistration.password);
                    objUP.IsMailSent = true;
                    objUP.CreatedDate = DateTime.Now;
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
                        OrganizationDetail org = new OrganizationDetail();
                        org.UserId = userId;
                        org.OrgName = objUserRegistration.OrgName;
                        org.OrgEmail = objUserRegistration.OrgEmail;
                        org.OrgPhoneNumber = objUserRegistration.OrgPhoneNumber;
                        org.OrgStreetName = objUserRegistration.OrgStreetName;
                        org.OrgPlotNo = objUserRegistration.OrgPlotNo;
                        org.OrgRegion = objUserRegistration.OrgRegion;
                        org.OrgBuildingName = objUserRegistration.OrgBuildingName;
                        org.OrgLandMark = objUserRegistration.OrgLandMark;
                        org.OrgBlockNo = objUserRegistration.OrgBlockNo;
                        org.UserAccountsType = objUserRegistration.UserAccountType;
                        org.OrganizationTypeId = objUserRegistration.OrganizationTypeId;

                        if (Session["RoleId"] != null && Session["RoleId"].ToString() == "1")
                            org.IsEnabled = true;
                        else
                            org.IsEnabled = false;
                        org.CreatedDate = DateTime.Now;
                        try
                        {
                            newOrgId = objData.SaveOrganizationDetails(org);
                            UserProfile objUPOrgId = new UserProfile();
                            objUPOrgId = objContext.UserProfiles.Find(userId);
                            objUPOrgId.OrganizationId = newOrgId;
                            objContext.SaveChanges();
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            success = false;
                            ErrorLog.LogThisError(ex);
                        }
                        UserAddressDetail objAddress = new UserAddressDetail();
                        objAddress.UserId = userId;
                        objAddress.FirstName = objUserRegistration.FirstName;
                        objAddress.LastName = objUserRegistration.LastName;
                        objAddress.Designation = objUserRegistration.Designation;
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



                        var selectedPayments = new List<PaymentMethodModel>();
                        var postedPaymentTypeIds = new string[0];
                        if (objUserRegistration.PostedPaymentMethods == null) objUserRegistration.PostedPaymentMethods = new PostedPaymentMethods();


                        // if a view model array of posted payment
                        // and is not empty,save selected ids
                        if (objUserRegistration.PostedPaymentMethods.PaymentTypeIds != null)
                        {
                            SelectedPaymentMethod objPM = new SelectedPaymentMethod();

                            postedPaymentTypeIds = objUserRegistration.PostedPaymentMethods.PaymentTypeIds;
                            for (int i = 0; i < postedPaymentTypeIds.Length; i++)
                            {
                                objPM.PaymentTypeId = Convert.ToInt32(postedPaymentTypeIds[i]);
                                objPM.UserId = userId;
                                objData.SaveSelectedPaymentMethod(objPM);
                            }

                        }

                    }


                    var selectedDepartments = new List<DepartmentModel>();
                    var postedDepartmentsIds = new string[0];
                    if (objUserRegistration.PostedDepartment == null) objUserRegistration.PostedDepartment = new PostedDepartment();


                    // if a view model array of posted payment
                    // and is not empty,save selected ids
                    if (objUserRegistration.PostedDepartment.DepartmentIDs != null)
                    {
                        SelectedDepartment objDep = new SelectedDepartment();

                        postedDepartmentsIds = objUserRegistration.PostedDepartment.DepartmentIDs;
                        for (int i = 0; i < postedDepartmentsIds.Length; i++)
                        {
                            objDep.DepartmentID = Convert.ToInt32(postedDepartmentsIds[i]);
                            objDep.OrgID = newOrgId;
                            objData.SaveSelectedDepartment(objDep);
                        }

                    }

                    //Adding customer Template
                    if (objUserRegistration.extraFields != null)
                    {
                        if (objUserRegistration.extraFields.Count() > 0)
                        {

                            foreach (ClientWiseCustomerTemplate item in objUserRegistration.extraFields)
                            {
                                item.KeyName = item.KeyName == null ? "Name" : item.KeyName;
                                var key = objContext.ClientWiseCustomerTemplates.Where(c => c.ClientID == newOrgId && c.KeyName == item.KeyName).FirstOrDefault();
                                var customerKey = objContext.TemplateKeywords.Where(t => t.TemplateKeyValue == item.KeyName.Replace(" ", "_")).FirstOrDefault();
                                if (key == null)
                                {
                                    ClientWiseCustomerTemplate objExtra = new ClientWiseCustomerTemplate();
                                    objExtra.ClientID = newOrgId;
                                    objExtra.KeyName = item.KeyName;
                                    objExtra.Show = item.Show;

                                    objContext.ClientWiseCustomerTemplates.Add(objExtra);

                                }
                                if (customerKey == null)
                                {
                                    TemplateKeyword keyObj = new TemplateKeyword();

                                    keyObj.TemplateKeyValue = item.KeyName.Replace(" ", "_");
                                    keyObj.TemplateKeyLabels = item.KeyName;
                                    keyObj.MultipleKeys = false;
                                    keyObj.IsEnabled = true;
                                    keyObj.TemplateKeyCategory = 1;
                                    objContext.TemplateKeywords.Add(keyObj);
                                }
                                objContext.SaveChanges();
                            }

                        }
                    }
                    else
                    {
                        ClientWiseCustomerTemplate objExtra = new ClientWiseCustomerTemplate();
                        objExtra.ClientID = newOrgId;
                        objExtra.KeyName = "Name";
                        objExtra.Show = true;

                        objContext.ClientWiseCustomerTemplates.Add(objExtra);

                        var keys = objContext.TemplateKeywords.Where(m => m.TemplateKeyValue == "Name").Count();

                        if (keys < 1)
                        {
                            TemplateKeyword keyObj = new TemplateKeyword();

                            keyObj.TemplateKeyValue = "Name";
                            keyObj.TemplateKeyLabels = "Name";
                            keyObj.MultipleKeys = false;
                            keyObj.IsEnabled = true;
                            keyObj.TemplateKeyCategory = 1;
                            objContext.TemplateKeywords.Add(keyObj);
                        }
                        objContext.SaveChanges();

                    }

                    if (success == true)
                    {

                        objLog.Action = "Insert";
                        objLog.UserId = userId;
                        objLog.FirstName = objUserRegistration.FirstName;
                        objLog.LastName = objUserRegistration.LastName;
                        objLog.Designation = objUserRegistration.Designation;
                        objLog.EmailAddress = objUserRegistration.EmailAddress;
                        objLog.IsEnabled = false;
                        objLog.ModifiedDate = DateTime.Now;
                        objLog.OrgId = newOrgId;
                        if (Session["UserId"] == null)
                        {
                            objLog.ModifierId = userId;
                        }
                        else
                        {
                            objLog.ModifierId = Convert.ToInt32(Session["UserId"]);
                        }

                        int result = objData.LogRegistrations(objLog);
                        // Log insert process

                        var postedPaymentTypeIds = new string[0];
                        if (objUserRegistration.PostedPaymentMethods.PaymentTypeIds != null)
                        {
                            postedPaymentTypeIds = objUserRegistration.PostedPaymentMethods.PaymentTypeIds;
                            for (int i = 0; i < postedPaymentTypeIds.Length; i++)
                            {
                                objData.LogUserPaymentType(result, userId, Convert.ToInt32(postedPaymentTypeIds[i]));
                            }

                        }



                        try
                        {
                            MailSend objMail = new MailSend();
                            if (Session["RoleId"] != null && Session["RoleId"].ToString() == "1")
                            {
                                objMail.SendMailForAdminUserCreation(objUserRegistration, "New Account Created", ConfigurationManager.AppSettings["ApplicationTitle"].ToString());
                            }
                            else
                            {
                                string AdminName = "";
                                VirtualAdvocateEntities db = new VirtualAdvocateEntities();
                                var objadmin = db.UserProfiles.Where(c => c.RoleId == 1 && c.IsEnabled == true).FirstOrDefault();
                                if (objadmin != null)
                                {
                                    var objAdminUP = db.UserAddressDetails.Find(objadmin.UserID);
                                    if (objAdminUP != null)
                                    {
                                        AdminName = objAdminUP.FirstName + " " + objAdminUP.LastName;
                                    }
                                }
                                objMail.RegisterNotificationEmailForAdmin(AdminName, ConfigurationManager.AppSettings["ApplicationTitle"].ToString(), Common.Helper.GetBaseUrl());
                                objMail.SendActivationEmail(objUserRegistration, ConfigurationManager.AppSettings["ApplicationTitle"].ToString(), Common.Helper.GetBaseUrl());
                            }
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

            }
            if (Convert.ToInt32(Session["RoleId"]) == 1)
            {
                return RedirectToAction("OrganizationList", "UsersManagement");

            }
            else
                return RedirectToAction("Thankyou", "UsersRegistration");
        }
        #endregion

        #region DueDiligenceRegister
        [HttpGet]
        public ActionResult DueDiligenceRegister(int? id)
        {
            VirtualAdvocateData objData = new VirtualAdvocateData();
            DueDiligenceUserViewModel obj = new DueDiligenceUserViewModel();
            obj.AvailablePaymentMethods = ServicesRepository.GetAllPaymentMethods().ToList(); // Getting all payment methods 
            var selectedPayMethods = new List<PaymentMethodModel>();
            obj.SelectedPaymentMethods = selectedPayMethods;
            if (id != null)
                obj.EnquiryType = id.Value.ToString();
            return View(obj);
        }
        #endregion

        #region DueDiligenceRegister
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DueDiligenceRegister(DueDiligenceUserViewModel
            objUserRegistration)
        {
            int userId = 0;
            DueDiligenceUserViewModel objLogDue = new DueDiligenceUserViewModel();
            objLogDue = objUserRegistration;
            using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
            {
                // var chkExisting = objContext.UserProfiles.Where(a => a.EmailAddress == objUserRegistration.EmailAddress).FirstOrDefault();
                //var errors = ModelState.Values.SelectMany(v => v.Errors);
                if (ModelState.IsValid)
                {
                    try
                    {
                        var success = false;
                        VirtualAdvocateData objData = new VirtualAdvocateData();
                        UserProfile objUP = new UserProfile();
                        objUP.RoleId = 7;
                        objUP.EmailAddress = objUserRegistration.EmailAddress;
                        objUP.IsEnabled = false;
                        objUP.HasActivated = false;
                        objUP.Password = Crypto.HashPassword(objUserRegistration.password);
                        objUP.IsMailSent = true;
                        objUP.CreatedDate = DateTime.Now;
                        //objUP.TermsConditions = objUserRegistration.TermsConditions;
                        try
                        {
                            userId = objData.SaveUserProfile(objUP);
                            success = true;
                        }
                        catch (Exception)
                        {
                            success = false;
                        }
                        if (userId != 0)
                        {
                            UserAddressDetail objAddress = new UserAddressDetail();
                            objAddress.UserId = userId;
                            objAddress.FirstName = objUserRegistration.FirstName;
                            objAddress.LastName = objUserRegistration.LastName;
                            objAddress.PhoneNumber = objUserRegistration.PhoneNumber;
                            objAddress.StreetName = objUserRegistration.StreetName;
                            objAddress.PlotNumber = objUserRegistration.PlotNo;
                            objAddress.Region = objUserRegistration.Region;
                            objAddress.BuildingName = objUserRegistration.BuildingName;
                            objAddress.LandMark = objUserRegistration.LandMark;
                            objAddress.BlockNumber = objUserRegistration.BlockNo;
                            objAddress.CreatedDate = DateTime.Now;
                            try
                            {
                                objData.SaveUserPersonalDetails(objAddress);
                                success = true;
                            }
                            catch (Exception)
                            {
                                success = false;
                            }


                            var selectedPayments = new List<PaymentMethodModel>();
                            var postedPaymentTypeIds = new string[0];
                            if (objUserRegistration.PostedPaymentMethods == null) objUserRegistration.PostedPaymentMethods = new PostedPaymentMethods();


                            // if a view model array of posted payment
                            // and is not empty,save selected ids
                            if (objUserRegistration.PostedPaymentMethods.PaymentTypeIds != null)
                            {
                                SelectedPaymentMethod objPM = new SelectedPaymentMethod();

                                postedPaymentTypeIds = objUserRegistration.PostedPaymentMethods.PaymentTypeIds;
                                for (int i = 0; i < postedPaymentTypeIds.Length; i++)
                                {
                                    objPM.PaymentTypeId = Convert.ToInt32(postedPaymentTypeIds[i]);
                                    objPM.UserId = userId;
                                    objData.SaveSelectedPaymentMethod(objPM);
                                }

                            }


                        }
                        if (success == true)
                        {
                            var postedPaymentTypeIds = new string[0];
                            int logid = objData.LogDueDiligenceUsers(objUserRegistration, userId); // Log insert process
                                                                                                   //Log insert Payment Type
                            if (objUserRegistration.PostedPaymentMethods.PaymentTypeIds != null)
                            {
                                postedPaymentTypeIds = objUserRegistration.PostedPaymentMethods.PaymentTypeIds;
                                for (int i = 0; i < postedPaymentTypeIds.Length; i++)
                                {
                                    objData.LogDuePaymentType(logid, userId, Convert.ToInt32(postedPaymentTypeIds[i]));
                                }
                            }


                            try
                            {
                                MailSend objMail = new MailSend();
                                objMail.SendActivationDueDiligenceEmail(objUserRegistration, ConfigurationManager.AppSettings["ApplicationTitle"].ToString(), Common.Helper.GetBaseUrl());

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


                }
                else
                {
                    //TempData["UserExist"] = "Yes";
                    // return View(objUserRegistration);
                    //return RedirectToAction("Login", "UsersRegistration");
                }



            }
            return RedirectToAction("Thankyou", "UsersRegistration");
        }
        #endregion

        #region CheckUserexist
        [HttpGet]
        public JsonResult CheckUserexist(string EmailAddress)
        {
            using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
            {
                var chkExisting =
                 objContext.UserProfiles
                .Where(a => string.Compare(a.EmailAddress, EmailAddress, true) == 0)
                .FirstOrDefault();

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
        #endregion

        #region ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        #endregion

        #region ForgotPassword
        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
                {

                    var userAvailable = objContext.UserProfiles.Where(au => au.EmailAddress == model.EmailAddress).FirstOrDefault();
                    if (userAvailable != null)
                    {
                        if (userAvailable.IsEnabled == true)
                        {
                            PasswordRequest request = new PasswordRequest();
                            request.RquestTime = Common.Helper.GetUtcDate(objContext).Ticks;
                            request.UserId = userAvailable.UserID;
                            objContext.PasswordRequests.Add(request);
                            objContext.SaveChanges();

                            MailSend acc = new MailSend();
                            try
                            {
                                string message = acc.SendMailForPasswordCreation(model.EmailAddress, "Virtual Advocate PASSWORD RESET REQUEST", ConfigurationManager.AppSettings["ApplicationTitle"].ToString(), request.RquestTime);
                                ViewBag.StatusMessage = message;
                            }
                            catch (Exception ex)
                            {
                                ErrorLog.LogThisError(ex);
                            }

                        }
                        else
                        {
                            ViewBag.StatusMessage = "User Is inactive can not send a password";
                        }

                    }
                    else
                    {
                        ViewBag.StatusMessage = "User Not Registered Please register";
                    }
                }
            }
            return View(model);
        }
        #endregion

        #region ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string EmailAddress, string CheckPoint)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.EmailAddress = EmailAddress;
            model.CheckPoint = CheckPoint;
            return View(model);
        }
        #endregion

        #region ResetPassword
        [AllowAnonymous]
        [HttpPost]
        public JsonResult ResetPassword(ResetPasswordViewModel model)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            bool operationSuccess = true;
            using (VirtualAdvocateEntities context = new VirtualAdvocateEntities())
            {
                try
                {
                    long result;
                    bool success = long.TryParse(model.CheckPoint, out result);

                    if (success)
                    {
                        var objCheckUsr =
                         context.UserProfiles.Where(un => un.EmailAddress == model.EmailAddress).FirstOrDefault();

                        var forgotPasswordRequests = context.PasswordRequests.Where(fpr => fpr.RquestTime == result && fpr.UserId == objCheckUsr.UserID).FirstOrDefault();
                        if (forgotPasswordRequests != null)
                        {
                            if (new DateTime(forgotPasswordRequests.RquestTime).AddHours(24) > Helper.GetUtcDate(context))
                            {
                                var userProfile = context.UserProfiles.Where(up => up.EmailAddress == model.EmailAddress).FirstOrDefault();
                                userProfile.Password = Crypto.HashPassword(model.NewPassword);
                                userProfile.UserPasswordLastExpieredOn = DateTime.Now.Date;
                                context.Configuration.ValidateOnSaveEnabled = false;
                                context.SaveChanges();

                                var userName = context.UserAddressDetails
                                    .Where(up => up.EmailAddress == model.EmailAddress).FirstOrDefault();

                                MailSend acc = new MailSend();
                                try
                                {
                                    acc.SendEmailforChangePassword(model.EmailAddress, userName.FirstName + " " + userName.LastName);

                                }
                                catch (Exception ex)
                                {
                                    ErrorLog.LogThisError(ex);
                                }

                                message = "Your password has been updated successfully";
                            }
                            else
                            {
                                message = "Link has expired";
                                operationSuccess = false;
                            }
                        }
                        else
                        {
                            message = "Invalid Link";
                            operationSuccess = false;
                        }
                    }
                    else
                    {
                        message = "Invalid link";
                        operationSuccess = false;
                    }

                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                    message = "An error occured while processing the request. Try again later";
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                }

            }

            return Json(new { message = message, success = operationSuccess }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region LogOff
        public ActionResult LogOff()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
            Response.Cache.SetNoStore();
            Response.AddHeader("Pragma", "no-cache");
            Response.Expires = 0;
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
        #endregion

        #region Thankyou
        public ActionResult Thankyou()
        {
            return View();
        }
        #endregion

        #region ChangePassword
        public ActionResult ChangePassword()
        {
            if (Session["UserId"] != null &&
             !string.IsNullOrEmpty(Session["UserId"].ToString()))
            {
                ChangePassword obj = new Models.ChangePassword();
                obj.getAllUsers = objData.getUserList(Convert.ToInt32(Session["RoleId"] != null ? Session["RoleId"].ToString() : "0"), Convert.ToInt32(Session["UserId"].ToString()), Convert.ToInt32(Session["DepartmentID"] != null ? Session["DepartmentID"].ToString() : "0"), Convert.ToInt32(Session["OrgId"] != null ? Session["OrgId"].ToString() : "0"));
                return View(obj);
            }
            else
                return RedirectToAction("Index", "Login");
        }
        #endregion

        #region ChangePassword
        [AllowAnonymous]
        [HttpPost]
        public JsonResult ChangePassword(ChangePassword obj)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            var message = string.Empty;
            bool operationSuccess = true;
            using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
            {
                try
                {
                    int userId = 0;
                    if (obj.userId == 0)
                        userId = Convert.ToInt32(Session["UserId"]);
                    else
                        userId = obj.userId;
                    var userAvailable = objContext.UserProfiles.Where(au => au.UserID == userId && au.HasActivated == true && au.IsEnabled == true).FirstOrDefault();

                    if (userAvailable != null)
                    {
                        if (Crypto.VerifyHashedPassword(userAvailable.Password, obj.password))
                        {
                            userAvailable.Password = Crypto.HashPassword(obj.newpassword);
                            userAvailable.UserPasswordLastExpieredOn = DateTime.Now.Date;
                            objContext.SaveChanges();
                            operationSuccess = true;
                            message = "Password Changed "; //success

                            var userName = objContext.UserAddressDetails
                                   .Where(up => up.UserId == userAvailable.UserID).FirstOrDefault();

                            MailSend acc = new MailSend();
                            try
                            {
                                acc.SendEmailforChangePassword(userAvailable.EmailAddress, userName.FirstName + " " + userName.LastName);

                            }
                            catch (Exception ex)
                            {
                                ErrorLog.LogThisError(ex);
                            }


                        }
                        else
                        {
                            operationSuccess = false;
                            message = "Password Wrong"; //password wrong
                        }

                    }
                    else
                    {
                        operationSuccess = false;
                        message = "An error occured while processing the request. Try again later";
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                    ViewBag.message = "An error occured while processing the request. Try again later";
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

            }
            return Json(new { message = message, success = operationSuccess }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Navigation

        #region About
        public ActionResult About()
        {
            return View();
        }
        #endregion

        #region Contact
        public ActionResult Contact()
        {
            return View();
        }
        #endregion

        #region Terms
        public ActionResult Terms()
        {
            return View();
        }
        #endregion

        #region Policy
        public ActionResult Policy()
        {
            return View();
        } 
        #endregion

        #endregion
    }
    #endregion
}
#endregion