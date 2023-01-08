using core_2;
using core_2.Game;
using glc_2.UI.Panels;
using glc_2.UI.Views;
using System.Collections.Generic;
using System.Xml;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace glc_2.UI.Tabs
{
    internal class CLibraryTab : TabView.Tab
    {
        private static View m_container;

        private static CPlatformPanel   m_platformPanel;
        private static CGamePanel       m_gamePanel;
        //private CGameInfoPanel   m_gameInfoPanel;
        //private CKeyBindingPanel m_keyBindingPanel;

        public CLibraryTab()
            : base()
        {
            Text = "Library";
            m_container = new View()
            {
                X = 0,
                Y = 0, // for menu
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                CanFocus = false,
            };

            InitialisePlatformPanel();
            InitialiseGamePanel();
            //InitializeGameInfoPanel();
            //InitialiseKeyBindingPanel();

            View = m_container;
            //View.KeyDown += m_keyBindingPanel.PerformKeyAction;
        }

        private void InitialisePlatformPanel()
        {
            m_platformPanel = new CPlatformPanel(new Square(0, 0, Dim.Percent(25), Dim.Percent(60)));
            m_platformPanel.ContainerView.SelectionChanged += PlatformListView_SelectedChanged;
            m_platformPanel.ContainerView.ObjectActivated += PlatformListView_ObjectActivated;

            m_container.Add(m_platformPanel.View);
        }

        private void InitialiseGamePanel()
        {
            CDataManager.LoadPlatformGames(m_platformPanel.CurrentNode.ID);

            //Dictionary<string, List<CGame>> games = CDataManager.LoadGames(m_platformPanel.CurrentNode.ID);

            m_gamePanel = new CGamePanel(new Square(Pos.Percent(25), 0, Dim.Fill(), Dim.Percent(60)), CDataManager.GetPlatformGames(m_platformPanel.CurrentNode.ID));
            m_gamePanel.ContainerView.OpenSelectedItem += GameListView_OpenSelectedItem;
            m_gamePanel.ContainerView.SelectedItemChanged += GameListView_SelectedChanged;

            m_container.Add(m_gamePanel.View);
        }
        /*
        private void InitializeGameInfoPanel()
        {
            //if(CSystemAttributeSQL.GetBoolValue(CSystemAttributeSQL.A_SHOW_GAME_INFO_PANEL))
            if(CDataManager.GetBoolSetting(CAppSettings.A_SHOW_GAME_INFO_PANEL, false))
            {
                m_gameInfoPanel = new CGameInfoPanel(Pos.Percent(25), Pos.Percent(60), Dim.Fill(), Dim.Fill());
                m_container.Add(m_gameInfoPanel.FrameView);
            }
            else
            {
                m_gamePanel.FrameView.Height = Dim.Fill();
            }
        }

        private void InitialiseKeyBindingPanel()
        {
            List<StatusItem> keyBindings = new List<StatusItem>
            {
                new StatusItem(Key.F, "~F~ Favourite", () =>
                {
                    SelectedGameToggleFavourite();
                }),
                new StatusItem(Key.E, "~E~ Edit", () =>
                {
                    SelectedGameEdit();
                }),
                new StatusItem(Key.R, "~R~ Set rating", () =>
                {
                    SelectedGameSetRating();
                }),
                new StatusItem(Key.R, "~S~ Search", () =>
                {
                    GameSearch();
                }),
                new StatusItem(Key.Enter, "~Entry~ Start game", () =>
                {
					//GameListView_OpenSelectedItem(new ListViewItemEventArgs(m_gamePanel.ContainerView.SelectedItem, m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem]));
					//GameListView_OpenSelectedItem(new CellActivatedEventArgs(m_gamePanel.ContainerView.Table, m_gamePanel.ContainerView.SelectedRow, m_gamePanel.ContainerView.SelectedColumn));
				}),
            };

            m_keyBindingPanel = new CKeyBindingPanel(0, Pos.Percent(60), Dim.Percent(25), Dim.Fill(), keyBindings);
            m_container.Add(m_keyBindingPanel.FrameView);
        }
        */

        /// <summary>
		/// Handle change in the platform list view
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void PlatformListView_SelectedChanged(object sender, SelectionChangedEventArgs<CPlatformNode> e)
        {
            var val = e.NewValue;
            if(val == null || val == m_platformPanel.CurrentNode)
            {
                return;
            }

            m_platformPanel.CurrentNode = val;
            if(val is CPlatformRootNode) // Switch to new multilist
            {
                CPlatformRootNode node = (CPlatformRootNode)val;

                // Going back to root
                if(m_platformPanel.CurrentNode is CPlatformTagNode tag && node.ID == tag.Parent.ID)
                {
                    m_gamePanel.MultiListMode();
                    return;
                }

                /*
                var temp = CDataManager.GetDefaultGames(node.ID);
                Dictionary<string, CGameList> gameList = new Dictionary<string, CGameList>();
                foreach(KeyValuePair<string, List<CGame>> kv in temp)
                {
                    gameList[kv.Key] = new CGameList(kv);
                }
                */

                CDataManager.LoadPlatformGames(node.ID);
                m_gamePanel.ContainerView.Source = new CGameDataMultilistSource(CDataManager.GetPlatformGames(node.ID));

                /*
                CPlatformRootNode node = (CPlatformRootNode)val;
                Dictionary<string, List<CGame>> sqlSource = new Dictionary<string, List<CGame>>();

                // Special search handler
                if(node.ID == -1) // (int)SpecialPlatformID.cSearch)
                {
                    foreach(CPlatformTagNode tag in node.Tags)
                    {
                        //sqlSource[tag.Name] = CGameSQL.GameSearch(tag.Name).ToList();
                    }
                }
                else if(node.ID > 0) // Normal node
                {
                    //sqlSource = CGameSQL.LoadPlatformGamesAsDict(node.ID);
                }
                else
                {
                    return;
                }

                Dictionary<string, CGameList> multilistSource = new Dictionary<string, CGameList>();
                foreach(KeyValuePair<string, List<CGame>> kv in sqlSource)
                {
                    multilistSource[kv.Key] = new CGameList(kv);
                }

                //m_gamePanel.NewMultilistSource(multilistSource);
                m_gamePanel.ContainerView.Source = new CGameDataMultilistSource(multilistSource);
                */
            }

            else if(val is CPlatformTagNode)
            {
                m_gamePanel.SingleListMode(val.Name);

                // TODO: Handle the following cases:
                //		Game search
                //		Favourites

                /*
                CPlatformTagNode node = (CPlatformTagNode)val;
                if(node.ID == -1) // (int)SpecialPlatformID.cSearch) // Handle search
                {
                    //m_gamePanel.ContentList = CGameSQL.GameSearch(node.Name).ToList();
                    //m_gamePanel.SingleListMode(node.Name);
                }
				else if(node.ID == (int)SpecialPlatformID.cFavourites) // Handle favourite games
				{
					//m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(node.PlatformID, true).ToList();
				}
                if(node.ID > 0)
                {
                    //m_gamePanel.SingleListMode(node.Name);
                }
				*/
            }
        }

        private static void PlatformListView_ObjectActivated(ObjectActivatedEventArgs<CPlatformNode> obj)
        {
            if(obj.ActivatedObject is CPlatformRootNode root)
            {
                if(root.IsExpanded)
                {
                    m_platformPanel.ContainerView.Collapse(root);
                }
                else
                {
                    m_platformPanel.ContainerView.Expand(root);
                }
                root.IsExpanded = !root.IsExpanded;
            }
            else if(obj.ActivatedObject is CPlatformTagNode leaf)
            {
                m_gamePanel.ContainerView.SetFocus();
            }
        }

        /// <summary>
		/// Handle game selection event
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void GameListView_OpenSelectedItem(MultilistViewItemEventArgs e)
        //private static void GameListView_OpenSelectedItem(ListViewItemEventArgs e)
        //private static void GameListView_OpenSelectedItem(CellActivatedEventArgs e)
        {
            CGame game = (CGame)e.Value;
            System.Diagnostics.Debug.WriteLine($"Selected game: {game.Name}");
            return;

            //GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem];
            //GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedRow];

            int width = 30;
            int height = 10;

            var buttons = new List<Button> ();
            var clicked = -1;

            var okeyButton = new Button ("Okay", is_default: true);
            okeyButton.Clicked += () =>
            {
                clicked = 0;
                Application.RequestStop();
            };
            buttons.Add(okeyButton);

            var closeButton = new Button ("Close", is_default: false);
            closeButton.Clicked += () =>
            {
                clicked = 1;
                Application.RequestStop();
            };
            buttons.Add(closeButton);

            // This tests dynamically adding buttons; ensuring the dialog resizes if needed and
            // the buttons are laid out correctly
            var dialog = new Dialog (game.Name, width, height, buttons.ToArray());

            Application.Run(dialog);
        }

        private static void GameListView_SelectedChanged(MultilistViewItemEventArgs e)
        //private static void GameListView_SelectedChanged(ListViewItemEventArgs e)
        //private static void GameListView_SelectedChanged(SelectedCellChangedEventArgs e)
        {
            /*
            if(!CSystemAttributeSQL.GetBoolValue(CSystemAttributeSQL.A_SHOW_GAME_INFO_PANEL))
            {
                return;
            }

            //if(m_gamePanel.ContentList.Count == 0)
            if(false)
            {
                m_infoPanel.FrameView.RemoveAll();
                return;
            }
            Game game = new Game("", 1, "", "", "", "");
            //GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem];
            //GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedRow];
            m_infoPanel.SwitchGameInfo(game);
            */
        }
    }
}
