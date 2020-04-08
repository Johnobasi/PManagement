using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PermissionManagement.Model;
using PermissionManagement.Utility;
using PermissionManagement.Services;
using PermissionManagement.Validation;
using System.Configuration;
using PermissionManagement.Repository;
using DataTables.Mvc;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

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

         [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
         public ActionResult AuditTrail()
         {
             var auditList = new AuditTrailListingResponse();
             return View(auditList);
         }
       
         [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)]
         public JsonResult ListAuditTrailData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
         {
             var defaultSortBy = Constants.SortField.ActionStartTime;
             var pagingParameter = MVCExtensionMethods.GetPagingParametersII(requestModel, defaultSortBy, Constants.SortOrder.Descending.ToLower());

             AuditTrailListingResponse auditList = _auditService.GetAuditList(pagingParameter);
             var data = auditList.AuditTrailListingResult;
             
             var d = new DataTablesResponse((int)requestModel.Draw, data, auditList.PagerResource.ResultCount, auditList.PagerResource.ResultCount);
             return new JsonNetResult() { Data = d, JsonRequestBehavior =  JsonRequestBehavior.AllowGet };
             //return Json(new DataTablesResponse((int)requestModel.Draw, data, auditList.PagerResource.ResultCount, auditList.PagerResource.ResultCount), JsonRequestBehavior.AllowGet);
         }

       [SecurityAccess(Constants.Modules.AuditTrail, Constants.AccessRights.View)] 
         public ActionResult AuditChange()
         {
             var auditChange = new AuditChangeListingResponse();
             return View(auditChange);
         }
        
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
           if (auditChange != null && auditChange.Count > 0)
           {
               result = RenderRazorViewToString(this.ControllerContext, "AuditChangeDiff", auditChange);
           }
           else
           {
               result = "No change found.";
           }

           return result;
       }

        //public ActionResult Index()
        //{
        //    var defaultSortBy = Constants.SortField.Username;
        //    var pagingParameter = MVCExtensionMethods.GetPagingParameters(ControllerContext.RequestContext.HttpContext.Request, defaultSortBy);
        //    pagingParameter.PageSize = 10000;

        //    AuditTrailListingResponse auditList = _auditService.GetAuditList(pagingParameter.PageNumber, pagingParameter.PageSize, pagingParameter.SortBy, pagingParameter.SortDirection, string.Empty, null);
        //    return View("AuditTrail", auditList);
        //}

        //public ActionResult AuditTrail()
        //{
        //    var defaultSortBy = Constants.SortField.AuditID;
        //    var pagingParameter = MVCExtensionMethods.GetPagingParameters(ControllerContext.RequestContext.HttpContext.Request, defaultSortBy);
        //    pagingParameter.PageSize = 10000;

        //    AuditTrailListingResponse auditList = _auditService.GetAuditList(pagingParameter.PageNumber, pagingParameter.PageSize, pagingParameter.SortBy, pagingParameter.SortDirection, string.Empty, null);
        //    return View(auditList);
        //}
       //  public ActionResult AuditChange()
       //{
       //    var defaultSortBy = Constants.SortField.Username;
       //    var pagingParameter = MVCExtensionMethods.GetPagingParameters(ControllerContext.RequestContext.HttpContext.Request, defaultSortBy);
       //    pagingParameter.PageSize = 10000;

       //    AuditChangeListingResponse auditChange = _auditService.GetAuditChange(pagingParameter.PageNumber, pagingParameter.PageSize, pagingParameter.SortBy, pagingParameter.SortDirection, string.Empty, null);
       //    return View(auditChange);
       //}

    }
}