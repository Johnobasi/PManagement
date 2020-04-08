using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Model
{
      public class Menu
    {
        public Guid MenuID { get; set; }
        public Guid ParentMenuID { get; set; }
        public Guid PageID { get; set; }
        public Menu Parent { get; set; }
        public bool HasChildren { get; set; }
        public bool Linkable { get; set; }
        public bool IsNewWindow { get; set; }
        public bool IsVisible { get; set; }
        public string MenuLink { get; set; }
        public string RawUrl { get; set; }
        public string DisplayName { get; set; }
        public int DisplayOrder { get; set; }

    }

    public class MenuDto
    {
        public Guid PageID { get; set; }
        public Guid ParentMenuID { get; set; }
        public string DisplayName { get; set; }
        public string MenuLink { get; set; }
    }
}
