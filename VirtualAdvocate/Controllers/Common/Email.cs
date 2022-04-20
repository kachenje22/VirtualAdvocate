using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Configuration;
using Elmah;

namespace VirtualAdvocate.Common
{
    public class Email
    {
        private IEmail emailProperties;

        public Email(IEmail emailProperties)
        {
            this.emailProperties = emailProperties;
        }

        /// <summary>
        /// To send email by collecting email body, to addresses and email subject
        /// </summary>
        /// <param name="webMail"></param>
        /// <param name="emailBody"></param>
        /// <param name="tempEmail"></param>
        /// <param name="emailSubject"></param>
        public bool SendEmail()
        {
            MailMessage webMail = ReturnMailMessage();
            bool returnValue;
            try
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.ServicePoint.MaxIdleTime = 1;                   
                    smtp.EnableSsl = true;
                    smtp.Timeout = 180000;
                    smtp.Send(webMail);
                }
                webMail.Dispose();
                returnValue = true;
            }
            catch (ArgumentNullException ane)
            {
                ErrorSignal.FromCurrentContext().Raise(ane);
                returnValue = false;
            }
            catch (ObjectDisposedException ode)
            {
                ErrorSignal.FromCurrentContext().Raise(ode);
                returnValue = false;
            }
            catch (InvalidOperationException ioe)
            {
                ErrorSignal.FromCurrentContext().Raise(ioe);
                returnValue = false;
            }
            catch (System.Net.Mail.SmtpFailedRecipientsException fre)
            {
                ErrorSignal.FromCurrentContext().Raise(fre);
                returnValue = false;
            }
            catch (System.Net.Mail.SmtpException sme)
            {
                ErrorSignal.FromCurrentContext().Raise(sme);
                returnValue = false;
            }

            return returnValue;
        }

        private MailMessage ReturnMailMessage()
        {
            MailMessage webMail = new MailMessage();
            string headerImage = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Images/Logo.png");
            LinkedResource[] Images = new LinkedResource[2];
            try
            {
                Images[0] = new LinkedResource(headerImage);
                Images[0].ContentId = "Header";
                AlternateView altView = AlternateView.CreateAlternateViewFromString(emailProperties.Body, null, "text/html");
                AlternateView imageHeader = new AlternateView(headerImage, MediaTypeNames.Image.Gif);
                imageHeader.ContentId = "Header";
                altView.LinkedResources.Add(Images[0]);
                webMail.AlternateViews.Add(altView);
                webMail.AlternateViews.Add(imageHeader);
                foreach (string item in emailProperties.ToAddress)
                {
                    webMail.To.Add(new MailAddress(item));
                }

                if (emailProperties.CCAddress != null)
                {
                    foreach (string item in emailProperties.CCAddress)
                    {
                        webMail.CC.Add(new MailAddress(item));
                    }
                }
                webMail.Subject = emailProperties.Subject;
                webMail.IsBodyHtml = true;
                webMail.Body = emailProperties.Body;

            }
            catch (ArgumentNullException ane)
            {
                ErrorSignal.FromCurrentContext().Raise(ane);
            }
            catch (FormatException fe)
            {
                ErrorSignal.FromCurrentContext().Raise(fe);
            }
            catch (System.Security.SecurityException se)
            {
                ErrorSignal.FromCurrentContext().Raise(se);
            }
            catch (IOException ioe)
            {
                ErrorSignal.FromCurrentContext().Raise(ioe);
            }
            catch (UnauthorizedAccessException uae)
            {
                ErrorSignal.FromCurrentContext().Raise(uae);
            }

            return webMail;
        }

        public bool SendEmailAttachment(string filename)
        {
            MailMessage webMail = ReturnMailMessageAttachment(filename);
            bool returnValue;
            try
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.ServicePoint.MaxIdleTime = 1;
                    smtp.EnableSsl = true;
                    smtp.Timeout = 180000;
                    smtp.Send(webMail);
                }
                webMail.Dispose();
                returnValue = true;
            }
            catch (ArgumentNullException ane)
            {
                ErrorSignal.FromCurrentContext().Raise(ane);
                returnValue = false;
            }
            catch (ObjectDisposedException ode)
            {
                ErrorSignal.FromCurrentContext().Raise(ode);
                returnValue = false;
            }
            catch (InvalidOperationException ioe)
            {
                ErrorSignal.FromCurrentContext().Raise(ioe);
                returnValue = false;
            }
            catch (System.Net.Mail.SmtpFailedRecipientsException fre)
            {
                ErrorSignal.FromCurrentContext().Raise(fre);
                returnValue = false;
            }
            catch (System.Net.Mail.SmtpException sme)
            {
                ErrorSignal.FromCurrentContext().Raise(sme);
                returnValue = false;
            }

            return returnValue;
        }
        private MailMessage ReturnMailMessageAttachment(string filename)
        {
            MailMessage webMail = new MailMessage();
            string headerImage = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Images/Logo.png");
            string attachFile= System.Web.Hosting.HostingEnvironment.MapPath("~/DueInvoiceFiles/"+ filename);
            LinkedResource[] Images = new LinkedResource[2];
            try
            {
                Images[0] = new LinkedResource(headerImage);
                Images[0].ContentId = "Header";
                AlternateView altView = AlternateView.CreateAlternateViewFromString(emailProperties.Body, null, "text/html");
                AlternateView imageHeader = new AlternateView(headerImage, MediaTypeNames.Image.Gif);
                imageHeader.ContentId = "Header";
                altView.LinkedResources.Add(Images[0]);
                webMail.AlternateViews.Add(altView);
                webMail.AlternateViews.Add(imageHeader);
                foreach (string item in emailProperties.ToAddress)
                {
                    webMail.To.Add(new MailAddress(item));
                }
                webMail.Subject = emailProperties.Subject;
                webMail.IsBodyHtml = true;
                webMail.Body = emailProperties.Body;
                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(attachFile);
                webMail.Attachments.Add(attachment);

            }
            catch (ArgumentNullException ane)
            {
                ErrorSignal.FromCurrentContext().Raise(ane);
            }
            catch (FormatException fe)
            {
                ErrorSignal.FromCurrentContext().Raise(fe);
            }
            catch (System.Security.SecurityException se)
            {
                ErrorSignal.FromCurrentContext().Raise(se);
            }
            catch (IOException ioe)
            {
                ErrorSignal.FromCurrentContext().Raise(ioe);
            }
            catch (UnauthorizedAccessException uae)
            {
                ErrorSignal.FromCurrentContext().Raise(uae);
            }

            return webMail;
        }
    }
}