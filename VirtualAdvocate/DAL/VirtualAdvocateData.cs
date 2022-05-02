#region NameSpaces
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VirtualAdvocate.Common;
using VirtualAdvocate.Models;
#endregion
#region VirtualAdvocate.DAL
namespace VirtualAdvocate.DAL
{
    #region VirtualAdvocateData
    public class VirtualAdvocateData
    {
        #region Account Services and Payment Methods
        /// <summary>
        /// Getting default services from virtual advocate
        /// </summary>
        /// <returns>Entire list of services</returns>
        //public List<AccountService> GetAccountServices()
        //{
        //    VirtualAdvocateEntities objContext = new VirtualAdvocateEntities();
        //    return objContext.AccountServices.Where(m => m.IsEnabled == true).ToList<AccountService>();
        //}

        /// <summary>
        /// Getting selected services from virtual advocate
        /// </summary>
        /// <returns>selected list of services</returns>
        //public List<Selecte> GetSelectedAccountServices(int UserId)
        //{
        //    VirtualAdvocateEntities objContext = new VirtualAdvocateEntities();
        //    return objContext.SelectedAccountServices.Where(m => m.UserId == UserId).ToList<SelectedAccountService>();
        //}

        /// <summary>
        /// Getting default payment methods from virtual advocate
        /// </summary>
        /// <returns></returns>
        public List<PaymentMethod> GetPaymentMethod()
        {
            VirtualAdvocateEntities objContext = new VirtualAdvocateEntities();
            return objContext.PaymentMethods.Where(m => m.Enabled == true).ToList<PaymentMethod>();
        }

        /// <summary>
        /// Getting selected payment methods from virtual advocate
        /// </summary>
        /// <returns></returns>
        //public List<SelectedPaymentMethod> GetSelectedPaymentMethod(int UserId)
        //{
        //    VirtualAdvocateEntities objContext = new VirtualAdvocateEntities();
        //    return objContext.SelectedPaymentMethods.Where(m => m.UserId == UserId).ToList<SelectedPaymentMethod>();
        //}

        /// <summary>
        /// Save Selected payment methods
        /// </summary>
        /// <param name="objPaymentMethod"></param>
        /// <returns></returns>
        public int SaveSelectedPaymentMethod(SelectedPaymentMethod objPaymentMethod)
        {
            int result = int.MinValue;
            if (objPaymentMethod != null)
            {
                using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
                {
                    objContext.SelectedPaymentMethods.Add(objPaymentMethod);
                    objContext.SaveChanges();
                }
            }
            return result;
        }

        ///// <summary>
        ///// Save Selected Account Services
        ///// </summary>
        ///// <param name="objServices"></param>
        ///// <returns></returns>
        //public int SaveSelectedAccountServices(SelectedAccountService objServices)
        //{
        //    int result = int.MinValue;
        //    if (objServices != null)
        //    {
        //        using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
        //        {
        //            objContext.SelectedAccountServices.Add(objServices);
        //            objContext.SaveChanges();
        //        }
        //    }
        //    return result;
        //}

        /// <summary>
        /// Save Selected Account Services
        /// </summary>
        /// <param name="objServices"></param>
        /// <returns></returns>
        public int SaveSelectedDepartment(SelectedDepartment objDepartment)
        {
            int result = int.MinValue;
            if (objDepartment != null)
            {
                using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
                {
                    objContext.SelectedDepartments.Add(objDepartment);
                    objContext.SaveChanges();
                }
            }
            return result;
        }


        //public int DeleteSelectedAccountServices(int? id)
        //{
        //    int result = int.MinValue;
        //    if (id != null)
        //    {
        //        using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
        //        {
        //            var objDeleteService = (from delser in objContext.SelectedAccountServices.Where(x => x.UserId == id)
        //                                    select delser
        //                                  );
        //            foreach(var del in objDeleteService)
        //            {
        //                objContext.SelectedAccountServices.Remove(del);                       
        //            }
        //            objContext.SaveChanges();
        //        }
        //    }
        //    return result;
        //}

        public int DeleteSelectedPaymentMethods(int? id)
        {
            int result = int.MinValue;
            if (id != null)
            {
                using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
                {
                    var objDeleteService = (from delser in objContext.SelectedPaymentMethods.Where(x => x.UserId == id)
                                            select delser
                                          );
                    foreach (var del in objDeleteService)
                    {
                        objContext.SelectedPaymentMethods.Remove(del);
                    }
                    objContext.SaveChanges();
                }
            }
            return result;
        }
        #endregion

