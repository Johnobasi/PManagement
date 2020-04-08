using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PermissionManagement.Model;
using PermissionManagement.Utility;
using PermissionManagement.Services;
using PermissionManagement.Validation;
using System.Configuration;
using PermissionManagement.Repository;
using DataTables.Mvc;

namespace PermissionManagement.Web
{
    public class UserSetupController : BaseController
    {
        private ISecurityService _securityService;
        private ICacheService _cacheService;
        private IFinacleRepository _finacleRepository;
        public UserSetupController(ISecurityService securityService, ICacheService cacheService, IFinacleRepository finacleRepository)
        {
            _securityService = securityService;
            _finacleRepository = finacleRepository;
            _cacheService = cacheService;
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult Index()
        {
            var userList = new UserListingResponse();
            return View("ListUser", userList);
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult ListUser()
        {
            var userList = new UserListingResponse();
            return View(userList);
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public JsonResult ListUserData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var defaultSortBy = Constants.SortField.Username;
            var pagingParameter = MVCExtensionMethods.GetPagingParametersII(requestModel, defaultSortBy, Constants.SortOrder.Ascending.ToLower());

            UserListingResponse userList = _securityService.GetUserList(pagingParameter);
            var data = userList.UserListingResult;

            return Json(new DataTablesResponse((int)requestModel.Draw, data, userList.PagerResource.ResultCount, userList.PagerResource.ResultCount), JsonRequestBehavior.AllowGet);
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult ListRole()
        {
            var roleList = _securityService.GetRoleList();
            return View(roleList);
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Create)]
        public ActionResult CreateUser()
        {
            var userToCreate = new User() { UserRole = new Role() { RoleId = Guid.Empty } };
            userToCreate.CreationDate = Helper.GetLocalDate();
            userToCreate.LastLogInDate = Helper.GetLocalDate();
            userToCreate.LastActivityDate = Helper.GetLocalDate();

            return View(userToCreate);
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(User model)
        {
            ValidationStateDictionary states = new ValidationStateDictionary();

            model.UserRole = new Role() { RoleId = model.RoleId, RoleName = (from r in _securityService.GetRoleList() where r.RoleId == model.RoleId select r.RoleName).FirstOrDefault() };
            model.InitiatedBy = ControllerContext.RequestContext.HttpContext.User.Identity.Name;

            if (model.AccountType == Constants.AccountType.LocalFinacle)
            {
                var finacleRecord = _finacleRepository.GetUserRoleFromFinacle(model.Username);
                if (finacleRecord != null)
                {
                    model.BranchID = finacleRecord.BranchCode;
                }
            }
            if (model.AccountType == Constants.AccountType.ADLocal && !string.IsNullOrEmpty(model.Username))
            {
                if (!Access.IsUserInAD(model.Username))
                {
                    var errorMsg = "The user does not exist on AD or AD service could not be reached.";
                    Danger(errorMsg, true);
                    SetAuditInfo(errorMsg, string.Empty);
                    return View(model);
                }
            }
            _securityService.AddUser(model, ref states);

            if (!states.IsValid)
            {
                model.UserRole = new Role() { RoleId = model.RoleId };
                ModelState.AddModelErrors(states);
                
                var errorList = ValidationHelper.BuildModelErrorList(states);
                SetAuditInfo(Helper.StripHtml(errorList, true), string.Empty);

                return View(model);
            }
            else
            {
                Success(Constants.Messages.AddSuccessful, true);
                return RedirectToAction("EditUser", new { id = model.Username});
            }
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult EditUser(string id)
        {
            //var userToEdit = _securityService.GetUserByUsername(id);
            var userToEdit = _securityService.GetUser(id);

            if (userToEdit != null)
            {
                userToEdit.RoleId = userToEdit.UserRole.RoleId;
                //_cacheService.AddAndTieToSession(Helper.GetCacheKey(Constants.ModelType.User, id, Helper.GetLoggedInUserID()), userToEdit);
                return View(userToEdit);
            }
            else
            {
                Warning(string.Format("User with id {0} not found in the system", id), true);
                return RedirectToAction("ListUser");
            }
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, new string[] { Constants.AccessRights.Edit, Constants.AccessRights.Verify })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(User model)
        {
            ValidationStateDictionary states = new ValidationStateDictionary();

            model.UserRole = new Role() { RoleId = model.RoleId, RoleName = (from r in _securityService.GetRoleList() where r.RoleId == model.RoleId select r.RoleName).FirstOrDefault()  };
            var dbApprovalStatus = Helper.GetLastApprovalStatus(ControllerContext.RequestContext.HttpContext.Request);

            //the user that put a record in pending mode will always be stored as initiated by - meaning the db will be updated.
            var permitEdit = Access.CanEdit(Constants.Modules.UserSetup, model.InitiatedBy, dbApprovalStatus, model.IsDeleted);

            if (permitEdit)
            {
                var updated = _securityService.EditUser(model, ref states);

                if (!states.IsValid)
                {
                    model.UserRole = new Role() { RoleId = model.RoleId };
                    ModelState.AddModelErrors(states);

                    var errorList = ValidationHelper.BuildModelErrorList(states);
                    SetAuditInfo(Helper.StripHtml(errorList, true), string.Empty);

                    return View(model);
                }
                else
                {
                    if (updated == 0)
                    {
                        Warning(Constants.Messages.ConcurrencyError, true);
                    }
                    else
                    {
                        Success(Constants.Messages.SaveSuccessful, true);
                    }
                    
                    return RedirectToAction("EditUser", new { id = model.Username });
                }
            }
            else
            {
                Warning(Constants.Messages.EditNotPermittedError, true);
                return RedirectToAction("EditUser", new { id = model.Username });
            }
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Delete)]
        public ActionResult DeleteUser(string id, string s)
        {
            _securityService.DeleteUser(id, s);

            Success("Changes to the Delete record was successfully saved to the database.", true);
            
            return RedirectToAction("ListUser");
        }

        //[SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Create)]
        //public ActionResult CreateProfile()
        //{
        //    var list = _securityService.GetModuleAccessList().ToLst();
        //    var profileToCreate = new ProfileViewModel() { CurrentProfile = new Profile(), ModuleAccessList = list };
        //    return View(profileToCreate);
        //}

        //[SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Create)]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CreateProfile(ProfileViewModel model)
        //{
        //    ValidationStateDictionary states = new ValidationStateDictionary();

        //    _securityService.AddProfile(model, ref states);

        //    if (!states.IsValid)
        //    {
        //        ModelState.AddModelErrors(states);
        //        return View(model);
        //    }
        //    else
        //    {
        //        Success(Constants.Messages.AddSuccessful, true);
        //        return RedirectToAction("EditProfile", new { id = model.CurrentProfile.ProfileId });
        //    }
        //}

       // [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        //public ActionResult EditProfile(Guid id)
        //{
        //    var profileToEdit = _securityService.GetProfile(id);
        //    return View(profileToEdit);
        //}

        //[SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Edit)]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditProfile(ProfileViewModel model)
        //{
        //    ValidationStateDictionary states = new ValidationStateDictionary();

        //    var updated = _securityService.EditProfile(model, ref states);

        //    if (!states.IsValid)
        //    {
        //        ModelState.AddModelErrors(states);
        //        return View(model);
        //    }
        //    else
        //    {
        //        if (updated == 0)
        //        {
        //            Warning(Constants.Messages.ConcurrencyError, true);
        //        }
        //        else
        //        {
        //            Success(Constants.Messages.SaveSuccessful, true);
        //        }
        //        return RedirectToAction("EditProfile", new { id = model.CurrentProfile.ProfileId });
        //    }
        //}

        [AuditFilter(AuditLogLevel.LevelThree)]       
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Delete)]
        public ActionResult DeleteRole(Guid id)
        {
            
            ValidationStateDictionary states = new ValidationStateDictionary();
            _securityService.DeleteRole(id, ref states);

            if (!states.IsValid)
            {
                var errorList = ValidationHelper.BuildModelErrorList(states);
                Danger(string.Format("{0}<br />{1}", Constants.Messages.ErrorNotice, errorList), true);
                return RedirectToAction("ListRole");
            }
            else
            {
                Success(Constants.Messages.DeleteSuccessful, true);
                return RedirectToAction("ListRole");
            }          
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Create)]
        public ActionResult CreateRole()
        {
            var list = _securityService.GetModuleAccessList().ToList();
            var roleToCreate = new RoleViewModel() { CurrentRole = new Role(), ModuleAccessList = list };
            return View(roleToCreate);
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRole(RoleViewModel model)
        {           
            {
                ValidationStateDictionary states = new ValidationStateDictionary();

                _securityService.AddRole(model, ref states);

                if (!states.IsValid)
                {
                    var errorList = ValidationHelper.BuildModelErrorList(states);
                    Danger(string.Format("{0}<br />{1}", Constants.Messages.ErrorNotice, errorList), true);
                    ModelState.AddModelErrors(states);

                    SetAuditInfo(Helper.StripHtml(errorList, true), string.Empty);

                    return View(model);
                }
                else
                {
                    Success(Constants.Messages.AddSuccessful, true);
                    return RedirectToAction("EditRole", new { id = model.CurrentRole.RoleId });
                }
            }
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult EditRole(Guid id)
        {
            var roleToEdit = _securityService.GetRole(id);
            return View(roleToEdit);
        }
        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Edit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRole(RoleViewModel model)
        {
            {
                ValidationStateDictionary states = new ValidationStateDictionary();

                var updated = _securityService.EditRole(model, ref states);

                if (!states.IsValid)
                {
                    var errorList = ValidationHelper.BuildModelErrorList(states);
                    Danger(string.Format("{0}<br />{1}", Constants.Messages.ErrorNotice, errorList), true);
                    ModelState.AddModelErrors(states);

                    SetAuditInfo(Helper.StripHtml(errorList, true), string.Empty);

                    return View(model);
                }
                else
                {
                    if (updated == 0)
                    {
                        Warning(Constants.Messages.ConcurrencyError, true);
                    }
                    else
                    {
                        Success(Constants.Messages.SaveSuccessful, true);
                    }
                    return RedirectToAction("EditRole", new { id = model.CurrentRole.RoleId });
                }
            }
         }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Edit)]
        public ActionResult LockAccount(string id)
        {
            _securityService.LockUnlockAccount(id, true);
            return RedirectToAction("ListUser");

        }
        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Edit)]
        public ActionResult UnlockAccount(string id)
        {
            _securityService.LockUnlockAccount(id, false);
            return RedirectToAction("ListUser");
        }


        //#region role selection helpers

        //void SaveState(RoleViewModel model)
        //{
        //    //create comma delimited list of profile ids
        //    model.
        //   // model.SavedRequested = string.Join(",", model.SelectedProfiles.Select(p => p.ProfileId.ToString()).ToArray());
        //    var selectedIDs = (from m in model.SelectedProfiles select m.ProfileId).ToArray();

        //    model.AvailableProfiles = _securityService.GetProfileList().Where(p => !selectedIDs.Contains(p.ProfileId)).Select(g => g).ToList();
        //}

        //void RemoveProfiles(RoleViewModel model)
        //{
        //    if (model.RequestedSelected != null)
        //    {
        //        model.SelectedProfiles.RemoveAll(p => model.RequestedSelected.Contains(p.ProfileId));
        //        model.RequestedSelected = null;
        //    }
        //}

        //void AddProfiles(RoleViewModel model)
        //{
        //    if (model.AvailableSelected != null)
        //    {
        //        var prods = _securityService.GetProfileList().Where(p => model.AvailableSelected.Contains(p.ProfileId));
        //        model.SelectedProfiles.AddRange(prods);
        //        model.AvailableSelected = null;
        //    }
        //}

        //void RestoreSavedState(RoleViewModel model)
        //{
        //    model.SelectedProfiles = new List<Profile>();

        //    //get the previously stored items
        //    if (!string.IsNullOrEmpty(model.SavedRequested))
        //    {
        //        string[] prodids = model.SavedRequested.Split(',');
        //        var prods = _securityService.GetProfileList().Where(p => prodids.Contains(p.ProfileId.ToString()));
        //        model.SelectedProfiles.AddRange(prods);
        //    }
        //}
        //#endregion profile selection helpers
    }

}