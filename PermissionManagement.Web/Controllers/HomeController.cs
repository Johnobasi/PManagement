using Newtonsoft.Json;
using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System;
using System.Configuration;
using System.Globalization;
using System.Web.Mvc;

namespace PermissionManagement.Web
{
    public class HomeController : BaseController
    {
        #region Properties and Variables
        private ISecurityService _securityService;
        private IFinacleRepository _finacleRepository;
        private IAuditService _auditService;
        private IAuditRepository _auditRepository;
        private ISecurityRepository _securityRepository; 
        #endregion
        public HomeController(ISecurityService securityService, IFinacleRepository finacleRepository, IAuditService auditService, IAuditRepository auditRepository, ISecurityRepository securityRepository)
        {
            _securityService = securityService;
            _finacleRepository = finacleRepository;
            _auditService = auditService;
            _auditRepository = auditRepository;
            _securityRepository = securityRepository;
        }
        public ActionResult Index()
        {
            var url = string.Empty;
            if (Helper.IsAuthenticated())
            {
                var roleName = ((Identity)ControllerContext.HttpContext.User.Identity).Roles;
                url = Helper.GetRootURL() + "/admin";
            }
            else { url = Helper.GetRootURL() + "/Login"; }
            return new RedirectResult(url);
        }

        #region Login Action Results
        public ActionResult Login()
        {
            var m = new LogInDto();
            var returnUrl = Request.QueryString["ReturnUrl"] as string;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                m.Message = GetUrlTargetMessage(returnUrl);
                m.ReturnUrl = returnUrl;
            }
            return View(m);
        }

        [AuditFilter()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogIn(LogInDto model)
        {
            bool IsSuccess = false;
            var url = string.Empty;
            var errorList = string.Empty;

            if (ModelState.IsValid)
            {
                ValidationStateDictionary states = new ValidationStateDictionary();
                bool isFirstLogIn = false;
                bool status = Access.SignIn(model.Username, model.Password, model.TokenCode, _securityService, _finacleRepository, out isFirstLogIn, ref states);

                if ((!states.IsValid))
                {
                    IsSuccess = false;
                    errorList = ValidationHelper.BuildModelErrorList(states);
                    Danger(string.Format("{0}<br />{1}", Constants.Messages.ErrorNotice, errorList), true);
                    ModelState.AddModelErrors(states);
                }
                else
                {
                    if (!String.IsNullOrEmpty(model.ReturnUrl))
                    {
                        url = model.ReturnUrl;
                        IsSuccess = true;
                    }
                    else
                    {
                        if (!isFirstLogIn)
                        {
                            var roleName = ((Identity)ControllerContext.HttpContext.User.Identity).Roles;
                            url = Helper.GetRootURL() + "/admin";
                            IsSuccess = true;
                        }
                        else
                        {
                            url = Helper.GetRootURL() + "/MyProfile/changepassword";
                            IsSuccess = true;
                        }
                    }
                }
            }

            model.Password = "XXXXXXXXXXXX";
            SetAuditInfo(IsSuccess ? "Successful" : Helper.StripHtml(errorList, true),
                JsonConvert.SerializeObject(model),
                string.IsNullOrEmpty(model.Username) ? "not-supplied" : model.Username);

            if (IsSuccess) { return new RedirectResult(url); }
            else
            {
                // If we got this far, something failed, redisplay form
                return View(model);
            }
        } 
        #endregion

        [SecurityAccess()]
        [AuditFilter()]
        public ActionResult LogOut()
        {
            Access.SignOut(_securityService);

            if (!string.IsNullOrEmpty((Request.QueryString["xxkeyxx"] as string)))
            { Danger(string.Format("You have been logged out due to inactivity for {0} minutes. Please log in again.", SecurityConfig.GetCurrent().Cookie.Timeout), true); }
            //var url = string.Format("{0}/{1}", Helper.GetRootURL(), SecurityConfig.GetCurrent().Login.Page);
            return RedirectToAction(SecurityConfig.GetCurrent().Login.Page);
        }

        #region ResetPassword Action Results
        [AuditFilter()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordDto model)
        {
            ValidationStateDictionary states = new ValidationStateDictionary();
            _securityService.ResetPassword(model.Username, model.Email, ref states);
            if (!states.IsValid)
            {
                var errorList = ValidationHelper.BuildModelErrorList(states);
                Danger(string.Format("{0}<br />{1}", Constants.Messages.ErrorNotice, errorList), true);
                ModelState.AddModelErrors(states);
                return View(model);
            }
            else { return View("ResetPasswordMessage"); }
        }

        public ActionResult ResetPassword() { return View(new ResetPasswordDto()); } 
        #endregion

        [SecurityAccess()]
        public JsonResult LastLogin(string id)
        {
            var sol_id = _securityRepository.GetUser(Helper.GetLoggedInUserID());
            string lastLogin = _auditRepository.GetLastLogin(id);
            string dt = string.IsNullOrEmpty(lastLogin) ? DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") : DateTime.Parse(lastLogin, CultureInfo.CreateSpecificCulture("en-US")).ToString("dd-MM-yyyy hh:mm:ss");
            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Error(string errorCode)
        {
            ViewBag.ErrorCode = errorCode;
            return View();
        }

        public ActionResult PageNotFound() { return View(); }

        private string GetUrlTargetMessage(string returnUrl)
        {
            returnUrl = returnUrl.Replace(Helper.GetRootURL(), string.Empty);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VirtualDirectory"]))
            { returnUrl = returnUrl.Replace(ConfigurationManager.AppSettings["VirtualDirectory"], string.Empty).ToLower(); }
            return string.Empty;
        }
    }
}