using PermissionManagement.Model;
using System;
using System.Collections.Generic;

namespace PermissionManagement.Repository
{
    public interface IReportsRepository
    {
        ReportsExceptionListingResponse GetExceptionList(PagerItemsII parameter);
        #region ExcelGeneration
        IEnumerable<ReportsExceptionDto> GetExceptionExcel(string exceptionId,string exceptionMessage , DateTime? fromDt, DateTime? toDate); 
        #endregion
        string GetExcepionMessageById(string id);
        UserListingReportsList GetUsersList(AllUserListModel reportType, PagerItemsII parameter);
        IEnumerable<ExpiredUserListingDto> GetExcel(string userType,string searchParam);
        IEnumerable<ExpiredUserListingDto> GetExcelReportForUsers(UserListingReports userInputs,DateTime ? fromDate,DateTime ? toDate, string reportType);
         

    }
}
