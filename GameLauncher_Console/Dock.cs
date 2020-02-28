using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameLauncher_Console.CGameData;

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
			cSel_SwitchNav	= -3,
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

			int nSelection = -1;
			for(;;)
			{
				nSelection = MenuSwitchboard();

				switch(nSelection)
				{
					case -5: // Exit application
						return;

					case -4: // Go back to first menu
						m_nFirstSelection = -1;
						continue;

					case -3: // Switch console state
						m_dockConsole.SwitchState();
						continue;

					case -2: // Rescan the game list
						if(m_nFirstSelection < 0)
							CRegScanner.ScanGames();
						continue;

					case -1: // Possible valid platform/game selection
					default:
						break;
				}

				if(nSelection > -1)
				{
					if(m_nFirstSelection < 0)
						m_nFirstSelection = nSelection;
					
					else if(m_nSecondSelection < 0)
						m_nSecondSelection = nSelection;
					
				}

				if(m_nSecondSelection > -1)
				{
					CGame selectedGame = CGameData.GetGame(m_nFirstSelection, m_nSecondSelection);
					StartGame(selectedGame);
				}
			}
		}

		/// <summary>
		/// Display menu and handle the selection
		/// </summary>
		private int MenuSwitchboard()
		{
			// Show initial options - platforms or all
			// Take the selection as a string (we'll figure out the enum later)
			// Display the options related to the initial selection (all games will show everything)
			//	Allow cancel with escape (make sure to print that in the heading)
			//  Run selected game.

			int nSelection = -1;

			if(m_nFirstSelection < 0)
			{
				string strHeader		= "Select platform.\n Press [Q] to terminate;\n Press [TAB] to switch console mode;\n Press [~] to rescan game collection";
				string[] platformArray	= CGameData.GetPlatformNames().ToArray();

				nSelection		= m_dockConsole.DisplayMenu(strHeader, CGameData.GetPlatformNames().ToArray());

				if(nSelection > -1)
					nSelection		= CGameData.GetPlatformEnum(platformArray[nSelection].Substring(0, platformArray[nSelection].IndexOf(':')));
			}
			else if(m_nSecondSelection < 0)
			{
				string strHeader	= "Select game.\n Press [Q] to terminate;\n Press [W] to return to previous menu;\n Press [TAB] to switch console mode;";
				nSelection			= m_dockConsole.DisplayMenu(strHeader, GetPlatformTitles((GamePlatform)m_nFirstSelection).ToArray());
			}
			else
				return -1;

			return nSelection;
		}

		/// <summary>
		/// Start the game process
		/// </summary>
		/// <param name="game"></param>
		private void StartGame(CGame game)
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
