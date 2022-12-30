using Terminal.Gui;
using System.Collections.Generic;

using core.Platform;
using glc.UI.Library;
using glc.UI.Settings;
using glc.ColourScheme;

namespace glc
{
    // Handles window initialisation + UI components
    public static class CAppWindow
    {
        private static Toplevel m_topLevel;

        // High-level UI components:
        private static TabView  m_tabView;
        private static StatusBar m_statusBar;

        /// <summary>
        /// Initialise the app window and UI components
        /// </summary>
        public static void Initialise(List<CBasicPlatform> platforms)
        {
            Application.Init();
            m_topLevel = Application.Top;

            if(CColourSchemeSQL.GetActiveColour(out ColourNode colour))
            {
                if(colour.IsSystem)
                {
                    colour.scheme = Colors.ColorSchemes[colour.Name];
                }
                m_topLevel.ColorScheme = colour.scheme;
            }

            // Create the tab view
            m_tabView = new TabView()
            {
                X = 0,
                Y = 0, // for menu
                Width = Dim.Fill(0),
                Height = Dim.Fill(1),
                CanFocus = false,
            };
            m_tabView.AddTab(new CLibraryTab(platforms), true);
            m_tabView.AddTab(new CSettingsTab(), false);

            // Create the status bar
            m_statusBar = new StatusBar()
            {
                Visible = true,
            };
            m_statusBar.Items = new StatusItem[]
            {
                new StatusItem(Key.Q | Key.CtrlMask, "~CTRL-Q~ Quit", () =>
                {
                    Application.RequestStop();
                    m_topLevel.Clear();
                }),
                new StatusItem(Key.P | Key.CtrlMask, "~CTRL-P~ Run command", () =>
                {

                }),
                new StatusItem(Key.S | Key.CtrlMask, "~CTRL-S~ Scan games", () => {

                }),
                new StatusItem (Key.CharMask, Application.Driver.GetType ().Name, null),
            };

            // Add UI to our toplevel
            m_topLevel.Add(m_statusBar);
            m_topLevel.Add(m_tabView);

            // Add some key handlers
            m_topLevel.KeyDown += KeyDownHandler;
        }

        /// <summary>
        /// Run the application
        /// </summary>
        public static void Run()
        {
            Application.Run();
        }

        /// <summary>
        /// Destroy all UI components
        /// </summary>
        public static void Free()
        {
            Application.Shutdown();
        }

        private static void KeyDownHandler(View.KeyEventEventArgs a)
        {
            //if (a.KeyEvent.Key == Key.Tab || a.KeyEvent.Key == Key.BackTab) {
            //	// BUGBUG: Work around Issue #434 by implementing our own TAB navigation
            //	if (_top.MostFocused == _categoryListView)
            //		_top.SetFocus (_rightPane);
            //	else
            //		_top.SetFocus (_leftPane);
            //}
        }
    }
}
