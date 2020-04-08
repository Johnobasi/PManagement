using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PermissionManagement.Model;
using PermissionManagement.Utility;

namespace PermissionManagement.Services
{
    public interface ILogService
    {
        void LogError(ExceptionLog error, bool fromErrorMail);
        bool SendEmail(IList<UserMailDto> toAddressList, IList<UserMailDto> ccAddressList, string messageText, string messageHtml, string subject, bool fromErrorMail);
    }
}
