using System.Collections.Generic;

namespace PermissionManagement.Model
{
    public class AllUserListModel
    {
        public ReportTypeEnum ReportTypeEnum { get; set; }
        public string Searchkey { get; set; }
        public List<UserListingReports> UserLstResult { get; set; }
        public PagerItems PagerResource { get; set; }
        public int SortOrder { get; set; }
        public PortalSetting psDormentDays;
        public UserListingReportsList UserListingReportsList { get; set; }

    }
}