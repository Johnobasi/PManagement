using DataTables.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Model
{
    public class PagerItems
    {
        public int ResultCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SortBy { get; set; }
        public string SortDirection { get; set; }
        public string siteSearch { get; set; }
    }

    public class PagerItemsII
    {
        public int ResultCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IOrderedEnumerable<Column> SortColumns { get; set; }
        public IEnumerable<Column> SearchColumns { get; set; }
        public string siteSearch { get; set; }
    }
}
