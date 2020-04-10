using System;
using System.Collections.Generic;

namespace PermissionManagement.Model
{
    public class AuditChangeDiff
    {
        public string PropertyName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public class AuditChange
    {
        [IdentityPrimaryKey]
        public long AuditChangeID { get; set; }
        public System.DateTime? ActionDateTime { get; set; }
        public string ClientIPAddress { get; set; }
        public string AuditType { get; set; }
        public string Username { get; set; }
        public string ValueBefore { get; set; }
        public string ValueAfter { get; set; }
        public string TableName { get; set; }
        public string Changes { get; set; }
        public string AffectedRecordID { get; set; }
    }

    public class AuditTrail
    {
        [IdentityPrimaryKey]
        public long AuditID { get; set; }
        public System.DateTime? ActionStartTime { get; set; }
        public System.DateTime? ActionEndTime { get; set; }
        public int ActionDurationInMs { get; set; }
        public string AuditAction { get; set; }
        public string ClientIPAddress { get; set; }
        public string Username { get; set; }
        public string AuditPage { get; set; }
        public string AuditType { get; set; }
        public string AuditMessage { get; set; }
        public string AuditData { get; set; }
        public string AuditHTTPAction { get; set; }
    }

    public class AuditTrailListingResponse
    {
        public IList<AuditTrailListingDto> AuditTrailListingResult { get; set; }
        public PagerItems PagerResource { get; set; }
    }

    public class AuditChangeListingResponse
    {
        public IList<AuditChangeListingDto> AuditChangeListingResult { get; set; }
        public PagerItems PagerResource { get; set; }
    }

    public class AuditChangeListingDto
    {
        public long AuditChangeID { get; set; }
        public System.DateTime? ActionDateTime { get; set; }
        public string Changes { get; set; }
        public string ClientIPAddress { get; set; }
        public string Username { get; set; }
        public string TableName { get; set; }
        public string AffectedRecordID { get; set; }
    }

    public class AuditTrailListingDto
    {
        public long AuditID { get; set; }
        public System.DateTime? ActionStartTime { get; set; }
        public System.DateTime? ActionEndTime { get; set; }
        public int ActionDurationInMs { get; set; }
        public string AuditAction { get; set; }
        public string ClientIPAddress { get; set; }
        public string Username { get; set; }
        public string AuditPage { get; set; }
        public string AuditType { get; set; }
        public string AuditMessage { get; set; }
        public string AuditHTTPAction { get; set; }
    }

    public class EditUserModel
    {
        public string Username { get; set; }
        public string Initial { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string StaffPosition { get; set; }
        public Guid RoleId { get; set; }
    }

    public class ApprovalLog
    {
        [IdentityPrimaryKey]
        public long ApprovalLogID { get; set; }
        public string InitiatorID { get; set; }
        public string PossibleVerifierID { get; set; }
        public string VerifierID { get; set; }
        public string ActivityName { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? EntryDate { get; set; }
        public string ActivityUrl { get; set; }
        public string RecordData { get; set; }
        public string LastComment { get; set; }
        public string RecordIdentification { get; set; }
        public string CancellationUrl { get; set; }
    }

    public class ApprovalNotification
    {
        public IList<UserMailDto> NotificationList { get; set; }
        public string NoticeType { get; set; }
        public string ActionUrl { get; set; }
        public UserMailDto InitiatedBy { get; set; }
        public UserMailDto ApprovedBy { get; set; }
        public string Comment { get; set; }
    }

    public class ItemListingResponse
    {
        public IList<ItemListingDto> ItemListingResult { get; set; }
        public PagerItems PagerResource { get; set; }
    }

    public class ItemListingDto
    {
        public long ApprovalLogID { get; set; }
        public string InitiatorID { get; set; }
        public string ActivityName { get; set; }
        public string ActivityUrl { get; set; }
        public string ApprovalStatus { get; set; }
        public string LastComment { get; set; }
    }

}
