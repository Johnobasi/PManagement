using System.Diagnostics.CodeAnalysis;

namespace PermissionManagement.Utility
{
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "It is safe to suppress a warning from this rule if instances of the value type will not be compared to each other.")]
    public sealed partial class Constants
    {
        public class ExceptionSortField
        {
            public const string ExceptionId = "ExceptionId";
            public const string ExceptionDateTime = "ExceptionDateTime";
            public const string ExceptionDetails = "ExceptionDetails";
            public const string ExceptionPage = "ExceptionPage";
            public const string LoggedInUser = "LoggedInUser";
            public const string UserIpAddress = "UserIpAddress";
            public const string ExceptionType = "ExceptionType";
            public const string ExceptionMessage = "ExceptionMessage";
            public const string ExceptionVersion = "ExceptionVersion";
            public const string ExceptionDateTimeTo = "ExceptionDateTimeTo";
        }
        public class ExpiredUserSortField
        {
            public const string Username = "UserName";
            public const string Firstname = "FirstName";
            public const string Lastname = "LastName";
            public const string Email = "Email";
            public const string AccountType = "AccountType";
            public const string StaffPosition = "StaffPosition";
            public const string AccountExpiryDate = "AccountExpiryDate";
            public const string CreationDate = "CreationDate";
            public const string LastLogInDate = "LastLogInDate";
            public const string BrachId = "BrachId";
            public const string NewUser = "NewUser";
            public const string DisabledUsers = "DisabledUser";
            public const string DormantUsers = "DormantUsers";
        }
        public class UserListSortField
        {
            public const string Username = "UserName";
            public const string Firstname = "FirstName";
            public const string Lastname = "LastName";
            public const string Email = "Email";
        }
        public class DebugLog
        {
            public const string CustomErrorLog = "ErrorLog";
        }

    }
}
