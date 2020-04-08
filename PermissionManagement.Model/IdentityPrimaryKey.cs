using System;

namespace PermissionManagement.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IdentityPrimaryKeyAttribute : Attribute
    {

    }
}
