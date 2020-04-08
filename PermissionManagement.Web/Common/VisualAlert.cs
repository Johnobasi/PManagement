using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PermissionManagement.Utility;

namespace PermissionManagement.Web
{
    public class Alert
    {
        public const string CookieMessageKey = "Messages";

        public string AlertStyle { get; set; }
        public string Message { get; set; }
        public bool Dismissable { get; set; }
    }

    public static class AlertStyles
    {
        public const string Success = "success";
        public const string Information = "info";
        public const string Warning = "warning";
        public const string Danger = "danger";
    }

    public class AlertMessageReader
    {
        public static IList<Alert> GetMessages()
        {
            //If we have message in the cookie, i.e messages was sent from another page.
            var request = HttpContext.Current.Request;
            IList<Alert> messageList = new List<Alert>();
            if (request.Cookies[Alert.CookieMessageKey] != null && request.Cookies[Alert.CookieMessageKey].HasKeys)
            {

                foreach (string cookieName in request.Cookies[Alert.CookieMessageKey].Values)
                {
                    string value = HttpUtility.UrlDecode(request.Cookies[Alert.CookieMessageKey].Values[cookieName]);

                    string[] parts = value.Split('|');

                    messageList.Add(new Alert() { AlertStyle = parts[0], Message = parts[1], Dismissable = parts[2].ToLower() == "true" ? true : false });
                }
            }

            HttpContext.Current.Response.Cookies[Alert.CookieMessageKey].Expires = Helper.GetLocalDate().AddDays(-1);
            return messageList;
        }
    }
}