using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;

namespace PermissionManagement.Services
{
    public class SecurityService : ISecurityService
    {

        private readonly ISecurityRepository repositoryInstance;
        private readonly IDatastoreValidationRepository validationRepositoryInstance;
        private IPortalSettingsService portalSettingsServiceInstance;
        private readonly ICacheService cacheService;
        private readonly ILogService logger;
        private DateTime businessStartHour, businessEndHour;
        IPasswordHistoryService passwordHistoryServiceInstance;

        private int expiredIDNoOfDays = -90;// Porvided Default Value to make sure existing implementation will not break if Configuration does not exist;

        public SecurityService(ISecurityRepository repository, IDatastoreValidationRepository validationRepository, ICacheService cacheInstance, ILogService logService, IPortalSettingsService portalSettingsService, IPasswordHistoryService passwordHistoryService) //IMessageService messageService,
        {
            repositoryInstance = repository;
            validationRepositoryInstance = validationRepository;
            cacheService = cacheInstance;
            logger = logService;
            portalSettingsServiceInstance = portalSettingsService;
            passwordHistoryServiceInstance = passwordHistoryService;
        }

        public List<ExportDto> GetUserListForExcel(string searchKey)
        {
            return repositoryInstance.GetUserListForExcel(searchKey);
        }
        public void UpdateUserRoleUserBranch(string username, Guid roleId, string branchCode)
        {
            repositoryInstance.UpdateUserRoleUserBranch(username, roleId, branchCode);
        }
        public void UpdateUserRole(string username, Guid roleId)
        {
            repositoryInstance.UpdateUserRole(username, roleId);
        }
        public void AddUserForSessionMgmt(User userDetails)
        {
            repositoryInstance.AddUserForSessionMgmt(userDetails);
        }

        public void AddRole(RoleViewModel roleToAdd, ref ValidationStateDictionary states)
        {
            var v = new RoleValidator().Validate(roleToAdd, validationRepositoryInstance);

            if (v.Errors.Count > 0)
            {
                states.Add(typeof(RoleViewModel), v);
                return;
            }

            roleToAdd.CurrentRole.RoleId = Helper.GetNextGuid();
            repositoryInstance.AddRole(roleToAdd);

            cacheService.Remove(Constants.General.RoleModuleAccessList);
            //cacheService.Remove(Constants.General.RoleProfileList);
            cacheService.Remove(Constants.General.RoleList);
            cacheService.RemoveEndWith(Constants.General.RoleNames);
        }

        public int EditRole(RoleViewModel roleToEdit, ref ValidationStateDictionary states)
        {
            var v = new RoleValidator().Validate(roleToEdit, validationRepositoryInstance);

            if (v.Errors.Count > 0)
            {
                states.Add(typeof(RoleViewModel), v);
                return 0;
            }
            var rowAffected = repositoryInstance.EditRole(roleToEdit);

            cacheService.Remove(Constants.General.RoleModuleAccessList);
            //cacheService.Remove(Constants.General.RoleProfileList);
            cacheService.Remove(Constants.General.RoleList);
            cacheService.RemoveEndWith(Constants.General.RoleNames);

            return rowAffected;
        }

        public void AddUser(User userToAdd, ref ValidationStateDictionary states)
        {
            var v = new UserValidator().Validate(userToAdd, validationRepositoryInstance);

            if (v.Errors.Count > 0)
            {
                states.Add(typeof(User), v);
                return;
            }

            var settings = SecurityConfig.GetCurrent();
            if (settings.Cookie.PasswordHashed && !string.IsNullOrEmpty(userToAdd.Password))
            {
                userToAdd.Password = PasswordHash.Hash(userToAdd.Username.ToLower(), userToAdd.Password);
            }
            else if (userToAdd.AccountType == Constants.AccountType.ADLocal)
            {
                userToAdd.Password = "Dummy";
            }
            userToAdd.ApprovalStatus = Constants.ApprovalStatus.Pending;
            userToAdd.IsFirstLogIn = true;
            userToAdd.RoleId = userToAdd.UserRole.RoleId;
            //check if the AccountExpires and set the number of days for Accountexpiry
            
            updateUserAccountExpiry(userToAdd);
            repositoryInstance.AddUser(userToAdd);
            RegistrationNotification(userToAdd, true);
        }

