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
	/// TODO: Add better controls and commands;
	/// TODO: Only scan the registry when it is required (either file is empty or user called for it)
	/// TODO: Check if the file is empty
	/// </summary>
	class CDock
	{
		private bool	m_bIsExit;
		CDockConsole	m_dockConsole;
		private int		m_nFirstSelection;
		private int		m_nSecondSelection;

		public CDock()
		{
			m_bIsExit		   = false;
			m_dockConsole	   = new CDockConsole(1, 5, CConsoleHelper.ConsoleState.cState_Navigate);
			m_nFirstSelection  = -10;
			m_nSecondSelection = -10;
		}

		public void Run()
		{
			// Check JSON file,
			// Create JSON and scan if missing

			// Load games into memory and scan the registry
			CJsonWrapper.Import();
			ScanGames();

			// Run the program loop until exit or a selection has been chosen
			while(!m_bIsExit && (m_nFirstSelection == -10 || m_nSecondSelection == -10))
			{
				MenuSwitchboard();
			}

			if(m_bIsExit)
			{
				return;
			}
			else if(m_nFirstSelection > -10 && m_nSecondSelection > -1)
			{
				CGame selectedGame = CGameData.GetGame(m_nFirstSelection, m_nSecondSelection);
				StartGame(selectedGame);
			}
		}

		/// <summary>
		/// Display menu and handle the selection
		/// </summary>
		private void MenuSwitchboard()
		{
			// Show initial options - platforms or all
			// Take the selection as a string (we'll figure out the enum later)
			// Display the options related to the initial selection (all games will show everything)
			//	Allow cancel with escape (make sure to print that in the heading)
			//  Run selected game.

			if(m_nFirstSelection == -10)
			{
				string strInitialTitle	= "Select platform | Press ESC to terminate";
				string[] platformArr = CGameData.GetPlatformNames().ToArray();
				m_nFirstSelection/*int nSelection*/ = m_dockConsole.ShowDockOptions(strInitialTitle, CGameData.GetPlatformNames().ToArray());
				int nPlatformEnumValue = CGameData.GetPlatformEnum(platformArr[m_nFirstSelection].Substring(0, platformArr[m_nFirstSelection].IndexOf(':')));
				m_nFirstSelection = nPlatformEnumValue;
				//m_strFirstSelection		= CGameData.GetPlatformString(nSelection);
			}
			else if(m_nSecondSelection == -10)
			{
				string strInitialTitle = "Select platform | Press ESC to terminate";
				m_nSecondSelection /*int nSelection*/ = m_dockConsole.ShowDockOptions(strInitialTitle, GetPlatformTitles((GamePlatform)m_nFirstSelection).ToArray());
				//m_strFirstSelection = CGameData.GetPlatformString(nSelection);
			}
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

		private void ScanGames()
		{
			List<CRegScanner.RegistryGameData> gameDataList = CRegScanner.GetGames();
			foreach(CRegScanner.RegistryGameData data in gameDataList)
			{
				CGameData.AddGame(data.m_strTitle, data.m_strLaunch, false, data.m_strPlatform);
			}
			CJsonWrapper.Export(CGameData.GetAllGames().ToList());
		}
	}
}
