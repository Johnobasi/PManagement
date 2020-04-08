using System;
using System.Collections.Generic;
using System.Linq;

namespace PermissionManagement.Validation
{
    public class ValidationStateDictionary : Dictionary<Type, ValidationState>
    {
        public ValidationStateDictionary() { }

        public ValidationStateDictionary(Type type, ValidationState validationState)
        {
            Add(type, validationState);
        }

        public bool IsValid
        {
            get
            {
                return this.All(validationState => validationState.Value.IsValid);
            }
        }

        //public void Add(Type type, object v)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
