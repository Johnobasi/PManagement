using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using PermissionManagement.Model;
using Dapper;

namespace PermissionManagement.Repository
{
    public class DatastoreValidationRepository : IDatastoreValidationRepository
    {
        private readonly IDbConnection context;
        private readonly DapperContext dapperContext;

        public DatastoreValidationRepository(DapperContext dbContext)
        {
            dapperContext = dbContext;
            context = dbContext.Connection;
        }

       public bool IsRoleNameExist(Guid roleID, string roleName)
        {
            var sql = "Select Count(RoleName) From Role c Where c.RoleName = @RoleName And c.RoleID <> @RoleID";
            var count = context.Query<Int64>(sql, new { RoleName = roleName, RoleID = roleID }).First();
            return count > 0;
        }
        public bool IsUsernameAlreadyExist(string username)
        {
            var sql = string.Empty;
           // if (string.IsNullOrEmpty(staffID))
               sql = "Select Count(Username) From [User] Where Username = @Username";
           // else
               // sql = "Select Count(Username) From [User] Where Username = @Username And StaffID <> @StaffID";

            var count = context.Query<Int64>(sql, new { Username = username}).First();
            return count > 0;
        }

        public bool IsEmailAlreadyExist(string email, string Username)
        {
            var sql = string.Empty;
            if (string.IsNullOrEmpty(Username))
                sql = "Select Count (Email) From [User] where Email = @Email";
            else
                sql = "Select Count(Email) From [User] Where Email = @Email And Username <> @Username";
            var count = context.Query<Int64>(sql, new { Email = email, Username = Username }).First();
            return count > 0;
        }
    }
}
