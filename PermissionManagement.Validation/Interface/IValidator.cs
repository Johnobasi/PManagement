using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Validation
{
   public interface IValidator <T>
    {
       ValidationState Validate(T entity, object otherData);
    }
}
