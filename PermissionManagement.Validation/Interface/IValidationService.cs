using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Validation
{
    public interface IValidationService
    {
        ValidationState Validate<T>(T model);
    }
}
