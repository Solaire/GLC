using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;
using Terminal.Gui.Trees;
using core_2;
using core_2.Platform;

namespace glc_2.UI.Panels
{
    internal class PlatformPanel : BasePanel<BasePlatform, TreeView<PlatformTreeNode>>
    {
        // TEMP
        PlatformRootNode m_searchRoot;

        /// <summary>
        /// Currently seelcted platform node (<see cref="PlatformRootNode"/> or <see cref="PlatformTagNode"/>)
        /// </summary>
        internal PlatformTreeNode CurrentNode
        {
            get;
            set;
        }

        /// <summary>
        /// Construct the panel
        /// </summary>
        /// <param name="square">Position nad size of the panel</param>
        internal PlatformPanel(Box square)
            : base()
        {
            m_searchRoot = new PlatformRootNode()
            {
                Name = "Search",
                ID = -1, // TODO: Replace -1 with search constant ID
                Tags = new List<PlatformTagNode>()

            };
            Initialise("Platforms", square, true);
        }

        /// <summary>
        /// Create a <see cref="TreeView"/> to display the platforms and their tags
        /// </summary>
        protected override void CreateContainerView()
        {
            m_containerView = new TreeView<PlatformTreeNode>()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                CanFocus = true
            };

            m_containerView.TreeBuilder = new PlatformTreeBuilder();

            foreach(BasePlatform platform in DataManager.Platforms)
            {
                PlatformRootNode root = new PlatformRootNode()
                {
                    Name = platform.Name,
                    ID = platform.ID,
                    Tags = new List<PlatformTagNode>()
                };

                foreach(CTag tag in DataManager.GetTagsForPlatform(platform.ID)) // TODO: check if method is only called once
                {
                    if(!tag.IsEnabled)
                    {
                        continue;
                    }
                    root.Tags.Add(new PlatformTagNode(root, tag.Name, tag.ID));
                }
                m_containerView.AddObject(root);
            }
            m_view.Add(m_containerView);

            m_containerView.GoToFirst();
            CurrentNode = m_containerView.SelectedObject;
        }

        /// <summary>
        /// Create a <see cref="PlatformTagNode"/> with the search term as the label and add
        /// to the search root node.
        /// </summary>
        /// <param name="searchTerm"></param>
        public void SetSearchResults(string searchTerm)
        {
            PlatformTagNode searchNode = new PlatformTagNode(m_searchRoot, searchTerm, -1); // TODO: Replace -1 with search constant

            // First search tag. Need to remove all nodes, so that the search root
            // is a the top of the tree
            if(!m_searchRoot.Tags.Any())
            {
                IEnumerable<PlatformTreeNode> existing = new List<PlatformTreeNode>(m_containerView.Objects);
                m_containerView.ClearObjects();

                m_searchRoot.Tags.Add(searchNode);
                m_containerView.AddObject(m_searchRoot);
                m_containerView.AddObjects(existing);
            }
            else if(!m_searchRoot.Tags.Any(tag => tag.Name == searchTerm))
            {
                m_searchRoot.Tags.Add(searchNode);
                m_containerView.RefreshObject(m_searchRoot);
            }

            // Expand the search node and navigate to the search term tag
            // (the LibraryTab.PlatformListView_SelectedChanged) will sort the rest out
            if(!m_searchRoot.IsExpanded)
            {
                m_containerView.Expand(m_searchRoot);
            }
            m_containerView.GoTo(m_searchRoot);
            m_containerView.GoTo(searchNode);
        }
    }

    /// <summary>
    /// Base class representing a <see cref="core_2.Platform.BasePlatform"/> or a <see cref="CTag"/>
    /// as a <see cref="TreeView"/> node.
    /// </summary>
    internal abstract class PlatformTreeNode
    {
        /// <summary>
        /// The primary key
        /// </summary>
        internal int ID
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the field
        /// </summary>
        internal string Name
        {
            get;
            set;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Implementation of <see cref="PlatformTreeNode"/> which represents a
    /// <see cref="core_2.Platform.BasePlatform"/>
    /// </summary>
    internal class PlatformRootNode : PlatformTreeNode
    {
        /// <summary>
        /// Flag determining if the node is expanded and should
        /// show the children
        /// </summary>
        internal bool IsExpanded
        {
            get;
            set;
        }

        /// <summary>
        /// List of children nodes
        /// </summary>
        internal List<PlatformTagNode> Tags
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Implementation of <see cref="PlatformTreeNode"/> which represents a
    /// <see cref="CTag"/>
    /// </summary>
    internal class PlatformTagNode : PlatformTreeNode
    {
        /// <summary>
        /// The parent node
        /// </summary>
        internal PlatformRootNode Parent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">the parent node</param>
        /// <param name="name">The name of the node to display</param>
        /// <param name="id">The primary key</param>
        internal PlatformTagNode(PlatformRootNode parent, string name, int id)
        {
            Parent = parent;
            Name = name;
            ID = id;
        }
    }

    /// <summary>
    /// Implementation of <see cref="ITreeBuilder{PlatformTreeNode}"/> which is used
    /// to create the tree of nodes to display in the <see cref="PlatformPanel"/>
    /// </summary>
    internal class PlatformTreeBuilder : ITreeBuilder<PlatformTreeNode>
    {
        public bool SupportsCanExpand => true;

        public bool CanExpand(PlatformTreeNode node)
        {
            return node is PlatformRootNode;
        }

        public IEnumerable<PlatformTreeNode> GetChildren(PlatformTreeNode node)
        {
            if(node is PlatformRootNode root)
            {
                return root.Tags;
            }
            return Enumerable.Empty<PlatformTreeNode>();
        }
    }
}
