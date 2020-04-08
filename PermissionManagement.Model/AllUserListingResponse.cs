using System.Collections.Generic;

namespace PermissionManagement.Model
{
    public class AllUserListingResponse
    {
        public List<ExpiredUserListingDto> ExpiredUserLstResult { get; set; }
        public PagerItems PagerResource { get; set; }
    }
}