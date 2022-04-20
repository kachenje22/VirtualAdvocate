using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtualAdvocate.Models;
namespace VirtualAdvocate.DAL
{
    public class VirtualAdvocateDocumentData
    {
        private VirtualAdvocateEntities db = new VirtualAdvocateEntities();

        public List<DocumentCategory> GetDocumentCategories()
        {
            return db.DocumentCategories.ToList<DocumentCategory>();
        }

        public List<DocumentSubCategory> GetDocumentSubCategories()
        {
            return db.DocumentSubCategories.ToList<DocumentSubCategory>();
        }

        public List<DocumentSubSubCategory> GetDocumentSubSubCategories(int? id)
        {
            return db.DocumentSubSubCategories.Where(m=>m.DocumentSubCategoryId==id).ToList<DocumentSubSubCategory>();
        }

        /// <summary>
        /// Getting Category List for binding dropdownlist - While adding sub category
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getCategoryOptionsList()
        {           
            List<DocumentCategory> objCat = new List<DocumentCategory>();
            objCat = GetDocumentCategories().Where(m => m.IsEnabled == true).ToList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (DocumentCategory dsc in objCat)
            {
                list.Add(new OptionsModel { ID = dsc.DocumentCategoryId, Name = dsc.DocumentCategoryName });
            }            
            return list;
        }


       

        /// <summary>
        /// Getting Roles List for binding dropdownlist - While adding sub category
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getUserRolesOptionsList()
        {
            List<Role> objRole = new List<Role>();
            objRole = db.Roles.Where(r=>r.RoleId==2|| r.RoleId == 5|| r.RoleId == 6) .ToList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (Role dsc in objRole)
            {
                list.Add(new OptionsModel { ID = dsc.RoleId, Name = dsc.RoleDescription });
            }
            return list;
        }


        /// <summary>
        /// Getting Category List for binding dropdownlist - While adding sub category
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getDepartmentOptionsList(int Org)
        {
            List<DepartmentModel> objCat = new List<DepartmentModel>();
            objCat = (from d in db.Departments join s in db.SelectedDepartments on d.Id equals s.DepartmentID  where s.OrgID==Org && d.IsEnabled==true select new DepartmentModel { Department=d.Name,Id=d.Id}).ToList() ;
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (DepartmentModel dsc in objCat)
            {
                list.Add(new OptionsModel { ID = dsc.Id, Name = dsc.Department });
            }
            return list;
        }


        public List<OptionsModel> getRoles(int roleID)
        {
            List<OptionsModel> list = new List<OptionsModel>();
            List<RoleModel> objRole = new List<RoleModel>();

            if (roleID == 1)
            {
                objRole = (from d in db.Roles where (d.RoleId != 1 && d.RoleId!=3) select new RoleModel { Role = d.RoleDescription, Id = d.RoleId }).ToList();
            }
            else if (roleID == 2)
            {
                objRole = (from d in db.Roles where (d.RoleId == 5 || d.RoleId == 6) select new RoleModel { Role = d.RoleDescription, Id = d.RoleId }).ToList();
            }
            else if (roleID == 6)
            {
                objRole = (from d in db.Roles where (d.RoleId == 5 ) select new RoleModel { Role = d.RoleDescription, Id = d.RoleId }).ToList();
            }
            foreach (RoleModel dsc in objRole)
            {
                list.Add(new OptionsModel { ID = dsc.Id, Name = dsc.Role });
            }
            return list;
        }

        /// <summary>
        /// Getting Category List for binding dropdownlist - While adding sub category
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getDepartmentOptionsList()
        {
            List<DepartmentModel> objCat = new List<DepartmentModel>();
            objCat = (from d in db.Departments where d.IsEnabled == true select new DepartmentModel { Department = d.Name, Id = d.Id }).ToList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (DepartmentModel dsc in objCat)
            {
                list.Add(new OptionsModel { ID = dsc.Id, Name = dsc.Department });
            }
            return list;
        }
        /// <summary>
        /// Getting Sub Category List for binding dropdownlist - While adding sub sub category
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getSubCategoryOptionsList(int? id)
        {
            List<DocumentSubCategory> objCat = new List<DocumentSubCategory>();
            objCat = GetDocumentSubCategories().Where(m=>m.DocumentCategoryId==id.Value && m.IsEnabled==true).ToList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (DocumentSubCategory dsc in objCat)
            {
                list.Add(new OptionsModel { ID = dsc.DocumentSubCategoryId, Name = dsc.DocumentSubCategoryName });
            }
            return list;
        }

