using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLauncher_Console
{
	class CDock
	{
		private bool m_IsExit = false;

		public void Run()
		{
			// Check JSON file,
			// Create JSON and scan if missing
			// Load games into memory

			CConsoleHelper consoleHelper = new CConsoleHelper(1, 5, CConsoleHelper.ConsoleState.cState_Navigate);
			
			// Run the program loop until exit or a selection has been chosen
			while(!m_IsExit)
			{

			}
		}

		private void 
	}
}