        #region User Management
        /// <summary>
        /// Inserting user information
        /// </summary>
        /// <param name="objUserProfile"></param>
        /// <returns>User Id</returns>
        public int SaveUserProfile(UserProfile objUserProfile)
        {
            int newUserid = 0;
            if (objUserProfile != null)
            {
                using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
                {
                    objContext.UserProfiles.Add(objUserProfile);
                    objContext.SaveChanges();
                    newUserid = objUserProfile.UserID;
                }
            }
            return newUserid;
        }

        /// <summary>
        /// inserting Organization details
        /// </summary>
        /// <param name="objOrgDetails"></param>
        /// <returns>Organization Id</returns>
        public int SaveOrganizationDetails(OrganizationDetail objOrgDetails)
        {
            int newOrgId = 0;
            if (objOrgDetails != null)
            {
                using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
                {
                    objContext.OrganizationDetails.Add(objOrgDetails);
                    objContext.SaveChanges();
                    newOrgId = objOrgDetails.OrganizationId;
                }
            }
            return newOrgId;
        }

        /// <summary>
        /// inserting address details
        /// </summary>
        /// <param name="objUserAddressDetail"></param>
        /// <returns></returns>
        public int SaveUserPersonalDetails(UserAddressDetail objUserAddressDetail)
        {
            int result = int.MinValue;
            if (objUserAddressDetail != null)
            {
                using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
                {
                    objContext.UserAddressDetails.Add(objUserAddressDetail);
                    objContext.SaveChanges();
                }
            }
            return result;
        }

        /// <summary>
        /// Loading default values for user Registration
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UserRegistrationModel getDefaultRegistration(int? id)
        {

            UserRegistrationModel obj = new UserRegistrationModel();

            obj.AvailablePaymentMethods = ServicesRepository.GetAllPaymentMethods().ToList(); // Getting all payment methods         
                                                                                              // obj.AvailableService = ServicesRepository.GetAllAccountServices().ToList();   // Getting all account services 

            var selectedAccServices = new List<AccountServicesModel>();
            var selectedPayMethods = new List<PaymentMethodModel>();
            obj.SelectedPaymentMethods = selectedPayMethods;
            int serveId = 0;
            if (id != null)
            {
                serveId = id.Value;
                // selectedAccServices.Add(ServicesRepository.GetDefaultService(serveId));
            }
            obj.SelectedService = selectedAccServices;



            return obj;
        }

        //public OrgUserViewModel getDefaultValues(int? Orgid)
        //{

        //    OrgUserViewModel obj = new OrgUserViewModel();
        //    //obj.AvailablePaymentMethods = ServicesRepository.GetAllPaymentMethods().ToList(); // Getting all payment methods         
        //    obj.AvailableService = ServicesRepository.GetAllAccountServices().ToList();   // Getting all account services 

        //    var selectedAccServices = new List<AccountServicesModel>();
        //    //var selectedPayMethods = new List<PaymentMethodModel>();
        //    //obj.SelectedPaymentMethods = selectedPayMethods;
        //    obj.SelectedService = selectedAccServices;

        //    return obj;
        //}

        /// <summary>
        /// Getting services which are permitted for a company
        /// </summary>
        /// <param name="Orgid"></param>
        /// <returns></returns>
        public OrgUserViewModel getOrgServicesForNewUser(int? Orgid)
        {

            OrgUserViewModel obj = new OrgUserViewModel();
            //var selectedAccServices = new List<AccountServicesModel>();
            //obj.SelectedService = selectedAccServices;
            //List<AccountServicesModel> objAvail = new List<AccountServicesModel>();
            //objAvail = getCompanyServices(Orgid);
            //obj.AvailableService = objAvail;
            return obj;
        }

        /// <summary>
        /// Getting Account Services based on organization
        /// </summary>
        /// <param name="Orgid"></param>
        /// <returns></returns>
        //public List<AccountServicesModel>  getCompanyServices(int? Orgid)
        //{
        //    List<AccountServicesModel> objAvail = new List<AccountServicesModel>();
        //    using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
        //    {
        //        OrganizationDetail objOrg = objContext.OrganizationDetails.Find(Orgid);
        //        var objASM = (from acser in objContext.AccountServices
        //                      join selser in objContext.SelectedAccountServices
        //                      on acser.ServiceId equals selser.ServiceId
        //                      where selser.UserId == objOrg.UserId
        //                      select acser
        //                                      );
        //        foreach (AccountService AC in objASM)
        //        {
        //            objAvail.Add(new AccountServicesModel { Id = AC.ServiceId, Name = AC.Service });
        //        }

