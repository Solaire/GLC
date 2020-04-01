using System;
using System.Diagnostics;
using System.Linq;

namespace GameLauncher_Console
{
	/// <summary>
	/// Main program logic - this is where all classes meet and get used
	/// </summary>
	public class CDock
	{
		CDockConsole	m_dockConsole;
		private int		m_nFirstSelection;
		private int		m_nSecondSelection;

		/// <summary>
		/// Selection options for special commands
		/// -1 should be used as the default state
		/// Once a command such as exit or back is executed, the state should be set to -1.
		/// </summary>
		private enum DockSelection
		{
			cSel_Help		= -6,
			cSel_Exit		= -5,
			cSel_Back		= -4,
			cSel_Fav		= -3,
			cSel_Rescan		= -2,
			cSel_Default	= -1,
		}

		/// <summary>
		/// Array of string containing the content of the help screen.
		/// </summary>
		private readonly string[] m_helpLines =
		{
			"GameLauncherDock - version 1.0",
			"",
			" This program will scan the registry for installed video games and display them as a list.",
			" The games are stored in a JSON file called \'games.json\', found in the same folder as this program.",
			" Following platforms are supported:",
			" *	Steam",
			" *	GOG",
			" *	Uplay",
			" *	Origin",
			" *	Epic Games Launcher",
			" *	Battle.net",
			" *	Bethesda.net launcher",
			"",
			" To manually add games, place their binary file (.exe) or shortcut into the \'customGames\' folder.",
			" The folder is found in the same folder as this program.",
			" Using shortcuts is recommended, as single binary files will fail to run due to dependencies on other assets.",
			""
		};
		
		/// <summary>
		/// Constructor
		/// </summary>
		public CDock()
		{
			m_dockConsole	   = new CDockConsole(1, 5, CConsoleHelper.ConsoleState.cState_Navigate);
			m_nFirstSelection  = -1;
			m_nSecondSelection = -1;
		}

		/// <summary>
		/// Run the main program loop.
		/// Return when game is launched or the user decided to exit.
		/// </summary>
		public void MainLoop()
		{
			CJsonWrapper.ImportFromJSON();
			CGameFinder.CheckCustomFolder();

			int nSelectionCode, nSelectionIndex;
			for(; ; )
			{
				MenuSwitchboard(out nSelectionCode, out nSelectionIndex);

				switch((DockSelection)nSelectionCode)
				{
					case DockSelection.cSel_Help:
						DisplayHelp();
					continue;

					case DockSelection.cSel_Exit: // Exit application
						return;

					case DockSelection.cSel_Back: // Go back to first menu
						m_nFirstSelection = -1;
						continue;

					case DockSelection.cSel_Fav: // Toggle game favourite
						if(m_nFirstSelection > -1)
						{
							CGameData.ToggleFavourite((CGameData.GamePlatform)m_nFirstSelection, nSelectionIndex);
							CJsonWrapper.Export(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
						}
						continue;

					case DockSelection.cSel_Rescan: // Rescan the game list
						if(m_nFirstSelection < 0)
						{
							Console.Clear();
							Console.Write("Scanning for games...");
							Logger.CLogger.LogInfo("Scanning for games...");
							CRegScanner.ScanGames();
						}
						continue;

					case DockSelection.cSel_Default: // Possible valid platform/game selection
					default:
						break;
				}

				if(nSelectionIndex > -1)
				{
					if(m_nFirstSelection < 0)
						m_nFirstSelection = nSelectionIndex;

					else if(m_nSecondSelection < 0)
						m_nSecondSelection = nSelectionIndex;

				}

				if(m_nSecondSelection > -1)
				{
					CGameData.CGame selectedGame = CGameData.GetPlatformGame((CGameData.GamePlatform)m_nFirstSelection, m_nSecondSelection);
					if(StartGame(selectedGame))
						return;

					else
					{
						Logger.CLogger.LogInfo("Cannot start game, remove game from file.");
						CGameData.RemoveGame(selectedGame);
						CJsonWrapper.Export(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
					}
				}
			}
		}

		/// <summary>
		/// Display menu and handle the selection
		/// </summary>
		private void MenuSwitchboard(out int nSelectionCode, out int nSelectionIndex)
		{
			// Show initial options - platforms or all
			// Take the selection as a string (we'll figure out the enum later)
			// Display the options related to the initial selection (all games will show everything)
			//	Allow cancel with escape (make sure to print that in the heading)
			//  Run selected game.

			if(m_nFirstSelection < 0)
			{
				string strHeader = "Select platform.\n Press [Q] to exit;\n Press [S] to rescan game collection; \n Press [H] for help";
				string[] platformArray = CGameData.GetPlatformNames().ToArray();

				nSelectionCode = m_dockConsole.DisplayMenu(strHeader, out nSelectionIndex, CGameData.GetPlatformNames().ToArray());

				if(nSelectionIndex > -1)
					nSelectionIndex = CGameData.GetPlatformEnum(platformArray[nSelectionIndex].Substring(0, platformArray[nSelectionIndex].IndexOf(':')));
			}
			else if(m_nSecondSelection < 0)
			{
				string strHeader = "Select game.\n Press [Q] to exit;\n Press [R] to return to previous menu;\n Press [F] add/remove from favourites;";
				nSelectionCode = m_dockConsole.DisplayMenu(strHeader, out nSelectionIndex, CGameData.GetPlatformTitles((CGameData.GamePlatform)m_nFirstSelection).ToArray());
			}
			else
			{
				nSelectionCode = -1;
				nSelectionIndex = -1;
			}
		}
		
		/// <summary>
		/// Start the game process
		/// </summary>
		/// <param name="game"></param>
		private bool StartGame(CGameData.CGame game)
		{
			Console.Clear();
			Logger.CLogger.LogDebug("Starting game: {0} ...", game.Title);
			Console.WriteLine("Starting game: {0} ...", game.Title);
			try
			{
				if(game.PlatformString == "GOG")
				{
					ProcessStartInfo gogProcess = new ProcessStartInfo();
					string clientPath = game.Launch.Substring(0, game.Launch.IndexOf('.') + 4);
					string arguments = game.Launch.Substring(game.Launch.IndexOf('.') + 4);
					gogProcess.FileName = clientPath;
					gogProcess.Arguments = arguments;
					Process.Start(gogProcess);
					return true;
				}
				Process.Start(game.Launch);
				return true;
			}
			catch(Exception e)
			{
				Logger.CLogger.LogError(e, "Cannot start game. Was game removed?");
				Console.WriteLine("Cannot launch game. Make sure that the game is installed.");
				Console.WriteLine("{0} will be removed from the list.", game.Title);
				return false;
			}
		}

		/// <summary>
		/// Print help screen and wait until the user has pressed a key
		/// </summary>
		private void DisplayHelp()
		{
			Console.Clear();
			foreach(string str in m_helpLines)
			{
				Console.WriteLine(str);
			}
			Console.WriteLine("Press any key to return to previous menu");
			Console.ReadKey();
		}
	}
}
