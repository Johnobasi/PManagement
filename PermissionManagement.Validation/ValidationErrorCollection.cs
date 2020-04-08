using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace PermissionManagement.Validation
{
    public class ValidationErrorCollection : Collection<ValidationError>
    {
        public void Add(string name, object attemptedValue, string message)
        {
            Add(new ValidationError(name, attemptedValue, message));
        }

        public void Add(string name, object attemptedValue, Exception exception)
        {
            Add(new ValidationError(name, attemptedValue, exception));
        }
    }
}
