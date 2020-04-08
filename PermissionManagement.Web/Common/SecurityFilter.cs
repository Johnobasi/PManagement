using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Diagnostics;
using System.Net.Http;
using PermissionManagement.Model;
using PermissionManagement.Utility;

namespace PermissionManagement.Web
{
      public class SecurityAccessAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        private string _moduleName;
        private string _accessRequired;
        private string[] _accessRequiredList;
        private bool _authorizationStatus;
        private bool? _verifyAreaAccessRight;

        public SecurityAccessAttribute()
        {
            
            _verifyAreaAccessRight = null;
        }

        public SecurityAccessAttribute(bool verifyAreaAccessRight)
        {
            _verifyAreaAccessRight = verifyAreaAccessRight;
        }

        public SecurityAccessAttribute(string moduleName, string accessRequired)
        {
            _moduleName = moduleName;
            _accessRequired = accessRequired;
        }

        public SecurityAccessAttribute(string moduleName, string[] accessRequired)
        {
            _moduleName = moduleName;
            _accessRequiredList = accessRequired;
        }
        private void AddAlert(string alertStyle, string message, bool dismissable)
        {
            string value = System.Web.HttpUtility.UrlEncode(alertStyle + '|' + message + '|' + dismissable.ToString());
            System.Web.HttpCookie cookie = System.Web.HttpContext.Current.Response.Cookies[Alert.CookieMessageKey];
            cookie.Values.Add(Alert.CookieMessageKey + (cookie.Values.Count + 1).ToString("00"), value);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var settings = SecurityConfig.GetCurrent();
            var rootURL = Helper.GetRootURL() + "/";
            string loginUrl = (string.Format("{0}{1}", rootURL, settings.Login.Page)).ToLower();
            string currentURL = Helper.GetCurrentURL();
            var returnURLIndex = currentURL.IndexOf("?ReturnUrl");
            currentURL = returnURLIndex > 0 ? currentURL.Substring(0, returnURLIndex) : currentURL;
            if (!string.IsNullOrEmpty(currentURL) && (currentURL.Equals(loginUrl, StringComparison.OrdinalIgnoreCase) || currentURL.Equals(rootURL, StringComparison.OrdinalIgnoreCase)))
            {
                _authorizationStatus = true;
                return _authorizationStatus;
            }

            if (_accessRequiredList != null)
            {
                _authorizationStatus = Access.IsAccessAllowed(httpContext, _moduleName, _accessRequiredList, _verifyAreaAccessRight);
            }
            else
            {
                _authorizationStatus = Access.IsAccessAllowed(httpContext, _moduleName, _accessRequired, _verifyAreaAccessRight);
            }

            return _authorizationStatus;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (!_authorizationStatus)
            {
                var settings = SecurityConfig.GetCurrent();
                var current = AuthCookie.GetCurrent();
                current.Delete();

                string loginUrl = (settings.Login.Url + settings.Login.Page).ToLower();

                var rawUrl = filterContext.HttpContext.Request.RawUrl;
                var redirectUrl = string.Empty;

                if (!string.IsNullOrEmpty(rawUrl) && rawUrl != loginUrl)
                {
                    if (!rawUrl.Contains("xxkeyxx")) //auto logout key
                    {
                        redirectUrl = "?ReturnUrl=" + HttpUtility.UrlEncode(rawUrl, filterContext.HttpContext.Request.ContentEncoding);
                        AddAlert(AlertStyles.Danger, "Your session has expired or you do not have access to the page you are trying to access.", true);
                    }
                    else
                    {
                        AddAlert(AlertStyles.Danger, string.Format("You have been logged out due to inactivity for {0} minutes. Please log in again.", SecurityConfig.GetCurrent().Cookie.Timeout), true);
                    }
                }              
        
                filterContext.Result = new RedirectResult(loginUrl + redirectUrl);
                return;
            }
        }
    }

}