        public int EditUser(User userToEdit, ref ValidationStateDictionary states)
        {
            var v = new UserValidator().Validate(userToEdit, validationRepositoryInstance);
            if (v.Errors.Count > 0)
            {
                states.Add(typeof(User), v);
                return 0;
            }
            userToEdit.RoleId = userToEdit.UserRole.RoleId;
            return repositoryInstance.EditUser(userToEdit);
        }

        public User GetUser(string username)
        {
            User user = repositoryInstance.GetUser(username);
            
            if (user != null)
            {
                if (user.IsDeleted && user.ApprovalStatus == Constants.ApprovalStatus.Approved)
                {
                    return null;
                }
            }
            return user;
        }

        public AuthenticationDataDto SignIn(string username, string password, bool sessionBased, ref ValidationStateDictionary states)
        {
            ValidationState v = new ValidationState();

            if (string.IsNullOrEmpty(username))
            {
                v.Errors.Add(new ValidationError("Username.UsernameRequiredError", username, "Username is not set."));
            }
            if (string.IsNullOrEmpty(password))
            {
                v.Errors.Add(new ValidationError("Password.PasswordRequiredError", password, "Password is not set"));
            }

            if (v.Errors.Count > 0)
            {
                states.Add(typeof(LogInDto), v);
                return new AuthenticationDataDto() { AppAuthenticationFailed = true };
            }

            var userObject = repositoryInstance.GetUser(username);

            if (userObject == null)
            {
                v.Errors.Add(new ValidationError("Password.InvalidUsername", username, "Invalid Username or Password"));
                states.Add(typeof(LogInDto), v);
                return null;
            }

            string lastLogin = userObject.LastLogInDate.HasValue ?
                userObject.LastLogInDate.Value.ToString("dd-MM-yyyy HH:mm:ss") 
                : Helper.GetLocalDate().ToString("dd-MM-yyyy HH:mm:ss");
            cacheService.AddAndTieToSession(string.Format("UserLastLogIn-{0}", userObject.Username), lastLogin);

            var roleID = GetRoleList().Where(f => f.RoleName == Constants.General.AdministratorRoleName).Select(r => r.RoleId).FirstOrDefault();
            if (!IsBusinessHour())
            {
                //allow administrator log in outside business hours.
                if (roleID != userObject.RoleId)
                {
                    v.Errors.Add(new ValidationError("User.NonBusinessHoursLoginError", username, "Login outside business hours is not allowed, Please try logging in during business hours."));
                    states.Add(typeof(LogInDto), v);
                    return new AuthenticationDataDto() { AppAuthenticationFailed = true };
                }
            }

            //Check if account is not locked,is approved and not deleted.
            if ((userObject.ApprovalStatus != Constants.ApprovalStatus.Approved) || userObject.IsLockedOut || userObject.IsDeleted)
            {
                v.Errors.Add(new ValidationError("Username.AccountLocked", username, "This account is inactive, has been locked or has been Deleted."));
                states.Add(typeof(LogInDto), v);

                return new AuthenticationDataDto() { AppAuthenticationFailed = true };
            }

            //check if user is Dormented.
            if (userObject.LastLogInDate == null)
            {
                //exclude administrator account from being dormant
                if (roleID != userObject.RoleId)
                {
                    int dormentNumberOfDays = NewUserIDDormentNumberDays();
                    if ((Helper.GetLocalDate() - (DateTime)userObject.CreationDate).Days >= dormentNumberOfDays)
                    {
                        UpdateUserDetails(userObject, isDorment: true);
                        v.Errors.Add(new ValidationError("Username.AccountDormented", username, "This account is Dormant as user has not logged for " + dormentNumberOfDays + " days from the day of account creation. Please contact the administrator."));
                        states.Add(typeof(LogInDto), v);
                        return new AuthenticationDataDto() { AppAuthenticationFailed = true };
                    }
                }
            }

            //prevent multi terminal login
            if (userObject.IsOnline)
            {
                var settings = SecurityConfig.GetCurrent();
                var timeOutDate = Helper.GetLocalDate().AddMinutes(settings.Cookie.Timeout * -1);
                if (userObject.LastActivityDate.HasValue && userObject.LastActivityDate.Value >= timeOutDate)
                {
                    v.Errors.Add(new ValidationError("Username.MultiTeminalLogin", username, "You are currently logged in on another terminal. Kindly log out there and retry."));
                    states.Add(typeof(LogInDto), v);
                    return new AuthenticationDataDto() { AppAuthenticationFailed = true };
                }
            }

            //if last log in date/last activity date more than 90 days ago, refuse log in and lock the account
            if (userObject.LastLogInDate.HasValue && userObject.LastLogInDate.Value.Date < Helper.GetLocalDate().AddDays(ActiveUserIDDormentNumberDays()))
            {
                //exclude administrator account from being dormant
                if (roleID != userObject.RoleId)
                {
                    repositoryInstance.UpdateBadPasswordCount(username, true);
                    UpdateUserDetails(userObject, isDorment: true);
                    v.Errors.Add(new ValidationError("Username.AccountDormented", username, "This account is Dormant. Please contact the administrator."));
                    states.Add(typeof(LogInDto), v);
                    return new AuthenticationDataDto() { AppAuthenticationFailed = true };
                }
            }

            //Check if the Account is Expired
            if (userObject.AccountExpiryDate != null)
            {
                if (userObject.AccountExpiryDate < DateTime.Now)
                {
                    //exclude administrator account from expiring
                    if (roleID != userObject.RoleId)
                    {
                        UpdateUserDetails(userObject, isAccountExpired: true);
                        v.Errors.Add(new ValidationError("Username.AccountExpired", username, "This account is Expired. Please contact the administrator."));
                        states.Add(typeof(LogInDto), v);
                        return new AuthenticationDataDto() { AppAuthenticationFailed = true };
                    }
                }
            }

            if (userObject.Password != "Dummy" && (userObject.AccountType == Constants.AccountType.LocalLocal || userObject.AccountType == Constants.AccountType.LocalFinacle))
            {
                //check the password match
                if (userObject.Password != password)
                {
                    //v.Errors.Add(new ValidationError("Username.InvalidUsername", username, "Invalid Username or Password"));
                    v.Errors.Add(new ValidationError("Password.InvalidPassword", password, "Invalid Username or Password"));
                    var settings = SecurityConfig.GetCurrent();
                    var lockAccount = userObject.BadPasswordCount + 1 >= settings.Cookie.MaximumPasswordRetries;
                    repositoryInstance.UpdateBadPasswordCount(username, lockAccount);
                    states.Add(typeof(LogInDto), v);
                    return new AuthenticationDataDto() { AppAuthenticationFailed = true };
                }
            }

            AuthenticationDataDto auth = new AuthenticationDataDto();
            {
                auth.SessionId = Helper.GetNextGuid().ToString();
                auth.Username = username;
                auth.Roles = userObject.UserRole != null ? userObject.UserRole.RoleName : null;
                auth.BranchCode = userObject.BranchID;
                auth.IsFirstLogIn = userObject.IsFirstLogIn;
                auth.FullName = string.Format("{0} {1}", userObject.FirstName, userObject.LastName);
                auth.IsPasswordSet = userObject.Password != "Dummy" && (userObject.AccountType == Constants.AccountType.LocalLocal || userObject.AccountType == Constants.AccountType.LocalFinacle);
                auth.IsRoleSet = userObject.AccountType == Constants.AccountType.ADLocal || userObject.AccountType == Constants.AccountType.LocalLocal; // userObject.UserRole != null && !string.IsNullOrEmpty(userObject.UserRole.RoleName);
                auth.AccountType = userObject.AccountType;
            }

            if (sessionBased)
                UpdateLogInSucceed(username, auth.SessionId);

            return auth;

        }