        /// <summary>
        /// Getting Sub Category List for binding dropdownlist - While adding sub sub category
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getSubSubCategoryOptionsList(int? id)
        {
            List<DocumentSubSubCategory> objCat = new List<DocumentSubSubCategory>();
            objCat = GetDocumentSubSubCategories(id).Where(m=>m.IsEnabled==true).ToList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (DocumentSubSubCategory dsc in objCat)
            {
                list.Add(new OptionsModel { ID = dsc.DocumentSubSubCategoryId, Name = dsc.SubDocumentCategoryName });
            }
            return list;
        }

        /// <summary>
        /// Getting template list
        /// </summary>
        /// <returns></returns>
        public List<DocumentTemplate> GetTemplateList()
        {
            return db.DocumentTemplates.ToList<DocumentTemplate>();
        }

        /// <summary>
        /// Getting Template List for binding dropdownlist - For Associate with others
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getTemplateList(int? id)
        {
            List<DocumentTemplate> objCat = new List<DocumentTemplate>();
            objCat = GetTemplateList().Where(m => m.DocumentCategory == id.Value && m.IsEnabled == true).ToList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (DocumentTemplate dsc in objCat)
            {
                list.Add(new OptionsModel { ID = dsc.TemplateId, Name = dsc.DocumentTitle });
            }
            return list;
        }

        public List<OptionsModel> getAllServices()
        {
            List<OrganizationDetail> objOrg = new List<OrganizationDetail>();
            objOrg = db.OrganizationDetails.Where(m => m.IsEnabled == true).ToList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (OrganizationDetail dsc in objOrg)
            {
                list.Add(new OptionsModel { ID = dsc.OrganizationId, Name = dsc.OrgName });
            }
            return list;
        }

        //public List<ServiceModel> getAllServicesforGrid()
        //{
        //    List<AccountService> objCat = new List<AccountService>();
        //    objCat = db.AccountServices.ToList();
        //    List<ServiceModel> list = new List<ServiceModel>();
        //    foreach (AccountService dsc in objCat)
        //    {
        //        list.Add(new ServiceModel { ID = dsc.ServiceId, Service = dsc.Service, ServiceDescription = dsc.ServicesDescription, IsEnabled = dsc.IsEnabled });
        //    }
        //    return list;
        //}


        //public List<OptionsModel> getTemplateList(int? id)
        //{
        //    List<DocumentTemplate> objCat = new List<DocumentTemplate>();
        //    objCat = GetTemplateList().Where(m => m.DocumentCategory == id.Value && m.IsEnabled == true).ToList();
        //    List<OptionsModel> list = new List<OptionsModel>();
        //    foreach (DocumentTemplate dsc in objCat)
        //    {
        //        list.Add(new OptionsModel { ID = dsc.TemplateId, Name = dsc.DocumentTitle });
        //        if (dsc.DocumentSubCategory != null)
        //        {

        //        }
        //    }
        //    return list;
        //}

        /// <summary>
        /// Create new template
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int AddTemplate(DocumentTemplate obj)
        {
            db.DocumentTemplates.Add(obj);
            db.SaveChanges();
            int result = obj.TemplateId;
            return result;
        }

