
namespace PermissionManagement.Utility
{
    public sealed partial class Constants
    {
        public class PortalSettingsMessageConstants
        {
            public const string SETTINGADDSUCCESSFUL = "Portal setting added successfully.";
            public const string SETTINGKEYEXISTS = "The provided KEY is already in Use.";
            public const string SETTINGSKEYVALUEEMPTY = "Settings Key and Value cannot be empty.";

            public const string UPDATESETTINGFAIL = "Setting update Failed.";
            public const string UPDATESETTINGSUCCESS = "Setting updated Successfuylly.";
            public const string UPDATESETTINGSERROR = "There was an error while updating the Setting.";
            public const string UPDATESETTINGSVALUEEMPTY = "Settings Value cannot be empty.";
            
        }

        public class PortalSettingsKeysConstants
        {
            public const string NEWUSERIDDORMANTNUMBERDAYS = "NewUserIDDormantNumberDays";
            public const string ACTIVEUSERIDDORMANTNUMBERDAYS = "ActiveUserIDDormantNumberDays";
            public const string BUSINESSHOURSTART = "BusinessHourStart";
            public const string BUSINESSHOURCLOSE = "BusinessHourClose";
            public const string PSID = "PSID";
            public const string UNUSABLEPREVIOUSPASSWORDSNUMBER = "UnusablePreviousPasswordsNumber";
            public const string ACCOUNTEXPIRYNUMBERDAYS = "AccountExpiryNumberDays";
            public const string AUDITLOGLEVEL = "AuditLogLevel";
        }

        public class PortalSettingsKeyFallBackValues
        {
            public const string NEWUSERIDDORMANTNUMBERDAYS = "3";
            public const string ACTIVEUSERIDDORMANTNUMBERDAYS = "90";
            public const string BUSINESSHOURSTART = "08:00:00";
            public const string BUSINESSHOURCLOSE = "17:00:00";
            public const string UNUSABLEPREVIOUSPASSWORDSNUMBER = "4";
            public const string ACCOUNTEXPIRYNUMBERDAYS = "90";
        }
    }
}
