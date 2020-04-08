using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using PermissionManagement.Model;
using PermissionManagement.Utility;
//using PermissionManagement.IoC;
using PermissionManagement.Services;
using DryIoc;

namespace PermissionManagement.Web
{
    public enum AuditLogLevel
    {
        LevelZero = 0,
        LevelOne = 1,
        LevelTwo = 2,
        LevelThree = 3,
        LevelFour = 4,
        LevelFive = 5
    }

    public class AuditFilter : ActionFilterAttribute
    {

        private DateTime actionStartTime;
        private string userName;
        private AuditLogLevel logLevel = AuditLogLevel.LevelZero;
        public AuditFilter()
        {
            logLevel = AuditLogLevel.LevelZero;
        }
        public AuditFilter(AuditLogLevel level)
        {
            logLevel = level;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            actionStartTime = Helper.GetLocalDate();
            userName = Helper.GetLoggedInUserID();
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (Helper.GetLogLevel() >= (int)logLevel)
            {
                var context = HttpContext.Current;
                var timeNow = Helper.GetLocalDate();
                var auditMessage = filterContext.HttpContext.Items["AuditOperationMessage"] as string;
                if (string.IsNullOrEmpty(auditMessage))
                {
                    auditMessage = "Successful";
                }
                else
                {
                    auditMessage = auditMessage.Length > 256 ? auditMessage.Substring(0, 256) : auditMessage;
                }
                var auditData = filterContext.HttpContext.Items["AuditOperationData"] as string;
                if (string.IsNullOrEmpty(auditData))
                {
                    auditData = "N/A";
                }
                else
                {
                    auditData = auditData.Length > 1024 ? auditData.Substring(0, 1024) : auditData;
                }
                var auditUserName = filterContext.HttpContext.Items["AuditUserName"] as string;
                if (!string.IsNullOrEmpty(auditUserName))
                {
                    userName = auditUserName;
                }
                AuditTrail trail = new AuditTrail()
                {
                    ActionStartTime = actionStartTime,
                    ActionEndTime = timeNow,
                    ActionDurationInMs = (int)timeNow.Subtract(actionStartTime).TotalMilliseconds,
                    AuditAction = filterContext.ActionDescriptor.ActionName,
                    ClientIPAddress = Helper.GetIPAddress(),
                    Username = userName == "anonymous" ? Helper.GetLoggedInUserID() : userName,
                    AuditPage = context.Request.RawUrl,
                    AuditType = "Audit Trail", //this.GetType().Name
                    AuditMessage = auditMessage,
                    AuditData = auditData
                };

                var auditService = ((IContainer)context.Application["container"]).Resolve<IAuditService>();
                auditService.AuditLog(trail);
            }
        }

    }
}