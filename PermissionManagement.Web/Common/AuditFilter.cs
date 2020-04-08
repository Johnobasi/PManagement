using DryIoc;
using PermissionManagement.Model;
//using PermissionManagement.IoC;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using System;
using System.Web;
using System.Web.Mvc;

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
        public AuditFilter(AuditLogLevel level, HttpVerbs action = HttpVerbs.Get)
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
            var context = HttpContext.Current;
            int configuredLogLevel = -1;
            if (!(int.TryParse(((IContainer)context.Application["container"]).Resolve<IPortalSettingsService>()
                                .GetSettingByKey(Constants.PortalSettingsKeysConstants.AUDITLOGLEVEL).Value
                                , out configuredLogLevel)))
            {
                configuredLogLevel = Helper.GetLogLevel();
            }
            if (configuredLogLevel >= (int)logLevel)
            {
                var timeNow = Helper.GetLocalDate();
                var action = context.Request.RequestType;
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
                    AuditPage = context.Request.RawUrl.Length > 500 ? context.Request.RawUrl.Substring(0, 500) : context.Request.RawUrl,
                    AuditType = "Audit Trail", //this.GetType().Name
                    AuditMessage = auditMessage,
                    AuditData = auditData,
                    AuditHTTPAction = context.Request.RequestType
                };

                var auditService = ((IContainer)context.Application["container"]).Resolve<IAuditService>();
                auditService.AuditLog(trail);
            }
        }

    }
}