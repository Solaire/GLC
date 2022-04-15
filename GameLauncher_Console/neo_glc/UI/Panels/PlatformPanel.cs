using Terminal.Gui;
using core;
using System.Collections.Generic;
using Terminal.Gui.Trees;
using System.Linq;

namespace glc
{
    public class CPlatformPanel : CFramePanel<CPlatform, TreeView<CPlatformNode>>
    {
        public CPlatformPanel(List<CPlatform> platforms, string name, Pos x, Pos y, Dim width, Dim height, bool canFocus)
            : base(name, x, y, width, height, canFocus)
        {
            m_contentList = platforms;
            Initialise(name, x, y, width, height, canFocus);
        }

        public override void CreateContainerView()
        {
            m_containerView = new TreeView<CPlatformNode>()
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
                List<PlatformLeafNode> tags = new List<PlatformLeafNode>
                {
                    new PlatformLeafNode("Favourites", platform.PrimaryKey),
                    new PlatformLeafNode("Installed", platform.PrimaryKey),
                    new PlatformLeafNode("Not installed", platform.PrimaryKey)
                };

                PlatformRootNode root = new PlatformRootNode()
                {
                    Name = platform.Name,
                    ID = platform.PrimaryKey,
                    Tags = tags
                };

                m_containerView.AddObject(root);
            }

            m_frameView.Add(m_containerView);
        }
    }

    public abstract class CPlatformNode
    {
        public int ID { get; set; }
    }

    public class PlatformRootNode : CPlatformNode
    {
        public string Name { get; set; }
        public bool IsExpanded { get; set; }

        public List<PlatformLeafNode> Tags { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }

    public class PlatformLeafNode : CPlatformNode
    {
        public string Tag { get; set; }

        public PlatformLeafNode(string name, int id)
        {
            Tag = name;
            ID = id;
        }

        public override string ToString()
        {
            return Tag;
        }
    }

    public class PlatformTreeBuilder : ITreeBuilder<CPlatformNode>
    {
        public bool SupportsCanExpand => true;

        public bool CanExpand(CPlatformNode model)
        {
            return model is PlatformRootNode;
        }

        public IEnumerable<CPlatformNode> GetChildren(CPlatformNode model)
        {
            if(model is PlatformRootNode a)
            {
                return a.Tags;
            }

            return Enumerable.Empty<CPlatformNode>();
        }
    }
}
