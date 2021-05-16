using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace GameLauncher_Console
{
	[ComImport]
	[Guid("00021401-0000-0000-C000-000000000046")]
	internal class ShellLink
	{
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("000214F9-0000-0000-C000-000000000046")]
	internal interface IShellLink
	{
		void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
		void GetIDList(out IntPtr ppidl);
		void SetIDList(IntPtr pidl);
		void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
		void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
		void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
		void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
		void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
		void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
		void GetHotkey(out short pwHotkey);
		void SetHotkey(short wHotkey);
		void GetShowCmd(out int piShowCmd);
		void SetShowCmd(int iShowCmd);
		void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
		void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
		void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
		void Resolve(IntPtr hwnd, int fFlags);
		void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
	}

	/// <summary>
	/// Console implementation for this project
	/// </summary>
	public class CConsoleHelper
	{
		/// <summary>
		/// Console state enum.
		/// The console supports two states:
		/// * Type	: User can type out commands
		/// * Browse: User can use the WASD keys to browse the items and enter/space to select item (early stage gui)
		/// </summary>
		public enum ConsoleState
		{
			cState_Unknown = -1,
			cState_Insert = 0,
			cState_Navigate = 1
		};

		/// <summary>
		/// Menu type enum.
		/// Menu elements can be displayed as either:
		/// * Grid: Print the elements across X columns (column number is controlled by m_OptionsPerLine)
		/// * List: Print the elements in a single list (m_nOptionsPerLine should be set to 1 to support arrow selection)
		/// </summary>
		public enum MenuType
		{
			cType_Unknown = -1,
			cType_List = 0,
			cType_Grid = 1
		}

		/// <summary>
		/// Colour mode enum.
		/// Colours can be switched between two sets of customizable presets:
		/// * Dark
		/// * Light
		/// </summary>
		public enum LightMode
		{
			cColour_Unknown = -1,
			cColour_Light = 0,
			cColour_Dark = 1
		}

		/// <summary>
		/// Image mien enum.
		/// Images can be enabled or disabled.
		/// </summary>
		public enum ImageMien
		{
			cImage_Unknown = -1,
			cImage_Disabled = 0,
			cImage_Enabled = 1
		}       
		
		/// <summary>
		/// Menu type enum.
		/// Menu elements can be displayed as either:
		/// * Grid: Print the elements across X columns (column number is controlled by m_OptionsPerLine)
		/// * List: Print the elements in a single list (m_nOptionsPerLine should be set to 1 to support arrow selection)
		/// </summary>
		public enum SortMethod
		{
			cSort_Unknown = -1,
			cSort_Alpha = 0,
			cSort_Freq = 1
		}

		public static ConsoleState m_ConsoleState = ConsoleState.cState_Unknown;
		public static MenuType m_MenuType = MenuType.cType_Unknown;
		public static LightMode m_LightMode = LightMode.cColour_Unknown;
		public static ImageMien m_ImageMien = ImageMien.cImage_Unknown;
		public static SortMethod m_SortMethod = SortMethod.cSort_Unknown;

		public static int m_nOptionsPerLine = 1;   // Control the number of columns
		public static int m_nSpacingPerLine = 15;  // Control the width of the columns
		public static int m_nMaxItemsPerPage = 1;

		/// <summary>
		/// Constructor:
		/// Call base class constructor
		/// </summary>
		/// <param name="nColumnCount">Numer of columns</param>
		/// <param name="nSpacing">Spacing between columns</param>
		/// <param name="state">Initial console state</param>
		public CConsoleHelper(int nColumnCount, int nSpacing, int nPaging) //, ConsoleState state) : base(nColumnCount, nSpacing, state)
		{
			m_nOptionsPerLine = Math.Max(1, nColumnCount);
			m_nSpacingPerLine = Math.Max(15, nSpacing);
			m_nMaxItemsPerPage = Math.Max(m_nOptionsPerLine, nPaging);
			//m_ConsoleState = (state == ConsoleState.cState_Unknown) ? ConsoleState.cState_Navigate : state;
			//m_MenuType = (type == MenuType.cType_Unknown) ? ((nColumnCount > 1) ? MenuType.cType_Grid : MenuType.cType_List) : type;
			//m_LightMode = (mode == LightMode.cColour_Unknown) ? LightMode.cColour_Dark : mode;
			//m_ImageMien = (mien == ImageMien.cImage_Unknown) ? ImageMien.cImage_Enabled : mien;
			//m_SortMethod = (method == SortMethod.cSort_Unknown) ? SortMethod.cSort_Freq : method;
		}

		/// <summary>
		/// Selection options for special commands
		/// -1 should be used as the default state
		/// Once a command such as exit or back is executed, the state should be set to -1.
		/// </summary>
		public enum DockSelection
		{
			cSel_Search = -18,
			cSel_Sort = -17,
			cSel_Image = -16,
			cSel_Colour = -15,
			cSel_View = -14,
			cSel_Input = -13,
			cSel_Hide = -12,
			cSel_Desktop = -11,
			cSel_Alias = -10,
			cSel_Uninst = -9,
			cSel_Redraw = -8,
			cSel_Fail = -7,
			cSel_Help = -6,
			cSel_Exit = -5,
			cSel_Back = -4,
			cSel_Fav = -3,
			cSel_Rescan = -2,
			cSel_Default = -1
		}

		/// <summary>
		/// Switch the console state
		/// </summary>
		public static void SwitchState()
		{
			m_ConsoleState = (ConsoleState)(((int)m_ConsoleState + 1) % 2);
		}

		/// <summary>
		/// Switch the menu type
		/// </summary>
		public static void SwitchType()
		{
			m_MenuType = (MenuType)(((int)m_MenuType + 1) % 2);
		}

		/// <summary>
		/// Switch the colour mode
		/// </summary>
		public static void SwitchMode()
		{
			m_LightMode = (LightMode)(((int)m_LightMode + 1) % 2);
		}

		/// <summary>
		/// Switch whether images are displayed
		/// </summary>
		public static void SwitchMien()
		{
			m_ImageMien = (ImageMien)(((int)m_ImageMien + 1) % 2);
		}

		/// <summary>
		/// Switch the colour mode
		/// </summary>
		public static void SwitchMethod()
		{
			m_SortMethod = (SortMethod)(((int)m_SortMethod + 1) % 2);
		}

		/// <summary>
		/// Function overload
		/// Display either the insert or navigate menu, depending on the console state.
		/// Returns selection code and selection index
		/// </summary>
		/// <param name="options">String array representing the available options</param>
		/// <returns>Selection code</returns>
		public int DisplayMenu(CConfig.Configuration config, CConfig.Hotkeys keys, CConfig.Colours cols, ref string game, params string[] options)
		{
			int code; //= -1;

			CDock.SetBgColour(cols.bgCC, cols.bgLtCC);
			if (!(bool)config.onlyCmdLine) Console.Clear();

			DrawMenuTitle(config, cols);
			if (IsSelectionValid(CDock.m_nSelectedPlatform, options.Length)) DrawInstruct(config, cols, true);
			else DrawInstruct(config, cols, false);
			int nStartY = Console.CursorTop + CDock.INSTRUCT_CUSHION;
			int nStopY = 2;
			if (m_ConsoleState == ConsoleState.cState_Insert)
				nStopY = CDock.INPUT_BOTTOM_CUSHION + CDock.INPUT_ITEM_CUSHION;

			do
			{
				if (m_ConsoleState == ConsoleState.cState_Insert)
				{
					code = HandleInsertMenu(nStartY, nStopY, config, cols, ref game, options);
					CLogger.LogDebug("HandleInsertMenu:{0},{1}", code, CDock.m_nCurrentSelection);
				}
				else //if (m_ConsoleState == ConsoleState.cState_Navigate)
				{
					code = HandleNavigationMenu(nStartY, nStopY, config, keys, cols, ref game, options);
					CLogger.LogDebug("HandleNavigationMenu:{0},{1}", code, CDock.m_nCurrentSelection);
				}
				if (code == (int)DockSelection.cSel_Exit    ||
					code == (int)DockSelection.cSel_Default)
					return code;
				if (options.Length == 0)
				{
					CDock.m_nCurrentSelection = -1;
					return code;
				}
			} while (!IsSelectionValid(CDock.m_nCurrentSelection, options.Length));
			return code;
		}

		/// <summary>
		/// Function override
		/// Validate selection
		/// </summary>
		/// <param name="nSelection">Selection as integer</param>
		/// <param name="nItemCount">Count of the possible selections</param>
		/// <returns>True if valid, otherwise false</returns>
		public static bool IsSelectionValid(int nSelection, int nItemCount)
		{
			return (nSelection > -1 && nSelection < nItemCount);
		}

		/// <summary>
		/// Function overload
		/// Selection handler in the browse state.
		/// Return selection code and selection index
		/// </summary>
		/// <param name="options">Array of available options</param>
		/// <returns>Selection code</returns>
		public int HandleNavigationMenu(int nStartY, int nStopY, CConfig.Configuration config, CConfig.Hotkeys keys, CConfig.Colours cols, ref string game, params string[] options)
		{
			Console.CursorVisible = false;

			ConsoleKey key;
			int nLastSelection = CDock.m_nCurrentSelection;

			if ((bool)config.sizeToFit)
			{
				// If the list items won't fit in the current window, resize to fit
				int numLines = (options.Length / m_nOptionsPerLine) + nStartY + CDock.INPUT_BOTTOM_CUSHION;
				if (numLines > Console.WindowHeight)
				{
					Console.WindowHeight = Math.Min(numLines, Console.LargestWindowHeight);
					m_nMaxItemsPerPage = Math.Max(m_nOptionsPerLine, Console.WindowHeight * m_nOptionsPerLine);
				}
			}

			// Print the selections
			int itemsPerPage = m_nMaxItemsPerPage - (m_nOptionsPerLine * (nStartY + nStopY));
			if (itemsPerPage < 1)
				return (int)DockSelection.cSel_Fail;
			int nPage = 0;
			if (CDock.m_nCurrentSelection >= itemsPerPage)
				nPage = CDock.m_nCurrentSelection / itemsPerPage;
			int pages = options.Length / itemsPerPage + (options.Length % itemsPerPage == 0 ? 0 : 1);
			CLogger.LogDebug("total items:{0}, lines:{1}, items per pg:{2}, page:{3}/{4}", options.Length, m_nMaxItemsPerPage, itemsPerPage, nPage, pages);

			if (!string.IsNullOrEmpty(game))
			{
				int maxTitles = (Math.Min(9, Console.WindowHeight - nStartY - CDock.INPUT_BOTTOM_CUSHION - CDock.INPUT_ITEM_CUSHION - 1));
				options = CGameData.GetPlatformTitles(CGameData.GamePlatform.All).ToArray();
				if (GameSearch(game, (bool)config.onlyCmdLine, maxTitles, options, out string[] optionsNew, out int nMatches) && nMatches > 0)
				{
					CDock.m_nSelectedPlatform = (int)CGameData.GamePlatform.Search;
					options = optionsNew;
					if ((bool)config.onlyCmdLine)
					{
						if (optionsNew.Length == 1) //nMatches == 1)
							return (int)DockSelection.cSel_Default;
						return (int)DockSelection.cSel_Exit;
					}

					//DrawMenuTitle(config, cols);
				}
				else
				{
					//CDock.m_nCurrentSelection = 0;
					CDock.m_nSelectedPlatform = -1;
					CDock.SetFgColour(cols.errorCC, cols.errorLtCC);
					Console.WriteLine("{0}: {1}!", CGameData.GetDescription(CGameData.GamePlatform.Search), CGameData.GetDescription(CGameData.Match.NoMatches));
					if ((bool)config.onlyCmdLine)
					{
						options = new string[0];
						return (int)DockSelection.cSel_Exit;
					}

					if ((bool)config.goToAll)
					{
						CDock.m_nSelectedPlatform = (int)CGameData.GamePlatform.All;
						//DrawMenuTitle(config, cols);
					}
					else
						options = GetPlatformNames(config).ToArray();
				}
				game = "";
			}

			if ((int)config.imageSize > 0 && (bool)config.imageBorder)
				CConsoleImage.ShowImageBorder(CDock.imgSize, CDock.imgLoc, CDock.IMG_BORDER_X_CUSHION, CDock.IMG_BORDER_Y_CUSHION);

			if (m_MenuType == MenuType.cType_List)
				DrawListMenu(CDock.m_nCurrentSelection, nPage, nStartY, nStopY, config, cols, options);
			
			else //if (m_MenuType == MenuType.cType_Grid)
				DrawGridMenu(CDock.m_nCurrentSelection, nPage, nStartY, nStopY, config, cols, options);
			
			if (!(bool)config.noPageSplit && pages > 1 && nPage < pages - 1)
			{
				CDock.SetFgColour(cols.subCC, cols.subLtCC);
				Console.WriteLine("... ({0}/{1})", nPage + 1, pages);
			}

			do
			{
				// Track the current selection
				if (CDock.m_nCurrentSelection != nLastSelection && IsSelectionValid(nLastSelection, options.Length) &&
					IsSelectionValid(CDock.m_nCurrentSelection, options.Length))
					UpdateMenu(nLastSelection, nPage,
						(int)config.iconSize > 0 ? CDock.ICON_LEFT_CUSHION + (int)config.iconSize + CDock.ICON_RIGHT_CUSHION: 0,
						nStartY, itemsPerPage, config, cols, options);

				if (m_MenuType == MenuType.cType_List && (int)config.iconSize > 0)
                {
					ConsoleColor iconColour = (int)config.iconRes > 48 ? ConsoleColor.Black : (m_LightMode == LightMode.cColour_Light ? cols.bgLtCC : cols.bgCC);
					for (int i = itemsPerPage * nPage; i > itemsPerPage * (nPage + 1); ++i)
					{
						DrawIcon(Console.CursorTop, i, options[i], config, iconColour);
					}
				}

				if ((int)config.imageSize > 0 && IsSelectionValid(CDock.m_nCurrentSelection, options.Length))
				{
					ConsoleColor imageColour = (int)config.imageRes > 48 ? ConsoleColor.Black : (m_LightMode == LightMode.cColour_Light ? cols.bgLtCC : cols.bgCC);
					var t = Task.Run(() =>
					{
						if (CDock.m_nSelectedPlatform > -1)
						{
							CGameData.CGame selectedGame = CGameData.GetPlatformGame((CGameData.GamePlatform)CDock.m_nSelectedPlatform, CDock.m_nCurrentSelection);
							CConsoleImage.ShowImage(CDock.m_nCurrentSelection, selectedGame.Title, selectedGame.Icon, false, CDock.imgSize, CDock.imgLoc, config, imageColour);
						}
						else
							CConsoleImage.ShowImage(CDock.m_nCurrentSelection, options[CDock.m_nCurrentSelection], options[CDock.m_nCurrentSelection], true, CDock.imgSize, CDock.imgLoc, config, imageColour);
					});
				}

				key = Console.ReadKey(true).Key;
				nLastSelection = CDock.m_nCurrentSelection;

				CLogger.LogDebug($"READKEY: {key}");
				if (key == keys.leftCK1 || key == keys.leftCK2)
				{
					if (CDock.m_nCurrentSelection > 0)
					{
						HandleSelectionLeft();
						CLogger.LogDebug("Selected {0}: [{1}]", CDock.m_nCurrentSelection, options[CDock.m_nCurrentSelection]);
					}
				}
				else if (key == keys.rightCK1 || key == keys.rightCK2)
				{
					if (CDock.m_nCurrentSelection < options.Length - 1)
					{
						HandleSelectionRight(options.Length);
						CLogger.LogDebug("Selected {0}: [{1}]", CDock.m_nCurrentSelection, options[CDock.m_nCurrentSelection]);
					}
				}
				else if (key == keys.upCK1 || key == keys.upCK2)
				{
					if (CDock.m_nCurrentSelection > 0)
					{
						HandleSelectionUp(itemsPerPage);
						CLogger.LogDebug("Selected {0}: [{1}]", CDock.m_nCurrentSelection, options[CDock.m_nCurrentSelection]);
						if (!(bool)config.noPageSplit && CDock.m_nCurrentSelection < itemsPerPage * nPage)
							return (int)DockSelection.cSel_Redraw;
					}
				}
				else if (key == keys.downCK1 || key == keys.downCK2)
				{
					if (CDock.m_nCurrentSelection < options.Length - 1)
					{
						HandleSelectionDown(itemsPerPage, options.Length);
						CLogger.LogDebug("Selected {0}: [{1}]", CDock.m_nCurrentSelection, options[CDock.m_nCurrentSelection]);
						if (!(bool)config.noPageSplit && CDock.m_nCurrentSelection >= itemsPerPage * (nPage + 1))
							return (int)DockSelection.cSel_Redraw;
					}
				}
				else if (key == keys.pageUpCK1 || key == keys.pageUpCK2)
				{
					if (CDock.m_nCurrentSelection > 0)
					{
						HandleSelectionPageUp(itemsPerPage, options.Length);
						CLogger.LogDebug("Selected {0}: [{1}]", CDock.m_nCurrentSelection, options[CDock.m_nCurrentSelection]);
						if (!(bool)config.noPageSplit && nPage > 0)
							return (int)DockSelection.cSel_Redraw;
					}
				}
				else if (key == keys.pageDownCK1 || key == keys.pageDownCK2)
				{
					if (CDock.m_nCurrentSelection < options.Length - 1)
					{
						HandleSelectionPageDown(itemsPerPage, options.Length);
						CLogger.LogDebug("Selected {0}: [{1}]", CDock.m_nCurrentSelection, options[CDock.m_nCurrentSelection]);
						if (!(bool)config.noPageSplit && nPage < pages)
							return (int)DockSelection.cSel_Redraw;
					}
				}
				else if (key == keys.searchCK1 || key == keys.searchCK2)
					return (int)DockSelection.cSel_Search; // Game search
				else if (key == keys.firstCK1 || key == keys.firstCK2)
				{
					if (CDock.m_nCurrentSelection > 0)
					{
						HandleSelectionFirst();
						CLogger.LogDebug("Selected {0}: [{1}]", CDock.m_nCurrentSelection, options[CDock.m_nCurrentSelection]);
						if (!(bool)config.noPageSplit && pages > 1)
							return (int)DockSelection.cSel_Redraw;
					}
				}
				else if (key == keys.lastCK1 || key == keys.lastCK2)
				{
					if (CDock.m_nCurrentSelection < options.Length - 1)
					{
						HandleSelectionLast(options.Length);
						CLogger.LogDebug("Selected {0}: [{1}]", CDock.m_nCurrentSelection, options[CDock.m_nCurrentSelection]);
						if (!(bool)config.noPageSplit && pages > 1)
							return (int)DockSelection.cSel_Redraw;
					}
				}
				else if (key == keys.imageCK1 || key == keys.imageCK2)
					return (int)DockSelection.cSel_Image; // Images mien
				
				else if (key == keys.sortCK1 || key == keys.sortCK2)
					return (int)DockSelection.cSel_Sort; // Alphabetic/Frequency Sort method

				else if (key == keys.modeCK1 || key == keys.modeCK2)
					return (int)DockSelection.cSel_Colour; // Dark/Light Colour mode

				else if (key == keys.viewCK1 || key == keys.viewCK2)
					return (int)DockSelection.cSel_View; // Grid/List view

				else if (key == keys.typeCK1 || key == keys.typeCK2)
					return (int)DockSelection.cSel_Input; // Arrows/Typing input

				else if (key == keys.desktopCK1 || key == keys.desktopCK2)
                {
					if (CDock.m_nSelectedPlatform > -1)
						return (int)DockSelection.cSel_Desktop; // Add shortcut to Desktop
                }
				else if (key == keys.uninstCK1 || key == keys.uninstCK2)
				{
					if (CDock.m_nSelectedPlatform > -1)
						return (int)DockSelection.cSel_Uninst; // Uninstall program
				}
				else if (key == keys.helpCK1 || key == keys.helpCK2)
					return (int)DockSelection.cSel_Help; // Print help

				else if (key == keys.quitCK1 || key == keys.quitCK2)
				{
					if (!(bool)config.preventQuit)
						return (int)DockSelection.cSel_Exit; // Exit program
				}
				else if (key == keys.backCK1 || key == keys.backCK2)
				{
					if (CDock.m_nSelectedPlatform > -1 && !((bool)config.goToAll))
						return (int)DockSelection.cSel_Back; // Return to first menu
				}
				else if (key == keys.hideCK1 || key == keys.hideCK2)
				{
					if (CDock.m_nSelectedPlatform > -1)
						return (int)DockSelection.cSel_Hide; // Remove game from list
				}
				else if (key == keys.faveCK1 || key == keys.faveCK2)
				{
					if (CDock.m_nSelectedPlatform > -1)
						return (int)DockSelection.cSel_Fav; // Add/remove game from favourites
				}
				else if (key == keys.aliasCK1 || key == keys.aliasCK2)
				{
					if (CDock.m_nSelectedPlatform > -1)
						return (int)DockSelection.cSel_Alias; // Set alias for insert mode or command line parameter
				}
				else if (key == keys.scanCK1 || key == keys.scanCK2)
					return (int)DockSelection.cSel_Rescan; // Rescan the registry and the 'customGames' folder for new games

			} while (key != keys.selectCK1 && key != keys.selectCK2);
			
			return -1;
		}

		/// <summary>
		/// Function overload
		/// Selection handler in the 'insert' mode
		/// </summary>
		/// <param name="options">List of menu items</param>
		/// <returns>Index of the selected item from the options parameter</returns>
		public int HandleInsertMenu(int nStartY, int nStopY, CConfig.Configuration config, CConfig.Colours cols, ref string game, params string[] options)
		{
			Console.CursorVisible = true;

			int nSelection = -1;
			bool bIsValidSelection = false;

			if ((bool)config.sizeToFit)
			{
				// If the list items won't fit in the current window, resize to fit
				int numLines = (options.Length / m_nOptionsPerLine) + nStartY + CDock.INPUT_BOTTOM_CUSHION;
				if (numLines > Console.WindowHeight)
				{
					Console.WindowHeight = Math.Min(numLines, Console.LargestWindowHeight);
					m_nMaxItemsPerPage = Math.Max(m_nOptionsPerLine, Console.WindowHeight * m_nOptionsPerLine);
				}
			}

			// Print the selections
			int itemsPerPage = m_nMaxItemsPerPage - (m_nOptionsPerLine * (nStartY + nStopY));
			if (itemsPerPage < 1)
				return (int)DockSelection.cSel_Fail;
			int nPage = 0;
			if (nSelection >= itemsPerPage)
				nPage = nSelection / itemsPerPage;
			int pages = options.Length / itemsPerPage + (options.Length % itemsPerPage == 0 ? 0 : 1);
			CLogger.LogDebug("total items:{0}, lines:{1}, items per pg:{2}, page:{3}/{4}", options.Length, m_nMaxItemsPerPage, itemsPerPage, nPage, pages);

			if (!string.IsNullOrEmpty(game))
			{
				int maxTitles = (Math.Min(9, Console.WindowHeight - nStartY - CDock.INPUT_BOTTOM_CUSHION - CDock.INPUT_ITEM_CUSHION - 1));
				options = CGameData.GetPlatformTitles(CGameData.GamePlatform.All).ToArray();
				if (GameSearch(game, (bool)config.onlyCmdLine, maxTitles, options, out string[] optionsNew, out int nMatches) && nMatches > 0)
				{
					CDock.m_nSelectedPlatform = (int)CGameData.GamePlatform.Search;
					options = optionsNew;
					if ((bool)config.onlyCmdLine)
					{
						if (optionsNew.Length == 1) //nMatches == 1)
							return (int)DockSelection.cSel_Default;
						return (int)DockSelection.cSel_Exit;
					}

					//DrawMenuTitle(config, cols);
				}
				else
				{
					nSelection = -1;
					CDock.m_nSelectedPlatform = -1;
					CDock.SetFgColour(cols.errorCC, cols.errorLtCC);
					Console.WriteLine("{0}: {1}!", CGameData.GetDescription(CGameData.GamePlatform.Search), CGameData.GetDescription(CGameData.Match.NoMatches));
					if ((bool)config.onlyCmdLine)
					{
						options = new string[0];
						return (int)DockSelection.cSel_Exit;
					}

					if ((bool)config.goToAll)
					{
						CDock.m_nSelectedPlatform = (int)CGameData.GamePlatform.All;
						//DrawMenuTitle(config, cols);
					}
					else
						options = GetPlatformNames(config).ToArray();
				}
				game = "";
			}

			if ((int)config.imageSize > 0 && (bool)config.imageBorder)
				CConsoleImage.ShowImageBorder(CDock.imgSize, CDock.imgLoc, CDock.IMG_BORDER_X_CUSHION, CDock.IMG_BORDER_Y_CUSHION);
			
			if (m_MenuType == MenuType.cType_List)
				DrawListMenu(nSelection, nPage, nStartY, nStopY, config, cols, options);

			else //if (m_MenuType == MenuType.cType_Grid)
				DrawGridMenu(nSelection, nPage, nStartY, nStopY, config, cols, options);
			
			if (!(bool)config.noPageSplit && pages > 1 && nPage < pages - 1)
			{
				CDock.SetFgColour(cols.subCC, cols.subLtCC);
				Console.WriteLine("... ({0}/{1})", nPage + 1, pages);
			}

			do
			{
				// Set the cursor to the bottom of the console
				try
				{
					Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - CDock.INPUT_BOTTOM_CUSHION);
				}
				catch (Exception e)
				{
					CLogger.LogError(e);
				}
				CDock.SetFgColour(cols.titleCC, cols.titleLtCC);
				Console.Write(">>> ");
				string strInput;
				strInput = Console.ReadLine();

				if (strInput.Length < 1) // Empty string input are invalid
					continue;

				else if (strInput.Equals("/help", CDock.IGNORE_CASE) || strInput.Equals("/h", CDock.IGNORE_CASE) ||
					strInput.Equals("/?", CDock.IGNORE_CASE))
					return (int)DockSelection.cSel_Help;

				else if (strInput.Equals("/scan", CDock.IGNORE_CASE) || strInput.Equals("/s", CDock.IGNORE_CASE))
					return (int)DockSelection.cSel_Rescan;

				else if (strInput.Equals("/back", CDock.IGNORE_CASE) || strInput.Equals("/b", CDock.IGNORE_CASE))
					return (int)DockSelection.cSel_Back;

				else if (strInput.Equals("/nav", CDock.IGNORE_CASE) || strInput.Equals("/n", CDock.IGNORE_CASE) ||
					strInput.Equals("/input", CDock.IGNORE_CASE) || strInput.Equals("/i", CDock.IGNORE_CASE))
					return (int)DockSelection.cSel_Input;

				else if (strInput.Equals("/view", CDock.IGNORE_CASE) || strInput.Equals("/v", CDock.IGNORE_CASE))
					return (int)DockSelection.cSel_View;

				else if (strInput.Equals("/grid", CDock.IGNORE_CASE) || strInput.Equals("/g", CDock.IGNORE_CASE))
				{
					m_MenuType = (MenuType)MenuType.cType_Grid;
					return -1;
				}
				else if (strInput.Equals("/list", CDock.IGNORE_CASE) || strInput.Equals("/l", CDock.IGNORE_CASE))
				{
					m_MenuType = (MenuType)MenuType.cType_List;
					return -1;
				}
				else if (strInput.Equals("/icon", CDock.IGNORE_CASE))
					return (int)DockSelection.cSel_Image;

				else if (strInput.Equals("/sort", CDock.IGNORE_CASE))
					return (int)DockSelection.cSel_Sort;

				else if (strInput.Equals("/alpha", CDock.IGNORE_CASE))
				{
					m_SortMethod = (SortMethod)SortMethod.cSort_Alpha;
					return -1;
				}
				else if (strInput.Equals("/freq", CDock.IGNORE_CASE))
				{
					m_SortMethod = (SortMethod)SortMethod.cSort_Freq;
					return -1;
				}
				else if (strInput.Equals("/colour", CDock.IGNORE_CASE) || strInput.Equals("/color", CDock.IGNORE_CASE) ||
					strInput.Equals("/col", CDock.IGNORE_CASE) || strInput.Equals("/mode", CDock.IGNORE_CASE))
					return (int)DockSelection.cSel_Colour;

				else if (strInput.Equals("/dark", CDock.IGNORE_CASE) || strInput.Equals("/dk", CDock.IGNORE_CASE))
				{
					m_LightMode = (LightMode)LightMode.cColour_Dark;
					return -1;
				}
				else if (strInput.Equals("/light", CDock.IGNORE_CASE) || strInput.Equals("/li", CDock.IGNORE_CASE))
				{
					m_LightMode = (LightMode)LightMode.cColour_Light;
					return -1;
				}
				else if (strInput.Equals("/exit", CDock.IGNORE_CASE) || strInput.Equals("/x", CDock.IGNORE_CASE) ||
					strInput.Equals("/quit", CDock.IGNORE_CASE) || strInput.Equals("/q", CDock.IGNORE_CASE))
					return (int)DockSelection.cSel_Exit;

			} while (!bIsValidSelection);

			return nSelection;
		}

		public void DrawIcon(int nY, int nItem, string strItem, CConfig.Configuration config, ConsoleColor iconColour)
        {
			//CLogger.LogDebug("test {0}x{1} = {2} ({3})", CDock.ICON_LEFT_CUSHION, nY, strItem, nItem);
			var t = Task.Run(() =>
			{
				Point iconPosition = new Point(CDock.ICON_LEFT_CUSHION, Console.WindowHeight / m_nMaxItemsPerPage + nY);
				if (CDock.m_nSelectedPlatform > -1)
				{
					CGameData.CGame selectedGame = CGameData.GetPlatformGame((CGameData.GamePlatform)CDock.m_nSelectedPlatform, nItem);
					CConsoleImage.ShowImage(CDock.m_nCurrentSelection, selectedGame.Title, selectedGame.Icon, false, CDock.iconSize, iconPosition, config, iconColour);
				}
				else
					CConsoleImage.ShowImage(CDock.m_nCurrentSelection, strItem, strItem, true, CDock.iconSize, iconPosition, config, iconColour);
			});
		}

		/// <summary>
		/// Function overload
		/// Draw the list options in a grid layout.
		/// </summary>
		/// <param name="nCursorPosition">Current position, which will be printed in a highlight colour</param>
		/// <param name="nStartY">Offset from the top of the window</param>
		/// <param name="itemList">List of items to be displayed</param>
		public void DrawGridMenu(int nCursorPosition, int nPage, int nStartY, int nStopY, CConfig.Configuration config, CConfig.Colours cols, params string[] itemList)
		{
			int itemsPerPage = m_nMaxItemsPerPage - (m_nOptionsPerLine * (nStartY + nStopY));
			int startIndex = 0;
			if (!(bool)config.noPageSplit)
				startIndex = nPage * itemsPerPage;

			CDock.SetFgColour(cols.entryCC, cols.entryLtCC);
			for (int i = startIndex; i < itemList.Length; ++i)
			{
				if (!(bool)config.noPageSplit && i >= itemsPerPage * (nPage + 1))
					break;
				try
				{
					Console.SetCursorPosition((i % m_nOptionsPerLine) * m_nSpacingPerLine,
						nStartY + ((i - (nPage * itemsPerPage)) / m_nOptionsPerLine));
				}
				catch (Exception e)
				{
					CLogger.LogError(e);
				}
				CLogger.LogDebug("Item {0} @ {1}x{2}x{3}", i, Console.CursorLeft, Console.CursorTop, nPage);
				if (i == nCursorPosition)
				{
					CDock.SetBgColour(cols.highbgCC, cols.highbgLtCC);
					CDock.SetFgColour(cols.highlightCC, cols.highlightLtCC);
				}
				Console.WriteLine(itemList[i].Substring(0, Math.Min(itemList[i].Length, m_nSpacingPerLine - CDock.COLUMN_CUSHION)));
				CDock.SetBgColour(cols.bgCC, cols.bgLtCC);
				CDock.SetFgColour(cols.entryCC, cols.entryLtCC);
			}
		}

		/// <summary>
		/// Function overload
		/// Draw a list of options as a list
		/// </summary>
		/// <param name="nCursorPosition">Current position, which will be printed in a red colour</param>
		/// <param name="nStartY">Offset from the top of the window</param>
		/// <param name="itemList">List of items to be displayed</param>
		public void DrawListMenu(int nSelection, int nPage, int nStartY, int nStopY, CConfig.Configuration config, CConfig.Colours cols, params string[] itemList)
		{
			int itemsPerPage = m_nMaxItemsPerPage - (nStartY + nStopY);
			int startIndex = 0;
			if (!(bool)config.noPageSplit)
				startIndex = nPage * itemsPerPage;

			CDock.SetFgColour(cols.entryCC, cols.entryLtCC);
			ConsoleColor iconColour = (int)config.iconRes > 48 ? ConsoleColor.Black : (m_LightMode == LightMode.cColour_Light ? cols.bgLtCC : cols.bgCC);
			for (int i = startIndex; i < itemList.Length; ++i)
			{
				if (!(bool)config.noPageSplit && i >= itemsPerPage * (nPage + 1))
					break;
				try
				{
					Console.SetCursorPosition((int)config.iconSize > 0 ? CDock.ICON_LEFT_CUSHION + (int)config.iconSize + CDock.ICON_RIGHT_CUSHION : 0,
						nStartY + i - (nPage * itemsPerPage));
				}
				catch (Exception e)
				{
					CLogger.LogError(e);
				}
				CLogger.LogDebug("Item {0} @ {1}x{2}x{3}", i, Console.CursorLeft, Console.CursorTop, nPage);
				if (i == nSelection)
				{
					CDock.SetBgColour(cols.highbgCC, cols.highbgLtCC);
					CDock.SetFgColour(cols.highlightCC, cols.highlightLtCC);
				}
				Console.WriteLine(itemList[i]);
				CDock.SetBgColour(cols.bgCC, cols.bgLtCC);
				CDock.SetFgColour(cols.entryCC, cols.entryLtCC);
				if ((int)config.iconSize > 0)
					DrawIcon(Console.CursorTop - 2, i, itemList[i], config, iconColour);
			}
		}


		/// <summary>
		/// Handle selection calculation when 'up' is selected
		/// </summary>
		public void HandleSelectionUp(int nItemsPerPage)
		{
			if (m_MenuType == MenuType.cType_Grid && CDock.m_nCurrentSelection >= m_nOptionsPerLine)
				CDock.m_nCurrentSelection -= m_nOptionsPerLine;

			else if (m_MenuType == MenuType.cType_List && CDock.m_nCurrentSelection >= 1)
				CDock.m_nCurrentSelection--;
		}

		/// <summary>
		/// Handle selection calculation when 'down' is selected
		/// </summary>
		/// <param name="nOptionCount">Number of items in the list</param>
		public void HandleSelectionDown(int nItemsPerPage, int nOptionCount)
		{
			if (m_MenuType == MenuType.cType_Grid && CDock.m_nCurrentSelection + m_nOptionsPerLine < nOptionCount)
				CDock.m_nCurrentSelection += m_nOptionsPerLine;

			else if (m_MenuType == MenuType.cType_List && CDock.m_nCurrentSelection + 1 < nOptionCount)
				CDock.m_nCurrentSelection++;
		}

		/// <summary>
		/// Handle selection calculation when 'left' is selected
		/// </summary>
		public void HandleSelectionLeft()
		{
			if (CDock.m_nCurrentSelection > 0)
			{
				if (m_MenuType == MenuType.cType_List)
					CDock.m_nCurrentSelection--;

				else //if (m_MenuType == MenuType.cType_Grid)
				{
					if (CDock.m_nCurrentSelection % m_nOptionsPerLine > 0)
						CDock.m_nCurrentSelection--;
				}
			}
		}

		/// <summary>
		/// Handle selection calculation when 'right' is selected
		/// </summary>
		/// <param name="nOptionCount">Numbers of items in the list</param>
		public void HandleSelectionRight(int nOptionCount)
		{
			if (CDock.m_nCurrentSelection + 1 < nOptionCount)
			{
				if (m_MenuType == MenuType.cType_List)
					CDock.m_nCurrentSelection++;

				else //if (m_MenuType == MenuType.cType_Grid)
				{
					if (CDock.m_nCurrentSelection % m_nOptionsPerLine < m_nOptionsPerLine - 1)
						CDock.m_nCurrentSelection++;
				}
			}
		}

		/// <summary>
		/// Handle selection calculation when 'PgUp' is selected
		/// </summary>
		public void HandleSelectionPageUp(int nItemsPerPage, int nOptionCount)
		{
			//if (m_MenuType == MenuType.cType_Grid)
			//{
				if (CDock.m_nCurrentSelection >= m_nOptionsPerLine)
				{
					CDock.m_nCurrentSelection -= nItemsPerPage;
					while (CDock.m_nCurrentSelection < 0)
					{
						CDock.m_nCurrentSelection += m_nOptionsPerLine;
					}
					if (CDock.m_nCurrentSelection > nOptionCount - 1)
						CDock.m_nCurrentSelection = 0;
				}
				else if (CDock.m_nCurrentSelection >= 1)
					CDock.m_nCurrentSelection = 0;  // same as Home
			//} else CDock.m_nCurrentSelection = 0;  // same as Home
		}

		/// <summary>
		/// Handle selection calculation when 'PgDn' is selected
		/// </summary>
		/// <param name="nOptionCount">Number of items in the list</param>
		public void HandleSelectionPageDown(int nItemsPerPage, int nOptionCount)
		{
			//if (m_MenuType == MenuType.cType_Grid)
			//{
				if (CDock.m_nCurrentSelection + m_nOptionsPerLine < nOptionCount)
				{
					CDock.m_nCurrentSelection += nItemsPerPage;
					while (CDock.m_nCurrentSelection > nOptionCount - 1)
					{
						CDock.m_nCurrentSelection -= m_nOptionsPerLine;
					}
					if (CDock.m_nCurrentSelection < 0)
						CDock.m_nCurrentSelection = 0;
				}
				else if (CDock.m_nCurrentSelection + 1 < nOptionCount)
					CDock.m_nCurrentSelection = nOptionCount - 1;  // same as End
			//} else CDock.m_nCurrentSelection = nOptionCount - 1;  // same as End
		}

		/// <summary>
		/// Handle selection calculation when 'Home' is selected
		/// </summary>
		public void HandleSelectionFirst()
		{
			CDock.m_nCurrentSelection = 0;
		}

		/// <summary>
		/// Handle selection calculation when 'End' is selected
		/// </summary>
		/// <param name="nOptionCount">Number of items in the list</param>
		public void HandleSelectionLast(int nOptionCount)
		{
			CDock.m_nCurrentSelection = nOptionCount - 1;
		}

		/// <summary>
		/// Function overload
		/// Update the printed list of menu items by re-colouring only the changed items on the list
		/// </summary>
		/// <param name="nPreviousSelection">Index of the previously highlighted item</param>
		/// <param name="nStartY">Starting Y position (places from top)</param>
		/// <param name="strPreviousOption">String value of the previously selected option</param>
		/// <param name="strCurrentOption">String value of the currently selected option</param>
		public void UpdateMenu(int nPreviousSelection, int nPage, int nListStartX, int nStartY, int nItemsPerPage, CConfig.Configuration config, CConfig.Colours cols, params string[] options)
		{
			string strPreviousOption = options[nPreviousSelection];
			string strCurrentOption = options[CDock.m_nCurrentSelection];
			try
			{
				if (m_MenuType == MenuType.cType_List)
					Console.SetCursorPosition(
						nListStartX,
						nStartY + CDock.m_nCurrentSelection - (nPage * nItemsPerPage));

				else //if (m_MenuType == MenuType.cType_Grid)
					Console.SetCursorPosition(
						(CDock.m_nCurrentSelection % m_nOptionsPerLine) * m_nSpacingPerLine,
						nStartY + ((CDock.m_nCurrentSelection - (nPage * nItemsPerPage)) / m_nOptionsPerLine));
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}

			CDock.SetBgColour(cols.highbgCC, cols.highbgLtCC);
			CDock.SetFgColour(cols.highlightCC, cols.highlightLtCC);
			if (m_MenuType == MenuType.cType_List)
				Console.Write(strCurrentOption);
			else
				Console.Write(strCurrentOption.Substring(0, Math.Min(strCurrentOption.Length, m_nSpacingPerLine - CDock.COLUMN_CUSHION)));

			try
			{
				if (m_MenuType == MenuType.cType_List)
					Console.SetCursorPosition(
						nListStartX,
						nStartY + nPreviousSelection - (nPage * nItemsPerPage));

				else //if (m_MenuType == MenuType.cType_Grid)
					Console.SetCursorPosition(
						(nPreviousSelection % m_nOptionsPerLine) * m_nSpacingPerLine,
						nStartY + ((nPreviousSelection - (nPage * nItemsPerPage)) / m_nOptionsPerLine));
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}

			CDock.SetBgColour(cols.bgCC, cols.bgLtCC);
			CDock.SetFgColour(cols.entryCC, cols.entryLtCC);
			if (m_MenuType == MenuType.cType_List)
			{
				Console.Write(strPreviousOption);
				
				// Redraw all icons below current selection
				if (config.iconSize > 0)
				{
					ConsoleColor iconColour = (int)config.iconRes > 48 ? ConsoleColor.Black : (m_LightMode == LightMode.cColour_Light ? cols.bgLtCC : cols.bgCC);
					int maxItems = Math.Min(nItemsPerPage * (nPage + 1), options.Length);
					for (int i = nItemsPerPage * nPage + CDock.m_nCurrentSelection; i < maxItems; ++i)
					{
						string option = options[i];
						DrawIcon(nStartY + i - 1, i, option, config, iconColour);
					}
				}
			}
			else
				Console.Write(strPreviousOption.Substring(0, Math.Min(strPreviousOption.Length, m_nSpacingPerLine - CDock.COLUMN_CUSHION)));
		}

		/// <summary>
		/// Search for a game by title or alias
		/// </summary>
		public static List<string> GetPlatformNames(CConfig.Configuration config)
		{
			List<string> platforms = new List<string>();

			foreach (var platformPair in CGameData.GetPlatforms())
			{
				if (platformPair.Value > 0)
					platforms.Add(platformPair.Key + ": " + platformPair.Value);
			}
			if (!(bool)config.hideSettings) platforms.Add(CDock.SETTINGS_TITLE);
			return platforms;
		}

		/// <summary>
		/// Create shortcut
		/// </summary>
		public static void MakeShortcut(string title, string path, string icon, string location)
        {
			IShellLink link = (IShellLink)new ShellLink();

			// setup shortcut information
			if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(path))
				return;
			link.SetDescription(title);
			link.SetPath(path);
			if (!string.IsNullOrEmpty(icon))
				link.SetIconLocation(icon, 0);

			try
			{
				// save it
				IPersistFile file = (IPersistFile)link;
				if (Directory.Exists(location))
					file.Save(Path.Combine(location, title + ".LNK"), false);
			}
            catch (Exception e)
			{
				CLogger.LogError(e);
			}
		}

		/// <summary>
		/// Search for a game by title or alias
		/// </summary>
		public static bool GameSearch(string strInput, bool consoleOutput, int maxTitles, string[] options, out string[] optionsNew, out int nMatches)
        {
			Dictionary<string, int> matches = new Dictionary<string, int>();
			List<CGameData.CMatch> searchResults = new List<CGameData.CMatch>();
			bool bValid = false;
			/*
			nSelection = Array.FindIndex(options, s => s.Equals(strInput, CDock.IGNORE_CASE));
			if (nSelection >= 0)
			{
				CLogger.LogInfo("{0}: {1}! [{2}]", CGameData.GetDescription(CGameData.GamePlatform.Search), CGameData.GetDescription(CGameData.Match.ExactTitle), nSelection);
				nMatches = 1;
				optionsNew = new string[1];
				optionsNew[0] = CGameData.GetPlatformGame(CGameData.GamePlatform.All, nSelection).Title;
				nSelection = 0;
				bValid = true;
			}
			else
			{
			*/
				matches = CGameData.FindMatchingTitles(strInput, maxTitles);
				nMatches = matches.Count;
				if (nMatches > 0)
				{
					if (consoleOutput) Console.WriteLine("{0}: {1} matches found!", CGameData.GetDescription(CGameData.GamePlatform.Search), nMatches);
					CLogger.LogInfo("{0}: {1} matches found!", CGameData.GetDescription(CGameData.GamePlatform.Search), nMatches);
					int i = 0;
					optionsNew = new string[nMatches];
					foreach (var match in matches.OrderByDescending(j => j.Value))
					{
						CLogger.LogInfo("* [{0}%] {1}. {2}", match.Value, i, match.Key);
						if (i == 0 && match.Value >= (int)CGameData.Match.ExactAlias)
						{
							optionsNew = new string[1];
							optionsNew[0] = match.Key;
							CDock.m_nCurrentSelection = Array.FindIndex(options, s => s.Equals(match.Key, CDock.IGNORE_CASE));
							if (consoleOutput)
							{
								if (match.Value == (int)CGameData.Match.ExactTitle)
									Console.WriteLine("* {0}: {1}", CGameData.GetDescription(CGameData.Match.ExactTitle), match.Key);
								else
									Console.WriteLine("* {0}: {1}", CGameData.GetDescription(CGameData.Match.ExactAlias), match.Key);
								break;
							}
						}
						else optionsNew[i] = match.Key;

						if (consoleOutput)
						{
							Console.WriteLine(" {0}. {1}", i + 1, match.Key);
							searchResults.Add(new CGameData.CMatch(match.Key, i + 1, match.Value));
						}

						i++;
					}
					if (nMatches == 1) CDock.m_nCurrentSelection = 0;
					bValid = true;
				}
				else
				{
					CLogger.LogInfo("{0}: {1}!", CGameData.GetDescription(CGameData.GamePlatform.Search), CGameData.GetDescription(CGameData.Match.NoMatches));
					optionsNew = null;
				}
				if (consoleOutput) { CJsonWrapper.ExportSearch(searchResults); }
			//}
			return bValid;
		}

		/// <summary>
		/// Construct key output
		/// </summary>
		public static string OutputKeys(string key1, string key2, string bracketL, string bracketR, string separator, string na, int padding)
		{
			string output;

			if (string.IsNullOrEmpty(key1) || key1.Equals("NoName"))
			{
				if (string.IsNullOrEmpty(key2) || key2.Equals("NoName"))
					//($"{i,-4}{myArray[i]}")
					output = na;
				else
					output = bracketL + key2 + bracketR;
			}
			else
			{
				output = bracketL + key1 + bracketR;
				if (!string.IsNullOrEmpty(key2) && !(key2.Equals("NoName")))
				{
					output = output.PadRight(padding) + separator + bracketL + key2 + bracketR;
				}
			}

			return output;
		}

		/// <summary>
		/// Write menu title
		/// </summary>
		public static void DrawMenuTitle(CConfig.Configuration config, CConfig.Colours cols)
		{
			if (!(bool)config.onlyCmdLine)
			{
				CDock.SetFgColour(cols.titleCC, cols.titleLtCC);
				Console.SetCursorPosition(0, 0);
				if (CDock.m_nSelectedPlatform > -1)
				{
					Console.WriteLine(CGameData.GetPlatformString(CDock.m_nSelectedPlatform));
					Console.WriteLine(new String('-', CGameData.GetPlatformString(CDock.m_nSelectedPlatform).Length));
				}
				else
				{
					Console.WriteLine(config.mainTitle);
					Console.WriteLine(new String('-', config.mainTitle.Length));
				}
			}
		}
		
		/// <summary>
		/// Write menu instructions
		/// </summary>
		public static void DrawInstruct(CConfig.Configuration config, CConfig.Colours cols, bool subMenu)
		{
			if (!(bool)config.onlyCmdLine)
			{
				// MESSAGE 1
				//CDock.SetFgColour(cols.titleCC, cols.titleLtCC);
				CDock.SetFgColour(cols.subCC, cols.subLtCC);
				if (m_ConsoleState == ConsoleState.cState_Insert)
				{
					if ((bool)config.msgCustom)
					{
						if (subMenu)
							Console.WriteLine(config.subTextIns1);
						else
							Console.WriteLine(config.mainTextIns1);
					}
					else
					{
						if (subMenu)
							Console.WriteLine(" Type title of a game.");
						else
							Console.WriteLine(" Type name of a platform.");
					}
				}
				else // Navigate
				{
					if ((bool)config.msgCustom)
					{
						if (subMenu)
							Console.WriteLine(config.subTextNav1);
						else
							Console.WriteLine(config.mainTextNav1);
					}
					else
					{
						if (subMenu)
						{
							if (m_MenuType == (MenuType)MenuType.cType_Grid)
							{
								Console.WriteLine(" Use {0}/{1}/{2}/{3} + {4} to select a game.",
									OutputKeys(CConfig.ShortenKeyName(config.leftKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.upKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.rightKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.downKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.selectKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0));
							}
							else
							{
								Console.WriteLine(" Use {0}/{1} + {2} to select a game.",
									OutputKeys(CConfig.ShortenKeyName(config.upKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.downKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.selectKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0));
							}
						}
						else  // main menu
						{
							if (m_MenuType == (MenuType)MenuType.cType_Grid)
							{
								Console.WriteLine(" Use {0}/{1}/{2}/{3} + {4} to select platform.",
									OutputKeys(CConfig.ShortenKeyName(config.leftKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.upKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.rightKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.downKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
									OutputKeys(CConfig.ShortenKeyName(config.selectKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0));
							}
							else Console.WriteLine(" Use {0}/{1} + {2} to select platform.",
								OutputKeys(CConfig.ShortenKeyName(config.upKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
								OutputKeys(CConfig.ShortenKeyName(config.downKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0),
								OutputKeys(CConfig.ShortenKeyName(config.selectKey1), String.Empty, String.Empty, String.Empty, String.Empty, "NA", 0));
						}
					}
				}

				// MESSAGE 2
				//CDock.SetFgColour(cols.subCC, cols.subLtCC);
				if (m_ConsoleState == ConsoleState.cState_Insert)
				{
					if ((bool)config.msgCustom)
					{
						if (subMenu)
							Console.WriteLine(config.subTextIns2);
						else
							Console.WriteLine(config.mainTextIns2);
					}
					else
					{
						/*
						if (subMenu)
						{
							Console.WriteLine(((bool)config.preventQuit ? String.Empty :
								" Enter \'/exit\' to quit;\n") +
								" Enter \'/help\' for more commands.");
						}
						else  // main menu
						{
						*/
						Console.WriteLine(((bool)config.preventQuit ? String.Empty :
							" Enter \'/exit\' to quit;\n") +
							" Enter \'/help\' for more commands.");
						//}
					}
				}
				else // Navigate
				{
					if ((bool)config.msgCustom)
					{
						if (subMenu)
							Console.WriteLine(config.subTextNav2);
						else
							Console.WriteLine(config.mainTextNav2);
					}
					else
					{
						if (subMenu)
						{
							//  0|-------|---------|---------|---------|40
							Console.WriteLine(((bool)config.preventQuit ? String.Empty :
								" Press {0} to exit;\n") +
								" Press {1} to return to main;\n" +
								" Press {2} to toggle favourites.",
								OutputKeys(CConfig.ShortenKeyName(config.quitKey1), CConfig.ShortenKeyName(config.quitKey2), String.Empty, String.Empty, " or ", "N/A", 0),
								OutputKeys(CConfig.ShortenKeyName(config.backKey1), CConfig.ShortenKeyName(config.backKey2), String.Empty, String.Empty, " or ", "N/A", 0),
								OutputKeys(CConfig.ShortenKeyName(config.faveKey1), CConfig.ShortenKeyName(config.faveKey2), String.Empty, String.Empty, " or ", "N/A", 0));
						}
						else  // main menu
						{
							//  0|-------|---------|---------|---------|40
							Console.WriteLine(((bool)config.preventQuit ? String.Empty :
								" Press {0} to exit;\n") +
								" Press {1} to rescan games;\n" +
								" Press {2} for help.",
								OutputKeys(CConfig.ShortenKeyName(config.quitKey1), CConfig.ShortenKeyName(config.quitKey2), String.Empty, String.Empty, " or ", "N/A", 0),
								OutputKeys(CConfig.ShortenKeyName(config.scanKey1), CConfig.ShortenKeyName(config.scanKey2), String.Empty, String.Empty, " or ", "N/A", 0),
								OutputKeys(CConfig.ShortenKeyName(config.helpKey1), CConfig.ShortenKeyName(config.helpKey2), String.Empty, String.Empty, " or ", "N/A", 0));
						}
					}
				}
			}
		}
	}
}