//using PermissionManagement.IoC;
using DryIoc;
using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;

namespace PermissionManagement.Web
{
    public sealed class Access
    {

        private Access()
        {
        }

        public static bool Is2FAEnabled()
        {
            var settings = SecurityConfig.GetCurrent();
            return settings.Cookie.Enable2FA;
        }

       public static bool SignIn(string username, string password, string tokenCode, ISecurityService authenticationService, IFinacleRepository finacleRepository, out bool isFirstLogIn, ref ValidationStateDictionary states)
        {
            isFirstLogIn = false;
            AuthenticationDataDto currentUser = null;
            bool validateWithAD = false;
            bool useFinacleRole = false;
            var hashedPassword = string.Empty;
            var settings = SecurityConfig.GetCurrent();

            if (settings.Cookie.PasswordHashed && !string.IsNullOrEmpty(password))
            {
                var lowerUsername = string.Empty;
                if (!string.IsNullOrEmpty(username))
                {
                    lowerUsername = username.ToLower();
                }
                hashedPassword = PasswordHash.Hash(lowerUsername, password);
            }
            currentUser = authenticationService.SignIn(username, hashedPassword, true, ref states);
            if (currentUser != null)
            {
                if (currentUser.AppAuthenticationFailed)
                {
                    return false;
                }
            }
            states.Clear();

            SetAuthMode(currentUser, ref validateWithAD, ref useFinacleRole);

            AuthenticationResponse result = new AuthenticationResponse();
            if (validateWithAD)
            {
                var ADResult = ADAuthentication(username, password, tokenCode, authenticationService, finacleRepository, states, settings, ref result);
                if (!ADResult)
                {
                    if (currentUser != null)
                    {
                        authenticationService.SignOut(currentUser.SessionId);
                    }
                    return false;
                }
            }

            if (useFinacleRole)
            {
                currentUser = FinacleAuthorization(username, authenticationService, finacleRepository, currentUser, result);
            }
            else
            {
                if (!ValidateLocalUserRole(states, currentUser))
                {
                    if (currentUser != null)
                    {
                        authenticationService.SignOut(currentUser.SessionId);
                    }
                    return false;
                }
            }

            if (!ValidateRoleAndUser(username, authenticationService, states, currentUser))
            {
                if (currentUser != null)
                {
                    authenticationService.SignOut(currentUser.SessionId);
                }
                return false;
            }

            if (!Check2FA(username, tokenCode, states, currentUser, settings))
            {
                if (currentUser != null)
                {
                    authenticationService.SignOut(currentUser.SessionId);
                }
                return false;
            }

            CreateThreadPrincipalAndAuthCookie(currentUser, settings);

            isFirstLogIn = validateWithAD == true ? false : currentUser.IsFirstLogIn;

            //if (useFinacleRole) /no longer needed.
            //{
            //    //cache the role, so that we don't go back to finacle on every session renewal
            //    ICacheService cacheService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ICacheService>();
            //    CacheItemPolicy policy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, settings.Cookie.Timeout + 1, 0) };
            //    cacheService.Add(string.Format("UserSessionID:{0}", currentUser.SessionId), currentUser.Roles, policy);
            //}
            return true;
        }

       private static bool ValidateRoleAndUser(string username, ISecurityService authenticationService, ValidationStateDictionary states, AuthenticationDataDto currentUser)
       {
           if (currentUser == null)
           {
               ValidationState v = new ValidationState();
               v.Errors.Add(new ValidationError("Username.InvalidUsername", username, "Invalid Username or Password"));
               states.Add(typeof(LogInDto), v);
               return false;
           }

           var localRole = authenticationService.GetRoleList().Where(f => f.RoleName.ToLower() == currentUser.Roles.ToLower()).Select(j => j.RoleName).FirstOrDefault();
           if (string.IsNullOrEmpty(localRole))
           {
               ValidationState v = new ValidationState();
               v.Errors.Add(new ValidationError("General.InvalidRole", currentUser.Roles, string.Format("User role {0} does not exist on this application, contact the administrator", currentUser.Roles)));
               states.Add(typeof(LogInDto), v);
               return false;
           }
           return true;
       }

