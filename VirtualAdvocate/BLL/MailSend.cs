using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtualAdvocate.Models;
using VirtualAdvocate.DAL;
using System.IO;
using System.Configuration;
using VirtualAdvocate.Common;

namespace VirtualAdvocate.BLL
{
    public class MailSend : IEmail
    {
        public string[] ToAddress { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public string[] CCAddress { get; set; }

        public bool SendActivationEmail(UserRegistrationModel objCCUser, string applicationTitleValue, string applicationAddress)
        {
            string emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/UserRegistrationTemplate.txt")).ReadToEnd();
            emailBody = emailBody.Replace("±FULLNAME", objCCUser.FirstName);
            emailBody = emailBody.Replace("±APPLICATIONLOGINURL", applicationAddress + @"/Login/Index");
            emailBody = emailBody.Replace("±APPLICATIONTITLE", applicationTitleValue);

            string emailAddress = null;
            //string emailAddress2 = string.Empty;

            ///This section changes the email address based on the application state. 
            ///

            if (objCCUser.EmailAddress != null)
            {
                emailAddress = objCCUser.EmailAddress;
            }
            else if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["TestEmailAccount"].ToString();//.Split(',')[0];
               // emailAddress2 = ConfigurationManager.AppSettings["TestEmailAccount"].ToString().Split(',')[1];
            }

            MailSend objAcc;
            if (emailAddress != null)
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Welcome To Virtual Advocate" };
            else
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Welcome To Virtual Advocate" };
            Email objEmail = new Email(objAcc);

            if (objEmail.SendEmail())
                return true;
            else
                return false;

        }

        public bool ActivationNotificationEmail(string FirstName, string EmailAddress, string applicationTitleValue, string applicationAddress)
        {
            string emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/UserActivationTemplate.txt")).ReadToEnd();
            emailBody = emailBody.Replace("±FULLNAME", FirstName);
            emailBody = emailBody.Replace("±APPLICATIONLOGINURL", applicationAddress + @"/Login/Index");
            emailBody = emailBody.Replace("±APPLICATIONTITLE", applicationTitleValue);

            string emailAddress = null;
            //string emailAddress2 = string.Empty;

            ///This section changes the email address based on the application state. 
            ///

            if (EmailAddress != null)
            {
                emailAddress = EmailAddress;
            }
            else if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["TestEmailAccount"].ToString();//.Split(',')[0];
                //emailAddress2 = ConfigurationManager.AppSettings["TestEmailAccount"].ToString().Split(',')[1];
            }

            MailSend objAcc;
            if (emailAddress != null)
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Your Virtual Advocate Account Has Been Activated" };
            else
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Your Virtual Advocate Account Has Been Activated" };
            Email objEmail = new Email(objAcc);

            if (objEmail.SendEmail())
                return true;
            else
                return false;

        }

        public bool SendActivationDueDiligenceEmail(DueDiligenceUserViewModel objCCUser, string applicationTitleValue, string applicationAddress)
        {
            string emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/UserRegistrationTemplate.txt")).ReadToEnd();
            emailBody = emailBody.Replace("±FULLNAME", objCCUser.FirstName);
            emailBody = emailBody.Replace("±APPLICATIONLOGINURL", applicationAddress + @"/Login/Index");
            emailBody = emailBody.Replace("±APPLICATIONTITLE", applicationTitleValue);

            string emailAddress=null;
            //string emailAddress2 = string.Empty;

            ///This section changes the email address based on the application state. 
            ///
            
            if(objCCUser.EmailAddress != null)
            { 
                emailAddress = objCCUser.EmailAddress;
            }
            else if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["TestEmailAccount"].ToString();//.Split(',')[0];
               // emailAddress2 = ConfigurationManager.AppSettings["TestEmailAccount"].ToString().Split(',')[1];
            }

            MailSend objAcc;
            if (emailAddress != null)
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Your Virtual Advocate Account" };
            else
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Your Virtual Advocate Account" };
            Email objEmail = new Email(objAcc);

