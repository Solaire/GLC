using System.Linq;
using System.Collections.Generic;

using core.Platform;
using core.Game;

using Terminal.Gui;
using Terminal.Gui.Trees;
using System;
using core.Tag;
using core;
using core.SystemAttribute;
using static Terminal.Gui.TableView;
using glc.UI.Views;

namespace glc.UI.Library
{
	public class CLibraryTab : TabView.Tab
    {
		private static View m_container;

		private static CPlatformTreePanel   m_platformPanel;
		private static CGamePanel			m_gamePanel;
		private static CGameInfoPanel		m_infoPanel;
		private static CKeyBindingPanel		m_keyBiningPanel;

		public CLibraryTab(List<CBasicPlatform> platforms)
			: base()
        {
			Text = "Library";

			// Construct the panels
			//List<GameObject> gameList = (platforms.Count > 0) ? CGameSQL.LoadPlatformGames(platforms[0].PrimaryKey).ToList() : new List<GameObject>();
			Dictionary<string, List<GameObject>> gameDictionary = (platforms.Count > 0) ? CGameSQL.LoadPlatformGamesAsDict(platforms[0].PrimaryKey) : new Dictionary<string, List<GameObject>>();
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
				new StatusItem(Key.Enter, "~Entry~ Start game", () =>
				{
					//GameListView_OpenSelectedItem(new ListViewItemEventArgs(m_gamePanel.ContainerView.SelectedItem, m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem]));
					//GameListView_OpenSelectedItem(new CellActivatedEventArgs(m_gamePanel.ContainerView.Table, m_gamePanel.ContainerView.SelectedRow, m_gamePanel.ContainerView.SelectedColumn));
				}),
			};

			m_platformPanel		= new CPlatformTreePanel(platforms, "Platforms", 0, 0, Dim.Percent(25), Dim.Percent(60), true);
			m_gamePanel			= new CGamePanel(gameDictionary, "Games", Pos.Percent(25), 0, Dim.Fill(), Dim.Percent(60), true);
			m_keyBiningPanel	= new CKeyBindingPanel(keyBindings, "Key bindings", 0, Pos.Percent(60), Dim.Percent(25), Dim.Fill());

			if(CSystemAttributeSQL.GetBoolValue(CSystemAttributeSQL.A_SHOW_GAME_INFO_PANEL))
            {
				m_infoPanel = new CGameInfoPanel("Game information", Pos.Percent(25), Pos.Percent(60), Dim.Fill(), Dim.Fill());
			}
			else
            {
				m_gamePanel.FrameView.Height = Dim.Fill();
			}

			// Hook up the triggers
			// Event triggers for the list view
			m_platformPanel.ContainerView.SelectionChanged += PlatformListView_SelectedChanged;
			m_platformPanel.ContainerView.ObjectActivated += PlatformListView_ObjectActivated;

			m_gamePanel.ContainerView.OpenSelectedItem += GameListView_OpenSelectedItem;
			m_gamePanel.ContainerView.SelectedItemChanged += GameListView_SelectedChanged;

			//m_gamePanel.ContainerView.CellActivated += GameListView_OpenSelectedItem;
			//m_gamePanel.ContainerView.SelectedCellChanged += GameListView_SelectedChanged;

			// Container to store all frames
			m_container = new View()
			{
				X = 0,
				Y = 0, // for menu
				Width = Dim.Fill(0),
				Height = Dim.Fill(0),
				CanFocus = false,
			};
			m_container.Add(m_platformPanel.FrameView);
			m_container.Add(m_gamePanel.FrameView);
			m_container.Add(m_keyBiningPanel.FrameView);

			if(CSystemAttributeSQL.GetBoolValue(CSystemAttributeSQL.A_SHOW_GAME_INFO_PANEL))
			{
				m_container.Add(m_infoPanel.FrameView);
			}

			View = m_container;

			View.KeyDown += KeyDownHandler;
		}

		/// <summary>
		/// Handle change in the platform list view
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void PlatformListView_SelectedChanged(object sender, SelectionChangedEventArgs<IPlatformTreeNode> e)
        {
			var val = e.NewValue;
			if(val == null)
            {
				return;
            }

			if(val is PlatformRootNode) // Switch to new multilist
            {
				PlatformRootNode node = (PlatformRootNode)val;
				m_gamePanel.NewMultilistSource(CGameSQL.LoadPlatformGamesAsDict(node.ID));
			}
			else if(val is PlatformTagNode)
            {
				// TODO: Handle the following cases:
				//		Game search
				//		Favourites

				PlatformTagNode node = (PlatformTagNode)val;
				if(node.ID == (int)SpecialPlatformID.cSearch) // Handle search
				{
					m_gamePanel.ContentList = CGameSQL.GameSearch(node.Name).ToList();
				}
				else if(node.ID == (int)SpecialPlatformID.cFavourites) // Handle favourite games
				{
					m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(node.PlatformID, true).ToList();
				}
				if(node.ID > 0)
                {
					m_gamePanel.SingleListMode(node.Name);
                }
            }

			/*
			if(val is PlatformRootNode)
            {
				PlatformRootNode node = (PlatformRootNode)val;
				m_gamePanel.m_contentDictionary = CGameSQL.LoadPlatformGamesAsDict(node.ID);
				m_gamePanel.singleListMode = false;

				//m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(node.ID).ToList();
				//m_gamePanel.ContainerView.Source = new CGameDataSource(m_gamePanel.ContentList);

				//m_gamePanel.UpdateTable();
			}
			else if(val is PlatformTagNode)
            {
				PlatformTagNode node = (PlatformTagNode)val;
				if(node.ID == (int)SpecialPlatformID.cSearch) // Handle search
				{
					m_gamePanel.ContentList = CGameSQL.GameSearch(node.Name).ToList();
                }
				else if(node.ID == (int)SpecialPlatformID.cFavourites) // Handle favourite games
                {
					m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(node.PlatformID, true).ToList();
				}
				else if(node.ID > 0) // Handle platform nodes
                {
					m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(node.PlatformID, node.Name).ToList();
				}

				m_gamePanel.singleSublist = val.Name;
				//m_gamePanel.ContainerView.Source = new CGameDataSource(m_gamePanel.ContentList);
				//m_gamePanel.UpdateTable();
			}
			else
            {
				return;
            }
			*/
		}

