using core_2.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Terminal.Gui;

namespace glc_2.UI.Panels
{
    internal class CKeyBindingPanel : CBasePanel<StatusItem, ListView>
    {
        private Dictionary<Key, StatusItem> m_bindings; // TODO: Will need a dictionary data source

        public CKeyBindingPanel(Square square, Dictionary<Key, StatusItem> items) // List<StatusItem> items)
            : base()
        {
            Initialise("Key bindings", square, false);
            m_containerView.Source = new CKeyBindingDataSource(items.Values.ToList());
            m_bindings = items;
        }

        protected override void CreateContainerView()
        {
            m_containerView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = false,
            };

            m_view.Add(m_containerView);
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

        // TODO: Optimise with a non-list structure
        public void PerformKeyAction(View.KeyEventEventArgs a)
        {
            if((a.KeyEvent.Key & Key.CtrlMask) != Key.CtrlMask || !m_bindings.ContainsKey(a.KeyEvent.Key))
            {
                return;
            }

            m_bindings[a.KeyEvent.Key].Action.Invoke();
        }
    }
}
