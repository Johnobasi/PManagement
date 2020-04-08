using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionManagement.Model;
using PermissionManagement.Utility;
using PermissionManagement.Repository;

namespace PermissionManagement.Repository.Implementation
{
    public partial class ReportsRepository : IReportsRepository
    {
        private UserListingReportsList result = new UserListingReportsList();
        public UserListingReportsList GetUsersList(AllUserListModel reportType, PagerItemsII parameter)
        {
            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT * FROM (");
            #region RowNumber and Order By Query
            if (parameter != null)
            {
                StringBuilder sbOrderBy = new StringBuilder("SELECT ROW_NUMBER() OVER(ORDER BY ");
                foreach (var column in parameter.SortColumns)
                {
                    if ((column.Data == "0") || column.Data == Constants.ExpiredUserSortField.Username)
                    {
                        sbOrderBy.Append("UserName ");
                    }
                    else if (column.Data == Constants.ExpiredUserSortField.Firstname)
                    {
                        sbOrderBy.Append("FirstName ");
                    }
                    else if (column.Data == Constants.ExpiredUserSortField.Lastname)
                    {
                        sbOrderBy.Append("LastName ");
                    }
                    else if (column.Data == Constants.ExpiredUserSortField.Email)
                    {
                        sbOrderBy.Append("Email ");
                    }
                    sbOrderBy.Append(column.SortDirection == 0 ? " asc )" : " desc )");
                }
                sbOrderBy.Append("AS UserName, FirstName, LastName, Email, IsFirstLogin, CreationDate, LastLogInDate, IsDormented, AccountExpiryDate, IsLockedOut From [User] ) AS TBL ");
                sbQuery.Append(sbOrderBy.ToString());
            }
            #endregion

            switch (reportType.ReportTypeEnum)
            {
                case ReportTypeEnum.NewUser:
                    {
                        sbQuery.Append("WHERE IsFirstLogin = '1'");
                        break;
                    }
                case ReportTypeEnum.ExpiredAccount:
                    {
                        sbQuery.Append("WHERE AccountExpiryDate <= GETDATE()");
                        break;
                    }
                case ReportTypeEnum.DormantUser:
                    {
                        sbQuery.Append("WHERE (CreationDate < GETDATE()-3 AND LastLogInDate IS NULL) OR IsDormented = '1'");
                        break;
                    }
                case ReportTypeEnum.DisabledUser:
                    {
                        sbQuery.Append("WHERE IsLockedOut = '1'");
                        break;
                    }
            }

            result.UserLstResult = _context.Query<UserListingReports>(sbQuery.ToString()).ToList();
            return result;
        }
        #region upen
        public UserListingReportsList AllUsersList(AllUserListModel reportType, List<string> parameter)
        {
            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("SELECT * FROM (");
            #region RowNumber and Order By Query
            if (parameter != null)
            {
                StringBuilder sbOrderBy = new StringBuilder("SELECT ROW_NUMBER() OVER(ORDER BY ");
                foreach (var column in parameter)
                {
                    if ((column.ToString() == "0") || column.ToString().ToLower().Contains(Constants.ExpiredUserSortField.Username.ToLower()))
                    {
                        sbOrderBy.Append("UserName ");
                    }
                    else if (column.ToString().ToLower().Contains(Constants.ExpiredUserSortField.Firstname.ToLower()))
                    {
                        sbOrderBy.Append("FirstName ");
                    }
                    else if (column.ToString().ToLower().Contains(Constants.ExpiredUserSortField.Lastname.ToLower()))
                    {
                        sbOrderBy.Append("LastName ");
                    }
                    else if (column.ToString().ToLower().Contains(Constants.ExpiredUserSortField.Email.ToLower()))
                    {
                        sbOrderBy.Append("Email ");
                    }
                    int sort = 0;
                    int.TryParse(column, out sort);
                    if (column.Contains("desc"))
                    {
                        var checkSort = column.Contains("desc") ? " desc )" : " asc )";
                        sbOrderBy.Append(checkSort);
                    }
                    else
                    {
                        sbOrderBy.Append(sort == 0 ? " asc )" : " desc )");
                    }

                }
                sbOrderBy.Append("AS UserName, FirstName, LastName, Email, IsFirstLogin, CreationDate, LastLogInDate, IsDormented, AccountExpiryDate, IsLockedOut From [User] ) AS TBL ");
                sbQuery.Append(sbOrderBy.ToString());
            }
            #endregion

            bool isSearch = !string.IsNullOrWhiteSpace(reportType.Searchkey);
            switch (reportType.ReportTypeEnum)
            {
                case ReportTypeEnum.NewUser:
                    {
                        //sbQuery.Append(isSearch ? "WHERE (IsFirstLogin = '1'" : "WHERE IsFirstLogin = '1'");
                        sbQuery.Append("WHERE IsFirstLogin = '1'");
                        break;
                    }
                case ReportTypeEnum.ExpiredAccount:
                    {
                        sbQuery.Append(isSearch ? "WHERE (AccountExpiryDate <= GETDATE()" : "WHERE AccountExpiryDate <= GETDATE()");
                        break;
                    }
                case ReportTypeEnum.DormantUser:
                    {
                        //sbQuery.Append(
                        //    isSearch ?
                        //        "WHERE ((CreationDate < GETDATE()-" + reportType.psDormentDays.Value + " AND LastLogInDate IS NULL) OR IsDormented = '1'" :
                        //        "WHERE (CreationDate < GETDATE()-" + reportType.psDormentDays.Value + " AND LastLogInDate IS NULL) OR IsDormented = '1'");
                        sbQuery.Append("WHERE ((CreationDate < GETDATE()-" + reportType.psDormentDays.Value + " AND LastLogInDate IS NULL) OR IsDormented = '1')");
                        break;
                    }
                case ReportTypeEnum.DisabledUser:
                    {
                        sbQuery.Append(isSearch ? "WHERE (IsLockedOut = '1'" : "WHERE IsLockedOut = '1'");
                        break;
                    }
            }
            if (!string.IsNullOrWhiteSpace(reportType.Searchkey))
            {
                sbQuery.Append(" and (");
                sbQuery.Append(" username like '%" + reportType.Searchkey + "%'");
                sbQuery.Append(" or firstname like '%" + reportType.Searchkey + "%'");
                sbQuery.Append(" or lastname like '%" + reportType.Searchkey + "%'");
                sbQuery.Append(" or email like '%" + reportType.Searchkey + "%')");
            }


            result.UserLstResult = _context.Query<UserListingReports>(sbQuery.ToString()).ToList();
            return result;
        }
        #endregion
    }
}
