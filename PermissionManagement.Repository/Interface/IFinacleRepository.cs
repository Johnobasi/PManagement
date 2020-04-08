using PermissionManagement.Model;

namespace PermissionManagement.Repository
{
    public interface IFinacleRepository
    {
        FinacleRole GetUserRoleFromFinacle(string userID);
        string GetUserTillAccountFromFinacle(string userID);
    }
}