            if (objEmail.SendEmail())
                return true;
            else
                return false;

        }

        public string SendMailForPasswordCreation(string EmailAddress, string emailSubject, string applicationTitleValue, long requestTime)
        {
            string emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/ForgotPasswordTemplate.txt")).ReadToEnd();

            string applicationAddress = Common.Helper.GetBaseUrl();

            emailBody = emailBody.Replace("±FULLNAME", EmailAddress);

            emailBody = emailBody.Replace("±SETPASSWORDURL", applicationAddress + "/UsersRegistration/ResetPassword?EmailAddress=" + HttpContext.Current.Server.UrlEncode(EmailAddress) + "&CheckPoint=" + requestTime);

            emailBody = emailBody.Replace("±APPLICATIONTITLE", applicationTitleValue);

            string emailAddress = null;
            //string emailAddress2 = string.Empty;

            ///This section changes the email address based on the application state. 
            ///

            if (EmailAddress != null)
            {
                emailAddress = EmailAddress;
            }
            else if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["TestEmailAccount"].ToString(); //.Split(',')[0];
               // emailAddress2 = ConfigurationManager.AppSettings["TestEmailAccount"].ToString().Split(',')[1];
            }

           string emailAddress1 = ConfigurationManager.AppSettings["AdminMailAddress"].ToString(); //.Split(',')[0];
            MailSend objAcc;
            if (emailAddress != null)
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress ,emailAddress1}, Subject = emailSubject };
            else
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress, emailAddress1 }, Subject = emailSubject };

            Email objEmail = new Email(objAcc);
            try { objEmail.SendEmail();
                return "Please Check your Email for the Password Reset Link";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            // if (objEmail.SendEmail())
            //{
            //    return "Please Check your Email for the Password Reset Link";
            //}
            //else
            //    return "A New password has been generated, but e-mail notification could not be sent";
        }

        public bool SendMailForUserCreation(OrgUserViewModel objCCUser, string emailSubject, string applicationTitleValue, string Comapanyname, int role)
        {
            string applicationName = ConfigurationManager.AppSettings["ApplicationName"];
            string emailBody = "";
            
           emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/UserCreationTemplate.txt")).ReadToEnd();
            
            
            string applicationAddress = Common.Helper.GetBaseUrl();

            emailBody = emailBody.Replace("±FIRSTNAME", objCCUser.FirstName);

            emailBody = emailBody.Replace("±COMPANY", Comapanyname);

            //emailBody = emailBody.Replace("±ACCOUNTNAME", account);

            emailBody = emailBody.Replace("±USERMAIL", objCCUser.EmailAddress);

            emailBody = emailBody.Replace("±AUTOGENERATEDPASSWORD", objCCUser.password);

            emailBody = emailBody.Replace("±APPLICATIONLOGINURL", applicationAddress + "/Login/Index");

            //emailBody = emailBody.Replace("±SETPASSWORDURL", applicationAddress + "/Account/ChangePassword");

            emailBody = emailBody.Replace("±APPLICATIONTITLE", applicationTitleValue);

            string emailAddress = null;
            //string emailAddress2 = string.Empty;

            ///This section changes the email address based on the application state. 
            ///

            if (objCCUser.EmailAddress != null)
            {
                emailAddress = objCCUser.EmailAddress;
            }
            else if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["TestEmailAccount"].ToString();//.Split(',')[0];
                //emailAddress2 = ConfigurationManager.AppSettings["TestEmailAccount"].ToString().Split(',')[1];
            }

            MailSend objAcc;

            if (emailAddress != null)
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = Comapanyname + " - " + "Virtual Advocate ACCOUNT CREATED & APPROVED" };
            else
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = Comapanyname + " - " + "Virtual Advocate ACCOUNT CREATED & APPROVED" };

            Email objEmail = new Email(objAcc);

            if (objEmail.SendEmail())
                return true;
            else
                return false;
        }

        public bool SendMailForAdminUserCreation(UserRegistrationModel objCCUser, string emailSubject, string applicationTitleValue)
        {
            string applicationName = ConfigurationManager.AppSettings["ApplicationName"];
            string emailBody = "";

            emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/AdminUserCreationTemplate.txt")).ReadToEnd();


            string applicationAddress = Common.Helper.GetBaseUrl();

            emailBody = emailBody.Replace("±FIRSTNAME", objCCUser.FirstName);

            
            emailBody = emailBody.Replace("±USERMAIL", objCCUser.EmailAddress);

            emailBody = emailBody.Replace("±AUTOGENERATEDPASSWORD", objCCUser.password);

            emailBody = emailBody.Replace("±APPLICATIONLOGINURL", applicationAddress + "/Login/Index");

            emailBody = emailBody.Replace("±APPLICATIONTITLE", applicationTitleValue);

            string emailAddress = null;
            //string emailAddress2 = string.Empty;

            ///This section changes the email address based on the application state. 
            ///

            if (objCCUser.EmailAddress != null)
            {
                emailAddress = objCCUser.EmailAddress;
            }
            else if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["TestEmailAccount"].ToString();//.Split(',')[0];
                //emailAddress2 = ConfigurationManager.AppSettings["TestEmailAccount"].ToString().Split(',')[1];
            }

            MailSend objAcc;

            if (emailAddress != null)
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Virtual Advocate ACCOUNT CREATED" };
            else
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Virtual Advocate ACCOUNT CREATED" };

            Email objEmail = new Email(objAcc);

            if (objEmail.SendEmail())
                return true;
            else
                return false;
        }

        public bool EnquiryNotificationEmailForAdmin(string AdminName,string SenderName,string inqtype, string Filed, string applicationTitleValue, string applicationAddress)
        {
            string emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/EnquiryNotificationForAdmin.txt")).ReadToEnd();
            emailBody = emailBody.Replace("<<ADMIN>>", AdminName);
            emailBody = emailBody.Replace("<<SENDER>>", SenderName);
            emailBody = emailBody.Replace("<<INQUIRYTYPE>>", inqtype);
            emailBody = emailBody.Replace("<<APPLICATIONTITLE>>", applicationTitleValue);
            emailBody = emailBody.Replace("<<FIELDS>>", Filed);

            string emailAddress = null;
            if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["AdminMailAddress"].ToString();
            }

            MailSend objAcc;
            if (emailAddress != null)
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "New Enquiry" };
            else
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "New Enquiry" };
            Email objEmail = new Email(objAcc);

            if (objEmail.SendEmail())
                return true;
            else
                return false;

        }
        public bool EnquiryNotificationEmailForDueUser(string FirstName, string EmailAddress,int day, string applicationTitleValue, string applicationAddress, string FileName)
        {
            string emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/DueDiligenceEnquiry.txt")).ReadToEnd();
            emailBody = emailBody.Replace("<<FULLNAME>>", FirstName);
            emailBody = emailBody.Replace("<<APPLICATIONTITLE>>", applicationTitleValue);
            emailBody = emailBody.Replace("<<DAY>>", day.ToString());
            

            string emailAddress = null;
            if (EmailAddress != null)
            {
                emailAddress = EmailAddress;
            }
            else if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["TestEmailAccount"].ToString();
            }
                      

            MailSend objAcc;
            if (emailAddress != null)
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Receipt of Due Diligence Inquiry" };
            else
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Receipt of Due Diligence Inquiry" };
            Email objEmail = new Email(objAcc);

            if (objEmail.SendEmailAttachment(FileName))
                return true;
            else
                return false;

        }

        public bool EnquiryReplyNotificationForDueUser(string FirstName, string EmailAddress, string timeline, string cost, string applicationTitleValue, string applicationAddress)
        {
            string emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/DueDiligenceEnquiryReply.txt")).ReadToEnd();
            emailBody = emailBody.Replace("<<FULLNAME>>", FirstName);
            emailBody = emailBody.Replace("<<APPLICATIONTITLE>>", applicationTitleValue);
            emailBody = emailBody.Replace("<<COST>>", cost);
            emailBody = emailBody.Replace("<<TIMELINE>>", timeline);

            string emailAddress = null;
            if (EmailAddress != null)
            {
                emailAddress = EmailAddress;
            }
            else if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["TestEmailAccount"].ToString();
            }

            MailSend objAcc;
            if (emailAddress != null)
            {
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "Time and Cost For Your Inquiry" };
                Email objEmail = new Email(objAcc);

                if (objEmail.SendEmail())
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }

        }

        public bool RegisterNotificationEmailForAdmin(string AdminName, string applicationTitleValue, string applicationAddress)
        {
            string emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RegisterNotificationForAdmin.txt")).ReadToEnd();
            emailBody = emailBody.Replace("<<ADMIN>>", AdminName);         
            emailBody = emailBody.Replace("<<APPLICATIONTITLE>>", applicationTitleValue);
            emailBody = emailBody.Replace("<<APPLICATIONLOGINURL>>", applicationAddress + "/Login/Index");
            string emailAddress = null;
            if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["AdminMailAddress"].ToString();
            }

            MailSend objAcc;
            if (emailAddress != null)
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "New User Registered" };
            else
                objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress }, Subject = "New User Registered" };
            Email objEmail = new Email(objAcc);

            if (objEmail.SendEmail())
                return true;
            else
                return false;

        }

        public bool SendEmailforChangePassword(string emailAaddress, string Name)
        {
           
            string emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/ChangePassword.txt")).ReadToEnd();
            emailBody = emailBody.Replace("±FULLNAME", Name);
          

            string emailAddress = null;
            //string emailAddress2 = string.Empty;

            ///This section changes the email address based on the application state. 
            ///

            if (emailAaddress != null)
            {
                emailAddress = emailAaddress;
            }
            else if (ConfigurationManager.AppSettings["ApplicationState"].ToString().Equals("0"))
            {
                emailAddress = ConfigurationManager.AppSettings["TestEmailAccount"].ToString();//.Split(',')[0];
                                                                                               // emailAddress2 = ConfigurationManager.AppSettings["TestEmailAccount"].ToString().Split(',')[1];
            }
           string  emailAddress1 = ConfigurationManager.AppSettings["AdminMailAddress"].ToString();

            MailSend objAcc;
             objAcc = new MailSend { Body = emailBody, ToAddress = new string[] { emailAddress, emailAddress1 }, Subject = "Password Change Alert" };
           
            Email objEmail = new Email(objAcc);

            if (objEmail.SendEmail())
                return true;
            else
                return false;

        }


    }
}