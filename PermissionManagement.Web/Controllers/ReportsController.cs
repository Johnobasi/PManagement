using DataTables.Mvc;
using PermissionManagement.Model;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using PermissionManagement.Web;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PermissionManagement.Controllers
{
    public class ReportsController : BaseController
    {
        #region Globalvariables

        private readonly IReportService _reportService;
        private ICacheService _cacheService;
        #endregion

        #region Constructor
        public ReportsController(IReportService reportService, ICacheService iCacheService)
        {
             _reportService = reportService; 
            _cacheService = iCacheService;
        }
        #endregion

        #region Index
        [SecurityAccess()]
        [AuditFilter(AuditLogLevel.LevelFive)]
        public ActionResult Index()
        {
            var exceptionList = new ReportsExceptionListingResponse();          
            return View(exceptionList);
        }
        #endregion

        #region Exception Report

        [SecurityAccess()]
        [AuditFilter(AuditLogLevel.LevelFive)]
        public ActionResult ExceptionList()
        {
            var exceptionList = new ReportsExceptionListingResponse();
            return View(exceptionList);
        }

        [SecurityAccess()]
        [HttpPost]
        [AuditFilter(AuditLogLevel.LevelFive)]
        public JsonResult ListExceptionData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var defaultSortBy = Constants.SortField.Username;
            var pagingParameter = MVCExtensionMethods.GetPagingParametersII(requestModel, defaultSortBy, Constants.SortOrder.Ascending.ToLower());

            ReportsExceptionListingResponse exceptionList = _reportService.GetExceptionList(pagingParameter);
            var data = exceptionList.UserListingResult;            
            return Json(new DataTablesResponse((int)requestModel.Draw, data, exceptionList.PagerResource.ResultCount, exceptionList.PagerResource.ResultCount), JsonRequestBehavior.AllowGet);
        }

        [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
        public string GetExcepionMessageById(string id)
        {
            var auditChange = _reportService.GetExcepionMessageById(id); //JsonConvert.DeserializeObject<List<string>>(_reportService.GetExcepionMessageById(id));
            return auditChange.ToString();
        }
        #region ExportExcel
        [SecurityAccess]
        [CompressFilter]
        [HttpPost]
        public void ExceptionList(FormCollection formCollection)
        {
            DateTimeHelper dateTimeHelper = new DateTimeHelper();
            string exceptionId = Request.Form["ExportExceptionID"];
            string exceptionMessage = Request.Form["ExportExceptionMessage"];
            DateTime? exceptionFromDate = dateTimeHelper.ConvertToDateTime(Request.Form["ExportExceptionfrom-date"]);
            DateTime? exceptionToDateTime = dateTimeHelper.ConvertToDateTime(Request.Form["ExportExceptionfrom-to-date"], true);
            IEnumerable<ReportsExceptionDto> excelData =_reportService.GetExceptionExcel(exceptionId, exceptionMessage, exceptionFromDate, exceptionToDateTime);
            new Export().ToFile(excelData, Response,"Exceptions_Report");
        } 
        #endregion
        #endregion

        #region User Report
        #region UsersList
        [SecurityAccess()]
        [AuditFilter(AuditLogLevel.LevelFive)]
        public ActionResult UsersList()
        {
            var userList = new AllUserListModel();                         
            return View(userList);
        }

        [SecurityAccess()]
        [AuditFilter(AuditLogLevel.LevelFive)]
        [HttpPost]
        public ActionResult UsersList(AllUserListModel requestModel)
        {
            var expiredUserList = new AllUserListModel();             
            IDataTablesRequest request = new DefaultDataTablesRequest();
            return View();
        }

        [SecurityAccess]
        [CompressFilter]
        [HttpPost]
        [AuditFilter(AuditLogLevel.LevelZero)]
        public void ExportExcel(string reportType, string searchKey)
        {
            #region local variables declaration
            DateTimeHelper dateTimeHelper = new DateTimeHelper();
            string userName = Request.Form["dUserName"];
            string firstName = Request.Form["dFirstName"];
            string lastName = Request.Form["dLastName"];
            string mail = Request.Form["dEmail"];
            UserListingReports uiInputSearchParam = new UserListingReports() { UserName = userName, FirstName = firstName, LastName = lastName, Email = mail };
            DateTime? fromDate = dateTimeHelper.ConvertToDateTime(Request.Form["dfrom-date"]);
            DateTime? toDate = dateTimeHelper.ConvertToDateTime(Request.Form["dto-date"], true); 
            #endregion

            IEnumerable<ExpiredUserListingDto> excelData =_reportService.GetExcelReportForUsers(uiInputSearchParam, fromDate, toDate, reportType);
           new Export().ToFile(excelData, Response, "UsersList_Report");
        }
        #endregion

        #region  User Data
        [SecurityAccess()]
        [AuditFilter(AuditLogLevel.LevelFive)]
        public JsonResult UserData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string reportType)// string reportType
        {
            var defaultSortBy = Constants.SortField.Username;
            var pagingParameter = MVCExtensionMethods.GetPagingParametersII(requestModel, defaultSortBy, Constants.SortOrder.Ascending.ToLower());
            AllUserListModel model = new AllUserListModel();
            if (reportType.ToLower() == ReportTypeEnum.DormantUser.ToString().ToLower())
            { model.ReportTypeEnum = ReportTypeEnum.DormantUser; }
            else if (reportType.ToLower() == ReportTypeEnum.NewUser.ToString().ToLower())
            { model.ReportTypeEnum = ReportTypeEnum.NewUser; }
            else if (reportType .ToLower() == ReportTypeEnum.DisabledUser.ToString().ToLower())
            { model.ReportTypeEnum = ReportTypeEnum.DisabledUser; }
            else if (reportType.ToLower() == ReportTypeEnum.ExpiredAccount.ToString().ToLower()){model.ReportTypeEnum = ReportTypeEnum.ExpiredAccount;}
            UserListingReportsList userListingReportsList = _reportService.GetUsersList(model, pagingParameter);
            var data = userListingReportsList.UserLstResult;
            return Json(new DataTablesResponse((int)requestModel.Draw, data, userListingReportsList.PagerResource.ResultCount, userListingReportsList.PagerResource.ResultCount), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

         
    }
}