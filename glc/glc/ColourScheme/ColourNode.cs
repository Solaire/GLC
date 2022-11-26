using core;
using Terminal.Gui;

namespace glc.ColourScheme
{
    public struct ColourNode : IDataNode
    {
        private readonly int    id;
        private readonly string name;
        private readonly string description;
        private bool   isEnabled;
        private bool   isSystem;

        public ColorScheme scheme;

        public int PrimaryKey
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Description
        {
            get { return description; }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        public bool IsSystem
        {
            get { return isSystem; }
            set { isSystem = value; }
        }

        public ColourNode(CColourSchemeSQL.CQryColourScheme qry)
        {
            this.id = qry.ColourSchemeID;
            this.name = qry.Name;
            this.description = qry.Description;
            this.isEnabled = qry.IsActive;
            this.isSystem = qry.IsSystem;

            this.scheme = null;
        }
    }
}
