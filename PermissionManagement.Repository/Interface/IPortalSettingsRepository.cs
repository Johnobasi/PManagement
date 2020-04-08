using PermissionManagement.Model;
using System.Collections.Generic;

namespace PermissionManagement.Repository
{
    public interface IPortalSettingsRepository
    {
        IEnumerable<PortalSetting> GetPortalSettings();
        string AddSetting(PortalSetting portalSetting);
        string UpdateSetting(ref PortalSetting portalSetting);
        PortalSetting GetSettingByKey(string key);
    }
}
