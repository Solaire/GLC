﻿using core_2;
using core_2.Game;
using glc_2.UI.Dialog;
using glc_2.UI.Panels;
using glc_2.UI.Views;
using System.Collections.Generic;
using System.Xml.Linq;
using Terminal.Gui;
using Terminal.Gui.Trees;

namespace glc_2.UI.Tabs
{
    internal class CLibraryTab : TabView.Tab
    {
        // TEMP
        private int searchCounter = 0;
        private static bool gameInfoPanelSetting = true;
        // END TEMP

        private static View m_container;

        private static PlatformPanel   m_platformPanel;
        private static CGamePanel       m_gamePanel;
        private static CGameInfoPanel   m_gameInfoPanel;
        private static KeyBindingPanel m_keyBindingPanel;

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
            InitializeGameInfoPanel();
            InitialiseKeyBindingPanel();

            View = m_container;
            View.KeyDown += KeyDownHandler;
        }

        #region Panel initialisation methods

        private void InitialisePlatformPanel()
        {
            m_platformPanel = new PlatformPanel(new Box(0, 0, Dim.Percent(25), Dim.Percent(60)));
            m_platformPanel.ContainerView.SelectionChanged += PlatformListView_SelectedChanged;
            m_platformPanel.ContainerView.ObjectActivated += PlatformListView_ObjectActivated;

            m_container.Add(m_platformPanel.View);
        }

        private void InitialiseGamePanel()
        {
            DataManager.LoadPlatformGames(m_platformPanel.CurrentNode.ID);

            m_gamePanel = new CGamePanel(new Box(Pos.Percent(25), 0, Dim.Fill(), Dim.Percent(60)), DataManager.GetPlatformGames(m_platformPanel.CurrentNode.ID));
            m_gamePanel.ContainerView.OpenSelectedItem += GameListView_OpenSelectedItem;
            m_gamePanel.ContainerView.SelectedItemChanged += GameListView_SelectedChanged;

            m_container.Add(m_gamePanel.View);
        }

        private void InitializeGameInfoPanel()
        {
            if(DataManager.GetBoolSetting(CAppSettings.A_SHOW_GAME_INFO_PANEL, gameInfoPanelSetting))
            {
                m_gameInfoPanel = new CGameInfoPanel(new Box(Pos.Percent(25), Pos.Percent(60), Dim.Fill(), Dim.Fill()));
                m_container.Add(m_gameInfoPanel.View);
            }
            else
            {
                m_gamePanel.View.Height = Dim.Fill();
            }
        }

        private void InitialiseKeyBindingPanel()
        {
            Dictionary<Key, StatusItem> keyBindings = new Dictionary<Key, StatusItem>()
            {
                {
                    Key.CtrlMask | Key.F,
                    new StatusItem(Key.CtrlMask | Key.F, "~F~ Favourite", () =>
                    {
                        SelectedGameToggleFavourite();
                    })
                },
                {
                    Key.CtrlMask | Key.E,
                    new StatusItem(Key.CtrlMask | Key.E, "~E~ Edit", () =>
                    {
                        SelectedGameEdit();
                    })
                },
                {
                    Key.CtrlMask | Key.R,
                    new StatusItem(Key.CtrlMask | Key.R, "~R~ Set rating", () =>
                    {
                        SelectedGameSetRating();
                    })
                },
                {
                    Key.CtrlMask | Key.S,
                    new StatusItem(Key.CtrlMask | Key.S, "~S~ Search", () =>
                    {
                        GameSearch();
                        //GameScanner();
                    })
                },
            };

            m_keyBindingPanel = new KeyBindingPanel(new Box(0, Pos.Percent(60), Dim.Percent(25), Dim.Fill()), keyBindings);
            m_container.Add(m_keyBindingPanel.View);
        }

        #endregion Panel initialisation methods

        #region Event handlers

        private static void KeyDownHandler(View.KeyEventEventArgs a)
        {
            //if (a.KeyEvent.Key == Key.Tab || a.KeyEvent.Key == Key.BackTab) {
            //	// BUGBUG: Work around Issue #434 by implementing our own TAB navigation
            //	if (_top.MostFocused == _categoryListView)
            //		_top.SetFocus (_rightPane);
            //	else
            //		_top.SetFocus (_leftPane);
            //}

            // All key bindings have ctrl mask
            if((a.KeyEvent.Key & Key.CtrlMask) == Key.CtrlMask)
            {
                m_keyBindingPanel.PerformKeyAction(a.KeyEvent);
            }
            else if(a.KeyEvent.Key == (Key.D1))
            {
                // TODO: local search mode (shift + semicolon \ colon)
            }
        }

