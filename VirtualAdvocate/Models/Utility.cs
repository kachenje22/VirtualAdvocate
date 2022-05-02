#region NameSpaces
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtualAdvocate.BLL;
using VirtualAdvocate.Common;
using VirtualAdvocate.Helpers;
#endregion
#region VirtualAdvocate.Models
namespace VirtualAdvocate.Models
{
    #region SendMailParam
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
    #endregion

    #region Utility
    public class Utility
    {
        #region RenderPartialViewToString
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

        #endregion

        #region ReadExcelFile
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
        #endregion

        #region SendMail
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
        #endregion

        #region GetDate
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
        #endregion
    } 
    #endregion

    #region CurrentUser
    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static int? OrgId { get; set; }
        public static int RoleId { get; set; }
        public static int? DepartmentId { get; set; }

        public static UserRoles Role { get; set; }

    }
    #endregion

    #region ConversionHelper
    public static class ConversionHelper
    {
        #region ConvertToDecimal
        public static decimal ToDecimal(this string decimalString)
        {
            decimal dValue = 0;
            decimal.TryParse(decimalString, out dValue);
            return dValue;
        }
        #endregion

        #region ToBoolean
        public static bool ToBoolean(this string boolString)
        {
            if (new string[] { "1", "TRUE", "T", "Y", "YES" }.Contains(boolString.ToUpper()))
                boolString = "true";
            bool bValue = false;
            bool.TryParse(boolString, out bValue);
            return bValue;
        }
        #endregion

        #region ConvertToInt
        public static int ToInt(this string intString)
        {
            int iValue = 0;
            int.TryParse(intString, out iValue);
            return iValue;
        }
        #endregion

        #region ConvertToInt64
        public static Int64 ToInt64(this string intString)
        {
            Int64 iValue = 0;
            Int64.TryParse(intString, out iValue);
            return iValue;
        }
        #endregion

        #region ConvertToDecimal2
        public static decimal ToDecimal2(this string decimalString)
        {
            decimal dValue = -1;
            decimal.TryParse(decimalString, out dValue);
            return dValue;
        }
        #endregion

        #region ConvertToInt
        public static int ToInt2(this string intString)
        {
            int iValue = -1;
            int.TryParse(intString, out iValue);
            return iValue;
        }
        #endregion

        #region ToDate
        public static DateTime ToDate(this string dateString)
        {
            DateTime dateTime = DateTime.MinValue;
            DateTime.TryParse(dateString, out dateTime);
            if (DateTime.MinValue.Equals(dateTime))
            {
                string[] formats = new string[2] { "dd/MM/yyyy", "MM/dd/yyyy" };
                try
                {
                    dateTime = DateTime.ParseExact(dateString, formats, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal);
                }
                catch (Exception ex)
                {

                }
            }
            return dateTime;
        }
        #endregion
    }
    #endregion

} 
#endregion