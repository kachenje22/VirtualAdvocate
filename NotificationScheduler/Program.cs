using NotificationScheduler.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;

namespace NotificationScheduler
{
    class Program
    {
        private static VirtualAdvocateEntities db = new VirtualAdvocateEntities();
        private static DateTime TodaysDate = DateTime.Today;

        static void Main(string[] args)
        {
            Logger.Log("Service started");
            try
            {
                DateTime minDate = new DateTime(2001, 1, 1), maxDate = new DateTime(2001, 1, 1);
                DateTime todaysDate = DateTime.Today;

                List<Entity> entities = new List<Entity>();

                var insurances = db.Insurances.Where(m => m.Status)
                    .Include("Property")
                    .Include("Property.FilledTemplateDetail")
                    .Select(s => new Entity
                    {
                        Id = s.Id,
                        DateOfExpiry = s.DateOfExpiry,
                        OrgId = s.Property.FilledTemplateDetail.OrgId.Value,
                        UserId = s.UserId.Value,
                        TemplateType = TemplateType.Insurance,
                        TemplateId = s.Property.FilledTemplateDetail.TemplateId
                    });
                
                var probations = db.ProbationDetails.Where(m => m.Status)
                    .Include("CustomerDetail")
                    .Include("UserProfile")
                    .Include("UserProfile.UserAddressDetails")
                    .Select(s => new Entity
                    {
                        Id = s.Id,
                        DateOfExpiry = s.DateOfExpiry,
                        OrgId = s.CustomerDetail.OrganizationId,
                        UserId = s.UserId.Value,
                        TemplateType = TemplateType.Probation,
                        CustomerName = s.CustomerDetail.CustomerName
                    }).ToList();

                var documents = db.DocumentDetails.Where(m => m.Status && m.DocumentStatus != 3)
                    .Include("FilledTemplateDetail")
                    .Select(s => new Entity
                    {
                        Id = s.Id,
                        DocumentId = s.DocumentId,
                        DateOfExpiry = s.DateToBeSubmitted,
                        OrgId = s.FilledTemplateDetail.OrgId.Value,
                        UserId = s.UserId,
                        TemplateId = s.FilledTemplateDetail.TemplateId,
                        TemplateType = TemplateType.General
                    }).ToList();

                entities.AddRange(insurances);
                entities.AddRange(probations);
                entities.AddRange(documents);

                //var superAdmins = db.UserProfiles.Where(m => m.RoleId == 1).ToList();

                foreach (var item in entities)
                {
                    try
                    {
                        //var orgId = item.Property.FilledTemplateDetail.OrgId;

                        var recurrsDetails = db.RecursiveNotificationDetails.FirstOrDefault(m => m.OrgId == item.OrgId);

                        if (recurrsDetails != null)
                        {
                            if (recurrsDetails.RecurrsBeforeDays != null)
                            {
                                minDate = item.DateOfExpiry.AddDays(-recurrsDetails.RecurrsBeforeDays.Value);
                            }
                            else
                            {
                                minDate = item.DateOfExpiry;
                            }

                            if (recurrsDetails.RecurrsAfterDays != null)
                            {
                                maxDate = item.DateOfExpiry.AddDays(recurrsDetails.RecurrsAfterDays.Value);
                            }
                            else
                            {
                                maxDate = item.DateOfExpiry;
                            }
                        }
                        else
                        {
                            minDate = item.DateOfExpiry;
                            maxDate = item.DateOfExpiry;
                        }

                        if (minDate.Date <= todaysDate.Date && maxDate.Date >= todaysDate.Date)
                        {
                            TemplateCategory category;
                            if (item.DateOfExpiry == DateTime.Today.Date)
                                category = TemplateCategory.OnExpiry;
                            else if (item.DateOfExpiry >= DateTime.Today.Date)
                                category = TemplateCategory.PriorToExpiry;
                            else
                                category = TemplateCategory.AfterExpiry;

                            var user = db.UserProfiles.FirstOrDefault(f => f.UserID == item.UserId);
                            var userDetails = db.UserAddressDetails.FirstOrDefault(f => f.UserId == item.UserId);
                            Placeholder placeholder;
                            if (item.TemplateType == TemplateType.Insurance)
                            {
                                placeholder = new Placeholder
                                {
                                    UserName = userDetails.FirstName,
                                    DocumentName = db.DocumentTemplates.FirstOrDefault(m => m.TemplateId == item.TemplateId).DocumentTitle,
                                    NumberOfDays = Math.Abs(Convert.ToInt32((item.DateOfExpiry - DateTime.Today).TotalDays)),
                                    Category = category,
                                    TemplateType = item.TemplateType
                                };
                            }
                            else if(item.TemplateType == TemplateType.Probation)
                            {
                                placeholder = new Placeholder
                                {
                                    UserName = userDetails.FirstName,
                                    EmployeeName = item.CustomerName,
                                    NumberOfDays = Math.Abs(Convert.ToInt32((item.DateOfExpiry - DateTime.Today).TotalDays)),
                                    Category = category,
                                    TemplateType = item.TemplateType
                                };
                            }
                            else
                            {
                                placeholder = new Placeholder
                                {
                                    UserName = userDetails.FirstName,
                                    DocumentName = db.FilledTemplateDetails.FirstOrDefault(f => f.RowId == item.DocumentId).FilledTemplateName,
                                    //EmployeeName = item.CustomerName,
                                    NumberOfDays = Math.Abs(Convert.ToInt32((item.DateOfExpiry - DateTime.Today).TotalDays)),
                                    Category = category,
                                    TemplateType = item.TemplateType
                                };
                            }

                            var uatmailid = System.Configuration.ConfigurationSettings.AppSettings["UATMailId"];

                            if (string.IsNullOrEmpty(uatmailid))
                            {
                                SendMail(user.EmailAddress, placeholder, item.OrgId, user.Department);
                            }
                            else
                            {
                                SendMail(uatmailid, placeholder, item.OrgId, user.Department);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(item.TemplateType.ToString() + "Id: " + item.Id + "\nError: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        public static void SendMail(string to, Placeholder placeholder, int? orgId, int? deptId)
        {
            Console.WriteLine("Mail Sending process..");
            try
            {
                List<UserProfile> admins = new List<UserProfile>();
                if (orgId != null && deptId != null)
                {
                    admins = db.UserProfiles.Where(m => m.OrganizationId == orgId.Value && m.Department == deptId.Value && m.RoleId == 6)
                        .ToList();
                }
                string template = string.Empty;
                string path = string.Empty;

                if (placeholder.TemplateType == TemplateType.Insurance)
                {
                    if (placeholder.Category == TemplateCategory.OnExpiry)
                    {
                        path = System.Configuration.ConfigurationSettings.AppSettings["RootPath"]
                            + System.Configuration.ConfigurationSettings.AppSettings["InsuranceOnExpiryTemplatePath"];

                        template = System.IO.File.ReadAllText(path);
                    }
                    else if (placeholder.Category == TemplateCategory.PriorToExpiry)
                    {
                        path = System.Configuration.ConfigurationSettings.AppSettings["RootPath"]
                            + System.Configuration.ConfigurationSettings.AppSettings["InsurancePriorToTemplatePath"];

                        template = System.IO.File.ReadAllText(path);
                        template = template.Replace("{NumberOfDays}", placeholder.NumberOfDays.ToString());
                    }
                    else
                    {
                        path = System.Configuration.ConfigurationSettings.AppSettings["RootPath"]
                           + System.Configuration.ConfigurationSettings.AppSettings["InsuranceAfterExpiryTemplatePath"];

                        template = System.IO.File.ReadAllText(path);
                        template = template.Replace("{NumberOfDays}", placeholder.NumberOfDays.ToString());
                    }

                    template = template.Replace("{UserName}", placeholder.UserName);
                    template = template.Replace("{DocumentName}", placeholder.DocumentName);
                }
                else if(placeholder.TemplateType == TemplateType.Probation)
                {
                    if (placeholder.Category == TemplateCategory.OnExpiry)
                    {
                        path = System.Configuration.ConfigurationSettings.AppSettings["RootPath"]
                           + System.Configuration.ConfigurationSettings.AppSettings["ProbationOnExpiryTemplatePath"];

                        template = System.IO.File.ReadAllText(path);
                    }
                    else if (placeholder.Category == TemplateCategory.PriorToExpiry)
                    {
                        path = System.Configuration.ConfigurationSettings.AppSettings["RootPath"]
                           + System.Configuration.ConfigurationSettings.AppSettings["ProbationPriorToTemplatePath"];

                        template = System.IO.File.ReadAllText(path);
                        template = template.Replace("{NumberOfDays}", placeholder.NumberOfDays.ToString());
                    }
                    else
                    {
                        path = System.Configuration.ConfigurationSettings.AppSettings["RootPath"]
                           + System.Configuration.ConfigurationSettings.AppSettings["ProbationAfterExpiryTemplatePath"];

                        template = System.IO.File.ReadAllText(path);
                        template = template.Replace("{NumberOfDays}", placeholder.NumberOfDays.ToString());
                    }

                    template = template.Replace("{UserName}", placeholder.UserName);
                    template = template.Replace("{EmployeeName}", placeholder.EmployeeName);
                }
                else
                {
                    if (placeholder.Category == TemplateCategory.OnExpiry)
                    {
                        path = System.Configuration.ConfigurationSettings.AppSettings["RootPath"]
                           + System.Configuration.ConfigurationSettings.AppSettings["GeneralOnExpiryTemplatePath"];

                        template = System.IO.File.ReadAllText(path);
                    }
                    else if (placeholder.Category == TemplateCategory.PriorToExpiry)
                    {
                        path = System.Configuration.ConfigurationSettings.AppSettings["RootPath"]
                           + System.Configuration.ConfigurationSettings.AppSettings["GeneralPriorToTemplatePath"];

                        template = System.IO.File.ReadAllText(path);
                        template = template.Replace("{NumberOfDays}", placeholder.NumberOfDays.ToString());
                    }
                    else
                    {
                        path = System.Configuration.ConfigurationSettings.AppSettings["RootPath"]
                           + System.Configuration.ConfigurationSettings.AppSettings["GeneralAfterExpiryTemplatePath"];

                        template = System.IO.File.ReadAllText(path);
                        template = template.Replace("{NumberOfDays}", placeholder.NumberOfDays.ToString());
                    }

                    template = template.Replace("{UserName}", placeholder.UserName);
                    template = template.Replace("{DocumentName}", placeholder.DocumentName);
                }

                string subject = System.Configuration.ConfigurationSettings.AppSettings["MailSubject"];


                MailSend objAcc;
                objAcc = new MailSend { Body = template, ToAddress = new string[] { to }, CCAddress = admins.Select(s => s.EmailAddress).ToArray(), Subject = subject };

                Email objEmail = new Email(objAcc);

                objEmail.SendEmail();

                //string mailId = System.Configuration.ConfigurationSettings.AppSettings["MailUserMailId"];
                //string mailPassword = System.Configuration.ConfigurationSettings.AppSettings["MailUserMailPassword"];
                //string subject = System.Configuration.ConfigurationSettings.AppSettings["MailSubject"];

                //MailMessage mail = new MailMessage();
                //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                //mail.From = new MailAddress(mailId);
                //mail.To.Add(to);
                //foreach (var item in admins)
                //{
                //    mail.CC.Add(new MailAddress(item.EmailAddress));
                //}
                //mail.Subject = subject;
                //mail.IsBodyHtml = true;
                //mail.Body = template;
                //SmtpServer.Port = 587;
                //SmtpServer.Credentials = new System.Net.NetworkCredential(mailId, mailPassword);
                //SmtpServer.EnableSsl = true;
                //Console.WriteLine("Sending mail..");
                //SmtpServer.Send(mail);
                //Console.WriteLine("Mail Sent");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //static void Main(string[] args)
        //{
        //    try
        //    {
        //        DateTime minDate = new DateTime(2001, 1, 1), maxDate = new DateTime(2001, 1, 1);
        //        DateTime todaysDate = DateTime.Today;


        //        var insurances = db.Insurances.Where(m => m.Status).Include("Property").Include("Property.FilledTemplateDetail");

        //        //var superAdmins = db.UserProfiles.Where(m => m.RoleId == 1).ToList();

        //        foreach (var item in insurances)
        //        {
        //            try
        //            {
        //                var orgId = item.Property.FilledTemplateDetail.OrgId;

        //                var recurrsDetails = db.RecursiveNotificationDetails.FirstOrDefault(m => m.OrgId == orgId);

        //                if (recurrsDetails != null)
        //                {
        //                    if (recurrsDetails.RecurrsBeforeDays != null)
        //                    {
        //                        minDate = item.DateOfExpiry.AddDays(-recurrsDetails.RecurrsBeforeDays.Value);
        //                    }
        //                    else
        //                    {
        //                        minDate = item.DateOfExpiry;
        //                    }

        //                    if (recurrsDetails.RecurrsAfterDays != null)
        //                    {
        //                        maxDate = item.DateOfExpiry.AddDays(recurrsDetails.RecurrsAfterDays.Value);
        //                    }
        //                    else
        //                    {
        //                        maxDate = item.DateOfExpiry;
        //                    }
        //                }
        //                else
        //                {
        //                    minDate = item.DateOfExpiry;
        //                    maxDate = item.DateOfExpiry;
        //                }

        //                if (minDate.Date <= todaysDate.Date && maxDate.Date >= todaysDate.Date)
        //                {
        //                    TemplateCategory category;
        //                    if (item.DateOfExpiry == DateTime.Today.Date)
        //                        category = TemplateCategory.OnExpiry;
        //                    else if (item.DateOfExpiry >= DateTime.Today.Date)
        //                        category = TemplateCategory.PriorToExpiry;
        //                    else
        //                        category = TemplateCategory.AfterExpiry;

        //                    var user = db.UserProfiles.FirstOrDefault(f => f.UserID == item.UserId);
        //                    var userDetails = db.UserAddressDetails.FirstOrDefault(f => f.UserId == item.UserId);

        //                    Placeholder placeholder = new Placeholder
        //                    {
        //                        UserName = userDetails.FirstName,
        //                        DocumentName = db.DocumentTemplates.FirstOrDefault(m => m.TemplateId == item.Property.FilledTemplateDetail.TemplateId).DocumentTitle,
        //                        NumberOfDays = Math.Abs(Convert.ToInt32((item.DateOfExpiry - DateTime.Today).TotalDays)),
        //                        Category = category,
        //                        TemplateType = TemplateType.Insurance
        //                    };

        //                    SendMail(user.EmailAddress, placeholder, orgId.Value, user.Department);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Log("InsuranceId: " + item.Id + "\nError: " + ex.Message);
        //            }
        //        }

        //        var probations = db.ProbationDetails.Where(m => m.Status).Include("CustomerDetail").Include("UserProfile").Include("UserProfile.UserAddressDetails");

        //        foreach (var item in probations)
        //        {
        //            try
        //            {
        //                var orgId = item.CustomerDetail.OrganizationId;

        //                var recurrsDetails = db.RecursiveNotificationDetails.FirstOrDefault(m => m.OrgId == orgId);

        //                if (recurrsDetails != null)
        //                {
        //                    if (recurrsDetails.RecurrsBeforeDays != null)
        //                    {
        //                        minDate = item.DateOfExpiry.AddDays(-recurrsDetails.RecurrsBeforeDays.Value);
        //                    }
        //                    else
        //                    {
        //                        minDate = item.DateOfExpiry;
        //                    }

        //                    if (recurrsDetails.RecurrsAfterDays != null)
        //                    {
        //                        maxDate = item.DateOfExpiry.AddDays(recurrsDetails.RecurrsAfterDays.Value);
        //                    }
        //                    else
        //                    {
        //                        maxDate = item.DateOfExpiry;
        //                    }
        //                }
        //                else
        //                {
        //                    minDate = item.DateOfExpiry;
        //                    maxDate = item.DateOfExpiry;
        //                }

        //                if (minDate.Date <= todaysDate.Date && maxDate.Date >= todaysDate.Date)
        //                {
        //                    TemplateCategory category;
        //                    if (item.DateOfExpiry == DateTime.Today.Date)
        //                        category = TemplateCategory.OnExpiry;
        //                    else if (item.DateOfExpiry >= DateTime.Today.Date)
        //                        category = TemplateCategory.PriorToExpiry;
        //                    else
        //                        category = TemplateCategory.AfterExpiry;

        //                    //var user = db.UserProfiles.FirstOrDefault(f => f.UserID == item.UserId);
        //                    //var userDetails = db.UserAddressDetails.FirstOrDefault(f => f.UserId == item.UserId);

        //                    Placeholder placeholder = new Placeholder
        //                    {
        //                        UserName = item.UserProfile.UserAddressDetails.FirstOrDefault().FirstName,
        //                        EmployeeName = item.CustomerDetail.CustomerName,
        //                        NumberOfDays = Math.Abs(Convert.ToInt32((item.DateOfExpiry - DateTime.Today).TotalDays)),
        //                        Category = category,
        //                        TemplateType = TemplateType.Probation
        //                    };

        //                    SendMail(item.UserProfile.EmailAddress, placeholder, orgId, item.UserProfile.Department);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Log("InsuranceId: " + item.Id + "\nError: " + ex.Message);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(ex.Message);
        //    }
        //}
    }
}
