using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NStack;
using Terminal.Gui;
using core;
using Terminal.Gui.Trees;

namespace glc
{
	public class CLibraryTab : TabView.Tab
    {
		private static View m_container;

		private static CPlatformPanel	m_platformPanel;
		private static CGamePanel		m_gamePanel;
		private static CGameInfoPanel	m_infoPanel;

		public CLibraryTab(List<CPlatform> platforms)
        {
			Text = "Library";

			// Construct the panels
			m_platformPanel = new CPlatformPanel(platforms, "Platforms", 0, 0, Dim.Percent(25), Dim.Fill(), true, Key.CtrlMask | Key.C);

			List<GameObject> gameList = (platforms.Count > 0) ? CGameSQL.LoadPlatformGames(platforms[0].ID).ToList() : new List<GameObject>();
			m_gamePanel	= new CGamePanel(gameList, "Games", Pos.Percent(25), 0, Dim.Fill(), Dim.Percent(60), true, Key.CtrlMask | Key.S);
			m_infoPanel = new CGameInfoPanel("", Pos.Percent(25), Pos.Percent(60), Dim.Fill(), Dim.Fill());

			// Hook up the triggers
			// Event triggers for the list view
			m_platformPanel.ContainerView.SelectionChanged += PlatformListView_SelectedChanged;
			m_platformPanel.ContainerView.ObjectActivated += PlatformListView_ObjectActivated;

			m_gamePanel.ContainerView.OpenSelectedItem += GameListView_OpenSelectedItem;
			m_gamePanel.ContainerView.SelectedItemChanged += GameListView_SelectedChanged;

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
			m_container.Add(m_infoPanel.FrameView);

			View = m_container;
		}

		/// <summary>
		/// Handle change in the platform list view
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void PlatformListView_SelectedChanged(object sender, SelectionChangedEventArgs<CPlatformNode> e)
        {
			var val = e.NewValue;
			if(val == null)
            {
				return;
            }

			if(val is PlatformRootNode)
            {
				PlatformRootNode node = (PlatformRootNode)val;
				m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(node.ID).ToList();
				m_gamePanel.ContainerView.Source = new CGameDataSource(m_gamePanel.ContentList);
			}
			else if(val is PlatformLeafNode)
            {
				PlatformLeafNode node = (PlatformLeafNode)val;
				if(node.Tag.ToLower() == "favourites") // TODO: change
                {
					m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(node.ID, true).ToList();
				}
				else
                {
					m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(node.ID, node.Tag).ToList();
				}
				m_gamePanel.ContainerView.Source = new CGameDataSource(m_gamePanel.ContentList);
			}
			else
            {
				return;
            }
		}

		private static void PlatformListView_ObjectActivated(ObjectActivatedEventArgs<CPlatformNode> obj)
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
			else if(obj.ActivatedObject is PlatformLeafNode leaf)
            {
				m_gamePanel.ContainerView.SetFocus();
			}
		}

		/// <summary>
		/// Handle game selection event
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void GameListView_OpenSelectedItem(ListViewItemEventArgs e)
		{
			if(m_gamePanel.ContentList.Count == 0)
            {
				return;
            }
			GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem];

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

		private static void GameListView_SelectedChanged(ListViewItemEventArgs e)
        {
			if(m_gamePanel.ContentList.Count == 0)
			{
				m_infoPanel.FrameView.RemoveAll();
				return;
			}
			GameObject game = m_gamePanel.ContentList[m_gamePanel.ContainerView.SelectedItem];
			m_infoPanel.SwitchGameInfo(game);
		}
	}
}
