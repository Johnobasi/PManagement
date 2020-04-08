using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.Security;
using System.Web.Profile;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace PermissionManagement.Web
{
    public class Menu
    {
        public Menu Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        private Menu _parent;
        public bool HasChildren
        {
            get { return _hasChildren; }
            set { _hasChildren = value; }
        }

        private bool _hasChildren;
        public bool Linkable
        {
            get { return _linkable; }
            set { _linkable = value; }
        }

        private bool _linkable;
        public string MenuUrl
        {
            get { return _url; }
            set { _url = value; }
        }

        private string _url;
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        private string _displayName;

        public string MenuLiClass { get; set; }
        public string MenuUlClass { get; set; }
    }
}