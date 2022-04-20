using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VirtualAdvocate.Models
{
    public class DocumentCategoryModel
    {
        [Required(ErrorMessage = "Please Select Organization")]
        public int ServiceId { get; set; }
     
        public int DocumentCategoryId { get; set; }
        [Required(ErrorMessage ="Please Enter Category Name")]
        public string DocumentCategoryName { get; set; }
        public string DocumentCategoryDescription { get; set; }
        public IEnumerable<OptionsModel> getAllServices { get; set; }
        [DataType(DataType.ImageUrl)]
        public string ImagePath { get; set; }
        public bool IsEnabled { get; set; }

    }
    public class DocumentSubCategoryModel
    {
        public int DocumentSubCategoryId { get; set; }
        [Required(ErrorMessage ="Please Select Category Name")]
        public int DocumentCategoryId { get; set; }
        [Required(ErrorMessage = "Please Enter Sub Document Category Name")]
        public string DocumentSubCategoryName { get; set; }
        public string DocumentSubCategoryDescription { get; set; }
        public IEnumerable<OptionsModel> getAllCategory { get; set; }
        public string ImagePath { get; set; }
    }
    public class OptionsModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
    public class DocumentSubSubCategoryModel
    {
        public int DocumentSubSubCategoryId { get; set; }
        [Required(ErrorMessage = "Please Select Category Name")]
        public int DocumentSubCategoryId { get; set; }
        [Required(ErrorMessage = "Please Enter Sub Document Category Name")]
        public string DocumentSubCategoryName { get; set; }
        public string DocumentSubCategoryDescription { get; set; }
        public IEnumerable<OptionsModel> getAllSubCategory { get; set; }
        public string ImagePath { get; set; }
    }
    public class DocumentTemplateListModel
    {
        public int TemplateId { get; set; }        
        public string DocumentCategory { get; set; }
        public string DocumentType { get; set; }
        public string AssociatedDocument { get; set; }
        public string DocumentFileName { get; set; }
        public decimal? Cost { get; set; }
        public string TemplateName { get; set; }
        public int? AssociatedDocumentId { get; set; }
        public bool IsEnabled { get; set; }
        public int ServiceId { get; set; }
        public int customerId { get; set; }
        public string customername { get; set; }
        public int? DocumentSubCategoryId { get; set; }
        public int? DocumentSubSubCategoryId { get; set; }
        public string DocumentSubCategoryName { get; set; }
        public string DocumentSubSubCategoryName { get; set; }
        public string DepartmentName { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
    }

    public class DocumentUploadModel
    {
        public int? TemplateId { get; set; }
        [Required(ErrorMessage = "Please Select Category Name")]
        public int DocumentCategoryId { get; set; }             
        public int? DocumentSubCategoryId { get; set; }
        public int? DocumentSubSubCategoryId { get; set; }
        [Required(ErrorMessage = "Please Enter Document Type")]
        public string DocumentType { get; set; }
        [Required(ErrorMessage = "Please Enter Document Title")]
        public string DocumentTitle { get; set; }
        public string DocumentDescription { get; set; }
        [Required(ErrorMessage = "Please Enter Template Cost ")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid Price/Only 2 Digits Allowed For After Decision Point")]
        public decimal Cost { get; set; }        
        public string TemplateName { get; set; }      
        [Required(ErrorMessage = "Please Upload Template")]
        [ValidateFile]
        public HttpPostedFileBase TemplateFile { get; set; }
        public int? AssociateTemplateId { get; set; }
        public IEnumerable<OptionsModel> getAllCategory { get; set; }
        public IEnumerable<OptionsModel> getAllSubCategory { get; set; }
        public IEnumerable<OptionsModel> getAllSubSubCategory { get; set; }
        public IEnumerable<OptionsModel> getDocumentList { get; set; }
        public IEnumerable<OptionsModel> getDepartmentlist { get; set; }
        public bool Mandatory { get; set; }       
       
        public int[] AssociateTemplateIds { get; set; }
        public MultiSelectList AssociateTemplateList { get; set; }
        public string OrderIds { get; set; }
        public IEnumerable<GetAssociatedDocuments_Result> associatedTemplate { get; set; }
        public int DepartmentID { get; set; }
    }


    public class EditDocumentUploadModel
    {
        public int? TemplateId { get; set; }
        [Required(ErrorMessage = "Please Select Category Name")]
        public int DocumentCategoryId { get; set; }
        public int? DocumentSubCategoryId { get; set; }
        public int? DocumentSubSubCategoryId { get; set; }
        [Required(ErrorMessage = "Please Enter Document Type")]
        public string DocumentType { get; set; }
        [Required(ErrorMessage = "Please Enter Document Title")]
        public string DocumentTitle { get; set; }
        public string DocumentDescription { get; set; }
        [Required(ErrorMessage = "Please Enter Template Cost ")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid Price/Only 2 Digits Allowed For After Decision Point")]
        public decimal Cost { get; set; }
        public string TemplateName { get; set; }               
        public HttpPostedFileBase TemplateFile { get; set; }
        public int? AssociateTemplateId { get; set; }
        public IEnumerable<OptionsModel> getAllCategory { get; set; }
        public IEnumerable<OptionsModel> getAllSubCategory { get; set; }
        public IEnumerable<OptionsModel> getAllSubSubCategory { get; set; }
        public IEnumerable<OptionsModel> getDocumentList { get; set; }
        public IEnumerable<OptionsModel> getDepartmentlist { get; set; }
        public bool Mandatory { get; set; }
        public int[] AssociateTemplateIds { get; set; }
        public MultiSelectList AssociateTemplateList { get; set; }
        public string OrderIds { get; set; }
        public int[] SelectedOrderIds { get; set; }
        public IEnumerable<GetAssociatedDocuments_Result> associatedTemplate { get; set; }
        public int DepartmentID { get; set; }

    }

    public class ValidateFileAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            int MaxContentLength = 1024 * 1024 * 5; //5 MB
            string[] AllowedFileExtensions = new string[] { ".doc", ".docx" };

            var file = value as HttpPostedFileBase;

            if (file == null)
                return false;
            else if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
            {
                ErrorMessage = "Please Upload Your Template of type: " + string.Join(", ", AllowedFileExtensions);
                return false;
            }
            else if (file.ContentLength > MaxContentLength)
            {
                ErrorMessage = "Your Document Is Too Large, Maximum Allowed Size Is : " + (MaxContentLength / 1024).ToString() + "MB";
                return false;
            }
            else
                return true;
        }
    }

    public class TemplateKeywordModel
    {
        //public int TemplateKeyId { get; set; }
        //[Required(ErrorMessage ="Please Enter Template Key")]
        //[Remote("CheckTemplateKey", "DocumentManagement",ErrorMessage ="Key Value Already Exists. Please Enter Different Key Value")]
        //public string TemplateKeyValue { get; set; }
        //[Required(ErrorMessage = "Please Enter Template Key Label")]
        //public string TemplateKeyLabels { get; set; }
        //public string TemplateKeyDescription { get; set; }
        //public bool IsEnabled { get; set; }
        public int? ClonedFrom { get; set; }
        public int TemplateKeyId { get; set; }
        [Required(ErrorMessage = "Please Enter Template Key")]
        [Remote("CheckTemplateKey", "DocumentManagement", ErrorMessage = "Key Value Already Exists. Please Enter Different Key Value")]
        public string TemplateKeyValue { get; set; }
        [Required(ErrorMessage = "Please Enter Template Key Label")]
        public string TemplateKeyLabels { get; set; }
        public string TemplateKeyDescription { get; set; }

        [DisplayName("Key Category")]
        public int TemplateKeyCategory { get; set; }

        public bool IsEnabled { get; set; }
        public IEnumerable<OptionsModel> getTemplateKeyCategory { get; set; }
        public bool MultipleKeys { get; set; }
        public bool SecurityCheck { get; set; }
        public bool TextArea { get; set; }
        public bool BigTextArea { get; set; }
        public bool Selected { get; set; }
        public int Order { get; set; }
        public bool IsAssetName { get; set; }

     

        public IEnumerable<OptionsModel> getTemplateKeys { get; set; }
    }

    public class EditTemplateKeywordModel
    {
        //public string category { get; set; }
        //public string categoryselected { get; set; }
        //public int TemplateKeyId { get; set; }       
        //public string TemplateKeyValue { get; set; }
        //[Required(ErrorMessage = "Please Enter Template Key Label")]
        //public string TemplateKeyLabels { get; set; }
        //public string TemplateKeyDescription { get; set; }
        //public bool Multiplekeys { get; set; }
        //public IEnumerable<OptionsModel> getAllCategory { get; set; }
        //public string DocumentCategoryId { get; set; }
        public int? ClonedFrom { get; set; }
        public int TemplateKeyId { get; set; }
        public string TemplateKeyValue { get; set; }
        [Required(ErrorMessage = "Please Enter Template Key Label")]
        public string TemplateKeyLabels { get; set; }
        public string TemplateKeyDescription { get; set; }
        public IEnumerable<OptionsModel> getTemplateKeyCategory { get; set; }
        public bool MultipleKeys { get; set; }

        public int TemplateKeyCategory { get; set; }
        public bool SecurityCheck { get; set; }
        public bool TextArea { get; set; }
        public bool BigTextArea { get; set; }
        public bool IsAssetName { get; set; }


        public IEnumerable<OptionsModel> getTemplateKeys { get; set; }
    }

    public class FilledFormDetailModel
    {
        public int RowId { get; set; }
        public string DocumentTitle { get; set; }       
        public decimal? Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FilledTemplateName { get; set; }
        public bool PaidStatus { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public int? OrgId { get; set; }
        public string CustomerName { get; set; }

    }

    public class CoverLetterModel
    {
        [Required(ErrorMessage = "Please Enter The Customer Name")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Please Enter The Bank Name")]
        public string BankName { get; set; }
        [Required(ErrorMessage = "Please Enter The Bank Address")]
        public string BankAddress { get; set; }
        public int UserId { get; set; }
        public int TemplateId { get; set; }
    }

    public class CustomerDetailsModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage ="Please Enter Customer Name")]
        public string CustomerName { get; set; }
      
        public string AccountNumber { get; set; }
        [Required(ErrorMessage = "Please Enter Address Details")]
        public string Address { get; set; }
        public int OrganizationId { get; set; }
       
        public string BankName { get; set; }
        public bool? IsEnabled { get; set; }
        [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
        [Remote("CheckCustomerExist", "DocumentManagement", ErrorMessage = "Customer Email Address Already Exists")]
        public string EmailAddress { get; set; }
        public IEnumerable<CustomerTemplateDetail> extraFields { get; set; }
    }

    public class ManualInvoiceModel
    {
       
        public decimal TotalAmount { get; set; }
        public List<ManualInvoiceListModel> getManualList { get; set; }
        public int? CustomerId { get; set; }
        public int? GroupId { get; set; }
        public float? Vat { get; set; }

    }
    public class ManualInvoiceListModel
    {
        public string DocumentTitle { get; set; }
        [Required(ErrorMessage = "Please Enter Valid Price")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid Price")]
        public decimal DocumentCost { get; set; }
        [Required(ErrorMessage ="Not Valid")]
        [Range(1,100,ErrorMessage ="Not Valid")]
        public int Quantity { get; set; }
        public int TemplateId { get; set; }
    }
    public class CustomerFilledFormDetailModel
    {
        public int RowId { get; set; }
        public string DocumentTitle { get; set; }
        public decimal? Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FilledTemplateName { get; set; }
        public bool PaidStatus { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public int? OrgId { get; set; }

    }

    public class PreviewClauses
    {
        public long ClauseID  { get; set; }
        public string Clause { get; set; }
      

    }
    public class ClouseModel
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "Please enter Clouse")]
        public string Clouse1 { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public IList<DocumentCategory> getAllCategory { get; set; }
        public int[] SelectedGroups { get; set; }
    }

    public class CustomerTemplateDetailModel
    {
        public int id { get; set; }
        public Nullable<int> CustID { get; set; }
        [Required(ErrorMessage = "Please enter Key Name")]
        public string FieldName { get; set; }
        [Required(ErrorMessage = "Please enter Key Value")]
        public string FieldValue { get; set; }
        public Nullable<int> FieldID { get; set; }
    }

    public class DepartmentModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter Department")]
        public string Department { get; set; }
        public string Description { get; set; }

    }

    public class RoleModel
    {
        public int Id { get; set; }      
        public string Role { get; set; }
    }

    public class EditUserRoleModel
    {
        public int userId { get; set; }
        public IList<OptionsModel> Roles { get; set; }
        public int UserRole { get; set; }
    }

    public class BulkDocumentTemplateListModel
    {
        public int TemplateId { get; set; }
        public string DocumentCategory { get; set; }
        public string DocumentType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public decimal? Cost { get; set; }
        public string TemplateName { get; set; }     
        public string DocumentSubCategoryName { get; set; }
        public string DocumentSubSubCategoryName { get; set; }
        public int BulkTemplateID { get; set; }
        public int DocumentSubCategoryId { get; set; }
        public int DocumentSubSubCategoryId { get; set; }

    }
}