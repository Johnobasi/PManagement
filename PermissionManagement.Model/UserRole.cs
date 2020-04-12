using System;

namespace PermissionManagement.Model
{
    public class UserRole
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string RowVersionNo { get; set; }
        public string IsDeleted { get; set; }

        public Role Role { get; set; }
        public Guid RoleId { get; set; }
    }
}
