
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

        public string Id { get; set; }
    }
}