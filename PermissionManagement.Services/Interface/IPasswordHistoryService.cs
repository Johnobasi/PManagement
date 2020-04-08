using PermissionManagement.Model;

namespace PermissionManagement.Services
{
    public interface IPasswordHistoryService
    {
        bool InsertPassword(PasswordHistoryModel passwordHistoryModel);

        bool IsRepeatingPassword(PasswordHistoryModel passwordHistoryModel, out int unUsablePreviousPasswordCount);

    }
}