        public void UpdateLogInSucceed(string username, string sessionId)
        {
            repositoryInstance.UpdateLogInSucceed(username, sessionId);
        }

        public void UpdateRenewSucceed(User userItem)
        {
            repositoryInstance.UpdateRenewSucceed(userItem);
        }

        public AuthenticationDataDto Renew(string sessionId)
        {
            var userObject = repositoryInstance.GetUserBySessionId(sessionId);

            if (userObject == null)
            {
                return null;
            }

            var settings = SecurityConfig.GetCurrent();
            bool renewSucceed = true;
            if ((!userObject.CurrentSessionId.HasValue || sessionId != userObject.CurrentSessionId.ToString()) ||
                (userObject.LastActivityDate.Value.AddMinutes(settings.Cookie.Timeout) < Helper.GetLocalDate()) ||
                ((userObject.ApprovalStatus != Constants.ApprovalStatus.Approved) || userObject.IsLockedOut || userObject.IsDeleted))
            {
                if (userObject.LastActivityDate.Value.AddMinutes(settings.Cookie.Timeout) < Helper.GetLocalDate())
                {
                    userObject.IsOnline = false;
                    userObject.CurrentSessionId = null;
                }
                renewSucceed = false;
            }
            else
            {
                if (settings.Cookie.SlidingExpiration)
                {
                    userObject.LastActivityDate = Helper.GetLocalDate();
                }
            }

            UpdateRenewSucceed(userObject);

            AuthenticationDataDto auth = null;

            if (renewSucceed)
            {
                auth = new AuthenticationDataDto();
                {
                    auth.SessionId = userObject.CurrentSessionId.Value.ToString();
                    auth.Username = userObject.Username;
                    auth.BranchCode = userObject.BranchID;
                    auth.Roles = userObject.UserRole == null ? null : userObject.UserRole.RoleName;
                    auth.FullName = string.Format("{0} {1}", userObject.FirstName, userObject.LastName);
                    auth.IsRoleSet = !(userObject.AccountType == Constants.AccountType.ADFinacle || userObject.AccountType == Constants.AccountType.LocalFinacle);
                }
            }

            return auth;
        }

