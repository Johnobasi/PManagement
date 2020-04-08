using PermissionManagement.Model;

namespace PermissionManagement.Repository
{
    public interface IPasswordHistoryRepository
    {
        bool InsertPassword(PasswordHistoryModel passwordHistoryModel);

        bool IsRepeatingPassword(PasswordHistoryModel passwordHistory, int unUsablePasswordCount);

    }
}
