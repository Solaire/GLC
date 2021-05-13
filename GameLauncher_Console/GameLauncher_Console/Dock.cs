using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Logger;
using Microsoft.Win32;

namespace GameLauncher_Console
{
	/// <summary>
	/// Main program logic - this is where all classes meet and get used
	/// </summary>
	public class CDock
	{
		public const int COLUMN_CUSHION = 2;
		public const int INPUT_BOTTOM_CUSHION = 2;
		public const int INPUT_ITEM_CUSHION = 2;
		public const int INSTRUCT_CUSHION = 1;
		public const int IMG_BORDER_X_CUSHION = 1;
		public const int IMG_BORDER_Y_CUSHION = 0;
		public const int ICON_LEFT_CUSHION = 0;
		public const int ICON_RIGHT_CUSHION = 1;
		public const string SETTINGS_TITLE = "Settings";
		public const CompareOptions IGNORE_ALL = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols;
		public const StringComparison IGNORE_CASE = StringComparison.CurrentCultureIgnoreCase;

		public static readonly string FILENAME = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);

		CConsoleHelper m_dockConsole;
		private int nColumnCount = 2;
		public static int m_nSelectedPlatform = -1;
		public static int m_nSelectedCategory = -1; // for future
		public static int m_nSelectedGame = -1;
		public static int m_nCurrentSelection = 0;

		public static bool noInteractive = false;
		public static Size imgSize;
		public static Size iconSize;
		public static Point imgLoc;

		/// <summary>
		/// Array of string containing the content of the help screen.
		/// </summary>
		private readonly string[] m_helpLines =
		{
		//  0|-------|---------|---------|---------|---------|---------|---------|---------|80
			" This program will scan your system for installed video games and display",
			" them as a list. The following platforms are supported:",
			" * Steam * Epic Games Launcher * GOG Galaxy * Ubisoft Connect * Battle.net",
			" * Origin * Amazon Games * Big Fish Games",
			"",
			" The games list and configuration are stored in .json files in the same folder",
			" as this program. You can manually add games by placing a shortcut (.lnk) in",
			" the \'customGames\' subfolder."
		};

		/// <summary>
		/// Constructor
		/// </summary>
		public CDock()
		{ }

		/// <summary>
		/// Run the main program loop.
		/// Return when game is launched or the user decided to exit.
		/// </summary>
		public void MainLoop(string[] args)
		{
			CJsonWrapper.ImportFromJSON(out CConfig.Configuration config, out CConfig.Hotkeys keys, out CConfig.Colours cols, out List<CGameData.CMatch> matches);
			CGameFinder.CheckCustomFolder();

			if ((bool)config.onlyCmdLine) noInteractive = true;
			int gameIndex = -1;
			string gameSearch = "";
			foreach (string arg in args)
			{
				gameSearch += arg + " ";
			}
			if (args.Length > 0)
			{
				if (gameSearch[0] == '/')
				{
					gameSearch = gameSearch.Substring(1);
					if (gameSearch[0].Equals('?') || gameSearch[0].Equals('h') || gameSearch[0].Equals('H'))
						noInteractive = true;
					else if (gameSearch[0].Equals('s') || gameSearch[0].Equals('S'))
					{
						CLogger.LogInfo("Scanning for games...");
						Console.Write("Scanning for games");  // ScanGames() will add dots for each platform
						CRegScanner.ScanGames((bool)config.onlyCustom);
						return;
					}
					else if (gameSearch[0].Equals('c') || gameSearch[0].Equals('C'))
					{
						if ((bool)config.onlyCmdLine)
						{
							CLogger.LogInfo("Switching {0}=false...", CConfig.CFG_USECMD);
							Console.WriteLine("Switching {0}=false...", CConfig.CFG_USECMD);
							config.onlyCmdLine = false;
							CJsonWrapper.ExportConfig(config);
						}
						else
						{
							CLogger.LogInfo("Switching {0}=true...", CConfig.CFG_USECMD);
							Console.WriteLine("Switching {0}=true...", CConfig.CFG_USECMD);
							config.onlyCmdLine = true;
							CJsonWrapper.ExportConfig(config);
						}
						return;
					}
					else if (gameSearch[0].Equals('p') || gameSearch[0].Equals('P'))
					{
						if (PathEnvironmentUpdate.Add(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), false))
						{
							CLogger.LogInfo("Added program location to PATH environment variable.");
							Console.WriteLine("Added {0}.exe location to PATH environment variable.", FILENAME);
						}
						else
						{
							SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogWarn("Unable to add program location to PATH!");
							Console.WriteLine("Unable to add program location to PATH!");
							Console.ResetColor();
						}
						return;
					}
					else
					{
						if (Int32.TryParse(gameSearch, out gameIndex))
						{
							if (gameIndex > 0 && gameIndex <= matches.ToArray().Length)
							{
								CLogger.LogInfo("Select {0} from prior search of {1} matches", gameIndex, matches.ToArray().Length);
							}
							else
							{
								SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("Invalid search index number!");
								Console.WriteLine("{0} is an invalid search index number!", gameIndex);
								Console.ResetColor();
								return;
							}
						}
						else
                        {
							SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogWarn("Invalid parameter!");
							Console.WriteLine("/{0}is an invalid parameter!", gameSearch);
							Console.ResetColor();
							return;
						}
					}
					gameSearch = "";
				}
				else
				{
					gameSearch = gameSearch.Substring(0, gameSearch.Length - 1);
					CLogger.LogInfo("Search from command line: {0}", gameSearch);
				}
			}

			// Some shell hosts, e.g., Git Bash, Mingw, MSYS2, won't support interactive mode properly
			// PowerCmd displays no output at all

			// Additionally, displaying images on the console is not possible with all shell hosts.
			// I've tested a number of alternative shells. These work (probably because they leverage conhost.exe):
			//   4NT, Clink, Cmder, Far Manager, Git CMD, Midnight Commander, ZTreeWin
			// These do not:
			//   Console2, FireCMD, Take Command (tcmd), Windows Terminal
			// Note that using Take Command (tcc) works, though if you run it through ConEmu some keys aren't supported

			int imgMargin = 0;

			CheckShellCapabilities(ref config, out string parent);

