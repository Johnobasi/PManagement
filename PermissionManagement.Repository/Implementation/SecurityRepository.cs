using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionManagement.Model;
using System.Data;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System.Data.SqlClient;
using System.Web.Mvc;
using Dapper;

namespace PermissionManagement.Repository
{
   public class SecurityRepository : ISecurityRepository
    {
        private readonly IDbConnection context;
        private readonly DapperContext dapperContext;
       
        
        public SecurityRepository(DapperContext dbContext)
        {
            dapperContext = dbContext;
            context = dbContext.Connection;
        }
        public void AddUserForSessionMgmt(User userDetails)
        {
            var username = context.Query<string>("SELECT Username FROM [User] WHERE Username = @Username", new { Username = userDetails.Username }).FirstOrDefault();
            if (string.IsNullOrEmpty(username))
            {
                var dbTransaction = dapperContext.GetTransaction();
                var success = context.Insert<User>(userDetails, databaseTableName: "[User]", excludeFieldList: new string[] { "ConfirmPassword", "UserRole", "RoleId" }, transaction: dbTransaction);
                var sql = "INSERT UsersInRoles (Username, RoleId, IsDeleted) VALUES(@Username, @RoleId, @IsDeleted) ";
                context.Execute(sql, new { Username = userDetails.Username, userDetails.UserRole.RoleId, userDetails.IsDeleted }, transaction: dbTransaction);
                dapperContext.CommitTransaction();
            }
        }

        public string GetUserID(string username)
        {
            return context.Query<string>("Select Username From [User] Where Username = @Username and IsDeleted = 0", new { Username = username }).FirstOrDefault();
        }

        public void AddUser(Model.User user)
        {
            AuditRepository ar = new AuditRepository(dapperContext);

            user.CreationDate = Helper.GetLocalDate();
            user.IsLockedOut = false;
            user.IsOnline = false;
            user.IsFirstLogIn = true;
            user.IsDeleted = false;

            IDbTransaction dbTransaction = dapperContext.GetTransaction();

           // user.StaffID = user.Username;  //GetEntityNextID(Constants.SequenceNames.Staff_ID, dbTransaction);
            //user.Username = GetEntityNextID(Constants.SequenceNames.Username, dbTransaction);
            user.IsOnline = true;
            user.IsFirstLogIn = true;
            user.BadPasswordCount = 0;
            user.CurrentSessionId = Helper.GetNextGuid();

            Model.User DummyObject = new User();

            //To handle maker checker functions
            user = ar.MakerCheckerHandller<User>(DummyObject, user, Constants.OperationType.Create, Constants.Modules.UserSetup, user.Username, dbTransaction);
  
            var success = context.Insert<User>(user, databaseTableName: "[User]", excludeFieldList: new string[] { "ConfirmPassword", "UserRole", "RoleId" }, transaction: dbTransaction);
            var sql = "INSERT UsersInRoles (Username, RoleId, IsDeleted) VALUES(@Username, @RoleId, @IsDeleted) ";
            context.Execute(sql, new { Username = user.Username, user.UserRole.RoleId, user.IsDeleted }, transaction: dbTransaction);

            ar.CreateAuditChange(DummyObject, user, dbTransaction,
                new string[] { "Email", "FirstName", "LastName", "Username", "Telephone",
                                "Initial", "ApprovalStatus", "ApprovedBy", "InitiatedBy", 
                                "ApprovalLogID", "IsDeleted", "UserRole.RoleName"
                             }
                         );

            dapperContext.CommitTransaction();
        }

