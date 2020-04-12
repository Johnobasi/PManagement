using System;

namespace PermissionManagement.IoC
{
    public interface IRegistrationKey
    {
        Type GetInstanceType();
        bool Equals(object obj);
        int GetHashCode();
    }

}
