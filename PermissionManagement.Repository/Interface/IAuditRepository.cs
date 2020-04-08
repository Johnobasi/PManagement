using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PermissionManagement.Model;
using System.Data;

namespace PermissionManagement.Repository
{
    public interface IAuditRepository
    {
       void AuditLog(AuditTrail log);
       void CreateAuditChange(object ValueBefore, object ValueAfter, IDbTransaction dbTransaction, string[] propertiesToCompare = null);

        //AuditTrailListingResponse GetAuditList(int pageNumber, int pageSize, string sortField, string sortOrder, string searchText, string[] searchFields);
       AuditTrailListingResponse GetAuditList(PagerItemsII auditparameter);
       AuditChangeListingResponse GetAuditChange(PagerItemsII auditChangeparameter);
       string GetAuditChangeRecord(long id);

        T MakerCheckerHandller<T>(T dbVersion, T incomingVersion, string operationType, string moduleName, string modelID, IDbTransaction dbTransaction);
        ItemListingResponse GetItemPendingApprovalList(int pageNumber, int pageSize, string sortField, string sortOrder, string searchText, string[] searchFields);
        string GetLastLogin(string username);
    }

}
