using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NStack;
using Terminal.Gui;
using core;

namespace glc
{
	/// <summary>
	/// Game library tab containing the panels, listviews and data sources
	/// related to platform and game selection
	/// </summary>
	public class CLibraryTab : TabView.Tab
	{
		private static View m_container;

		private static FrameView m_platformPanel;
		private static ListView  m_platformListView;
		private static List<CPlatform> m_platformList;
		private static int m_platformSelection;

		private static FrameView m_gamePanel;
		private static ListView  m_gameListView;
		private static List<GameObject> m_gameList;
		private static int m_gameSelection;

		public CLibraryTab(List<CPlatform> platforms)
		{
			Text = "Library";
			m_platformList = platforms;

			// Construct our platform and game panels
			ConstructPlatformPanel();
			ConstructGamePanel(0);

			// Container to store all frames
			m_container = new View()
			{
				X = 0,
				Y = 0, // for menu
				Width = Dim.Fill(0),
				Height = Dim.Fill(0),
				CanFocus = false,
			};
			m_container.Add(m_platformPanel);
			m_container.Add(m_gamePanel);

			View = m_container;
		}

		/// <summary>
		/// Construct platform panel (left) and fetch platform data
		/// </summary>
		private static void ConstructPlatformPanel()
		{
			// Construct the actual panel
			m_platformPanel = new FrameView("Platforms")
			{
				X = 0,
				Y = 0, // for menu
				Width = 25,
				Height = Dim.Fill(0),
				CanFocus = true,
				Shortcut = Key.CtrlMask | Key.C
			};
			m_platformPanel.Title = $"{m_platformPanel.Title} ({m_platformPanel.ShortcutTag})";
			m_platformPanel.ShortcutAction = () => m_platformPanel.SetFocus();

			// Construct the platform list view
			//m_platformList = new List<CPlatform>();//CPlatform.GetPlatforms(true);
			m_platformListView = new ListView(new CPlatformListDataSource(m_platformList))
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
		private static void ConstructGamePanel(int platformFK)
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

	/// <summary>
	/// ListDataSource implementation for the GameObject list
	/// </summary>
	internal class CGameListDataSource : IListDataSource
	{
		private readonly int length;

		public List<GameObject> Games { get; set; }

		public bool IsMarked(int item) => false;

		public int Count => Games.Count;

		public int Length => length;

		public CGameListDataSource(List<GameObject> itemList)
		{
			Games = itemList;
			length = GetMaxLengthItem();
		}

		public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
		{
			container.Move(col, line);
			// Equivalent to an interpolated string like $"{Scenarios[item].Name, -widtestname}"; if such a thing were possible
			var s = String.Format (String.Format ("{{0,{0}}}", 0), Games[item].Title);
			RenderUstr(driver, $"{s}", col, line, width, start);
		}
		public void SetMark(int item, bool value)
		{
		}

		int GetMaxLengthItem()
		{
			if(Games?.Count == 0)
			{
				return 0;
			}

			int maxLength = 0;
			for(int i = 0; i < Games.Count; i++)
			{
				var s = String.Format (String.Format ("{{0,{0}}}", length), Games[i].Title);
				var sc = $"{s}  {Games[i].Title}";
				var l = sc.Length;
				if(l > maxLength)
				{
					maxLength = l;
				}
			}

			return maxLength;
		}

		// A slightly adapted method from: https://github.com/migueldeicaza/gui.cs/blob/fc1faba7452ccbdf49028ac49f0c9f0f42bbae91/Terminal.Gui/Views/ListView.cs#L433-L461
		private void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width, int start = 0)
		{
			int used = 0;
			int index = start;
			while(index < ustr.Length)
			{
				(var rune, var size) = Utf8.DecodeRune(ustr, index, index - ustr.Length);
				var count = Rune.ColumnWidth (rune);
				if(used + count >= width) break;
				driver.AddRune(rune);
				used += count;
				index += size;
			}

			while(used < width)
			{
				driver.AddRune(' ');
				used++;
			}
		}

		public IList ToList()
		{
			return Games;
		}
	}

	/// <summary>
	/// ListDataSource implementation for the PlatformObject list
	/// </summary>
	internal class CPlatformListDataSource : IListDataSource
	{
		private readonly int length;

		public List<CPlatform> Platforms { get; set; }

		public bool IsMarked(int item) => false;

		public int Count => Platforms.Count;

		public int Length => length;

		public CPlatform this[int index] => Platforms[index];

		public CPlatformListDataSource(List<CPlatform> itemList)
		{
			Platforms = itemList;
			length = GetMaxLengthItem();
		}

		public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
		{
			container.Move(col, line);
			// Equivalent to an interpolated string like $"{Scenarios[item].Name, -widtestname}"; if such a thing were possible
			var s = String.Format (String.Format ("{{0,{0}}}", 0), Platforms[item].Name);
			RenderUstr(driver, $"{s}", col, line, width, start);
		}
		public void SetMark(int item, bool value)
		{
		}

		int GetMaxLengthItem()
		{
			if(Platforms?.Count == 0)
			{
				return 0;
			}

			int maxLength = 0;
			for(int i = 0; i < Platforms.Count; i++)
			{
				var s = String.Format (String.Format ("{{0,{0}}}", length), Platforms[i].Name);
				var sc = $"{s}  {Platforms[i].Description}";
				var l = sc.Length;
				if(l > maxLength)
				{
					maxLength = l;
				}
			}

			return maxLength;
		}

		// A slightly adapted method from: https://github.com/migueldeicaza/gui.cs/blob/fc1faba7452ccbdf49028ac49f0c9f0f42bbae91/Terminal.Gui/Views/ListView.cs#L433-L461
		private void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width, int start = 0)
		{
			int used = 0;
			int index = start;
			while(index < ustr.Length)
			{
				(var rune, var size) = Utf8.DecodeRune(ustr, index, index - ustr.Length);
				var count = Rune.ColumnWidth (rune);
				if(used + count >= width) break;
				driver.AddRune(rune);
				used += count;
				index += size;
			}

			while(used < width)
			{
				driver.AddRune(' ');
				used++;
			}
		}

		public IList ToList()
		{
			return Platforms;
		}
	}
}