       private static bool ADAuthentication(string username, string password, string tokenCode, ISecurityService authenticationService, IFinacleRepository finacleRepository, ValidationStateDictionary states, SecurityConfig settings, ref AuthenticationResponse result)
       {
           var status3 = true;
           result = Authentication(new AuthenticationRequest()
           {
               UserID = username,
               Password = password,
               TokenCode = tokenCode
           }, false, settings, authenticationService, finacleRepository);
           if (result.ResponseCode != "0")
           {
               ValidationState v = new ValidationState();
               v.Errors.Add(new ValidationError("Username.InvalidUsername", username, "Invalid Username or Password"));
               states.Add(typeof(LogInDto), v);
               status3 = false;
           }
           return status3;
       }

       private static void SetAuthMode(AuthenticationDataDto currentUser, ref bool validateWithAD, ref bool useFinacleRole)
       {
           if (currentUser == null)
           {
               validateWithAD = true;
               useFinacleRole = true;
           }

           if (currentUser != null && !currentUser.IsPasswordSet && !currentUser.IsRoleSet)
           {
               validateWithAD = true;
               useFinacleRole = true;
           }
           if (currentUser != null && !currentUser.IsPasswordSet && currentUser.IsRoleSet)
           {
               validateWithAD = true;
               useFinacleRole = false;
           }
           if (currentUser != null && currentUser.IsPasswordSet && !currentUser.IsRoleSet)
           {
               validateWithAD = false;
               useFinacleRole = true;
           }
           if (currentUser != null && currentUser.IsPasswordSet && currentUser.IsRoleSet)
           {
               validateWithAD = false;
               useFinacleRole = false;
           }
       }

       private static void CreateThreadPrincipalAndAuthCookie(AuthenticationDataDto currentUser, SecurityConfig settings)
       {
           //create the cookie                
           Identity identity = new Identity(new Guid(currentUser.SessionId), currentUser.Username, currentUser.Roles, currentUser.FullName, currentUser.BranchCode, currentUser.AccountType);
           Principal principal = new Principal(identity, identity.Roles, identity.AccountType);

           System.Web.HttpContext.Current.User = principal;
           System.Threading.Thread.CurrentPrincipal = principal;

           var cookie = new AuthCookie();
           {
               cookie.SessionUid = currentUser.SessionId;
               if (settings.Cookie.CookieOnlyCheck)
               {
                   cookie.Username = currentUser.Username;
                   cookie.UserRoles = currentUser.Roles;
                   cookie.BranchCode = currentUser.BranchCode;
                   cookie.AccountType = currentUser.AccountType;
                   cookie.AuthExpiry = Helper.GetLocalDate().AddMinutes(settings.Cookie.Timeout);
               }
               cookie.Save();
           }

           if (!settings.Cookie.CookieOnlyCheck)
           {
               HttpContext.Current.Cache.Insert(currentUser.SessionId, currentUser, null, DateTime.UtcNow.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration);
           }
       }

       private static bool Check2FA(string username, string tokenCode, ValidationStateDictionary states, AuthenticationDataDto currentUser, SecurityConfig settings)
       {
           bool status2 = true;
           if (settings.Cookie.Enable2FA)
           {
               var status = (currentUser.IsPasswordSet) && settings.Cookie.ExemptLocalAccountFrom2FA;

               if (!status)
               {
                   var response = Auth2FAresponse(new Authentication2FARequest() { TokenCode = tokenCode, UserID = username });
                   if (response.ResponseCode != "0")
                   {
                       ValidationState v = new ValidationState();
                       v.Errors.Add(new ValidationError("TokenCode.InvalidTokenCode", tokenCode, response.ResponseDescription)); //"Invalid token code"
                       states.Add(typeof(LogInDto), v);
                       status2 =  false;
                   }
               }
           }
           return status2;
       }

       private static bool ValidateLocalUserRole(ValidationStateDictionary states, AuthenticationDataDto currentUser)
       {
           bool finalStatus = true;
           if (currentUser != null)
           {
               if (string.IsNullOrEmpty(currentUser.Roles))
               {
                   ValidationState v = new ValidationState();
                   v.Errors.Add(new ValidationError("General.InvalidRole", currentUser.Roles, string.Format("User role {0} does not exist on this application, contact the administrator", currentUser.Roles)));
                   states.Add(typeof(LogInDto), v);
                   return finalStatus;
               }
           }
           else
           {
               ValidationState v = new ValidationState();
               v.Errors.Add(new ValidationError("General.InvalidRole", currentUser.Roles, string.Format("User role {0} does not exist on this application, contact the administrator", currentUser.Roles)));
               states.Add(typeof(LogInDto), v);
               return finalStatus;
           }
           return finalStatus;
       }

