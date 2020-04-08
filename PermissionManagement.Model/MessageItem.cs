using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PermissionManagement.Utility;

namespace PermissionManagement.Model
{
        public class MessageItem
        {

            private Int64 _messageID;

            [IdentityPrimaryKey()]
            public Int64 MessageID
            {
                get { return _messageID; }
                set { _messageID = value; }
            }

            private string _to;
            public string ToAddress
            {
                get { return _to; }
                set { _to = value; }
            }

            private string _cc;
            public string Cc
            {
                get { return _cc; }
                set { _cc = value; }
            }

            private string _subject;
            public string Subject
            {
                get { return _subject; }
                set { _subject = value; }
            }

            private string _htmlBody;
            public string HtmlBody
            {
                get { return _htmlBody; }
                set { _htmlBody = value; }
            }

            private string _textBody;
            public string TextBody
            {
                get { return _textBody; }
                set { _textBody = value; }
            }

            private IDictionary<string, byte[]> _attachmentList = new Dictionary<string, byte[]>();
            public IDictionary<string, byte[]> AttachmentList
            {
                get { return _attachmentList; }
                set { _attachmentList = value; }
            }

        }

        public class MessageStateInfo
        {

            private Int64 _messageID;
            public Int64 MessageID
            {
                get { return _messageID; }
                set { _messageID = value; }
            }

            private bool _IsSending;
            public bool IsSending
            {
                get { return _IsSending; }
                set { _IsSending = value; }
            }

            private Nullable<System.DateTime> _sentDate;
            public Nullable<System.DateTime> SentDate
            {
                get { return _sentDate; }
                set { _sentDate = value; }
            }

            private int _retriesLeft;
            public int RetriesLeft
            {
                get { return _retriesLeft; }
                set { _retriesLeft = value; }
            }

            private Nullable<System.DateTime> _lastAttemptDate;
            public Nullable<System.DateTime> LastAttemptDate
            {
                get { return _lastAttemptDate; }
                set { _lastAttemptDate = value; }
            }

            public string LastFailureReason { get; set; }

            public void MarkAsComplete()
            {
                _sentDate = Helper.GetLocalDate();
                _retriesLeft = 0;
            }

            public void MarkAsFailed()
            {
                _sentDate = null;
                _retriesLeft = _retriesLeft - 1;
            }

        }

        public class MessageItemWithState : MessageItem
        {

            private MessageStateInfo _stateInfo = new MessageStateInfo();
            public MessageStateInfo StateInfo
            {
                get { return _stateInfo; }
                set { _stateInfo = value; }
            }

        }

        public class MessageOutboundDto
        {
            public string HtmlBody { get; set; }
            public bool IsSending { get; set; }
            public DateTime? LastAttemptDate { get; set; }
            public int MessageID { get; set; }
            public int RemainingRetryCount { get; set; }
            public string LastFailureReason { get; set; }
            public DateTime? SentDate { get; set; }
            public string Subject { get; set; }
            public string TextBody { get; set; }
            public string ToAddress { get; set; }
            public string CcAddress { get; set; }
        }

}
