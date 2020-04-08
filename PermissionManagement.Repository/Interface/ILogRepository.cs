using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionManagement.Model;


namespace PermissionManagement.Repository
{
    public interface ILogRepository
    {
        void LogError(ExceptionLog error);
    }
}
