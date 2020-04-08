using System;

namespace PermissionManagement.Model
{
    public class PasswordHistoryModel
    {
        public string userName { get; set; }
        public string password { get; set; }
        public DateTime? createDate { get; set; }
    }
}
