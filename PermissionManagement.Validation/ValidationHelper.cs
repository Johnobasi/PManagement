using System;
using System.Collections.Generic;
using System.Text;

namespace PermissionManagement.Validation
{
    public static class ValidationHelper
    {
        public static string BuildModelErrorList(ValidationStateDictionary validationStateDictionary)
        {
            var builder = new StringBuilder();

            foreach (KeyValuePair<Type, ValidationState> validationState in validationStateDictionary)
            {
                foreach (ValidationError validationError in validationState.Value.Errors)
                {
                    builder.AppendLine(string.Format("<small>- {0}</small><br />", validationError.Message));
                }
            }
            return builder.ToString();
        }
    }
}