       private static AuthenticationDataDto FinacleAuthorization(string username, ISecurityService authenticationService, IFinacleRepository finacleRepository, AuthenticationDataDto currentUser, AuthenticationResponse result)
       {
           //get finacle role
           FinacleRole finacleRole = finacleRepository.GetUserRoleFromFinacle(username);
           if (finacleRole == null)
               finacleRole = new FinacleRole() { UserID = username, BranchCode = null, ApplicationName = string.Empty };

           var roleId = authenticationService.GetRoleList().Where(r => r.RoleName.ToLower() == finacleRole.ApplicationName.ToLower()).Select(k => k.RoleId).FirstOrDefault();

           if (currentUser == null)
           {
               //create the user record for session  management purpose, only for AD/Finacle users logging in for the first time, 
               //if role is recognised in this application
               var userObject = new User()
               {
                   BadPasswordCount = 0,
                   CreationDate = Helper.GetLocalDate(),
                   CurrentSessionId = Helper.GetNextGuid(),
                   Email = result.Email,
                   FirstName = result.FirstName,
                   //ApprovalStatus = true,
                   IsFirstLogIn = false,
                   Initial = string.Empty,
                   LastLogInDate = Helper.GetLocalDate(),
                   IsLockedOut = false,
                   IsOnline = true,
                   LastActivityDate = Helper.GetLocalDate(),
                   LastName = result.LastName,
                   InitiatedBy = username,
                   //ApprovedBy = "NULL",
                   Username = username,
                   Telephone = "N/A",
                   Password = "Dummy",
                   AccountType = Constants.AccountType.ADFinacle,
                   ApprovalStatus = Constants.ApprovalStatus.Approved,
                   BranchID = finacleRole.BranchCode,
                   IsDeleted = false,
                   UserRole = new Role() {  RoleId = roleId }
               };

               if (roleId != Guid.Empty)
               {
                   authenticationService.AddUserForSessionMgmt(userObject);
               }

               currentUser = new AuthenticationDataDto();
               {
                    currentUser.SessionId = userObject.CurrentSessionId.ToString();
                    currentUser.Username = username;
                    currentUser.Roles = finacleRole.ApplicationName;
                    currentUser.IsFirstLogIn = false;
                    currentUser.FullName = string.Format("{0} {1}", userObject.FirstName, userObject.LastName);
                    currentUser.IsPasswordSet = false;
                    currentUser.BranchCode = finacleRole.BranchCode;
               }

           }

           //with finacle authorization, we check whether the locally saved role
           //is the same as we got from Finacle, if not we update the local db.
           //and we always override local role even if available
           //this ensure if the role changed in Finacle it is inherited in the application.
           //we also ensure if user branch in finacle has changed, is changed here too
           if ((currentUser.Roles.ToLower() != finacleRole.ApplicationName.ToLower() && roleId != Guid.Empty) || (currentUser.BranchCode != finacleRole.BranchCode && !string.IsNullOrEmpty(finacleRole.BranchCode)))
           {
               authenticationService.UpdateUserRoleUserBranch(currentUser.Username, roleId, finacleRole.BranchCode);
               currentUser.Roles = finacleRole.ApplicationName;
               currentUser.BranchCode = finacleRole.BranchCode;
           }
           

           return currentUser;
       }

        public static AuthenticationDataDto Renew(string sessionUid, ISecurityService authenticationService)
        {
            //Attempt to retrieve roles from cache, if we succeed, we skip renewal.
            var userRole = HttpContext.Current.Cache[sessionUid] as AuthenticationDataDto;
            if (userRole != null) return userRole;

            var currentUser = authenticationService.Renew(sessionUid);

            if (currentUser == null)
            {
                return null;
            }
            else
            {              
                //if (!currentUser.IsRoleSet) //no longer needed
                //{
                //    //this means the user is authorised with Finacle Role - So the role may not be in our local database
                //    ICacheService cacheService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ICacheService>();
                //    var userRoleName = cacheService.Get(string.Format("UserSessionID:{0}", sessionUid)) as string;
                //    if (userRoleName != null)
                //        currentUser.Roles = userRoleName;
                //}

                //keep info in cache for 30 seconds.
                HttpContext.Current.Cache.Insert(sessionUid, currentUser, null, DateTime.UtcNow.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration);
                return currentUser;
            }
        }

