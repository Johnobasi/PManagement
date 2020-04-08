using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionManagement.Model;
using PermissionManagement.Utility;
using System.Data;

namespace PermissionManagement.Services
{
    public interface IAuditService
    {
        void AuditLog(AuditTrail log);
        AuditTrailListingResponse GetAuditList(PagerItemsII auditparameter);
        AuditChangeListingResponse GetAuditChange(PagerItemsII auditChangeparameter);
        ItemListingResponse GetItemPendingApprovalList(int pageNumber, int pageSize, string sortField, string sortOrder, string searchText, string[] searchFields);

        string GetAuditChangeRecord(long id);
    }
       

}
