using PermissionManagement.Model;
using PermissionManagement.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
namespace PermissionManagement.Repository
{
    public class PortalSettingsRepository : IPortalSettingsRepository
    {
        #region Properties and Variables
        private readonly IDbConnection context;
        private readonly DapperContext dapperContext;
        ILogRepository log;
        #endregion

        #region Constructors
        public PortalSettingsRepository(DapperContext dbContext)
        {
            dapperContext = dbContext;
            context = dbContext.Connection;
            log = new LogRepository(dbContext);
        }
        #endregion

        #region IPortalSettingsRepository Implementation
        public IEnumerable<PortalSetting> GetPortalSettings()
        {
             return context.Query<PortalSetting>("SELECT [Key], [Value] ,[PSID] FROM PortalSettings") as IEnumerable<PortalSetting>; 
        }

        public string AddSetting(PortalSetting portalSetting)
        {
            string addSettingsStatusMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(portalSetting.Key) && !string.IsNullOrWhiteSpace(portalSetting.Value))
            {
                if (!CheckIfKeyExists(portalSetting.Key))
                {
                    if (context.Execute("INSERT INTO PortalSettings ([Key],[Value]) VALUES(@Key,@Value)", new { Key = portalSetting.Key, Value = portalSetting.Value }) == 1)
                    {
                        addSettingsStatusMessage = Constants.PortalSettingsMessageConstants.SETTINGADDSUCCESSFUL;
                    }
                }
                else { addSettingsStatusMessage = Constants.PortalSettingsMessageConstants.SETTINGKEYEXISTS; }
            }
            else { addSettingsStatusMessage = Constants.PortalSettingsMessageConstants.SETTINGSKEYVALUEEMPTY; }
            
            return addSettingsStatusMessage;
        }

        public string UpdateSetting(ref PortalSetting portalSetting)
        {
            string addSettingsStatusMessage = Constants.PortalSettingsMessageConstants.UPDATESETTINGFAIL;
            try
            {
                if (!string.IsNullOrWhiteSpace(portalSetting.Value) && !string.IsNullOrWhiteSpace(portalSetting.Key))
                {
                    int executeStatus = -1;
                    executeStatus = context.Execute("UPDATE PortalSettings SET [Value] = @Value WHERE [Key] = @Key AND [PSID] = @Psid",
                                                                            new { Key = portalSetting.Key, Value = portalSetting.Value, PSID = portalSetting.PSID });
                    if (executeStatus == 1) { addSettingsStatusMessage = Constants.PortalSettingsMessageConstants.UPDATESETTINGSUCCESS; }
                }
            }
            catch (Exception ex) { addSettingsStatusMessage = Constants.PortalSettingsMessageConstants.UPDATESETTINGSERROR; throw ex; }
            return addSettingsStatusMessage;
        }

        public PortalSetting GetSettingByKey(string key)
        {

            PortalSetting portalSetting = new PortalSetting();
            string sql = "SELECT [Key], [Value], [PSID] FROM PortalSettings WHERE [KEY] = @Key";
            portalSetting = context.Query<PortalSetting>(sql, new { KEY = key }).FirstOrDefault();
            return portalSetting;           
        }

        #endregion

        #region Private Methods
        private bool CheckIfKeyExists(string key)
        {
                
            return context.ExecuteScalar("IF((SELECT COUNT (*) FROM PortalSettings WHERE [KEY] = @Key) > 0) BEGIN SELECT 'true' END ELSE BEGIN SELECT 'false' END",
                                               new { KEY = key })
                                               .ToString().ToLower() == "true" ? true : false;
        } 
        #endregion
    }
}
