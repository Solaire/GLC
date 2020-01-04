using GameLauncherDock.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLauncher_Console
{
	/// <summary>
	/// Application console-based frontend
	/// </summary>
	public class CConsole
	{
		/// <summary>
		/// Console state enum.
		/// The console supports two states:
		/// * Type	: User can type out commands
		/// * Browse: User can use the WASD keys to browse the items and enter/space to select item (early stage gui)
		/// </summary>
		private enum ConsoleState
		{
			cState_Unknown	= -1,
			cState_Browse	= 0,
			cState_Type		= 1,
		};

		private ConsoleState m_consoleState = ConsoleState.cState_Unknown;
		private CGameManager m_gameManager;

		/// <summary>
		/// Main console loop.
		/// Load and read the XML file and present the user with a list of items to pick.
		/// Process and handle console input
		/// </summary>
		public void ConsoleStart()
		{
			m_consoleState = ConsoleState.cState_Type;


		}

		/// <summary>
		/// Await and handle input from the 'type' state
		/// </summary>
		private void HandleTypeInput()
		{

		}

		/// <summary>
		/// Await and handle input from the 'browse' state
		/// </summary>
		private void HandleBrowseInput()
		{

		}

		/// <summary>
		/// Switch the console state
		/// </summary>
		private void SwitchState()
		{
			if(m_consoleState == ConsoleState.cState_Type)
			{
				m_consoleState = ConsoleState.cState_Browse;
			}
			else
			{
				m_consoleState = ConsoleState.cState_Type;
			}
		}
	}
}
