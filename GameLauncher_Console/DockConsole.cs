using Logger;
using System;

namespace GameLauncher_Console
{
	/// <summary>
	/// Console implementation for this project
	/// </summary>
	sealed class CDockConsole : CConsoleHelper
	{
		/// <summary>
		/// Constructor:
		/// Call base class constructor
		/// </summary>
		/// <param name="nColumns">Numer of columns</param>
		/// <param name="nSpacing">Spacing between columns</param>
		/// <param name="state">Initial console state</param>
		public CDockConsole(int nColumns, int nSpacing, ConsoleState state) : base(nColumns, nSpacing, state)
		{

		}

		/// <summary>
		/// Function overload
		/// Display either the insert or navigate menu, depending on the console state.
		/// Returns selection code and selection index
		/// </summary>
		/// <param name="strMenuTitle">Helper heading text block displayed on top of the console.</param>
		/// <param name="nSelectionIndex">Index of the selected option - reference</param>
		/// <param name="options">String array representing the available options</param>
		/// <returns>Selection code</returns>
		public int DisplayMenu(string strMenuTitle, out int nSelectionIndex, params string[] options)
		{
			Console.Clear();
			int nSelectionCode = -1;

			do
			{
				nSelectionCode = HandleNavigationMenu(strMenuTitle, out nSelectionIndex, true, options);
				CLogger.LogDebug("Current Selection = {0}", nSelectionCode);

				if(nSelectionIndex == 0 && options.Length == 0)
				{
					nSelectionIndex = -1;
					return nSelectionCode;
				}

			} while(!IsSelectionValid(nSelectionIndex, options.Length));

			return nSelectionCode;
		}

		/// <summary>
		/// Function override
		/// Validate selection
		/// </summary>
		/// <param name="nSelection">Selection as integer</param>
		/// <param name="nItemCount">Count of the possible selections</param>
		/// <returns>True if valid, otherwise false</returns>
		protected override bool IsSelectionValid(int nSelection, int nItemCount)
		{
			return (-1 < nSelection && nSelection < nItemCount);
		}
		
		/// <summary>
		/// Function overload
		/// Selection handler in the browse state.
		/// Return selection code and selection index
		/// </summary>
		/// <param name="strHeader">Helper header text block which will appear on top of the console</param>
		/// <param name="nSelectionIndex">Index of the option array - reference</param>
		/// <param name="bCanExit">Flag indicating if exiting is allowed - unused in this function</param>
		/// <param name="options">Array of available options</param>
		/// <returns>Selection code</returns>
		public int HandleNavigationMenu(string strHeader, out int nSelectionIndex, bool bCanExit, params string[] options)
		{
			// Setup
			int nCurrentSelection = 0;
			int nLastSelection = 0;

			ConsoleKey key;
			Console.CursorVisible = false;

			// Print the selections
			Console.Clear();
			Console.WriteLine(strHeader);
			int nStartY = Console.CursorTop + 1;

			if(m_MenuType == MenuType.cType_Grid)
				DrawGridMenu(nCurrentSelection, nStartY, options);

			else if(m_MenuType == MenuType.cType_List)
				DrawListMenu(nCurrentSelection, nStartY, options);

			do
			{
				// Track the current selection
				if(nCurrentSelection != nLastSelection)
					UpdateMenu(nLastSelection, nCurrentSelection, nStartY, options[nLastSelection], options[nCurrentSelection]);

				key = Console.ReadKey(true).Key;
				nLastSelection = nCurrentSelection;
				nSelectionIndex = nCurrentSelection;

				CLogger.LogDebug("{0} key registered", key);
				switch(key)
				{
					case ConsoleKey.LeftArrow:
						CLogger.LogDebug("Cursor on new selection: {0}: {1}", nCurrentSelection, options[nCurrentSelection]);
						HandleSelectionLeft(ref nCurrentSelection);
						break;

					case ConsoleKey.RightArrow:
						CLogger.LogDebug("Cursor on new selection: {0}: {1}", nCurrentSelection, options[nCurrentSelection]);
						HandleSelectionRight(ref nCurrentSelection, options.Length);
						break;

					case ConsoleKey.UpArrow:
						CLogger.LogDebug("Cursor on new selection: {0}: {1}", nCurrentSelection, options[nCurrentSelection]);
						HandleSelectionUp(ref nCurrentSelection);
						break;

					case ConsoleKey.DownArrow:
						CLogger.LogDebug("Cursor on new selection: {0}: {1}", nCurrentSelection, options[nCurrentSelection]);
						HandleSelectionDown(ref nCurrentSelection, options.Length);
						break;

					case ConsoleKey.Q:
						return -5;

					case ConsoleKey.W:
						return -4;

					case ConsoleKey.F:
						return -3;

					case ConsoleKey.Oem3:
						return -2;

					default:
						break;
				}
			} while(key != ConsoleKey.Enter);

			Console.CursorVisible = true;
			return -1;
		}
	}
}
