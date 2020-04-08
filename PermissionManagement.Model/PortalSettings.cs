using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PermissionManagement.Model
{
    public class PortalSetting
    {
        [Display(Name = "Key")]
        public string Key { get; set; }
        [Display(Name = "Value")]
        public string Value { get; set; }
        [IdentityPrimaryKey]
        public int PSID { get; set; }
    }

    public class PortalSettings
    {
        public IEnumerable<PortalSetting> settings;
    }
}
