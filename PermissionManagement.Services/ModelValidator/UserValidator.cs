using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Utility;
using PermissionManagement.Validation;
using System;

namespace PermissionManagement.Services
{
    public class UserValidator : IValidator<User>
    {
        public ValidationState Validate(User userToValidate, object otherData)
        {
            ValidationState v = new ValidationState();


            if (string.IsNullOrEmpty(userToValidate.Username))
            {
                v.Errors.Add(new ValidationError("Username.RequiredError", userToValidate.Username, "Username is not set"));
            }
            else
            {
                if (userToValidate.Username.Length > 32)
                {
                    v.Errors.Add(new ValidationError("Username.MaxLengthExceededError", userToValidate.Username, string.Format("Username must be less than or equal to {0} characters long.", 32)));
                }
                else if (userToValidate.Username.Length < 3)
                {
                    v.Errors.Add(new ValidationError("Username.MinLengthExceededError", userToValidate.Username, string.Format("Username must not be less than {0} characters long.", 3)));
                }
            }
            if (string.IsNullOrEmpty(userToValidate.FirstName))
            {
                v.Errors.Add(new ValidationError("FirstName.RequiredError", userToValidate.FirstName, "FirstName is not set"));
            }
            else if (userToValidate.FirstName.Length > 32)
            {
                v.Errors.Add(new ValidationError("FirstName.MaxLengthExceededError", userToValidate.FirstName, string.Format("FirstName must be less than or equal to {0} characters long.", 32)));
            }
            if (string.IsNullOrEmpty(userToValidate.LastName))
            {
                v.Errors.Add(new ValidationError("LastName.RequiredError", userToValidate.LastName, "LastName is not set"));
            }
            else if (userToValidate.LastName.Length > 32)
            {
                v.Errors.Add(new ValidationError("LastName.MaxLengthExceededError", userToValidate.LastName, string.Format("LastName must be less than or equal to {0} characters long.", 32)));
            }

            if (string.IsNullOrEmpty(userToValidate.Email) && !Helper.IsAcceptNoEmail())
            {
                v.Errors.Add(new ValidationError("Email.Required", userToValidate.Email, "Email must be specified"));
            }
            else
            {
                if (!string.IsNullOrEmpty(userToValidate.Email))
                {
                    if (userToValidate.Email.Length > 64)
                    {
                        v.Errors.Add(new ValidationError("Email.MaxLengthExceededError", userToValidate.Email, string.Format("Email must be less than or equal to {0} characters long.", 64)));
                    }
                    if (!Helper.IsEmailValid(userToValidate.Email))
                    {
                        v.Errors.Add(new ValidationError("Email.InvalidError", userToValidate.Email, "Email is invalid."));
                    }
                }
            }

            if (string.IsNullOrEmpty(userToValidate.Initial))
            {
                v.Errors.Add(new ValidationError("Initial.StaffInitialNotSetError", userToValidate.Initial, "Staff Initial is not set"));
            }

            if (!string.IsNullOrEmpty(userToValidate.Telephone))
            {
                if (userToValidate.Telephone.Length < 5)
                {
                    v.Errors.Add(new ValidationError("Telephone.TelephoneTooShortError", userToValidate.Telephone, "Telephone number should be greater than five digits"));
                }
                else if (!Helper.IsDigitOnly(userToValidate.Telephone))
                {
                    v.Errors.Add(new ValidationError("Telephone.InvalidError", userToValidate.Telephone, "Only numbers are allowed for Phone numbers"));
                }
            }

            if (!string.IsNullOrEmpty(userToValidate.Username) && userToValidate.RowVersionNo2 == 0) //we only validate password at fresh create
            {
                if (userToValidate.AccountType != Constants.AccountType.ADFinacle && userToValidate.AccountType != Constants.AccountType.ADLocal)
                {
                    if (string.IsNullOrEmpty(userToValidate.Password))
                    {
                        v.Errors.Add(new ValidationError("Password.PasswordRequiredError", userToValidate.Password, "Password is not set."));
                    }
                    else
                    {
                        if (userToValidate.Password.Length > 16)
                        {
                            v.Errors.Add(new ValidationError("Password.MaxLengthExceededError", userToValidate.Password, string.Format("Password must be greater than or equal to {0} characters long.", 16)));
                        }
                        else if (userToValidate.Password.Length < 8)
                        {
                            v.Errors.Add(new ValidationError("Password.MinLengthExceededError", userToValidate.Password, string.Format("Password must not be less than {0} characters long.", 8)));
                        }
                        else if ((int)Utility.PasswordAdvisor.CheckStrength(userToValidate.Password) < 4)
                        {
                            v.Errors.Add(new ValidationError("Password.ComplexityViolationError", userToValidate.Password, "Password failed the company's password complexity policy check."));
                        }
                    }
                    if (string.IsNullOrEmpty(userToValidate.ConfirmPassword))
                    {
                        v.Errors.Add(new ValidationError("ConfirmPassword.PasswordRequiredError", userToValidate.ConfirmPassword, "ConfirmPassword is not set."));
                    }
                    else
                    {
                        if (userToValidate.ConfirmPassword.Length > 16)
                        {
                            v.Errors.Add(new ValidationError("ConfirmPassword.MaxLengthExceededError", userToValidate.ConfirmPassword, string.Format("ConfirmPassword must be less than or equal to {0} characters long.", 16)));
                        }
                        else if (userToValidate.ConfirmPassword.Length < 8)
                        {
                            v.Errors.Add(new ValidationError("ConfirmPassword.MinLengthExceededError", userToValidate.ConfirmPassword, string.Format("ConfirmPassword must not be less than {0} characters long.", 8)));
                        }
                    }

                    if (userToValidate.Password != userToValidate.ConfirmPassword)
                    {
                        v.Errors.Add(new ValidationError("Password.PasswordAndConfirmPasswordNotMatch", userToValidate.Password, "Password and confirm password must match."));
                    }
                }
                else
                {
                    userToValidate.Password = "Dummy";
                }
            }

            if (userToValidate.AccountType != Constants.AccountType.LocalFinacle && userToValidate.AccountType != Constants.AccountType.ADFinacle )
              {
                  if (userToValidate.UserRole == null || userToValidate.UserRole.RoleId == Guid.Empty)
                  {
                      v.Errors.Add(new ValidationError("UserRole.RoleId.RoleMustBeSelected", userToValidate.RoleId, "User role must be specified."));
                  }
              }

            if (userToValidate.AccountType == Constants.AccountType.LocalFinacle)
            {
                if ((string.IsNullOrEmpty(userToValidate.BranchID)))
                {
                    v.Errors.Add(new ValidationError("AccountType.InvalidAccountTypeError", userToValidate.AccountType, "The User must have a Finacle Role for this Account Type."));
                }
            }

            if (userToValidate.AccountType == Constants.AccountType.LocalLocal)
            {
                if (!string.IsNullOrWhiteSpace(userToValidate.BranchID))
                {
                    int branchID = 0;
                    if (!int.TryParse(userToValidate.BranchID,out branchID))
                    {
                        v.Errors.Add(new ValidationError("BranchID.InvalidBranchIDError", userToValidate.BranchID, "Branch ID can contain only numbers"));
                    }
                }
                else
                {
                    v.Errors.Add(new ValidationError("BranchID.InvalidBranchIDError", userToValidate.BranchID, "Branch ID cannot be empty"));
                }
            }
            var validationRepository = otherData as IDatastoreValidationRepository;
            if (validationRepository != null)
            {
                if (userToValidate.RowVersionNo2 == 0)
                {
                    if (validationRepository.IsUsernameAlreadyExist(userToValidate.Username))
                    {
                        v.Errors.Add(new ValidationError("Username.UsernameAlreadyExist", userToValidate.Username, "This username is already taken."));
                    }
                }
                if (!string.IsNullOrEmpty(userToValidate.Email))
                {
                    if (!Helper.IsDuplicateEmailPermitted())
                    {
                        if (validationRepository.IsEmailAlreadyExist(userToValidate.Email, userToValidate.Username))
                        {
                            v.Errors.Add(new ValidationError("Email.EmailAlreadyExist", userToValidate.Email, "This email is already taken."));
                        }
                    }
                }                            
            }

            return v;
        }

    }
}
