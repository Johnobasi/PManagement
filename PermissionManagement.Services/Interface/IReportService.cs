using PermissionManagement.Model;
using System;
using System.Collections.Generic;

namespace PermissionManagement.Services
{
    public interface IReportService
    {
        #region Exception Report
        ReportsExceptionListingResponse GetExceptionList(PagerItemsII parameter);
        IEnumerable<ReportsExceptionDto> GetExceptionExcel(string exceptionId, string exceptionMessage, DateTime? fromDt, DateTime? toDate);
        string GetExcepionMessageById(string id);
        #endregion

        #region User List Report 
        UserListingReportsList GetUsersList(AllUserListModel reportType, PagerItemsII parameters);
        #region UsersExcel
        IEnumerable<ExpiredUserListingDto> GetExcel(string userType, string searchParam);
        IEnumerable<ExpiredUserListingDto> GetExcelReportForUsers(UserListingReports userInputs, DateTime? fromDate, DateTime? toDate, string reportType);
        #endregion
        #endregion


    }
}
