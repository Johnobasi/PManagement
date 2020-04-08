using System;

namespace PermissionManagement.Repository
{
     public interface IDatastoreValidationRepository
    {

        bool IsUsernameAlreadyExist(string username);
        bool IsEmailAlreadyExist(string email, string Username);
        bool IsRoleNameExist(Guid roleID, string roleName);
     }

}
