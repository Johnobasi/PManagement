using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using PermissionManagement.Model;
using PermissionManagement.Utility;
using PermissionManagement.Services;
using PermissionManagement.Validation;
using System.Configuration;
using Newtonsoft.Json;

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

        
        [SecurityAccess()]
        public ActionResult Edit(string id)
        {
            if (Helper.GetLoggedInUserID().ToLower().Equals(id.ToLower()))
            {
                var userToEdit = _securityService.GetUser(id);
                userToEdit.RoleId = userToEdit.UserRole.RoleId;
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
            var updated = _securityService.EditUser(model, ref states);

            if (!states.IsValid)
            {
                model.UserRole = new Role() { RoleId = model.RoleId };
                ModelState.AddModelErrors(states);
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
                //return RedirectToAction("Edit", new { id = model.StaffID });
                return RedirectToAction("Edit", new { id = model.Username});
            }
        }

        [SecurityAccess()]
        public ActionResult ChangePassword()
        {
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
    }
}