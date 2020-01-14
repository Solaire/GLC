using Logger;
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
			cState_Navigate	= 0,
			cState_Insert	= 1,
		};

		private ConsoleState m_consoleState = ConsoleState.cState_Unknown;
		private readonly string[] m_MainMenuOptions =
		{
			"Steam",
			"Gog",
			"Origin",
			"Uplay",
			"Epic store",
			"Bethesda.NET",
			"Battlenet"
		};

		/// <summary>
		/// Main console loop.
		/// Load and read the XML file and present the user with a list of items to pick.
		/// Process and handle console input
		/// </summary>
		public void ConsoleStart()
		{
			CLogger.LogDebug("Console starting");
			m_consoleState = ConsoleState.cState_Navigate;
			ShowMainMenu();
			Console.ReadLine();
		}

		private void ShowMainMenu()
		{
			CLogger.LogDebug("Calling main menu");
			Console.Clear();
			int nSelection = 0;

			do
			{
				CLogger.LogDebug("Main menu loop running");

				string strMainMenuText = "Game Launcher Dock (Console Edition)\nMake your selection (press ESC to close program)";

				nSelection = HandleNavigation(strMainMenuText, true, m_MainMenuOptions);
				CLogger.LogDebug("Current Selection = {0}", nSelection);

			} while(nSelection > -1);

			CLogger.LogDebug("Main menu closing");
			Console.WriteLine("Goodbye");
		}

		/// <summary>
		/// Await and handle input from the 'browse' state
		/// </summary>
		private int HandleNavigation(string strTitleText, bool bCanCancel, params string[] options)
		{
			const int nStartX = 15;
			const int nStartY = 8;
			const int nOptionsPerLine = 3;
			const int nSpacingPerLine = 14;

			int nCurrentSelection = 0;

			ConsoleKey key;

			Console.CursorVisible = false;

			do
			{
				Console.Clear();
				Console.WriteLine(strTitleText);

				for(int i = 0; i < options.Length; i++)
				{
					Console.SetCursorPosition(nStartX + (i % nOptionsPerLine) * nSpacingPerLine, nStartY + i / nOptionsPerLine);

					if(i == nCurrentSelection)
						Console.ForegroundColor = ConsoleColor.Red;

					Console.WriteLine(options[i]);
					Console.ResetColor();
				}

				key = Console.ReadKey(true).Key;

				switch(key)
				{
					case ConsoleKey.LeftArrow:
						if(nCurrentSelection > 0 && nCurrentSelection % nOptionsPerLine > 0)
						{
							nCurrentSelection--;
							CLogger.LogDebug("Key press registered: {0}", key);
							CLogger.LogDebug("Curosr on new selection: {0}: {1}", nCurrentSelection, options[nCurrentSelection]);
						}
						break;

					case ConsoleKey.RightArrow:
						if(nCurrentSelection + 1 < options.Length && nCurrentSelection % nOptionsPerLine < nOptionsPerLine - 1)
						{
							nCurrentSelection++;
							CLogger.LogDebug("Key press registered: {0}", key);
							CLogger.LogDebug("Curosr on new selection: {0}: {1}", nCurrentSelection, options[nCurrentSelection]);
						}
						break;

					case ConsoleKey.UpArrow:
						if(nCurrentSelection >= nOptionsPerLine)
						{
							nCurrentSelection -= nOptionsPerLine;
							CLogger.LogDebug("Key press registered: {0}", key);
							CLogger.LogDebug("Curosr on new selection: {0}: {1}", nCurrentSelection, options[nCurrentSelection]);
						}
						break;

					case ConsoleKey.DownArrow:
						if(nCurrentSelection + nOptionsPerLine < options.Length)
						{ 
							nCurrentSelection += nOptionsPerLine;
							CLogger.LogDebug("Key press registered: {0}", key);
							CLogger.LogDebug("Curosr on new selection: {0}: {1}", nCurrentSelection, options[nCurrentSelection]);
						}
						break;

					case ConsoleKey.Escape:
						CLogger.LogDebug("ESC key registered");
						if(bCanCancel)
							return -1;
						break;
				}

			} while(key != ConsoleKey.Enter);

			Console.CursorVisible = true;
			return nCurrentSelection;
		}

		/// <summary>
		/// Switch the console state
		/// </summary>
		private void SwitchState()
		{
			m_consoleState = (ConsoleState)((int)m_consoleState % 2);
		}
	}
}
