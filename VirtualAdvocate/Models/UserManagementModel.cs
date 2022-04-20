using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualAdvocate.Models
{
    public class UserManagementModel
    {
        public int UserId { get; set; }       
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string RoleDescription { get; set; }
    }
    public class AllUserList
    {        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string RoleDescription { get; set; }
        public int UserID { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsLocked { get; set; }
    }
    public class AllOrganizationList
    {
        public string OrganizationName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Services { get; set; }
     
        public int OrganizationId { get; set; }
        public bool IsEnabled { get; set; }
        public int? userAccountType { get; set; }
        public DateTime? CreatedDate { get; set; }
         public bool IsLocked { get; set; }
       
    }

    public class OrganizationUserList
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string RoleDescription { get; set; }
        public string Services { get; set; }
        public int UserID { get; set; }
        public bool IsEnabled { get; set; }
        public int OrgId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsLocked { get; set; }
    }
    public class OrgUsersList
    {
        public int OrgId { get; set; }
        public IEnumerable<OrganizationUserList> getCompanyUserList { get; set; }
    }
}