        public static AuthenticationDataDto Renew(string sessionUid)
        {
            ISecurityService authenticationService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ISecurityService>();
            return Renew(sessionUid, authenticationService);
        }

        public static IList<string> GetViewableModules(string roleName)
        {
            ISecurityService authenticationService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ISecurityService>();

            var g = authenticationService.GetRoleList().Where(m => m.RoleName.ToLower() == roleName.ToLower()).Select(m => m).FirstOrDefault();
            if (g == null) { return new List<string>(); }
            var g1 = authenticationService.GetRoleModuleAccessList().Where(h => h.RoleId == g.RoleId).Select(s => s);
            return g1.Where(h => h.AggregateAccess.Contains(Constants.AccessRights.View)).Select(b => b.ModuleName.ToLower()).Distinct().ToList();
        }

        public static bool IsAreaRestricted(string rootArea, string areaName, ISecurityService authenticationService)
        {
            if (authenticationService.GetModuleAccessList().Where(m => m.ModuleName.ToLower() == rootArea.ToLower()).Count() > 0) { return false; }
            return authenticationService.GetModuleAccessList().Where(m => m.ModuleName.ToLower() == areaName.ToLower()).Count() > 0;
        }

        public static bool IsAreaAccesibleToUser(string roleName, string rootArea, string areaName, ISecurityService authenticationService)
        {
            var g = authenticationService.GetRoleList().Where(m => m.RoleName.ToLower() == roleName.ToLower()).Select(m => m).FirstOrDefault();
            if (g == null) { return false; }
            var g1 = authenticationService.GetRoleModuleAccessList().Where(h => h.RoleId == g.RoleId).Select(s => s).ToList();

            return g1.Where(h => h.ModuleName.ToLower() == rootArea.ToLower() && h.AggregateAccess.Contains(Constants.AccessRights.View)).Count() > 0
                   || g1.Where(h => h.ModuleName.ToLower() == areaName.ToLower() && h.AggregateAccess.Contains(Constants.AccessRights.View)).Count() > 0;
        }

        public static bool IsAccessRightInRoleProfile(string moduleName, string accessRight)
        {
            var roleName = System.Web.HttpContext.Current.User.Identity.IsAuthenticated ? ((Identity)System.Web.HttpContext.Current.User.Identity).Roles : string.Empty;
            var  r = IsAccessRightInRoleProfile(roleName, moduleName, accessRight);
            return r;
        }

        public static bool IsMakerCheckerNotInitiatorOrChecker(string moduleName, string initiatedBy)
        {
            if (IsAccessRightInRoleProfile(moduleName, Constants.AccessRights.Verify) == true) return true;
 
            var r =(Access.IsAccessRightInRoleProfile(moduleName, Constants.AccessRights.MakeOrCheck) && initiatedBy != Helper.GetLoggedInUserID());
            return r;
        }

        public static bool IsFormEditable(string moduleName, string currentApprovalStatus)
        {
            return !(Access.IsAccessRightInRoleProfile(moduleName, Constants.AccessRights.MakeOrCheck) &&
                currentApprovalStatus == Constants.ApprovalStatus.Pending) &&
                Access.IsAccessRightInRoleProfile(Constants.Modules.UserSetup, Constants.AccessRights.Edit);
        }

        public static bool CanEdit(string moduleName, string initiatedBy, string approvalStatus, bool IsDeleted)
        {
            if (Access.IsAccessRightInRoleProfile(moduleName, Constants.AccessRights.Verify) == true  && approvalStatus == Constants.ApprovalStatus.Pending) return true;

            if (Access.IsAccessRightInRoleProfile(moduleName, Constants.AccessRights.MakeOrCheck) && approvalStatus == Constants.ApprovalStatus.Pending && initiatedBy != Helper.GetLoggedInUserID()) return true;  //&& IsDeleted == false

            if (Access.IsAccessRightInRoleProfile(moduleName, Constants.AccessRights.MakeOrCheck) && approvalStatus == Constants.ApprovalStatus.Approved) return true;

            if (Access.IsAccessRightInRoleProfile(moduleName, Constants.AccessRights.Edit) && approvalStatus == Constants.ApprovalStatus.Approved) return true;

            if (Access.IsAccessRightInRoleProfile(moduleName, Constants.AccessRights.MakeOrCheck) && approvalStatus == Constants.ApprovalStatus.RejectedForCorrection  && initiatedBy == Helper.GetLoggedInUserID()) return true;

            return false;
        }

