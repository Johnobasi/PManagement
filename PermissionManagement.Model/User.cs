using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace PermissionManagement.Model
{
    public class User
    {
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _username;
        private string _password;
        private string _confirmPassword;
        private Role _userRole;
        private bool _isLockedOut;
        private string _telephone;
        private System.DateTime? _creationDate;
        private System.DateTime? _lastLogInDate;
        private System.DateTime? _lastActivityDate;
        private bool _isOnline;
        private Guid? _currentSessionId;
        private string _accountType;
        private string _branchID;
        private string _approvalStatus;
        private long _approvalLogID;
        private bool _isDeleted;
       
        public Guid RoleId { get; set; }

        private int _badPasswordCount;
        [XmlIgnore()]
        public Role UserRole
        {
            get { return _userRole; }
            set { _userRole = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { _confirmPassword = value; }
        }

        public bool IsLockedOut
        {
            get { return _isLockedOut; }
            set { _isLockedOut = value; }
        }
        [RegularExpression("^\\+{0,1}[0-9]*$", ErrorMessage = "Telephone number must be numeric")]
        public string Telephone
        {
            get { return _telephone; }
            set { _telephone = value; }
        }

        public System.DateTime? CreationDate
        {
            get { return _creationDate; }
            set { _creationDate = value; }
        }

        public System.DateTime? LastLogInDate
        {
            get { return _lastLogInDate; }
            set { _lastLogInDate = value; }
        }

        public System.DateTime? LastActivityDate
        {
            get { return _lastActivityDate; }
            set { _lastActivityDate = value; }
        }

        public bool IsOnline
        {
            get { return _isOnline; }
            set { _isOnline = value; }
        }

        public System.Guid? CurrentSessionId
        {
            get { return _currentSessionId; }
            set { _currentSessionId = value; }
        }

        public int BadPasswordCount
        {
            get { return _badPasswordCount; }
            set { _badPasswordCount = value; }
        }

        public string AccountType
        {
            get { return _accountType; }
            set { _accountType = value; }
        }

        public string BranchID
        {
            get { return _branchID; }
            set { _branchID = value; }
        }

        public string ApprovalStatus
        {
            get { return _approvalStatus; }
            set { _approvalStatus = value; }
        }

        public long ApprovalLogID
        {
            get { return _approvalLogID; }
            set { _approvalLogID = value; }
        }

        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }

        public bool IsFirstLogIn { get; set; }

        public Int64 RowVersionNo2 { get; set; }
        public string InitiatedBy { get; set; }
        public string ApprovedBy { get; set; }

        [Display(Name = "Staff Position")]
        public string StaffPosition { get; set; }

        [Display(Name = "Title")]
        public string Initial { get; set; }

        public bool IsDormented { get; set; }
        public DateTime? AccountExpiryDate { get; set; }
        public bool IsAccountExpired { get; set; }
    }

    public class UserListingResponse
    {
        public IList<UserListingDto> UserListingResult { get; set; }
        public PagerItems PagerResource { get; set; }
    }

    public class UserListingDto
    {
        public string UserRole { get; set; }
        public Guid UserRoleID { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsLockedOut { get; set; }
        public string AccountType { get; set; }
        public string ApprovalStatus { get; set; }
        public bool IsDeleted { get; set; }
        public string InitiatedBy { get; set; }
    }

    public class UserMailDto
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
    }

    public class StaffInitialDTO
    {
        public string StaffInitialID { get; set; }
        public string Description { get; set; }
    }

    public class AccountTypeDTO
    {
        public string AccountTypeID { get; set; }
        public string Description { get; set; }
    }

    public class ApprovalStatusDTO
    {
        public string ApprovalStatusID { get; set; }
        public string Description { get; set; }
    }
    public class ExportDto
    {
        public string UserRole { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsLockedOut { get; set; }
        public string AccountType { get; set; }
        public string ApprovalStatus { get; set; }
        public bool IsDeleted { get; set; }
        public string InitiatedBy { get; set; }
    }

}
