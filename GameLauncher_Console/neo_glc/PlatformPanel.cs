#define TREE

using Terminal.Gui;
using core;
using System;
using System.Collections.Generic;
using Terminal.Gui.Trees;
using System.Linq;

namespace glc
{
#if TREE
    public class CPlatformPanel : CFramePanel<CPlatform, TreeView<CPlatformNode>>
#else
    //public class CPlatformPanel : CFramePanel<CPlatform, ListView>
#endif // TREE
    {
        public CPlatformPanel(List<CPlatform> platforms, string name, Pos x, Pos y, Dim width, Dim height, bool canFocus, Key focusShortCut)
            : base(name, x, y, width, height, canFocus, focusShortCut)
        {
            m_contentList = platforms;
            Initialise(name, x, y, width, height, canFocus, focusShortCut);
        }

        public override void CreateContainerView()
        {
#if TREE
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
                List<PlatformLeafNode> groups = new List<PlatformLeafNode>
                {
                    new PlatformLeafNode("Favourites"),
                    new PlatformLeafNode("Installed"),
                    new PlatformLeafNode("Not installed")
                };

                PlatformRootNode root = new PlatformRootNode()
                {
                    Name = platform.Name,
                    ID = platform.ID,
                    Groups = groups
                };

                m_containerView.AddObject(root);
            }

#else
            m_containerView = new ListView(new CPlatformDataSource(m_contentList))
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = true,
            };
#endif // TREE

            m_frameView.Add(m_containerView);
        }
    }

    internal class CPlatformDataSource : CGenericDataSource<CPlatform>
    {
        public CPlatformDataSource(List<CPlatform> itemList)
            : base(itemList)
        {

        }

        protected override string ConstructString(int itemIndex)
        {
            return String.Format(String.Format("{{0,{0}}}", 0), ItemList[itemIndex].Name);
        }

        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].Description;
        }
    }

    public abstract class CPlatformNode
    {

    }

    public class PlatformRootNode : CPlatformNode
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public bool IsExpanded { get; set; }

        public List<PlatformLeafNode> Groups { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }

    public class PlatformLeafNode : CPlatformNode
    {
        public string Group { get; set; }

        public PlatformLeafNode(string name)
        {
            Group = name;
        }

        public override string ToString()
        {
            return Group;
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
                return a.Groups;
            }

            return Enumerable.Empty<CPlatformNode>();
        }
    }
}
