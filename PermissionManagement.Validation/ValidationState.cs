using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Validation
{
    public class ValidationState
    {
        private readonly ValidationErrorCollection errors;

        public ValidationState()
        {
            errors = new ValidationErrorCollection();
        }

        public ValidationErrorCollection Errors
        {
            get { return errors; }
        }

        public bool IsValid
        {
            get
            {
                return Errors.Count == 0;
            }
        }
    }
}
