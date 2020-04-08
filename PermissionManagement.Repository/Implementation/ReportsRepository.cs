using PermissionManagement.Model;
using PermissionManagement.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using Dapper;

namespace PermissionManagement.Repository
{
    public partial class ReportsRepository : IReportsRepository
    {
        #region GlobalVariables
        private readonly IDbConnection _context;
        private readonly DapperContext _dapperContext;
        private UserListingReportsList result = new UserListingReportsList();
        IPortalSettingsRepository portalSettingsRepository;

         
        #endregion

        #region constructor
        public ReportsRepository(DapperContext dbContext, IPortalSettingsRepository portalSettingsRepositoryInstance)
        {
            _dapperContext = dbContext;
            _context = dbContext.Connection;
            portalSettingsRepository = portalSettingsRepositoryInstance;
        }
        #endregion

        #region Exception Reports
        public ReportsExceptionListingResponse GetExceptionList(PagerItemsII parameter)
        {
             
            var result = new ReportsExceptionListingResponse() { PagerResource = new PagerItems() };

            var orderByField = string.Empty;
            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM (");
            sql.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY ");

            var sortSql = new StringBuilder();
            #region sortColumns
            foreach (var column in parameter.SortColumns)
            {
                sortSql.Append(sortSql.Length > 0 ? "," : "");

                if ((column.Data == "0") || column.Data == Constants.ExceptionSortField.ExceptionId)
                {
                    sql.Append("ExceptionId ");
                    sortSql.Append("ExceptionId ");
                }
                else if (column.Data == Constants.ExceptionSortField.ExceptionDateTime)
                {
                    sql.Append("ExceptionDateTime ");
                    sortSql.Append("ExceptionDateTime ");
                }

                else if (column.Data == Constants.ExceptionSortField.ExceptionPage)
                {
                    sql.Append("ExceptionPage ");
                    sortSql.Append("ExceptionPage ");
                }
                else if (column.Data == Constants.ExceptionSortField.LoggedInUser)
                {
                    sql.Append("LoggedInUser ");
                    sortSql.Append("LoggedInUser ");
                }
                else if (column.Data == Constants.ExceptionSortField.ExceptionType)
                {
                    sql.Append("ExceptionType ");
                    sortSql.Append("ExceptionType ");
                }
                else if (column.Data == Constants.ExceptionSortField.ExceptionMessage)
                {
                    sql.Append("ExceptionMessage ");
                    sortSql.Append("ExceptionMessage ");
                }
                else if (column.Data == Constants.ExceptionSortField.ExceptionVersion)
                {
                    sql.Append("ExceptionVersion ");
                    sortSql.Append("ExceptionVersion ");
                }

                sql.Append(column.SortDirection == 0 ? " asc" : " desc");
                sortSql.Append(column.SortDirection == 0 ? " asc" : " desc");
            }
            #endregion
            #region localParams
            var exceptionIdFilter = string.Empty;
            var exceptionTypeFilter = string.Empty;
            DateTime exceptionDateFromFilter = DateTime.Now.AddDays(-365);
            DateTime exceptionDateTimeToFilter = DateTime.Now;
            #endregion
            var whereClause = new StringBuilder();
            whereClause.Append(" WHERE ");
            var filter = string.Empty;
            foreach (var column in parameter.SearchColumns)
            {
                if (column.Data == Constants.ExceptionSortField.ExceptionDateTime && column.Search.Value != Constants.General.YadcfDelimiter)
                {
                    var dateFilter = column.Search.Value.Split(Constants.General.YadcfDelimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var start = column.Search.Value.StartsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter[0];
                    var end = column.Search.Value.EndsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter.Length > 1 ? dateFilter[1] : dateFilter[0];
                    if (!string.IsNullOrEmpty(start))
                    {

                        exceptionDateFromFilter = DateTime.Parse(start, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        exceptionDateFromFilter = new DateTime(exceptionDateFromFilter.Year, exceptionDateFromFilter.Month, exceptionDateFromFilter.Day, 00, 00, 00);
                        whereClause.AppendFormat("(ExceptionDateTime >= @exceptionDateFromFilter) AND ");
                    }
                    if (!string.IsNullOrEmpty(end))
                    {
                        exceptionDateTimeToFilter = DateTime.Parse(end, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        exceptionDateTimeToFilter = new DateTime(exceptionDateFromFilter.Year, exceptionDateTimeToFilter.Month, exceptionDateTimeToFilter.Day, 23, 59, 59);
                        whereClause.AppendFormat("(ExceptionDateTime <= @exceptionDateToFilter) AND ");
                    }
                }
                else if (column.Data == Constants.ExceptionSortField.ExceptionId && !string.IsNullOrEmpty(column.Search.Value))
                {
                    exceptionIdFilter = column.Search.Value.Trim().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    exceptionIdFilter = string.Format("%{0}%", exceptionIdFilter);
                    whereClause.Append(" (exceptionId LIKE @exceptionIdFilter) AND ");
                }
           
                else if (column.Data == Constants.ExceptionSortField.ExceptionMessage  && !string.IsNullOrEmpty(column.Search.Value))
                {
                    exceptionTypeFilter = column.Search.Value.Trim().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    exceptionTypeFilter = string.Format("%{0}%", exceptionTypeFilter);
                    whereClause.Append(" (ExceptionMessage LIKE @ExceptionTypeFilter) AND ");
                }
                
            }
            if (whereClause.Length > 7)
            {
                whereClause.Remove(whereClause.Length - 4, 4);
            }
            var globalFilter = string.Empty;
            if (!string.IsNullOrEmpty(parameter.siteSearch))
            {
                globalFilter = parameter.siteSearch.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                globalFilter = string.Format("%{0}%", globalFilter);
                whereClause.Append(" OR ((ExceptionID LIKE @GlobalSearchFilter) OR (ExceptionType LIKE @GlobalSearchFilter)) ");
            }
            sql.AppendLine(") AS NUMBER, ExceptionId,  ExceptionDateTime, ExceptionDetails, ExceptionPage, LoggedInUser, ExceptionType, ExceptionMessage, ExceptionVersion ");
            sql.AppendLine("From [exceptionlog] ");
            sql.AppendFormat("{0}) AS TBL ", whereClause.Length > 7 ? whereClause.ToString() : string.Empty);
            sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
            sql.AppendFormat("ORDER BY {0} ", sortSql.ToString());
           

            result.PagerResource.ResultCount = (int)_context.Query<Int64>(
                string.Format("Select Count(ExceptionId) From [exceptionlog] {0} ", whereClause.Length > 7 ? whereClause.ToString() : string.Empty),
                new
                {
                    StartPage = ((parameter.PageNumber - 1) * parameter.PageSize) + 1,
                    EndPage = (parameter.PageNumber * parameter.PageSize),
                    GlobalSearchFilter = globalFilter,
                    ExceptionIDFilter = exceptionIdFilter,
                    ExceptionTypeFilter = exceptionTypeFilter,
                    ExceptionDateFromFilter = exceptionDateFromFilter,
                    exceptionDateToFilter = exceptionDateTimeToFilter
                }).First();
            

            result.UserListingResult = _context.Query<ReportsExceptionDto>(sql.ToString(), //+ whereClause,
                new
                {
                    StartPage = ((parameter.PageNumber - 1) * parameter.PageSize) + 1,
                    EndPage = (parameter.PageNumber * parameter.PageSize),
                    GlobalSearchFilter = globalFilter,
                    ExceptionIDFilter = exceptionIdFilter,
                    ExceptionTypeFilter = exceptionTypeFilter,
                    ExceptionDateFromFilter = exceptionDateFromFilter,
                    ExceptionDateToFilter = exceptionDateTimeToFilter

                }).ToList();
            return result;
        }

        public IEnumerable<ReportsExceptionDto> GetExceptionExcel(string exceptionId = "", string exceptionMessage = "", DateTime? fromDt = null, DateTime? toDate = null)
        {
            DateTimeHelper dtHelper = new DateTimeHelper();
            IEnumerable<ReportsExceptionDto> excelDtos = null;
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT [ExceptionId], [ExceptionDateTime] ,[ExceptionPage],[LoggedInUser],[UserIpAddress],[ExceptionType] ,[ExceptionMessage],[ExceptionVersion]  FROM [exceptionlog] where ");
            int exceptionID = 0;
            int.TryParse(exceptionId, out exceptionID);
            sql.Append(exceptionID > 0 ? "[ExceptionId] LIKE '%" + exceptionID + "%' AND" : "");
            sql.Append("[ExceptionMessage] LIKE '%" + exceptionMessage + "%' ");
            if (fromDt != null)
            {
                sql.Append(" AND [ExceptionDateTime] >= '" + dtHelper.FormatDateTime(fromDt.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' ");
            }
            if (toDate != null)
            {
                sql.Append(" AND [ExceptionDateTime] <= '" + dtHelper.FormatDateTime(toDate.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + " '");
            }
            excelDtos = _context.Query<ReportsExceptionDto>(sql.ToString()).ToList();
            return excelDtos;
        }

        public string GetExcepionMessageById(string id)
        {
            var result = new List<string>(); //new List<string>();
            var sql = new StringBuilder();
            sql.AppendLine("SELECT  ExceptionDetails");
            sql.AppendLine("From [exceptionlog] ");
            sql.AppendLine("WHERE exceptionid = @exceptionid");
            result = _context.Query<string>(sql.ToString(), new { ExceptionId = id }).ToList();
            return result.FirstOrDefault();
        }
        #endregion

        #region User List Reports

        public IEnumerable<ExpiredUserListingDto> GetExcelReportForUsers(UserListingReports userInputs,
            DateTime? fromDate, DateTime? toDate, string reportType)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT [Username], [FirstName] ,[LastName],[Email],[IsLockedOut],[AccountType],[StaffPosition] ,[AccountExpiryDate],[CreationDate],[LastLogInDate] FROM [User]  ");
            var user = GetUserType(reportType);
            sql.Append(user);
            var searchData = GetSearchParam(userInputs,fromDate,toDate,reportType);
            sql.Append(searchData);
            return _context.Query<ExpiredUserListingDto>(sql.ToString()) as IEnumerable<ExpiredUserListingDto>;
        }
        #region singlesearch
        public IEnumerable<ExpiredUserListingDto> GetExcel(string userType, string searchParam)
        {// username,firstname,lastname,email
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT [Username], [FirstName] ,[LastName],[Email],[IsLockedOut],[AccountType],[StaffPosition] ,[AccountExpiryDate],[CreationDate],[LastLogInDate] FROM [User]  ");
            var user = GetUserType(userType);
            sql.Append(user);
            var searchData = GetSearchParam(searchParam);
            sql.Append(searchData);
            return _context.Query<ExpiredUserListingDto>(sql.ToString()) as IEnumerable<ExpiredUserListingDto>;
        }
        #endregion

        private static StringBuilder GetSearchParam(UserListingReports searchParam,DateTime ? frDateTime,DateTime? toDateTime,string reportType)
        {
            StringBuilder sqlQuery = new StringBuilder(); DateTimeHelper dtHelper = new DateTimeHelper();

            UserListingReports objSearchParam= new UserListingReports();


            if (searchParam != null && reportType!="AllUsers")
            {
                sqlQuery.Append(" AND ( ([Username] LIKE " + "'%" + searchParam.UserName + "%' )" +
                    "  AND " + "([FirstName] LIKE '%" + searchParam.FirstName + "%')" +
                    " AND ( [LastName]  LIKE '%" + searchParam.LastName + "%')" + 
                    " AND ([Email]  LIKE '%" + searchParam.Email + "%'))");
                if (frDateTime !=null)
                {
                    sqlQuery.Append(" And ([CreationDate] >= '" + dtHelper.FormatDateTime(frDateTime.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' ");

                }
                if (toDateTime != null)
                {
                    sqlQuery.Append(" AND [CreationDate] <= '" + dtHelper.FormatDateTime(toDateTime.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + " ')");
                }

            }
            else
            {
                sqlQuery.Append("  ( ([Username] LIKE " + "'%" + searchParam.UserName + "%' )" +
                       "  AND " + "([FirstName] LIKE '%" + searchParam.FirstName + "%')" +
                       " AND ( [LastName]  LIKE '%" + searchParam.LastName + "%')" +
                       " AND ([Email]  LIKE '%" + searchParam.Email + "%'))");
                if (frDateTime != null)
                {
                    sqlQuery.Append(" And ([CreationDate] >= '" + dtHelper.FormatDateTime(frDateTime.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' ");

                }
                if (toDateTime != null)
                {
                    sqlQuery.Append(" AND [CreationDate] <= '" + dtHelper.FormatDateTime(toDateTime.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + " ')");
                }

            }
            return sqlQuery;
        }
        private static StringBuilder GetSearchParam(string searchParam)
        {
            StringBuilder sqlQuery = new StringBuilder();
            if (!string.IsNullOrEmpty(searchParam))
            {
                sqlQuery.Append(" AND ( ([Username] LIKE " + "'%" + searchParam + "%' )" + "  OR " + "([FirstName] LIKE '%" + searchParam + "%')" + " OR( [LastName]  LIKE '%" + searchParam + "%')" + " OR ([Email]  LIKE '%" + searchParam + "%'))");
            }
            return sqlQuery;
        }

        private static StringBuilder GetUserType(string sql)
        {
            var sqlstring = new StringBuilder();
            if (!string.IsNullOrEmpty(sql))
            {
                if (sql == ReportTypeEnum.NewUser.ToString())
                {
                    sqlstring.Append("WHERE (IsFirstLogin = '1' )");
                }

                else if (sql.ToLower() == ReportTypeEnum.ExpiredAccount.ToString().ToLower())
                {
                    sqlstring.Append("WHERE  ( AccountExpiryDate <= GETDATE() )");
                }
                else if (sql.ToLower() == ReportTypeEnum.DormantUser.ToString().ToLower())
                {
                    sqlstring.Append("WHERE ((CreationDate < GETDATE()-3 AND LastLogInDate IS NULL) OR IsDormented = '1') ");
                }
                else if (sql.ToLower() == ReportTypeEnum.DisabledUser.ToString().ToLower())
                {
                    sqlstring.Append("WHERE ( IsLockedOut = '1' ) ");
                }
                else if (sql.ToLower()==ReportTypeEnum.AllUsers.ToString().ToLower())
                {
                    sqlstring.Append("WHERE ");
                }

            }
            return sqlstring;
        }

        #region upen
        public UserListingReportsList GetUsersList(AllUserListModel reportType, PagerItemsII parameter)
        {
            var result = new UserListingReportsList() { PagerResource = new PagerItems() };
            var orderByField = string.Empty;
            var sql = new StringBuilder();
            sql.Append("SELECT * FROM(");
            sql.Append("SELECT ROW_NUMBER() OVER (ORDER BY");
            var sortSql = new StringBuilder();

            #region sortColumns
            foreach (var column in parameter.SortColumns)
            {
                sortSql.Append(sortSql.Length > 0 ? "," : "");
                if ((column.Data == "0") || column.Data == Constants.ExpiredUserSortField.Username)
                {
                    sql.Append(" UserName "); sortSql.Append("UserName ");
                }
                else if (column.Data==Constants.ExpiredUserSortField.CreationDate)
                {
                    sql.Append(" CreationDate "); sortSql.Append("CreationDate ");
                }
                else if (column.Data == Constants.ExpiredUserSortField.Firstname)
                {
                    sql.Append(" FirstName "); sortSql.Append("FirstName ");
                }
                else if (column.Data == Constants.ExpiredUserSortField.Lastname)
                {
                    sql.Append(" LastName "); sortSql.Append("LastName ");
                }
                else if (column.Data == Constants.ExpiredUserSortField.Email)
                {
                    sql.Append(" Email "); sortSql.Append("Email ");
                }
                sql.Append(column.SortDirection == 0 ? " asc" : " desc");
                sortSql.Append(column.SortDirection == 0 ? " asc" : " desc");
            }
            #endregion

            #region localCommandParams
            var userNameFilter = string.Empty;
            var firstNameFilter = string.Empty;
            var lastNameFilter = string.Empty;
            var eMailFilter = string.Empty;
            DateTime creationDateTimeFromFilter = DateTime.Now;
            DateTime creationDateTimeFromTo = DateTime.Now;

            #endregion


            #region  commented
            var whereClause = new StringBuilder();
            //whereClause.Append(" WHERE ");
            var globalFilter = string.Empty;
            var filter = string.Empty;
            #region old search 
            foreach (var column in parameter.SearchColumns)
            {
                if (column.Data == Constants.ExpiredUserSortField.CreationDate && column.Search.Value != Constants.General.YadcfDelimiter)
                {
                    var dateFilter = column.Search.Value.Split(Constants.General.YadcfDelimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var start = column.Search.Value.StartsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter[0];
                    var end = column.Search.Value.EndsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter.Length > 1 ? dateFilter[1] : dateFilter[0];
                    if (!string.IsNullOrEmpty(start))
                    {

                        creationDateTimeFromFilter = DateTime.Parse(start, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        creationDateTimeFromFilter = new DateTime(creationDateTimeFromFilter.Year, creationDateTimeFromFilter.Month, creationDateTimeFromFilter.Day, 00, 00, 00);
                        whereClause.AppendFormat(" AND ( CreationDate >= @creationDateTimeFromFilter) ");
                    }
                    if (!string.IsNullOrEmpty(end))
                    {
                        creationDateTimeFromTo = DateTime.Parse(end, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        creationDateTimeFromTo = new DateTime(creationDateTimeFromTo.Year, creationDateTimeFromTo.Month, creationDateTimeFromTo.Day, 23, 59, 59);
                        whereClause.AppendFormat(" AND ( CreationDate <= @creationDateTimeFromTo)");
                    }
                }
                else if (column.Data == Constants.ExpiredUserSortField.Username && !string.IsNullOrEmpty(column.Search.Value))
                {
                    userNameFilter = column.Search.Value.Trim().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    userNameFilter = string.Format("%{0}%", userNameFilter);
                    whereClause.Append(" AND (username like @userNameFilter)  ");
                }
                else if (column.Data == Constants.ExpiredUserSortField.Firstname && !string.IsNullOrEmpty(column.Search.Value))
                {
                    firstNameFilter = column.Search.Value.Trim().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    firstNameFilter = string.Format("%{0}%", firstNameFilter);
                    whereClause.Append(" AND (Firstname like @firstNameFilter)   ");
                }
                else if (column.Data == Constants.ExpiredUserSortField.Lastname && !string.IsNullOrEmpty(column.Search.Value))
                {
                    lastNameFilter = column.Search.Value.Trim().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    lastNameFilter = string.Format("%{0}%", lastNameFilter);
                    whereClause.Append(" AND (Lastname like @lastNameFilter) ");
                }
                else if (column.Data == Constants.ExpiredUserSortField.Email && !string.IsNullOrEmpty(column.Search.Value))
                {
                    eMailFilter = column.Search.Value.Trim().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    eMailFilter = string.Format("%{0}%", eMailFilter);
                    whereClause.Append(" AND (Email like @eMailFilter) ");
                }

            }
            #endregion
            //if (whereClause.Length > 7)
            //{
            //    whereClause.Remove(whereClause.Length - 4, 4);
            //}

            if (!string.IsNullOrEmpty(parameter.siteSearch))
            {
                globalFilter = parameter.siteSearch.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                globalFilter = string.Format("%{0}%", globalFilter);
                whereClause.Append(" OR ((username LIKE @GlobalSearchFilter) OR (email LIKE @GlobalSearchFilter)) ");
            }
            #endregion

            sql.AppendLine(") AS NUMBER,UserName, FirstName, LastName, Email, IsFirstLogin, CreationDate, LastLogInDate, IsDormented, AccountExpiryDate, IsLockedOut ");
            
            var strcountAppend = new StringBuilder();

            switch (reportType.ReportTypeEnum)
            {

                  case ReportTypeEnum.AllUsers:
                    {
                        sql.AppendLine("From [User] ");
                        break;
                    }
                case ReportTypeEnum.NewUser:
                    {
                        sql.AppendLine("From [User] ");
                        sql.Append("WHERE (IsFirstLogin = '1' )");
                        strcountAppend.Append("WHERE (IsFirstLogin = '1' )");
                        break;
                    }
                case ReportTypeEnum.ExpiredAccount:
                    {
                        sql.AppendLine("From [User] ");
                        sql.Append("WHERE  ( AccountExpiryDate <= GETDATE() )");
                        strcountAppend.Append("WHERE  ( AccountExpiryDate <= GETDATE() )");
                        break;
                    }
                case ReportTypeEnum.DormantUser:
                    {
                        sql.AppendLine("From [User] ");
                        sql.Append("WHERE ((CreationDate < GETDATE()-" + portalSettingsRepository.GetSettingByKey(Constants.PortalSettingsKeysConstants.NEWUSERIDDORMANTNUMBERDAYS).Value + " AND LastLogInDate IS NULL) OR IsDormented = '1')");
                        strcountAppend.Append("WHERE ((CreationDate < GETDATE()-" + portalSettingsRepository.GetSettingByKey(Constants.PortalSettingsKeysConstants.NEWUSERIDDORMANTNUMBERDAYS).Value + " AND LastLogInDate IS NULL) OR IsDormented = '1')");
                        break;
                    }
                case ReportTypeEnum.DisabledUser:
                    {
                        sql.AppendLine("From [User] ");
                        sql.Append("WHERE ( IsLockedOut = '1' )");
                        strcountAppend.Append("WHERE ( IsLockedOut = '1' )");
                        break;
                    }
              
            }
            //var whereClause = new StringBuilder();
            ////whereClause.Append(" WHERE ");
            //var globalFilter = string.Empty;
            //var filter = string.Empty;
            //if (!string.IsNullOrWhiteSpace(parameter.siteSearch.Trim()))
            //{
            //    globalFilter = parameter.siteSearch.Trim().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
            //    globalFilter = string.Format("%{0}%", globalFilter);
            //    whereClause.Append(" and ((username LIKE @GlobalSearchFilter) OR (firstname LIKE @GlobalSearchFilter) OR (LastName LIKE @GlobalSearchFilter) OR (email LIKE @GlobalSearchFilter)) ");
            //}
            #endregion
            if (reportType.ReportTypeEnum.ToString() == "AllUsers")
            {
                whereClause = CheckWhereClauseForAllUsers(ReportTypeEnum.AllUsers.ToString(), whereClause);
                sql.AppendLine(whereClause.Length > 5 ? whereClause.ToString() : string.Empty);
                sql.AppendLine(") AS TBL ");
            }
            else
            {
                sql.AppendLine(whereClause.Length > 5 ? whereClause.ToString() : string.Empty);
                sql.AppendLine(") AS TBL ");
            }
           

            #region ReportType

            #endregion

            //sql.AppendFormat("{0}) AS TBL ", whereClause.Length > 7 ? whereClause.ToString() : string.Empty);

            #region SearchParms

            sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
            sql.AppendFormat("ORDER BY {0} ", sortSql.ToString());
            #region RecordsCount
            result.PagerResource.ResultCount = (int)_context.Query<Int64>(
       string.Format("Select Count(Username) From [User] {0}{1} ", strcountAppend, whereClause.Length > 7 ? whereClause.ToString() : string.Empty),

                   new
                   {
                       StartPage = ((parameter.PageNumber - 1) * parameter.PageSize) + 1,
                       EndPage = (parameter.PageNumber * parameter.PageSize),
                       GlobalSearchFilter = globalFilter,
                       UserNameFilter = userNameFilter,
                       FirstNameFilter = firstNameFilter,
                       LastNameFilter = lastNameFilter,
                       eMailFilter = eMailFilter,
                       creationDateTimeFromFilter=creationDateTimeFromFilter,
                       creationDateTimeFromTo=creationDateTimeFromTo
                   }).First();

            var sqlQuery = sql.ToString();
            result.UserLstResult = _context.Query<UserListingReports>(
                string.Format(sqlQuery),
                new
                {
                    StartPage = ((parameter.PageNumber - 1) * parameter.PageSize) + 1,
                    EndPage = (parameter.PageNumber * parameter.PageSize),
                    GlobalSearchFilter = globalFilter,
                    UserNameFilter = userNameFilter,
                    FirstNameFilter = firstNameFilter,
                    LastNameFilter = lastNameFilter,
                    EMailFilter = eMailFilter,
                    CreationDateTimeFromFilter = creationDateTimeFromFilter,
                    CreationDateTimeFromTo = creationDateTimeFromTo

                }).ToList();

            return result;
            #endregion

            #endregion

        }
        public UserListingReportsList GetUsersListSingleSearch(AllUserListModel reportType, PagerItemsII parameter)
        {
            var result = new UserListingReportsList() { PagerResource = new PagerItems() };
            var orderByField = string.Empty;
            var sql = new StringBuilder();
            sql.Append("SELECT * FROM(");
            sql.Append("SELECT ROW_NUMBER() OVER (ORDER BY");
            var sortSql = new StringBuilder();
            #region sortColumns
            foreach (var column in parameter.SortColumns)
            {
                sortSql.Append(sortSql.Length > 0 ? "," : "");
                if ((column.Data == "0") || column.Data == Constants.ExpiredUserSortField.Username)
                {
                    sql.Append(" UserName "); sortSql.Append("UserName ");
                }
                else if (column.Data == Constants.ExpiredUserSortField.Firstname)
                {
                    sql.Append(" FirstName "); sortSql.Append("FirstName ");
                }
                else if (column.Data == Constants.ExpiredUserSortField.Lastname)
                {
                    sql.Append(" LastName "); sortSql.Append("LastName ");
                }
                else if (column.Data == Constants.ExpiredUserSortField.Email)
                {
                    sql.Append(" Email "); sortSql.Append("Email ");
                }
                sql.Append(column.SortDirection == 0 ? " asc" : " desc");
                sortSql.Append(column.SortDirection == 0 ? " asc" : " desc");
            }
            #endregion
            #region localCommandParams
            var userNameFilter = string.Empty;
            var firstNameFilter = string.Empty;
            var lastNameFilter = string.Empty;
            var eMailFilter = string.Empty;
            #endregion


            #region  commented
            #region old search Comment
            //foreach (var column in parameter.siteSearch)
            //{
            //    if (column.Data == Constants.ExpiredUserSortField.Username && !string.IsNullOrEmpty(column.Search.Value))
            //    {
            //        userNameFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
            //        userNameFilter = string.Format("%{0}%", userNameFilter);
            //        whereClause.Append(" (username like @userNameFilter) AND ");
            //    }
            //    else if (column.Data == Constants.ExpiredUserSortField.Firstname && !string.IsNullOrEmpty(column.Search.Value))
            //    {
            //        firstNameFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
            //        firstNameFilter = string.Format("%{0}%", firstNameFilter);
            //        whereClause.Append(" (username like @firstNameFilter) AND ");
            //    }
            //    else if (column.Data == Constants.ExpiredUserSortField.Lastname && !string.IsNullOrEmpty(column.Search.Value))
            //    {
            //        lastNameFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
            //        lastNameFilter = string.Format("%{0}%", lastNameFilter);
            //        whereClause.Append(" (username like @lastNameFilter) AND ");
            //    }
            //    else if (column.Data == Constants.ExpiredUserSortField.Email && !string.IsNullOrEmpty(column.Search.Value))
            //    {
            //        eMailFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
            //        eMailFilter = string.Format("%{0}%", eMailFilter);
            //        whereClause.Append(" (username like @eMailFilter) AND ");
            //    }
            //} 
            #endregion
            //if (whereClause.Length > 7)
            //{
            //    whereClause.Remove(whereClause.Length - 4, 4);
            //}

            //if (!string.IsNullOrEmpty(parameter.siteSearch))
            //{
            //    globalFilter = parameter.siteSearch.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
            //    globalFilter = string.Format("%{0}%", globalFilter);
            //    whereClause.Append(" OR ((username LIKE @GlobalSearchFilter) OR (email LIKE @GlobalSearchFilter)) ");
            //}
            #endregion

            sql.AppendLine(") AS NUMBER,UserName, FirstName, LastName, Email, IsFirstLogin, CreationDate, LastLogInDate, IsDormented, AccountExpiryDate, IsLockedOut ");
            sql.AppendLine("From [User] ");
            var strcountAppend = new StringBuilder();

            switch (reportType.ReportTypeEnum)
            {
                case ReportTypeEnum.NewUser:
                    {
                        sql.Append("WHERE (IsFirstLogin = '1' )");
                        strcountAppend.Append("WHERE (IsFirstLogin = '1' )");
                        break;
                    }
                case ReportTypeEnum.ExpiredAccount:
                    {
                        sql.Append("WHERE  ( AccountExpiryDate <= GETDATE() )");
                        strcountAppend.Append("WHERE  ( AccountExpiryDate <= GETDATE() )");
                        break;
                    }
                case ReportTypeEnum.DormantUser:
                    {
                        sql.Append("WHERE ((CreationDate < GETDATE()-" + portalSettingsRepository.GetSettingByKey(Constants.PortalSettingsKeysConstants.NEWUSERIDDORMANTNUMBERDAYS).Value + " AND LastLogInDate IS NULL) OR IsDormented = '1')");
                        strcountAppend.Append("WHERE ((CreationDate < GETDATE()-" + portalSettingsRepository.GetSettingByKey(Constants.PortalSettingsKeysConstants.NEWUSERIDDORMANTNUMBERDAYS).Value + " AND LastLogInDate IS NULL) OR IsDormented = '1')");
                        break;
                    }
                case ReportTypeEnum.DisabledUser:
                    {
                        sql.Append("WHERE ( IsLockedOut = '1' )");
                        strcountAppend.Append("WHERE ( IsLockedOut = '1' )");
                        break;
                    }
            }
            var whereClause = new StringBuilder();
            //whereClause.Append(" WHERE ");
            var globalFilter = string.Empty;
            var filter = string.Empty;
            if (!string.IsNullOrWhiteSpace(parameter.siteSearch.Trim()))
            {
                globalFilter = parameter.siteSearch.Trim().Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                globalFilter = string.Format("%{0}%", globalFilter);
                whereClause.Append(" and ((username LIKE @GlobalSearchFilter) OR (firstname LIKE @GlobalSearchFilter) OR (LastName LIKE @GlobalSearchFilter) OR (email LIKE @GlobalSearchFilter)) ");
            }
            #endregion
            sql.AppendLine(whereClause.Length > 5 ? whereClause.ToString() : string.Empty);
            sql.AppendLine(") AS TBL ");

            #region ReportType

            #endregion
            //sql.AppendFormat("{0}) AS TBL ", whereClause.Length > 7 ? whereClause.ToString() : string.Empty);
            #region SearchParms

            sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
            sql.AppendFormat("ORDER BY {0} ", sortSql.ToString());
            #region RecordsCount
            result.PagerResource.ResultCount = (int)_context.Query<Int64>(
       string.Format("Select Count(Username) From [User] {0}{1} ", strcountAppend, whereClause.Length > 7 ? whereClause.ToString() : string.Empty),

                   new
                   {
                       StartPage = ((parameter.PageNumber - 1) * parameter.PageSize) + 1,
                       EndPage = (parameter.PageNumber * parameter.PageSize),
                       GlobalSearchFilter = globalFilter,
                       UserNameFilter = userNameFilter,
                       FirstNameFilter = firstNameFilter,
                       LastNameFilter = lastNameFilter,
                       eMailFilter = eMailFilter
                   }).First();

            var sqlQuery = sql.ToString();
            result.UserLstResult = _context.Query<UserListingReports>(
                string.Format(sqlQuery),
                new
                {
                    StartPage = ((parameter.PageNumber - 1) * parameter.PageSize) + 1,
                    EndPage = (parameter.PageNumber * parameter.PageSize),
                    GlobalSearchFilter = globalFilter,
                    UserNameFilter = userNameFilter,
                    FirstNameFilter = firstNameFilter,
                    LastNameFilter = lastNameFilter,
                    EMailFilter = eMailFilter

                }).ToList();

            return result;
            #endregion

            #endregion

        }
        private StringBuilder CheckWhereClauseForAllUsers(string usertype, StringBuilder whereclause)
        {
            StringBuilder sql = new StringBuilder();
            int indexat = whereclause.ToString().IndexOf("AND");
            if (whereclause.Length>30 )
            {
                if (indexat <0)
                {
                    return sql;
                }
                else
                {
                sql.Append(ReplaceFirst(whereclause.ToString(), "AND", "Where"));                     
                }         
            }
            return sql;
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
          var genstring= text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
            return genstring;
        }
    }
}
