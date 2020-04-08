using PermissionManagement.Model;
using PermissionManagement.Validation;
using System;
using System.Collections.Generic;

namespace PermissionManagement.Services
{
    public interface ISecurityService
    {
        User GetUser(string username);
        User GetUserBySessionId(string sessionId);
        //User GetUserByUsername(string Username);
        AuthenticationDataDto SignIn(string username, string password, bool sessionBased, ref ValidationStateDictionary states);
        AuthenticationDataDto Renew(string sessionId);
        void SignOut(string sessionId);
        void UpdateLogInSucceed(string username, string sessionId);
        void ResetPassword(string username, string email, ref ValidationStateDictionary states);
        void UpdateRenewSucceed(User userItem);
        void UserChangePassword(string userName, string oldPassword, string newPassword, string confirmNewPassword, ref ValidationStateDictionary states);
        void AddUser(User userToAdd, ref ValidationStateDictionary states);
        int EditUser(User userToEdit, ref ValidationStateDictionary states);
       // bool ActivateAccount(string activationCode);
        UserListingResponse GetUserList(PagerItemsII parameter);
       // IEnumerable<Profile> GetProfileList();
        IEnumerable<RoleModuleAccess> GetModuleAccessList();
        RoleViewModel GetRole(Guid roleId);
        void AddRole(RoleViewModel roleToAdd, ref ValidationStateDictionary states);
        int EditRole(RoleViewModel roleToEdit, ref ValidationStateDictionary states);
       IEnumerable<Role> GetRoleList();
       // RoleProfileModel GetRole(Guid id);
        //void AddRole(RoleProfileModel roleToAdd, ref ValidationStateDictionary states);
        //int EditRole(RoleProfileModel roleToEdit, ref ValidationStateDictionary states);
        IEnumerable<RoleModuleAccessAggregate> GetRoleModuleAccessList();
        void LockUnlockAccount(string id, bool isLocked);
        void DeleteUser(string id, string optionTaken);
        string GetUserPassword(string username);
        void DeleteRole(Guid id, ref ValidationStateDictionary states);
        void AddUserForSessionMgmt(User userDetails);
        void UpdateUserRole(string username, Guid roleId);
        void UpdateUserRoleUserBranch(string username, Guid roleId, string branchCode);
        List<ExportDto> GetUserListForExcel(string searchKey);

    }
}
