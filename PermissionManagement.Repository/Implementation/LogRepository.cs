using PermissionManagement.Model;
using PermissionManagement.Utility;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;

namespace PermissionManagement.Repository
{
      public class LogRepository : ILogRepository
    {
        private readonly IDbConnection context;
        private readonly DapperContext dapperContext;

        public LogRepository(DapperContext dbContext)
        {
            dapperContext = dbContext;
            context = dbContext.Connection;
        }

        public void LogError(ExceptionLog error)
        {
            ////if the same error has been logged from the same ip less than 5 minutes ago, 
            ////simply update the existing one.

            error.ExceptionDateTime = Helper.GetLocalDate();
            System.DateTime timeFiveMinutesAgo = Helper.GetLocalDate().AddMinutes(-5);
            dapperContext.RollbackTransaction();
            dapperContext.OpenIfNot();
            var ex = context.Query<ExceptionLog>("Select * From ExceptionLog Where UserIPAddress = @UserIPAddress And ExceptionPage = @ExceptionPage And ExceptionType = @ExceptionType And ExceptionDatetime > @timeFiveMinutesAgo; ", new { UserIPAddress = error.UserIPAddress, ExceptionPage = error.ExceptionPage, ExceptionType = error.ExceptionType, timeFiveMinutesAgo = timeFiveMinutesAgo }).FirstOrDefault();

            if (ex == null)
            {
                context.Insert<ExceptionLog>(error, excludeFieldList: new string[] { "ExceptionId" });
            }
            else
            {
                var sql = new StringBuilder();
                error.ExceptionId = ex.ExceptionId;
                sql.AppendLine("UPDATE ExceptionLog SET ExceptionDatetime = @ExceptionDatetime, ExceptionDetails = @ExceptionDetails, ExceptionMessage = @ExceptionMessage, LoggedInUser = @LoggedInUser, ExceptionVersion = @ExceptionVersion ");
                sql.AppendLine("Where ExceptionId = @ExceptionId; ");
                context.Execute(sql.ToString(), error);
            }
            dapperContext.CloseIfNot();
        }
    }
}
