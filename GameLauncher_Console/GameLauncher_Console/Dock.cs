using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
		public const int INPUT_ITEM_CUSHION = 1;
		public const int INSTRUCT_CUSHION = 1;
		public const int IMG_BORDER_X_CUSHION = 1;
		public const int IMG_BORDER_Y_CUSHION = 0;
		public const int ICON_LEFT_CUSHION = 0;
		public const int ICON_RIGHT_CUSHION = 1;
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
		public static Size sizeIcon;
		public static Size sizeImage;
		public static Point locImage;

		/// <summary>
		/// Array of string containing the content of the help screen.
		/// </summary>
		private readonly string[] m_helpLines =
		{
		//  0|-------|---------|---------|---------|---------|---------|---------|---------|80
			" This program will scan your system for installed video games and display",
			" them as a list. The following platforms are supported:",
			" * Amazon * Battle.net * Bethesda * Big Fish * Epic * GOG * Indiegala * itch",
			" * Origin * Paradox * Steam * Ubisoft * custom",
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
			bool import, parseError = false;
			import = CJsonWrapper.ImportFromINI(out CConfig.ConfigVolatile cfgv, out CConfig.Hotkeys keys, out CConfig.Colours cols);
			if (!import) parseError = true;
			import = CJsonWrapper.ImportFromJSON(out List<CGameData.CMatch> matches);
			if (!import) parseError = true;
			CGameFinder.CheckCustomFolder();

			if (parseError)
			{
				Console.Write("Press any key to continue...");
				Console.ReadKey();
				Console.WriteLine();
			}

			bool consoleOutput = (bool)CConfig.GetConfigBool(CConfig.CFG_USECMD);
			if (consoleOutput) noInteractive = true;

			bool setup = false;
			bool browse = false;
			//string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			string path = Directory.GetCurrentDirectory();

			int gameIndex = -1;
			string gameSearch = "";
			foreach (string arg in args)
			{
				gameSearch += arg + " ";
			}
			if (args.Count() > 0)
			{
				if (gameSearch[0] == '/' || gameSearch[0] == '-')
				{
					gameSearch = gameSearch.Substring(1);
					if (gameSearch[0].Equals('?') || gameSearch[0].Equals('h') || gameSearch[0].Equals('H'))
						noInteractive = true;
					else if (gameSearch[0].Equals('s') || gameSearch[0].Equals('S'))
					{
						CLogger.LogInfo("Scanning for games...");
						Console.Write("Scanning for games");  // ScanGames() will add dots for each platform
						CRegScanner.ScanGames((bool)CConfig.GetConfigBool(CConfig.CFG_USECUST), !(bool)CConfig.GetConfigBool(CConfig.CFG_IMGSCAN), false);
						return;
					}
					else if (gameSearch[0].Equals('c') || gameSearch[0].Equals('C'))
					{
						if (consoleOutput)
						{
							CLogger.LogInfo("Switching {0} = false...", CConfig.CFG_USECMD);
							Console.WriteLine("Switching {0} = false...", CConfig.CFG_USECMD);
							CConfig.SetConfigValue(CConfig.CFG_USECMD, false);
							CJsonWrapper.ExportConfig();
						}
						else
						{
							CLogger.LogInfo("Switching {0} = true...", CConfig.CFG_USECMD);
							Console.WriteLine("Switching {0} = true...", CConfig.CFG_USECMD);
							CConfig.SetConfigValue(CConfig.CFG_USECMD, true);
							CJsonWrapper.ExportConfig();
						}
						return;
					}
					else if (gameSearch[0].Equals('p') || gameSearch[0].Equals('P'))
					{
						if (PathEnvironmentUpdate.Add(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), false))
						{
							CLogger.LogInfo("Added program location to PATH.");
							Console.WriteLine("Added {0}.exe location to your PATH environment variable.", FILENAME);
						}
						else
						{
							SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogWarn("Unable to add program location to PATH!");
							Console.WriteLine("ERROR: Unable to add the program location to your PATH environment variable!");
							Console.ResetColor();
						}
						return;
					}
					else
					{
						if (int.TryParse(gameSearch, out gameIndex))
						{
							if (gameIndex > 0 && gameIndex <= matches.ToArray().Count())
							{
								CLogger.LogInfo("Select {0} from prior search of {1} matches", gameIndex, matches.ToArray().Count());
							}
							else
							{
								SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("Invalid search index number!");
								Console.WriteLine($"ERROR: {gameIndex} is an invalid search index number!");
								Console.ResetColor();
								return;
							}
						}
						else
						{
							SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogWarn("Invalid parameter!");
							Console.WriteLine($"ERROR: /{gameSearch}is an invalid parameter!"); // there's a trailing space
							Console.ResetColor();
							return;
						}
					}
					gameSearch = "";
				}
				else
				{
					gameSearch = gameSearch.Substring(0, gameSearch.Count() - 1);
					CLogger.LogInfo($"Search from command line: {gameSearch}");
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

			CheckShellCapabilities(ref cfgv, out string parent);

			if (cfgv.imageSize > 0)
				imgMargin = cfgv.imageSize + COLUMN_CUSHION + (cfgv.imageBorder ? IMG_BORDER_X_CUSHION : 0);
			try
			{
				/*
				req'd width for columns, assuming default values, e.g., image size = 16:
				2 = 72+
				3 = 99+
				4 = 126+
				...
				*/
				int ww = Console.WindowWidth;
				if (!cfgv.listView)
					nColumnCount = (ww - imgMargin) / ((ushort)CConfig.GetConfigNum(CConfig.CFG_COLSIZE) + COLUMN_CUSHION);
				else nColumnCount = 1;
				if (nColumnCount <= 1) // shrink image to fit
				{
					nColumnCount = 1;
					cfgv.imageBorder = false;
					if (cfgv.imageSize > 0)
						cfgv.imageSize = (ushort)Math.Min(cfgv.imageSize, ww / 2);
				}
			}
			catch (Exception e)
			{
				if (!consoleOutput)
				{
					SetFgColour(cols.errorCC, cols.errorLtCC);
					CLogger.LogError(e, "Couldn't get console width. Your terminal host does not support interactive mode.");
					Console.WriteLine("ERROR: Your terminal host does not support interactive mode.");  // e.g., Git Bash, Mingw, MSYS2 (though strangely mintty works fine with Cygwin)
					Console.ResetColor();
					noInteractive = true;
				}
			}

			if (noInteractive && gameIndex < 1 && string.IsNullOrEmpty(gameSearch))
			{
				/*
				if (!consoleOutput)
				{
					SetFgColour(cols.errorCC, cols.errorLtCC);
					CLogger.LogWarn("Interactive mode is disabled.");
					Console.WriteLine("ERROR: Interactive mode is disabled.");
					Console.ResetColor();
				}
				*/
				DisplayUsage(cols, parent, matches);
				return;
			}

			if (cfgv.imageSize > 0)
				CConsoleImage.GetImageProperties(cfgv.imageSize, (ushort)CConfig.GetConfigNum(CConfig.CFG_IMGPOS), out sizeImage, out locImage);
			if (cfgv.iconSize > 0)
				CConsoleImage.GetIconSize(cfgv.iconSize, out sizeIcon);
			if (cfgv.typingInput && CConsoleHelper.m_ConsoleState == CConsoleHelper.ConsoleState.cState_Unknown)
				CConsoleHelper.m_ConsoleState = CConsoleHelper.ConsoleState.cState_Insert;
			else CConsoleHelper.m_ConsoleState = CConsoleHelper.ConsoleState.cState_Navigate;
			if (cfgv.listView && CConsoleHelper.m_MenuType == CConsoleHelper.MenuType.cType_Unknown)
				CConsoleHelper.m_MenuType = CConsoleHelper.MenuType.cType_List;
			else
				CConsoleHelper.m_MenuType = (nColumnCount > 1) ? CConsoleHelper.MenuType.cType_Grid : CConsoleHelper.MenuType.cType_List;
			if ((bool)CConfig.GetConfigBool(CConfig.CFG_USELITE) && CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Unknown)
				CConsoleHelper.m_LightMode = CConsoleHelper.LightMode.cColour_Light;
			else CConsoleHelper.m_LightMode = CConsoleHelper.LightMode.cColour_Dark;
			if (cfgv.imageSize > 0 || cfgv.iconSize > 0)
				CConsoleHelper.m_ImageMien = CConsoleHelper.ImageMien.cImage_Enabled;
			else CConsoleHelper.m_ImageMien = CConsoleHelper.ImageMien.cImage_Disabled;
			if ((bool)CConfig.GetConfigBool(CConfig.CFG_USEALPH))
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
					MenuSwitchboard(cfgv, keys, cols, ref setup, ref browse, ref path, ref gameSearch, ref nSelectionCode);
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
						if (!consoleOutput)
						{
							if (!cfgv.dontSaveChanges) CJsonWrapper.ExportConfig();
							CGameData.ClearNewGames();
							CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
							Console.Clear();
						}
						return;

					case CConsoleHelper.DockSelection.cSel_Fail:
						Console.ResetColor();
						Console.CursorVisible = true;
						if (!consoleOutput) Console.Clear();
						SetFgColour(cols.errorCC, cols.errorLtCC);
						CLogger.LogWarn("Console height too small to support interactive mode.");
						Console.WriteLine("ERROR: Console height is too small to support interactive mode.");
						DisplayUsage(cols, parent, matches);
						return;

					case CConsoleHelper.DockSelection.cSel_Redraw:
						continue;

					case CConsoleHelper.DockSelection.cSel_Back:
						m_nSelectedPlatform = -1;
						m_nCurrentSelection = 0;
						continue;

					case CConsoleHelper.DockSelection.cSel_Help:
						DisplayHelp(Console.WindowHeight, cols);
						continue;

					case CConsoleHelper.DockSelection.cSel_Rescan: // Rescan the game list
						try
						{
							Console.SetCursorPosition(0, Console.WindowHeight - INPUT_BOTTOM_CUSHION);
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						Console.ResetColor();
						CLogger.LogInfo("Scanning for games...");
						Console.Write("Scanning for games");  // ScanGames() will add dots for each platform
						CRegScanner.ScanGames((bool)CConfig.GetConfigBool(CConfig.CFG_USECUST), !(bool)CConfig.GetConfigBool(CConfig.CFG_IMGSCAN), false);
						continue;

					case CConsoleHelper.DockSelection.cSel_Input: // Toggle arrows/typing input
						if (CConsoleHelper.m_ConsoleState == CConsoleHelper.ConsoleState.cState_Navigate)
						{
							CLogger.LogInfo("Switching to insert input...");
							CConfig.SetConfigValue(CConfig.CFG_USETYPE, true);
							cfgv.typingInput = true;
						}
						else
						{
							CLogger.LogInfo("Switching to navigate input...");
							CConfig.SetConfigValue(CConfig.CFG_USETYPE, false);
							cfgv.typingInput = false;
						}
						CConsoleHelper.SwitchState();
						//m_dockConsole = new CConsoleHelper(nColumnCount, (Console.WindowWidth - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount);
						continue;

					case CConsoleHelper.DockSelection.cSel_View: // Toggle grid/list view
						int ww = 0;
						int wh = 0;
						try
                        {
							ww = Console.WindowWidth;
							wh = Console.WindowHeight;
                        }
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						if (CConsoleHelper.m_MenuType == CConsoleHelper.MenuType.cType_Grid)
						{
							// Switch to single-column list (with small icons possible)
							CLogger.LogInfo("Switching to list menu type...");
							CConfig.SetConfigValue(CConfig.CFG_USELIST, true);
							cfgv.listView = true;
							nColumnCount = 1;
							if (cfgv.imageSize > 0)
								cfgv.imageSize = (ushort)Math.Min(cfgv.imageSize, ww / 2);
							CConsoleHelper.m_nMaxItemsPerPage = Math.Max(1, wh);
						}
						else
						{
							// Switch to multi-column grid (no small icons)
							CLogger.LogInfo("Switching to grid menu type...");
							CConfig.SetConfigValue(CConfig.CFG_USELIST, false);
							cfgv.listView = false;
							nColumnCount = (ww - imgMargin) / ((ushort)CConfig.GetConfigNum(CConfig.CFG_COLSIZE) + COLUMN_CUSHION);
							if (nColumnCount <= 1) // shrink image to fit
							{
								nColumnCount = 1;
								cfgv.imageBorder = false;
								if (cfgv.imageSize > 0)
									cfgv.imageSize = (ushort)Math.Min(cfgv.imageSize, ww / 2);
							}
							CConsoleHelper.m_nMaxItemsPerPage = Math.Max(nColumnCount, wh * nColumnCount);
						}
						CConsoleHelper.SwitchType();
						m_dockConsole = new CConsoleHelper(nColumnCount, (ww - imgMargin) / nColumnCount, wh * nColumnCount);
						continue;

					case CConsoleHelper.DockSelection.cSel_Colour: // Toggle dark/light mode
						if (CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Dark)
						{
							CLogger.LogInfo("Switching to light colour mode...");
							CConfig.SetConfigValue(CConfig.CFG_USELITE, true);
						}
						else
						{
							CLogger.LogInfo("Switching to dark colour mode...");
							CConfig.SetConfigValue(CConfig.CFG_USELITE, false);
						}
						CConsoleHelper.SwitchMode();
						/*
						try
						{
							m_dockConsole = new CConsoleHelper(nColumnCount, (Console.WindowWidth - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount);
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						*/
						continue;

					case CConsoleHelper.DockSelection.cSel_Image: // Toggle image mien
						if (CConsoleHelper.m_ImageMien == CConsoleHelper.ImageMien.cImage_Enabled)
						{
							CLogger.LogInfo("Disabling images...");
							if (!cfgv.dontSaveChanges)
							{
								CConfig.SetConfigValue(CConfig.CFG_ICONSIZE, 0);
								CConfig.SetConfigValue(CConfig.CFG_IMGSIZE, 0);
							}
							cfgv.iconSize = 0;
							cfgv.imageSize = 0;
							cfgv.imageBorder = (bool)CConfig.GetConfigBool(CConfig.CFG_IMGBORD);
							imgMargin = 0;
						}
						else
						{
							CLogger.LogInfo("Enabling images...");
							if (!cfgv.dontSaveChanges)
							{
								CConfig.SetConfigDefault(CConfig.CFG_ICONSIZE);
								cfgv.iconSize = (ushort)CConfig.GetConfigNum(CConfig.CFG_ICONSIZE);
								CConfig.SetConfigDefault(CConfig.CFG_IMGSIZE);
								cfgv.imageSize = (ushort)CConfig.GetConfigNum(CConfig.CFG_IMGSIZE);
							}
							else
							{
								if (!ushort.TryParse(CConfig.GetConfigDefault(CConfig.CFG_ICONSIZE), out cfgv.iconSize))
									cfgv.iconSize = 0;
								if (!ushort.TryParse(CConfig.GetConfigDefault(CConfig.CFG_IMGSIZE), out cfgv.imageSize))
									cfgv.imageSize = 0;
							}
							cfgv.imageBorder = (bool)CConfig.GetConfigBool(CConfig.CFG_IMGBORD);
							imgMargin = cfgv.imageSize + COLUMN_CUSHION + (cfgv.imageBorder ? IMG_BORDER_X_CUSHION : 0);
							m_nCurrentSelection = 0;
						}
						CConsoleHelper.SwitchMien();
						try
						{
							ww = Console.WindowWidth;
							nColumnCount = (ww - imgMargin) / ((ushort)CConfig.GetConfigNum(CConfig.CFG_COLSIZE) + COLUMN_CUSHION);
							if (nColumnCount <= 1) // shrink image to fit
							{
								nColumnCount = 1;
								cfgv.imageBorder = false;
								if (cfgv.imageSize > 0)
									cfgv.imageSize = (ushort)Math.Min(cfgv.imageSize, ww / 2);
							}
							m_dockConsole = new CConsoleHelper(nColumnCount, (ww - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount);
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						continue;

					case CConsoleHelper.DockSelection.cSel_Sort: // Toggle freq/alpha method
						if (CConsoleHelper.m_SortMethod == CConsoleHelper.SortMethod.cSort_Freq)
						{
							CLogger.LogInfo("Switching to alphabetic sort method...");
							CConfig.SetConfigValue(CConfig.CFG_USEALPH, true);
						}
						else
						{
							CLogger.LogInfo("Switching to frequency sort method...");
							CConfig.SetConfigValue(CConfig.CFG_USEALPH, false);
						}
						CConsoleHelper.SwitchMethod();
						CGameData.SortGames(CConsoleHelper.m_SortMethod == CConsoleHelper.SortMethod.cSort_Alpha,
							(bool)CConfig.GetConfigBool(CConfig.CFG_USEFAVE),
							(bool)CConfig.GetConfigBool(CConfig.CFG_USEINST), true);
						/*
						try
						{
							m_dockConsole = new CConsoleHelper(nColumnCount, (Console.WindowWidth - imgMargin) / nColumnCount, Console.WindowHeight * nColumnCount);
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						*/
						continue;

					case CConsoleHelper.DockSelection.cSel_Settings: // Game settings
						//setup = true;
						break;

					case CConsoleHelper.DockSelection.cSel_Search: // Find game
						gameSearch = InputPrompt("Search >>> ", cols);
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
						/*
						try
						{
							Console.SetCursorPosition(0, Console.WindowHeight - INPUT_BOTTOM_CUSHION);
							Console.WriteLine();
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						*/
						if (!string.IsNullOrEmpty(gameSearch))
						{
							CLogger.LogInfo($"Search for: {gameSearch}");
							m_nSelectedPlatform = (int)CGameData.GamePlatform.Search;
						}
						else
							CLogger.LogInfo("No search term provided.");
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
					List<string> platforms = CConsoleHelper.GetPlatformNames();
					if (!noInteractive && CConsoleHelper.IsSelectionValid(m_nCurrentSelection, platforms.Count))
					{
						if (!(bool)CConfig.GetConfigBool(CConfig.CFG_NOCFG) && m_nCurrentSelection == platforms.Count - 1)
						{
							// calling directly won't work currently, as all of these functions aren't static
							//CConsoleHelper.DisplayCfg(cfgv, keys, cols, ref setup);
							//setup = true;
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
					CLogger.LogDebug($"Selected platform: {selectedPlatform}");
					/*
					if (m_nSelectedCategory > -1)
					{
						string selectedCategory = CGameData.GetCategoryString(m_nSelectedCategory);
						CLogger.LogDebug($"Selected category: {selectedCategory}");
					}
					*/
					if (CConsoleHelper.IsSelectionValid(m_nCurrentSelection, CGameData.GetPlatformTitles((CGameData.GamePlatform)m_nSelectedPlatform).Count))
					{
						m_nSelectedGame = m_nCurrentSelection;
						selectedGame = CGameData.GetPlatformGame((CGameData.GamePlatform)m_nSelectedPlatform, m_nSelectedGame);
						if (selectedGame != null) CLogger.LogDebug($"Selected game: {selectedGame.Title}");
					}
				}

				// Game selection required

				if (selectedGame != null)
				{
					switch ((CConsoleHelper.DockSelection)nSelectionCode)
					{
						case CConsoleHelper.DockSelection.cSel_Shortcut: // Export [a single] shortcut
							string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
							CLogger.LogInfo("Add shortcut: {0}\\{1}.LNK", desktopPath, selectedGame.Title);
							CConsoleHelper.MakeShortcut(selectedGame.Title, selectedGame.Launch, selectedGame.Icon, desktopPath);
							continue;

						case CConsoleHelper.DockSelection.cSel_Uninst: // Uninstall game
							if (!UninstallGame(selectedGame))
								Thread.Sleep(4000);
							else
							{
								CGameData.RemoveGame(selectedGame);
								CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
								m_nCurrentSelection--;
								if (CGameData.GetPlatformGameList((CGameData.GamePlatform)m_nSelectedPlatform).ToList().Count < 1)
									m_nSelectedPlatform = -1;
							} 
							continue;

						// TODO: Hide the game with a flag rather than removing the game from the database
						case CConsoleHelper.DockSelection.cSel_Hide: // Remove from list
							CLogger.LogInfo($"Hiding game: {selectedGame.Title}");
							CGameData.RemoveGame(selectedGame);
							CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
							m_nCurrentSelection--;
							if (CGameData.GetPlatformGameList((CGameData.GamePlatform)m_nSelectedPlatform).ToList().Count < 1)
								m_nSelectedPlatform = -1;
							continue;
						/*
						case CConsoleHelper.DockSelection.cSel_Hide: // Toggle game hidden
							CLogger.LogInfo($"Toggling hidden: {selectedGame.Title}");
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
							CLogger.LogInfo($"Toggling favourite: {selectedGame.Title}");
							CGameData.ToggleFavourite(
								(CGameData.GamePlatform)m_nSelectedPlatform,
								m_nCurrentSelection,
								CConsoleHelper.m_SortMethod == CConsoleHelper.SortMethod.cSort_Alpha,
								(bool)CConfig.GetConfigBool(CConfig.CFG_USEFAVE),
								(bool)CConfig.GetConfigBool(CConfig.CFG_USEINST),
								true);
							CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
							if (m_nSelectedPlatform == (int)CGameData.GamePlatform.Favourites &&
								CGameData.GetPlatformGameList(CGameData.GamePlatform.Favourites).ToList().Count < 1)
								m_nSelectedPlatform = -1;
							continue;

						case CConsoleHelper.DockSelection.cSel_Alias: // Set alias for insert mode or command line parameter
							string newAlias = InputPrompt(string.Format("Alias [{0}] >>> ", selectedGame.Alias), cols);
							if (!string.IsNullOrEmpty(newAlias))
							{
								CLogger.LogInfo("Set alias to: {0} for {1}", newAlias, selectedGame.Title);
								selectedGame.Alias = newAlias;
							}
							else
								CLogger.LogInfo("No alias provided.");
							continue;

						default:
							break;
					}
				}
				else
				{
					if ((CConsoleHelper.DockSelection)nSelectionCode == CConsoleHelper.DockSelection.cSel_Shortcut) // Export [multiple] shortcuts
						browse = true;
					else if ((CConsoleHelper.DockSelection)nSelectionCode == CConsoleHelper.DockSelection.cSel_Launcher) // Open launcher
                    {
						CLogger.LogDebug("Run Launcher: {0}", CGameData.GetDescription((CGameData.GamePlatform)m_nSelectedPlatform));
						try
						{
							switch ((CGameData.GamePlatform)m_nSelectedPlatform)
							{
								case CGameData.GamePlatform.Steam:
									Process.Start("steam://open/games");
									break;
								case CGameData.GamePlatform.GOG:
									using (RegistryKey key = Registry.LocalMachine.OpenSubKey(CRegScanner.GOG_REG_CLIENT, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
									{
										string launcherPath = key.GetValue("client") + "\\GalaxyClient.exe";
										if (File.Exists(launcherPath))
											Process.Start(launcherPath);
										else
										{
											//SetFgColour(cols.errorCC, cols.errorLtCC);
											CLogger.LogWarn("Cannot start {0} launcher.", CRegScanner.GOG_NAME.ToUpper());
											Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
											//Console.ResetColor();
										}
									}
									break;
								case CGameData.GamePlatform.Uplay:
									Process.Start("uplay://");
									break;
								case CGameData.GamePlatform.Origin:
									Process.Start("origin://");
									//Process.Start("eadm://");  // This was added by EA Desktop, but for now "origin://" and "origin2://" still work with it
									break;
								case CGameData.GamePlatform.Epic:
									Process.Start("com.epicgames.launcher://");
									break;
								case CGameData.GamePlatform.Bethesda:
									Process.Start("bethesdanet://");
									break;
								case CGameData.GamePlatform.Battlenet:
									Process.Start("battlenet://"); // "blizzard://" works too [TODO: is one more compatible with older versions?]
									break;
								case CGameData.GamePlatform.Rockstar:
									Process.Start("rockstar://");
									break;
								case CGameData.GamePlatform.Amazon:
									Process.Start("amazon-games://");
									break;
								case CGameData.GamePlatform.BigFish:
									using (RegistryKey key = Registry.LocalMachine.OpenSubKey(CRegScanner.BIGFISH_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
									{
										string launcherPath = key.GetValue("InstallationPath") + "\\bfgclient.exe";
										if (File.Exists(launcherPath))
											Process.Start(launcherPath);
										else
										{
											//SetFgColour(cols.errorCC, cols.errorLtCC);
											CLogger.LogWarn("Cannot start {0} launcher.", CRegScanner.BIGFISH_NAME.ToUpper());
											Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
											//Console.ResetColor();
										}
									}
									break;
								case CGameData.GamePlatform.Arc:
									Process.Start("arc://");
									break;
								case CGameData.GamePlatform.Itch:
									Process.Start("itch://library/");
									break;
								case CGameData.GamePlatform.Paradox:
									using (RegistryKey key = Registry.LocalMachine.OpenSubKey(CRegScanner.PARADOX_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
									{
										string launcherPath = key.GetValue(CRegScanner.PARADOX_PATH) + "\\Paradox Launcher.exe";
										if (File.Exists(launcherPath))
											Process.Start(launcherPath);
										else
										{
											//SetFgColour(cols.errorCC, cols.errorLtCC);
											CLogger.LogWarn("Cannot start {0} launcher.", CRegScanner.PARADOX_NAME.ToUpper());
											Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
											//Console.ResetColor();
										}
									}
									break;
								case CGameData.GamePlatform.Plarium:
									Process.Start("plariumplay://");
									break;
								//case CGameData.GamePlatform.Twitch: // TODO?
								//	break;
								case CGameData.GamePlatform.Wargaming:
									Process.Start("wgc://");
									break;
								case CGameData.GamePlatform.IGClient:
									using (RegistryKey key = Registry.LocalMachine.OpenSubKey(CRegScanner.IG_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM64
									{
										Process igcProcess = new Process();
										string launcherPath = key.GetValue("InstallLocation") + "\\IGClient.exe";
										if (File.Exists(launcherPath))
										{
											igcProcess.StartInfo.FileName = launcherPath;
											igcProcess.StartInfo.UseShellExecute = false;
											igcProcess.StartInfo.RedirectStandardOutput = true;
											igcProcess.StartInfo.RedirectStandardError = true;
											igcProcess.Start();
										}
										else
										{
											//SetFgColour(cols.errorCC, cols.errorLtCC);
											CLogger.LogWarn("Cannot start {0} launcher.", CRegScanner.IG_NAME.ToUpper());
											Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
											//Console.ResetColor();
										}
									}
									break;
								default:
									break;
							}
						}
						catch (Exception e)
						{
							//SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogError(e, "Cannot start launcher.");
							Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
							//Console.ResetColor();
						}
						m_nSelectedPlatform = -1; // This prevents the platform from being chosen, but selection ends up going back to the first item in the list
					}

				}

				if (m_nSelectedGame > -1 && selectedGame != null)
				{
					if (selectedGame.IsInstalled)
					{
						CGameData.NormaliseFrequencies(selectedGame);
						matches = new List<CGameData.CMatch>() { new CGameData.CMatch(selectedGame.Title, 1, 100) };
						CJsonWrapper.ExportSearch(matches);
						if (!cfgv.dontSaveChanges) CJsonWrapper.ExportConfig();

						Console.ResetColor();
						Console.CursorVisible = true;
						if (!noInteractive) Console.Clear();
						else Console.WriteLine();
#if !DEBUG
						if (!StartGame(selectedGame))
						{
							SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogWarn("Remove game from file.");
							Console.WriteLine("{0} will be removed from the list.", selectedGame.Title);
							Console.ResetColor();
							CGameData.RemoveGame(selectedGame);
						}
#else
						// DEBUG MODE
						// Make sure we've written to configuraiton file *before* this point, as we're setting overrides to CConfig.config below
						// Don't run the game in debug mode
						Console.BackgroundColor = ConsoleColor.Black;
						Console.ForegroundColor = ConsoleColor.Gray;
						Console.WriteLine("       ID : {0}", selectedGame.ID);
						Console.WriteLine("    Title : {0}", selectedGame.Title);
						Console.WriteLine("    Alias : {0}", selectedGame.Alias);
						Console.WriteLine("   Launch : {0}", selectedGame.Launch);
						Console.WriteLine("     Icon : {0}", selectedGame.Icon);
						Console.WriteLine("Uninstall : {0}", selectedGame.Uninstaller);
						Console.WriteLine();
						Console.WriteLine("DEBUG mode - game will not be launched; press Enter to exit...");
						if (noInteractive)
							cfgv.imageSize = 0;
						else
						{
							cfgv.imageSize = 32;
							CConfig.SetConfigValue(CConfig.CFG_IMGPOS, 100);
							CConfig.SetConfigValue(CConfig.CFG_IMGRES, 256);
							cfgv.imageBorder = true;
							CConfig.SetConfigValue(CConfig.CFG_IMGRTIO, false);
						}
						if (cfgv.imageSize > 0)
						{
							CConsoleImage.GetImageProperties(cfgv.imageSize, (ushort)CConfig.GetConfigNum(CConfig.CFG_IMGPOS), out sizeImage, out locImage);
							if (cfgv.imageBorder)
							{
								CConsoleImage.ShowImageBorder(sizeImage, locImage, IMG_BORDER_X_CUSHION, IMG_BORDER_Y_CUSHION);
								CConsoleImage.ShowImage(m_nCurrentSelection, selectedGame.Title, selectedGame.Icon, false, sizeImage, locImage, CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Light ? cols.bgLtCC : cols.bgCC);
								Console.SetCursorPosition(0, 8);
							}
							else
								CConsoleImage.ShowImage(m_nCurrentSelection, selectedGame.Title, selectedGame.Icon, false, sizeImage, locImage, CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Light ? cols.bgLtCC : cols.bgCC);
						}
						Console.ReadLine();
#endif
						CGameData.ClearNewGames();
						CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
						return;
					}
					else  // game not installed
                    {
						if (!InstallGame(selectedGame, cols))
							Thread.Sleep(4000);
						else
						{
							CGameData.RemoveGame(selectedGame);
							CRegScanner.ScanGames((bool)CConfig.GetConfigBool(CConfig.CFG_USECUST), !(bool)CConfig.GetConfigBool(CConfig.CFG_IMGSCAN), false);
							m_nCurrentSelection--;
							if (CGameData.GetPlatformGameList((CGameData.GamePlatform)m_nSelectedPlatform).ToList().Count < 1)
								m_nSelectedPlatform = -1;
						}
					}
				}
			}
		}

		/// <summary>
		/// Display menu and handle the selection
		/// </summary>
		private void MenuSwitchboard(CConfig.ConfigVolatile cfgv, CConfig.Hotkeys keys, CConfig.Colours cols, ref bool setup, ref bool browse, ref string path, ref string gameSearch, ref int nSelectionCode)
		{
			// Show initial options - platforms or all
			// Take the selection as a string (we'll figure out the enum later)
			// Display the options related to the initial selection (all games will show everything)
			//	Allow cancel with escape (make sure to print that in the heading)
			//  Run selected game.

			nSelectionCode = -1;

			List<string> platforms = CConsoleHelper.GetPlatformNames();
			if (platforms.Count < 1)
			{
				SetFgColour(cols.errorCC, cols.errorLtCC);
				CLogger.LogWarn("No games found!");
				Console.WriteLine("ERROR: No games found!");
				Console.ResetColor();
				System.Environment.Exit(-1);
			}

			if (!noInteractive)
			{
				if ((bool)CConfig.GetConfigBool(CConfig.CFG_USEALL))
					m_nSelectedPlatform = (int)CGameData.GamePlatform.All;
				else if (!(platforms.Contains(CGameData.GetPlatformString(CGameData.GamePlatform.Favourites))) &&
					platforms.Count < 3)  // if there's only one platform + All, then choose All
				{
					CLogger.LogDebug("Only one valid platform found.");
					m_nSelectedPlatform = (int)CGameData.GamePlatform.All;
				}
			}

			if (setup)                          // show setting menu
			{
				ushort oldImgSize = cfgv.imageSize;
				cfgv.imageSize = 0;
				ushort oldIconSize = cfgv.iconSize;
				cfgv.iconSize = 0;
				bool oldListView = cfgv.listView;
				cfgv.listView = true;

				nSelectionCode = m_dockConsole.DisplayCfg(cfgv, keys, cols, ref setup);

				cfgv.listView = oldListView;
				cfgv.imageSize = oldImgSize;
				cfgv.iconSize = oldIconSize;

				// TODO: Do setup

				nSelectionCode = (int)CConsoleHelper.DockSelection.cSel_Redraw;
				m_nCurrentSelection = 0;
				m_nSelectedGame = -1;
				m_nSelectedPlatform = -1;
				setup = false;
			}
			else if (browse)                    // show filesystem browser
			{
				string oldPath = path;
				ushort oldImgSize = cfgv.imageSize;
				cfgv.imageSize = 0;
				ushort oldIconSize = cfgv.iconSize;
				cfgv.iconSize = 0;

				nSelectionCode = m_dockConsole.DisplayFS(cfgv, keys, cols, ref browse, ref path);

				if (!string.IsNullOrEmpty(path) && oldPath.Equals(path) && !path.Equals(CConfig.GetConfigString(CConfig.CFG_TXTROOT)))
				{
					int ww = 0;
					string strPlatform = CGameData.GetPlatformString(m_nSelectedPlatform);
					cfgv.imageSize = oldImgSize;
					cfgv.iconSize = oldIconSize;
					string answer = InputPrompt($"Create shortcuts for \"{strPlatform}\" here [y/n]? >>> ", cols);
					ClearInputLine(cols);
					if (answer[0] == 'Y' || answer[0] == 'y')
					{
						try
						{
							ww = Console.WindowWidth;
							SetBgColour(cols.bgCC, cols.bgLtCC);
							Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - INPUT_BOTTOM_CUSHION);
						}
						catch (Exception e)
						{
							CLogger.LogError(e);
						}
						//SetFgColour(cols.titleCC, cols.titleLtCC);
						CLogger.LogInfo($"Creating shortcuts for \"{strPlatform}\" in {path}");
						int x = 0;
						int nGame = 0;
						CGameData.GamePlatform platform = (CGameData.GamePlatform)m_nSelectedPlatform;

						foreach (string strGame in CGameData.GetPlatformTitles(platform))
						{
							CGameData.CGame game = CGameData.GetPlatformGame(platform, nGame);
							if (game != null)
								CConsoleHelper.MakeShortcut(game.Title, game.Launch, game.Icon, path);
							if (x > ww)
							{
								x = 0;
								ClearInputLine(cols);
							}
							Console.Write(".");
							x++;
							nGame++;
						}
					}
					nSelectionCode = (int)CConsoleHelper.DockSelection.cSel_Redraw;
					m_nSelectedGame = -1;
					m_nSelectedPlatform = -1;
					browse = false;
				}
			}
			else if (m_nSelectedPlatform < 0)   // show main menu (platform list)
				nSelectionCode = m_dockConsole.DisplayMenu(cfgv, keys, cols, ref gameSearch, platforms.ToArray());
			else if (m_nSelectedGame < 0)       // show game list
				nSelectionCode = m_dockConsole.DisplayMenu(cfgv, keys, cols, ref gameSearch, CGameData.GetPlatformTitles((CGameData.GamePlatform)m_nSelectedPlatform).ToArray());
		}

		/// <summary>
		/// Install the game
		/// </summary>
		/// <param name="game"></param>
		private bool InstallGame(CGameData.CGame game, CConfig.Colours cols)
		{
			try
			{
				Console.SetCursorPosition(0, Console.WindowHeight - INPUT_BOTTOM_CUSHION);
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			try
			{
				switch (game.Platform)
				{
					case CGameData.GamePlatform.Steam:
						string answerSteam = InputPrompt($"Install game {game.Title} [y/n]? >>> ", cols);
						ClearInputLine(cols);
						if (answerSteam[0] == 'Y' || answerSteam[0] == 'y')
						{
							Console.ResetColor();
							Console.Clear();
							CLogger.LogInfo($"Installing game: {game.Title}");
							Process.Start(string.Format("steam://install/{0}", CRegScanner.GetSteamGameID(game.ID)));
							return true;
						}
						return false;
					case CGameData.GamePlatform.Amazon:
						string answerAmazon = InputPrompt($"Install game {game.Title} [y/n]? >>> ", cols);
						ClearInputLine(cols);
						if (answerAmazon[0] == 'Y' || answerAmazon[0] == 'y')
						{
							Console.ResetColor();
							Console.Clear();
							CLogger.LogInfo($"Installing game: {game.Title}");
							Process.Start(string.Format($"amazon-games://play/{game.ID}"));
							return true;
						}
						return false;
					case CGameData.GamePlatform.Itch:
						string answerItch = InputPrompt($"Install game {game.Title} [y/n]? >>> ", cols);
						ClearInputLine(cols);
						if (answerItch[0] == 'Y' || answerItch[0] == 'y')
						{
							Console.ResetColor();
							Console.Clear();
							CLogger.LogInfo($"Installing game: {game.Title}");
							Process.Start(string.Format("itch://games/{0}", CRegScanner.GetItchGameID(game.ID)));
							return true;
						}
						return false;
					case CGameData.GamePlatform.IGClient:
						using (RegistryKey key = Registry.LocalMachine.OpenSubKey(CRegScanner.IG_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM64
						{
							Process igcProcess = new Process();
							string launcherPath = key.GetValue("InstallLocation") + "\\IGClient.exe";
							if (File.Exists(launcherPath))
							{
								string answerIG = InputPrompt($"Install game {game.Title} [y/n]? >>> ", cols);
								ClearInputLine(cols);
								if (answerIG[0] == 'Y' || answerIG[0] == 'y')
								{
									CLogger.LogInfo($"Installing game: {game.Title}");
									igcProcess.StartInfo.FileName = launcherPath;
									igcProcess.StartInfo.UseShellExecute = false;
									igcProcess.StartInfo.RedirectStandardOutput = true;
									igcProcess.StartInfo.RedirectStandardError = true;
									igcProcess.Start();
									return true;
								}
							}
							else
							{
								//SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("Cannot start {0} launcher.", CRegScanner.IG_NAME.ToUpper());
								Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
								//Console.ResetColor();
							}
						}
						return false;
					case CGameData.GamePlatform.Paradox:
						using (RegistryKey key = Registry.LocalMachine.OpenSubKey(CRegScanner.PARADOX_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
						{
							string launcherPath = key.GetValue(CRegScanner.PARADOX_PATH) + "\\Paradox Launcher.exe";
							if (File.Exists(launcherPath))
							{
								string answerParadox = InputPrompt($"Install game {game.Title} [y/n]? >>> ", cols);
								ClearInputLine(cols);
								if (answerParadox[0] == 'Y' || answerParadox[0] == 'y')
								{
									Process.Start(launcherPath);
									return true;
								}
							}
							else
							{
								//SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("Cannot start {0} launcher.", CRegScanner.PARADOX_NAME.ToUpper());
								Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
								//Console.ResetColor();
							}
						}
						return false;
					default:
						//SetFgColour(cols.errorCC, cols.errorLtCC);
						CLogger.LogWarn("Install not supported for this platform.");
						Console.WriteLine("Install not supported for this platform.");
						//Console.ResetColor();
						return false;
				}
			}
			catch (Exception e)
			{
				//SetFgColour(cols.errorCC, cols.errorLtCC);
				CLogger.LogError(e, "Cannot start launcher.");
				Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
				//Console.ResetColor();
				return false;
			}
		}

		/// <summary>
		/// Uninstall the game
		/// </summary>
		/// <param name="game"></param>
		private bool UninstallGame(CGameData.CGame game)
		{
			try
			{
				Console.SetCursorPosition(0, Console.WindowHeight - INPUT_BOTTOM_CUSHION);
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			if (string.IsNullOrEmpty(game.Uninstaller))
			{
				//SetFgColour(cols.errorCC, cols.errorLtCC);
				CLogger.LogWarn("Uninstaller not found.");
				Console.WriteLine("An uninstaller wasn't found.");
				//Console.ResetColor();
				return false;
			}
			Console.ResetColor();
			Console.Clear();
			CLogger.LogInfo($"Uninstalling game: {game.Title}");
			Console.WriteLine($"Uninstalling game: {game.Title}");
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
			CLogger.LogInfo($"Starting game: {game.Title}");
			Console.WriteLine($"Starting game: {game.Title}");
			try
			{
				switch (game.Platform)
				{
					case CGameData.GamePlatform.GOG:
						//CLogger.LogInfo("Setting up a {0} game...", CRegScanner.GOG_NAME_LONG);
						ProcessStartInfo gogProcess = new ProcessStartInfo();
						string gogClientPath = game.Launch.Contains(".") ? game.Launch.Substring(0, game.Launch.IndexOf('.') + 4) : game.Launch;
						string gogArguments = game.Launch.Contains(".") ? game.Launch.Substring(game.Launch.IndexOf('.') + 4) : String.Empty;
						gogProcess.FileName = gogClientPath;
						gogProcess.Arguments = gogArguments;
						Process.Start(gogProcess);
						break;
					default:
						Process.Start(game.Launch);
						break;
				}
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
		private void DisplayHelp(int height, CConfig.Colours cols)
		{
			int line = 0;

			Console.Clear();
			WriteWithBreak(ref line, height, cols.titleCC, cols.titleLtCC, cols.titleCC, cols.titleLtCC,
				string.Format("{0} version {1}", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product,
				Assembly.GetEntryAssembly().GetName().Version.ToString()));
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
				if (!(bool)CConfig.GetConfigBool(CConfig.CFG_USEALL)) //&& !(bool)config.onlyCustom)
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
						"        /back | /b");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"         /nav | /n");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"        /scan | /s");
				/*
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					("    /alias | /a");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					("/uninstall | /uninst | /u");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"     /view | /v");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"     /grid | /g");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"     /list | /l");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"   /colour | /color | /col");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"    /light | /lt");
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"     /dark | /dk");
				*/
				if (!(bool)CConfig.GetConfigBool(CConfig.CFG_NOQUIT))
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
						"   /exit | /x | /quit | /q");
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
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYHELP1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYHELP2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"            Left: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYLT1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYLT2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"              Up: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYUP1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYUP2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"           Right: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYRT1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYRT2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"            Down: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYDN1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYDN2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"          Select: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYSEL1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYSEL2)), "[", "]", " | ", "N/A", 8));
				if (!(bool)CConfig.GetConfigBool(CConfig.CFG_USEALL)) //&& !(bool)config.onlyCustom)
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"            Back: " +
						CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYBACK1)),
						CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYBACK2)), "[", "]", " | ", "N/A", 8));
				/*
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"        Settings: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYCFG1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYCFG2)), "[", "]", " | ", "N/A", 8));
				*/
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"          Search: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYFIND1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYFIND2)), "[", "]", " | ", "N/A", 8));
				/*
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"Search Auto-Comp: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYTAB1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYTAB2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"   Search Cancel: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYESC1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYESC2)), "[", "]", " | ", "N/A", 8));
				*/
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"         Page Up: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYPGUP1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYPGUP2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"       Page Down: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYPGDN1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYPGDN2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"           First: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYHOME1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYHOME2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"            Last: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYEND1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYEND2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"    Rescan Games: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYSCAN1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYSCAN2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"  Start Launcher: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYPLAT1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYPLAT2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"Toggle Favourite: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYFAVE1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYFAVE2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"       Set Alias: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYALIAS1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYALIAS2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"       Hide Game: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYHIDE1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYHIDE2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"  Uninstall Game: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYUNIN1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYUNIN2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"Export Shortcuts: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYCUT1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYCUT2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"  Nav/Type Input: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYTYPE1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYTYPE2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"  Grid/List View: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYVIEW1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYVIEW2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					" Dark/Light Mode: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYMODE1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYMODE2)), "[", "]", " | ", "N/A", 8));
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"   Toggle Images: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYIMG1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYIMG2)), "[", "]", " | ", "N/A", 8));
				/*
				WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
					"     Sort Method: " +
					CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYSORT1)),
					CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYSORT2)), "[", "]", " | ", "N/A", 8));
				*/
				if (!(bool)CConfig.GetConfigBool(CConfig.CFG_NOQUIT))
					WriteWithBreak(ref line, height, cols.entryCC, cols.entryLtCC, cols.titleCC, cols.titleLtCC,
						"            Quit: " +
						CConsoleHelper.OutputKeys(CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYQUIT1)),
						CConfig.ShortenKeyName(CConfig.GetConfigString(CConfig.CFG_KEYQUIT2)), "[", "]", " | ", "N/A", 8));
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
		public static void DisplayUsage(CConfig.Colours cols, string parent, List<CGameData.CMatch> matches)
		{
			Console.WriteLine();
			//				  0|-------|---------|---------|---------|---------|---------|---------|---------|80
			SetFgColour(cols.titleCC);
			Console.WriteLine("{0} version {1}",
				Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product,
				Assembly.GetEntryAssembly().GetName().Version.ToString());
			Console.WriteLine(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>().Description);
			Console.WriteLine();
			Console.WriteLine("Usage:");
			Console.ResetColor();
			Console.WriteLine("Start in interactive mode by running with no parameters,");
			Console.WriteLine("or launch a game by entering search on the command-line, e.g.:");
			Console.WriteLine(" .\\{0} \"My Game\"", FILENAME);
			Console.WriteLine("If there are multiple results in command-line mode, to select an item from a");
			Console.WriteLine("prior search, enter e.g.:");
			Console.WriteLine(" .\\{0} /2", FILENAME);
			Console.WriteLine();
			SetFgColour(cols.titleCC);
			Console.WriteLine("Other parameters:");
			Console.ResetColor();
			Console.WriteLine("   /1 : Replay the previously launched game");
			if (matches.Count > 0)
			{
				SetFgColour(cols.titleCC);
				Console.WriteLine("        [{0}]", matches[0].m_strTitle);
				Console.ResetColor();
			}
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
			if (colour > (ConsoleColor)(-1))
			{
				if (Console.BackgroundColor != colour)
					Console.ForegroundColor = colour;
			}
		}

		/// <summary>
		/// Set foreground colour for light or dark mode
		/// </summary>
		public static void SetFgColour(ConsoleColor colour, ConsoleColor lightColour)
		{
			if (CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Light && lightColour > (ConsoleColor)(-1))
			{
				if (Console.BackgroundColor != lightColour)
					Console.ForegroundColor = lightColour;
			}
			else if (colour > (ConsoleColor)(-1))
			{
				if (Console.BackgroundColor != colour)
					Console.ForegroundColor = colour;
			}
		}

		/// <summary>
		/// Draw background on input line
		/// </summary>
		public static string InputPrompt(string prompt, CConfig.Colours cols)
		{
			SetFgColour(cols.inputCC, cols.inputLtCC);
			try
			{
				int y = Console.WindowTop + Console.WindowHeight - INPUT_BOTTOM_CUSHION;
				Console.SetCursorPosition(0, y);
				SetBgColour(cols.inputbgCC, cols.inputbgLtCC);
				for (int i = 0; i < Console.WindowWidth; ++i)
				{
					Console.Write(" ");
				}
				Console.SetCursorPosition(0, y);
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			Console.Write(prompt);
			Console.CursorVisible = true;
			return Console.ReadLine();
		}

		/// <summary>
		/// Draw background on input line
		/// </summary>
		public static void ClearInputLine(CConfig.Colours cols)
		{
			try
			{
				int y = Console.WindowTop + Console.WindowHeight - INPUT_BOTTOM_CUSHION;
				SetBgColour(cols.bgCC, cols.bgLtCC);
				Console.SetCursorPosition(0, y);
				for (int i = 0; i < Console.WindowWidth; ++i)
				{
					Console.Write(" ");
				}
				Console.SetCursorPosition(0, y);
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
		}

		/// <summary>
		/// Clear image with appropriate light or dark mode
		/// </summary>
		public static void ClearColour(ConsoleColor bgDark, ConsoleColor bgLight)
		{
			if (CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Light && bgLight > (ConsoleColor)(-1))
				CConsoleImage.ClearImage(sizeImage, locImage, bgLight);
			else if (CConsoleHelper.m_LightMode == CConsoleHelper.LightMode.cColour_Dark && bgDark > (ConsoleColor)(-1))
				CConsoleImage.ClearImage(sizeImage, locImage, bgDark);
			else
				CConsoleImage.ClearImage(sizeImage, locImage, ConsoleColor.Black);
		}

		/// <summary>
		/// Check whether current shell has known issues with interactive applications and image display
		/// <returns>bool, whether interactive mode is supported</returns>
		/// </summary>
		public static void CheckShellCapabilities(ref CConfig.ConfigVolatile cfgv, out string parentName)
		{
			parentName = "";
			bool shellError = false;
			try
			{
				parentName = ParentProcessUtilities.GetParentProcess().ProcessName;
				CLogger.LogInfo($"Parent process: {parentName}");
				/*
				if (parentName.Equals("explorer") && !(bool)CConfig.GetConfigBool(CConfig.CFG_USECMD))
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
					CLogger.LogInfo($"Parent process parent: {parentParentName}");
					
					// With PowerCmd, Console.Write() is not visible at all, so we can't even show an error message!
					if (parentParentName.Equals("PowerCmd"))
					{
						if (!(bool)CConfig.GetConfigBool(CConfig.CFG_USECMD))
						{
							//SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogWarn("{0} host not supported.", parentParentName);
							Console.WriteLine("ERROR: Your {0} host is not not supported.", parentParentName);
							Console.ResetColor();
							shellError = true;
							noInteractive = true;
						}
					}
					else if (!((bool)CConfig.GetConfigBool(CConfig.CFG_USECMD)))
					{
						if (parentName.Equals("tcc") &&            // many keys won't work with combination of ConEmu64 + Take Command (tcc)
								 parentParentName.Equals("ConEmuC64"))  // (though tcc is fine otherwise)
						{
							if (!(bool)CConfig.GetConfigBool(CConfig.CFG_USECMD) ||
								(!cfgv.typingInput &&
								// This isn't really the best test, but it's for a corner case...
								(CConfig.GetConfigString(CConfig.CFG_KEYUP1).Equals("UpArrow") ||
								CConfig.GetConfigString(CConfig.CFG_KEYUP2).Equals("UpArrow"))))
							{
								//SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("Many keys (arrows, F1-F12) do not work in {0} host with {1}.\nSwitching to typing input...", parentParentName, parentName);
								Console.WriteLine("WARNING: Many keys (arrows, F1-F12) do not work in {0} host with {1}.\nSwitching to typing input...", parentParentName, parentName);
								//Console.ResetColor();
								cfgv.typingInput = true;
								shellError = true;
							}
						}

						else if (parentParentName.Equals("FireCMD"))    // Displays menus, but colours aren't displayed, making navigation mode nigh useless
						{
							if (!(bool)CConfig.GetConfigBool(CConfig.CFG_USETYPE))
							{
								//SetFgColour(cols.errorCC, cols.errorLtCC);
								/*
								CLogger.LogWarn("{0} host not supported.", parentParentName);
								Console.WriteLine("ERROR: Your {0} host is not supported.", parentParentName);
								//Console.ResetColor();
								return;
								*/
								CLogger.LogWarn("{0} host does not support navigation mode.\nSwitching input state...", CConfig.CFG_USETYPE, parentParentName);
								Console.WriteLine("WARNING: {0} == false, but your {1} host does not support this.\nSwitching input state...", CConfig.CFG_USETYPE, parentParentName);
								//Console.ResetColor();
								cfgv.typingInput = true;
								shellError = true;
							}
							if ((ushort)CConfig.GetConfigNum(CConfig.CFG_IMGSIZE) > 0 || (ushort)CConfig.GetConfigNum(CConfig.CFG_ICONSIZE) > 0)
							{
								//SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("{0} host does not support images.\nDisabling images...", parentParentName);
								Console.WriteLine("WARNING: {0} or \n{1} > 0, but your {2} host does not support this.\nDisabling images...", CConfig.CFG_IMGSIZE, CConfig.CFG_ICONSIZE, parentParentName);
								//Console.ResetColor();
								cfgv.iconSize = 0;
								cfgv.imageSize = 0;
								shellError = true;
							}
						}
						/*
						else if (parentParentName.Equals("ZTW64"))		// I have inconsistently observed weird issues with newline characters
						{
							//SetFgColour(cols.errorCC, cols.errorLtCC);
							CLogger.LogWarn("{0} host sometimes has display issues.", parentParentName);
							Console.WriteLine("WARNING: Your {0} host may have display issues.", parentParentName);
							//Console.ResetColor();
							shellError = true;
						}
						*/
						else
						{
							// check for known non-conhost terminal hosts (images not supported)
							if (((ushort)CConfig.GetConfigNum(CConfig.CFG_IMGSIZE) > 0 || (ushort)CConfig.GetConfigNum(CConfig.CFG_ICONSIZE) > 0) &&
								(parentParentName.Equals("WindowsTerminal") ||			// Windows Terminal
								 parentParentName.StartsWith("ServiceHub.Host.CLR") ||	// Visual Studio 
								 parentParentName.Equals("Code") ||						// Visual Studio Code
								 parentParentName.Equals("Hyper") ||					// Hyper
								 parentParentName.Equals("tcmd") ||						// Take Command
								 parentParentName.Equals("bash") ||						// Cygwin Terminal (mintty), though this one may give false negatives (MSYS in 
								 parentParentName.Equals("Console")))					// Console2 or ConsoleZ
							{
								//TODO: Show this warning only once?

								//SetFgColour(cols.errorCC, cols.errorLtCC);
								CLogger.LogWarn("{0} host does not support images.\nDisabling images...", parentParentName);
								Console.WriteLine("WARNING: {0} or \n{1} > 0, but your {2} host does not support this.\nDisabling images...", CConfig.CFG_IMGSIZE, CConfig.CFG_ICONSIZE, parentParentName);
								//Console.ResetColor();
								cfgv.iconSize = 0;
								cfgv.imageSize = 0;
								shellError = true;
							}
						}
					}
				}

				//TODO: Enable this warning after we have a method to show it only once.

				/*
				CConsoleImage.CONSOLE_FONT_INFO_EX currentFont = new CConsoleImage.CONSOLE_FONT_INFO_EX();
				CConsoleImage.GetCurrentConsoleFontEx(CConsoleImage.GetStdHandle(CConsoleImage.STD_OUTPUT_HANDLE), false, currentFont);
				if (currentFont.FaceName.Equals("Terminal"))
				{
					//SetFgColour(cols.errorCC, cols.errorLtCC);
					CLogger.LogWarn("Terminal set to \"Raster Fonts,\" which may display incorrect characters.");
					Console.WriteLine("WARNING: Your terminal is set to \"Raster Fonts,\" which may display incorrect characters.");
					//Console.ResetColor();
					shellError = true;
				}
				*/
			}
			catch (Exception e)
			{
				CLogger.LogError(e, "INFO: Couldn't get parent process. Terminal host may not be supported.");
			}
			if (shellError)
				Thread.Sleep(4000);
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
						CLogger.LogDebug($"New PATH: {newPath}");
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
			int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out uint _); // out uint returnLength);
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