			if ((int)config.imageSize > 0)
				imgMargin = (int)config.imageSize + COLUMN_CUSHION + ((bool)config.imageBorder ? IMG_BORDER_X_CUSHION : 0);
			try
			{
				/*
				req'd width for columns, assuming default values, e.g., image size = 16:
				2 = 72+
				3 = 99+
				4 = 126+
				...
				*/
				if (!(bool)config.listOutput)
					nColumnCount = (Console.WindowWidth - imgMargin) / ((int)config.columnSize + COLUMN_CUSHION);
				else nColumnCount = 1;
				if (nColumnCount <= 1) // shrink image to fit
				{
					nColumnCount = 1;
					config.imageBorder = false;
					if ((int)config.imageSize > 0)
						config.imageSize = Math.Min((int)config.imageSize, Console.WindowWidth / 2);
				}
			}
			catch (Exception e)
			{
				if (!(bool)config.onlyCmdLine)
				{
					SetFgColour(cols.errorCC, cols.errorLtCC);
					CLogger.LogError(e, "ERROR: Couldn't get console width. Your terminal host does not support interactive mode.");
					Console.WriteLine("ERROR: Your terminal host does not support interactive mode.");  // e.g., Git Bash, Mingw, MSYS2 (though strangely mintty works fine with Cygwin)
					Console.ResetColor();
					noInteractive = true;
				}
			}

			if (noInteractive && gameIndex < 1 && string.IsNullOrEmpty(gameSearch))
			{
				/*
				if (!(bool)config.onlyCmdLine)
				{
					SetFgColour(cols.errorCC, cols.errorLtCC);
					CLogger.LogWarn("ERROR: Interactive mode is disabled.");
					Console.WriteLine("ERROR: Interactive mode is disabled.");
					Console.ResetColor();
				}
				*/
				DisplayUsage(cols, parent);
				return;
			}

			if ((int)config.imageSize > 0)
				CConsoleImage.GetImageProperties((int)config.imageSize, (int)config.imagePosition, out imgSize, out imgLoc);
			if ((int)config.iconSize > 0)
				CConsoleImage.GetIconSize((int)config.iconSize, out iconSize);
			if ((bool)config.typeInput && CConsoleHelper.m_ConsoleState == CConsoleHelper.ConsoleState.cState_Unknown)
				CConsoleHelper.m_ConsoleState = CConsoleHelper.ConsoleState.cState_Insert;
			else CConsoleHelper.m_ConsoleState = CConsoleHelper.ConsoleState.cState_Navigate;
			if ((bool)config.listOutput && CConsoleHelper.m_MenuType == CConsoleHelper.MenuType.cType_Unknown)
				CConsoleHelper.m_MenuType = CConsoleHelper.MenuType.cType_List;
			else
				CConsoleHelper.m_MenuType = (nColumnCount > 1) ? CConsoleHelper.MenuType.cType_Grid : CConsoleHelper.MenuType.cType_List;
			if ((bool)config.lightMode && CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Unknown)
				CConsoleHelper.m_LightMode = CConsoleHelper.LightMode.cColour_Light;
			else CConsoleHelper.m_LightMode = CConsoleHelper.LightMode.cColour_Dark;
			if ((int)config.imageSize > 0 || (int)config.iconSize > 0)
				CConsoleHelper.m_ImageMien = CConsoleHelper.ImageMien.cImage_Enabled;
			else CConsoleHelper.m_ImageMien = CConsoleHelper.ImageMien.cImage_Disabled;
			if ((bool)config.alphaSort)
				CConsoleHelper.m_SortMethod = CConsoleHelper.SortMethod.cSort_Alpha;
			else CConsoleHelper.m_SortMethod = CConsoleHelper.SortMethod.cSort_Freq;

