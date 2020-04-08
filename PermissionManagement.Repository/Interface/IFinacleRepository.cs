using PermissionManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Repository
{
    public interface IFinacleRepository
    {
        FinacleRole GetUserRoleFromFinacle(string userID);
        string GetUserTillAccountFromFinacle(string userID);
    }
}
