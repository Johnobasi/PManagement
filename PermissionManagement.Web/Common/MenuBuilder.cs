using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.Security;
using System.Web.Profile;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using PermissionManagement.Utility;

namespace PermissionManagement.Web
{
    public class MenuBuilder
    {

        public MenuBuilder()
        {
        }

        public static Menu MakeMenu(string displayName, bool hasChildren, bool linkable, string menuUrl, string menuLiClass, string menuUlClass, Menu parent)
        {
            return new Menu()
            {
                DisplayName = displayName,
                HasChildren = hasChildren,
                Linkable = linkable,
                MenuLiClass = menuLiClass,
                MenuUlClass = menuUlClass,
                MenuUrl = menuUrl,
                Parent = parent
            };
        }
        public static IList<Menu> GetMenu(string userRole, string currentRequestPath)
        {
            var menuList = BuildMenu(userRole, currentRequestPath);
            return menuList;
        }

        public static IList<Menu> BuildMenu(string userRole, string currentRequestPath)
        {
            IList<Menu> menuList = new List<Menu>();
            var menuItem = new Menu();
            var childItem = new Menu();

            if (Access.IsAccessRightInRoleProfile(Constants.Modules.UserSetup, Constants.AccessRights.View))
            {
                menuItem = MakeMenu("User Setup", true, false, "#", "fa fa-users fa-fw", "nav nav-second-level", null);
                menuList.Add(menuItem);
                childItem = MakeMenu("User List", false, true, "/UserSetup/ListUser", "fa fa-list fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
                childItem = MakeMenu("Add New User", false, true, "/UserSetup/CreateUser", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
                childItem = MakeMenu("Role List", false, true, "/UserSetup/ListRole", "fa fa-list fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
                childItem = MakeMenu("Add New Role", false, true, "/UserSetup/CreateRole", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
            }

            if (Access.IsAccessRightInRoleProfile(Constants.Modules.Report, Constants.AccessRights.View))
            {
                menuItem = MakeMenu("Reports", true, false, "#", "fa fa-building-o fa-fw", "nav nav-second-level", null);
                menuList.Add(menuItem);
                childItem = MakeMenu("Reports List", false, true, "/Reports/ListReports", "fa fa-list fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
            }

            if (Access.IsAccessRightInRoleProfile(Constants.Modules.AuditTrail, Constants.AccessRights.View))
            {
                menuItem = MakeMenu("Audit Trail", true, false, "#", "fa fa-building-o fa-fw", "nav nav-second-level", null);
                menuList.Add(menuItem);
                childItem = MakeMenu("Activity Log", false, true, "/Audit/AuditTrail", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
                childItem = MakeMenu("Change Log", false, true, "/Audit/AuditChange", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
            }
            if (Helper.IsAuthenticated())
            {
                menuItem = MakeMenu("My Profile", true, false, "#", "fa fa-building-o fa-fw", "nav nav-second-level", null);
                menuList.Add(menuItem);
                childItem = MakeMenu("My Details", false, true, "/MyProfile", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
                childItem = MakeMenu("Change Password", false, true, "/MyProfile/ChangePassword", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
            }

            return menuList;
        }

        public static string BuildII(IList<Menu> menuList, string currentRequestPath)
        {

            StringBuilder menuHtml = new StringBuilder();
            IEnumerable<Menu> topLevel = (from m in menuList where m.Parent == null select m);
            string displayName = string.Empty;


            foreach (Menu menuItem in topLevel)
            {
                displayName = menuItem.DisplayName;
                menuHtml.AppendLine(GetBeginLiII(menuItem, currentRequestPath) + GetLinkII(menuItem, currentRequestPath));
                IEnumerable<Menu> level = (from m in menuList where m.Parent != null select m).Where(i => i.Parent.DisplayName == displayName).Select(k => k);

                if (level.Count() > 0)
                {
                    menuHtml.AppendLine(string.Format("<ul class=\"{0}\">", menuItem.MenuUlClass));
                    string html = BuildLevelII(menuList, level, currentRequestPath);
                    menuHtml.AppendLine(html);
                    menuHtml.AppendLine("</ul>");

                }
                menuHtml.AppendLine("</li>");
            }

            return menuHtml.ToString();

        }

        private static string BuildLevelII(IList<Menu> menuList, IEnumerable<Menu> level, string currentRequestPath)
        {

            StringBuilder menuHtml = new StringBuilder();
            string displayName = string.Empty;


            foreach (Menu menuItem in level)
            {
                displayName = menuItem.DisplayName;
                menuHtml.AppendLine(GetBeginLiII(menuItem, currentRequestPath) + GetLinkII(menuItem, currentRequestPath));
                IEnumerable<Menu> level2 = (from m in menuList where m.Parent != null select m).Where(i => i.Parent.DisplayName == displayName).Select(k => k);

                if (level2.Count() > 0)
                {
                    menuHtml.AppendLine(string.Format("<ul class=\"{0}\">", menuItem.MenuUlClass));
                    string html = BuildLevelII(menuList, level2, currentRequestPath);
                    menuHtml.AppendLine(html);
                    menuHtml.AppendLine("</ul>");

                }
                menuHtml.AppendLine("</li>");
            }

            return menuHtml.ToString();

        }

        private static string GetBeginLiII(Menu menuItem, string currentRequestPath)
        {
            if (menuItem.DisplayName == "Divider")
                return "<li class=\"divider\">";

            return "<li>";
        }

        private static string GetLinkII(Menu menuItem, string currentRequestPath)
        {

            if (menuItem.DisplayName == "Divider") return string.Empty;

            //first decide if the urllink is the current request url, so we make its href to be #
            string menuUrl = Helper.GetRootURL() + menuItem.MenuUrl;
            if (menuUrl.ToLower().Trim() == currentRequestPath.ToLower().Trim() | !menuItem.Linkable)
            {
                menuUrl = "#";
            }
            //string format = "<a href=\"{0}\" title=\"{1}\">{3}{2}{4}</a>";
            string format = "<a href=\"{0}\" title=\"{1}\"><i class=\"{3}\"></i>{2}{4}</a>";
            //return string.Format(format, menuUrl, menuItem.DisplayName, menuItem.DisplayName,  menuItem.Parent != null ? string.Empty : string.Format("<i class=\"{0}\"></i>", menuItem.MenuLiClass), menuItem.HasChildren ? "<span class=\"fa arrow\"></span>" : string.Empty);
            return string.Format(format, menuUrl, menuItem.DisplayName, menuItem.DisplayName, menuItem.MenuLiClass, menuItem.HasChildren ? "<span class=\"fa arrow\"></span>" : string.Empty);

        }

        private static string BuildIII(IList<Menu> menuList, string currentRequestPath)
        {

            StringBuilder menuHtml = new StringBuilder();
            IEnumerable<Menu> topLevel = (from m in menuList where m.Parent == null select m);
            string displayName = string.Empty;


            foreach (Menu menuItem in topLevel)
            {
                displayName = menuItem.DisplayName;
                menuHtml.AppendLine(GetBeginLiIII(menuItem, currentRequestPath) + GetLinkIII(menuItem, currentRequestPath));
                IEnumerable<Menu> level = (from m in menuList where m.Parent != null select m).Where(i => i.Parent.DisplayName == displayName).Select(k => k);

                if (level.Count() > 0)
                {
                    menuHtml.AppendLine(string.Format("<ul class=\"{0}\">", menuItem.MenuUlClass));
                    string html = BuildLevelIII(menuList, level, currentRequestPath);
                    menuHtml.AppendLine(html);
                    menuHtml.AppendLine("</ul>");

                }
                menuHtml.AppendLine("</li>");
            }

            return menuHtml.ToString();

        }

        private static string BuildLevelIII(IList<Menu> menuList, IEnumerable<Menu> level, string currentRequestPath)
        {

            StringBuilder menuHtml = new StringBuilder();
            string displayName = string.Empty;


            foreach (Menu menuItem in level)
            {
                displayName = menuItem.DisplayName;
                menuHtml.AppendLine(GetBeginLiIII(menuItem, currentRequestPath) + GetLinkIII(menuItem, currentRequestPath));
                IEnumerable<Menu> level2 = (from m in menuList where m.Parent != null select m).Where(i => i.Parent.DisplayName == displayName).Select(k => k);

                if (level2.Count() > 0)
                {
                    menuHtml.AppendLine(string.Format("<ul class=\"{0}\">", menuItem.MenuUlClass));
                    string html = BuildLevelIII(menuList, level2, currentRequestPath);
                    menuHtml.AppendLine(html);
                    menuHtml.AppendLine("</ul>");

                }
                menuHtml.AppendLine("</li>");
            }

            return menuHtml.ToString();

        }

        private static string GetBeginLiIII(Menu menuItem, string currentRequestPath)
        {
            if (menuItem.DisplayName == "Divider")
                return "<li class=\"divider\">";

            string menuUrl = Helper.GetRootURL() + menuItem.MenuUrl;
            if (menuUrl.ToLower().Trim() == currentRequestPath.ToLower().Trim())
            {
                return "<li class=\"active\">";
            }
            if (menuItem.HasChildren) { return "<li class=\"dropdown\">"; }
            return "<li>";
        }

        private static string GetLinkIII(Menu menuItem, string currentRequestPath)
        {
            if (menuItem.DisplayName == "Divider") return string.Empty;

            //first decide if the urllink is the current request url, so we make its href to be #
            string menuUrl = Helper.GetRootURL() + menuItem.MenuUrl;
            if (menuUrl.ToLower().Trim() == currentRequestPath.ToLower().Trim() | !menuItem.Linkable)
            {
                menuUrl = "#";
            }
            if (menuItem.HasChildren)
            {
                string format = "<a href=\"{0}\" data-toggle=\"dropdown\" class=\"dropdown-toggle\" title=\"{1}\">{2}<b class=\"caret\"></b></a>";
                return string.Format(format, "#", menuItem.DisplayName, menuItem.DisplayName);
            }
            else
            {
                string format = "<a href=\"{0}\" title=\"{1}\">{2}</a>";
                return string.Format(format, menuUrl, menuItem.DisplayName, menuItem.DisplayName);
            }
        }
    }
}