			m_dockConsole = new CConsoleHelper(nColumnCount, (Console.WindowWidth - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount); //, state, type, mode, method);

			for (; ; )
			{
				m_nSelectedGame = -1;
				CGameData.CGame selectedGame = null;
				int nSelectionCode = -1;

				if (gameIndex < 1)
				{
					MenuSwitchboard(config, keys, cols, ref gameSearch, ref nSelectionCode);
					CLogger.LogDebug("MenuSwitchboard:{0},{1}", nSelectionCode, m_nCurrentSelection);
				}
				else
				{
					m_nSelectedGame = (int)Array.FindIndex(
						CGameData.GetPlatformTitles(CGameData.GamePlatform.All).ToArray(),
						s => s.Equals(matches.ToArray()[gameIndex - 1].m_strTitle, IGNORE_CASE));
					if (m_nSelectedGame > -1)
						nSelectionCode = (int)CConsoleHelper.DockSelection.cSel_Default;
					else
						nSelectionCode = (int)CConsoleHelper.DockSelection.cSel_Exit;
				}

				// Game selection not required

				switch ((CConsoleHelper.DockSelection)nSelectionCode)
				{
					case CConsoleHelper.DockSelection.cSel_Exit:
						Console.ResetColor();
						Console.CursorVisible = true;
						if (!(bool)config.onlyCmdLine)
						{
							if (!(bool)config.ignoreChanges) CJsonWrapper.ExportConfig(config);
							Console.Clear();
						}
						return;

					case CConsoleHelper.DockSelection.cSel_Fail:
						Console.ResetColor();
						Console.CursorVisible = true;
						if (!(bool)config.onlyCmdLine) Console.Clear();
						SetFgColour(cols.errorCC, cols.errorLtCC);
						CLogger.LogWarn("ERROR: Console height too small to support interactive mode.");
						Console.WriteLine("ERROR: Console height too small to support interactive mode.");
						DisplayUsage(cols, parent);
						return;

					case CConsoleHelper.DockSelection.cSel_Redraw:
						continue;

					case CConsoleHelper.DockSelection.cSel_Back:
						m_nSelectedPlatform = -1;
						m_nCurrentSelection = 0;
						continue;

					case CConsoleHelper.DockSelection.cSel_Help:
						DisplayHelp(Console.WindowHeight, config, cols);
						continue;

					case CConsoleHelper.DockSelection.cSel_Rescan: // Rescan the game list
						Console.ResetColor();
						CLogger.LogInfo("Scanning for games...");
						try
						{
							Console.SetCursorPosition(0, Console.WindowHeight - INPUT_BOTTOM_CUSHION);
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						Console.Write("Scanning for games");
						CRegScanner.ScanGames((bool)config.onlyCustom);
						continue;

					case CConsoleHelper.DockSelection.cSel_Input: // Toggle arrows/typing input
						if (CConsoleHelper.m_ConsoleState == CConsoleHelper.ConsoleState.cState_Navigate)
						{
							CLogger.LogInfo("Switching to insert input...");
							config.typeInput = true;
						}
						else
						{
							CLogger.LogInfo("Switching to navigate input...");
							config.typeInput = false;
						}
						CConsoleHelper.SwitchState();
						//m_dockConsole = new CConsoleHelper(nColumnCount, (Console.WindowWidth - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount);
						continue;

					case CConsoleHelper.DockSelection.cSel_View: // Toggle grid/list view
						if (CConsoleHelper.m_MenuType == CConsoleHelper.MenuType.cType_Grid)
						{
							// Switch to single-column list (small icons possible)
							try
							{
								nColumnCount = 1;
								if ((int)config.imageSize > 0)
									config.imageSize = Math.Min((int)config.imageSize, Console.WindowWidth / 2);
							}
							catch (Exception e)
							{
								CLogger.LogError(e);
							}
							CLogger.LogInfo("Switching to list menu type...");
							config.listOutput = true;
							CConsoleHelper.m_nMaxItemsPerPage = Math.Max(1, Console.WindowHeight);
						}
						else
						{
							// Switch to multi-column grid (no small icons)
							try
							{
								nColumnCount = (Console.WindowWidth - imgMargin) / ((int)config.columnSize + COLUMN_CUSHION);
								if (nColumnCount <= 1) // shrink image to fit
								{
									nColumnCount = 1;
									config.imageBorder = false;
									if ((int)config.imageSize > 0)
										config.imageSize = Math.Min((int)config.imageSize, Console.WindowWidth / 2);
								}
							}
							catch (Exception e)
							{
								CLogger.LogError(e);
							}
							CLogger.LogInfo("Switching to grid menu type...");
							config.listOutput = false;
							CConsoleHelper.m_nMaxItemsPerPage = Math.Max(nColumnCount, Console.WindowHeight * nColumnCount);
						}
						CConsoleHelper.SwitchType();
						m_dockConsole = new CConsoleHelper(nColumnCount, (Console.WindowWidth - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount);
						continue;

					case CConsoleHelper.DockSelection.cSel_Colour: // Toggle dark/light mode
						if (CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Dark)
						{
							CLogger.LogInfo("Switching to light colour mode...");
							config.lightMode = true;
						}
						else
						{
							CLogger.LogInfo("Switching to dark colour mode...");
							config.lightMode = false;
						}
						CConsoleHelper.SwitchMode();
						//m_dockConsole = new CConsoleHelper(nColumnCount, (Console.WindowWidth - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount);
						continue;

					case CConsoleHelper.DockSelection.cSel_Image: // Toggle image mien
						if (CConsoleHelper.m_ImageMien == CConsoleHelper.ImageMien.cImage_Enabled)
						{
							CLogger.LogInfo("Disabling images...");
							config.imageSize = 0;
							config.iconSize = 0;
							imgMargin = 0;
						}
						else
						{
							CLogger.LogInfo("Enabling images...");
							config.iconSize = CConfig.DEF_ICONSIZE;
							config.imageSize = CConfig.DEF_IMGSIZE;
							imgMargin = (int)config.imageSize + COLUMN_CUSHION + ((bool)config.imageBorder ? IMG_BORDER_X_CUSHION : 0);
							m_nCurrentSelection = 0;
						}
						CConsoleHelper.SwitchMien();
						try
						{
							nColumnCount = (Console.WindowWidth - imgMargin) / ((int)config.columnSize + COLUMN_CUSHION);
							if (nColumnCount <= 1) // shrink image to fit
							{
								nColumnCount = 1;
								config.imageBorder = false;
								if ((int)config.imageSize > 0)
									config.imageSize = Math.Min((int)config.imageSize, Console.WindowWidth / 2);
							}
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						m_dockConsole = new CConsoleHelper(nColumnCount, (Console.WindowWidth - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount);
						continue;

					case CConsoleHelper.DockSelection.cSel_Sort: // Toggle freq/alpha method
						if (CConsoleHelper.m_SortMethod == CConsoleHelper.SortMethod.cSort_Freq)
						{
							CLogger.LogInfo("Switching to alphabetic sort method...");
							config.alphaSort = true;
						}
						else
						{
							CLogger.LogInfo("Switching to frequency sort method...");
							config.alphaSort = false;
						}
						CConsoleHelper.SwitchMethod();
						CGameData.SortGames(CConsoleHelper.m_SortMethod == CConsoleHelper.SortMethod.cSort_Alpha, (bool)config.faveSort, true);
						//m_dockConsole = new CConsoleHelper(nColumnCount, (Console.WindowWidth - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount);
						continue;

					case CConsoleHelper.DockSelection.cSel_Search: // Find game
						SetFgColour(cols.titleCC, cols.titleLtCC);
						try
						{
							Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - INPUT_BOTTOM_CUSHION);
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						Console.Write("Search >>> ");
						Console.CursorVisible = true;
						/*
						char key = Console.ReadKey();
						if (key == keys.cancelCK1 || key == keys.cancelCK2)
						{
							//[cancel search]
						}
						else if (key == keys.completeCK1 || key == keys.completeCK2)
						{
							//[auto-complete]
						}
						else if char.IsLetterOrDigit(key) Console.Write(key);
						*/
						gameSearch = Console.ReadLine();
						try
						{
							Console.SetCursorPosition(0, Console.WindowHeight - INPUT_BOTTOM_CUSHION);
							Console.WriteLine();
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						if (!string.IsNullOrEmpty(gameSearch))
						{
							CLogger.LogInfo("Search for: {0}", gameSearch);
							m_nSelectedPlatform = (int)CGameData.GamePlatform.Search;
						}
						else
							CLogger.LogWarn("No search term!");
						Console.ResetColor();
						continue;

					case CConsoleHelper.DockSelection.cSel_Default: // Platform/game selection
						if (gameIndex > 0)
							m_nSelectedPlatform = (int)CGameData.GamePlatform.All;
						else if (!string.IsNullOrEmpty(gameSearch))
							m_nSelectedPlatform = (int)CGameData.GamePlatform.Search;
						break;

					default:
						break;
				}

				CLogger.LogDebug("Platform {0}, Game {1}, Selection {2}", m_nSelectedPlatform, m_nSelectedGame, m_nCurrentSelection); //, m_nSelectedCategory
				if (m_nSelectedPlatform < 0)
				{
					List<string> platforms = CConsoleHelper.GetPlatformNames(!(bool)config.hideSettings);
					if (!noInteractive && CConsoleHelper.IsSelectionValid(m_nCurrentSelection, platforms.Count))
					{
						if (m_nCurrentSelection == platforms.Count - 1)
						{ //DoSettingsMenu();
						}
						else
						{
							m_nSelectedPlatform = CGameData.GetPlatformEnum(platforms[m_nCurrentSelection], true);
							m_nSelectedGame = -1;
							m_nCurrentSelection = 0;
						}
					}
				}
				else
				{
					string selectedPlatform = CGameData.GetPlatformString(m_nSelectedPlatform);
					CLogger.LogDebug("Selected platform: {0}", selectedPlatform);
					/*
					if (m_nSelectedCategory > -1)
					{
						string selectedCategory = CGameData.GetCategoryString(m_nSelectedCategory);
						CLogger.LogDebug("Selected category: {0}", selectedCategory);
					}
					*/
					if (CConsoleHelper.IsSelectionValid(m_nCurrentSelection, CGameData.GetPlatformTitles((CGameData.GamePlatform)m_nSelectedPlatform).Count))
					{
						m_nSelectedGame = m_nCurrentSelection;
						selectedGame = CGameData.GetPlatformGame((CGameData.GamePlatform)m_nSelectedPlatform, m_nSelectedGame);
						if (selectedGame != null) CLogger.LogDebug("Selected game: {0}", selectedGame.Title);
					}
				}

				// Game selection required

				if (selectedGame != null)
				{
					switch ((CConsoleHelper.DockSelection)nSelectionCode)
					{
						case CConsoleHelper.DockSelection.cSel_Desktop: // Add shortcut to Desktop
							string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
							CLogger.LogInfo("Add shortcut: {0}\\{1}.LNK", desktopPath, selectedGame.Title);
							CConsoleHelper.MakeShortcut(selectedGame.Title, selectedGame.Launch, selectedGame.Icon, desktopPath);
							continue;

						case CConsoleHelper.DockSelection.cSel_Uninst: // Uninstall game
							if (UninstallGame(selectedGame)) CGameData.RemoveGame(selectedGame);
							CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
							m_nCurrentSelection--;
							if (CGameData.GetPlatformGameList((CGameData.GamePlatform)m_nSelectedPlatform).ToList().Count < 1)
								m_nSelectedPlatform = -1;
							continue;
						case CConsoleHelper.DockSelection.cSel_Hide: // Remove from list
							CLogger.LogInfo("Hiding game: {0}", selectedGame.Title);
							CGameData.RemoveGame(selectedGame);
							CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
							m_nCurrentSelection--;
							if (CGameData.GetPlatformGameList((CGameData.GamePlatform)m_nSelectedPlatform).ToList().Count < 1)
								m_nSelectedPlatform = -1;
							continue;
						/*
						// Needs work
						case CConsoleHelper.DockSelection.cSel_Hide: // Toggle game hidden
							CLogger.LogInfo("Toggling hidden: {0}", selectedGame.Title);
							CGameData.ToggleHidden(
								(CGameData.GamePlatform)m_nSelectedPlatform,
								m_nCurrentSelection,
								CConsoleHelper.m_SortMethod == CConsoleHelper.SortMethod.cSort_Alpha,
								(bool)config.faveSort,
								true);
							CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
							if (m_nSelectedPlatform == (int)CGameData.GamePlatform.Hidden &&
								CGameData.GetPlatformGameList(CGameData.GamePlatform.Hidden).ToList().Count < 1)
								m_nSelectedPlatform = -1;
							continue;
						*/

						case CConsoleHelper.DockSelection.cSel_Fav: // Toggle game favourite
							CLogger.LogInfo("Toggling favourite: {0}", selectedGame.Title);
							CGameData.ToggleFavourite(
								(CGameData.GamePlatform)m_nSelectedPlatform,
								m_nCurrentSelection,
								CConsoleHelper.m_SortMethod == CConsoleHelper.SortMethod.cSort_Alpha,
								(bool)config.faveSort,
								true);
							CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
							if (m_nSelectedPlatform == (int)CGameData.GamePlatform.Favourites &&
								CGameData.GetPlatformGameList(CGameData.GamePlatform.Favourites).ToList().Count < 1)
								m_nSelectedPlatform = -1;
							continue;

						case CConsoleHelper.DockSelection.cSel_Alias: // Set alias for insert mode or command line parameter
							SetFgColour(cols.titleCC, cols.titleLtCC);
							try
							{
								Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - INPUT_BOTTOM_CUSHION);
							}
							catch (Exception e)
							{
								CLogger.LogError(e);
							}
							Console.Write("Alias [{0}] >>> ", selectedGame.Alias);
							Console.CursorVisible = true;
							string newAlias = Console.ReadLine();
							try
							{
								Console.SetCursorPosition(0, Console.WindowHeight - INPUT_BOTTOM_CUSHION);
								Console.WriteLine();
							}
							catch (Exception e)
							{
								CLogger.LogError(e);
							}
							if (!string.IsNullOrEmpty(newAlias))
							{
								CLogger.LogInfo("Set alias to: {0} for {1}", newAlias, selectedGame.Title);
								selectedGame.Alias = newAlias;
							}
							else
								CLogger.LogWarn("Alias not set!");
							Console.ResetColor();
							continue;

						default:
							break;
					}
				}

				if (m_nSelectedGame > -1 && selectedGame != null)
				{
					CGameData.NormaliseFrequencies(selectedGame);
					matches = new List<CGameData.CMatch>() { new CGameData.CMatch(selectedGame.Title, 1, 100) };
					CJsonWrapper.ExportSearch(matches);
					if (!(bool)config.ignoreChanges) CJsonWrapper.ExportConfig(config);

					Console.ResetColor();
					Console.CursorVisible = true;
					if (!noInteractive) Console.Clear();
					else Console.WriteLine();
#if !DEBUG
					if(!StartGame(selectedGame))
					{
						SetFgColour(cols.errorCC, cols.errorLtCC);
						CLogger.LogWarn("Remove game from file.");
						Console.WriteLine("{0} will be removed from the list.", selectedGame.Title);
						Console.ResetColor();
						CGameData.RemoveGame(selectedGame);
					}
#else
					// Don't run the game in debug mode
					Console.WriteLine("       ID : {0}", selectedGame.ID);
					Console.WriteLine("    Title : {0}", selectedGame.Title);
					Console.WriteLine("    Alias : {0}", selectedGame.Alias);
					Console.WriteLine("   Launch : {0}", selectedGame.Launch);
					Console.WriteLine("     Icon : {0}", selectedGame.Icon);
					Console.WriteLine("Uninstall : {0}", selectedGame.Uninstaller);
					Console.WriteLine();
					Console.WriteLine("DEBUG mode - game will not be launched; press Enter to exit...");
					if (noInteractive)
						config.imageSize = 0;
					else
					{
						config.imageSize = 32;
						config.imagePosition = 100;
						config.imageRes = 256;
						config.imageBorder = true;
						config.imageIgnoreRatio = false;
					}
					if ((int)config.imageSize > 0)
					{
						CConsoleImage.GetImageProperties((int)config.imageSize, (int)config.imagePosition, out imgSize, out imgLoc);
						if ((bool)config.imageBorder)
						{
							CConsoleImage.ShowImageBorder(imgSize, imgLoc, IMG_BORDER_X_CUSHION, IMG_BORDER_Y_CUSHION);
							CConsoleImage.ShowImage(m_nCurrentSelection, selectedGame.Title, selectedGame.Icon, false, imgSize, imgLoc, config, CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Light ? cols.bgLtCC : cols.bgCC);
							Console.SetCursorPosition(0, 8);
						}
						else
							CConsoleImage.ShowImage(m_nCurrentSelection, selectedGame.Title, selectedGame.Icon, false, imgSize, imgLoc, config, CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Light ? cols.bgLtCC : cols.bgCC);
					}
					Console.ReadLine();
#endif
					CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
					return;
				}
			}
		}

		/// <summary>
		/// Display menu and handle the selection
		/// </summary>
		private void MenuSwitchboard(CConfig.Configuration config, CConfig.Hotkeys keys, CConfig.Colours cols, ref string gameSearch, ref int nSelectionCode)
		{
			// Show initial options - platforms or all
			// Take the selection as a string (we'll figure out the enum later)
			// Display the options related to the initial selection (all games will show everything)
			//	Allow cancel with escape (make sure to print that in the heading)
			//  Run selected game.

			nSelectionCode = -1;

			List<string> platforms = CConsoleHelper.GetPlatformNames(!(bool)config.hideSettings);

			if (!noInteractive)
			{
				if ((bool)config.goToAll)
					m_nSelectedPlatform = (int)CGameData.GamePlatform.All;
				else if (!(platforms.Contains(CGameData.GetPlatformString(CGameData.GamePlatform.Favourites))) &&
					platforms.Count < 3)  // if there's only one platform + All, then choose All
				{
					CLogger.LogDebug("Only one valid platform found.");
					m_nSelectedPlatform = (int)CGameData.GamePlatform.All;
				}
			}

			if (m_nSelectedPlatform < 0)    // show main menu (platform list)
				nSelectionCode = m_dockConsole.DisplayMenu(config, keys, cols, ref gameSearch, platforms.ToArray());
			else if (m_nSelectedGame < 0)   // show game list
				nSelectionCode = m_dockConsole.DisplayMenu(config, keys, cols, ref gameSearch, CGameData.GetPlatformTitles((CGameData.GamePlatform)m_nSelectedPlatform).ToArray());
		}

		/// <summary>
		/// Uninstall the game
		/// </summary>
		/// <param name="game"></param>
		private bool UninstallGame(CGameData.CGame game)
		{
			if (string.IsNullOrEmpty(game.Uninstaller))
			{
				//SetFgColour(cols.errorCC, cols.errorLtCC);
				CLogger.LogWarn("ERROR: Uninstaller wasn't found.");
				Console.WriteLine("ERROR: Uninstaller wasn't found.");
				//Console.ResetColor();
				return false;
			}
			Console.ResetColor();
			Console.Clear();
			CLogger.LogInfo("Uninstalling game: {0}", game.Title);
			Console.WriteLine("Uninstalling game: {0}", game.Title);
			try
			{
				Process.Start(game.Uninstaller);
				return true;
			}
			catch (Exception e)
			{
				//SetFgColour(cols.errorCC, cols.errorLtCC);
				CLogger.LogError(e, "Cannot start uninstaller.");
				Console.WriteLine("ERROR: Uninstaller couldn't start. Were the game files manually deleted?");
				//Console.ResetColor();
				return false;
			}
		}

		/// <summary>
		/// Start the game process
		/// </summary>
		/// <param name="game"></param>
		private bool StartGame(CGameData.CGame game)
		{
			CLogger.LogInfo("Starting game: {0}", game.Title);
			Console.WriteLine("Starting game: {0}", game.Title);
			try
			{
				if (game.PlatformString == CGameData.GetPlatformString(CGameData.GamePlatform.GOG))
				{
					CLogger.LogInfo("Setting up a GOG game...");
					ProcessStartInfo gogProcess = new ProcessStartInfo();
					string clientPath = game.Launch.Contains(".") ? game.Launch.Substring(0, game.Launch.IndexOf('.') + 4) : game.Launch;
					string arguments = game.Launch.Contains(".") ? game.Launch.Substring(game.Launch.IndexOf('.') + 4) : String.Empty;
					gogProcess.FileName = clientPath;
					gogProcess.Arguments = arguments;
					Process.Start(gogProcess);
					return true;
				}
				Process.Start(game.Launch);
				return true;
			}
			catch (Exception e)
			{
				//SetFgColour(cols.errorCC, cols.errorLtCC);
				CLogger.LogError(e, "Cannot start game.");
				Console.WriteLine("ERROR: Cannot launch game. Were the game files manually deleted?");
				//Console.ResetColor();
				return false;
			}
		}

		/// <summary>
		/// Print output and wait until the user has pressed a key to show next page
		/// </summary>
		private void WriteWithBreak(ref int line, int height, ConsoleColor outputCol, ConsoleColor outputColLt, ConsoleColor breakCol, ConsoleColor breakColLt, string output)
		{
			if (line > height - INPUT_BOTTOM_CUSHION)
			{
				SetFgColour(breakCol, breakColLt);
				Console.Write("Press any key for more...");
				Console.ReadKey();
				Console.Clear();
				line = 0;
			}
			SetFgColour(outputCol, outputColLt);
			Console.WriteLine(output);
			line++;
		}

		private void WriteWithBreak(ref int line, int height, ConsoleColor breakCol, ConsoleColor breakColLt)
        {
			WriteWithBreak(ref line, height, ConsoleColor.Black, ConsoleColor.Black, breakCol, breakColLt, String.Empty);
        }

		/// <summary>
		/// Print help screen and wait until the user has pressed a key
		/// </summary>
		private void DisplayHelp(int height, CConfig.Configuration config, CConfig.Colours cols)
		{
			int line = 0;

			Console.Clear();
			WriteWithBreak(ref line, height, cols.titleCC, cols.titleLtCC, cols.titleCC, cols.titleLtCC,
				Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product +
				" version " +
				Assembly.GetEntryAssembly().GetName().Version.ToString());
			foreach (string str in m_helpLines)
			{
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC, str);
			}
			WriteWithBreak(ref line, height, cols.titleCC, cols.titleLtCC);
			if (CConsoleHelper.m_ConsoleState == CConsoleHelper.ConsoleState.cState_Insert)
			{
				SetFgColour(cols.titleCC, cols.titleLtCC);
				WriteWithBreak(ref line, height, cols.titleCC, cols.titleLtCC, cols.titleCC, cols.titleLtCC,
					"These are the currently accepted commands:");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"   /help | /h | /?");
				if (!(bool)config.goToAll) //&& !(bool)config.onlyCustom)
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
						"        /back | /b");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"         /nav | /n");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"        /scan | /s");
				//WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
				//	("    /alias | /a");
				//WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
				//	("/uninstall | /uninst | /u");
				//WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
				//	"     /view | /v");
				//WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
				//	"     /grid | /g");
				//WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
				//	"     /list | /l");
				//WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
				//	"   /colour | /color | /col");
				//WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
				//	"    /light | /lt");
				//WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
				//	"     /dark | /dk");
				if (!(bool)config.preventQuit)
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
						"     /exit | /x | /quit | /q");
				else
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
						"     [Ctrl]+[C] to quit.");
			}
			else //if (CConsoleHelper.m_ConsoleState == CConsoleHelper.ConsoleState.cState_Navigate)
			{
				WriteWithBreak(ref line, height, cols.titleCC, cols.titleLtCC, cols.titleCC, cols.titleLtCC,
					" These are the currently available keys:");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"            Help: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.helpKey1),
					CConfig.ShortenKeyName(config.helpKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"            Left: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.leftKey1),
					CConfig.ShortenKeyName(config.leftKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"              Up: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.upKey1),
					CConfig.ShortenKeyName(config.upKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"           Right: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.rightKey1),
					CConfig.ShortenKeyName(config.rightKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"            Down: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.downKey1),
					CConfig.ShortenKeyName(config.downKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"          Select: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.selectKey1),
					CConfig.ShortenKeyName(config.selectKey2), "[", "]", " | ", "N/A", 8));
				if (!(bool)config.goToAll) //&& !(bool)config.onlyCustom)
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
						"            Back: " +
						CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.backKey1),
						CConfig.ShortenKeyName(config.backKey2), "[", "]", " | ", "N/A", 8));
				/*
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"          Search: " + CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.searchKey1),
					CConfig.ShortenKeyName(config.searchKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"Search Auto-Comp: " + CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.completeKey1),
					CConfig.ShortenKeyName(config.completeKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"   Search Cancel: " + CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.cancelKey1),
					CConfig.ShortenKeyName(config.cancelKey2), "[", "]", " | ", "N/A", 8));
				*/
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"         Page Up: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.pageUpKey1),
					CConfig.ShortenKeyName(config.pageUpKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"       Page Down: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.pageDownKey1),
					CConfig.ShortenKeyName(config.pageDownKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"           First: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.firstKey1),
					CConfig.ShortenKeyName(config.firstKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"            Last: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.lastKey1),
					CConfig.ShortenKeyName(config.lastKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"    Rescan Games: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.scanKey1),
					CConfig.ShortenKeyName(config.scanKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"Toggle Favourite: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.faveKey1),
					CConfig.ShortenKeyName(config.faveKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"       Set Alias: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.aliasKey1),
					CConfig.ShortenKeyName(config.aliasKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"       Hide Game: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.hideKey1),
					CConfig.ShortenKeyName(config.hideKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"  Uninstall Game: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.uninstKey1),
					CConfig.ShortenKeyName(config.uninstKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"Desktop Shortcut: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.desktopKey1),
					CConfig.ShortenKeyName(config.desktopKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"  Nav/Type Input: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.typeKey1),
					CConfig.ShortenKeyName(config.typeKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"  Grid/List View: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.viewKey1),
					CConfig.ShortenKeyName(config.viewKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					" Dark/Light Mode: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.modeKey1),
					CConfig.ShortenKeyName(config.modeKey2), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"   Toggle Images: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.imageKey1),
					CConfig.ShortenKeyName(config.imageKey2), "[", "]", " | ", "N/A", 8));
				//WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
				//	"     Sort Method: " +
				//	CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.sortKey1),
				//	CConfig.ShortenKeyName(config.sortKey2), "[", "]", " | ", "N/A", 8));
				if (!(bool)config.preventQuit)
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
						"            Quit: " +
						CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(config.quitKey1),
						CConfig.ShortenKeyName(config.quitKey2), "[", "]", " | ", "N/A", 8));
				else
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
						"            Quit: [Ctrl]+[C]");
			}
			WriteWithBreak(ref line, height, cols.titleCC, cols.titleLtCC);
			SetFgColour(cols.titleCC, cols.titleLtCC);
			Console.Write("Press any key to return to previous menu...");
			Console.ReadKey();
		}

		/// <summary>
		/// Print usage screen
		/// </summary>
		public static void DisplayUsage(CConfig.Colours cols, string parent)
		{
			Console.WriteLine();
			//				  0|-------|---------|---------|---------|---------|---------|---------|---------|80
			SetFgColour(cols.titleCC, cols.titleLtCC);
			Console.WriteLine("{0} version {1}",
				Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product,
				Assembly.GetEntryAssembly().GetName().Version.ToString());
			Console.WriteLine(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>().Description);
			Console.WriteLine();
			Console.WriteLine("Usage:");
			Console.ResetColor();
			Console.WriteLine("Launch a game by entering search on the command-line, e.g.:");
			Console.WriteLine(" .\\{0} \"My Game\"", FILENAME);
			Console.WriteLine("If there are multiple results, use number from prior search, e.g.:");
			Console.WriteLine(" .\\{0} /2", FILENAME);
			Console.WriteLine();
			SetFgColour(cols.titleCC, cols.titleLtCC);
			Console.WriteLine("Other parameters:");
			Console.ResetColor();
			Console.WriteLine("   /1 : Replay the previously launched game");
			Console.WriteLine("   /S : Rescan your games");
			//Console.WriteLine("/U \"My Game\": Uninstall game");
			//Console.WriteLine("/A myalias \"My Game Name\": Change game's alias");
			Console.WriteLine("   /C : Toggle command-line only mode");
			Console.WriteLine("   /P : Add {0}.exe location to your path", FILENAME);
			Console.WriteLine("/?|/H : Display this help");
			if (parent.Equals("explorer"))
            {
				Console.WriteLine();
				Console.Write("Press any key to close...");
				Console.ReadKey();
			}
		}

		/// <summary>
		/// Print help screen and wait until the user has pressed a key
		/// </summary>
		/*
		public static void DoSettings()
        {
			foreach (setting in )
        }
		*/

		/// <summary>
		/// Set background colour
		/// </summary>
		public static void SetBgColour(ConsoleColor colour)
		{
			if (colour > (ConsoleColor)(-1)) Console.BackgroundColor = colour;
		}

		/// <summary>
		/// Set background colour for light or dark mode
		/// </summary>
		public static void SetBgColour(ConsoleColor colour, ConsoleColor lightColour)
		{
			if (CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Light && lightColour > (ConsoleColor)(-1))
				Console.BackgroundColor = lightColour;
			else if (colour > (ConsoleColor)(-1))
				Console.BackgroundColor = colour;
		}

		/// <summary>
		/// Set foreground colour
		/// </summary>
		public static void SetFgColour(ConsoleColor colour)
		{
			if (colour > (ConsoleColor)(-1)) Console.ForegroundColor = colour;
		}

		/// <summary>
		/// Set foreground colour for light or dark mode
		/// </summary>
		public static void SetFgColour(ConsoleColor colour, ConsoleColor lightColour)
		{
			if (CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Light && lightColour > (ConsoleColor)(-1))
				Console.ForegroundColor = lightColour;
			else if (colour > (ConsoleColor)(-1))
				Console.ForegroundColor = colour;
		}

		/// <summary>
		/// Clear image with appropriate light or dark mode
		/// </summary>
		public static void ClearColour(ConsoleColor bgDark, ConsoleColor bgLight)
		{
			if (CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Light && bgLight > (ConsoleColor)(-1))
				CConsoleImage.ClearImage(imgSize, imgLoc, bgLight);
			else if (bgDark > (ConsoleColor)(-1))
				CConsoleImage.ClearImage(imgSize, imgLoc, bgDark);
			else
				CConsoleImage.ClearImage(imgSize, imgLoc, ConsoleColor.Black);
		}

		/// <summary>
		/// Check whether current shell has known issues with interactive applications and image display
		/// <returns>bool, whether interactive mode is supported</returns>
		/// </summary>
		public static void CheckShellCapabilities(ref CConfig.Configuration config, out string parentName)
		{
			parentName = "";
			bool shellError = false;
			try
			{
				parentName = ParentProcessUtilities.GetParentProcess().ProcessName;
				CLogger.LogInfo("Parent process: {0}", parentName);
				/*
				if (parentName.Equals("explorer") && !(bool)config.onlyCmdLine)
                {
					MessageBox.Show("ERROR: Interactive mode is disabled.");  // Show usage hint in Message Box? Or just pause before closing?
				}
				else
				*/

				// 4NT and clink work just fine, but don't have process parents; although clink's parent is cmd, so it winds up in the catch()
				if (!parentName.Equals("4nt"))
				{
					IntPtr parentHandle = ParentProcessUtilities.GetParentProcess().Handle;

					// if cmd or PowerShell is launched via shortcut or Run box, this will be "explorer"
					string parentParentName = ParentProcessUtilities.GetParentProcess(parentHandle).ProcessName;
					CLogger.LogInfo("Parent process parent: {0}", parentParentName);
					
					// With PowerCmd, Console.Write() is not visible at all, so we can't even show an error message!
					if (parentParentName.Equals("PowerCmd"))
					{
						if (!(bool)config.onlyCmdLine)
						{
							//SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogWarn("ERROR: Your {0} host is not not supported.", parentParentName);
							/*
							Console.WriteLine("ERROR: Your {0} host is not not supported.", parentParentName);
							Console.ResetColor();
							shellError = true;
							*/
							noInteractive = true;
						}
					}
					else if (!((bool)config.onlyCmdLine))
					{
						if (parentName.Equals("tcc") &&            // many keys won't work with combination of ConEmu64 + Take Command (tcc)
								 parentParentName.Equals("ConEmuC64"))  // (though tcc is fine otherwise)
						{
							if (!(bool)config.onlyCmdLine ||
								(!(bool)config.typeInput &&
								// this isn't really a great test...
								(config.upKey1.Equals("UpArrow", IGNORE_CASE) || config.upKey1.Equals("Up", IGNORE_CASE)) ||
								(config.upKey2.Equals("UpArrow", IGNORE_CASE) || config.upKey2.Equals("Up", IGNORE_CASE))))
							{
								//SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("WARNING: Many keys (arrows, F1-F12) do not work in {0} host with {1}.\nSwitching to typing input...", parentParentName, parentName);
								Console.WriteLine("WARNING: Many keys (arrows, F1-F12) do not work in {0} host with {1}.\nSwitching to typing input...", parentParentName, parentName);
								//Console.ResetColor();
								config.typeInput = true;
								shellError = true;
							}
						}

						else if (parentParentName.Equals("FireCMD"))    // Displays menus, but colours aren't displayed, making navigation mode nigh useless
						{
							if (!(bool)config.typeInput)
							{
								//SetFgColour(cols.errorCC, cols.errorLtCC);
								/*
								CLogger.LogWarn("ERROR: Your {0} host is not not supported.", parentParentName);
								Console.WriteLine("ERROR: Your {0} host is not not supported.", parentParentName);
								//Console.ResetColor();
								return;
								*/
								CLogger.LogWarn("WARNING: {0}=false, but your {1} host does not support this.\nSwitching input state...", CConfig.CFG_USETYPE, parentParentName);
								Console.WriteLine("WARNING: {0}=false, but your {1} host does not support this.\nSwitching input state...", CConfig.CFG_USETYPE, parentParentName);
								//Console.ResetColor();
								config.typeInput = true;
								shellError = true;
							}
							if ((int)config.imageSize > 0 || (int)config.iconSize > 0)
							{
								//SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("WARNING: {0} or {1} > 0, but your {2} host does not support this.\nDisabling images...", CConfig.CFG_IMGSIZE, CConfig.CFG_ICONSIZE, parentParentName);
								Console.WriteLine("WARNING: {0} or {1} > 0, but your {2} host does not support this.\nDisabling images...", CConfig.CFG_IMGSIZE, CConfig.CFG_ICONSIZE, parentParentName);
								//Console.ResetColor();
								config.iconSize = 0;
								config.imageSize = 0;
								shellError = true;
							}
						}
						/*
						else if (parentParentName.Equals("ZTW64"))		// I have observed weird issues with newline characters
						{
							//SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogWarn("WARNING: Your {0} host may have display issues.", parentParentName);
							Console.WriteLine("WARNING: Your {0} host may have display issues.", parentParentName);
							//Console.ResetColor();
							shellError = true;
						}
						*/
						else
						{
							// check for known non-conhost terminal hosts (images not supported)
							if (((int)config.imageSize > 0 || (int)config.iconSize > 0) &&
								(parentParentName.Equals("WindowsTerminal") ||          // Windows Terminal
								 parentParentName.StartsWith("ServiceHub.Host.CLR") ||  // Visual Studio 
								 parentParentName.Equals("Code") ||                     // Visual Studio Code
								 parentParentName.Equals("Hyper") ||                    // Hyper
								 parentParentName.Equals("tcmd") ||                     // Take Command
								 parentParentName.Equals("bash") ||                     // Cygwin Terminal (mintty), though this one may give false negatives (MSYS in 
								 parentParentName.Equals("Console")))                   // Console2 or ConsoleZ
							{
								//SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("WARNING: {0} or {1} > 0, but your {2} host does not support this.\nDisabling images...", CConfig.CFG_IMGSIZE, CConfig.CFG_ICONSIZE, parentParentName);
								Console.WriteLine("WARNING: {0} or {1} > 0, but your {2} host does not support this.\nDisabling images...", CConfig.CFG_IMGSIZE, CConfig.CFG_ICONSIZE, parentParentName);
								//Console.ResetColor();
								config.iconSize = 0;
								config.imageSize = 0;
								shellError = true;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, "Couldn't get parent process. Your terminal host may not be supported.");
				//Console.WriteLine("Couldn't get parent process. Your terminal host may not be supported.");
				//shellError = true;
			}
			if (shellError)
				Thread.Sleep(5000);
		}

		/// <summary>
		/// A utility class to update path environment variable.
		/// </summary>
		public struct PathEnvironmentUpdate
		{
			const int HWND_BROADCAST = 0xffff;
			const uint WM_SETTINGCHANGE = 0x001a;

			[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			static extern bool SendNotifyMessage(
				IntPtr hWnd,
				uint Msg,
				UIntPtr wParam,
				string lParam);

			public static bool Add(string inPath, bool allUsers)
			{
				try
				{
					RegistryKey envKey;
					string newPath;
					if (allUsers)
					{
						envKey = Registry.LocalMachine.OpenSubKey(
							@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment",
							true);
					}
					else
					{
						envKey = Registry.CurrentUser.OpenSubKey("Environment", true);
						string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
						if (inPath.Length > profile.Length &&
							inPath.Substring(0, profile.Length).Equals(profile))
						{
							inPath = "%USERPROFILE%" + inPath.Substring(profile.Length);
						}
					}
					using (envKey)
					{
						if (envKey == null)
						{
							CLogger.LogWarn("Could not access environment!");
							return false;
						}
						var oldPath = envKey.GetValue("PATH");
						if (oldPath != null)
							newPath = (string)oldPath + ";" + inPath;
						else
							newPath = inPath;
						CLogger.LogDebug("New PATH: {0}", newPath);
						envKey.SetValue("PATH", newPath);
						SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
						return true;
					}
				}
				catch (Exception e)
				{
					CLogger.LogError(e);
				}
				return false;
			}
		}
	}

	/// <summary>
	/// A utility class to determine a process parent.
	/// https://stackoverflow.com/a/3346055
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ParentProcessUtilities
	{
		const int PROCESS_BASIC_INFORMATION = 0;

		// These members must match PROCESS_BASIC_INFORMATION
		internal IntPtr Reserved1;
		internal IntPtr PebBaseAddress;
		internal IntPtr Reserved2_0;
		internal IntPtr Reserved2_1;
		internal IntPtr UniqueProcessId;
		internal IntPtr InheritedFromUniqueProcessId;

		[DllImport("ntdll.dll")]
		private static extern int NtQueryInformationProcess(
			IntPtr processHandle,
			int processInformationClass,
			ref ParentProcessUtilities processInformation,
			int processInformationLength,
			out uint returnLength);

		/// <summary>
		/// Gets the parent process of the current process.
		/// </summary>
		/// <returns>An instance of the Process class.</returns>
		public static Process GetParentProcess()
		{
			return GetParentProcess(Process.GetCurrentProcess().Handle);
		}

		/// <summary>
		/// Gets the parent process of specified process.
		/// </summary>
		/// <param name="id">The process id.</param>
		/// <returns>An instance of the Process class.</returns>
		public static Process GetParentProcess(int id)
		{
			Process process = Process.GetProcessById(id);
			return GetParentProcess(process.Handle);
		}

		/// <summary>
		/// Gets the parent process of a specified process.
		/// </summary>
		/// <param name="handle">The process handle.</param>
		/// <returns>An instance of the Process class.</returns>
		public static Process GetParentProcess(IntPtr handle)
		{
			ParentProcessUtilities pbi = new ParentProcessUtilities();
			int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out uint returnLength);
			if (status != 0)
				throw new Win32Exception(status);

			try
			{
				return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
			}
			catch (ArgumentException)
			{
				// not found
				return null;
			}
		}
	}
}
