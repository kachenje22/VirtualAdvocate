#region NameSpaces
using System;
using System.Configuration;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using VirtualAdvocate.Common;
using VirtualAdvocate.DAL;
using VirtualAdvocate.Models;
#endregion
#region VirtualAdvocate.Controllers
namespace VirtualAdvocate.Controllers
{
    #region LoginController
    public class LoginController : Controller
    {
        #region Index
        // GET: Login
        public ActionResult Index()
        {
            LoginModel objLogin = new LoginModel();
            return View(objLogin);
        }
        #endregion

        #region Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        } 
        #endregion

        [HttpPost]
        public ActionResult Login(LoginModel Login, 
          string returnUrl, string Email)
        {
            if (ModelState.IsValid)
            {
                int status = ValidateUser(Login);
                if (status.Equals(1))
                {
                    UserProfile user = new VirtualAdvocateEntities().UserProfiles.First(usr => usr.EmailAddress.Equals(Login.EmailAddress.Trim()));
                    OrganizationDetail objOrg = new VirtualAdvocateEntities().OrganizationDetails.Where(x => x.UserId == user.UserID).FirstOrDefault();
                    using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
                    {
                        if (user.RoleId != 1)
                        {
                            LoginHistory obj = new LoginHistory();

                            obj.LoginDate = DateTime.Now;
                            obj.Status = status;
                            obj.UserId = user.UserID;

                            objContext.LoginHistories.Add(obj);
                            objContext.SaveChanges();
                        }
                    }

                    if (objOrg != null)
                    {
                        Session["OrgId"] = objOrg.OrganizationId;
                    }
                    else
                    {
                        Session["OrgId"] = user.OrganizationId;
                    }
                    //FormsAuthentication.SetAuthCookie(model.UserName, true);
                    Session["UserId"] = user.UserID;
                    Session["RoleId"] = user.RoleId;
                    Session["DepartmentID"] = user.Department;
                    //if (Login.RememberMe)
                    //{
                    //    Response.Cookies["EmailAddress"].Expires = DateTime.Now.AddDays(30);
                    //    Response.Cookies["Password"].Expires = DateTime.Now.AddDays(30);
                    //    FormsAuthentication.SetAuthCookie(Login.EmailAddress, true);
                    //}
                    //else
                    //{
                    FormsAuthentication.SetAuthCookie(Login.EmailAddress, false);
                    // }
                    // var accessibleAccountCount = new VirtualAdvocateEntities().AccountUsers.Where(au => au.Users.EmailAddress == model.UserName && au.Accounts.IsActive).Count();
                    if (!string.IsNullOrEmpty(returnUrl))
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("DashBoard", "DocumentManagement", new { flagForNotification = 1 });

                }
                else if (status.Equals(2))
                {
                    ViewBag.ErrorMessage = "The user password is wrong";
                }
                else if (status.Equals(3))
                {
                    ViewBag.ErrorMessage = "User cannot login until admin approves";

                }
                else if (status.Equals(4))
                {
                    ViewBag.ErrorMessage = "Invalid User!";
                }
                else if (status.Equals(6))
                {
                    ViewBag.ErrorMessage = "Password has been expired. Please change your password";
                }
                else if (status.Equals(7))
                {
                    ViewBag.ErrorMessage = "Your account is locked, Please contact your Administrator";
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid User!";
                }
            }
            else if (Login.EmailAddress != "" && Login.Password != "" && Login.EmailAddress != null && Login.Password != null)
            {
                // If we got this far, something failed, redisplay form
                ViewBag.ErrorMessage = "The email or password provided is incorrect";
            }


            TempData["ErrorMessage"] = ViewBag.ErrorMessage;
            return RedirectToAction("Index", new LoginModel());
        }

        public int ValidateUser(LoginModel loginModel)
        {
            int status = 0;
            using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
            {
                try
                {
                    var objCheckUsr = objContext.UserProfiles.Where(un => un.EmailAddress == loginModel.EmailAddress && un.IsEnabled == false).FirstOrDefault();
                    var PendingUser = objContext.UserProfiles.Where(au => au.EmailAddress == loginModel.EmailAddress && au.HasActivated == false && au.IsEnabled == false).FirstOrDefault();
                    var userAvailable = objContext.UserProfiles.Where(au => au.EmailAddress == loginModel.EmailAddress && au.HasActivated == true && au.IsEnabled == true).FirstOrDefault();

                    if (userAvailable != null)
                    {
                        if (userAvailable.UnusedUser)
                        {
                            status = 7;
                        }
                        else
                        {
                            if (Crypto.VerifyHashedPassword(userAvailable.Password, loginModel.Password))
                            {
                                int cutoffdays = Convert.ToInt32(ConfigurationManager.AppSettings["PasswordExpiry"]);
                                if (userAvailable.UserPasswordLastExpieredOn != null ? (userAvailable.UserPasswordLastExpieredOn.Value.AddDays(cutoffdays)) > DateTime.Now.Date ? false : true : false)
                                {
                                    status = 6;
                                }

                                else
                                {
                                    int usercutoffdays = Convert.ToInt32(ConfigurationManager.AppSettings["UserExpiry"]);

                                    var userlogin = objContext.LoginHistories.Where(d => d.UserId == userAvailable.UserID && d.Status == 1).OrderByDescending(cv => cv.LoginDate).FirstOrDefault();
                                    if (userlogin != null && ((userlogin.LoginDate.AddDays(usercutoffdays)) > DateTime.Now.Date ? false : true))
                                    {
                                        userAvailable.UnusedUser = true;

                                        status = 7;
                                    }
                                    else
                                    {
                                        status = 1; //success
                                    }
                                }
                            }
                            else
                                status = 2; //password wrong

                        }
                    }

                    else if (PendingUser != null)
                    {
                        status = 3; // waiting for admin approve
                    }
                    else if (objCheckUsr != null)
                    {
                        status = 4; // inactive user
                    }
                    else
                    {
                        status = 5;
                    }
                    objContext.SaveChanges();

                }
                catch (Exception ex)
                {
                    status = 5;
                    ErrorLog.LogThisError(ex);
                }

            }

            return status;
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult CheckPasswordResetAlert()
        {
            using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
            {
                try
                {
                    Int32 userID = Convert.ToInt32(Session["UserId"].ToString());
                    var userAvailable = objContext.UserProfiles.Where(au => au.UserID == userID).FirstOrDefault();
                    int cutoffdays = Convert.ToInt32(ConfigurationManager.AppSettings["PasswordExpiry"]);
                    if (userAvailable.UserPasswordLastExpieredOn != null ? (userAvailable.UserPasswordLastExpieredOn.Value.AddDays(cutoffdays)) == DateTime.Now.Date.AddDays(3) ? true : false : false)
                    {
                        return Json("3");
                    }
                    else if (userAvailable.UserPasswordLastExpieredOn != null ? (userAvailable.UserPasswordLastExpieredOn.Value.AddDays(cutoffdays)) == DateTime.Now.Date.AddDays(2) ? true : false : false)
                    {
                        return Json("2");
                    }
                    else if (userAvailable.UserPasswordLastExpieredOn != null ? (userAvailable.UserPasswordLastExpieredOn.Value.AddDays(cutoffdays)) == DateTime.Now.Date.AddDays(1) ? true : false : false)
                    {
                        return Json("1");
                    }
                    else
                        return Json("0");
                }
                catch (Exception ex)
                {
                    ErrorLog.LogThisError(ex);
                    return Json("0");
                }

            }
        }
    } 
    #endregion
} 
#endregion