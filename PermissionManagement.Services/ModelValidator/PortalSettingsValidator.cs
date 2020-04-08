using PermissionManagement.Model;
using PermissionManagement.Validation;

namespace PermissionManagement.Services.ModelValidator
{
    class PortalSettingsValidator : IValidator<PortalSetting>
    {
        ValidationState v = new ValidationState();
        public ValidationState Validate(PortalSetting portalSettingToValidate, object otherData)
        {
            portalSettingToValidate.Key = RemoveSpecialCharacters(portalSettingToValidate.Key, new char[] { '!', '@', ',', ':', ';' });
            validatestring(portalSettingToValidate.Key, "Key");
            validatestring(portalSettingToValidate.Value, "Value");
            if (v.IsValid)
            {
                portalSettingToValidate.Key = portalSettingToValidate.Key.Trim();
                portalSettingToValidate.Value = portalSettingToValidate.Value.Trim();
            }
            return v;
        }

        private void validatestring(string stringToValidate, string property)
        {
            if (string.IsNullOrWhiteSpace(stringToValidate))
            {
                v.Errors.Add(new ValidationError("PortalSetting." + property + ".RequiredError", stringToValidate, "Portal settings " + property + " cannot be empty"));
            }
            if (stringToValidate != null)
            {
                if (string.IsNullOrEmpty(stringToValidate.Trim()))
                {
                    v.Errors.Add(new ValidationError("PortalSetting." + property + ".RequiredError", stringToValidate, "Portal settings " + property + " cannot be empty"));
                }
            }
        }

        private string RemoveSpecialCharacters(string inputString, char[] specialCharacteres)
        {
            if (!string.IsNullOrEmpty(inputString))
            {
                for (int i = 0; i < specialCharacteres.Length; i++)
                {
                    inputString = inputString.Replace(specialCharacteres[i].ToString(), string.Empty);
                }
            }
            return inputString;
        }
    }
}
