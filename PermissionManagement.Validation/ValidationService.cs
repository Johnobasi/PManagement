using System;

namespace PermissionManagement.Validation
{
   public class ValidationService : IValidationService
    {

        public ValidationService()
        {

        }

        public IValidator<T> GetValidatorFor<T>(T entity)
        {
            return null;
        }

        public ValidationState Validate<T>(T entity)
        {
            IValidator<T> validator = GetValidatorFor(entity);

            if (validator == null) // or just return null?
                throw new Exception(string.Format("No validator found for type ({0})", entity.GetType()));

            return validator.Validate(entity, null);
        }

    }
}
