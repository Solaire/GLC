using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NStack;
using Terminal.Gui;
using core;

namespace neo_glc
{
    class CSettingsTab : TabView.Tab
    {
		private static View m_container;
		private static List<CPlatform> m_platformList;

		private static FrameView m_settingTypePanel;
		private static ListView  m_settingTypeListView;
		private static List<string> m_settingTypes;
		private static int m_settingTypeSelection;

		private static FrameView m_settingsPanel;
		private static ListView  m_settingsListView;
		private static List<GameObject> m_settingsList;
		private static int m_settingSelection;

		public CSettingsTab(List<CPlatform> platforms)
        {
			Text = "Settings";
			m_platformList = platforms;

			// Construct our platform and game panels
			ConstructSettingGroupPanel();
			ConstructSettingsPanel();

			// Container to store all frames
			m_container = new View()
			{
				X = 0,
				Y = 0, // for menu
				Width = Dim.Fill(0),
				Height = Dim.Fill(0),
				CanFocus = false,
			};
			m_container.Add(m_settingTypePanel);
			m_container.Add(m_settingsPanel);

			View = m_container;
		}

		/// <summary>
		/// Construct platform panel (left) and fetch platform data
		/// </summary>
		private static void ConstructSettingGroupPanel()
		{
			// Construct the actual panel
			m_settingTypePanel = new FrameView("Platforms")
			{
				X = 0,
				Y = 0, // for menu
				Width = 25,
				Height = Dim.Fill(0),
				CanFocus = true,
				Shortcut = Key.CtrlMask | Key.C
			};
			m_settingTypePanel.Title = $"{m_platformPanel.Title} ({m_platformPanel.ShortcutTag})";
			m_settingTypePanel.ShortcutAction = () => m_settingTypePanel.SetFocus();

			// Construct the platform list view
			//m_platformList = new List<CPlatform>();//CPlatform.GetPlatforms(true);
			m_settingTypeListView = new ListView(new CPlatformListDataSource(m_platformList))
			{
				X = 0,
				Y = 0,
				Width = Dim.Fill(0),
				Height = Dim.Fill(0),
				AllowsMarking = false,
				CanFocus = true,
			};

			// Event triggers for the list view
			m_platformListView.OpenSelectedItem += (a) =>
			{
				m_gamePanel.SetFocus();
			};
			m_platformListView.SelectedItemChanged += PlatformListView_SelectedChanged;

			// Add to our container
			m_platformPanel.Add(m_platformListView);
		}

		/// <summary>
		/// Construct the games panel (right)
		/// Populate the panel with games from specified platform
		/// </summary>
		private static void ConstructSettingsPanel()
		{
			// construct the containing frame
			m_gamePanel = new FrameView("Games")
			{
				X = 25,
				Y = 0, // for menu
				Width = Dim.Fill(),
				Height = Dim.Fill(0),
				CanFocus = true,
				Shortcut = Key.CtrlMask | Key.S
			};
			m_gamePanel.Title = $"{m_gamePanel.Title} ({m_gamePanel.ShortcutTag})";
			m_gamePanel.ShortcutAction = () => m_gamePanel.SetFocus();

			// Construct the game list view
			//m_gameList = (platformFK > 0) ? CGame.GetPlatformGames(platformFK) : CGame.GetAllGames().ToList();
			m_gameList = (platformFK > 0) ? CGameSQL.LoadPlatformGames(platformFK).ToList() : CGameSQL.LoadAllGames().ToList();
			m_gameListView = new ListView(new CGameListDataSource(m_gameList))
			{
				X = 0,
				Y = 0,
				Width = Dim.Fill(0),
				Height = Dim.Fill(0),
				AllowsMarking = false,
				CanFocus = true,
			};

			// Add triggers
			m_gameListView.OpenSelectedItem += GameListView_OpenSelectedItem;

			// Add to the containing panel
			m_gamePanel.Add(m_gameListView);

			// Set current selection to 0
			m_gameSelection = 0;
		}

		/// <summary>
		/// Handle change in the platform list view
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void PlatformListView_SelectedChanged(ListViewItemEventArgs e)
		{
			if(m_platformList.Count == 0)
			{
				return;
			}
			m_gameSelection = (m_platformSelection != m_platformListView.SelectedItem) ? 0 : m_gameListView.SelectedItem;

			m_platformSelection = m_platformListView.SelectedItem;
			CPlatform platform = m_platformList[m_platformSelection];
			m_gameList = CGameSQL.LoadPlatformGames(platform.ID).ToList();
			m_gameListView.Source = new CGameListDataSource(m_gameList);
			m_gameListView.SelectedItem = m_gameSelection;
		}

		/// <summary>
		/// Handle game selection event
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void GameListView_OpenSelectedItem(ListViewItemEventArgs e)
		{
			GameObject game = m_gameList[m_gameListView.SelectedItem];

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
