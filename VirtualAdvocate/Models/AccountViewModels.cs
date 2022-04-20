using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
namespace VirtualAdvocate.Models
{
    public class DueDiligenceUserViewModel
    {
        [Required(ErrorMessage = "Please Select Enquiry Type")]
        public string EnquiryType { get; set; }

        [Required(ErrorMessage = "Please Enter First Name")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please Enter Last Name")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please Enter Email Address")]
        [DisplayName("Email Address")]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        [Remote("CheckUserexist", "UsersRegistration", ErrorMessage = "User Email Address Already Exists")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Please Enter Phone Number")]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please Enter Password")]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string password { get; set; }

        [DataType(DataType.Password)]
        [DisplayName("Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("password", ErrorMessage = "Confirm Password Does Not Match")]
        public string Confirmpassword { get; set; }

        [Required(ErrorMessage = "Please Enter Street Name")]
        [DisplayName("Street Name")]
        public string StreetName { get; set; }

        [Required(ErrorMessage = "Please Enter Building Name")]
        [DisplayName("Building Name")]
        public string BuildingName { get; set; }

        [Required(ErrorMessage = "Please Enter Plot Number")]
        [DisplayName("Plot Number")]
        public string PlotNo { get; set; }

        [Required(ErrorMessage = "Please Enter Block Number")]
        [DisplayName("Block Number")]
        public string BlockNo { get; set; }

        [Required(ErrorMessage = "Please Enter Region")]
        [DisplayName("Region")]
        public string Region { get; set; }

        [DisplayName("Near To")]
        public string LandMark { get; set; }

        [Required(ErrorMessage = "Please Accept The Terms & Conditions")]
        public bool TermsConditions { get; set; }
        public IEnumerable<PaymentMethodModel> AvailablePaymentMethods { get; set; }
        public IEnumerable<PaymentMethodModel> SelectedPaymentMethods { get; set; }
        [Required(ErrorMessage = "Select Atleast One Payment Method")]
        public PostedPaymentMethods PostedPaymentMethods { get; set; }


    }
    public class UserRegistrationModel
    {
        [Required(ErrorMessage = "Please Enter First name")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please Enter Last Name")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please Enter Email Address")]
        [DisplayName("Email Address")]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        [System.Web.Mvc.Remote("CheckUserexist", "UsersRegistration", ErrorMessage = "User Email Address Already Exists")]
        public string EmailAddress { get; set; }

        //[Required(ErrorMessage = "Please enter phone number")]
        //[DisplayName("Phone Number")]
        //public string PhoneNumber { get; set; }

        public string Designation { get; set; }

        [Required(ErrorMessage = "Please Enter Password")]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string password { get; set; }

        [DisplayName("Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("password", ErrorMessage = "Confirm Password Does Not Match")]
        public string Confirmpassword { get; set; }

        [Required(ErrorMessage = "Please Enter Organization Name")]
        [DisplayName("Organization Name")]
        public string OrgName { get; set; }

        [Required(ErrorMessage = "Please Enter Phone Number")]
        [DisplayName("Organization Phone Number")]
        public string OrgPhoneNumber { get; set; }

        [Required(ErrorMessage = "Please Enter Email Address")]
        [DisplayName("Organization Email")]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        public string OrgEmail { get; set; }

        [Required(ErrorMessage = "Please Enter Street Name")]
        [DisplayName("Street Name")]
        public string OrgStreetName { get; set; }

        [Required(ErrorMessage = "Please Enter Building Name")]
        [DisplayName("Building Name")]
        public string OrgBuildingName { get; set; }

        [Required(ErrorMessage = "Please Enter Plot Number")]
        [DisplayName("Plot Number")]
        public string OrgPlotNo { get; set; }

        [Required(ErrorMessage = "Please Enter Block Number")]
        [DisplayName("Block Number")]
        public string OrgBlockNo { get; set; }

        [Required(ErrorMessage = "Please Enter Region")]
        [DisplayName("Region")]
        public string OrgRegion { get; set; }

        [DisplayName("Near To")]
        public string OrgLandMark { get; set; }
        public int UserAccountType { get; set; }
        [Required(ErrorMessage = "Please Accept The Terms & Conditions")]
        public bool TermsConditions { get; set; }

        public IEnumerable<PaymentMethodModel> AvailablePaymentMethods { get; set; }
        public IEnumerable<PaymentMethodModel> SelectedPaymentMethods { get; set; }
        [Required(ErrorMessage = "Select Atleast One Payment Method")]
        public PostedPaymentMethods PostedPaymentMethods { get; set; }

        [Required(ErrorMessage = "Please Select Type Of Organization")]
        public int OrganizationTypeId { get; set; }
        public IEnumerable<OptionsModel> getAllOrganizationTypes { get; set; }

        public IEnumerable<AccountServicesModel> AvailableService { get; set; }
        public IEnumerable<AccountServicesModel> SelectedService { get; set; }

        public PostedServices PostedServices { get; set; }

        public IEnumerable<DepartmentModel> AvailableDepartment { get; set; }
        public IEnumerable<DepartmentModel> SelectedDepartment { get; set; }
        [Required(ErrorMessage = "Select Atleast One Department")]
        public PostedDepartment PostedDepartment { get; set; }


        public IEnumerable<ClientWiseCustomerTemplate> extraFields { get; set; }

    }
    public class OrganizationViewModel
    {

        [Required(ErrorMessage = "Please Enter Organization Name")]
        [DisplayName("Organization Name")]
        public string OrgName { get; set; }

        [Required(ErrorMessage = "Please Enter Phone Number")]
        [DisplayName("Organization Phone Number")]
        public string OrgPhoneNumber { get; set; }

        [Required(ErrorMessage = "Please Enter Email Address")]
        [DisplayName("Organization Email")]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        public string OrgEmail { get; set; }

        [Required(ErrorMessage = "Please Enter Street Name")]
        [DisplayName("Street Name")]
        public string OrgStreetName { get; set; }

        [Required(ErrorMessage = "Please Enter Building Name")]
        [DisplayName("Building Name")]
        public string OrgBuildingName { get; set; }

        [Required(ErrorMessage = "Please Enter Plot Number")]
        [DisplayName("Plot Number")]
        public string OrgPlotNo { get; set; }

        [Required(ErrorMessage = "Please Enter Block Number")]
        [DisplayName("Block Number")]
        public string OrgBlockNo { get; set; }

        [Required(ErrorMessage = "Please Enter Region")]
        [DisplayName("Region")]
        public string OrgRegion { get; set; }

        [DisplayName("Near To")]
        public string OrgLandMark { get; set; }
        public int OrganizationId { get; set; }
        public int UserAccountsType { get; set; }
        public int userId { get; set; }
        public List<UserAccountType> userAccountTypes { get; set; }

        [Required(ErrorMessage = "Please Select Type Of Organization")]
        public int OrganizationTypeId { get; set; }
        public IEnumerable<OptionsModel> getAllOrganizationTypes { get; set; }
        [Required(ErrorMessage = "Please Accept The Terms & Conditions")]
        public bool TermsConditions { get; set; }
        public IEnumerable<DepartmentModel> AvailableDepartment { get; set; }
        public IEnumerable<DepartmentModel> SelectedDepartment { get; set; }

        public PostedDepartment PostedDepartment { get; set; }
        public IEnumerable<ClientWiseCustomerTemplate> extraFields { get; set; }
    }

    public class PersonalDetailsViewModel
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "Please Enter First Name")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please Enter Last Name")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        //[Required(ErrorMessage = "Please Enter Email Address")]
        //[DisplayName("Email Address")]
        //[EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        //[Remote("CheckUserexist", "UsersRegistration", ErrorMessage = "User email address already exists")]
        public string EmailAddress { get; set; }

        public string Designation { get; set; }

        //[Required(ErrorMessage = "Please Enter Street Name")]
        [DisplayName("Street Name")]
        public string StreetName { get; set; }

        //[Required(ErrorMessage = "Please Enter Building Name")]
        [DisplayName("Building Name")]
        public string BuildingName { get; set; }

        //[Required(ErrorMessage = "Please Enter Plot Number")]
        [DisplayName("Plot Number")]
        public string PlotNo { get; set; }

        //[Required(ErrorMessage = "Please Enter Block Number")]
        [DisplayName("Block Number")]
        public string BlockNo { get; set; }

        //[Required(ErrorMessage = "Please Enter Region")]
        [DisplayName("Region")]
        public string Region { get; set; }

        [DisplayName("Near To")]
        public string LandMark { get; set; }
        [DisplayName("Department")]
        public int Department { get; set; }
        [DisplayName("Role")]
        public int roleID { get; set; }
        public IEnumerable<OptionsModel> getDepartmentList { get; set; }
        public IEnumerable<OptionsModel> getRoleList { get; set; }
    }

    public class OrgUserViewModel
    {

        [Required(ErrorMessage = "Please Enter First Name")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please Enter Last Name")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please Enter Email Address")]
        [DisplayName("Email Address")]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        [Remote("CheckUserexist", "UsersRegistration", ErrorMessage = "User email address already exists")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Please Enter Phone Number")]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please Enter Password")]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string password { get; set; }

        [DataType(DataType.Password)]
        [DisplayName("Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("password", ErrorMessage = "Confirm Password Does Not Match")]
        public string Confirmpassword { get; set; }

        [Required(ErrorMessage = "Please Enter Street Name")]
        [DisplayName("Street Name")]
        public string StreetName { get; set; }

        [Required(ErrorMessage = "Please Enter Building Name")]
        [DisplayName("Building Name")]
        public string BuildingName { get; set; }

        [Required(ErrorMessage = "Please Enter Plot Number")]
        [DisplayName("Plot Number")]
        public string PlotNo { get; set; }

        [Required(ErrorMessage = "Please Enter Block Number")]
        [DisplayName("Block Number")]
        public string BlockNo { get; set; }

        [Required(ErrorMessage = "Please Enter Region")]
        [DisplayName("Region")]
        public string Region { get; set; }

        [DisplayName("Near To")]
        public string LandMark { get; set; }


        public string Designation { get; set; }
        public int OrgId { get; set; }

        public IEnumerable<AccountServicesModel> AvailableService { get; set; }
        public IEnumerable<AccountServicesModel> SelectedService { get; set; }
        [Required(ErrorMessage = "Select Atleast One Service")]
        public PostedServices PostedServices { get; set; }

        public IEnumerable<PaymentMethodModel> AvailablePaymentMethods { get; set; }
        public IEnumerable<PaymentMethodModel> SelectedPaymentMethods { get; set; }
        [Required(ErrorMessage = "Select Atleast One Payment Method")]
        public PostedPaymentMethods PostedPaymentMethods { get; set; }

        public IEnumerable<DepartmentModel> AvailableDepartment { get; set; }
        public IEnumerable<DepartmentModel> SelectedDepartment { get; set; }
        [Required(ErrorMessage = "Select Atleast One Payment Method")]
        public PostedDepartment PostedDepartment { get; set; }


        public int Department { get; set; }
        [DisplayName("Role")]
        public int RoleID { get; set; }
        public IEnumerable<OptionsModel> getRoleList { get; set; }

        public IEnumerable<OptionsModel> getDepartmentList { get; set; }

    }

    public class AccountServicesModel
    {
        //Integer value of a checkbox
        public int Id { get; set; }

        //String name of a checkbox
        public string Name { get; set; }

        //Boolean value to select a checkbox
        //on the list
        public bool IsSelected { get; set; }

        //Object of html tags to be applied
        //to checkbox, e.g.:'new{tagName = "tagValue"}'
        public object Tags { get; set; }
    }

    public class PaymentMethodModel
    {
        //Integer value of a checkbox
        public int Id { get; set; }

        //String name of a checkbox
        public string Name { get; set; }

        //Boolean value to select a checkbox
        //on the list
        public bool IsSelected { get; set; }

        //Object of html tags to be applied
        //to checkbox, e.g.:'new{tagName = "tagValue"}'
        public object Tags { get; set; }
    }

    public class PostedPaymentMethods
    {
        public string[] PaymentTypeIds { get; set; }
    }

    public class PostedDepartment
    {
        public string[] DepartmentIDs { get; set; }
    }

    public class PostedServices
    {
        public string[] ServiceIds { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Please Enter Email Address")]
        [DisplayName("Email Address")]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]

        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Please Enter Password")]
        // [DataType(DataType.Password)]
        [DisplayName("Password")]

        public string Password { get; set; }


        [Display(Name = "Keep me signed in")]
        public bool RememberMe { get; set; }
        // public List<AccountService> getAllServices { get; set; }
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Please Enter Email Address")]
        [DisplayName("Email Address")]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        public string EmailAddress { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string EmailAddress { get; set; }
        public string CheckPoint { get; set; }
        [Required]
        [DisplayName("New Password")]
        public string NewPassword { get; set; }
        [Required]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword")]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    public class ProfileViewModel
    {
        public int userId { get; set; }
        public int OrganizationId { get; set; }
        #region Personal Details
        public string RoleDescription { get; set; }
        public int RoleId { get; set; }
        public string EnquiryType { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Email Address")]
        public string EmailAddress { get; set; }

        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [DisplayName("Street Name")]
        public string StreetName { get; set; }

        [DisplayName("Building Name")]
        public string BuildingName { get; set; }

        [DisplayName("Plot Number")]
        public string PlotNo { get; set; }

        [DisplayName("Block Number")]
        public string BlockNo { get; set; }

        [DisplayName("Region")]
        public string Region { get; set; }

        [DisplayName("Near to")]
        public string LandMark { get; set; }

        [DisplayName("Designation")]
        public string Designation { get; set; }

        // public List<AccountService> getSelectedService { get; set; }

        public List<PaymentMethod> getSeletedPayment { get; set; }

        #endregion
        #region Organization Details

        [DisplayName("Organization Name")]
        public string OrgName { get; set; }

        [DisplayName("Organization Phone Number")]
        public string OrgPhoneNumber { get; set; }

        [DisplayName("Organization Email")]
        public string OrgEmail { get; set; }

        [DisplayName("Street Name")]
        public string OrgStreetName { get; set; }

        [DisplayName("Building Name")]
        public string OrgBuildingName { get; set; }

        [DisplayName("Plot Number")]
        public string OrgPlotNo { get; set; }

        [DisplayName("Block Number")]
        public string OrgBlockNo { get; set; }

        [DisplayName("Region")]
        public string OrgRegion { get; set; }

        [DisplayName("Near to")]
        public string OrgLandMark { get; set; }

        public int UserAccountTypeid { get; set; }

        [DisplayName("Account Type")]
        public string UserAccountType { get; set; }

        #endregion
        #region Service & Payment methods
        public string AccountServices { get; set; }
        public string PaymentMethods { get; set; }

        public string Department { get; set; }
        #endregion'


    }

    public class AccountServiceModel
    {
        #region EditAccount Services
        public int userId { get; set; }
        public IEnumerable<AccountServicesModel> AvailableService { get; set; }
        public IEnumerable<AccountServicesModel> SelectedService { get; set; }
        //[Required(ErrorMessage = "Select Atleast One Service")]
        public PostedServices PostedServices { get; set; }

        #endregion
    }
    public class EditPaymentMethodeModel
    {
        public int userId { get; set; }
        public IEnumerable<PaymentMethodModel> AvailablePaymentMethods { get; set; }
        public IEnumerable<PaymentMethodModel> SelectedPaymentMethods { get; set; }
        [Required(ErrorMessage = "Select Atleast One Payment Method")]
        public PostedPaymentMethods PostedPaymentMethods { get; set; }
    }

    public class ChangePassword
    {
        [Required(ErrorMessage = "Please Enter Old Password")]
        [DataType(DataType.Password)]
        [DisplayName("Old Password")]
        public string password { get; set; }

        [Required(ErrorMessage = "Please Enter New Password")]
        [DataType(DataType.Password)]
        [DisplayName("New Password")]
        public string newpassword { get; set; }

        [DataType(DataType.Password)]
        [DisplayName("Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("newpassword", ErrorMessage = "Confirm Password Does Not Match")]
        public string Confirmpassword { get; set; }
        public IEnumerable<OptionsModel> getAllUsers { get; set; }
        public int userId { get; set; }
    }

    public class DueDiligenceEnquiryViewModel
    {
        public int EnquiryId { get; set; }
        [Required(ErrorMessage = "Please Select Enquiry Type")]
        public int EnquiryTypeId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegName { get; set; }
        public string BusinessName { get; set; }
        public string BusinessRegistrationNumber { get; set; }
        public string CertificateTitleNo { get; set; }
        public string PlotNumber { get; set; }
        public string BlockNumber { get; set; }
        public string Area { get; set; }
        public string Municipality { get; set; }
        public string Region { get; set; }
        public int EnquiryType { get; set; }
        public int UserId { get; set; }
        public bool? PaidStatus { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime? CreatedDate { get; set; }
        public IEnumerable<OptionsModel> getAllEnquiryType { get; set; }
        [Required(ErrorMessage = "Please Enter Valid Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? TimeLine { get; set; }
        [Required(ErrorMessage = "Please Enter Valid Amount")]
        public decimal? Cost { get; set; }
        public string ReportDocument { get; set; }
        public string InvoiceDocument { get; set; }
        public bool? ReplyStatus { get; set; }
        public HttpPostedFileBase AttachFile { get; set; }
    }

    public class DueDiligenceAttachViewModel
    {
        public int EnquiryId { get; set; }
        public int EnquiryTypeId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegName { get; set; }
        public string BusinessName { get; set; }
        public string BusinessRegistrationNumber { get; set; }
        public string CertificateTitleNo { get; set; }
        public string PlotNumber { get; set; }
        public string BlockNumber { get; set; }
        public string Area { get; set; }
        public string Municipality { get; set; }
        public string Region { get; set; }
        public int EnquiryType { get; set; }
        public int UserId { get; set; }
        public bool? PaidStatus { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime? CreatedDate { get; set; }
        public IEnumerable<OptionsModel> getAllEnquiryType { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? TimeLine { get; set; }
        public decimal? Cost { get; set; }
        public string ReportDocument { get; set; }
        public string InvoiceDocument { get; set; }
        public bool? ReplyStatus { get; set; }
        [Required(ErrorMessage = "Please Select The Attachment File")]
        public HttpPostedFileBase AttachFile { get; set; }
    }

    public class DueDiligenceUserEnquiryViewModel
    {
        public int EnquiryId { get; set; }
        [Required(ErrorMessage = "Please Select Enquiry Type")]
        public int EnquiryTypeId { get; set; }
        //[Required(ErrorMessage = "Please Enter Company Name")]
        public string CompanyName { get; set; }
        //[Required(ErrorMessage = "Please Enter Company Incorporation Number")]
        public string CompanyRegName { get; set; }
        //[Required(ErrorMessage = "Please Enter Business Name")]
        public string BusinessName { get; set; }
        //[Required(ErrorMessage = "Please Enter Business Registration Number")]
        public string BusinessRegistrationNumber { get; set; }
        public string CertificateTitleNo { get; set; }
        public string PlotNumber { get; set; }
        public string BlockNumber { get; set; }
        public string Area { get; set; }
        public string Municipality { get; set; }
        public string Region { get; set; }
        public int EnquiryType { get; set; }
        public int UserId { get; set; }
        public bool? PaidStatus { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime? CreatedDate { get; set; }
        public IEnumerable<OptionsModel> getAllEnquiryType { get; set; }

    }

    public class DueDiligenceEnquiryListViewModel
    {
        public int EnquiryId { get; set; }
        public int EnquiryTypeId { get; set; }
        public string EnquiryType { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool? PaidStatus { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ReportDocument { get; set; }
        public string InvoiceDocument { get; set; }
        public bool? ReplyStatus { get; set; }
        public HttpPostedFileBase AttachFile { get; set; }
    }

    public class InvoiceModelDetail
    {
        public int InvoiceId { get; set; }
        public int CustomerId { get; set; }
        public int GroupId { get; set; }
        [Required(ErrorMessage = "Please Enter The Total Amount")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid Price/Only 2 Digits Allowed For After Decision Point")]
        public decimal TotalAmount { get; set; }
        public string InvoiceDocumentName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? PaidStatus { get; set; }
    }

    public class InvoiceListModel
    {
        public bool PaidStatus { get; set; }
        public string InvoiceDocumentName { get; set; }
        public string DocumentTitle { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CustomerId { get; set; }
        public int groupid { get; set; }
        public string CustomerName { get; set; }
    }

    public class ReportsListModel
    {

        public int ReportTypeId { get; set; }
        public IEnumerable<OptionsModel> getAllReportType { get; set; }

        public int OrgId { get; set; }
        public IEnumerable<OptionsModel> getAllOrganization { get; set; }
        public int TemplateId { get; set; }
        public IEnumerable<OptionsModel> getAllDocumentTypes { get; set; }

    
        //[DataType(DataType.Date)]
        public string FromDate { get; set; }
        //[DataType(DataType.Date)]
        public string ToDate { get; set; }
        public IEnumerable<GetReportData_Result> getReportDetails { get; set; }

        public int? UserId { get; set; }
        public IEnumerable<OptionsModel> getAllOrgUsers { get; set; }

        public int? OrgUserId { get; set; }
        public IEnumerable<OptionsModel> getAllOrgUsers1 { get; set; }

        public int IndividualUserId { get; set; }
        public IEnumerable<OptionsModel> getSingleUserCompanyList { get; set; }

        public int? ExcelExportStatus { get; set; }
        public int DocumentCategoryId { get; set; }
        public int? DocumentSubCategoryId { get; set; }
        public int? DocumentSubSubCategoryId { get; set; }
        public IEnumerable<OptionsModel> getAllCategory { get; set; }
        public IEnumerable<OptionsModel> getAllSubCategory { get; set; }
        public IEnumerable<OptionsModel> getAllSubSubCategory { get; set; }
        public int RoleId { get; set; }
        public int? CurrentOrgId { get; set; }

    }

    public class ReportDetailsModel
    {

        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public int groupid { get; set; }
        public string CompanyName { get; set; }
        public string User { get; set; }
        public string DocumentTitle { get; set; }
        public string DocumentType { get; set; }
        public string OrgName { get; set; }
        public string Cost { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CustomerName { get; set; }
        public bool? PaidStatus { get; set; }
    }

    public class DashBoardModel
    {
        public int UserCount { get; set; }
        public int? OrgId { get; set; }
        public int IndividualCount { get; set; }
        public int CompanyUserCount { get; set; }
        public int CategoryCount { get; set; }
        public int SubCategoryCount { get; set; }
        public int SubSubCategoryCount { get; set; }
        public int TotalDocumentCountOrg { get; set; }
        public int MonthlyDocumentCount { get; set; }
        public IEnumerable<GetTotalDocumentCountByOrganization_sp_Result> getDocumentCountPerOrg { get; set; }
        public Chart OrgCategoryChart { get; set; }
        public IEnumerable<GetOrganizationCountByCategoy_Result> getOrganizationCountCategorywise { get; set; }


        public IEnumerable<GetCategoriesOrgUsersTotalCount_Result> getCategoriesOrgUsersTotalCount { get; set; }
        public IEnumerable<GetCategoriesOrgUsersTotalCount_Result> getSubCategoriesOrgUsersTotalCount { get; set; }
        public IEnumerable<GetCategoriesOrgUsersTotalCount_Result> getSubSubCategoriesOrgUsersTotalCount { get; set; }

        public IEnumerable<GetCategoriesTotalCount_Result> getCategoriesTotalCount { get; set; }
        public IEnumerable<GetCategoriesTotalCount_Result> getSubCategoriesTotalCount { get; set; }
        public IEnumerable<GetCategoriesTotalCount_Result> getSubSubCategoriesTotalCount { get; set; }

        public IEnumerable<GetCategoriesCurrnetMonthTotalCount_Result> getCategoriesCurrnetMonthTotalCount { get; set; }
        public IEnumerable<GetCategoriesCurrnetMonthTotalCount_Result> getSubCatCurrnetMonthTotalCount { get; set; }
        public IEnumerable<GetCategoriesCurrnetMonthTotalCount_Result> getSubSubCatCurrnetMonthTotalCount { get; set; }

        public IEnumerable<GetCategoriesOrgUsersCurrnetMonthTotalCount_Result> getCategoriesOrgUsersCurrnetMonthTotalCount { get; set; }
        public IEnumerable<GetCategoriesOrgUsersCurrnetMonthTotalCount_Result> getSubCatOrgUsersCurrnetMonthTotalCount { get; set; }
        public IEnumerable<GetCategoriesOrgUsersCurrnetMonthTotalCount_Result> getSubSubCatOrgUsersCurrnetMonthTotalCount { get; set; }

        public IEnumerable<OrganizationCountGraph> createOrganizationGraph { get; set; }
        public IEnumerable<NewEnquiriesDueDiligence_sp_Result> DueEnquiryCount { get; set; }
        public IEnumerable<NewEnquiriesDueDiligence_sp_Result> DueEnquiryMonthCount { get; set; }
        public IEnumerable<GraphMonthlyIndividualRegister_Result> IndividualUserCount { get; set; }

        public IEnumerable<DueInvoiceThisMonth_Result> displayInvoiceThisMonth { get; set; }
        public decimal? displayInvoiceTotalAmount { get; set; }
        public IEnumerable<CategoryInvoiceTotalAmount_Result> displayCategoryInvoiceTotalAmount_Result { get; set; }

        public IEnumerable<Notification> Notifications { get; set; }

        public IEnumerable<Notification> GetNotificationDetails(NotificationModel model)
        {
            List<Entity> result = new List<Entity>();
            var db = new VirtualAdvocateEntities();
            DateTime minDate = new DateTime(2001, 1, 1), maxDate = new DateTime(2001, 1, 1);
            DateTime todaysDate = DateTime.Today;

            List<Entity> entities = new List<Entity>();

            var insurances = db.Insurances.Where(m => m.Status)
                .Include("Property")
                .Include("Property.FilledTemplateDetail")
                .Include("UserProfile")
                .Where(m => m.UserId == model.UserId || (model.RoleId == 6 && m.UserProfile.OrganizationId == model.OrganizationId && m.UserProfile.Department == model.DepartmentId))
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
                .Where(m => m.UserId == model.UserId || (model.RoleId == 6 && m.UserProfile.OrganizationId == model.OrganizationId && m.UserProfile.Department == model.DepartmentId))
                .Select(s => new Entity
                {
                    Id = s.Id,
                    DateOfExpiry = s.DateOfExpiry,
                    OrgId = s.CustomerDetail.OrganizationId,
                    UserId = s.UserId.Value,
                    TemplateType = TemplateType.Probation,
                    CustomerName = s.CustomerDetail.CustomerName
                });

            var documents = db.DocumentDetails.Where(m => m.Status)
                .Include("FilledTemplateDetail")
                .Include("UserProfile")
                .Where(m => m.DocumentStatus != (int)DocumentDetailStatus.Complete && m.UserId == model.UserId || (model.RoleId == 6 && m.DocumentStatus != (int)DocumentDetailStatus.Complete && m.UserProfile.OrganizationId == model.OrganizationId && m.UserProfile.Department == model.DepartmentId))
                .Select(s => new Entity
                {
                    Id = s.Id,
                    DateOfExpiry = s.DateToBeSubmitted,
                    OrgId = s.FilledTemplateDetail.OrgId.Value,
                    UserId = model.UserId,
                    TemplateType = TemplateType.Document,
                    CustomerName = db.CustomerDetails.FirstOrDefault(m => m.CustomerId == s.FilledTemplateDetail.CustomerId).CustomerName
                });

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
                            item.TemplateCategory = TemplateCategory.OnExpiry;
                        else if (item.DateOfExpiry >= DateTime.Today.Date)
                            item.TemplateCategory = TemplateCategory.PriorToExpiry;
                        else
                            item.TemplateCategory = TemplateCategory.AfterExpiry;

                        result.Add(item);
                        //SendMail(user.EmailAddress, placeholder, item.OrgId, user.Department);
                    }
                }
                catch (Exception ex)
                {
                    //Logger.Log(item.TemplateType.ToString() + "Id: " + item.Id + "\nError: " + ex.Message);
                }
            }

            List<Notification> notifications = new List<Notification>();

            if (model.FlatForNotification != 1)
            {
                notifications = result.Where(m => m.TemplateCategory == TemplateCategory.AfterExpiry).GroupBy(g => g.TemplateType)
                .Select(s => new Notification
                {
                    TemplateType = s.Key,
                    PriorToExpiry = s.Where(m => m.TemplateCategory == TemplateCategory.PriorToExpiry).Count(),
                    OnExpiry = s.Where(m => m.TemplateCategory == TemplateCategory.OnExpiry).Count(),
                    AfterExpiry = s.Where(m => m.TemplateCategory == TemplateCategory.AfterExpiry).Count()
                }).ToList();
            }
            else
            {
                notifications = result.GroupBy(g => g.TemplateType)
                   .Select(s => new Notification
                   {
                       TemplateType = s.Key,
                       PriorToExpiry = s.Where(m => m.TemplateCategory == TemplateCategory.PriorToExpiry).Count(),
                       OnExpiry = s.Where(m => m.TemplateCategory == TemplateCategory.OnExpiry).Count(),
                       AfterExpiry = s.Where(m => m.TemplateCategory == TemplateCategory.AfterExpiry).Count()
                   }).ToList();

            }

            //var data = result.Where(m => m.TemplateCategory == TemplateCategory.AfterExpiry).GroupBy(g => g.TemplateType)
            //    .Select(s => new Notification
            //    {
            //        TemplateType = s.Key,
            //        PriorToExpiry = s.Where(m => m.TemplateCategory == TemplateCategory.PriorToExpiry).Count(),
            //        OnExpiry = s.Where(m => m.TemplateCategory == TemplateCategory.OnExpiry).Count(),
            //        AfterExpiry = s.Where(m => m.TemplateCategory == TemplateCategory.AfterExpiry).Count()
            //    });

            return notifications;
        }

    }
    public class OrganizationCountGraph
    {
        public string OrganizationType { get; set; }
        public int Value { get; set; }
    }
    public class GraphData
    {
        public string label { get; set; }
        public int value { get; set; }
    }

    public class LogCategoryViewModel
    {

        public int LogId { get; set; }
        public string Action { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? DocumentCategoryId { get; set; }
        public string DocumentCategoryDescription { get; set; }
        public string DocumentCategoryName { get; set; }
        public bool? IsEnabled { get; set; }
        public int? ServiceId { get; set; }
        public string ServiceName { get; set; }

    }

    public class LogSubCategoryViewModel
    {

        public int LogId { get; set; }
        public string Action { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? DocumentSubCategoryId { get; set; }
        public int? DocumentCategoryId { get; set; }
        public string DocumentSubCategoryName { get; set; }
        public string SubCategoryDescription { get; set; }
        public bool? IsEnabled { get; set; }
        public string DocumentCategoryName { get; set; }
    }
    public class LogSubSubCategoryViewModel
    {

        public int LogId { get; set; }
        public string Action { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? DocumentSubSubCategoryId { get; set; }
        public string SubSubCategoryDescription { get; set; }
        public string SubDocumentCategoryName { get; set; }
        public int? DocumentSubCategoryId { get; set; }
        public bool? IsEnabled { get; set; }
        public string DocumentSubCategoryName { get; set; }

    }

    public class Entity
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
        public DateTime DateOfExpiry { get; set; }
        public int UserId { get; set; }
        public TemplateType TemplateType { get; set; }
        public int TemplateId { get; set; }
        public string CustomerName { get; set; }
        public TemplateCategory TemplateCategory { get; set; }
    }

    public enum TemplateCategory
    {
        OnExpiry,
        PriorToExpiry,
        AfterExpiry
    }

    public enum TemplateType
    {
        General,
        Insurance,
        Probation,
        Document
    }

    public class Notification
    {
        public TemplateType TemplateType { get; set; }
        public int PriorToExpiry { get; set; }
        public int OnExpiry { get; set; }
        public int AfterExpiry { get; set; }
    }

    public class NotificationModel
    {
        public int UserId { get; set; }
        public int FlatForNotification { get; set; }
        public int RoleId { get; set; }
        public int DepartmentId { get; set; }
        public int OrganizationId { get; set; }
    }

}
