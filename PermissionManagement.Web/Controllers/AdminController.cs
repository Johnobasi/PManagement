using PermissionManagement.Model;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using System.Web.Mvc;

namespace PermissionManagement.Web
{
    public class AdminController : BaseController
    {
        private ISecurityService _securityService;
        private IAuditService _auditService;
        public AdminController(ISecurityService securityService, IAuditService auditService)
        {
            _securityService = securityService;
            _auditService = auditService;
        }
        // GET: Admin
        [SecurityAccess()]
        public ActionResult Index()
        {
            var defaultSortBy = Constants.SortField.ActivityName;
            var pagingParameter = MVCExtensionMethods.GetPagingParameters(ControllerContext.RequestContext.HttpContext.Request, defaultSortBy);
            pagingParameter.PageSize = 10000;
            ItemListingResponse itemPendingApprovalList = _auditService.GetItemPendingApprovalList(pagingParameter.PageNumber, pagingParameter.PageSize, pagingParameter.SortBy, pagingParameter.SortDirection, string.Empty, null);
            return View(itemPendingApprovalList);
        }

        [SecurityAccess()]
        [AuditFilter(AuditLogLevel.LevelOne)]
        public ActionResult CancelRequest(string id, string d)
        {
            var moduleName = Crypto.Decrypt(Helper.Base64Decode(d));

            var url = Helper.GetRootURL() + "/admin";
            //check login user has edit right on the module
            var IsAllowed = Access.AreAccessRightsInRoleProfile(moduleName, new string[] { Constants.AccessRights.Create, Constants.AccessRights.MakeOrCheck, Constants.AccessRights.Edit }, false);

            if (!IsAllowed)
            {
                Danger("You do not the permssion to cancel this request", true);
                return new RedirectResult(url);
            }

            string finalMessage = string.Empty;
            bool status = false;
            switch (moduleName)
            {
                case Constants.Modules.UserSetup:
                    status = _securityService.CancelUserSetupRequest(id);
                    break;
                default:
                    finalMessage = "Unrecognised request cancellation";
                    break;
            }
            if (!status && string.IsNullOrEmpty(finalMessage))
            {
                finalMessage = "Unable to cancel this request. Please try again.";
            }

            if (!string.IsNullOrEmpty(finalMessage))
            {
                Danger(finalMessage, true);
            }
            SetAuditInfo(string.IsNullOrEmpty(finalMessage) ? "Successful" : finalMessage,
            string.Format("Record ID: {0}, Module Name: {1}", id, moduleName),
            Helper.GetLoggedInUserID());

            return new RedirectResult(url);
        }
    }
}