        //    }
        //    return objAvail;
        //}

        /// <summary>
        /// User Selected Services
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        //public List<AccountServicesModel> getUserSelectedServices(int? userId)
        // {
        //     List<AccountServicesModel> objSelected = new List<AccountServicesModel>();
        //     using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
        //     {
        //         var objSelectedServices = (from acser in objContext.AccountServices
        //                                    join selser in objContext.SelectedAccountServices
        //                                    on acser.ServiceId equals selser.ServiceId
        //                                    where selser.UserId == userId &&  acser.IsEnabled==true
        //                                    select acser
        //                                 );
        //         foreach (var objservice in objSelectedServices)
        //         {
        //             objSelected.Add(ServicesRepository.GetDefaultService(objservice.ServiceId));
        //         }
        //     }
        //     return objSelected;
        // }

        /// <summary>
        /// Getting permitted services and change it according to the user
        /// </summary>
        /// <param name="Orgid"></param>
        /// <returns></returns>
        public AccountServiceModel getOrgServicesForEditUser(int? userId)
        {
            VirtualAdvocateEntities objContext = new VirtualAdvocateEntities();
            AccountServiceModel obj = new AccountServiceModel();
            var selectedAccServices = new List<AccountServicesModel>();
            List<AccountServicesModel> objAvail = new List<AccountServicesModel>();
            UserProfile objUp = objContext.UserProfiles.Find(userId);
            // objAvail = getCompanyServices(objUp.OrganizationId);
            obj.AvailableService = objAvail;
            // obj.SelectedService = getUserSelectedServices(userId);
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
            return obj;
        }

        //public AccountServiceModel getServices(int? id)
        //{
        //    AccountServiceModel obj = new AccountServiceModel();
        //    obj.AvailableService = ServicesRepository.GetAllAccountServices().ToList();   // Getting all account services 
        //    var selectedAccServices = new List<AccountServicesModel>();
        //    int userId = 0;
        //    if (id != null)
        //    {
        //        userId = id.Value;
        //    }
        //        using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
        //            {
        //        var objSelectedServices = (from acser in objContext.AccountServices
        //                                   join selser in objContext.SelectedAccountServices
        //                                   on acser.ServiceId equals selser.ServiceId
        //                                   where selser.UserId == userId
        //                                   select acser
        //                                   );
        //                foreach(var objservice in objSelectedServices)
        //                {
        //                    selectedAccServices.Add(ServicesRepository.GetDefaultService(objservice.ServiceId));
        //                }
        //                obj.SelectedService = selectedAccServices;
        //            }


