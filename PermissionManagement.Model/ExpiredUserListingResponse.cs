using System.Collections.Generic;

namespace PermissionManagement.Model
{
    public class ExpiredUserListingResponse
    {
        public List<ExpiredUserListingDto> ExpiredUserLstResult { get; set; }
        public PagerItems PagerResource { get; set; }
    }

    public class UserListingReportsList
    {
        public List<UserListingReports> UserLstResult { get; set; }
        public PagerItems PagerResource { get; set; }
    }
}