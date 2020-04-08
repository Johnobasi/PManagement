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
    }
}