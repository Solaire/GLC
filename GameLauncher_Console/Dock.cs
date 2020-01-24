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
		private bool	m_IsExit = false;
		CDockConsole	m_dockConsole;
		LinkedList<int> m_selectionList;

		public CDock()
		{
			m_dockConsole	= new CDockConsole(1, 5, CConsoleHelper.ConsoleState.cState_Navigate);
			m_selectionList = new LinkedList<int>();
		}

		public void Run()
		{
			// Check JSON file,
			// Create JSON and scan if missing
			// Load games into memory

			for(int i = 0; i < 15; i++)
			{
				CGameData.AddGame("Title " + i, "", false, "Steam");
			}
			
			// Run the program loop until exit or a selection has been chosen
			while(!m_IsExit)
			{
				MenuSwitchboard();
			}
		}

		private void MenuSwitchboard()
		{
			if(m_selectionList.Count == 0)
			{
				string strInitialTitle = "Select platform: ";
				int nSelection = m_dockConsole.ShowDockOptions(strInitialTitle, CGameData.GetPlatformNames().ToArray());
				m_selectionList.AddLast(nSelection);
			}
			else
			{
				string strInitialTitle = "Select Game: ";
				int nSelection = m_dockConsole.ShowDockOptions(strInitialTitle, CGameData.GetPlatformTitles((GamePlatform)m_selectionList.First.Value).ToArray());
				m_selectionList.AddLast(nSelection);
			}
		}
	}
}