        public static bool AreAccessRightsInRoleProfile(string moduleName, string[] accessRights, bool isExactMatch)
        {
            var roleName = System.Web.HttpContext.Current.User.Identity.IsAuthenticated ? ((Identity)System.Web.HttpContext.Current.User.Identity).Roles : string.Empty;
            ISecurityService authenticationService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ISecurityService>();

            var g = authenticationService.GetRoleList().Where(m => m.RoleName.ToLower() == roleName.ToLower()).Select(m => m).FirstOrDefault();
            if (g == null) { return false; }
            var g1 = authenticationService.GetRoleModuleAccessList().Where(h => h.RoleId == g.RoleId).Select(s => s);

            if (isExactMatch)
            return g1.Where(h => h.ModuleName.ToLower() == moduleName.ToLower() 
                && Helper.AreItemsInList(Helper.CSVToStringArray(h.AggregateAccess), accessRights)).Count() > 0;
            else
                return g1.Where(h => h.ModuleName.ToLower() == moduleName.ToLower()
                    && Helper.IsAnyItemInList(Helper.CSVToStringArray(h.AggregateAccess), accessRights)).Count() > 0;
        }

        public static bool IsAccessRightInRoleProfile(string roleName, string moduleName, string accessRight)
        {
            ISecurityService authenticationService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ISecurityService>();
            return IsAccessRightInRoleProfile(roleName, moduleName, accessRight, authenticationService);
        }

        public static bool IsAccessRightInRoleProfile(string roleName, string moduleName, string accessRight, ISecurityService authenticationService)
        {
            var g = authenticationService.GetRoleList().Where(m => m.RoleName.ToLower() == roleName.ToLower()).Select(m => m).FirstOrDefault();
            if (g == null) { return false; }
            return authenticationService.GetRoleModuleAccessList().Where(h => h.RoleId == g.RoleId && h.ModuleName.ToLower() == moduleName.ToLower() && h.AggregateAccess.Contains(accessRight)).Count() > 0; 
        }

        public static bool IsAdminModuleInRoleProfile()
        {
            var roleName = ((Identity)System.Web.HttpContext.Current.User.Identity).Roles;
            ISecurityService authenticationService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ISecurityService>();
            return IsAdminModuleInRoleProfile(roleName, authenticationService);
        }

        public static bool IsAdminModuleInRoleProfile(string roleName, ISecurityService authenticationService)
        {
            var g = authenticationService.GetRoleList().Where(m => m.RoleName.ToLower() == roleName.ToLower()).Select(m => m).FirstOrDefault();
            if (g == null) { return false; }

            return authenticationService.GetRoleModuleAccessList().Where(p => p.RoleId == g.RoleId).Where(h => h.IsAdmin == true).Count() > 0;
        }

        public static void SignOut(ISecurityService authenticationService)
        {
            var cookie = AuthCookie.GetCurrent();

            if (cookie != null)
            {
                if (!string.IsNullOrEmpty(cookie.SessionUid))
                {
                    HttpContext.Current.Cache.Remove(cookie.SessionUid);
                    //ICacheService cacheService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ICacheService>();
                    //cacheService.Remove(string.Format("UserSessionID:{0}", cookie.SessionUid));
                    if (string.IsNullOrEmpty(cookie.Username))
                    {
                        authenticationService.SignOut(cookie.SessionUid);
                    }
                }

                cookie.SessionUid = null;
                cookie.Username = null;
                cookie.UserRoles = null;
                cookie.BranchCode = null;
                cookie.AuthExpiry = Helper.GetLocalDate().AddDays(-1);
                cookie.Delete();
            }

            //create a new anonymous identity/principal.
            var identity = new System.Security.Principal.GenericIdentity("");
            var principal = new System.Security.Principal.GenericPrincipal(identity, null);

            //assign the anonymous principle to the context
            System.Web.HttpContext.Current.User = principal;
            System.Threading.Thread.CurrentPrincipal = principal;
        }

