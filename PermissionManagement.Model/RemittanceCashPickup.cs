using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Model
{
    public class RemittanceCashPickup : TransactionStatus
    { 
            [IdentityPrimaryKeyAttribute]
            public long Id { get; set; }
            public string IMTO { get; set; }
            public string ReferenceNumber { get; set; }
            public string RemittanceTrackingCode { get; set; }
            public string SenderFirstName { get; set; }
            public string SenderMiddleName { get; set; }
            public string SenderLastName { get; set; }
            public string SenderTelephoneNo { get; set; }
            public string SenderMobileNo { get; set; }
            public string SenderEmail { get; set; }
            public string SenderAddress { get; set; }
            public string SenderNationalityCountryCode { get; set; }
            public string SenderBirthDate { get; set; }
            public string ReceiverFirstName { get; set; }
            public string ReceiverMiddleName { get; set; }
            public string ReceiverLastName { get; set; }
            public string ReceiverTelephoneNo { get; set; }
            public string ReceiverMobileNo { get; set; }
            public string ReceiverEmail { get; set; }
            public string ReceiverAddress { get; set; }
            public string ReceiverNationalityCountryCode { get; set; }
            public string RemittanceRecordingDate { get; set; }
            public string SendingCountryCode { get; set; }
            public string DestinationCountryCode { get; set; }
            public decimal PayingAmount { get; set; }
            public decimal PayoutAmount { get; set; }
            public string PayingCurrencyCode { get; set; }
            public string PayoutCurrencyCode { get; set; }
            public decimal PayoutSettlementRate { get; set; }
            public decimal SettlementPayoutAmount { get; set; }
            public string RemittanceStatus { get; set; }
            public string RecieveOrderCode { get; set; }
            public string SendingReason { get; set; }
            public string RecieverBankName { get; set; }
            public string RecieverBranchName { get; set; }
            public string RecieverAccountNumber { get; set; }
            public string OtherBank { get; set; }
            public decimal PayingToPayOutRate { get; set; }
            public string RecieverBankCode { get; set; }
            public string RecieverBranchCode { get; set; }
            public string RecieverRoutingNumber { get; set; }
            public string RecieverSortCode { get; set; }
            public string RecieverSwiftBIC { get; set; }

            public string SenderFundSource { get; set; }
            public string SenderOccupation { get; set; }
            public string SenderIDType { get; set; }
            public string SenderIDNumber { get; set; }
            public string ReceiverIDType { get; set; }
            public string ReceiverIDNumber { get; set; }
            public string PaymentStatusOverride { get; set; }
            public string OverrideStatus { get; set; }
            public string OverrideActionBy { get; set; }
            public DateTime? OverrideDateTime { get; set; }

            public string ApprovedStatus { get; set; }
            public string InitiatedBy { get; set; }
            public string ApprovedBy { get; set; }
            public string ApprovedLogId { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime RowVersionNo { get; set; }

            public Guid RoleId { get; set; }
            public UserRole UserRole { get; set; }


    }
}
