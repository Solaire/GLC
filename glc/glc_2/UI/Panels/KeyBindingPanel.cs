using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace glc_2.UI.Panels
{
    /// <summary>
    /// Implementation of <see cref="BasePanel{StatusItem, ListView}"/> which will
    /// store and display the keybindings used in the app. The methods called by the
    /// keybindings might not be present in this class.
    /// </summary>
    internal class KeyBindingPanel : BasePanel<StatusItem, ListView>
    {
        private Dictionary<Key, StatusItem> m_bindings; // TODO: Will need a dictionary data source

        /// <summary>
        /// Construct the panel and create the data source
        /// </summary>
        /// <param name="box">Position and size of the panel</param>
        /// <param name="items">Dictionary of <see cref="StatusItem"/>, keyed on <see cref="Key"/></param>
        internal KeyBindingPanel(Box box, Dictionary<Key, StatusItem> items) // List<StatusItem> items)
        {
            Initialise("Key bindings", box, false);
            m_containerView.Source = new KeyBindingDataSource(items.Values.ToList());
            m_bindings = items;
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Implementation of <see cref="ListDataSource{StatusItem}"/>
        /// </summary>
        internal class KeyBindingDataSource : ListDataSource<StatusItem>
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="itemList">List of <see cref="StatusItem"/></param>
            public KeyBindingDataSource(List<StatusItem> itemList)
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

        /// <summary>
        /// If the <see cref="KeyEvent.Key"/> is bound to a <see cref="StatusItem"/>, invoke the
        /// bound action.
        /// </summary>
        /// <param name="keyEvent">The key event</param>
        public void PerformKeyAction(KeyEvent keyEvent)
        {
            if((keyEvent.Key & Key.CtrlMask) != Key.CtrlMask || !m_bindings.ContainsKey(keyEvent.Key))
            {
                return;
            }

            m_bindings[keyEvent.Key].Action.Invoke();
        }
    }
}
