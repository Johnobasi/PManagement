using PermissionManagement.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
                Parent = parent,
                Id = "mnu" + Regex.Replace(displayName, @"\s+", "")
            };
        }
        public static IList<Menu> GetMenu(Identity userContext, string currentRequestPath)
        {
            var menuList = BuildMenu(userContext.Roles, userContext.AccountType, currentRequestPath);
            return menuList;
        }

        public static IList<Menu> BuildMenu(string userRole, string accountType, string currentRequestPath)
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

            if (Access.IsAccessRightInRoleProfile(Constants.Modules.RemitlyPayout, Constants.AccessRights.View))
            {
                menuItem = MakeMenu("RemitlyPayOut IMTO", true, false, "#", "fa fa-users fa-fw", "nav nav-second-level", null);
                menuList.Add(menuItem);
                childItem = MakeMenu("List Branch CashPickUp", false, true, "/RemitlyPickout/Index", "fa fa-list fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
                childItem = MakeMenu("Fetch Trnasfer", false, true, "/RemitlyPayout/RetrieveReference", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
                childItem = MakeMenu("Edit Transfer", false, true, "/RemitlyPayout/EditRemitlyCashPayout", "fa fa-list fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
                childItem = MakeMenu("Approve Transfer", false, true, "/RemitlyPayout/ApproveRemitlyCashPayout", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
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
                if (!string.IsNullOrWhiteSpace(accountType))
                {
                    if (!string.Equals(accountType.ToLower(), Constants.AccountType.ADLocal.ToLower()) &&
                        !string.Equals(accountType.ToLower(), Constants.AccountType.ADFinacle.ToLower()))
                    {
                        menuItem = MakeMenu("My Profile", true, false, "#", "fa fa-building-o fa-fw", "nav nav-second-level", null);
                        menuList.Add(menuItem);
                        childItem = MakeMenu("My Details", false, true, "/MyProfile", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                        menuList.Add(childItem);
                        childItem = MakeMenu("Change Password", false, true, "/MyProfile/ChangePassword", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                        menuList.Add(childItem);
                    }
                }
            }

            if (Access.IsAccessRightInRoleProfile(Constants.Modules.UserSetup, Constants.AccessRights.View))
            {
                GenerateSiteSettingsMenu(menuList);
            }

            if (Access.IsAccessRightInRoleProfile(Constants.Modules.Reports, Constants.AccessRights.View))
            {
                menuItem = MakeMenu("Reports", true, false, "#", "fa fa-building-o fa-fw", "nav nav-second-level", null);
                menuList.Add(menuItem);
                childItem = MakeMenu("Exceptions Report", false, true, "/Reports/ExceptionList", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
                menuList.Add(childItem);
                childItem = MakeMenu("Users Report", false, true, "/Reports/UsersList", "fa fa-edit fa-fw", "nav nav-second-level", menuItem);
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
                    menuHtml.AppendLine(string.Format("<ul class=\"{0}\" id='ul{1}'>", menuItem.MenuUlClass, menuItem.Id));
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
                    menuHtml.AppendLine(string.Format("<ul class=\"{0}\" id='ul{1}'>", menuItem.MenuUlClass, menuItem.Id));
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

            return "<li id='li" + menuItem.Id + "'>";
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
            string format = "<a href=\"{0}\" title=\"{1}\" id=\"href{5}\"><i class=\"{3}\"></i>{2}{4}</a>";
            //return string.Format(format, menuUrl, menuItem.DisplayName, menuItem.DisplayName,  menuItem.Parent != null ? string.Empty : string.Format("<i class=\"{0}\"></i>", menuItem.MenuLiClass), menuItem.HasChildren ? "<span class=\"fa arrow\"></span>" : string.Empty);
            return string.Format(format, menuUrl, menuItem.DisplayName, menuItem.DisplayName, menuItem.MenuLiClass, menuItem.HasChildren ? "<span class=\"fa arrow\"></span>" : string.Empty, menuItem.Id);

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
                    menuHtml.AppendLine(string.Format("<ul class=\"{0}\" id='ul{1}'>", menuItem.MenuUlClass, menuItem.Id));
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
                    menuHtml.AppendLine(string.Format("<ul class=\"{0}\" id='ul{1}'>", menuItem.MenuUlClass, menuItem.Id));
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

        private static void GenerateSiteSettingsMenu(IList<Menu> menuList)
        {
            //   var menuItem = new Menu();
            var childItem = new Menu();

            Menu menuItem = MakeMenu("Portal Settings", true, false, "#", "fa fa-cogs", "nav nav-second-level", null);
            menuList.Add(menuItem);
            menuList.Add(MakeMenu("View Settings", false, true, "/PortalSettings/ViewSettingsList", "fa fa-list-ul", "nav nav-second-level", menuItem));
            menuList.Add(MakeMenu("Add Settings", false, true, "/PortalSettings/AddSettings", "fa fa-cog", "nav nav-second-level", menuItem));
        }
    }
}