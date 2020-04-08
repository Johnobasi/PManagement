using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Validation;
using PermissionManagement.Utility;

namespace PermissionManagement.Services
{
    //    public class RoleProfileModelValidator : IValidator<RoleProfileModel>
    //{
    //    public ValidationState Validate(RoleProfileModel roleToValidate, object otherData)
    //    {
    //        ValidationState v = new ValidationState();

    //        if (string.IsNullOrEmpty(roleToValidate.CurrentRole.RoleName))
    //        {
    //            v.Errors.Add(new ValidationError("CurrentRole.RoleName.NameRequiredError", roleToValidate.CurrentRole.RoleName, "Role name is not set"));
    //        }
    //        else if (roleToValidate.CurrentRole.RoleName.Length > 64)
    //        {
    //            v.Errors.Add(new ValidationError("CurrentRole.RoleName.NameTooLongError", roleToValidate.CurrentRole.RoleName, string.Format("Role name too long, maximum length is {0}", 64)));
    //        }
    //        if (!string.IsNullOrEmpty(roleToValidate.CurrentRole.Description))
    //        {
    //            if (roleToValidate.CurrentRole.Description.Length > 256)
    //            {
    //                v.Errors.Add(new ValidationError("CurrentRole.Description.DescriptionTooLongError", roleToValidate.CurrentRole.Description, string.Format("Role description too long, maximum length is {0}", 256)));
    //            }
    //        }

    //        var validationRepository = otherData as IDatastoreValidationRepository;

    //        if (validationRepository != null)
    //        {
    //            if (validationRepository.IsRoleNameExist(roleToValidate.CurrentRole.RoleId, roleToValidate.CurrentRole.RoleName))
    //            {
    //                v.Errors.Add(new ValidationError("CurrentRole.RoleName.NameAlreadyExistError", roleToValidate.CurrentRole.RoleName, "Role name already dbVersion"));
    //            }
    //        }

    //        if (roleToValidate.SelectedProfiles == null || roleToValidate.SelectedProfiles.Count == 0)
    //        {
    //            v.Errors.Add(new ValidationError("RequestedSelected.NameAlreadyExistError", roleToValidate.CurrentRole.RoleName, "A minimum of one profile must be selected"));
    //        }

    //        return v;
    //    }
    //}
}