        public void UpdateUserRole(string username, Guid roleId)
        {
            var sql = "UPDATE UsersInRoles SET RoleId = @RoleID WHERE Username = @Username";
            context.Execute(sql, new { Username = username, RoleID = roleId });
        }
        public void UpdateUserRoleUserBranch(string username, Guid roleId, string branchCode)
        {
            var sql = new StringBuilder();
            if (roleId != Guid.Empty)
                sql.AppendLine("UPDATE UsersInRoles SET RoleId = @RoleID WHERE Username = @Username; ");
            
            if (!string.IsNullOrEmpty(branchCode))
                sql.AppendLine("UPDATE [User] SET BranchID = @BranchID WHERE Username = @Username; ");

            context.Execute(sql.ToString(), new { Username = username, RoleID = roleId, BranchID = branchCode });
        }

        public int EditUser(Model.User user)
        {
            AuditRepository ar = new AuditRepository(dapperContext);

            var dbVersion = GetUser(user.Username);
          
            IDbTransaction dbTransaction = dapperContext.GetTransaction();

            //To handle maker checker functions
            //maker checker can return the db version of an object depending on user action
            user = ar.MakerCheckerHandller<User>(dbVersion, user, Constants.OperationType.Edit, Constants.Modules.UserSetup, user.Username, dbTransaction);

            var sql = ("UPDATE [User] SET Email = @Email, FirstName = @FirstName, LastName = @LastName, Username = @Username, Telephone = @Telephone, Initial = @Initial, ApprovalStatus = @ApprovalStatus, ApprovedBy = @ApprovedBy, InitiatedBy = @InitiatedBy, ApprovalLogID = @ApprovalLogID, IsDeleted = @IsDeleted Where Username = @Username AND CONVERT(bigint,RowVersionNo) = @RowVersionNo2;");
            var rowAffected = context.Execute(sql.ToString(), user, transaction: dbTransaction);
            sql = ("UPDATE UsersInRoles SET RoleId = @RoleId, IsDeleted = @IsDeleted WHERE Username = @Username;");
            context.Execute(sql.ToString(), user, transaction: dbTransaction);

            //To Create an Audit Record
            ar.CreateAuditChange(dbVersion, user, dbTransaction,
                new string[] { "Email", "FirstName", "LastName", "Username", "Telephone",
                                "Initial", "ApprovalStatus", "ApprovedBy", "InitiatedBy", 
                                "ApprovalLogID", "IsDeleted", "UserRole.RoleName"
                             }
                         );

            dapperContext.CommitTransaction();

            //if transaction commits, then send notification

            return rowAffected;
        }

        public void UpdateBadPasswordCount(string username, bool lockAccount)
        {
            var sql = ("UPDATE [User] SET BadPasswordCount = BadPasswordCount + 1, IsLockedOut = @IsLockedOut Where Username = @Username");
            context.Execute(sql.ToString(), new { IsLockedOut = lockAccount, Username = username });
        }

        public bool ActivateAccount(string activationCode)
        {
            var sql = ("UPDATE [User] SET IsLockedOut = 0, IsLockedOut = 0 Where Username = @Username");
            var count = context.Execute(sql.ToString(), new { Username = activationCode });
            return count > 0;
        }

        public Model.User GetUser(string username)
        {
            //var sql = "Select *, CONVERT(bigint,RowVersionNo) as RowVersionNo2 From [User] Where Username = @Username; Select r.* From Role r INNER JOIN UsersInRoles u ON r.RoleId = u.RoleId AND u.Username = (Select Username From [User] Where Username = @Username)";
            var sql = "Select *, CONVERT(bigint,RowVersionNo) as RowVersionNo2 From [User] Where Username = @Username  and (IsDeleted = 0 OR (IsDeleted = 1 AND ApprovalStatus = 'Pending')); Select r.* From Role r INNER JOIN UsersInRoles u ON r.RoleId = u.RoleId AND u.Username = (Select Username From [User] Where Username = @Username)";
            User user = null;
            using (var multi = context.QueryMultiple(sql, new { Username = username }))
            {
                user = multi.Read<User>().FirstOrDefault();
                if (user != null)
                {
                    user.UserRole = multi.Read<Role>().FirstOrDefault();
                }
            }
            if (user != null && user.UserRole != null)
                user.RoleId = user.UserRole.RoleId;

            return user;
        }