		private static void PlatformListView_ObjectActivated(ObjectActivatedEventArgs<IPlatformTreeNode> obj)
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
		//private static void GameListView_OpenSelectedItem(ListViewItemEventArgs e)
		//private static void GameListView_OpenSelectedItem(CellActivatedEventArgs e)
		{
			GameObject game = (GameObject)e.Value;
			System.Diagnostics.Debug.WriteLine($"Selected game: {game.Title}");
			return;

			if(m_gamePanel.ContentList.Count == 0)
            {
				return;
            }
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
			var dialog = new Dialog (game.Title, width, height, buttons.ToArray());

			Application.Run(dialog);
		}

		private static void GameListView_SelectedChanged(MultilistViewItemEventArgs e)
		//private static void GameListView_SelectedChanged(ListViewItemEventArgs e)
		//private static void GameListView_SelectedChanged(SelectedCellChangedEventArgs e)
        {
			if(!CSystemAttributeSQL.GetBoolValue(CSystemAttributeSQL.A_SHOW_GAME_INFO_PANEL))
			{
				return;
            }

			if(m_gamePanel.ContentList.Count == 0)
			{
				m_infoPanel.FrameView.RemoveAll();
				return;
			}
			GameObject game = new GameObject();
			//GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem];
			//GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedRow];
			m_infoPanel.SwitchGameInfo(game);
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

			// Game search
			if(a.KeyEvent.Key == (Key.CtrlMask | Key.F))
            {
				GameSearch();
            }

			// Sanity check
			else if((a.KeyEvent.Key & Key.CtrlMask) != Key.CtrlMask)
            {
				m_keyBiningPanel.PerformKeyAction(a);
			}
		}

		private static void GameSearch()
        {
			string searchTerm = "";
			CEditStringDlg searchDlg = new CEditStringDlg("Game search", searchTerm);
			if(searchDlg.Run(ref searchTerm))
            {
				m_platformPanel.SetSearchResults(searchTerm);
				if(CSystemAttributeSQL.GetBoolValue(CSystemAttributeSQL.A_SHOW_SEARCH_IN_DIALOG))
                {
					// TODO: after better game list frame
					//CGameListDlg gameListDlg = new CGameListDlg(searchTerm);
					//gameListDlg.Run();
				}
            }
        }

		private static void SelectedGameToggleFavourite()
        {
			if(m_gamePanel.ContentList.Count == 0)
			{
				return;
			}
			GameObject game = new GameObject();
			//GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem];
			//GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedRow];
			game.ToggleFavourite();

			if(CSystemAttributeSQL.GetBoolValue(CSystemAttributeSQL.A_SHOW_GAME_INFO_PANEL))
            {
				m_infoPanel.SwitchGameInfo(game); // TODO: Doesn't fully update
            }
		}

		// Create a large dialog
		private static void SelectedGameEdit()
        {
			if(m_gamePanel.ContentList.Count == 0)
			{
				return;
			}

			GameObject game = new GameObject();
			//GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem];
			//GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedRow];
			List<TagObject> tempTagList = CTagSQL.GetTagsforPlatform(game.PlatformFK);
			List<IDataNode> currentTags = new List<IDataNode>(tempTagList.Cast<IDataNode>());

			CEditGameInfoDlg editGameInfoDlg = new CEditGameInfoDlg(game, currentTags);
			if(editGameInfoDlg.Run(ref game))
			{
				game.Update();
			}
			/*
			if(m_gamePanel.ContentList.Count == 0)
			{
				return;
			}

			GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem];
			int rating = 0;
			CEditGameInfoDlg editGameInfoDlg = new CEditGameInfoDlg(game);
			if(editGameInfoDlg.Run(ref game))
			{
				game.Update();
			}
			*/
		}

		// TODO: Save to database
		// TODO: Neaten up
		private static void SelectedGameSetRating()
        {
			if(m_gamePanel.ContentList.Count == 0)
			{
				return;
			}

			GameObject game = new GameObject();
			//GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem];
			//GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedRow];
			int rating = 0;
			CEditRatingDlg ratingDlg = new CEditRatingDlg($"{game.Title} - rating", rating);
			if(ratingDlg.Run(ref rating))
			{
				game.UpdateRating(rating);
			}
		}
	}
}

// TODO:
// - Game edit dialog
// * Game info + list refresh
// * Game list to include favourite + rating information
// * Game list for root platforms (grouped by tag)
// * Game sorting + dialog
// - Setting to disable game info panel
// - Setting to display search results in a new dialog
// * Predefined tags (favourites, if there are any)