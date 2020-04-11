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
    public class RemitlyPayoutController : BaseController
    {
        #region Properties and Variables
        private ISecurityService _securityService;
        private ICacheService _cacheService;
        private IFinacleRepository _flexCubeRepository;
        #endregion

        public RemitlyPayoutController(ISecurityService securityService, ICacheService cacheService, IFinacleRepository finacleRepository)
        {
            _securityService = securityService;
            _flexCubeRepository = finacleRepository;
            _cacheService = cacheService;
        }


        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.RemitlyPayout, Constants.AccessRights.View)]
        public ActionResult Index()
        {
            //List cash pickup for the branch...
            return View();
        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.RemitlyPayout, Constants.AccessRights.View)]

        public ActionResult RetrieveReference()
        {
            //form display input for branch teller to enter reference number presented by customer

            //call RemmitlyAPI to get the details

            //if successful, write to db and redirect page to display the details

            //if not display the appropriate error

            return View();

        }

        [AuditFilter(AuditLogLevel.LevelThree)]
        [SecurityAccess(Constants.Modules.RemitlyPayout, Constants.AccessRights.View)]
        public ActionResult EditRemitlyCashPayout(string referenceNumber)
        {
            //the form to display the details of retrieved ference number (its a get)

            return View();
        }

        //[AuditFilter(AuditLogLevel.LevelOne)]
        //[SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Create, Constants.AccessRights.Edit })]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditRemitlyCashPayout(RemittanceCashPickup model)
        //{
        //    //the form to to process thre retrieved reference number
        //    //here teller accept and record customer identity.
        //    //system validate and send for approval

        //    return View();
        //}

        //[AuditFilter(AuditLogLevel.LevelOne)]
        //[SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        //public ActionResult ApproveRemitlyCashPayout(string referenceNumber)
        //{
        //    //the form for approval personel to view and take action on approval or reject or reject for correction

        //    return View();
        //}

        //[AuditFilter()]
        //[SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[MultipleButton(Name = "action", Argument = "RejectForCorrectionRemitlyCashPayout")]
        //public ActionResult RejectForCorrectionRemitlyCashPayout(RemittanceCashPickup model)
        //{
        //    return ExecuteAction(model, Constants.ApprovalStatus.RejectedForCorrection);
        //}

        //[AuditFilter()]
        //[SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[MultipleButton(Name = "action", Argument = "RejectRemitlyCashPayout")]
        //public ActionResult RejectRemitlyCashPayout(RemittanceCashPickup model)
        //{
        //    return ExecuteAction(model, Constants.ApprovalStatus.Rejected);
        //}

        //[AuditFilter()]
        //[SecurityAccess(Constants.Modules.RemitlyPayout, new string[] { Constants.AccessRights.Verify, Constants.AccessRights.MakeOrCheck })]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[MultipleButton(Name = "action", Argument = "ApproveRemitlyCashPayout")]
        //public ActionResult ApproveRemitlyCashPayout(RemittanceCashPickup model)
        //{
        //    return ExecuteAction(model, Constants.ApprovalStatus.Approved);
        //}

        //private ActionResult ExecuteAction(RemittanceCashPickup model, string approvalStatus)
        //{
        //    ValidationStateDictionary states = new ValidationStateDictionary();

        //    model = _remittanceService.GetRemittance("REMITLY", model.ReferenNumber);
        //    var dbApprovalStatus = Constants.ApprovalStatus.Pending;

        //    //the user that put a record in pending mode will always be stored as initiated by - meaning the db will be updated.
        //    var permitEdit = Access.CanEdit(Constants.Modules.RemitlyPayout, model.InitiatedBy, dbApprovalStatus, model.IsDeleted);
        //    if (permitEdit)
        //    {
        //        model.ApprovalStatus = approvalStatus;

        //        var updated = _remittanceService.EditRemittance(model, ref states);
        //        if (!states.IsValid)
        //        {
        //            model.UserRole = new Role() { RoleId = model.RoleId };
        //            ModelState.AddModelErrors(states);
        //            var errorList = ValidationHelper.BuildModelErrorList(states);
        //            SetAuditInfo(Helper.StripHtml(errorList, true), string.Empty);
        //            return View(model);
        //        }
        //        else
        //        {
        //            if (updated == 0) { Warning(Constants.Messages.ConcurrencyError, true); }
        //            else { Success(Constants.Messages.SaveSuccessful, true); }
        //            return RedirectToAction("ListRemittance");
        //        }
        //    }
        //    else
        //    {
        //        Warning(Constants.Messages.EditNotPermittedError, true);
        //        return RedirectToAction("ListRemittance");
        //    }
        //}
    }
}
