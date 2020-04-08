using PermissionManagement.Model;
using System;
using System.Collections.Generic;

namespace PermissionManagement.Services
{
    public interface IAuditService
    {
        void AuditLog(AuditTrail log);
        AuditTrailListingResponse GetAuditList(PagerItemsII auditparameter);
        AuditChangeListingResponse GetAuditChange(PagerItemsII auditChangeparameter);
        ItemListingResponse GetItemPendingApprovalList(int pageNumber, int pageSize, string sortField, string sortOrder, string searchText, string[] searchFields);

        string GetAuditChangeRecord(long id);

        List<AuditTrailListingDto> GetAuditTrailForExport(AuditTrail searchCriteria, DateTime? actionStartTo = null, DateTime? actionEndTo = null);
        List<AuditChangeListingDto> GetAuditChangeForExport(AuditChange searchCriteria, DateTime? actionDateTo);
    }
       

}
