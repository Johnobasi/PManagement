using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Repository.Interface;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System;
using System.Configuration;
using System.Net.Http;
using System.Web.Mvc;


namespace PermissionManagement.Web
{
    public class RemitlyPayoutController : BaseController
    {
        #region Properties and Variables
        private ISecurityService _securityService;
        private ICacheService _cacheService;
        private IFinacleRepository _flexCubeRepository;
        private ICashPickup _cashPickupService;
        #endregion

        public RemitlyPayoutController(ISecurityService securityService, ICacheService cacheService, IFinacleRepository finacleRepository, ICashPickup cashPickupService)
        {
            _securityService = securityService;
            _flexCubeRepository = finacleRepository;
            _cacheService = cacheService;
            _cashPickupService = cashPickupService;
        }


        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.RemitlyPayout, Constants.AccessRights.View)]
        public ActionResult Index()
        {
            //List cash pickup for the branch...
            var cashPickup = _cashPickupService.ListCashPickup();

            return View(cashPickup);
        }

        [HttpGet]
        public ActionResult RetrieveReference()
        {
            return View();
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.RemitlyPayout, Constants.AccessRights.View)]
        public ActionResult RetrieveReference(string referenceNumber)
        {
            //form display input for branch teller to enter reference number presented by customer
            if (referenceNumber == null)
            {
                throw new Exception("Cannot get details for requested reference Number");
            }

            //call RemmitlyAPI to get the details
            var refRetrieve = _cashPickupService.RetrieveReference(referenceNumber);
            return View(refRetrieve);

            //if successful, write to db and redirect page to display the details

            //if not display the appropriate error

            //return View();
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.RemitlyPayout, Constants.AccessRights.View)]
        public ActionResult EditRemitlyCashPayout(string referenceNumber)
        {
            //the form to display the details of retrieved reference number (its a get)
            var EditedCashPayOut = _cashPickupService.EditRemittance(referenceNumber);
            return View(EditedCashPayOut);
        }

        [AuditFilter(AuditLogLevel.LevelOne)]
        [SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Create, Constants.AccessRights.Edit })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRemitlyCashPayout(RemittanceCashPickup model)
        {
            //the form to to process thre retrieved reference number
            using (var client = new HttpClient())
            {
                string url = ConfigurationManager.AppSettings["RemitlyBaseURL"];

                var content = new MultipartFormDataContent();
                client.BaseAddress = new Uri(url);
                var responseTask = client.PostAsync($"/IMTOAPI.Project/Remitly/UpdateTransfer/", content);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Cash Pickup was successfully updated.";

                    return RedirectToAction("Index", new { CashPickupIdId = model.Id });
                }

                if ((int)result.StatusCode == 422)
                {
                    ModelState.AddModelError("", "CashPickup entry Already Exists!");
                }
                else
                {
                    ModelState.AddModelError("", "Some kind of error. CashPickup not updated!");
                }
            }

            var cashPicks = _cashPickupService.EditRemittance(model.ReferenceNumber);
            return View(cashPicks);
            //here teller accept and record customer identity.
            //system validate and send for approval

            //return View();
        }

        [AuditFilter(AuditLogLevel.LevelOne)]
        [SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        public ActionResult ApproveRemitlyCashPayout(string referenceNumber)
        {
            //the form for approval personel to view and take action on approval or reject or reject for correction
            var CashPickupToApproved = _cashPickupService.RetrieveReference(referenceNumber);
            if (referenceNumber == null)
            {
                return HttpNotFound();
            }
            return View(CashPickupToApproved);
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "RejectForCorrectionRemitlyCashPayout")]
        public ActionResult RejectForCorrectionRemitlyCashPayout(RemittanceCashPickup model)
        {
            return ExecuteAction(model, Constants.ApprovalStatus.RejectedForCorrection);
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "RejectRemitlyCashPayout")]
        public ActionResult RejectRemitlyCashPayout(RemittanceCashPickup model)
        {
            return ExecuteAction(model, Constants.ApprovalStatus.Rejected);
        }

        [AuditFilter()]
        [SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "ApproveRemitlyCashPayout")]
        public ActionResult ApproveRemitlyCashPayout(RemittanceCashPickup model)
        {
            return ExecuteAction(model, Constants.ApprovalStatus.Approved);
        }

        private ActionResult ExecuteAction(RemittanceCashPickup model, string approvalStatus)
        {
            ValidationStateDictionary states = new ValidationStateDictionary();

            var models = _cashPickupService.RetrieveReference(model.ReferenceNumber);
            var dbApprovalStatus = Constants.ApprovalStatus.Pending;

            //the user that put a record in pending mode will always be stored as initiated by - meaning the db will be updated.
            var permitEdit = Access.CanEdit(Constants.Modules.RemitlyPayout, model.InitiatedBy, dbApprovalStatus, model.IsDeleted);
            if (permitEdit)
            {
                model.ApprovedStatus = approvalStatus;

                var updated = _cashPickupService.EditRemittance(model.ReferenceNumber);
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
                    if (updated == null) { Warning(Constants.Messages.ConcurrencyError, true); }
                    else { Success(Constants.Messages.SaveSuccessful, true); }
                    return RedirectToAction("Index");
                }
            }
            else
            {
                Warning(Constants.Messages.EditNotPermittedError, true);
                return RedirectToAction("Index");
            }
        }
    }
}
