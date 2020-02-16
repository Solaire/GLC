using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameLauncher_Console.CGameData;

namespace GameLauncher_Console
{
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

			// Load games into memory
			CJsonWrapper.Import();
			CGameData.AddGame("custom game 1", "launch/1.exe", false, "invalid");
			CGameData.AddGame("custom game 2", "launch/2.exe", true,  "custom");
			CJsonWrapper.Export(CGameData.GetAllGames());

			// Run the program loop until exit or a selection has been chosen
			while(!m_bIsExit && (m_nFirstSelection == -10 || m_nSecondSelection == -10))
			{
				MenuSwitchboard();
			}

			if(m_bIsExit)
			{
				// TODO:
			}
			else if(m_nFirstSelection > -10 && m_nSecondSelection > -1)
			{
				// TODO:
				// CGameData.Launch(m_nFirstSelection, m_nSecondSelection);
			}
		}

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
	}
}
