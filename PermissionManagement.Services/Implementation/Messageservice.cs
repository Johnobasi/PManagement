using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Threading;
using System.IO;
using System.Management;
using PermissionManagement.Utility;
using PermissionManagement.Repository;
using PermissionManagement.Model;
using System.Net.Mail;
using System.Net.Mime;

namespace PermissionManagement.Services
{
        public class MessageService : IMessageService
    {
        private readonly IMessageRepository repositoryInstance;
        private System.Threading.Timer timer;
        private readonly MailSettings mailSetting;
        public MessageService(IMessageRepository repository)
        {
            var timerDelegate = new TimerCallback(SendFromQueue);
            timer = new System.Threading.Timer(timerDelegate, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            repositoryInstance = repository;
            mailSetting = MailSettings.GetCurrent();
        }

        public void Start()
        {
            timer.Change(new TimeSpan(0, 0, 1, 0, 0), new TimeSpan(0, 0, 1, 0, 0));
        }

        private void SendFromQueue(object state)
        {
            //suspend timer until this method completes
            timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            try
            {
                SendQueuedEmails();
            }
            catch (Exception ) { }
            finally
            {
                timer.Change(new TimeSpan(0, 0, 5, 0, 0), new TimeSpan(0, 0, 5, 0, 0));
            }
        }

        private bool SendEmail(IList<UserMailDto> toAddressList, IList<UserMailDto> ccAddressList, string messageText, string messageHtml, string subject, bool fromErrorMail)
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

                repositoryInstance.SendMessageToQueue(message);

            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }

        private void SendQueuedEmails()
        {

            if (mailSetting.IsEnabled)
            {
                IList<MessageItemWithState> messagesToSend = repositoryInstance.GetMessagesToSend(true, new TimeSpan(0, 0, 5, 0, 0));
                foreach (MessageItemWithState m in messagesToSend)
                {
                    try
                    {
                        SendMessage(m);
                        m.StateInfo.LastFailureReason = string.Empty;
                        m.StateInfo.MarkAsComplete();
                    }
                    catch (Exception ex)
                    {
                        m.StateInfo.LastFailureReason = ex.Message;
                        m.StateInfo.MarkAsFailed();
                    }
                    finally
                    {
                        repositoryInstance.UpdateMessageStatus(m.StateInfo);
                    }
                }
            }
        }

        private void SendMessage(MessageItemWithState m)
        {
            var mailItem = new MailMessage();

            try
            {
                foreach (string s in m.ToAddress.Split(','))
                {
                    var splitted = s.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var mailAddress = new MailAddress(splitted[1], splitted[0]);
                    mailItem.To.Add(mailAddress);
                }

                if (!string.IsNullOrEmpty(m.Cc))
                {
                    foreach (string s in m.Cc.Split(','))
                    {
                        var splitted = s.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        var mailAddress = new MailAddress(splitted[1], splitted[0]);
                        mailItem.CC.Add(mailAddress);
                    }
                }

                mailItem.Body = m.TextBody;

                //first we create the Plain Text part
                if (!String.IsNullOrEmpty(m.TextBody))
                {
                    AlternateView plainView = AlternateView.CreateAlternateViewFromString(m.TextBody, System.Text.Encoding.UTF8, MediaTypeNames.Text.Plain);
                    mailItem.AlternateViews.Add(plainView);
                }

                //then we create the Html part
                if (!String.IsNullOrEmpty(m.HtmlBody))
                {
                    AlternateView plainView = AlternateView.CreateAlternateViewFromString(m.HtmlBody, System.Text.Encoding.UTF8, MediaTypeNames.Text.Html);
                    mailItem.AlternateViews.Add(plainView);
                }

                mailItem.From = new MailAddress(mailSetting.From, mailSetting.FromName);
                mailItem.IsBodyHtml = !String.IsNullOrEmpty(m.HtmlBody);
                mailItem.Priority = MailPriority.Normal;
                mailItem.Subject = m.Subject;

                var mailService = new SmtpClient();
                mailService.Host = mailSetting.Server;
                mailService.Port = mailSetting.Port;
                mailService.EnableSsl = mailSetting.UseSsl;

                if (!string.IsNullOrEmpty(mailSetting.Login) && !string.IsNullOrEmpty(mailSetting.Password))
                {
                    var cred = new System.Net.NetworkCredential(mailSetting.Login, mailSetting.Password);
                    mailService.Credentials = cred;
                }

                mailService.Send(mailItem);
                mailItem.Dispose();
            }
            finally
            {
                if (mailItem != null) { mailItem.Dispose(); }
            }
        }
    }
}
