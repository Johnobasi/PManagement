﻿using System;

namespace PermissionManagement.Model
{
    public class PasswordHistoryModel
    {
        [IdentityPrimaryKey]
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
}
