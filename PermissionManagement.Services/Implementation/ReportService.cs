using PermissionManagement.Model;
using PermissionManagement.Repository;
using System;
using System.Collections.Generic;

namespace PermissionManagement.Services
{
    public class ReportService : IReportService
    {
        #region GlobalVariables
        private readonly IReportsRepository _reportsRepo;
        private IPortalSettingsService portalSettingsService;
        #endregion

        #region constructor 
        public ReportService(IReportsRepository reportsRepository, IPortalSettingsService portalSettingsServiceInstance)
        {
            _reportsRepo = reportsRepository;
            portalSettingsService = portalSettingsServiceInstance;
        }
        #endregion

        #region Exception Service
        public ReportsExceptionListingResponse GetExceptionList(PagerItemsII parameter)
        {
            return _reportsRepo.GetExceptionList(parameter);
        }
        public string GetExcepionMessageById(string id)
        {
            return _reportsRepo.GetExcepionMessageById(id);
        }
       public IEnumerable<ReportsExceptionDto> GetExceptionExcel(string exceptionId, string exceptionMessage, DateTime? fromDt, DateTime? toDate)
       {
           return _reportsRepo.GetExceptionExcel(exceptionId, exceptionMessage, fromDt, toDate);
       }
        #endregion

        #region User List Service
        public UserListingReportsList GetUsersList(AllUserListModel reportType, PagerItemsII parameters)
        {
            UserListingReportsList UserListingReportListResponse = _reportsRepo.GetUsersList(reportType, parameters);
            return UserListingReportListResponse;
        }
       public IEnumerable<ExpiredUserListingDto> GetExcel(string userType,string searchKey)
       {
           return _reportsRepo.GetExcel(userType, searchKey);
       }

        public IEnumerable<ExpiredUserListingDto> GetExcelReportForUsers(UserListingReports userInputs,
            DateTime? fromDate, DateTime? toDate, string reportType)
        {
            return _reportsRepo.GetExcelReportForUsers(userInputs, fromDate, toDate, reportType);
        }
        #endregion
    }
}