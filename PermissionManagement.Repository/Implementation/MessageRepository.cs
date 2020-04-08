using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PermissionManagement.Model;
using System.Data;
using PermissionManagement.Utility;
using Dapper;

namespace PermissionManagement.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IDbConnection context;
        private readonly DapperContext dapperContext;

        public MessageRepository(DapperContext dbContext)
        {
            dapperContext = dbContext;
            context = dbContext.Connection;
        }

        public void SendMessageToQueue(MessageItem mailMessage)
        {
            var sql = new StringBuilder();
            sql.AppendLine("INSERT INTO MessageItem (ToAddress, CcAddress, Subject, HtmlBody, TextBody, RemainingRetryCount, IsSending) ");
            sql.AppendLine("VALUES(@ToAddress, @CcAddress, @Subject, @HtmlBody, @TextBody, @RemainingRetryCount, @IsSending); ");

            context.Execute(sql.ToString(), new
            {
                ToAddress = mailMessage.ToAddress,
                CcAddress = mailMessage.Cc,
                Subject = mailMessage.Subject,
                HtmlBody = mailMessage.HtmlBody,
                TextBody = mailMessage.TextBody,
                RemainingRetryCount = 3,
                IsSending = false
            });
        }

        public IList<MessageItemWithState> GetMessagesToSend(bool allAvailable, TimeSpan lastExecuteTime)
        {
            IList<MessageItemWithState> messages = new List<MessageItemWithState>();
            System.DateTime dateImproverished = Helper.GetLocalDate().Subtract(lastExecuteTime);

            var sql = new StringBuilder();
            sql.Append("SELECT ");
            if (!allAvailable)
            {
                sql.Append("TOP 1 ");
            }
            sql.AppendLine("* FROM MessageItem WHERE IsSending = 0 AND RemainingRetryCount > 0 AND (SentDate IS NULL); ");  // OR LastAttemptDate >= @DateImproverished
            var result = context.Query<MessageOutboundDto>(sql.ToString(), new { DateImproverished = dateImproverished }).ToList();

            messages = result.Select(n => new MessageItemWithState
            {
                StateInfo = new MessageStateInfo
                {
                    IsSending = n.IsSending,
                    LastAttemptDate = n.LastAttemptDate,
                    MessageID = n.MessageID,
                    RetriesLeft = n.RemainingRetryCount,
                    SentDate = n.SentDate,
                    LastFailureReason = n.LastFailureReason
                },
                HtmlBody = n.HtmlBody,
                Subject = n.Subject,
                TextBody = n.TextBody,
                ToAddress = n.ToAddress,
                Cc = n.CcAddress,
                MessageID = n.MessageID
            }).ToList();

            if (result.Count > 0)
            {
                result.ToList().ForEach(n => n.IsSending = true);
            }

            context.Execute("UPDATE MessageItem SET IsSending = @IsSending WHERE MessageID = @MessageID; ", result);
            return messages;
        }

        public void UpdateMessageStatus(MessageStateInfo message)
        {
            context.Execute("UPDATE MessageItem SET IsSending = @IsSending, LastAttemptDate = @LastAttemptDate, SentDate = @SentDate, RemainingRetryCount = @RemainingRetryCount, LastFailureReason = @LastFailureReason WHERE MessageID = @MessageID; ",
                new
                {
                    IsSending = false,
                    LastAttemptDate = Helper.GetLocalDate(),
                    SentDate = message.SentDate,
                    RemainingRetryCount = message.RetriesLeft,
                    MessageID = message.MessageID,
                    LastFailureReason = message.LastFailureReason
                });
        }
    }
}
