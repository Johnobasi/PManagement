using System;

namespace PermissionManagement.Model
{
    public  class ReportsExceptionDto
    {
        public int ? ExceptionId { get; set; }
        public DateTime? ExceptionDateTime { get; set; }
        public string ExceptionDetails { get; set; }
        public string ExceptionPage { get; set; }
        public string LoggedInUser { get; set; }
        public string UserIpAddress { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionVersion { get; set; }

    }
}
 