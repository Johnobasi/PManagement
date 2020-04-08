using PermissionManagement.Model;

namespace PermissionManagement.Repository
{
    public interface ILogRepository
    {
        void LogError(ExceptionLog error);
    }
}
