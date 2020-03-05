using System;
using System.Diagnostics;
using System.Linq;

namespace GameLauncher_Console
{
	/// <summary>
	/// Main program logic - this is where all classes meet and get used
	/// TODO: Test console state switching and add comments - including help
	/// </summary>
	class CDock
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
			cSel_Exit		= -5,
			cSel_Back		= -4,
			cSel_Fav		= -3,
			cSel_Rescan		= -2,
			cSel_Default	= -1,
		}

		public CDock()
		{
			m_dockConsole	   = new CDockConsole(1, 5, CConsoleHelper.ConsoleState.cState_Navigate);
			m_nFirstSelection  = -1;
			m_nSecondSelection = -1;
		}

		public void Run()
		{
			// Check JSON file,
			// Create JSON and scan if missing

			// Load games into memory and scan the registry
			CJsonWrapper.Import();

			int nSelectionCode, nSelectionIndex;
			for(; ; )
			{
				MenuSwitchboard(out nSelectionCode, out nSelectionIndex);

				switch((DockSelection)nSelectionCode)
				{
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
							CGameData.ClearGames(false);
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
					StartGame(selectedGame);
					return;
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
				string strHeader = "Select platform.\n Press [Q] to terminate;\n Press [~] to rescan game collection";
				string[] platformArray = CGameData.GetPlatformNames().ToArray();

				nSelectionCode = m_dockConsole.DisplayMenu(strHeader, out nSelectionIndex, CGameData.GetPlatformNames().ToArray());

				if(nSelectionIndex > -1)
					nSelectionIndex = CGameData.GetPlatformEnum(platformArray[nSelectionIndex].Substring(0, platformArray[nSelectionIndex].IndexOf(':')));
			}
			else if(m_nSecondSelection < 0)
			{
				string strHeader = "Select game.\n Press [Q] to terminate;\n Press [W] to return to previous menu;\n Press [F] add/remove from favourites;";
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
		private void StartGame(CGameData.CGame game)
		{
			if(game.PlatformString == "GOG")
			{
				ProcessStartInfo gogProcess = new ProcessStartInfo();
				string clientPath = game.Launch.Substring(0, game.Launch.IndexOf('.') + 4);
				string arguments = game.Launch.Substring(game.Launch.IndexOf('.') + 4);
				gogProcess.FileName = clientPath;
				gogProcess.Arguments = arguments;
				Process.Start(gogProcess);
				return;
			}
			Process.Start(game.Launch);
		}
	}
}
