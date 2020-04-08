using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Services.ModelValidator;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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
        public IList<PortalSetting> GetPortalSettings() { return portalSettingsRepository.GetPortalSettings(); }

        public string AddPortalSetting(PortalSetting portalSetting, ref ValidationStateDictionary states)
        {
            validationState = validator.Validate(portalSetting, states);
            if (validationState.IsValid) { return portalSettingsRepository.AddSetting(portalSetting); }
            return validationState.Errors[0].Message;
        }

        public PortalSetting GetSettingByKey(string Key)
        {
            //var key = string.Format("{0}-{1}", Constants.PortalSettingsKeysConstants.PSID, Key);
            var cacheKey = string.Format("PORTALSETTINGS");
            var setting = cacheService.Get(cacheKey) as IList<PortalSetting>;
            if (setting == null)
            {
                setting = portalSettingsRepository.GetPortalSettings();
                cacheService.Add(cacheKey, setting);
                RepositoryServicesPortalSetting.PortalSetting = setting.ToList();
            }

            var item = setting.Where(f => f.Key == Key).Select(i => i).FirstOrDefault();

            #region To Set the FallBack Value if available in "Constants.PortalSettingsKeyFallBackValues"
            if (item == null || string.IsNullOrEmpty(item.Value))
            {
                FieldInfo property = typeof(Constants.PortalSettingsKeyFallBackValues).GetField(Key.ToUpper());
                item.Value = property.GetValue(property).ToString();
            }
            #endregion

            return item;
        }

        public string UpdateSetting(PortalSetting portalSetting)
        {
            string updateStatusMsg = portalSettingsRepository.UpdateSetting(ref portalSetting);

            RepositoryServicesPortalSetting.PortalSetting[
                    RepositoryServicesPortalSetting.PortalSetting.FindIndex(
                    item => item.Key == portalSetting.Key)].Value = portalSetting.Value;

            return updateStatusMsg;
        }
        #endregion
    }
}
