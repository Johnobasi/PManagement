using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Utility;

namespace PermissionManagement.Services
{
    public class PasswordHistoryService : IPasswordHistoryService
    {
        #region Properties and Variables
        IPasswordHistoryRepository passwordHistoryRepository;
        ICacheService cacheService;
        IPortalSettingsRepository portalSettingsRepository;
        //  PasswordHistoryValidator validator = new PasswordHistoryValidator();
        //  ValidationState validationState = new ValidationState();
        #endregion

        #region Constructors
        public PasswordHistoryService(IPasswordHistoryRepository passwordHistoryRepo, IPortalSettingsRepository portalSettingsRepo, ICacheService cacheServiceInstance)
        {
            passwordHistoryRepository = passwordHistoryRepo;
            cacheService = cacheServiceInstance;
            portalSettingsRepository = portalSettingsRepo;
        }
        #endregion


        public bool InsertPassword(PasswordHistoryModel passwordHistoryModel)
        {
            return passwordHistoryRepository.InsertPassword(passwordHistoryModel);
        }

        public bool IsRepeatingPassword(PasswordHistoryModel passwordHistoryModel, out int unUsablePreviousPasswordCount)
        {
            int constValue = 0;
            int.TryParse(Constants.PortalSettingsKeyFallBackValues.UNUSABLEPREVIOUSPASSWORDSNUMBER,out constValue);
            unUsablePreviousPasswordCount = constValue;
            PortalSetting portalSetting = portalSettingsRepository.GetSettingByKey(Constants.PortalSettingsKeysConstants.UNUSABLEPREVIOUSPASSWORDSNUMBER);
            if (!string.IsNullOrWhiteSpace(portalSetting.Value))
            {
                int.TryParse(portalSetting.Value, out unUsablePreviousPasswordCount);
                if (unUsablePreviousPasswordCount == 0)
                {
                    unUsablePreviousPasswordCount = constValue;
                }
            }
            return passwordHistoryRepository.IsRepeatingPassword(passwordHistoryModel, unUsablePreviousPasswordCount);
        }
    }
}
