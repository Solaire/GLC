using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;
using Terminal.Gui.Trees;
using core_2;
using core_2.Platform;

namespace glc_2.UI.Panels
{
    internal class CPlatformPanel : CBasePanel<CPlatform, TreeView<CPlatformNode>>
    {
        public CPlatformNode CurrentNode { get; set; }

        internal CPlatformPanel(Square square)
            : base()
        {
            Initialise("Platforms", square, true);
        }

        protected override void CreateContainerView()
        {
            m_containerView = new TreeView<CPlatformNode>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                CanFocus = true
            };

            m_containerView.TreeBuilder = new CPlatformTreeBuilder();

            foreach(CPlatform platform in CDataManager.Platforms)
            {
                CPlatformRootNode root = new CPlatformRootNode()
                {
                    Name = platform.Name,
                    ID = platform.ID,
                    Tags = new List<CPlatformTagNode>()
                };

                foreach(CTag tag in CDataManager.GetTagsForPlatform(platform.ID)) // TODO: check if method is only called once
                {
                    if(!tag.IsEnabled)
                    {
                        continue;
                    }
                    root.Tags.Add(new CPlatformTagNode(root, tag.Name, tag.ID));
                }
                m_containerView.AddObject(root);
            }
            m_view.Add(m_containerView);

            m_containerView.GoToFirst();
            CurrentNode = m_containerView.SelectedObject;
        }
    }

    internal abstract class CPlatformNode
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    internal class CPlatformRootNode : CPlatformNode
    {
        /*
        private int m_id;
        private string m_name;

        #region IPlatformNode
        public int ID
        {
            get => m_id;
            set { m_id = value; }
        }

        public string Name
        {
            get => m_name;
            set { m_name = value; }
        }
        #endregion IPlatformNode
        */

        public bool IsExpanded { get; set; }

        public List<CPlatformTagNode> Tags { get; set; }
    }

    internal class CPlatformTagNode : CPlatformNode
    {
        public CPlatformRootNode Parent
        {
            get;
            private set;
        }
        /*
        private string m_name;
        private int m_id;

        #region IPlatformNode
        public int ID
        {
            get => m_id;
            set { m_id = value; }
        }

        public string Name
        {
            get => m_name;
            set { m_name = value; }
        }
        #endregion IPlatformNode
        */

        internal CPlatformTagNode(CPlatformRootNode parent, string name, int id)
        {
            Parent = parent;
            Name = name;//m_name = name;
            ID = id;//m_id = id;
        }
    }

    internal class CPlatformTreeBuilder : ITreeBuilder<CPlatformNode>
    {
        public bool SupportsCanExpand => true;

        public bool CanExpand(CPlatformNode node)
        {
            return node is CPlatformRootNode;
        }

        public IEnumerable<CPlatformNode> GetChildren(CPlatformNode node)
        {
            if(node is CPlatformRootNode root)
            {
                return root.Tags;
            }
            return Enumerable.Empty<CPlatformNode>();
        }
    }
}
