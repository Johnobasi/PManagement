using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Threading;
using PermissionManagement.Utility;
using System.IO;

namespace PermissionManagement.Web
{
    [SecurityAccess()]
    public class BaseController : Controller
    {
        /// <summary>
        /// To supply additional information to be logged as part of the audit trail
        /// </summary>
        /// <param name="operationMessage">The operation status meesage to log - e.g. Successful or other error message</param>
        /// <param name="operationData">The user supplied data that is of interest to log, this can be a serialized data</param>
        public void SetAuditInfo(string operationMessage, string operationData)
        {
            SetAuditInfo(operationMessage, operationData, Helper.GetLoggedInUserID());
        }

        /// <summary>
        /// To supply additional information to be logged as part of the audit trail
        /// </summary>
        /// <param name="operationMessage">The operation status meesage to log - e.g. Successful or other error message</param>
        /// <param name="operationData">The user supplied data that is of interest to log, this can be a serialized data</param>
        /// <param name="userName">The user performing the action. Use this overloads only on LogIn Action</param>
        public void SetAuditInfo(string operationMessage, string operationData, string userName)
        {
            this.ControllerContext.HttpContext.Items["AuditOperationMessage"] = operationMessage;
            this.ControllerContext.HttpContext.Items["AuditOperationData"] = operationData;
            this.ControllerContext.HttpContext.Items["AuditUserName"] = userName;
        }

        public void Success(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Success, message, dismissable);
        }

        public void Information(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Information, message, dismissable);
        }

        public void Warning(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Warning, message, dismissable);
        }

        public void Danger(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Danger, message, dismissable);
        }

        private void AddAlert(string alertStyle, string message, bool dismissable)
        {
            string value = System.Web.HttpUtility.UrlEncode(alertStyle + '|' + message + '|' + dismissable.ToString());
            System.Web.HttpCookie cookie = System.Web.HttpContext.Current.Response.Cookies[Alert.CookieMessageKey];
            cookie.Values.Add(Alert.CookieMessageKey + (cookie.Values.Count + 1).ToString("00"), value);
        }

        public static String RenderRazorViewToString(ControllerContext controllerContext, String viewName, Object model)
        {
            controllerContext.Controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var ViewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var ViewContext = new ViewContext(controllerContext, ViewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, sw);
                ViewResult.View.Render(ViewContext, sw);
                ViewResult.ViewEngine.ReleaseView(controllerContext, ViewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

    }
}