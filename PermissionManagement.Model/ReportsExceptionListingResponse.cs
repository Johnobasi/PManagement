using System.Collections.Generic;

namespace PermissionManagement.Model
{
    public class ReportsExceptionListingResponse
    {
        public List<ReportsExceptionDto> UserListingResult { get; set; }
        public PagerItems PagerResource { get; set; }
    }
}