        public void SignOut(string sessionId)
        {
            repositoryInstance.SignOut(sessionId);
        }

        //public bool ActivateAccount(string activationCode)
        //{
        //    return repositoryInstance.ActivateAccount(activationCode);
        //}

        public void ResetPassword(string username, string email, ref ValidationStateDictionary states)
        {
            ValidationState v = new ValidationState();
            string firstName = string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                v.Errors.Add(new ValidationError("Username.UsernameRequired", null, "Username is not set."));
            }

            if (v.Errors.Count > 0)
            {
                states.Add(typeof(ChangePasswordDto), v);
                return;
            }

            var userObject = repositoryInstance.GetUser(username);
            if (userObject == null)
            {
                v.Errors.Add(new ValidationError("Username.InvalidAccountDetails", username, "Invalid user details, please check and try again."));
            }
            else
            {
                if (userObject.AccountType != Constants.AccountType.LocalFinacle && userObject.AccountType != Constants.AccountType.LocalLocal)
                {
                    v.Errors.Add(new ValidationError("Username.InvalidAccountDetails", username, "Password cannot be changed. Please contact your administrator"));
                }
            }

            if (v.Errors.Count > 0)
            {
                states.Add(typeof(ChangePasswordDto), v);
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                v.Errors.Add(new ValidationError("Email.EmailRequired", null, "Email is not set."));
            }
            else
            {
                if (!Helper.IsEmailValid(email))
                {
                    v.Errors.Add(new ValidationError("Email.EmailInvalid", null, "Email is Invalid."));
                }

                if (userObject.Username.ToLower() != username.ToLower() || userObject.Email.ToLower() != email.ToLower())
                {
                    v.Errors.Add(new ValidationError("Username.UsernameAndEmailNotMatch", username, "Unable to do a password reset for this details."));
                }
            }

            if (v.Errors.Count > 0)
            {
                states.Add(typeof(ChangePasswordDto), v);
                return;
            }

