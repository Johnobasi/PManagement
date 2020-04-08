using System;

namespace PermissionManagement.Model
{
    public class ExpiredUserListingDto
    {
        
            //public string UserRole { get; set; }
            //public Guid UserRoleID { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public bool IsLockedOut { get; set; }
            public string AccountType { get; set; }
            public string StaffPosition { get; set; }
            public DateTime ? AccountExpiryDate { get; set; }
            public DateTime? CreationDate { get; set; }
            public DateTime? LastLogInDate { get; set; }
         
    }

    public class UserListingReports
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CreationDate { get; set; }
    }
}