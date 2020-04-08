using Newtonsoft.Json;
using PermissionManagement.Model;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System;
using System.Web.Mvc;

namespace PermissionManagement.Web
{
    public class MyProfileController : BaseController
    {
        private ISecurityService _securityService;

        public MyProfileController(ISecurityService securityService)
        {
            _securityService = securityService;
        }
        // GET: MyProfile

        [SecurityAccess()]
        public ActionResult Index()
        {
            var user = _securityService.GetUser(ControllerContext.RequestContext.HttpContext.User.Identity.Name);
            var url = Helper.GetRootURL() + "/MyProfile/Edit/" + user.Username.ToString();
            return new RedirectResult(url);
        }

        #region Edit Action Results
        [SecurityAccess()]
        public ActionResult Edit(string id)
        {
            if (Helper.GetLoggedInUserID().ToLower().Equals(id.ToLower()))
            {
                var userToEdit = _securityService.GetUser(id);
                userToEdit.RoleId = userToEdit.UserRole.RoleId;
                if (userToEdit.AccountType != Constants.AccountType.LocalLocal && userToEdit.AccountType != Constants.AccountType.LocalFinacle)
                {
                    this.Information(string.Format("{0}<br />{1}", Constants.Messages.EditNotPermittedError, "Record can only be edited on source system"), true);
                }
                return View(userToEdit);
            }
            else
            {
                throw new Exception("User not found.");
                return null;
            }
        }

        [AuditFilter()]
        [SecurityAccess()]
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Edit(User model)
        {
            ValidationStateDictionary states = new ValidationStateDictionary();
            model.UserRole = new Role() { RoleId = model.RoleId };

            //model.AccountType = Crypto.Decrypt(model.AccountType);
            if (model.AccountType != Constants.AccountType.LocalLocal && model.AccountType != Constants.AccountType.LocalFinacle)
            {
                this.Danger(string.Format("{0}<br />{1}", Constants.Messages.EditNotPermittedError, "Record can only be edited on source system"), true);
                model.UserRole = new Role() { RoleId = model.RoleId };
                return View(model);
            }

            var updated = _securityService.EditUser(model, ref states);
            if (!states.IsValid)
            {
                model.UserRole = new Role() { RoleId = model.RoleId };
                ModelState.AddModelErrors(states);
                return View(model);
            }
            else
            {
                if (updated == 0) { Warning(Constants.Messages.ConcurrencyError, true); }
                else { Success(Constants.Messages.SaveSuccessful, true); }
                //return RedirectToAction("Edit", new { id = model.StaffID });
                return RedirectToAction("Edit", new { id = model.Username });
            }
        }
        #endregion

        #region ChangePassword Action Results

        [SecurityAccess()]
        public ActionResult ChangePassword()
        {
            var userToEdit = _securityService.GetUser(Helper.GetLoggedInUserID());
            ViewBag.IsFirstLogIn = userToEdit.IsFirstLogIn != null ? userToEdit.IsFirstLogIn : false;

            if (userToEdit.AccountType != Constants.AccountType.LocalLocal && userToEdit.AccountType != Constants.AccountType.LocalFinacle)
            {
                this.Information(string.Format("{0}<br />{1}", Constants.Messages.EditNotPermittedError, "Password can only be changed on source system"), true);
                var url = Helper.GetRootURL() + "/admin";
                return new RedirectResult(url);
            }
            return View();
        }

        [AuditFilter()]
        [SecurityAccess()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordDto model)
        {
            ValidationStateDictionary states = new ValidationStateDictionary();
            var username = ((Identity)ControllerContext.HttpContext.User.Identity).Name;

            var userToEdit = _securityService.GetUser(username);
            ViewBag.IsFirstLogIn = userToEdit.IsFirstLogIn;

            if (userToEdit.AccountType != Constants.AccountType.LocalLocal && userToEdit.AccountType != Constants.AccountType.LocalFinacle)
            {
                this.Danger(string.Format("{0}<br />{1}", Constants.Messages.EditNotPermittedError, "Password can only be changed on source system"), true);
                return View(userToEdit);
            }

            _securityService.UserChangePassword(username, model.OldPassword, model.NewPassword, model.ConfirmPassword, ref states);

            if (!states.IsValid)
            {
                var errorList = ValidationHelper.BuildModelErrorList(states);
                Danger(string.Format("{0}<br />{1}", Constants.Messages.ErrorNotice, errorList), true);
                ModelState.AddModelErrors(states);
                SetAuditInfo(Helper.StripHtml(errorList, true), JsonConvert.SerializeObject(model), username);
                return View(model);
            }
            else
            {
                Success("<b>Password change</b> was successfully saved to the database.", true);
                SetAuditInfo("Successful", JsonConvert.SerializeObject(model), username);
                return View("ChangePasswordSuccess");
            }
        }
        #endregion
    }
}