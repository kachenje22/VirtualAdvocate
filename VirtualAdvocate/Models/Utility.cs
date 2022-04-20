using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using VirtualAdvocate.BLL;
using VirtualAdvocate.Common;

namespace VirtualAdvocate.Models
{
    public class SendMailParam
    {
        public List<string> To { get; set; }
        public List<string> CC { get; set; }
        public string ChangeFrom { get; set; }
        public string ChangeTo { get; set; }
        public string ToUserName { get; set; }
        public string UserName { get; set; }
        public string DocumentName { get; set; }
        public int RoleId { get; set; }
        public DocumentDetailStatus Status { get; set; }
        public string RejectedReason { get; set; }
    }
    public class Utility
    {
        public static string RenderPartialViewToString(Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        public static DataTable ReadExcelFile(HttpPostedFileBase file)
        {
            try
            {
                DataTable dataTable = new DataTable();
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int col = 1;

                    for (int row = 1; worksheet.Cells[row, col].Value != null; col++)
                    {
                        dataTable.Columns.Add(worksheet.Cells[row, col].Value.ToString().ToUpper().Trim());
                    }
                    DataRow dr;
                    int i = 0;
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        i = 0;
                        dr = dataTable.NewRow();
                        for (int column = 1; column <= dataTable.Columns.Count; column++)
                        {
                            if (worksheet.Cells[row, column].Value == null)
                            {
                                dr[i] = string.Empty;
                            }
                            else
                            {
                                dr[i] = worksheet.Cells[row, column].Value.ToString();
                            }
                            i++;
                        }
                        dataTable.Rows.Add(dr);
                    }

                }
                return dataTable;
            }
            catch
            {
                throw;
            }
        }

        public static void SendMail(SendMailParam param)
        {
            try
            {
                string template = string.Empty;
                if (param.RoleId == 5)
                {
                    template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath(System.Configuration.ConfigurationSettings.AppSettings["DocumentStatusChangeTemplatePath"]));

                    template = template.Replace("{ToUserName}", param.ToUserName);
                    template = template.Replace("{Username}", param.UserName);
                    template = template.Replace("{DocumentName}", param.DocumentName);
                    template = template.Replace("{ChangeFrom}", param.ChangeFrom);
                    template = template.Replace("{ChangeTo}", param.ChangeTo);
                }
                else if (param.RoleId == 6)
                {
                    if (param.Status == DocumentDetailStatus.Accept)
                    {
                        template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath(System.Configuration.ConfigurationSettings.AppSettings["DocumentApproveTemplatePath"]));

                        template = template.Replace("{Username}", param.UserName);
                        template = template.Replace("{DocumentName}", param.DocumentName);
                        template = template.Replace("{ChangeFrom}", param.ChangeFrom);
                        template = template.Replace("{ChangeTo}", param.ChangeTo);
                    }
                    else
                    {
                        template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath(System.Configuration.ConfigurationSettings.AppSettings["DocumentRejectedTemplatePath"]));

                        template = template.Replace("{Username}", param.UserName);
                        template = template.Replace("{DocumentName}", param.DocumentName);
                        template = template.Replace("{ChangeFrom}", param.ChangeFrom);
                        template = template.Replace("{ChangeTo}", param.ChangeTo);
                        template = template.Replace("{Reason}", param.RejectedReason);
                    }
                }

                string subject = System.Configuration.ConfigurationSettings.AppSettings["ChangeStatusMailSubject"];


                MailSend objAcc;

                var uatmailid = System.Configuration.ConfigurationSettings.AppSettings["UATMailId"];

                if (string.IsNullOrEmpty(uatmailid))
                {
                    objAcc = new MailSend { Body = template, ToAddress = param.To.ToArray(), CCAddress = param.CC.ToArray(), Subject = subject };
                }
                else
                {
                    objAcc = new MailSend { Body = template, ToAddress = new string[] { uatmailid }, CCAddress = param.CC.ToArray(), Subject = subject };
                }
                
                Email objEmail = new Email(objAcc);

                objEmail.SendEmail();


                //string mailId = System.Configuration.ConfigurationSettings.AppSettings["MailUserMailId"];
                //string mailPassword = System.Configuration.ConfigurationSettings.AppSettings["MailUserMailPassword"];
                //string subject = System.Configuration.ConfigurationSettings.AppSettings["ChangeStatusMailSubject"];

                //MailMessage mail = new MailMessage();
                //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                //mail.From = new MailAddress(mailId);

                
                //foreach (var item in param.To)
                //    mail.To.Add(item);

                //foreach (var item in param.CC)
                //    mail.CC.Add(item);

                //mail.Subject = subject;
                //mail.IsBodyHtml = true;
                //mail.Body = template;
                //SmtpServer.Port = 587;
                //SmtpServer.Credentials = new System.Net.NetworkCredential(mailId, mailPassword);
                //SmtpServer.EnableSsl = true;
                //SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string GetDate(object date)
        {
            double res;
            string result = string.Empty;
            if (!double.TryParse(date.ToString(), out res))
            {
                var dateTime = DateTime.ParseExact(date.ToString(), new string[] { "dd-MM-yyyy", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
                var timeZoneDt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                result = timeZoneDt.ToString("dd-MM-yyyy");

            }
            else if (double.TryParse(date.ToString(), out res))
            {
                try
                {
                    var convertedDate = DateTime.FromOADate(Convert.ToDouble(date.ToString().Trim()));
                    result = DateTime.SpecifyKind(convertedDate, DateTimeKind.Utc).ToString("dd-MM-yyyy");
                }
                catch (Exception ex)
                {
                    result = string.Empty;
                }
            }

            return result;
        }
    }
    
}