            string newPassword = (new PasswordGenerator()).GeneratePassword(12);
            var hashedPassword = string.Empty;
            var settings = SecurityConfig.GetCurrent();
            if (settings.Cookie.PasswordHashed)
            {
                hashedPassword = PasswordHash.Hash(username.ToLower(), newPassword);
            }
            else
            {
                hashedPassword = newPassword;
            }
            repositoryInstance.ResetUserPassword(username, hashedPassword, true);

            bool status = SendPasswordResetMail(username, firstName, email, newPassword);

            if (!status)
            {
                v.Errors.Add(new ValidationError("Username.UnableToRetrievePassword", username, "Unable to do a password reset for this details."));
                states.Add(typeof(ChangePasswordDto), v);
            }

            return;
        }

        public void UserChangePassword(string userName, string oldPassword, string newPassword, string confirmNewPassword, ref ValidationStateDictionary states)
        {
            ValidationState v = new ValidationState();

            if (string.IsNullOrEmpty(oldPassword))
            {
                v.Errors.Add(new ValidationError("OldPassword.InvalidOldPassword", oldPassword, "Old Password is not set"));
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                v.Errors.Add(new ValidationError("NewPassword.InvalidNewPassword", newPassword, "New Password is not set"));
            }
            else if (newPassword.Length < 8)
            {
                v.Errors.Add(new ValidationError("NewPassword.InvalidNewPassword", newPassword, string.Format("New Password minimum length is {0}", 8)));
            }

            if (string.IsNullOrEmpty(confirmNewPassword))
            {
                v.Errors.Add(new ValidationError("ConfirmNewPassword.InvalidConfirmPassword", confirmNewPassword, "Confirm New Password is not set"));
            }
            else if (newPassword.Length < 8)
            {
                v.Errors.Add(new ValidationError("ConfirmNewPassword.InvalidNewPassword", newPassword, string.Format("Confirm Password minimum length is {0}", 8)));
            }

            if (confirmNewPassword != newPassword)
            {
                v.Errors.Add(new ValidationError("ConfirmNewPassword.ConfirmPasswordNotMatch", confirmNewPassword, "Password and confirm password not equal."));
            }
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(newPassword))
            {
                if (userName.ToLower() == newPassword.ToLower())
                {
                    v.Errors.Add(new ValidationError("NewPassword.ConfirmPasswordNotMatch", newPassword, "Password cannot be user name."));
                }
            }

            var userObject = repositoryInstance.GetUser(userName);
            if (userObject == null)
            {
                v.Errors.Add(new ValidationError("OldPassword.InvalidAccountDetails", userName, "Invalid user details, please check and try again."));
            }
            else
            {
                if (userObject.AccountType != Constants.AccountType.LocalFinacle && userObject.AccountType != Constants.AccountType.LocalLocal)
                {
                    v.Errors.Add(new ValidationError("OldPassword.InvalidAccountDetails", userName, "Password cannot be changed. Please contact your administrator"));
                }
            }
            if ((int)Utility.PasswordAdvisor.CheckStrength(newPassword) < 4)
            {
                v.Errors.Add(new ValidationError("Password.ComplexityViolationError", newPassword, "Password failed the company's password complexity policy check."));
            }
            if (!v.IsValid)
            {
                states.Add(typeof(ChangePasswordDto), v);
                return;
            }

            var storePassword = userObject.Password;
            var settings = SecurityConfig.GetCurrent();
            if (settings.Cookie.PasswordHashed)
            {
                oldPassword = PasswordHash.Hash(userName.ToLower(), oldPassword);
                newPassword = PasswordHash.Hash(userName.ToLower(), newPassword);
            }

            if (oldPassword != storePassword)
            {
                v.Errors.Add(new ValidationError("OldPassword.InvalidOldPassword", oldPassword, "Password change not successful. Old password is incorrect."));
                states.Add(typeof(ChangePasswordDto), v);
                return;
            }
            int unUsablePreviousPasswordCount = 0;
            if (passwordHistoryServiceInstance.IsRepeatingPassword(new PasswordHistoryModel() { UserName = userName, Password = newPassword }, out unUsablePreviousPasswordCount))
            {
                v.Errors.Add(new ValidationError("NewPassword.PreviouslyUsedError", oldPassword, "Password change not successful. Last " + unUsablePreviousPasswordCount + " Passwords cannot be used."));
                states.Add(typeof(ChangePasswordDto), v);
                return;
            }

