using PermissionManagement.Model;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using PermissionManagement.Web;
using System.Web.Mvc;

namespace PermissionManagement.Controllers
{
    public class PortalSettingsController : BaseController
    {
        #region Properties and Variables
        private IPortalSettingsService portalSettingsServiceInstance;
        PortalSettings portalSettings = new PortalSettings();
        PortalSetting portalSetting = new PortalSetting();
        ValidationStateDictionary validationStateDict = new ValidationStateDictionary();

        public PortalSettingsController(IPortalSettingsService portalSettingsService)
        { 
            portalSettingsServiceInstance = portalSettingsService; 
        }

        #endregion

        #region Portal Settings Action Results
        #region View Settings List
        [AuditFilter(AuditLogLevel.LevelFive)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult ViewSettingsList()
        {
            portalSettings.settings = portalSettingsServiceInstance.GetPortalSettings();
            return View(portalSettings.settings);
        }

        [CompressFilter]
        [HttpPost]
        [ActionName("PortalSettingsListExcelDownload")]
        [AuditFilter(AuditLogLevel.LevelFive)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public void PortalSettingsListExcelDownload()
        {
            portalSettings.settings = portalSettingsServiceInstance.GetPortalSettings();
            new Export().ToFile(portalSettings.settings, Response,"PortalSettings_Report");
        }

        [CompressFilter]
        [HttpPost]
        [ActionName("PortalSettingsListWordDownload")]
        [AuditFilter(AuditLogLevel.LevelFive)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public void PortalSettingsListWordDownload()
        {
            portalSettings.settings = portalSettingsServiceInstance.GetPortalSettings();
            new Export().ToFile(portalSettings.settings, Response, "PortalSettings_Report",EXPORTTYPE.WORD);
        }

        #endregion

        #region Add Settings
        [AuditFilter(AuditLogLevel.LevelFive)]
        [HttpGet]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult AddSettings() { return View(); }

        [AuditFilter()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Edit)]
        public ActionResult AddSettings(PortalSetting portalSetting)
        {
            ViewBag.status = portalSettingsServiceInstance.AddPortalSetting(portalSetting, ref validationStateDict);
            return View();
        }
        #endregion

        #region Edit Settings
        [AuditFilter(AuditLogLevel.LevelFive)]
        [HttpGet]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult EditSettings(int PSID, string key)
        {
            portalSetting = portalSettingsServiceInstance.GetSettingByKey(key);
            return View(portalSetting);
        }

        [AuditFilter(AuditLogLevel.LevelZero)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Edit)]
        public ActionResult EditSettings(PortalSetting portalSetting)
        {
            ViewBag.status = portalSettingsServiceInstance.UpdateSetting(portalSetting);
            return View();
        }
        #endregion

        #endregion
    }
}