        internal static bool IsAccessAllowed(HttpContextBase context, string moduleName, string[] accessRightList, bool? verifyAreaAccessRight)
        {
            ISecurityService authenticationService = PerfomAccessCheck(context);
            if ((string.IsNullOrEmpty(moduleName) && accessRightList == null))
            {

                //check permission against contentarea, if any, otherwise, return true
                if (!verifyAreaAccessRight.HasValue) { return context.User.Identity.IsAuthenticated; }

                if (verifyAreaAccessRight.HasValue && !verifyAreaAccessRight.Value) { return true; }

                if (verifyAreaAccessRight.Value)
                {
                    var routeData = context.Request.RequestContext.RouteData;
                    var areaName = routeData.Values["areaName"] as string;
                    var areaParent = routeData.Values["rootParent"] as string;

                    if (string.IsNullOrEmpty(areaName)) { return true; } //if we misapplied such that we have no area name, then we cannot proceed.

                    var r = IsAreaRestricted(areaParent, areaName, authenticationService);

                    if (!r) { return true; }
                    if (!context.User.Identity.IsAuthenticated) { return false; }

                    return IsAreaAccesibleToUser(((Identity)context.User.Identity).Roles, areaParent, areaName, authenticationService);
                }
            }

            if ((!string.IsNullOrEmpty(moduleName) && accessRightList != null) && !context.User.Identity.IsAuthenticated)
            { return false; }

            //finally check user permission against the module requested permission
            var urlAuthorized = Access.AreAccessRightsInRoleProfile(moduleName, accessRightList, false);
            return urlAuthorized;

        }

        internal static bool IsAccessAllowed(HttpContextBase context, string moduleName, string accessRight, bool? verifyAreaAccessRight)
        {
            ISecurityService authenticationService = PerfomAccessCheck(context);

            if ((string.IsNullOrEmpty(moduleName) && string.IsNullOrEmpty(accessRight)))
            {
                //check permission against contentarea, if any, otherwise, return true
                if (!verifyAreaAccessRight.HasValue) { return context.User.Identity.IsAuthenticated; }

                if (verifyAreaAccessRight.HasValue && !verifyAreaAccessRight.Value) { return true; }

                if (verifyAreaAccessRight.Value)
                {
                    var routeData = context.Request.RequestContext.RouteData;
                    var areaName = routeData.Values["areaName"] as string;
                    var areaParent = routeData.Values["rootParent"] as string;

                    if (string.IsNullOrEmpty(areaName)) { return true; } //if we misapplied such that we have no area name, then we cannot proceed.

                    var r = IsAreaRestricted(areaParent, areaName, authenticationService);

                    if (!r) { return true; }
                    if (!context.User.Identity.IsAuthenticated) { return false; }

                    return IsAreaAccesibleToUser(((Identity)context.User.Identity).Roles, areaParent, areaName, authenticationService);
                }
            }

            if ((!string.IsNullOrEmpty(moduleName) && !string.IsNullOrEmpty(accessRight)) && !context.User.Identity.IsAuthenticated)
            { return false; }

            //finally check user permission against the module requested permission
            var urlAuthorized = Access.IsAccessRightInRoleProfile(((Identity)context.User.Identity).Roles, moduleName, accessRight, authenticationService);
            return urlAuthorized;
        }

        private static ISecurityService PerfomAccessCheck(HttpContextBase context)
        {
            var settings = SecurityConfig.GetCurrent();
            var current = AuthCookie.GetCurrent();
            ISecurityService authenticationService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ISecurityService>();

            var cookieNotFound = (current == null || current.SessionUid == null);
            var cookiedDeleted = false;

            AuthenticationDataDto dto = null;

            if (cookieNotFound)
            {
                if (current != null)
                {
                    current.Delete();
                    cookiedDeleted = true;
                }
            }
            else if (settings.Cookie.CookieOnlyCheck && current.AuthExpiry < Helper.GetLocalDate())
            {
                current.Delete();
                cookiedDeleted = true;
            }
            else if (!settings.Cookie.CookieOnlyCheck)
            {
                dto = Access.Renew(current.SessionUid, authenticationService);
                if (dto == null)
                {
                    current.Delete();
                    cookiedDeleted = true;
                }
            }

            if (!cookiedDeleted)
            {
                if (dto == null)
                    dto = new AuthenticationDataDto();

                Identity identity = new Identity(new Guid(current.SessionUid),
                                                settings.Cookie.CookieOnlyCheck ? current.Username : dto.Username,
                                                settings.Cookie.CookieOnlyCheck ? current.UserRoles : dto.Roles, dto.FullName,
                                                settings.Cookie.CookieOnlyCheck ? current.BranchCode : dto.BranchCode,
                                                settings.Cookie.CookieOnlyCheck ? current.AccountType : dto.AccountType);
                var principal = new Principal(identity, identity.Roles,identity.AccountType);
                context.User = principal;
                Thread.CurrentPrincipal = principal;

                if (settings.Cookie.SlidingExpiration && settings.Cookie.CookieOnlyCheck)
                {
                    current.AuthExpiry = Helper.GetLocalDate().AddMinutes(settings.Cookie.Timeout);
                    current.Save();
                }
            }

            var ip = context.Request.UserHostAddress;
            return authenticationService;
        }

