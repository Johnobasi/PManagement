using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Validation;

namespace PermissionManagement.Services
{
    public class RoleValidator : IValidator<RoleViewModel>
    {
        public ValidationState Validate(RoleViewModel roleToValidate, object otherData)
        {
            ValidationState v = new ValidationState();

            if (string.IsNullOrEmpty(roleToValidate.CurrentRole.RoleName))
            {
                v.Errors.Add(new ValidationError("CurrentRole.RoleName.NameRequiredError", roleToValidate.CurrentRole.RoleName, "Role name is not set"));
            }
            else if (roleToValidate.CurrentRole.RoleName.Length > 45)
            {
                v.Errors.Add(new ValidationError("CurrentRole.RoleName.NameTooLongError", roleToValidate.CurrentRole.RoleName, string.Format("Role name too long, maximum length is {0}", 45)));
            }
            if (!string.IsNullOrEmpty(roleToValidate.CurrentRole.Description))
            {
                if (roleToValidate.CurrentRole.Description.Length > 256)
                {
                    v.Errors.Add(new ValidationError("CurrentRole.Description.DescriptionTooLongError", roleToValidate.CurrentRole.Description, string.Format("Role description too long, maximum length is {0}", 256)));
                }
            }

            var validationRepository = otherData as IDatastoreValidationRepository;

            if (validationRepository != null)
            {
                if (validationRepository.IsRoleNameExist(roleToValidate.CurrentRole.RoleId, roleToValidate.CurrentRole.RoleName))
                {
                    v.Errors.Add(new ValidationError("CurrentProfile.ProfileName.NameAlreadyExistError", roleToValidate.CurrentRole.RoleName, "Role name already exist"));
                }
            }

            return v;
        }
    }
}

//if (roleToValidate.ModuleAccessList != null)
//{
//    bool verifierSet = false;
//    foreach(var m in roleToValidate.ModuleAccessList)
//    {
//        if ((m.CreateAccess == true || m.DeleteAccess == true || m.EditAccess == true) && m.VerifyAccess == true) 
//        {
//            verifierSet = true;
//        }
//    }

//    var verifier = (from moduleAccess in roleToValidate.ModuleAccessList
//                     select moduleAccess).Where(j => (j.VerifyAccess == true) && (j.CreateAccess == true
//                         || j.DeleteAccess == true || j.EditAccess == true)).
//                         Select(h => h).FirstOrDefault();

//    //if (verifierSet && initiatorSet)
//    if (verifier != null)
//    {
//        v.Errors.Add(new ValidationError("CurrentProfile.ProfileName.InCompatibleModuleSelectiion", roleToValidate.CurrentRole.RoleName, "A user cannot have a combination of Verifier and (Create/Edit/Delete)."));
//    }
//}