        public string GetUserPassword(string userName)
        {
            var sql = "SELECT TOP 1 Password FROM [User] WHERE Username = @Username;";
            return context.Query<string>(sql, new { Username = userName }).FirstOrDefault();
        }

        //public Model.User GetUserByUsername(string Username)
        //{
        //    //UPDATE Table1 SET Column1 = @newvalue1, Column2 = @newvalue2 
        //    //WHERE CONVERT(bigint,RowVersionNo) = @RowVersionNo2
        //    //select Id, Value,CONVERT(bigint,RowVersionNo) as RowVersionNo2 from t1

        //    var sql = "Select *, CONVERT(bigint,RowVersionNo) as RowVersionNo2 From [User] Where Username = @Username; Select r.* From Role r INNER JOIN UsersInRoles u ON r.RoleId = u.RoleId AND u.Username = @Username; ";
        //    User user = null;
        //    using (var multi = context.QueryMultiple(sql, new { Username = Username }))
        //    {
        //        user = multi.Read<User>().FirstOrDefault();
        //        if (user != null)
        //        {
        //            user.UserRole = multi.Read<Role>().FirstOrDefault();
        //        }
        //    }
        //    return user;
        //}

        public User GetUserBySessionId(string sessionId)
        {
            var sql = "Select *, CONVERT(bigint,RowVersionNo) as RowVersionNo2 From [User] Where CurrentSessionId = @SessionId AND IsDeleted = 0; Select r.* From Role r INNER JOIN UsersInRoles u ON r.RoleId = u.RoleId AND u.Username = (Select Username From [User] Where CurrentSessionId = @sessionId AND IsDeleted = 0)";
            User user = null;
            using (var multi = context.QueryMultiple(sql, new { SessionId = new Guid(sessionId) }))
            {
                user = multi.Read<User>().FirstOrDefault();
                if (user != null)
                {
                    user.UserRole = multi.Read<Role>().FirstOrDefault();
                }
            }
            return user;
        }

        public void SignOut(string sessionId)
        {
            var sql = "UPDATE [User] SET IsOnline = 0, CurrentSessionId = @NextSession WHERE CurrentSessionId = @SessionId; ";
            context.Execute(sql.ToString(), new { SessionId = new Guid(sessionId), NextSession = Helper.GetNextGuid() });
        }

        public void UpdateLogInSucceed(string username, string sessionId)
        {
            var sql = "UPDATE [User] SET IsOnline = 1, BadPasswordCount = 0, CurrentSessionId = @CurrentSessionId, LastLogInDate = @LastLogInDate, LastActivityDate =@LastActivityDate WHERE Username = @Username";
            context.Execute(sql.ToString(), new { CurrentSessionId = sessionId, LastLogInDate = Helper.GetLocalDate(), LastActivityDate = Helper.GetLocalDate(), Username = username, });
        }

        public void UpdateRenewSucceed(Model.User userItem)
        {
            if (userItem.CurrentSessionId == Guid.Empty || userItem.CurrentSessionId == null)
            {
                var sql = "UPDATE [User] SET IsOnline = 0, CurrentSessionId = @NextSession WHERE Username = @Username";
                context.Execute(sql.ToString(), new { Username = userItem.Username, NextSession = Helper.GetNextGuid() });
            }
            else
            {
                var sql = "UPDATE [User] SET LastActivityDate = @LastActivityDate WHERE Username = @Username";
                context.Execute(sql.ToString(), new { LastActivityDate = userItem.LastActivityDate, Username = userItem.Username });
            }
        }

        public string VerifyUsernameAndEmailMatch(string username, string email)
        {
            var sql = "SELECT FirstName FROM [User] WHERE Username = @Username AND Email = @Email";
            return context.Query<string>(sql, new { Username = username, Email = email }).FirstOrDefault();
        }

