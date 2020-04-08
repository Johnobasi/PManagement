using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PermissionManagement.Utility;

namespace PermissionManagement.Web
{
    public static class Utility
    {
        public static string GetFirstName()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return "Anonymous";
            return ((Identity)(HttpContext.Current.User.Identity)).FullName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
        }
        public static string GetLastName()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return "";
            return ((Identity)(HttpContext.Current.User.Identity)).FullName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
        }
    }
}