        /// <summary>
        /// Edit Template
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int EditTemplate(DocumentTemplate obj)
        {
            db.SaveChanges();
            int result = int.MinValue;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyvalue"></param>
        /// <returns></returns>
        public TemplateKeyword getKeyFieldId(string keyvalue)
        {
            var obj = db.TemplateKeywords.Where(x => x.TemplateKeyValue == keyvalue && x.IsEnabled == true).FirstOrDefault(); // Get Key Details
           
            return obj;
        }

        public TemplateKeyword getKeyDetails(int keyId)
        {
            var obj = db.TemplateKeywords.Where(x => x.TemplateKeyId == keyId && x.IsEnabled == true ).FirstOrDefault(); // Get Key Details

            return obj;
        }

        public bool CheckTemplateKeyExist(int TemplateId, int keyId)
        {
            var objExist = db.TemplateKeysPointers.Where(m => m.TemplateKeyId ==keyId && m.TemplateId == TemplateId).FirstOrDefault(); // Check already key exists for this template
            if (objExist != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //public int saveKeyId(TemplateKeysPointer obj)
        //{
        //    db.TemplateKeysPointers.Add(obj);

        //}

        /// <summary>
        /// Super admin - get all invoice list
        /// </summary>
        /// <returns></returns>
        public List<InvoiceList_Result> GetAllInvoiceList()
        {
            return db.Get_InvoiceList().ToList();
        }

        public List<InvoiceListAcAdmin_sp_Result> getInvoiceListAcAdmin_sp(int? orgId, int? userId)
        {
            return db.InvoiceListAcAdmin_sp(orgId, userId).ToList<InvoiceListAcAdmin_sp_Result>();
        }

        public List<GenerateReport_Result> GenerateReportsByFilter(int RoleId,int? reportType, DateTime? fromDate, DateTime? toDate, int? orgId, int? UserId, int? documentTypeId,int? documentSubId, int? documentSubSubId,int?DepartmentID)
        {
         return db.GenerateReport(RoleId,reportType,  fromDate, toDate,  orgId, UserId, documentTypeId, documentSubId, documentSubSubId,DepartmentID).ToList<GenerateReport_Result>();
        }
        public List<GetorganizationUserList_sp_Result> GetorganizationUserList(int? orgId)
        {
            return db.GetorganizationUserList_sp(orgId).ToList<GetorganizationUserList_sp_Result>();
        }

        public List<OptionsModel> getUsersByOrganization(int? orgid)
        {
            List<GetorganizationUserList_sp_Result> objOrgUsers = new List<GetorganizationUserList_sp_Result>();
            objOrgUsers = GetorganizationUserList(orgid);
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (GetorganizationUserList_sp_Result dsc in objOrgUsers)
            {
                list.Add(new OptionsModel { ID = dsc.UserID, Name = dsc.Name });
            }
            return list;
        }

        public List<OptionsModel> getUsersByDepartment(int? dept,int org)
        {

            var objDepUsers = from u in db.UserProfiles
                              join ua in db.UserAddressDetails on u.UserID equals ua.UserId
                              where u.Department == dept && u.OrganizationId == org && u.RoleId!=1 && u.RoleId!=2 
                              select new UserManagementModel
                              {
                                  FirstName = ua.FirstName + " "+ ua.LastName,
                                  UserId=ua.UserId
                              };
            List <OptionsModel> list = new List<OptionsModel>();
            foreach (UserManagementModel dsc in objDepUsers)
            {
                list.Add(new OptionsModel { ID = dsc.UserId, Name = dsc.FirstName });
            }
            return list;
        }
        public List<OptionsModel> getIndividualusrs(int userId, int  roleId, int department, int orgId)
        {
            List<GetIndividualUserList_Result> objUsers = new List<GetIndividualUserList_Result>();
            objUsers = GetIndividualUserList(userId, roleId, department,  orgId);
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (GetIndividualUserList_Result dsc in objUsers)
            {
                list.Add(new OptionsModel { ID = dsc.ID, Name = dsc.Name });
            }
            return list;
        }
        public List<GetIndividualUserList_Result> GetIndividualUserList(int userId,int roleId,int department,int orgId)
        {
            return db.GetIndividualUserList(userId,roleId,department, orgId).ToList<GetIndividualUserList_Result>();
        }
        //public  List<GetDocumentCategoryByUser_Result> getDocumentCategoryByUser(int? userID)
        //{
        //    return db.GetDocumentCategoryByUser(userID).ToList<GetDocumentCategoryByUser_Result>();
        //}

        public List<DocumentCategoryByUser_sp_Result1> getDocumentCategoryByuser(int? userID)
        {
            return db.DocumentCategoryByUser_sp(userID).ToList<DocumentCategoryByUser_sp_Result1>();
        }
        public List<OptionsModel> getDocumentCategoryFilteredByUser(int? userID)
        {
            List<DocumentCategoryByUser_sp_Result1> objDocumentCategory = new List<DocumentCategoryByUser_sp_Result1>();
            objDocumentCategory = getDocumentCategoryByuser(userID);
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (DocumentCategoryByUser_sp_Result1 dsc in objDocumentCategory)
            {
                list.Add(new OptionsModel { ID = dsc.ID, Name = dsc.Name });
            }
            return list;
        }

        /// <summary>
        /// Total Number Of Documents Created For This Month- Account Admin(Organization), Account User, Individual User
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <returns>Total</returns>
        public int? GetCurrentMonthCount(int? orgId, int? userId,int? DepartmentID)
        {
            return db.GetCurrentMonthCount(orgId, userId,DepartmentID).FirstOrDefault();
        }

        /// <summary>
        ///  Total Count For Each Organization
        /// </summary>
        /// <returns>OrgName, Filled Document Count</returns>
        public List<GetTotalDocumentCountByOrganization_sp_Result> GetTotalDocumentCountByOrganization_sp()
        {
            return db.GetTotalDocumentCountByOrganization_sp().ToList<GetTotalDocumentCountByOrganization_sp_Result>();
        }

        /// <summary>
        /// Total Number Of Documents Created - Account Admin(Organization), Account User
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int? GetTotalDocumentCountOrgUser_sp(int? orgId, int? userId)
        {
            return db.GetTotalDocumentCountOrgUser_sp(orgId, userId).FirstOrDefault();
        }

        public List<GetOrganizationCountByCategoy_Result> GetOrganizationCountByCategoy()
        {
            return db.GetOrganizationCountByCategoy().ToList<GetOrganizationCountByCategoy_Result>();
        }

        /// <summary>
        /// get Category, Sub, Subsub category count for current month - Account Admin, Individual Users,Company users
        /// </summary>
        /// <param name="categoryType"></param>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<GetCategoriesOrgUsersCurrnetMonthTotalCount_Result> getCategoriesOrgUsersCurrnetMonthTotalCount(int? categoryType, int? orgId, int? userId)
        {
            return db.GetCategoriesOrgUsersCurrnetMonthTotalCount(categoryType, orgId, userId).ToList<GetCategoriesOrgUsersCurrnetMonthTotalCount_Result>();
        }

        /// <summary>
        /// get Category, Sub, Subsub category total count  - Account Admin, Individual Users
        /// </summary>
        /// <param name="categoryType"></param>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<GetCategoriesOrgUsersTotalCount_Result> getCategoriesOrgUsersTotalCount(int? categoryType, int? orgId, int? userId,int? DepartmentID)
        {
            return db.GetCategoriesOrgUsersTotalCount(categoryType, orgId, userId,DepartmentID).ToList<GetCategoriesOrgUsersTotalCount_Result>();
        }

        /// <summary>
        /// get Category, Sub, Subsub category total count  - Super Admin
        /// </summary>
        /// <param name="categoryType"></param>       
        /// <returns></returns>
        public List<GetCategoriesTotalCount_Result> GetCategoriesTotalCount(int? categoryType)
        {
            return db.GetCategoriesTotalCount(categoryType).ToList<GetCategoriesTotalCount_Result>();
        }

        /// <summary>
        /// Getting Company Registration users count based for each month
        /// </summary>
        /// <returns>Company Name, month name and count</returns>
        public List<getGraphMonthlyCompanyRegister_Result> getGraphMonthlyCompanyRegister()
        {
            return db.getGraphMonthlyCompanyRegister().ToList<getGraphMonthlyCompanyRegister_Result>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public List<NewEnquiriesDueDiligence_sp_Result> getNewEnquiriesDueDiligence(int? month)
        {
            return db.NewEnquiriesDueDiligence_sp(month).ToList<NewEnquiriesDueDiligence_sp_Result>();
        }

        /// <summary>
        /// Individual User Registration Count per month
        /// </summary>
        /// <returns></returns>
        public List<GraphMonthlyIndividualRegister_Result> getGraphMonthlyIndividualRegister()
        {
            return db.getGraphMonthlyIndividualRegister().ToList<GraphMonthlyIndividualRegister_Result>();
        }

        public List<CategoryInvoiceTotalAmount_Result> getCategoryInvoiceTotalAmount(int? userId)
        {
          return db.CategoryInvoiceTotalAmount(userId).ToList<CategoryInvoiceTotalAmount_Result>();
        }

        public List<DueInvoiceThisMonth_Result> getDueInvoiceThisMonth()
        {
            return db.DueInvoiceThisMonth().ToList<DueInvoiceThisMonth_Result>();
        }

        public List<GraphInvoiceTotalAmount_Result> getGraphInvoiceTotalAmount(int? userId)
        {
            return db.GraphInvoiceTotalAmount(userId).ToList<GraphInvoiceTotalAmount_Result>();
        }

        public decimal? getInvoiceTotalAmount(int? userId)
        {
            return db.InvoiceTotalAmount(userId).FirstOrDefault();
        }

        public List<GetCategoriesCurrnetMonthTotalCount_Result> getCategoriesCurrnetMonthTotalCount(int? categoryType)
        {
            return db.GetCategoriesCurrnetMonthTotalCount(categoryType).ToList<GetCategoriesCurrnetMonthTotalCount_Result>();
        }

        #region Log

        public List<ViewLogCategory_Result> ViewLogCategory(int? logId,int id)
        {
            return db.ViewLogCategory(logId,id).ToList<ViewLogCategory_Result>();
        }
        public List<ViewLogSubCategory_Result> ViewLogSubCategory(int? logId,int id)
        {
            return db.ViewLogSubCategory(logId,id).ToList<ViewLogSubCategory_Result>();
        }
        public List<ViewLogSubSubCategory_Result> ViewLogSubSubCategory(int? logId,int id)
        {
            return db.ViewLogSubSubCategory(logId,id).ToList<ViewLogSubSubCategory_Result>();
        }
        public List<ViewLogTemplateUpload_Result> ViewLogTemplateUpload(int? logId,int? id)
        {
            return db.ViewLogTemplateUpload(logId,id).ToList<ViewLogTemplateUpload_Result>();
        }
        public List<LogRegistrationList_Result> LogRegistration()
        {
            return db.LogRegistrationList().ToList<LogRegistrationList_Result>(); 
        }
        public List<ViewLogRegistration_Result1> ViewLogRegistration(int? logId)
        {
            return db.ViewLogRegistration(logId).ToList<ViewLogRegistration_Result1>();
        }
        public List<ViewLogService_Result> ViewLogService(int? logId,int? serviceID)
        {
            return db.ViewLogService(logId,serviceID).ToList<ViewLogService_Result>();
        }
        #endregion
        #region Multiselect Dropdown
        public void insertAssociateTemplate(int? templateId, int? associateTemplateId,int? ordervalue,bool?  mandatory)
        {
            db.UpdateAssociateTemplate(templateId, associateTemplateId, ordervalue,mandatory);
        }
        #endregion
        /// <summary>
        /// Getting Category List for binding dropdownlist - While adding Key category
        /// </summary>
        /// <returns></returns>
        public List<OptionsModel> getTemplateKeyCategoryList(int? id)
        {
            List<KeyCategory> objCat = new List<KeyCategory>();
            objCat = GetKeyCategories().Where(m => (id == null || m.Id == id) && m.IsEnabled == true).OrderBy(m=>m.CategoryOrder).ToList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (KeyCategory dsc in objCat)
            {
                list.Add(new OptionsModel { ID = dsc.Id, Name = dsc.CategoryName });
            }
            return list;
        }

        public List<OptionsModel> getTemplateKeyList()
        {
            List<TemplateKeyword> objCat = new List<TemplateKeyword>();
            objCat = db.TemplateKeywords.Where(m =>  m.IsEnabled == true && m.AddedByClient==false).OrderBy(m => m.TemplateKeyId).ToList();
            List<OptionsModel> list = new List<OptionsModel>();
            foreach (TemplateKeyword dsc in objCat)
            {
                list.Add(new OptionsModel { ID = dsc.TemplateKeyId, Name = dsc.TemplateKeyLabels });
            }
            return list;
        }

        public List<KeyCategoryModel> getAllKeycategoriesforGrid()
        {
            List<KeyCategory> objCat = new List<KeyCategory>();
            objCat = db.KeyCategories.ToList();
            List<KeyCategoryModel> list = new List<KeyCategoryModel>();
            foreach (KeyCategory dsc in objCat)
            {
                list.Add(new KeyCategoryModel { ID = dsc.Id, CategoryName = dsc.CategoryName, CategoryDescription = dsc.CategoryDescription, IsEnabled = dsc.IsEnabled });
            }
            return list;
        }

        public List<KeyCategory> GetKeyCategories()
        {
            return db.KeyCategories.ToList<KeyCategory>();
        }

        public List<OptionsModel> getUserList(int roleID,int userID,int DepartmentID,int orgID)
        {
            try
            {
                //List<UserAddressDetail> objUsers = new List<UserAddressDetail>();
               var objUsers = (from e in db.UserAddressDetails
                            join p in db.UserProfiles on
e.UserId equals p.UserID
                            where p.IsEnabled == true && (roleID==1 || ( roleID==2 && p.OrganizationId==orgID) || (roleID==6 && p.OrganizationId == orgID &&  p.Department==DepartmentID))
                            select new  { UserId = e.UserId, FirstName = e.FirstName, LastName = e.LastName }).ToList();
                List<OptionsModel> list = new List<OptionsModel>();
                for (int i=0;i< objUsers.Count();i++)
                {
                    list.Add(new OptionsModel { ID = objUsers[i].UserId, Name = objUsers[i].FirstName + " " + objUsers[i].LastName });
                }
                return list;
            }
            catch (Exception ex)
            { return null; }
        }
    }
}