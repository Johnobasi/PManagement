using System.Diagnostics.CodeAnalysis;

namespace PermissionManagement.Utility
{
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "It is safe to suppress a warning from this rule if instances of the value type will not be compared to each other.")]
    public sealed partial class Constants
    {       
        public class ModelType
        {
            public const string User = "User";
            public const string Role = "Role";
        }

        public class OperationType
        {
            public const string Create = "Create";
            public const string Edit = "Edit";
            public const string Delete = "Delete";
        }

        public class ApprovalNoticeType
        {
            public const string ApprovalRequest = "ApprovalRequest";
            public const string ApprovalExecuted = "ApprovalExecuted";
            public const string ApprovalRejected = "ApprovalRejected";
            public const string ApprovalRejectedForCorrection = "ApprovalRejectedForCorrection";
        }

        public class SequenceNames
        {
            public const string Username = "Username";

        }

        public class Messages
        {
            public const string ConcurrencyError =
                    "This record's detail has been changed in the database since last retrieval. The up to date snapshot is displayed. Please re-apply your changes and save again."
                ;

            public const string SaveSuccessful = "Changes to the record was successfully saved to the database.";
            public const string DeleteSuccessful = "The selected record was successfully deleted from the database.";
            public const string AddSuccessful = "New record was successfully added to the database.";

            public const string ErrorNotice =
                    "There were errors encountered in processing the request, please review the error(s) below and correct them."
                ;

            public const string EditNotPermittedError =
                "You are not permitted to edit this record. Please contact the administrator";
        }

        public class SortOrder
        {
            public const string Ascending = "ASC";
            public const string Descending = "DESC";
        }

        public class SortField
        {
            public const string DateCreated = "DateCreated";
            public const string PageTitle = "PageTitle";
            public const string Username = "Username";
            public const string Name = "Name";
            public const string StartDate = "StartDate";
            public const string EndDate = "EndDate";
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string Email = "Email";
            public const string Role = "Role";
            public const string FullName = "FullName";
            public const string ContactName = "ContactName";
            public const string ContactEmail = "ContactEmail";
            public const string AccessTypeName = "AccessTypeName";
            public const string Address = "Address";
            public const string Title = "Title";
            public const string AuditID = "AuditID";
            public const string AuditAction = "AuditAction";
            public const string AuditType = "AuditType";
            public const string ActivityName = "ActivityName";
            public const string ApprovalLogID = "ApprovalLogID";
            public const string ActionEndTime = "ActionEndTime";
            public const string ActionStartTime = "ActionStartTime";
            public const string ApprovalStatus = "ApprovalStatus";
            public const string AuditPage = "AuditPage";
            public const string AuditMessage = "AuditMessage";
            public const string AuditChangeID = "AuditChangeID";

            public static string ActionDateTime = "ActionDateTime";

            //public static string ValueBefore = "ValueBefore";
            //public static string ValueAfter = "ValueAfter";
            public static string Changes = "Changes";

            public static string TableName = "TableName";
            public static string AffectedRecordID = "AffectedRecordID";
            public static string AuditHTTPAction = "AuditHTTPAction";
        }

        public class AccessRights
        {
            public const string View = "View";
            public const string Delete = "Delete";
            public const string Create = "Create";
            public const string Edit = "Edit";
            public const string Verify = "Verify";
            public const string MakeOrCheck = "MakeOrCheck";
        }

        public class ApprovalStatus
        {
            public const string Pending = "Pending";
            public const string Rejected = "Rejected";
            public const string RejectedForCorrection = "RejectedForCorrection";
            public const string Approved = "Approved";
            public const string Abandoned = "Abandoned";
        }

        public class AccountType
        {
            public const string ADFinacle = "AD/Finacle";
            public const string LocalFinacle = "Local/Finacle";
            public const string LocalLocal = "Local/Local";
            public const string ADLocal = "AD/Local";
        }

        public class Modules
        {
            public const string UserSetup = "UserSetup";
            public const string Reports = "Reports";
            public const string Admin = "Admin";
            public const string AuditTrail = "Audit";
        }

        public class DateFormats
        {
            public const string ShortDate = "dd/MM/yyyy";
            public const string LongDate = "dd MMMM yyyy";
            public const string ShortDateTime = "dd/MM/yyyy HH:mm";
            /// <summary>
            /// Returns the string format "yyyy-MM-dd HH:mm:ss"
            /// </summary>
            public const string LongDateTimeHyphen = "yyyy-MM-dd HH:mm:ss";
        }

        public class PageOrientation
        {
            public const string Portrait = "Portrait";
            public const string Landscape = "Landscape";
        }

        public class SortClause
        {
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string Email = "Email";
            public const string EmailAddress = "EmailAddress";
            public const string Description = "Description";
            public const string AddedDateTime = "AddedDateTime";
            public const string DateTaken = "StartDateTime";
            public const string FullName = "FullName";
            public const string ContactName = "ContactName";
            public const string TransactionDate = "TransactionDate";
        }

        public class General
        {
            public const string YadcfDelimiter = "-yadcf_delim-";
            public const string NotAvailable = "NA";
            public const string Yes = "y";
            public const string No = "n";
            public const string YesInWord = "yes";
            public const string NoInWord = "no";
            public const string SentinelNumber = "-99999";
            public const string EmailLogo = "<img src='{0}/Assets/Default/Images/email_logo.gif' />";
            public const string LogoXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "<data logo=\"{0}\"/>";
            public const string ModuleAccessList = "ModuleAccessList";
            public const string RoleModuleAccessList = "RoleModuleAccessList";
            public const string RoleList = "RoleList";
            public const string RoleNames = "RoleNames";
            public const string MenuList = "MenuList";
            public const string ErrorPage = "error";
            public const string PageNotFound = "pagenotfound";
            public const string StaffInitialList = "StaffInitialList";
            public const string UserList = "UserList";
            public const string PleaseSelectMessage = "Please select...";
            public const string SessionTimeOut = "sessiontimeout";
            public const string AccountTypeList = "AccountTypeList";
            public const string ApprovalStatusList = "ApprovalStatusList";
            public const string AuditList = "AuditList";
            public const string AdministratorRoleName = "Administrator";
        }

        public class QueryStrings
        {
            public const string LogOut = "logout";
            public const string ReturnUrl = "ReturnUrl";
            public const string EditMode = "e";
            public const string EnryptedToken = "a";
            public const string ID = "id";
            public const string Username = "uid";
            public const string CreateMode = "c";
            public const string FileToDownload = "f";
        }

        public class Alignment
        {
            public const int Left = 0;
            public const int Center = 1;
            public const int Right = 2;
        }
        }
}
