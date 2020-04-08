using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PermissionManagement.Model
{

    public class FinacleRole
    {
        public string UserID { get; set; }
        public string ApplicationName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }

    }
    public class Role
    {
        private Guid _roleId;
        public Guid RoleId
        {
            get { return _roleId; }
            set { _roleId = value; }
        }

        private string _roleName;
        public string RoleName
        {
            get { return _roleName; }
            set { _roleName = value; }
        }

        private string _roleDescription;
        public string Description
        {
            get { return _roleDescription; }
            set { _roleDescription = value; }
        }
        public Int64 RowVersionNo2 { get; set; }

        public bool IsDeleted { get; set; }
    }

    //public class RoleProfile
    //{
    //    public Guid RoleId { get; set; }
    //    public Guid ProfileId { get; set; }
    //    public Int64 RowVersionNo2 { get; set; }
    //}

    //public class Profile
    //{
    //    public Guid ProfileId { get; set; }
    //    public string ProfileName { get; set; }
    //    public string ProfileDescription { get; set; }
    //    public Int64 RowVersionNo2 { get; set; }
    //}

    public class Module
    {
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }
        public bool IsModule { get; set; }
        public bool IsAdmin { get; set; }
        public Int64 RowVersionNo2 { get; set; }
    }

    public class RoleModuleAccess
    {
        public Guid RoleId { get; set; }
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool? CreateAccess { get; set; }
        public bool? EditAccess { get; set; }
        public bool? DeleteAccess { get; set; }
        public bool? ViewAccess { get; set; }
        public bool? VerifyAccess { get; set; }
        public bool? MakeOrCheckAccess { get; set; }
        public Int64 RowVersionNo2 { get; set; }
    }

    public class RoleViewModel
    {
        public Role CurrentRole { get; set; }
        public IEnumerable<RoleModuleAccess> ModuleAccessList { get; set; }
        public bool IsDeleted { get; set; }
    }

    //public class RoleViewModel
    //{
    //    public Role CurrentRole { get; set; }
    //    public List<Profile> AvailableProfiles { get; set; }
    //    public List<Profile> SelectedProfiles { get; set; }

    //    public Guid[] AvailableSelected { get; set; }
    //    public Guid[] RequestedSelected { get; set; }
    //    public string SavedRequested { get; set; }
    //}

    //public class RoleProfileModel
    //{
    //    public Role CurrentRole { get; set; }
    //    public List<Profile> SelectedProfiles { get; set; }
    //}

    public class RoleModuleAccessAggregate
    {
        public Guid RoleId { get; set; }
        public Guid ModuleId { get; set; }
        public bool IsAdmin { get; set; }
        public string ModuleName { get; set; }
        public string AggregateAccess { get; set; }
    }

    [DataContract]
    public class Authentication2FARequest
    {
        [DataMember]
        public string TokenCode { get; set; }
        [DataMember]
        public string UserID { get; set; }
    }

    [DataContract]
    public class AuthenticationRequest
    {
        [DataMember]
        public string UserID { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string TokenCode { get; set; }
    }

    [DataContract]
    public class BaseResponse
    {
        [DataMember]
        public string ResponseCode { set; get; }

        [DataMember]
        public string ResponseDescription { set; get; }
    }

    [DataContract]
    public class AuthenticationResponse : BaseResponse
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Role { get; set; }

        [DataMember]
        public string BranchCode { get; set; }

        [DataMember]
        public string TellerTillAccount { get; set; }
    }


    [DataContract]
    public class Authentication2FAResponse : BaseResponse
    {

    }

}
