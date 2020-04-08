using System.Web.Mvc;
using PermissionManagement.Validation;
using System;
using System.Collections.Generic;
using System.Globalization;
using PermissionManagement.Model;
using System.Web;
using System.Text;
using System.Linq;
using System.Collections;
using System.Web.Routing;
using System.Configuration;
using System.Reflection;
using System.Data;
using System.Web.Security;
using System.Xml.Linq;
using System.Web.Mvc.Html;
using System.Text.RegularExpressions;
using System.Threading;
using PermissionManagement.Utility;
//using PermissionManagement.IoC;
using DryIoc;
using PermissionManagement.Services;
using DataTables.Mvc;

namespace PermissionManagement.Web
{
    public static class MVCExtensionMethods
    {
        static Regex pagerRegex = new Regex("(\\?page=(.*?)&pagesize=(.*?)&sortby=(.*?)&sortdirection=(.*?)&)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static String GetTopMenu(this HtmlHelper htmlHelper)
        {
            return "Yes";
        }

        public static HtmlString GetRootUrl(this HtmlHelper htmlHelper)
        {
            return new HtmlString(Helper.GetRootURL());
        }

        public static int GetTimeOutInMilliseconds(this HtmlHelper htmlHelper)
        {
            return SecurityConfig.GetCurrent().Cookie.Timeout * 60 * 1000;
        }

        public static HtmlString GetSideMenu(this HtmlHelper htmlHelper)
        {
            var context = HttpContext.Current;
            var id = ((Identity)context.User.Identity);
            var cacheService = ((IContainer)context.Application["container"]).Resolve<ICacheService>();

            var menuList = cacheService.Get(string.Format("{0}-Menu-{1}", id.Roles, Constants.General.RoleNames)) as IList<Menu>;

            if (menuList == null)
            {
                menuList = MenuBuilder.GetMenu(id.Roles, context.Request.Url.ToString());
                cacheService.Add(string.Format("{0}-Menu-{1}", id.Roles, Constants.General.RoleNames), menuList);
            }
            var menuHtml = MenuBuilder.BuildII(menuList, context.Request.Url.ToString());
            return new HtmlString(menuHtml);
        }

        public static string GetPostUrl(this HtmlHelper htmlHelper, string subPathOffRoot, string resourceRelativeUri)
        {
            return string.Format("{0}/{1}{2}", Helper.GetRootURL(), subPathOffRoot != "/" && subPathOffRoot != string.Empty ? subPathOffRoot + "/" : string.Empty, resourceRelativeUri);
        }

        public static void AddModelErrors(this ModelStateDictionary modelStateDictionary, ValidationStateDictionary validationStateDictionary)
        {
            foreach (KeyValuePair<Type, ValidationState> validationState in validationStateDictionary)
            {
                foreach (ValidationError validationError in validationState.Value.Errors)
                {
                    var index = validationError.Name.LastIndexOf(".");
                    string modelProperty = validationError.Name.Substring(0, index > 0 ? index : validationError.Name.Length);

                    modelStateDictionary.AddModelError(modelProperty, validationError.Message);

                    modelStateDictionary.SetModelValue(
                        modelProperty,
                        new ValueProviderResult(
                            validationError.AttemptedValue,
                            validationError.AttemptedValue as string,
                            CultureInfo.CurrentCulture
                            )
                        );
                }
            }
        }

        public static IList<string> GetErrors(ValidationStateDictionary validationStateDictionary)
        {
            var list = new List<string>();
            foreach (KeyValuePair<Type, ValidationState> validationState in validationStateDictionary)
            {
                foreach (ValidationError validationError in validationState.Value.Errors)
                {
                    list.Add(validationError.Message);
                }
            }

            return list;
        }

        public static PagerItems GetPagingParameters(HttpRequestBase request, string defaultSortBy)
        {
            return GetPagingParameters(request, defaultSortBy, "ASC");
        }

        public static PagerItems GetPagingParameters(HttpRequestBase request, string defaultSortBy, string defaultsortDirection)
        {
            var p = new PagerItems();
            p.PageNumber = Helper.GetPageNumber(request.QueryString["Page"]);
            p.PageSize = Helper.GetPageSize(request.QueryString["PageSize"]);
            p.SortBy = Helper.GetSortBy(request.QueryString["SortBy"], defaultSortBy);
            p.SortDirection = Helper.GetSortDirection(request.QueryString["SortDirection"], defaultsortDirection);
            p.siteSearch = Helper.GetSearchText(request.QueryString["searchText"]);
            return p;
        }

        public static PagerItemsII GetPagingParametersII(DataTables.Mvc.IDataTablesRequest requestModel, string defaultSortBy, string defaultsortDirection)
        {
            var pagingParameter = new PagerItemsII();

            pagingParameter.PageSize = requestModel.Length;
            pagingParameter.CurrentPage = requestModel.Start == 0 ? 1 : (requestModel.Start + 1) / requestModel.Length;
            pagingParameter.PageNumber = requestModel.Start == 0 ? 1 : ((requestModel.Start + 1) / requestModel.Length) + 1;
            pagingParameter.SortColumns = requestModel.Columns.GetSortedColumns();
            if (pagingParameter.SortColumns == null || pagingParameter.SortColumns.Count() == 0)
            {
                IList<Column> v = new List<Column>();
                var c = new Column(defaultSortBy, defaultSortBy, true, true, String.Empty, false);
                c.SetColumnOrdering(1, defaultsortDirection.ToLower());
                v.Add(c);
                pagingParameter.SortColumns = (IOrderedEnumerable<Column>)v;
            }
            pagingParameter.SearchColumns = requestModel.Columns.GetFilteredColumns();

            pagingParameter.siteSearch = requestModel.Search.Value;

            return pagingParameter;
        }

        public static string GetUserStatus(this HtmlHelper htmlHelper, string ApprovalStatus, bool isLocked)
        {
            return ApprovalStatus == Constants.ApprovalStatus.Approved && !isLocked ? "Active" : ApprovalStatus == Constants.ApprovalStatus.Pending && isLocked ? "Locked" : "Inactive";
        }
        public static IEnumerable<Role> RoleDropDownList(this HtmlHelper htmlHelper)
        {
            var current = HttpContext.Current;
            var securityService = ((IContainer)current.Application["container"]).Resolve<ISecurityService>();
            var roleList = securityService.GetRoleList().ToList();
            roleList.Insert(0, new Role() { RoleId = Guid.Empty, RoleName = Constants.General.PleaseSelectMessage });
            return roleList;
        }

       #region Initials drop down list
        public static IEnumerable<StaffInitialDTO> StaffInitialDropDownList(this HtmlHelper htmlHelper)
        {
            var context = HttpContext.Current;

            var cacheService = ((IContainer)context.Application["container"]).Resolve<ICacheService>();

            var staffInitialList = cacheService.Get(Constants.General.StaffInitialList) as IList<StaffInitialDTO>;

            if (staffInitialList == null)
            {
                //var underwritingSetupService = ((IIocContainer)context.Application["container"]).Resolve<IUnderwritingSetupService>();
                var list = new List<StaffInitialDTO>();
                list.Add(new StaffInitialDTO() { StaffInitialID = string.Empty, Description = "Please select...." });
                list.Add(new StaffInitialDTO() { StaffInitialID = "Mr", Description = "Mr" });
                list.Add(new StaffInitialDTO() { StaffInitialID = "Mrs", Description = "Mrs" });
                list.Add(new StaffInitialDTO() { StaffInitialID = "Ms", Description = "Ms" });
                list.Add(new StaffInitialDTO() { StaffInitialID = "Miss", Description = "Miss" });
                list.Add(new StaffInitialDTO() { StaffInitialID = "Dr", Description = "Dr" });
                list.Add(new StaffInitialDTO() { StaffInitialID = "Eng", Description = "Eng" });

                staffInitialList = list;
                cacheService.Add(Constants.General.StaffInitialList, list);
            }
            return staffInitialList;
        }

        #endregion

        #region AccountType drop down list
        public static IEnumerable<AccountTypeDTO> AccountTypeDropDownList(this HtmlHelper htmlHelper, bool isCreate)
        {
            var context = HttpContext.Current;

            var cacheService = ((IContainer)context.Application["container"]).Resolve<ICacheService>();

            var accountTypeList = cacheService.Get(Constants.General.AccountTypeList) as IList<AccountTypeDTO>;

            if (accountTypeList == null)
            {
                var list = new List<AccountTypeDTO>();
                //list.Add(new AccountTypeDTO() { AccountTypeID = string.Empty, Description = "Please select...." });
                list.Add(new AccountTypeDTO() { AccountTypeID = Constants.AccountType.ADFinacle, Description = Constants.AccountType.ADFinacle });
                list.Add(new AccountTypeDTO() { AccountTypeID = Constants.AccountType.ADLocal, Description = Constants.AccountType.ADLocal });
                list.Add(new AccountTypeDTO() { AccountTypeID = Constants.AccountType.LocalLocal, Description = Constants.AccountType.LocalLocal });
                list.Add(new AccountTypeDTO() { AccountTypeID = Constants.AccountType.LocalFinacle, Description = Constants.AccountType.LocalFinacle });

                accountTypeList = list;
                cacheService.Add(Constants.General.AccountTypeList, list);
            }
            //remove AD/Finacle if we are creating new user - as it is not selectable.
            if (isCreate)
            {
                var finalList = accountTypeList.ToList();
                finalList.RemoveAt(0);
                return finalList;
            }
            else
            {
                return accountTypeList;
            }
        }

        #endregion

        #region ApprovalStatus drop down list
        public static IEnumerable<ApprovalStatusDTO> ApprovalStatusDropdownList(this HtmlHelper htmlHelper, string currentStatus)
        {
            var list = new List<ApprovalStatusDTO>();
            if (currentStatus == Constants.ApprovalStatus.Approved)
            {
                list.Add(new ApprovalStatusDTO() { ApprovalStatusID = Constants.ApprovalStatus.Approved, Description = Constants.ApprovalStatus.Approved });
            }
            else if (currentStatus == Constants.ApprovalStatus.Pending)
            {
                list.Add(new ApprovalStatusDTO() { ApprovalStatusID = Constants.ApprovalStatus.Approved, Description = Constants.ApprovalStatus.Approved });
                list.Add(new ApprovalStatusDTO() { ApprovalStatusID = Constants.ApprovalStatus.RejectedForCorrection, Description = Constants.ApprovalStatus.RejectedForCorrection });
                list.Add(new ApprovalStatusDTO() { ApprovalStatusID = Constants.ApprovalStatus.Rejected, Description = Constants.ApprovalStatus.Rejected });        
            }
            else if (currentStatus == Constants.ApprovalStatus.RejectedForCorrection)
            {
                list.Add(new ApprovalStatusDTO() { ApprovalStatusID = Constants.ApprovalStatus.RejectedForCorrection, Description = Constants.ApprovalStatus.RejectedForCorrection });
            }
            else if (currentStatus == Constants.ApprovalStatus.Rejected)
            {
                list.Add(new ApprovalStatusDTO() { ApprovalStatusID = Constants.ApprovalStatus.Rejected, Description = Constants.ApprovalStatus.Rejected });
            }   
            return list;
        }

        #endregion

        //#region User drop down list
        //public static IEnumerable<UserDTO> UserDropDownList(this HtmlHelper htmlHelper)
        //{
        //    var context = HttpContext.Current;

        //    var cacheService = ((IIocContainer)context.Application["container"]).Resolve<ICacheService>();

        //    var userList = cacheService.Get(Constants.General.UserList) as IList<UserDTO>;

        //    if (userList == null)
        //    {
        //        var securityService = ((IIocContainer)context.Application["container"]).Resolve<ISecurityService>();
        //        userList = securityService.GetUserList(1, 5000, Constants.SortField.StaffID,
        //            Constants.SortOrder.Ascending, string.Empty, null).UserListingResult.Select(f =>
        //                new UserDTO
        //                {
        //                    StaffID = f.StaffID,
        //                    FullDescription = string.Format("{0} | {1} | {2} | {3}",
        //                    f.StaffID, f.Username, f.FirstName, f.LastName)
        //                }).ToList();

        //        cacheService.Add(Constants.General.UserList, userList);
        //    }
        //    return userList;
        //}

       // #endregion

        public static string GetLink(this HtmlHelper htmlHelper, string displayText, string subPathOffRoot, string resourceRelativeUri)
        {
            return string.Format("<a href=\"{0}\" title=\"{1}\" >{1}</a>", GetPostUrl(htmlHelper, subPathOffRoot, resourceRelativeUri), displayText);
        }
    }
}