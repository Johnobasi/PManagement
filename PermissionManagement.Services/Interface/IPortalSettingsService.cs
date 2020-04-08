using PermissionManagement.Model;
using PermissionManagement.Validation;
using System.Collections.Generic;

namespace PermissionManagement.Services
{
    public interface IPortalSettingsService
    {
        string AddPortalSetting(PortalSetting portalSetting, ref ValidationStateDictionary states);
        IEnumerable<PortalSetting> GetPortalSettings();
        string UpdateSetting(PortalSetting portalSetting);
        PortalSetting GetSettingByKey(string key);
    }
}