        public void ResetUserPassword(string username, string newPassword, bool isReset)
        {
            var sql = "UPDATE [User] SET IsFirstLogIn = @ResetRequired, Password = @Password, BadPasswordCount = 0 WHERE Username = @Username";
            context.Execute(sql.ToString(), new { Password = newPassword, Username = username, ResetRequired = isReset });
        }

        public UserListingResponse GetUserList(PagerItemsII parameter)
        {
            //Better paging query which works for SQL Server 2012 or higher
            //SELECT ID_EXAMPLE, NM_EXAMPLE, DT_CREATE
            //FROM TB_EXAMPLE
            //ORDER BY ID_EXAMPLE
            //OFFSET ((@PageNumber - 1) * @RowspPage) ROWS
            //FETCH NEXT @RowspPage ROWS ONLY;
            var result = new UserListingResponse() { PagerResource = new PagerItems() };

            var orderByField = string.Empty;
            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM (");

            sql.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY ");

            var sortSql = new StringBuilder();

            foreach (var column in parameter.SortColumns)
            {
                sortSql.Append(sortSql.Length > 0 ? "," : "");

                if (column.Data == Constants.SortField.Username)
                {
                    sql.Append("u.Username ");
                    sortSql.Append("Username ");
                }
                else if (column.Data == Constants.SortField.Role)
                {
                    sql.Append("r.RoleName ");
                    sortSql.Append("RoleName ");
                }
                else if (column.Data == Constants.SortField.FirstName)
                {
                    sql.Append("FirstName ");
                    sortSql.Append("FirstName ");
                }
                else if (column.Data == Constants.SortField.LastName)
                {
                    sql.Append("LastName ");
                    sortSql.Append("LastName ");
                }
                else if (column.Data == Constants.SortField.Email)
                {
                    sql.Append("Email ");
                    sortSql.Append("Email ");
                }
                sql.Append(column.SortDirection == 0 ? " asc" : " desc");
                sortSql.Append(column.SortDirection == 0 ? " asc" : " desc");
            }

            var whereClause = string.Empty;
            var filter = string.Empty;
            if (!string.IsNullOrEmpty(parameter.siteSearch))
            {
                filter = parameter.siteSearch.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                filter = string.Format("%{0}%", filter);
                whereClause = (" and ((u.Username LIKE @SearchFilter) OR (FirstName LIKE @SearchFilter) OR (LastName LIKE @SearchFilter) OR ( r.RoleName LIKE @SearchFilter) OR (Email LIKE @SearchFilter)) ");
            }

            sql.AppendLine(") AS NUMBER, u.Username, FirstName, LastName, Email, ApprovalStatus, IsLockedOut, InitiatedBy, u.IsDeleted, ");
            sql.AppendLine("r.RoleId as UserRoleID, r.RoleName as UserRole ");
            sql.AppendLine("From [User] u inner join UsersInRoles ur on ur.Username = u.Username inner join Role r on ur.RoleID = r.RoleID ");
            sql.AppendFormat("WHERE (u.ApprovalStatus = 'Pending' or (u.ApprovalStatus = 'Approved' and u.IsDeleted = 0)) {0}) AS TBL ", whereClause);
            sql.AppendLine();
            sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
            sql.AppendFormat("ORDER BY {0} ", sortSql.ToString());

            //if (!string.IsNullOrEmpty(parameter.siteSearch))
            //{
            //    filter = parameter.siteSearch.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
            //    filter = string.Format("%{0}%", filter);
            //    whereClause = (" WHERE ((u.Username LIKE @SearchFilter) OR (FirstName LIKE @SearchFilter) OR (LastName LIKE @SearchFilter) OR ( r.RoleName LIKE @SearchFilter) OR (Email LIKE @SearchFilter)) ");
            //}

            //sql.AppendLine(") AS NUMBER, u.Username, FirstName, LastName, Email, ApprovalStatus, IsLockedOut, ");
            //sql.AppendLine("r.RoleId as UserRoleID, r.RoleName as UserRole ");
            //sql.AppendLine("From [User] u inner join UsersInRoles ur on ur.Username = u.Username inner join Role r on ur.RoleID = r.RoleID ");
            //sql.AppendFormat("{0}) AS TBL ", whereClause);
            //sql.AppendLine();
            //sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
            //sql.AppendFormat("ORDER BY {0} ", sortSql.ToString());

            result.PagerResource.ResultCount = (int)context.Query<Int64>(
                string.Format("Select Count(u.Username) From [User] u INNER JOIN UsersInRoles ur on ur.Username = u.Username INNER JOIN Role r on ur.RoleID = r.RoleID {0}", whereClause),
                new
                {
                    StartPage = ((parameter.PageNumber - 1) * parameter.PageSize) + 1,
                    EndPage = (parameter.PageNumber * parameter.PageSize),
                    SearchFilter = filter
                }).First();

            result.UserListingResult = context.Query<UserListingDto>(sql.ToString(),
                new
                {
                    StartPage = ((parameter.PageNumber - 1) * parameter.PageSize) + 1,
                    EndPage = (parameter.PageNumber * parameter.PageSize),
                    SearchFilter = filter
                }).ToList();

            return result;
        }

        public IEnumerable<Role> GetRoleList()
        {
            return context.Query<Role>("Select * From Role ").ToList();
        }

        public IEnumerable<RoleModuleAccess> GetModuleAccessList()
        {
            var sql = new StringBuilder();
            sql.AppendLine("select ModuleId, ModuleName, NULL AS CreateAccess, NULL AS EditAccess, NULL AS DeleteAccess, NULL AS ViewAccess, NULL AS VerifyAccess, NULL AS MakeOrCheckAccess, IsModule From Module order by IsModule DESC, ModuleName ASC; ");
            return context.Query<RoleModuleAccess>(sql.ToString()).ToList();
        }

        public RoleViewModel GetRole(Guid roleId)
        {
            var sql = new StringBuilder();
            sql.AppendLine("Select RoleId, RoleName, Description, CONVERT(bigint,RowVersionNo) as RowVersionNo2 From Role Where RoleId = @RoleId; ");
            sql.AppendLine("select * FROM ");
            sql.AppendLine("( ");
            sql.AppendLine("select m.ModuleId, m.ModuleName, p.RoleId, CreateAccess, EditAccess, DeleteAccess,  VerifyAccess, MakeOrCheckAccess, ");
            sql.AppendLine("ViewAccess, IsModule From Module m inner join rolemoduleaccess p on m.ModuleId = p.ModuleId  ");
            sql.AppendLine("Where p.RoleId = @RoleID  ");
            sql.AppendLine("union ");
            sql.AppendLine("select ModuleId, ModuleName, NULL AS RoleId,  NULL AS CreateAccess, NULL AS EditAccess, NULL AS DeleteAccess,  ");
            sql.AppendLine("NULL AS VerifyAccess, NULL AS ViewAccess, NULL AS MakeOrCheckAccess, IsModule From Module ");
            sql.AppendLine("where ModuleID NOT IN (SELECT ModuleID FROM RoleModuleAccess Where roleId = @RoleID))  ");
            sql.AppendLine("as tt order by IsModule DESC, ModuleName ASC; ");

            RoleViewModel model = new RoleViewModel();
            using (var multi = context.QueryMultiple(sql.ToString(), new { RoleId = roleId }))
            {
                model.CurrentRole = multi.Read<Role>().FirstOrDefault();
                if (model.CurrentRole != null)
                {
                    model.ModuleAccessList = multi.Read<RoleModuleAccess>().ToList();
                }
            }
            return model;
        }

        public void AddRole(RoleViewModel roleToAdd)
        {
            roleToAdd.IsDeleted = false;
           
            foreach (var m in roleToAdd.ModuleAccessList)
            {
                m.RoleId = roleToAdd.CurrentRole.RoleId;
            }

            var t = dapperContext.GetTransaction();

            var success = context.Insert<Role>(roleToAdd.CurrentRole, transaction: t);
            success = context.InsertMany<RoleModuleAccess>(roleToAdd.ModuleAccessList.Where
                                                        (m => m.CreateAccess == true || m.DeleteAccess == true
                                                            || m.EditAccess == true || m.ViewAccess == true
                                                            || m.VerifyAccess == true || m.MakeOrCheckAccess == true
                                                        ).ToList(),  excludeFieldList: new string[] { "ModuleName" }, transaction: t);

            dapperContext.CommitTransaction();
        }

        public int EditRole(RoleViewModel roleToEdit)
        {
            var t = dapperContext.GetTransaction();

            var sql = "UPDATE Role SET RoleName = @RoleName, Description = @Description Where RoleId = @RoleId AND CONVERT(bigint,RowVersionNo) = @RowVersionNo2; ";
            var rowAffected = context.Execute(sql, roleToEdit.CurrentRole, transaction: t);
            if (rowAffected > 0)
            {
                var list = context.Query<Guid>("SELECT ModuleID FROM RoleModuleAccess WHERE RoleID = @RoleID; ", new { RoleID = roleToEdit.CurrentRole.RoleId }, transaction: t).ToList();

                sql = "UPDATE RoleModuleAccess SET ViewAccess = @ViewAccess, CreateAccess = @CreateAccess, EditAccess = @EditAccess, DeleteAccess = @DeleteAccess, VerifyAccess = @VerifyAccess, MakeOrCheckAccess = @MakeOrCheckAccess WHERE RoleId = @RoleID AND ModuleID = @ModuleID; ";

                var temp =roleToEdit.ModuleAccessList.Where(m => (m.CreateAccess == true || m.DeleteAccess == true
                                                                || m.EditAccess == true || m.ViewAccess == true || m.VerifyAccess == true || m.MakeOrCheckAccess == true)
                                                                && list.Contains(m.ModuleId)).Select(s => s).ToList();

                foreach (var m in temp)
                {
                    m.RoleId = roleToEdit.CurrentRole.RoleId;
                    context.Execute(sql, m, transaction: t);
                }

                temp = roleToEdit.ModuleAccessList.Where(m => (m.CreateAccess == true || m.DeleteAccess == true
                                                                || m.EditAccess == true || m.ViewAccess == true || m.VerifyAccess == true || m.MakeOrCheckAccess == true)
                                                                && !list.Contains(m.ModuleId)).Select(s => s).ToList();
                foreach (var g in temp)
                {
                    g.RoleId = roleToEdit.CurrentRole.RoleId;
                }
                var success = context.InsertMany<RoleModuleAccess>(temp, excludeFieldList: new string[] { "ModuleName" }, transaction: t);

                var tempId = roleToEdit.ModuleAccessList.Where(m => (m.CreateAccess == false && m.DeleteAccess == false
                                                    && m.EditAccess == false && m.ViewAccess == false && m.VerifyAccess == false && m.MakeOrCheckAccess == false)
                                                    && list.Contains(m.ModuleId)).Select(s => s.ModuleId).ToList();
                if (tempId.Count > 0)
                {
                    context.Execute("DELETE FROM RoleModuleAccess WHERE ModuleID IN @ModuleIDList AND RoleID = @RoleID; ", new { RoleID = roleToEdit.CurrentRole.RoleId, ModuleIDList = tempId }, transaction: t);
                }

                dapperContext.CommitTransaction();
            }
            else
            {
                dapperContext.RollbackTransaction();
            }
            return rowAffected;
        }

        //public IEnumerable<Role> GetRoleList()
        //{
        //    return context.Query<Role>("Select * From Role; ").ToList();
        //}

        //public RoleViewModel GetRole(Guid id)
        //{
        //    var sql = new StringBuilder();
        //    sql.AppendLine("Select *, CONVERT(bigint,RowVersionNo) as RowVersionNo2 From Role Where RoleId = @RoleId; ");
        //    //sql.AppendLine("Select ProfileId, ProfileName, ProfileDescription From Profile Where ProfileId IN (Select ProfileId From RoleProfile Where RoleId = @RoleId); ");

        //    RoleViewModel model = new RoleViewModel();
        //    using (var multi = context.QueryMultiple(sql.ToString(), new { RoleId = id }))
        //    {
        //        model.CurrentRole = multi.Read<Role>().FirstOrDefault();
        //        if (model.CurrentRole != null)
        //        {
        //            model.CurrentRole = multi.Read<Role>().ToList();
        //        }
        //    }
        //    return model;
        //}

        //public void AddRole(RoleProfileModel roleToAdd)
        //{
        //    var t = dapperContext.GetTransaction();

        //    var success = context.Insert<Role>(roleToAdd.CurrentRole, transaction: t);
        //    var p = (from r in roleToAdd.SelectedProfiles select new RoleProfile { ProfileId = r.ProfileId, RoleId = roleToAdd.CurrentRole.RoleId }).ToList();
        //    success = context.InsertMany<RoleProfile>(p, transaction: t);

        //    dapperContext.CommitTransaction();
        //}

        //public int EditRole(RoleProfileModel roleToEdit)
        //{
        //    var t = dapperContext.GetTransaction();

        //    var sql = "UPDATE Role SET RoleName = @RoleName, Description = @Description WHERE RoleID = @RoleID AND CONVERT(bigint,RowVersionNo) = @RowVersionNo2; ";
        //    var rowAffected = context.Execute(sql, roleToEdit.CurrentRole, transaction: t);

        //    if (rowAffected > 0)
        //    {
        //        sql = "DELETE FROM RoleProfile WHERE RoleID = @RoleID; ";
        //        context.Execute(sql, new { RoleID = roleToEdit.CurrentRole.RoleId }, transaction: t);

        //        var p = (from r in roleToEdit.SelectedProfiles select new RoleProfile { ProfileId = r.ProfileId, RoleId = roleToEdit.CurrentRole.RoleId }).ToList();
        //        var success = context.InsertMany<RoleProfile>(p, transaction: t);

        //        dapperContext.CommitTransaction();
        //    }
        //    else
        //    {
        //        dapperContext.RollbackTransaction();
        //    }

        //    return rowAffected;
        //}

        public IEnumerable<RoleModuleAccessAggregate> GetRoleModuleAccessList()
        {
            var sql = new StringBuilder();
            sql.AppendLine("Select p.RoleId, m.ModuleId, m.ModuleName, ");
            sql.AppendLine("CASE WHEN CreateAccess = 1 THEN 'Create,' ELSE '' END +  ");
            sql.AppendLine("CASE WHEN EditAccess = 1 THEN 'Edit,' ELSE '' END + ");
            sql.AppendLine("CASE WHEN DeleteAccess = 1 THEN 'Delete,' ELSE '' END + ");
            sql.AppendLine("CASE WHEN VerifyAccess = 1 THEN 'Verify,' ELSE '' END + ");
            sql.AppendLine("CASE WHEN MakeOrCheckAccess = 1 THEN 'MakeOrCheck,' ELSE '' END + ");
            sql.AppendLine("CASE WHEN ViewAccess = 1 THEN 'View' ELSE '' END ");
            sql.AppendLine("As AggregateAccess, m.IsAdmin  ");
            sql.AppendLine("FROM RoleModuleAccess p INNER JOIN Module m ON p.ModuleId = m.ModuleId; ");
            return context.Query<RoleModuleAccessAggregate>(sql.ToString()).ToList();
        }


        public void LockUnlockAccount(string id, bool isLocked)
        {
            context.Execute("UPDATE [User] SET BadPasswordCount = 0, IsLockedOut = @IsLocked WHERE Username = @id; ", new { id = id, IsLocked = isLocked });
            //context.Execute("UPDATE [User] SET BadPasswordCount = 0, ApprovalStatus = @status, IsLockedOut = @statusReverse WHERE Username = @id; ", new { status = status, statusReverse = !status, id = id });
        }

        public void DeleteUser(string id, string optionTaken)
        {
            var dbVersion = context.Query<User>("Select * from dbo.[User] where Username = @Username;", new { Username = id}).FirstOrDefault();
            var incomingVersion = context.Query<User>("Select * from dbo.[User] where Username = @Username;", new { Username = id }).FirstOrDefault();
            incomingVersion.ApprovalStatus = string.IsNullOrEmpty(optionTaken) ? Constants.ApprovalStatus.Pending : optionTaken;
            var sql = new StringBuilder();
            var tr = dapperContext.GetTransaction();

            AuditRepository ar = new AuditRepository(dapperContext);
           
            //To handle maker checker functions
            //maker checker can return the db version of an object depending on user action
            incomingVersion = ar.MakerCheckerHandller<User>(dbVersion, incomingVersion, Constants.OperationType.Delete, Constants.Modules.UserSetup, id, tr);
            
            context.Update<User>(incomingVersion, databaseTableName: "[User]", excludeFieldList: new string[] { "RoleId", "UserRole", "ConfirmPassword" }, primaryKeyList: new string[] { "Username" }, transaction: tr);

            ar.CreateAuditChange(dbVersion, incomingVersion, tr);

            dapperContext.CommitTransaction();

         }

     
        private string GetEntityNextID(string sequenceName, IDbTransaction dbTransaction)
        {
            var sql1 = new StringBuilder();
            sql1.AppendLine("UPDATE SequenceHandler ");
            sql1.AppendLine("SET CurrentSequenceNo = CurrentSequenceNo + 1 WHERE SequenceName = @SequenceName; ");
            sql1.AppendLine("SELECT NextNo = ");
            sql1.AppendLine("CASE  ");
            sql1.AppendLine("   WHEN SequencePrefix IS NULL OR SequencePrefix = ''  ");
            sql1.AppendLine("   THEN REPLICATE('0', SequenceLength - LEN(CONVERT(NVARCHAR(50), CurrentSequenceNo)))  + CONVERT(NVARCHAR(50), CurrentSequenceNo)  ");
            sql1.AppendLine("   WHEN SequencePrefix IS NOT NULL  ");
            sql1.AppendLine("   THEN SequencePrefix + REPLICATE('0', SequenceLength - LEN(CONVERT(VARCHAR(50), SequencePrefix)) - LEN(CONVERT(VARCHAR(50), CurrentSequenceNo)))  + CONVERT(VARCHAR(50), CurrentSequenceNo)  ");
            sql1.AppendLine("END   ");
            sql1.AppendLine("FROM SequenceHandler  WHERE SequenceName = @SequenceName; ");
            var sequenceNo = context.Query<string>(sql1.ToString(), new { SequenceName = sequenceName }, transaction: dbTransaction).FirstOrDefault();

            return sequenceNo;
        }

        public bool DeleteRole(Guid id)
        {
             bool status = false;
             var du = context.Query<User>("Select * from dbo.[Role] where RoleID = @RoleID;", new { RoleID = id }).FirstOrDefault();
             var roleExist = context.Query<Int64>("SELECT COUNT(Username) FROM UsersInRoles WHERE RoleID = @RoleID", new { RoleID = id }).FirstOrDefault();
             if (roleExist == 0)
             {
                 var tr = dapperContext.GetTransaction();
                 context.Execute("DELETE FROM RoleModuleAccess WHERE RoleID = @RoleID; DELETE FROM [Role] WHERE RoleID = @RoleID;", new { RoleID = id }, transaction: tr);
                 AuditRepository ar = new AuditRepository(dapperContext);
                 ar.CreateAuditChange(du, null, tr);
                 status = true;
             }
             return status;
        }
        


    }
}
