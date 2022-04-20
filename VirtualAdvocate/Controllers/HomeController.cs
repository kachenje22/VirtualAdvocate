using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using VirtualAdvocate.Common;
using VirtualAdvocate.DAL;
using VirtualAdvocate.Models;
using System.Configuration;

namespace VirtualAdvocate.Controllers
{
    public class HomeController : Controller
    {
        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            NewInquiry obj = new NewInquiry();

            return View(obj);
        }

        public ActionResult Service()
        {
            ViewBag.Message = "Your contact page.";
            NewTicket ticketObj = new NewTicket();
            return View(ticketObj);
        }
        public ActionResult Solutions()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult ContactSuccess()
        {

            return View();
        }

        [HttpPost]
        public ActionResult RaiseTicket(NewTicket ticketObj)
        {
            ticketObj.date = DateTime.UtcNow;
            Ticket obj = new Ticket();
            obj.Email = ticketObj.Email;
            obj.Issue = ticketObj.Issue;
            obj.BusinessImpact = ticketObj.BusinessImpact;
            obj.Organization = ticketObj.Organization;
            obj.Phone = ticketObj.Phone;
            obj.ContactPerson = ticketObj.ContactPerson;
            obj.CreatedOn = ticketObj.date;
            obj.Status = "Created";
            db.Tickets.Add(obj);
            db.SaveChanges();
            SendNewTicketMail(ticketObj);
            SendTicketConfirmationMail(ticketObj);
            return Json("", JsonRequestBehavior.DenyGet);
        }


        [HttpPost]
        public ActionResult AddContact(NewInquiry inquiryObj)
        {
           
                inquiryObj.CreatedDate = DateTime.UtcNow;
                Inquiry obj = new Inquiry();
                obj.Email = inquiryObj.Email;
                obj.Issue = inquiryObj.Issue;
                obj.Name = inquiryObj.Name;
                obj.Organization = inquiryObj.Organization;
                obj.Phone = inquiryObj.Phone;
                obj.Status = "Created";
                obj.CreatedOn = inquiryObj.CreatedDate;
                db.Inquiries.Add(obj);
                db.SaveChanges();
                SendNewInquiryMail(inquiryObj);
                SendInquiryConfirmationMail(inquiryObj);
                return RedirectToAction("ContactSuccess", "Home");
            
            
            
            //return Json(new { message = message }, JsonRequestBehavior.AllowGet);
        }

        private void SendInquiryConfirmationMail(NewInquiry inquiryObj)
        {

            string emailBody = "";

            emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/NewInquiryConfirmation.html")).ReadToEnd();

            emailBody = emailBody.Replace("$$Name$$", inquiryObj.Name);



            string emailAddress = null;

            if (inquiryObj.Email != null)
            {
                emailAddress = inquiryObj.Email;
            }

            SendMail(emailBody, "Virtual Advocate - Inquiry Confirmation", emailAddress);

        }

        private void SendTicketConfirmationMail(NewTicket inquiryObj)
        {

            string emailBody = "";

            emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/NewTicketConfirmation.html")).ReadToEnd();

            emailBody = emailBody.Replace("$$Name$$", inquiryObj.ContactPerson);



            string emailAddress = null;

            if (inquiryObj.Email != null)
            {
                emailAddress = inquiryObj.Email;
            }

            SendMail(emailBody, "Virtual Advocate - Ticket Confirmation", emailAddress);

        }

        private void SendNewInquiryMail(NewInquiry inquiryObj)
        {
            string ReceiverMail = ConfigurationManager.AppSettings["AdminMailAddress"];

            string emailBody = "";

            emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/NewInquiry.html")).ReadToEnd();

            emailBody = emailBody.Replace("$$Name$$", inquiryObj.Name);

            emailBody = emailBody.Replace("$$Organization$$", inquiryObj.Organization);

            emailBody = emailBody.Replace("$$Email$$", inquiryObj.Email);

            emailBody = emailBody.Replace("$$Phone$$", inquiryObj.Phone);

            emailBody = emailBody.Replace("$$Issue$$", inquiryObj.Issue);

            emailBody = emailBody.Replace("$$CreatedOn$$", inquiryObj.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss tt"));

            string emailAddress = null;

            if (inquiryObj.Email != null)
            {
                emailAddress = inquiryObj.Email;
            }

            SendMail(emailBody, "New Inquiry", ReceiverMail);

        }

        private void SendNewTicketMail(NewTicket ticketObj)
        {
            string ReceiverMail = ConfigurationManager.AppSettings["AdminMailAddress"];

            string emailBody = "";

            emailBody = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/NewTicket.html")).ReadToEnd();

            emailBody = emailBody.Replace("$$ContactPerson$$", ticketObj.ContactPerson);

            emailBody = emailBody.Replace("$$Email$$", ticketObj.Email);

            emailBody = emailBody.Replace("$$Phone$$", ticketObj.Phone);

            emailBody = emailBody.Replace("$$BusinessImpact$$", ticketObj.BusinessImpact);

            emailBody = emailBody.Replace("$$Organization$$", ticketObj.Organization);

            emailBody = emailBody.Replace("$$Issue$$", ticketObj.Issue);

            emailBody = emailBody.Replace("$$Date$$", ticketObj.date.ToString("dd/MM/yyyy HH:mm:ss tt"));

            string emailAddress = null;

            if (ticketObj.Email != null)
            {
                emailAddress = ticketObj.Email;
            }

            SendMail(emailBody, "New Ticket", ReceiverMail);
        }


        private bool SendMail(string emailBody, string emailSubject, string toaddress)
        {
            bool isSent = false;

            MailMessage webMail = new MailMessage();
            webMail.Subject = emailSubject;
            webMail.IsBodyHtml = true;
            webMail.Body = emailBody;
            webMail.To.Add(new MailAddress(toaddress));
            try
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.ServicePoint.MaxIdleTime = 1;
                    smtp.EnableSsl = true;
                    smtp.Timeout = 180000;
                    smtp.Send(webMail);
                    isSent = true;
                }
                webMail.Dispose();
            }

            catch (System.Net.Mail.SmtpException sme)
            {
                isSent = false;
            }


            return isSent;
        }

        public ActionResult RealEstate()
        {
            return View();
        }
        public ActionResult HumanResources()
        {
            return View();
        }
        public ActionResult LawFirms()
        {
            return View();
        }
        public ActionResult DueDiligence()
        {
            return View();
        }

        public ActionResult Banks()
        {
            return View();
        }
    }
}