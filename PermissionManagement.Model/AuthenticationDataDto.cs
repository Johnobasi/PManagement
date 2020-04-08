
namespace PermissionManagement.Model
{
    public class AuthenticationDataDto
    {
        public string SessionId { get; set; }

        public string Username { get; set; }

        public string Roles { get; set; }

        public string FullName { get; set; }

        public bool IsFirstLogIn { get; set; }

        public bool IsPasswordSet { get; set; }

        public bool IsRoleSet { get; set; }

        public bool AppAuthenticationFailed { get; set; }

        public string BranchCode { get; set; }

        public string AccountType { get; set; }
    }
}