        public static AuthenticationResponse Authentication(AuthenticationRequest request, bool finacleRoleCheck, SecurityConfig settings, ISecurityService securityService, IFinacleRepository finacleRepository)
        {
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();
            try
            {
                request.UserID = request.UserID.ToUpper();
                authenticationResponse = Access.AdCallForNameEmail(request);

                if (finacleRoleCheck)
                {
                    if (authenticationResponse.ResponseCode == "0")
                    {
                        var result = finacleRepository.GetUserRoleFromFinacle(request.UserID);

                        if (result != null)
                        {
                            authenticationResponse.BranchCode = result.BranchCode;
                            authenticationResponse.Role = result.ApplicationName;
                        }
                    }
                    if (authenticationResponse.ResponseCode == "0" && !string.IsNullOrEmpty(authenticationResponse.Role))
                    {
                        var result = finacleRepository.GetUserTillAccountFromFinacle(request.UserID);
                        authenticationResponse.TellerTillAccount = result;
                    }
                }
            }
            catch (Exception ex)
            {
                if (request != null)
                    request.Password = "XXXXXXXXXXXX";

                authenticationResponse.ResponseCode = "1001";
                authenticationResponse.ResponseDescription = "Unable to authenticate the user. Please contact the administrator.";
            }

            if (authenticationResponse.ResponseCode != "0")
            {
                var resp = new AuthenticationResponse() { ResponseCode = authenticationResponse.ResponseCode, ResponseDescription = authenticationResponse.ResponseDescription };
                authenticationResponse = resp;
            }
            return authenticationResponse;
        }


        public static bool IsUserInAD(string username)
        {
            var status = false;
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();
            try
            {
                ADService.Service Adservice = new ADService.Service();
                Adservice.Url = ConfigurationManager.AppSettings["ADServiceURL"];
                string response = Adservice.ADUserDetails(username);
                if (!string.IsNullOrEmpty(response))
                    status = true;
            }
            catch(Exception ex)
            {
                status = false;
            }
            return status;
        }

