using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System.Configuration;
using System.Data; 

namespace PermissionManagement.Services
{
    public class AuditService : IAuditService
     {
        private readonly IAuditRepository repositoryInstance;

        public AuditService(IAuditRepository repository)
        {
            repositoryInstance = repository;
        }

        public void AuditLog(AuditTrail log)
        {
            repositoryInstance.AuditLog(log);
        }


        //public void CreateAuditChange(object ValueBefore, object ValueAfter, IDbTransaction dbTransaction)
        //{
        //    repositoryInstance.CreateAuditChange(ValueBefore, ValueAfter, dbTransaction: dbTransaction);
        //    //if (dbTransaction != null)
        //    //{
        //    //    repositoryInstance.CreateAuditChange(ValueBefore, ValueAfter, dbTransaction: dbTransaction);
        //    //}
        //}

       //private void Audit(AuditTrail log)
       // {
       //     var status = true;
                          
       //         AuditLog(new AuditTrail()
       //             {
       //                 ActionDateTime = Helper.GetLocalDate(),
       //                 AuditAction = string.Format("Email Sending Error - {0}"),
       //                 AuditMessage = MessageText,
       //                 AuditPage = "MailSender",
       //                 AuditType = "MailSender",
       //                 AuditVersion = "MailSender 1.0",
       //                 LoggedInUser = Helper.GetLoggedInUser(),
       //                 ClientIPAddress = Helper.GetIPAddress()
       //             }, true);
       //  }

        public AuditTrailListingResponse GetAuditList(PagerItemsII auditparameter)
        {
            return repositoryInstance.GetAuditList(auditparameter);
        }
        public AuditChangeListingResponse GetAuditChange(PagerItemsII auditChangeparameter)
        {
            return repositoryInstance.GetAuditChange(auditChangeparameter);
        }

        public string GetAuditChangeRecord(long id)
        {
            return repositoryInstance.GetAuditChangeRecord(id);
        }
        public ItemListingResponse GetItemPendingApprovalList(int pageNumber, int pageSize, string sortField, string sortOrder, string searchText, string[] searchFields)
        {
            return repositoryInstance.GetItemPendingApprovalList(pageNumber, pageSize, sortField, sortOrder, searchText, searchFields);
        }

     }
}