        /// <summary>
        /// Handle change in the platform list view
        /// </summary>
        /// <param name="e">The event argument</param>
        private static void PlatformListView_SelectedChanged(object sender, SelectionChangedEventArgs<PlatformTreeNode> e)
        {
            // Only process if acutally changed the node
            if(e.NewValue == null || e.NewValue == m_platformPanel.CurrentNode)
            {
                return;
            }

            // Moving from root to a tag of another root
            if(m_platformPanel.CurrentNode is PlatformRootNode root
                && e.NewValue is PlatformTagNode t
                && root != t.Parent)
            {
                DataManager.LoadPlatformGames(t.Parent.ID);
                m_gamePanel.ContainerView.Source = new CGameDataMultilistSource(DataManager.GetPlatformGames(t.Parent.ID));
                m_gamePanel.SingleListMode(t.Name);

                m_platformPanel.CurrentNode = e.NewValue;
                return;
            }

            m_platformPanel.CurrentNode = e.NewValue;
            if(m_platformPanel.CurrentNode is PlatformRootNode)
            {
                PlatformRootNode node = (PlatformRootNode)m_platformPanel.CurrentNode;

                // Root of the current tree. Switch back to multilist mode
                if(m_platformPanel.CurrentNode is PlatformTagNode tag && node.ID == tag.Parent.ID)
                {
                    m_gamePanel.MultiListMode();
                    return;
                }

                // Switching to new root. Load the platform games, if not done already
                // NOTE: Platform should be poulated once - extra calls should be no op
                DataManager.LoadPlatformGames(node.ID);
                m_gamePanel.ContainerView.Source = new CGameDataMultilistSource(DataManager.GetPlatformGames(node.ID));
                return;
            }

            // Moving to a tag node in the current root. Switch to single list mode
            m_gamePanel.SingleListMode(m_platformPanel.CurrentNode.Name);
        }

        private static void PlatformListView_ObjectActivated(ObjectActivatedEventArgs<PlatformTreeNode> obj)
        {
            if(obj.ActivatedObject is PlatformRootNode root)
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
            else if(obj.ActivatedObject is PlatformTagNode leaf)
            {
                m_gamePanel.ContainerView.SetFocus();
            }
        }

        /// <summary>
		/// Handle game selection event
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void GameListView_OpenSelectedItem(MultilistViewItemEventArgs e)
        {
            if(!DataManager.LaunchGame((Game)e.Value))
            {
                throw new System.Exception("Could not launch game");
            }
            Application.RequestStop();

            /*
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
            */
        }

        private static void GameListView_SelectedChanged(MultilistViewItemEventArgs e)
        {
            if(!DataManager.GetBoolSetting(CAppSettings.A_SHOW_GAME_INFO_PANEL, gameInfoPanelSetting))
            {
                return;
            }

            Game game = (Game)e.Value;
            m_gameInfoPanel.SetGameInfo(game);
        }

        #endregion Event handlers

        #region Key binding actions

        // TODO
        private void SelectedGameToggleFavourite()
        {
            if(!m_gamePanel.View.HasFocus)
            {
                return;
            }

            Game selectedItem = m_gamePanel.ContainerView.GetSelectedItem();
            selectedItem.ToggleFavourite();

            System.Diagnostics.Debug.WriteLine($"Game: {selectedItem.Name}, Favourite: {selectedItem.IsFavourite}");
        }

        private void SelectedGameEdit()
        {
            if(!m_gamePanel.View.HasFocus)
            {
                return;
            }

            Game selectedItem = m_gamePanel.ContainerView.GetSelectedItem();
            selectedItem.ToggleFavourite();

            System.Diagnostics.Debug.WriteLine($"Game: {selectedItem.Name}, Entered edit mode");
        }

        private void SelectedGameSetRating()
        {
            if(!m_gamePanel.View.HasFocus)
            {
                return;
            }

            Game selectedItem = m_gamePanel.ContainerView.GetSelectedItem();
            selectedItem.ToggleFavourite();

            System.Diagnostics.Debug.WriteLine($"Game: {selectedItem.Name}, Entered rating mode");
        }

        private void GameSearch()
        {
            string searchTerm = string.Empty;
            EditStringDlg searchDlg = new EditStringDlg("Game Search", string.Empty);
            if(!searchDlg.Run())
            {
                return;
            }

            // TODO: Better implementation
            searchTerm = searchDlg.NewValue;
            DataManager.GameSearch(searchTerm);
            m_platformPanel.SetSearchResults(searchTerm);
            if(DataManager.GetBoolSetting(CAppSettings.SHOW_SEARCH_IN_DLG, true))
            {
                Terminal.Gui.Dialog dlg = new Terminal.Gui.Dialog(searchTerm)
                {
                    X = 4,
                    Y = 4,
                    Width  = Dim.Percent(80),
                    Height = Dim.Percent(80)
                };

                CMultilistView view = new CMultilistView()
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                    CanFocus = true,
                };

                // TODO: Replace -1 with search constant
                DataManager.LoadPlatformGames(-1);
                view.Source = new CGameDataMultilistSource(DataManager.GetPlatformGames(-1));
                view.SingleListMode(searchTerm);
                dlg.Add(view);
                Application.Run(dlg);
            }
        }

        private void GameScanner()
        {
            DataManager.GameScanner();
        }

        #endregion Key binding actions
    }
}