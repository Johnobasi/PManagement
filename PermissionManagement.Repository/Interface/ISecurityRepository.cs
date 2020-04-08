using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionManagement.Model;

namespace PermissionManagement.Repository
{
   public interface ISecurityRepository
    {
        User GetUser(string username);
        //User GetUserByUsername(string Username);
        User GetUserBySessionId(string sessionId);
        void SignOut(string sessionId);
        void UpdateLogInSucceed(string username, string sessionId);
        string VerifyUsernameAndEmailMatch(string username, string email);
        void UpdateRenewSucceed(Model.User userItem);
        void ResetUserPassword(string username, string newPassword, bool isReset);
        string GetUserPassword(string userName);
        void UpdateBadPasswordCount(string username, bool lockAccount);
        void AddUser(User userToAdd);
        int EditUser(User userToEdit);
        void DeleteUser(string id, string optionTaken);
        bool ActivateAccount(string activationCode);
        UserListingResponse GetUserList(PagerItemsII parameter);
        //IEnumerable<Profile> GetProfileList();
        IEnumerable<RoleModuleAccess> GetModuleAccessList();
        RoleViewModel GetRole(Guid roleId);
        void AddRole(RoleViewModel roleToAdd);
        int EditRole(RoleViewModel roleToEdit);
        bool DeleteRole(Guid id);
        IEnumerable<Role> GetRoleList();
       // ProfileViewModel GetRole(Guid id);
       // void AddRole(RoleProfileModel roleToAdd);
       // int EditRole(RoleProfileModel roleToEdit);
        IEnumerable<RoleModuleAccessAggregate> GetRoleModuleAccessList();
        void LockUnlockAccount(string id, bool isLocked);
        void AddUserForSessionMgmt(User userDetails);
        void UpdateUserRole(string username, Guid roleId);
        void UpdateUserRoleUserBranch(string username, Guid roleId, string branchCode);
    }
}