        public static AuthenticationResponse AdCallForNameEmail(AuthenticationRequest request)
        {
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();
            try
            {

                ADService.Service Adservice = new ADService.Service();
                Adservice.Url = ConfigurationManager.AppSettings["ADServiceURL"];

                if (Helper.IsTest())
                {
                    string response = Adservice.ADUserDetails(request.UserID);
                    if (!string.IsNullOrEmpty(response))
                    {
                        string[] ResponseArray = response.Split('|');
                        if (ResponseArray != null)
                        {
                            string[] list = ResponseArray[0].Split(" ".ToCharArray());
                            authenticationResponse.FirstName = list[0];
                            authenticationResponse.LastName = list.Length > 2 ? list[2] : list[1];
                            if (ResponseArray[1] != null)
                            {
                                authenticationResponse.Email = ResponseArray[1];
                            }
                            authenticationResponse.ResponseCode = "0";
                            authenticationResponse.ResponseDescription = "Success";
                        }
                        else
                        {
                            authenticationResponse.ResponseCode = "1001";
                            authenticationResponse.ResponseDescription = "User not authorized.";
                        }
                        return authenticationResponse;
                    }
                }
                else
                {
                    string response = Adservice.ADValidateUser(request.UserID, request.Password);
                    if (!string.IsNullOrEmpty(response))
                    {
                        string[] ResponseArray = response.Split('|');
                        if (ResponseArray != null)
                        {
                            if (ResponseArray[0] == "true")
                            {

                                string[] list = ResponseArray[1].Split(" ".ToCharArray());
                                authenticationResponse.FirstName = list[0];
                                authenticationResponse.LastName = list.Length > 2 ? list[2] : list[1];
                                string _response = Adservice.ADUserDetails(request.UserID);
                                if (_response != null)
                                {
                                    string[] _responseArray = _response.Split('|');
                                    if (_responseArray[1] != null)
                                    {
                                        authenticationResponse.Email = _responseArray[1];
                                    }
                                    authenticationResponse.ResponseCode = "0";
                                    authenticationResponse.ResponseDescription = "Success";
                                }
                            }
                            else
                            {
                                authenticationResponse.ResponseCode = "1001";
                                authenticationResponse.ResponseDescription = "User not authorized.";
                            }
                        }
                        return authenticationResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                if (request != null)
                    request.Password = "XXXXXXXXXXXX";

                authenticationResponse.ResponseCode = "1001";
                authenticationResponse.ResponseDescription = "Unable Authenticate the user. Please contact the administrator";
            }

            return authenticationResponse;
        }

        public static Authentication2FAResponse Auth2FAresponse(Authentication2FARequest request)
        {
            Authentication2FAResponse response = new Authentication2FAResponse();

            try
            {
                request.UserID = request.UserID.ToUpper();
                string message = string.Empty;
                bool authenticate = AuthenticateUser(request.UserID, request.TokenCode, ref message, "");
                if (authenticate)
                {
                    response.ResponseCode = "0";
                    response.ResponseDescription = "Success";
                }
                else
                {
                    response.ResponseCode = "1001";
                    response.ResponseDescription = "Failed - " + message;
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = "96";
                response.ResponseDescription = "Miscellaneous Error";
            }

            return response;
        }

        public static bool AuthenticateUser(string UserID, string PassCode, ref string message, string SerialNumber)
        {
            bool result = false;
            EntrustWrapper.AuthRequest authRequest = new EntrustWrapper.AuthRequest();
            EntrustWrapper.AuthResponse authResp = new EntrustWrapper.AuthResponse();
            try
            {
                authRequest.CustID = UserID.ToUpper();
                authRequest.PassCode = PassCode;

                EntrustWrapper.AuthWrapperClient client = new EntrustWrapper.AuthWrapperClient();
                authResp = client.AuthMethod(authRequest);

                if (authResp.Authenticated)
                {
                    result = true;
                }
                message = authResp.Message;

            }
            catch (Exception ex)
            {
               message = ex.Message;
            }

            return result;
        }

        public static string CheckAccessByCookie()
        {
            var current = AuthCookie.GetCurrent();
            var settings = SecurityConfig.GetCurrent();
            var cookieNotFound = (current == null || current.SessionUid == null);

            AuthenticationDataDto dto = null;
            if (cookieNotFound || (settings.Cookie.CookieOnlyCheck && current.AuthExpiry < Helper.GetLocalDate()))
            {
                return null;
            }
            else if (!settings.Cookie.CookieOnlyCheck)
            {
                dto = Access.Renew(current.SessionUid);
                if (dto == null)
                {
                    return null;
                }
                else
                {
                    return string.Format("{0}||{1}", dto.Username.Trim(), dto.Roles.Trim());
                }
            }
            else if (settings.Cookie.CookieOnlyCheck)
            {
                return string.Format("{0}||{1}", current.Username.Trim(), current.UserRoles.Trim());
            }
            return null;
        }

        internal static bool IsSessionActive(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return false;
            }
            var settings = SecurityConfig.GetCurrent();
            var current = AuthCookie.GetCurrent();
            ISecurityService authenticationService = ((IContainer)System.Web.HttpContext.Current.Application["container"]).Resolve<ISecurityService>();
            var userObject = authenticationService.GetUserBySessionId(sessionId);
            bool isActive = false;

            if (userObject != null)
            {
                if ((!userObject.CurrentSessionId.HasValue || sessionId != userObject.CurrentSessionId.ToString()) ||
                    (userObject.LastActivityDate.Value.AddMinutes(settings.Cookie.Timeout) > Helper.GetLocalDate()) ||
                    ((userObject.ApprovalStatus != Constants.ApprovalStatus.Approved) || userObject.IsLockedOut || userObject.IsDeleted))
                {
                    if (userObject.LastActivityDate.Value.AddMinutes(settings.Cookie.Timeout) > Helper.GetLocalDate())
                    {
                        isActive = true;
                    }
                }
            }

            return isActive;
        }
    }

}