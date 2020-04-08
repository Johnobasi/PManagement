using PermissionManagement.Model;
using System.Collections.Generic;

namespace PermissionManagement.Services
{
    public interface ILogService
    {
        void LogError(ExceptionLog error, bool fromErrorMail);
        bool SendEmail(IList<UserMailDto> toAddressList, IList<UserMailDto> ccAddressList, string messageText, string messageHtml, string subject, bool fromErrorMail);
    }
}
