using core.Game;
using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace glc.UI.Library
{
    // TODO: Replace with a custom list view or something that allows multiple columns
    public class CKeyBindingPanel : CFramePanel<StatusItem, ListView>
    {
        public CKeyBindingPanel(List<StatusItem> keyBindings, string name, Pos x, Pos y, Dim width, Dim height)
            : base(name, x, y, width, height, false)
        {
            m_contentList = keyBindings;
            Initialise(name, x, y, width, height, false);
        }

        public override void CreateContainerView()
        {
            m_containerView = new ListView(new CKeyBindingDataSource(m_contentList))
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = false,
            };

            m_frameView.Add(m_containerView);
        }

        internal class CKeyBindingDataSource : CGenericDataSource<StatusItem>
        {
            public CKeyBindingDataSource(List<StatusItem> itemList)
                : base(itemList)
            {

            }

            protected override string ConstructString(int itemIndex)
            {
                return String.Format(String.Format("{{0,{0}}}", 0), ItemList[itemIndex].Title);
            }

            protected override string GetString(int itemIndex)
            {
                return ItemList[itemIndex].Title.ToString();
            }

            public override void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
            {
                base.Render(container, driver, false, item, (line >= container.Bounds.Height) ? 1 : 0, line, width, start);
            }
        }

        public void PerformKeyAction(View.KeyEventEventArgs a)
        {
            for(int i = 0; i < m_contentList.Count; ++i)
            {
                if((m_contentList[i].Shortcut & a.KeyEvent.Key) == m_contentList[i].Shortcut)
                {
                    m_contentList[i].Action.Invoke();
                    return;
                }
            }
        }
    }

    /*
    public class CKeyBindingPanel
    {
        private FrameView   m_frameView;
        private StatusItem[] m_items;

        public FrameView FrameView { get { return m_frameView; } }

        public CKeyBindingPanel(string name, Pos x, Pos y, Dim width, Dim height)
        {
            // FrameView construction
            m_frameView = new FrameView(name)
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                CanFocus = false
            };
            m_frameView.Title = $"{m_frameView.Title}";
            InitStatusItems();
        }

        public void InitStatusItems()
        {
            m_items = new StatusItem[]
            {
                new StatusItem(Key.F, "~Q~ Favourite", () =>
                {

                }),
                new StatusItem(Key.E, "~E~ Edit", () =>
                {

                }),
                new StatusItem(Key.R, "~R~ Set rating", () =>
                {

                }),
                new StatusItem(Key.Enter, "~Entry~ Start game", () =>
                {

                }),
                new StatusItem(Key.Enter, "~H~ Hide game", () =>
                {

                })
            };

            // TODO: draw those column-by column
            // Will need to get the frame height
            int row = 0;
            for(int i = 0; i < m_items.Length - 1; i += 2)
            {
                AddLabel(m_items[i].Title.ToString(), 0, row, Dim.Percent(50), 1, TextAlignment.Left);
                AddLabel(m_items[i + 1].Title.ToString(), Pos.Percent(50), row++, Dim.Percent(50), 1, TextAlignment.Left);
            }

            // Handle odds
            if(m_items.Length % 2 == 1)
            {
                AddLabel(m_items[m_items.Length - 1].Title.ToString(), 0, row, Dim.Percent(50), 1, TextAlignment.Left);
            }
        }

        private void AddLabel(string title, Pos x, int y, Dim width, Dim height, TextAlignment alignment)
        {
            Label label = new Label(title)
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                TextAlignment = alignment
            };
            m_frameView.Add(label);
        }
    }
    */
}
