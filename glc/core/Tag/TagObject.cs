using core.DataAccess;

namespace core.Tag
{
    public struct TagObject : IDataNode
    {
        public int tagID;
        public string name;
        public string description;
        public bool isEnabled;
        public bool isInternal;

        public int PrimaryKey
        {
            get { return tagID; }
            private set { tagID = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        public TagObject(int tagID, string name, string description, bool isActive, bool isInternal)
        {
            this.tagID = tagID;
            this.name = name;
            this.description = description;
            this.isEnabled = isActive;
            this.isInternal = isInternal;
        }

        public TagObject(CTagSQL.CQryTag qry)
        {
            this.tagID = qry.TagID;
            this.name = qry.Name;
            this.description = qry.Description;
            this.isEnabled = qry.IsActive;
            this.isInternal = qry.IsInternal;
        }

        public TagObject(CTagSQL.CQryTagsForPlatform qry)
        {
            this.tagID = qry.TagID;
            this.name = qry.Name;
            this.description = qry.Description;
            this.isEnabled = qry.PlatformEnabled;
            this.isInternal = false; // Unused
        }
    }
}
