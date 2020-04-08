using PermissionManagement.Model;

namespace PermissionManagement.Repository
{
    public interface IFinacleRepository
    {
        FinacleRole GetUserRoleFromFlexcube(string userID);
        string GetUserTillAccountFromFinacle(string userID);
    }
}
