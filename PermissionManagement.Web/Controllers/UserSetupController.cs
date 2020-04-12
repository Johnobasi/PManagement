using DataTables.Mvc;
using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PermissionManagement.Web
{
    public class UserSetupController : BaseController
    {
        #region Properties and Variables
        private ISecurityService _securityService;
        private ICacheService _cacheService;
        private IFinacleRepository _flexCubeRepository;
        #endregion

        public UserSetupController(ISecurityService securityService, ICacheService cacheService, IFinacleRepository finacleRepository)
        {
            _securityService = securityService;
            _flexCubeRepository = finacleRepository;
            _cacheService = cacheService;
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult Index()
        {
            return RedirectToAction("ListUser");
            //var userList = new UserListingResponse();
            //return View("ListUser", userList);
        }

        #region Roles Action Results

        #region Create Role
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

        #endregion

        #region Edit Roles
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
                    if (updated == 0) { Warning(Constants.Messages.ConcurrencyError, true); }
                    else { Success(Constants.Messages.SaveSuccessful, true); }
                    return RedirectToAction("EditRole", new { id = model.CurrentRole.RoleId });
                }
            }
        }
        #endregion

        #region Other Action Results
        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.View)]
        public ActionResult ListRole()
        {
            var roleList = _securityService.GetRoleList();
            return View(roleList);
        }
        [SecurityAccess]
        [CompressFilter]
        [HttpPost]
        public void ExportExcel(string searchKey)
        {
            IEnumerable<Role> roleList = null;
            roleList = string.IsNullOrEmpty(searchKey) ? _securityService.GetRoleList() : _securityService.GetRoleList().Where(c => c.RoleName.ToString() == searchKey || c.Description == searchKey);
            //IEnumerable<ExpiredUserListingDto> excelData = _reportService.GetExcelReportForUsers(uiInputSearchParam, fromDate, toDate, reportType);
            new Export().ToFile(roleList, Response, "RolesList_Report");
        }
        //ExportUsersToExcel

        [SecurityAccess]
        [CompressFilter]
        [HttpPost]
        public void ExportUsersToExcel(FormCollection formCollection)
        {
            List<ExportDto> usersList = null;
            usersList = _securityService.GetUserListForExcel(formCollection["searchKey"]);
            new Export().ToFile(usersList, Response, "UsersList_Report");
        }

        [SecurityAccess]
        [CompressFilter]
        [HttpPost]
        public void ExportUsersToWord(FormCollection formCollection)
        {
            List<ExportDto> usersList = null;
            usersList = _securityService.GetUserListForExcel(formCollection["searchKey"]);
            new Export().ToFile(usersList, Response, "UsersList_Report",EXPORTTYPE.WORD);
        }

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

        #endregion

        #endregion

        #region User Action Results

        #region Create User
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
            if (model.AccountType == Constants.AccountType.LocalFinacle || model.AccountType == Constants.AccountType.ADLocal)
            {
                var flexcubeRecord = _flexCubeRepository.GetUserRoleFromFlexcube(model.Username);
                if (flexcubeRecord != null) { model.BranchID = flexcubeRecord.BranchCode; }
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
                return RedirectToAction("EditUser", new { id = model.Username });
            }
        }

        #endregion

        #region Edit User
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
        [SecurityAccess(Constants.Modules.UserSetup, new string[] { Constants.AccessRights.Edit })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(User model)
        {
            ValidationStateDictionary states = new ValidationStateDictionary();
            model.UserRole = new Role() { RoleId = model.RoleId, RoleName = (from r in _securityService.GetRoleList() where r.RoleId == model.RoleId select r.RoleName).FirstOrDefault() };
            model.InitiatedBy = ControllerContext.RequestContext.HttpContext.User.Identity.Name;

            if (Access.IsFormEditable(Constants.Modules.UserSetup, model.ApprovalStatus))
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
                    if (updated == 0) { Warning(Constants.Messages.ConcurrencyError, true); }
                    else { Success(Constants.Messages.SaveSuccessful, true); }
                    return RedirectToAction("EditUser", new { id = model.Username });
                }
            }
            else
            {
                Warning(Constants.Messages.EditNotPermittedError, true);
                return RedirectToAction("EditUser", new { id = model.Username });
            }
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.UserSetup, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        public ActionResult ApproveUser(string id)
        {
            var userToEdit = _securityService.GetUser(id);
            ViewBag.ModelId = userToEdit.Username;

            if (userToEdit != null && userToEdit.ApprovalStatus == Constants.ApprovalStatus.Pending)
            {
                userToEdit.RoleId = userToEdit.UserRole.RoleId;             
                return View(userToEdit);
            }
            else
            {
                if (userToEdit != null && userToEdit.ApprovalStatus != Constants.ApprovalStatus.Pending)
                {
                    Warning(string.Format("Record {0} not in pending approval state", id), true);
                }
                else
                {
                    Warning(string.Format("User with id {0} not found in the system", id), true);
                }
                return RedirectToAction("ListUser");
            }
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "RejectForCorrectionUser")]
        public ActionResult RejectForCorrectionUser(User model)
        {
            return ExecuteAction(model, Constants.ApprovalStatus.RejectedForCorrection);
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "RejectUser")]
        public ActionResult RejectUser(User model)
        {
            return ExecuteAction(model, Constants.ApprovalStatus.Rejected);
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "ApproveUser")]
        public ActionResult ApproveUser(User model)
        {
            return ExecuteAction(model, Constants.ApprovalStatus.Approved); 
        }

        private ActionResult ExecuteAction(User model, string approvalStatus)
        {
            ValidationStateDictionary states = new ValidationStateDictionary();

            model = _securityService.GetUser(model.Username);
            var dbApprovalStatus = Constants.ApprovalStatus.Pending;

            //the user that put a record in pending mode will always be stored as initiated by - meaning the db will be updated.
            var permitEdit = Access.InApprovableState(Constants.Modules.UserSetup, model.InitiatedBy, dbApprovalStatus, model.IsDeleted);
            if (permitEdit)
            {
                model.ApprovalStatus = approvalStatus;

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
                    if (updated == 0) { Warning(Constants.Messages.ConcurrencyError, true); }
                    else { Success(Constants.Messages.SaveSuccessful, true); }
                    return RedirectToAction("ListUser");
                }
            }
            else
            {
                Warning(Constants.Messages.EditNotPermittedError, true);
                return RedirectToAction("ListUser");
            }
        }

        #endregion

        #region Other Action Results

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

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.UserSetup, Constants.AccessRights.Delete)]
        public ActionResult DeleteUser(string id, string s)
        {
            _securityService.DeleteUser(id, s);
            Success("Changes to the Delete record was successfully saved to the database.", true);
            return RedirectToAction("ListUser");
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

        public JsonResult GetUserInfo(string id)
        {
            var userToView = _securityService.GetUser(id);
            JsonResult result = new JsonResult() { Data = userToView, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return result;
        }
        #endregion

        #endregion
    }
}