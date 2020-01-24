using Logger;
using System;

namespace GameLauncher_Console
{
	class CDockConsole : CConsoleHelper
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

		public int ShowDockOptions(string strMenuTitle, params string[] options)
		{
			Console.Clear();
			int nSelection = 0;

			do
			{

				if(m_consoleState == ConsoleState.cState_Navigate)
					nSelection = HandleNavigationMenu(strMenuTitle, true, options);
				else if(m_consoleState == ConsoleState.cState_Insert)
					nSelection = HandleInsertMenu(strMenuTitle, options);

				CLogger.LogDebug("Current Selection = {0}", nSelection);

			} while(!IsSelectionValid(nSelection, options.Length));

			return nSelection;
		}

		/// <summary>
		/// Check if selected item is within range
		/// </summary>
		/// <param name="nSelection">Index of the selected item</param>
		/// <param name="nItemCount">Number of items</param>
		/// <returns>True if within range, otherwise false</returns>
		private bool IsSelectionValid(int nSelection, int nItemCount)
		{
			return (-1 < nSelection && nSelection < nItemCount);
		}
	}
}