        //    return obj;
        //}
        public EditPaymentMethodeModel getPayments(int? id)
        {
            EditPaymentMethodeModel obj = new EditPaymentMethodeModel();
            obj.AvailablePaymentMethods = ServicesRepository.GetAllPaymentMethods().ToList();   // Getting all account services 
            var selectedPaymentServices = new List<PaymentMethodModel>();
            int userId = 0;
            if (id != null)
            {
                userId = id.Value;
            }
            using (VirtualAdvocateEntities objContext = new VirtualAdvocateEntities())
            {
                var objSelectedPayment = (from acser in objContext.PaymentMethods
                                          join selser in objContext.SelectedPaymentMethods
                                          on acser.PaymentTypeId equals selser.PaymentTypeId
                                          where selser.UserId == userId
                                          select acser
                                           );
                foreach (var objpay in objSelectedPayment)
                {
                    selectedPaymentServices.Add(ServicesRepository.GetPay(objpay.PaymentTypeId));
                }
                obj.SelectedPaymentMethods = selectedPaymentServices;
            }
            return obj;
        }
        /// <summary>
        /// Save Personal Details after changes
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int EditPersonalDetails(PersonalDetailsViewModel objUser)
        {
            try
            {
                using (VirtualAdvocateEntities context = new VirtualAdvocateEntities())
                {
                    var obj = context.UserAddressDetails.Where(ua => ua.UserId == objUser.UserId).FirstOrDefault();
                    var objMain = context.UserProfiles.Where(ua => ua.UserID == objUser.UserId).FirstOrDefault();
                    if (objUser.Department != 0)
                        objMain.Department = objUser.Department;
                    if (objUser.roleID > 0)
                        objMain.RoleId = objUser.roleID;

                    if (objUser != null)
                    {
                        obj.FirstName = objUser.FirstName;
                        obj.LastName = objUser.LastName;
                        // obj.EmailAddress = objUser.EmailAddress;
                        obj.StreetName = objUser.StreetName;
                        obj.BuildingName = objUser.BuildingName;
                        obj.BlockNumber = objUser.BlockNo;
                        obj.PlotNumber = objUser.PlotNo;
                        obj.Region = objUser.Region;
                        obj.LandMark = objUser.LandMark;
                        obj.Designation = objUser.Designation;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                return 1;
            }
            return 0;
        }
        public int LogDueDiligenceUsers(DueDiligenceUserViewModel objUserRegistration, int userId)
        {
            int logId = 0;
            try
            {
                using (VirtualAdvocateEntities context = new VirtualAdvocateEntities())
                {
                    LogDueDiligenceUser objLog = new LogDueDiligenceUser();
                    objLog.UserId = userId;
                    objLog.FirstName = objUserRegistration.FirstName;
                    objLog.LastName = objUserRegistration.LastName;
                    objLog.EmailAddress = objUserRegistration.EmailAddress;
                    objLog.PhoneNumber = objUserRegistration.PhoneNumber;
                    objLog.StreetName = objUserRegistration.StreetName;
                    objLog.PlotNumber = objUserRegistration.PlotNo;
                    objLog.Region = objUserRegistration.Region;
                    objLog.BuildingName = objUserRegistration.BuildingName;
                    objLog.LandMark = objUserRegistration.LandMark;
                    objLog.BlockNumber = objUserRegistration.BlockNo;
                    objLog.ModifierId = userId;
                    objLog.RoleId = 4;
                    objLog.ModifiedDate = DateTime.Now;
                    context.LogDueDiligenceUsers.Add(objLog);
                    context.SaveChanges();
                    logId = objLog.LogId;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return logId;
        }
        public void LogDuePersonalDetails(PersonalDetailsViewModel objUserRegistration, int Modifier)
        {

            try
            {
                using (VirtualAdvocateEntities context = new VirtualAdvocateEntities())
                {
                    var obj = context.UserAddressDetails.Where(ua => ua.UserId == objUserRegistration.UserId).FirstOrDefault();
                    LogDueDiligenceUser objLog = new LogDueDiligenceUser();
                    objLog.UserId = objUserRegistration.UserId;
                    objLog.FirstName = objUserRegistration.FirstName;
                    objLog.LastName = objUserRegistration.LastName;
                    objLog.EmailAddress = objUserRegistration.EmailAddress;
                    objLog.PhoneNumber = obj.PhoneNumber;
                    objLog.StreetName = objUserRegistration.StreetName;
                    objLog.PlotNumber = objUserRegistration.PlotNo;
                    objLog.Region = objUserRegistration.Region;
                    objLog.BuildingName = objUserRegistration.BuildingName;
                    objLog.LandMark = objUserRegistration.LandMark;
                    objLog.BlockNumber = objUserRegistration.BlockNo;
                    objLog.ModifierId = Modifier;
                    objLog.RoleId = 4;
                    objLog.Action = "Update";
                    objLog.ModifiedDate = DateTime.Now;
                    context.LogDueDiligenceUsers.Add(objLog);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
        }
        public void LogPersonalDetails(PersonalDetailsViewModel objUserRegistration, int Modifier, int RoleId, int OrgId)
        {

            try
            {
                using (VirtualAdvocateEntities context = new VirtualAdvocateEntities())
                {
                    var obj = context.UserAddressDetails.Where(ua => ua.UserId == objUserRegistration.UserId).FirstOrDefault();
                    LogRegistration objLog = new LogRegistration();
                    objLog.UserId = objUserRegistration.UserId;
                    objLog.FirstName = objUserRegistration.FirstName;
                    objLog.LastName = objUserRegistration.LastName;
                    objLog.EmailAddress = objUserRegistration.EmailAddress;
                    objLog.Designation = objUserRegistration.Designation;
                    objLog.PhoneNumber = obj.PhoneNumber;
                    objLog.StreetName = objUserRegistration.StreetName;
                    objLog.PlotNumber = objUserRegistration.PlotNo;
                    objLog.Region = objUserRegistration.Region;
                    objLog.BuildingName = objUserRegistration.BuildingName;
                    objLog.LandMark = objUserRegistration.LandMark;
                    objLog.BlockNumber = objUserRegistration.BlockNo;
                    objLog.ModifierId = Modifier;
                    objLog.RoleId = RoleId;
                    objLog.OrgId = OrgId;
                    objLog.Action = "Update";
                    objLog.IsEnabled = true;
                    objLog.ModifiedDate = DateTime.Now;
                    context.LogRegistrations.Add(objLog);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
        }
        #endregion

        #region Organization

        /// <summary>
        /// Get all the organization types
        /// </summary>
        /// <returns></returns>
        public List<OrganizationType> GetOrganizationTypes()
        {
            VirtualAdvocateEntities objContext = new VirtualAdvocateEntities();
            return objContext.OrganizationTypes.Where(m => m.IsEnabled == true).ToList<OrganizationType>();
        }

        /// <summary>
        /// Getting organization details based on the Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OrganizationDetail getOrganizationDetails(int? id)
        {
            OrganizationDetail objOrg = new OrganizationDetail();
            try
            {
                using (VirtualAdvocateEntities context = new VirtualAdvocateEntities())
                {
                    objOrg = context.OrganizationDetails.Where(ua => ua.OrganizationId == id).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                //return 1;
            }
            return objOrg;
        }

        /// <summary>
        /// Binding all organization types into ListItem
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getOrganizationTypesOptionsList()
        {
            List<OrganizationType> objCat = new List<OrganizationType>();
            objCat = GetOrganizationTypes();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (OrganizationType dsc in objCat)
            {
                list.Add(new OptionsModel { ID = dsc.OrganizationTypeId, Name = dsc.OrganizationType1 });
            }
            return list;
        }

        #endregion

        #region Due Diligence Form submit
        /// <summary>
        /// Binding all organization types into ListItem
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getAllEnquiryType()
        {
            List<DueDiligenceEnquiryType> objDue = new List<DueDiligenceEnquiryType>();
            objDue = GetDueDiligenceEnquiryTypes();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (DueDiligenceEnquiryType dsc in objDue)
            {
                list.Add(new OptionsModel { ID = dsc.EnquiryTypeId, Name = dsc.EnquiryType });
            }
            return list;
        }

        public List<DueDiligenceEnquiryType> GetDueDiligenceEnquiryTypes()
        {
            List<DueDiligenceEnquiryType> obj = new List<DueDiligenceEnquiryType>();
            try
            {

                VirtualAdvocateEntities context = new VirtualAdvocateEntities(); ;
                obj = context.DueDiligenceEnquiryTypes.Where(x => x.IsEnabled == true).ToList<DueDiligenceEnquiryType>();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return obj;
        }
        #endregion

        #region Reports
        public List<OptionsModel> getAllReportsType()
        {
            List<ReportType> objReport = new List<ReportType>();
            objReport = GetReportsType();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (ReportType dsc in objReport)
            {
                list.Add(new OptionsModel { ID = dsc.ReportTypeId, Name = dsc.ReportTypeName });
            }
            return list;
        }

        public List<ReportType> GetReportsType()
        {
            List<ReportType> obj = new List<ReportType>();
            try
            {

                VirtualAdvocateEntities context = new VirtualAdvocateEntities(); ;
                obj = context.ReportTypes.Where(x => x.IsEnabled == true).ToList<ReportType>();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return obj;
        }

        public List<OptionsModel> getAllCompany()
        {
            List<OrganizationDetail> objReport = new List<OrganizationDetail>();
            objReport = GetMultiUserCompanyList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (OrganizationDetail dsc in objReport)
            {
                list.Add(new OptionsModel { ID = dsc.OrganizationId, Name = dsc.OrgName });
            }
            return list;
        }

        public List<OrganizationDetail> GetMultiUserCompanyList() // only multiple user company
        {
            List<OrganizationDetail> obj = new List<OrganizationDetail>();
            try
            {

                VirtualAdvocateEntities context = new VirtualAdvocateEntities();
                obj = context.OrganizationDetails.Where(x => x.IsEnabled == true && x.UserAccountsType == 2).ToList<OrganizationDetail>();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return obj;
        }

        public List<OptionsModel> getSingleUserCompanyList()
        {
            List<OrganizationDetail> objReport = new List<OrganizationDetail>();
            objReport = GetSingleUserCompanyList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (OrganizationDetail dsc in objReport)
            {
                list.Add(new OptionsModel { ID = dsc.OrganizationId, Name = dsc.OrgName });
            }
            return list;
        }

        public List<OrganizationDetail> GetSingleUserCompanyList() // only single user company
        {
            List<OrganizationDetail> obj = new List<OrganizationDetail>();
            try
            {

                VirtualAdvocateEntities context = new VirtualAdvocateEntities();
                obj = context.OrganizationDetails.Where(x => x.IsEnabled == true && x.UserAccountsType == 1).ToList<OrganizationDetail>();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return obj;
        }

        public List<OptionsModel> getAllDocumentType()
        {
            List<DocumentTemplate> objReport = new List<DocumentTemplate>();
            objReport = GetDocumentTypes();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (DocumentTemplate dsc in objReport)
            {
                list.Add(new OptionsModel { ID = dsc.TemplateId, Name = dsc.DocumentType });
            }
            return list;
        }

        public List<DocumentTemplate> GetDocumentTypes()
        {
            List<DocumentTemplate> obj = new List<DocumentTemplate>();
            try
            {

                VirtualAdvocateEntities context = new VirtualAdvocateEntities();
                obj = context.DocumentTemplates.Where(x => x.IsEnabled == true).ToList<DocumentTemplate>();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
            return obj;
        }



        #endregion

        #region Log
        public int LogRegistrations(LogRegistration obj)
        {
            int logid = 0;
            try
            {
                VirtualAdvocateEntities context = new VirtualAdvocateEntities();
                context.LogRegistrations.Add(obj);
                context.SaveChanges();
                logid = obj.LogId;
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            return logid;
        }

        public void LogDueRegistrations(int userid, bool IsEnabled, int Modifier)
        {
            try
            {
                VirtualAdvocateEntities db = new VirtualAdvocateEntities();
                LogDueDiligenceUser objLog = new LogDueDiligenceUser();
                var obj = db.UserProfiles.Where(ua => ua.UserID == userid).FirstOrDefault();
                var objUserRegistration = db.UserAddressDetails.Where(ua => ua.UserId == userid).FirstOrDefault();

                // Assign Values for Log
                objLog.UserId = userid;
                objLog.FirstName = objUserRegistration.FirstName;
                objLog.LastName = objUserRegistration.LastName;
                objLog.EmailAddress = objUserRegistration.EmailAddress;
                objLog.PhoneNumber = objUserRegistration.PhoneNumber;
                objLog.StreetName = objUserRegistration.StreetName;
                objLog.PlotNumber = objUserRegistration.PlotNumber;
                objLog.Region = objUserRegistration.Region;
                objLog.BuildingName = objUserRegistration.BuildingName;
                objLog.LandMark = objUserRegistration.LandMark;
                objLog.BlockNumber = objUserRegistration.BlockNumber;
                objLog.ModifierId = Modifier;
                objLog.RoleId = obj.RoleId;
                objLog.ModifiedDate = DateTime.Now;
                objLog.IsEnabled = IsEnabled;
                if (IsEnabled == true)
                {
                    objLog.Action = "Active";
                }
                else
                {
                    objLog.Action = "Inactive";
                }
                db.LogDueDiligenceUsers.Add(objLog);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
        }

        public void LogDuePaymentType(int logid, int userid, int paymenttypeid)
        {
            try
            {
                LogDueUserPaymentType obj = new LogDueUserPaymentType();
                obj.LogId = logid;
                obj.PaymentTypeId = paymenttypeid;
                obj.UserId = userid;
                VirtualAdvocateEntities context = new VirtualAdvocateEntities();
                context.LogDueUserPaymentTypes.Add(obj);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
        }

        public void LogUserPaymentType(int logid, int userid, int paymenttypeid)
        {
            try
            {
                LogUserPaymentType obj = new LogUserPaymentType();
                obj.LogId = logid;
                obj.PaymentTypeId = paymenttypeid;
                obj.UserId = userid;
                VirtualAdvocateEntities context = new VirtualAdvocateEntities();
                context.LogUserPaymentTypes.Add(obj);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }
        }

        //public void LogAccountServices(int logid, int userid, int serviceid)
        //{
        //    try
        //    {
        //        LogUserService obj = new LogUserService();
        //        obj.LogId = logid;
        //        obj.ServiceId = serviceid;
        //        obj.UserId = userid;
        //        VirtualAdvocateEntities context = new VirtualAdvocateEntities();
        //        context.LogUserServices.Add(obj);
        //        context.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.LogThisError(ex);
        //    }
        //}
        #endregion
    } 
    #endregion
} 
#endregion