            repositoryInstance.ResetUserPassword(userName, newPassword, false);
            passwordHistoryServiceInstance.InsertPassword(new PasswordHistoryModel() { UserName = userName, Password = newPassword });

        }

        public UserListingResponse GetUserList(PagerItemsII parameter)
        {
            return repositoryInstance.GetUserList(parameter);
        }

        public IEnumerable<Role> GetRoleList()
        {
            //return cacheService.GetOrAdd(Constants.General.RoleList, repositoryInstance.GetRoleList()) as IEnumerable<Role>;

            var roleList = cacheService.Get(Constants.General.RoleList) as IEnumerable<Role>;
            if (roleList == null)
            {
                roleList = repositoryInstance.GetRoleList();
                cacheService.Add(Constants.General.RoleList, roleList);
            }
            return roleList;
        }

        //public IEnumerable<Profile> GetProfileList()
        //{
        //    var profileList = cacheService.Get(Constants.General.ProfileList) as IEnumerable<Profile>;
        //    if (profileList == null)
        //    {
        //        profileList = repositoryInstance.GetProfileList();
        //        cacheService.Add(Constants.General.ProfileList, profileList);
        //    }
        //    return profileList;
        //}

        public IEnumerable<RoleModuleAccess> GetModuleAccessList()
        {
            var moduleAccessList = cacheService.Get(Constants.General.ModuleAccessList) as IEnumerable<RoleModuleAccess>;
            if (moduleAccessList == null)
            {
                moduleAccessList = repositoryInstance.GetModuleAccessList();
                cacheService.Add(Constants.General.ModuleAccessList, moduleAccessList);
            }
            return moduleAccessList;
        }

        public IEnumerable<RoleModuleAccessAggregate> GetRoleModuleAccessList()
        {
            var moduleAccessList = cacheService.Get(Constants.General.RoleModuleAccessList) as IEnumerable<RoleModuleAccessAggregate>;
            if (moduleAccessList == null)
            {
                moduleAccessList = repositoryInstance.GetRoleModuleAccessList();
                cacheService.Add(Constants.General.RoleModuleAccessList, moduleAccessList, new CacheItemPolicy() { SlidingExpiration = new TimeSpan(12, 0, 0) });
            }
            return moduleAccessList;
        }

        public RoleViewModel GetRole(Guid roleId)
        {
            return repositoryInstance.GetRole(roleId);
        }

        //public RoleProfileModel GetRole(Guid id)
        //{
        //    return repositoryInstance.GetRole(id);
        //}

        //public void AddRole(RoleProfileModel roleToAdd, ref ValidationStateDictionary states)
        //{
        //    var v = new RoleProfileModelValidator().Validate(roleToAdd, validationRepositoryInstance);

        //    if (v.Errors.Count > 0)
        //    {
        //        states.Add(typeof(RoleProfileModel), v);
        //        return;
        //    }

        //    roleToAdd.CurrentRole.RoleId = Helper.GetNextGuid();
        //    repositoryInstance.AddRole(roleToAdd);
        //    cacheService.Remove(Constants.General.RoleList);
        //    cacheService.Remove(Constants.General.ProfileModuleAccessList);
        //    cacheService.Remove(Constants.General.RoleProfileList);
        //    cacheService.RemoveEndWith(Constants.General.RoleNames);
        //}

        //public int EditRole(RoleProfileModel roleToEdit, ref ValidationStateDictionary states)
        //{
        //    var v = new RoleProfileModelValidator().Validate(roleToEdit, validationRepositoryInstance);

        //    if (v.Errors.Count > 0)
        //    {
        //        states.Add(typeof(RoleProfileModel), v);
        //        return 0;
        //    }

        //    var rowAffected = repositoryInstance.EditRole(roleToEdit);
        //    cacheService.Remove(Constants.General.RoleList);
        //    cacheService.Remove(Constants.General.ProfileModuleAccessList);
        //    cacheService.Remove(Constants.General.RoleProfileList);
        //    cacheService.RemoveEndWith(Constants.General.RoleNames);

        //    return rowAffected;
        //}

        public void LockUnlockAccount(string id, bool isLocked)
        {
            repositoryInstance.LockUnlockAccount(id, isLocked);
        }

        public void DeleteUser(string id, string optionTaken)
        {
            repositoryInstance.DeleteUser(id, optionTaken);
        }

        public void DeleteRole(Guid id, ref ValidationStateDictionary states)
        {
            ValidationState v = new ValidationState();

            bool result = repositoryInstance.DeleteRole(id);
            if (!result)
            {
                v.Errors.Add(new ValidationError("RoleName.UsserExistForTheRole", null, "Role cannot be deleted as there are users with that role. Re-assign the user to another role an try again."));
                states.Add(typeof(Role), v);
            }
        }

        public string GetUserPassword(string username)
        {
            return repositoryInstance.GetUserPassword(username);
        }

        private void RegistrationNotification(User userData, bool withLoginDetails)
        {
            var mailSetting = MailSettings.GetCurrent();
            if (!mailSetting.IsEnabled) return;

            if (string.IsNullOrEmpty(userData.Email))
                return;

            var companyName = ConfigurationManager.AppSettings["CompanyName"].ToString();
            var activationLink = Helper.GetRootURL() + "Account/AccountActivate?id=" + Helper.UrlEncode(Crypto.Encrypt(userData.Username));

            //activationLink = Helper.MakeLink(activationLink, "Activation link");
            var final = string.Empty;
            if (withLoginDetails)
            {
                var messageFormat = "Dear {1},{0}{0}Thank you for registering on our site.{0}{0}Your login details are as follows:{0}{0}Username: {4}{0}{0}Password: {5}{0}{0}Endeavour to change this password to your choice after logging in.{0}{0}Please click the link below to activate your account{0}{0}{2}{0}{0}Regards.{0}{0}{3}";
                final = string.Format(messageFormat, System.Environment.NewLine, userData.FirstName, activationLink, companyName, userData.Username, userData.ConfirmPassword);
            }
            else
            {
                var messageFormat = "Dear {1},{0}{0}Thank you for registering on our site.{0}{0}Please click the link below to activate your account{0}{0}{2}{0}{0}Regards.{0}{0}{3}";
                final = string.Format(messageFormat, System.Environment.NewLine, userData.FirstName, activationLink, companyName);
            }
            var u = new UserMailDto() { DisplayName = string.Format("{0}{1}", userData.FirstName, string.IsNullOrEmpty(userData.LastName) ? string.Empty : " " + userData.LastName), EmailAddress = userData.Email };
            var addressList = new List<UserMailDto>();
            addressList.Add(u);

            logger.SendEmail(addressList, null, final, string.Empty, "Registration confirmation", false);
        }


        private bool SendPasswordResetMail(string username, string userFirstName, string emailAddress, string newPassword)
        {
            var mailSetting = MailSettings.GetCurrent();
            if (!mailSetting.IsEnabled) return true;

            var status = true;
            var companyName = ConfigurationManager.AppSettings["CompanyName"].ToString();
            var messageFormat = "Dear {1},{0}{0}Your password has been reset. The new password is {2}{0}{0}Please change this password as soon as you sign in.{0}{0}Regards.{0}{0}{3}";
            var final = string.Format(messageFormat, System.Environment.NewLine, userFirstName, newPassword, companyName);

            var addressList = new List<UserMailDto>();
            addressList.Add(new UserMailDto() { DisplayName = userFirstName, EmailAddress = emailAddress });
            logger.SendEmail(addressList, null, final, string.Empty, "Password reset confirmation", false);
            return status;
        }

        /// <summary>
        /// Returns the Number of days after which the UserID will be Expired.
        /// pulls the value from PortalSettings table for Key: expiredIDNoOfDays, if key doesnt exist -90 is set to default value
        /// </summary>
        /// <returns>Returns Int number of days</returns>
        private int ActiveUserIDDormentNumberDays()
        {
            return GetNumberOfDays(Constants.PortalSettingsKeysConstants.ACTIVEUSERIDDORMANTNUMBERDAYS,
                                   Convert.ToInt16(Constants.PortalSettingsKeyFallBackValues.ACTIVEUSERIDDORMANTNUMBERDAYS)) * (-1);
        }

        private int NewUserIDDormentNumberDays()
        {
            return GetNumberOfDays(Constants.PortalSettingsKeysConstants.NEWUSERIDDORMANTNUMBERDAYS,
                                 Convert.ToInt16(Constants.PortalSettingsKeyFallBackValues.NEWUSERIDDORMANTNUMBERDAYS));
        }

        private int GetNumberOfDays(string key, int defaultValue)
        {
            bool tryGetexpiredIDNoOfDays = false;
            PortalSetting portalsetting = portalSettingsServiceInstance.GetSettingByKey(key);
            tryGetexpiredIDNoOfDays = int.TryParse(portalsetting != null ? portalsetting.Value : string.Empty, out expiredIDNoOfDays);
            expiredIDNoOfDays = expiredIDNoOfDays == 0 ? defaultValue : expiredIDNoOfDays;
            return expiredIDNoOfDays;
        }

        #region Validate Business hours
        private bool IsBusinessHour()
        {
            bool isWithinBusinessHour = false;
            if (businessStartHour == default(DateTime)) { businessStartHour = getConvertedDateTime(portalSettingsServiceInstance.GetSettingByKey(Constants.PortalSettingsKeysConstants.BUSINESSHOURSTART).Value, Constants.PortalSettingsKeyFallBackValues.BUSINESSHOURSTART); }
            if (businessEndHour == default(DateTime)) { businessEndHour = getConvertedDateTime(portalSettingsServiceInstance.GetSettingByKey(Constants.PortalSettingsKeysConstants.BUSINESSHOURCLOSE).Value, Constants.PortalSettingsKeyFallBackValues.BUSINESSHOURCLOSE); }
            if ((DateTime.Now > businessStartHour) && (DateTime.Now < businessEndHour)) { isWithinBusinessHour = true; }
            return isWithinBusinessHour;
        }

        private DateTime getConvertedDateTime(string time, string defaultTime)
        {
            DateTime businessTime = new DateTime();
            if (!string.IsNullOrWhiteSpace(time))
            {
                if (!DateTime.TryParseExact(time, "HH:mm:ss", null, DateTimeStyles.None, out businessTime))
                { businessTime = DateTime.ParseExact(defaultTime, "HH:mm:ss", CultureInfo.InvariantCulture); }
            }
            return businessTime;
        }

        #endregion

        private void UpdateUserDetails(User userObject, bool? isDorment = null, bool? isAccountExpired = null)
        {
            if (isAccountExpired != null) { userObject.IsAccountExpired = (bool)isAccountExpired; }
            if (isDorment != null) { userObject.IsDormented = (bool)isDorment; }

            repositoryInstance.UpdateUserInfo(userObject);
        }

        private void updateUserAccountExpiry(User userObject)
        {
            if (userObject.IsAccountExpired)
            {
                int accountExpiryNumberDays = Convert.ToInt32(Constants.PortalSettingsKeyFallBackValues.ACCOUNTEXPIRYNUMBERDAYS);
                int.TryParse(portalSettingsServiceInstance.GetSettingByKey(Constants.PortalSettingsKeysConstants.ACCOUNTEXPIRYNUMBERDAYS).Value, out accountExpiryNumberDays);
                userObject.AccountExpiryDate = DateTime.Now.Date.AddDays(accountExpiryNumberDays);
                userObject.IsAccountExpired = false;
            }
        }

        public User GetUserBySessionId(string sessionId)
        {
            var user = repositoryInstance.GetUserBySessionId(sessionId);
            return user;
        }
    }
}
