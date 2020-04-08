using PermissionManagement.Model;
using System.Data;
using System.Text;
using Dapper;
using System;

namespace PermissionManagement.Repository
{
    public class PasswordHistoryRepository : IPasswordHistoryRepository
    {
        #region Properties and Variables
        private readonly IDbConnection context;
        private readonly DapperContext dapperContext;
        ILogRepository log;
        #endregion

        #region Constructors
        public PasswordHistoryRepository(DapperContext dbContext)
        {
            dapperContext = dbContext;
            context = dbContext.Connection;
            log = new LogRepository(dbContext);
        }
        #endregion

        public bool InsertPassword(PasswordHistoryModel passwordHistoryModel)
        {
            passwordHistoryModel.CreatedTime = DateTime.Now;

            return context.Execute("INSERT INTO PasswordHistory ([UserName],[Password],[CreatedTime]) VALUES(@Username,@Password,@CreatedTime)",
                                    new
                                    {
                                        Username = passwordHistoryModel.UserName,
                                        Password = passwordHistoryModel.Password,
                                        CreatedTime = passwordHistoryModel.CreatedTime
                                    }) == 1 ? true : false;
        }

        public bool IsRepeatingPassword(PasswordHistoryModel passwordHistory, int unUsablePasswordCount)
        {
            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT COUNT([Password]) FROM (SELECT TOP " + unUsablePasswordCount + " ");
            sbQuery.Append(" * FROM PasswordHistory WHERE UserName = @UserName ORDER BY CreateDate DESC) PWDHist WHERE Password = @Password");
            var count = (context.ExecuteScalar(sbQuery.ToString(), new
                                                                   {
                                                                       PreviousPasswordsCount = unUsablePasswordCount,
                                                                       UserName = passwordHistory.UserName,
                                                                       Password = passwordHistory.Password
                                                                   }));
            int passwordCount = 0;
            int.TryParse(count.ToString(), out passwordCount);
            return passwordCount > 0 ? true : false;
        }
    }
}
