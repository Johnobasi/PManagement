using DataTables.Mvc;
using Newtonsoft.Json;
using PermissionManagement.Model;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PermissionManagement.Web
{
    public class AuditController : BaseController
    {
        private IAuditService _auditService;
        private ISecurityService _securityService;

        public AuditController(IAuditService auditService, ISecurityService securityService)
        {
            _auditService = auditService;
            _securityService = securityService;
        }

        [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
        public ActionResult Index()
        {
            var auditList = new AuditTrailListingResponse();
            return View("AuditTrail", auditList);
        }

        #region Audit Trail
        [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
        public ActionResult AuditTrail()
        {
            var auditList = new AuditTrailListingResponse();
            return View(auditList);
        }

        [AuditFilter(AuditLogLevel.LevelFive)]
        [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
        [HttpPost]
        public void AuditTrail(FormCollection[] formData)
        {
            DateTimeHelper dtHelper = new DateTimeHelper();
            string str = Request.Form["XLAuditAction"];
            AuditTrail auditTrail = new AuditTrail();
            auditTrail.Username = Request.Form["XLUserName"];
            auditTrail.AuditAction = Request.Form["XLAuditAction"];
            auditTrail.ActionStartTime = dtHelper.ConvertToDateTime(Request.Form["XLActionStartFrom"].ToString());
            auditTrail.ActionEndTime = dtHelper.ConvertToDateTime(Request.Form["XLActionEndFrom"].ToString());
            List<AuditTrailListingDto> auditReport = _auditService.GetAuditTrailForExport(auditTrail, dtHelper.ConvertToDateTime((Request.Form["XLActionStartTo"].ToString()), true),
                                                                                                       dtHelper.ConvertToDateTime((Request.Form["XLActionEndTo"].ToString()), true));

            new Export().ToFile(auditReport, Response, "AuditTrail_Report");
        }

        [AuditFilter(AuditLogLevel.LevelFive)]
        [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
        public JsonResult ListAuditTrailData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var defaultSortBy = Constants.SortField.ActionStartTime;
            var pagingParameter = MVCExtensionMethods.GetPagingParametersII(requestModel, defaultSortBy, Constants.SortOrder.Descending.ToLower());
            AuditTrailListingResponse auditList = _auditService.GetAuditList(pagingParameter);
            var data = auditList.AuditTrailListingResult;
            var d = new DataTablesResponse((int)requestModel.Draw, data, auditList.PagerResource.ResultCount, auditList.PagerResource.ResultCount);
            return new JsonNetResult() { Data = d, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            //return Json(new DataTablesResponse((int)requestModel.Draw, data, auditList.PagerResource.ResultCount, auditList.PagerResource.ResultCount), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Audit Change
        [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
        public ActionResult AuditChange()
        {
            var auditChange = new AuditChangeListingResponse();
            return View(auditChange);
        }

        [AuditFilter(AuditLogLevel.LevelFive)]
        [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
        [HttpPost]
        public void AuditChange(FormCollection[] formData)
        {
            DateTimeHelper dtHelper = new DateTimeHelper();
            List<AuditChangeListingDto> auditReport = _auditService.GetAuditChangeForExport(
                new AuditChange() { 
                    Username = Request.Form["XLUserName"],
                    TableName = Request.Form["XLTableName"],
                    AffectedRecordID = Request.Form["XLAffectedRecord"],
                    ActionDateTime = dtHelper.ConvertToDateTime(Request.Form["XLActionStartFrom"]),
                    Changes = Request.Form["XLChanges"]
                }, dtHelper.ConvertToDateTime(Request.Form["XLActionStartTo"].ToString(), true));

            new Export().ToFile(auditReport, Response, "AuditChange_Report");
        }

        [AuditFilter(AuditLogLevel.LevelFive)]
        [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
        public JsonResult ListAuditChangeData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var defaultSortBy = Constants.SortField.ActionDateTime;
            var pagingParameter = MVCExtensionMethods.GetPagingParametersII(requestModel, defaultSortBy, Constants.SortOrder.Descending.ToLower());
            AuditChangeListingResponse auditChange = _auditService.GetAuditChange(pagingParameter);
            var data = auditChange.AuditChangeListingResult;
            var d = new DataTablesResponse((int)requestModel.Draw, data, auditChange.PagerResource.ResultCount, auditChange.PagerResource.ResultCount);
            return new JsonNetResult() { Data = d, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
        public string GetAuditChangeData(string id)
        {
            var changeId = long.Parse(id);
            var auditChange = JsonConvert.DeserializeObject<List<AuditChangeDiff>>(_auditService.GetAuditChangeRecord(changeId));
            return AuditChangeFormat(auditChange);
        }

        private string AuditChangeFormat(List<AuditChangeDiff> auditChange)
        {
            string result = string.Empty;
            if (auditChange != null && auditChange.Count > 0) { result = RenderRazorViewToString(this.ControllerContext, "AuditChangeDiff", auditChange); }
            else { result = "No change found."; }
            return result;
        }
        #endregion

        #region Private Methods

        #endregion
    }
}