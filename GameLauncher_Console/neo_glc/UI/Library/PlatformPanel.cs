using System.Collections.Generic;
using System.Linq;

using core.Platform;
using core.Tag;

using Terminal.Gui;
using Terminal.Gui.Trees;

namespace glc.UI.Library
{
    public class CPlatformTreePanel : CFramePanel<CBasicPlatform, TreeView<IPlatformTreeNode>>
    {
        PlatformRootNode m_searchNode;
        bool m_gotSearchNode;

        public CPlatformTreePanel(List<CBasicPlatform> platforms, string name, Pos x, Pos y, Dim width, Dim height, bool canFocus)
            : base(name, x, y, width, height, canFocus)
        {
            m_searchNode = new PlatformRootNode()
            {
                Name = "Search",
                ID = -1,
                Tags = new List<PlatformTagNode>(),
            };
            m_gotSearchNode = false;

            m_contentList = platforms;
            Initialise(name, x, y, width, height, canFocus);
        }

        public override void CreateContainerView()
        {
            m_containerView = new TreeView<IPlatformTreeNode>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                CanFocus = true,
            };

            m_containerView.TreeBuilder = new PlatformTreeBuilder();

            foreach(CPlatform platform in m_contentList)
            {
                PlatformRootNode root = new PlatformRootNode()
                {
                    Name    = platform.Name,
                    ID      = platform.PrimaryKey,
                    Tags    = new List<PlatformTagNode>()
                };

                List<TagObject> tags = CTagSQL.GetTagsforPlatform(platform.PrimaryKey);

                foreach(TagObject tag in tags)
                {
                    if(!tag.isEnabled)
                    {
                        continue;
                    }

                    root.Tags.Add(new PlatformTagNode(tag.PrimaryKey, tag.Name));
                }

                m_containerView.AddObject(root);
            }

            m_frameView.Add(m_containerView);
        }

        public void SetSearchResults(string searchTerm)
        {
            if(!m_gotSearchNode)
            {
                IEnumerable<IPlatformTreeNode> existing = new List<IPlatformTreeNode>(m_containerView.Objects);
                m_containerView.ClearObjects();

                m_searchNode.Tags.Add(new PlatformTagNode(0, searchTerm));
                m_containerView.AddObject(m_searchNode);
                m_containerView.AddObjects(existing);

                m_gotSearchNode = true;
                return;
            }

            if(m_searchNode.Tags.FindIndex(tag => tag.Name == searchTerm) == -1)
            {
                m_searchNode.Tags.Add(new PlatformTagNode(0, searchTerm));
                m_containerView.RefreshObject(m_searchNode);
                return;
            }
        }
    }

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
        public PlatformTagNode(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    public class PlatformTreeBuilder : ITreeBuilder<IPlatformTreeNode>
    {
        public bool SupportsCanExpand => true;

        public bool CanExpand(IPlatformTreeNode model)
        {
            return model is PlatformRootNode;
        }

        public IEnumerable<IPlatformTreeNode> GetChildren(IPlatformTreeNode model)
        {
            if(model is PlatformRootNode a)
            {
                return a.Tags;
            }

            return Enumerable.Empty<IPlatformTreeNode>();
        }
    }
}