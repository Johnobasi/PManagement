using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PermissionManagement.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository repositoryInstance;
        private readonly IMessageRepository messageRepositoryInstance;

        public LogService(ILogRepository repository, IMessageRepository messageRepository)
        {
            repositoryInstance = repository;
            messageRepositoryInstance = messageRepository;
        }

        public void LogError(ExceptionLog error, bool fromErrorMail)
        {
            repositoryInstance.LogError(error);
            var send = ConfigurationManager.AppSettings["SendErrorMail"];
            if (!string.IsNullOrEmpty(send))
            {
                var t = false;
                bool.TryParse(send, out t);
                if (t)
                    SendMailToAdmin(error, fromErrorMail);
            }
        }

        private bool SendMailToAdmin(ExceptionLog error, bool fromErrorMail)
        {
            var mailSetting = MailSettings.GetCurrent();
            if (!mailSetting.IsEnabled) return true;

            var companyName = ConfigurationManager.AppSettings["CompanyName"].ToString();

            var contactPerson = mailSetting.Contacts["error_related"];

            var messageFormat = "Dear {1},{0}{0}Please find below a brief information about an error that just occured on your site{0}{0}Error Page: {2}{0}{0}Error Message: {3}{0}{0}User: {4}{0}{0}User IP: {5}{0}{0}Error Time: {6}{0}{0}Regards.{0}{0}{7}";
            var final = string.Format(messageFormat, System.Environment.NewLine, contactPerson.DisplayName, error.ExceptionPage, error.ExceptionMessage, error.LoggedInUser, error.UserIPAddress, error.ExceptionDateTime, companyName);

            var addressList = new List<UserMailDto>();
            addressList.Add(new UserMailDto() { DisplayName = contactPerson.DisplayName, EmailAddress = contactPerson.Email });
            return SendEmail(addressList, null, final, string.Empty, "Site Error Notification", fromErrorMail);
        }

        public bool SendEmail(IList<UserMailDto> toAddressList, IList<UserMailDto> ccAddressList, string messageText, string messageHtml, string subject, bool fromErrorMail)
        {
            var status = true;
            try
            {
                if (toAddressList == null || toAddressList.Count == 0) return false;

                var message = new MessageItem();
                message.HtmlBody = messageHtml;
                message.TextBody = messageText;
                message.Subject = subject;
                message.ToAddress = Helper.ToStringCSV<string>(toAddressList.Select(f => string.Format("{0}|{1}", f.DisplayName ?? f.EmailAddress, f.EmailAddress)).ToArray());
                if (ccAddressList != null && ccAddressList.Count > 0)
                    message.Cc = Helper.ToStringCSV<string>(ccAddressList.Select(f => string.Format("{0}|{1}", f.DisplayName ?? f.EmailAddress, f.EmailAddress)).ToArray());

                messageRepositoryInstance.SendMessageToQueue(message);

            }
            catch (Exception ex)
            {
                //I don't want to rethrow here really, log perhaps.
                if (!fromErrorMail)
                {
                    LogError(new ExceptionLog()
                    {
                        ExceptionDateTime = Helper.GetLocalDate(),
                        ExceptionDetails = string.Format("Email Sending Error - {0}", Helper.ToStringCSV<string>((from m in toAddressList select m.EmailAddress).ToArray())),
                        ExceptionMessage = ex.Message,
                        ExceptionPage = "MailSender",
                        ExceptionType = "MailSender",
                        ExceptionVersion = "MailSender 1.0",
                        LoggedInUser = Helper.GetLoggedInUser(),
                        UserIPAddress = Helper.GetIPAddress()
                    }, true);
                }
                status = false;
            }
            return status;
        }
    }
}
