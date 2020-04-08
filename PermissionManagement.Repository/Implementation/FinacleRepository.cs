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
using Oracle.ManagedDataAccess;
using Dapper;

namespace PermissionManagement.Repository
{
    public class FinacleRepository : IFinacleRepository
    {
        private readonly IDbConnection context;
        private readonly DapperContext dapperContext;


        public FinacleRepository(DapperContext dbContext)
        {
            dapperContext = dbContext;
            context = dbContext.Connection;
        }

        public FinacleRole GetUserRoleFromFinacle(string userID)
        {
            var sql = "select user_id AS UserID, USER_APPL_NAME AS ApplicationName, a.sol_id AS BranchCode, b.sol_desc AS BranchName from tbaadm.upr a, tbaadm.Sol b where a.sol_id=b.sol_id  and a.del_flg='N' and b.del_flg='N' and user_id= :UserID";
            return context.Query<FinacleRole>(sql, new { UserID = userID.ToUpper() }).FirstOrDefault();
        }

        public string GetUserTillAccountFromFinacle(string userID)
        {
            var sql = "select foracid from tbaadm.gam G,tbaadm.gec T WHERE g.bacid = t.emp_cash_acct and t.emp_id = :UserID AND g.sol_id = t.sol_id and  g.Del_flg = 'N' AND Acct_crncy_code = 'NGN'";
            return context.Query<string>(sql, new { UserID = userID.ToUpper() }).FirstOrDefault();
        }
    }
}
