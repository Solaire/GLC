using core.Platform;

namespace core
{
    /// <summary>
    /// Class encapsulating all data management, which
    /// includes the data access (SQL) as well as game/platform caching.
    /// </summary>
    internal class CDataManger
    {
        private List<IPlatformTreeNode> m_platforms;

        public CDataManger(List<CBasicPlatform> platforms)
        {
            BuildPlatformTree(platforms);
        }

        private void BuildPlatformTree(List<CBasicPlatform> platforms)
        {
            foreach(CBasicPlatform platform in platforms)
            {
                PlatformRootNode root = new PlatformRootNode()
                {
                    Name = platform.Name,
                    ID = platform.PrimaryKey,
                    Tags = new List<PlatformTagNode>()
                };
            }
        }
    }

    // TODO: Use separate class
    public interface IPlatformTreeNode
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }

    public abstract class CPlatformTreeNode : IPlatformTreeNode
    {
        protected int id;
        protected string name;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class PlatformRootNode : CPlatformTreeNode
    {
        public bool IsExpanded { get; set; }

        public List<PlatformTagNode> Tags { get; set; }
    }

    public class PlatformTagNode : CPlatformTreeNode
    {
        private int platformID;

        public PlatformTagNode(int id, string name, int platformID)
        {
            this.id = id;
            this.name = name;
            this.platformID = platformID;
        }

        public int PlatformID
        {
            get { return platformID; }
            set { platformID = value; }
        }
    }
}
