#define TREE

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

		public CLibraryTab(List<CPlatform> platforms)
        {
			Text = "Library";

			// Construct the panels
			m_platformPanel = new CPlatformPanel(platforms, "Platforms", 0, 0, 50, Dim.Fill(), true, Key.CtrlMask | Key.C);

			List<GameObject> gameList = (platforms.Count > 0) ? CGameSQL.LoadPlatformGames(platforms[0].ID).ToList() : new List<GameObject>();
			m_gamePanel		= new CGamePanel(gameList, "Games", 50, 0, Dim.Fill(), Dim.Fill(), true, Key.CtrlMask | Key.S);

			// Hook up the triggers
			// Event triggers for the list view
			//m_platformPanel.ContainerView.OpenSelectedItem += (a) =>
			/*
			m_platformPanel.ContainerView.ObjectActivated += (a) =>
			{
				m_gamePanel.FrameView.SetFocus();
			};
			*/
			//m_platformPanel.ContainerView.SelectedItemChanged += PlatformListView_SelectedChanged;
			m_platformPanel.ContainerView.SelectionChanged += PlatformListView_SelectedChanged;
			m_platformPanel.ContainerView.ObjectActivated += M_containerView_ObjectActivated;

			m_gamePanel.ContainerView.OpenSelectedItem += GameListView_OpenSelectedItem;

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

			View = m_container;
		}

		/// <summary>
		/// Handle change in the platform list view
		/// </summary>
		/// <param name="e">The event argument</param>
#if TREE
		private static void PlatformListView_SelectedChanged(object sender, SelectionChangedEventArgs<CPlatformNode> e)
        {
			var val = e.NewValue;

			if(m_platformPanel.ContentList.Count == 0 || val == null || !(val is PlatformRootNode))
			{
				return;
			}
			PlatformRootNode node = (PlatformRootNode)val;

			//m_gamePanel.ListSelection = (m_platformPanel.ListSelection != m_platformPanel.ContainerView.SelectedItem) ? 0 : m_gamePanel.ContainerView.SelectedItem;

			//m_platformPanel.ListSelection = m_platformPanel.ContainerView.SelectedItem;
			//CPlatform platform = m_platformPanel.ContentList[m_platformPanel.ListSelection];
			m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(node.ID).ToList();
			m_gamePanel.ContainerView.Source = new CGameDataSource(m_gamePanel.ContentList);
			m_gamePanel.ContainerView.SelectedItem = m_gamePanel.ListSelection;
		}

		private static void M_containerView_ObjectActivated(ObjectActivatedEventArgs<CPlatformNode> obj)
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
#else
		private static void PlatformListView_SelectedChanged(SelectionChangedEventArgs<string> e)
		{
			if(m_platformPanel.ContentList.Count == 0)
			{
				return;
			}
			m_gamePanel.ListSelection = (m_platformPanel.ListSelection != m_platformPanel.ContainerView.SelectedItem) ? 0 : m_gamePanel.ContainerView.SelectedItem;

			m_platformPanel.ListSelection = m_platformPanel.ContainerView.SelectedItem;
			CPlatform platform = m_platformPanel.ContentList[m_platformPanel.ListSelection];
			m_gamePanel.ContentList = CGameSQL.LoadPlatformGames(platform.ID).ToList();
			m_gamePanel.ContainerView.Source = new CGameDataSource(m_gamePanel.ContentList);
			m_gamePanel.ContainerView.SelectedItem = m_gamePanel.ListSelection;
		}
#endif // TREE

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
			GameObject game = m_gamePanel.ContentList[m_gamePanel.ListSelection];

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
	}
}
