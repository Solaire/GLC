using glc_2.UI.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace glc_2
{
    internal static class CWindow
    {
        private static Toplevel m_toplevel;


        private static TabView m_tabView;
        private static StatusBar m_statusBar;

        public static void Initialise()
        {
            Application.Init();
            m_toplevel = Application.Top;

            SetColour();
            InitialiseTabView();
            InitialiseStatusBar();

            m_toplevel.Add(m_tabView);
            m_toplevel.Add(m_statusBar);

            m_toplevel.KeyDown += KeyDownHandler;
        }

        private static void SetColour()
        {
            // TODO:
        }

        private static void InitialiseTabView()
        {
            m_tabView = new TabView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(1),
                CanFocus = false
            };
            m_tabView.AddTab(new CLibraryTab(), true);
            //m_tabView.Add(new CSettingsTab(), true);
        }

        private static void InitialiseStatusBar()
        {
            m_statusBar = new StatusBar()
            {
                Visible = true,
            };
            m_statusBar.Items = new StatusItem[]
            {
                new StatusItem(Key.Q | Key.CtrlMask, "~C^Q~ Quit", () =>
                {
                    Application.RequestStop();
                }),
                /*
                new StatusItem(Key.Q | Key.CtrlMask, "~C^R~ Run command", () =>
                {

                }),
                */
                new StatusItem(Key.F | Key.CtrlMask, "~C^F~ Search", () =>
                {

                }),
                new StatusItem(Key.S | Key.CtrlMask, "~C^S~ Scan games", () =>
                {

                }),
                new StatusItem (Key.Null, Application.Driver.GetType().Name, null)
            };
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
