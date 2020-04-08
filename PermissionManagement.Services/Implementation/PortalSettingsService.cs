using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Services.ModelValidator;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System.Collections.Generic;
using System.Reflection;

namespace PermissionManagement.Services
{
    public class PortalSettingsService : IPortalSettingsService
    {
        #region Properties and Variables
        IPortalSettingsRepository portalSettingsRepository;
        ICacheService cacheService;
        PortalSettingsValidator validator = new PortalSettingsValidator();
        ValidationState validationState = new ValidationState();
        #endregion

        #region Constructors
        public PortalSettingsService(IPortalSettingsRepository portalSettingsRepo, ICacheService cacheServiceInstance)
        {
            portalSettingsRepository = portalSettingsRepo;
            cacheService = cacheServiceInstance;
        }
        #endregion

        #region IPortasSetting Implementation
        public IEnumerable<PortalSetting> GetPortalSettings() { return portalSettingsRepository.GetPortalSettings(); }

        public string AddPortalSetting(PortalSetting portalSetting, ref ValidationStateDictionary states)
        {
            validationState = validator.Validate(portalSetting, states);
            if (validationState.IsValid) { return portalSettingsRepository.AddSetting(portalSetting); }
            return validationState.Errors[0].Message;
        }

        public PortalSetting GetSettingByKey(string Key)
        {
            var key = string.Format("{0}-{1}", Constants.PortalSettingsKeysConstants.PSID, Key);
            var setting = cacheService.Get(key) as PortalSetting;
            if (setting == null)
            {
                setting = portalSettingsRepository.GetSettingByKey(Key);
                #region To Set the FallBack Value if available in "Constants.PortalSettingsKeyFallBackValues"
                if (setting != null && string.IsNullOrWhiteSpace(setting.Value))
                {
                    FieldInfo property = typeof(Constants.PortalSettingsKeyFallBackValues).GetField(Key.ToUpper());
                    setting.Value = property.GetValue(property).ToString();
                }
                #endregion
                cacheService.Add(key, setting);
            }
            return setting;
        }

        public string UpdateSetting(PortalSetting portalSetting)
        {
            var key = string.Format("{0}-{1}", Constants.PortalSettingsKeysConstants.PSID, portalSetting.Key);
            cacheService.Remove(key);
            string updateStatusMsg = portalSettingsRepository.UpdateSetting(ref portalSetting);
            cacheService.Add(key, portalSetting);
            return updateStatusMsg;
        }
        #endregion
    }
}
