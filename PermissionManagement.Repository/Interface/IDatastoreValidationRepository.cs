using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionManagement.Model;


namespace PermissionManagement.Repository
{
     public interface IDatastoreValidationRepository
    {

        bool IsUsernameAlreadyExist(string username);
        bool IsEmailAlreadyExist(string email, string Username);
        bool IsRoleNameExist(Guid roleID, string roleName);
     }

}
