using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMTO.Common.RemitlyCommon
{
    public class BaseAction  //start, release, reverse, approve
    {
        public string remarks { get; set; }
        public string hostname { get; set; }
        public string employee { get; set; }
        public string action { get; set; }
    }

    public class RejectAction : BaseAction  //abort, reject, review, notify
    {
        public string error_code { get; set; }
    }

    public class CompleteAction : BaseAction  //complete
    {
        public DateTime completed_on { get; set; }
    }

    public enum ErrorCode
    {
        
        INVALID_ACCOUNT_NUMBER,
        INVALID_ROUTING_NUMBER,
        INVALID_ACCOUNT_TYPE,
        INVALID_ACCOUNT_CURRENCY,
        RECEIVER_NAME_MISMATCH,
        INVALID_RECEIVER_ADDRESS,
        INVALID_RECEIVER_ID,
        INVALID_RECEIVER_PHONE,
        ABOVE_DAILY_TXN_COUNT_LIMIT,
        ABOVE_DAILY_AMT_RECEIVE_LIMIT,
        LIMIT_EXCEEDED,
        REQUIRE_PURPOSE_CODE,
        REQUIRE_RELATIONSHIP,
        REQUIRE_SOURCE_OF_FUNDS,
        REQUIRE_SENDER_INFO,
        RISK_DECLINE,
        SANCTIONS_DECLINE,
        INVALID_SENDER_DETAILS,
        INVALID_RECEIVER_DETAILS,
        INVALID_EXCHANGE_RATE,
        PARTNER_FUNDING_ACCT_INSUFFICIENT,
        EXPIRED_TRANSACTION,
        COMPLIANCE_FORM_REQUIRED,
        AUTHORIZATION_FORM_REQUIRED,
        UNABLE_TO_CONTACT_RECEIVER,
        RECEIVER_KYC_FAILED,
        NETWORK_ERROR,
        OTHER_ERROR
    }

}
