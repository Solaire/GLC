using Logger;
using System;
using System.Collections.Generic;

// Useful stack overflow question:
// https://stackoverflow.com/questions/888533/how-can-i-update-the-current-line-in-a-c-sharp-windows-console-app

namespace GameLauncher_Console
{
	/// <summary>
	/// Console helper class which can handle typing and navigation. 
	/// In the most basic form, the class will print a list of options and handle user selection.
	/// </summary>
	public class CConsoleHelper
	{
		/// <summary>
		/// Console state enum.
		/// The console supports two states:
		/// * Type	: User can type out commands
		/// * Browse: User can use the WASD keys to browse the items and enter/space to select item (early stage gui)
		/// </summary>
		public enum ConsoleState
		{
			cState_Unknown	= -1,
			cState_Navigate	= 0,
			cState_Insert	= 1,
		};

		/// <summary>
		/// Menu type enum.
		/// Menu elements can be displayed as either:
		/// * Grid: Print the elements across X columns (column number is controlled by m_OptionsPerLine)
		/// * List: Print the elements in a signle list (m_nOptionsPerLine should be set to 1 to support arrow selection)
		/// </summary>
		public enum MenuType
		{
			cType_Unknown	= -1,
			cType_Grid		= 0,
			cType_List		= 1,
		}

		protected ConsoleState m_ConsoleState	= ConsoleState.cState_Unknown;
		protected MenuType	   m_MenuType		= MenuType.cType_Unknown;

		protected int m_nOptionsPerLine; // Control the number of columns
		protected int m_nSpacingPerLine; // Control the space between the columns
		
		/// <summary>
		/// Default constructor:
		/// Set the console state to insert mode
		/// Set the menu type to list (1 column)
		/// Set the line spacing to 10
		/// </summary>
		public CConsoleHelper()
		{
			m_ConsoleState		= ConsoleState.cState_Insert;
			m_MenuType			= MenuType.cType_Grid;
			m_nOptionsPerLine	= 1;
			m_nSpacingPerLine	= 10;
		}

		/// <summary>
		/// Constructor overload:
		/// Set the member variables to argument parameters
		/// </summary>
		/// <param name="nColumnCount">Number or columns (set to 1 if argument is 0 or less)</param>
		/// <param name="nSpacing">Spacing between lines (set to 5 if argument is less than 5) </param>
		/// <param name="state">Console state (set to insert mode if argument is -1)</param>
		public CConsoleHelper(int nColumnCount, int nSpacing, ConsoleState state)
		{
			m_nSpacingPerLine	= Math.Max(5, nSpacing);
			m_nOptionsPerLine	= Math.Max(1, nColumnCount);
			m_ConsoleState		= (state == ConsoleState.cState_Unknown) ? ConsoleState.cState_Insert : state;
			m_MenuType			= (nColumnCount > 1) ? MenuType.cType_Grid : MenuType.cType_List;
		}

		/// <summary>
		/// Switch the console state
		/// </summary>
		public void SwitchState()
		{
			m_ConsoleState = (ConsoleState)((int)m_ConsoleState % 2);
		}

		/// <summary>
		/// Display either the insert or navigate menu, depending on the console state
		/// </summary>
		/// <param name="strMenuTitle">Helper heading text block displayed on top of the console.</param>
		/// <param name="options">String array representing the available options</param>
		/// <returns>Index of the selection array, or any other valid integer</returns>
		public int DisplayMenu(string strMenuTitle, params string[] options)
		{
			Console.Clear();
			int nSelection = 0;

			do
			{
				if(m_ConsoleState == ConsoleState.cState_Navigate)
				{
					nSelection = HandleNavigationMenu(strMenuTitle, true, options);
				}
				else if(m_ConsoleState == ConsoleState.cState_Insert)
				{
					nSelection = HandleInsertMenu(strMenuTitle, options);
				}

				CLogger.LogDebug("Current Selection = {0}", nSelection);

			} while(!IsSelectionValid(nSelection, options.Length));

			return nSelection;
		}

		/// <summary>
		/// Validate selection
		/// </summary>
		/// <param name="nSelection">Selection as integer</param>
		/// <param name="nItemCount">Count of the possible selections</param>
		/// <returns>True if valid, otherwise false</returns>
		protected virtual bool IsSelectionValid(int nSelection, int nItemCount)
		{
			return (-1 <= nSelection && nSelection < nItemCount);
		}

		/// <summary>
		/// Selection handler in the 'browse' state
		/// </summary>
		/// <param name="strHeader">Helper header text block which will appear on top of the console</param>
		/// <param name="bCanExit">Boolean which controls if the menu can be escaped with ESC</param>
		/// <param name="options">Array of strings representing the possible selections</param>
		/// <returns>Index of the selected item from the options parameter or -1 (Exit)</returns>
		protected virtual int HandleNavigationMenu(string strHeader, bool bCanExit, params string[] options)
		{
			// Setup
			int nCurrentSelection   = 0;
			int nLastSelection		= 0;

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

					case ConsoleKey.Escape:
						if(bCanExit)
							return -1;
						break;

					default:
						break;
				}
			} while(key != ConsoleKey.Enter);

			Console.CursorVisible = true;
			return nCurrentSelection;
		}

		/// <summary>
		/// Selection handler in the 'insert' mode
		/// </summary>
		/// <param name="strTitleText">Text which will appear at the top of the menu.</param>
		/// <param name="options">List of menu items</param>
		/// <returns>Index of the selected item from the options parameter</returns>
		protected virtual int HandleInsertMenu(string strTitleText, params string[] options)
		{
			// Setup
			int nCurrentSelection = 0;
			Console.CursorVisible = true;
			bool bIsValidSelection = false;

			do
			{
				// Refresh console before redraw, print the title and set the menu start position
				Console.Clear();
				Console.WriteLine(strTitleText);
				int nStartY = Console.CursorTop + 1;

				if(m_MenuType == MenuType.cType_Grid)
					DrawGridMenu(nCurrentSelection, nStartY, options);

				else if(m_MenuType == MenuType.cType_List)
					DrawListMenu(nCurrentSelection, nStartY, options);

				// Set the cursor to the bottom of the console
				Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - 2);
				Console.Write(">>> ");
				string strInput = Console.ReadLine();

				if(strInput.Length < 1) // Empty strings are invalid
					continue;
				
				for(int i = 0; i < options.Length; i++) // Loop over the menu items and see if anything is a match
				{
					if(strInput.ToLower().Contains(options[i].ToLower()))
					{
						bIsValidSelection = true;
						nCurrentSelection = i;
						break;
					}
				}

				// No match - check if we entered the exit command
				if(!bIsValidSelection && strInput.ToLower() == "wq") //TODO: Change to a list of supported commands (configurable)
				{
					bIsValidSelection = true;
					nCurrentSelection = -1;
				}

			} while(!bIsValidSelection);

			return nCurrentSelection;
		}

		/// <summary>
		/// Draw the list options in a grid layout.
		/// </summary>
		/// <param name="nCursorPosition">Current position, which will be printed in a red colour</param>
		/// <param name="nStartTop">Offset from the top of the window</param>
		/// <param name="itemList">List of items to be displayed</param>
		protected void DrawGridMenu(int nCursorPosition, int nStartTop, params string[] itemList)
		{
			for(int i = 0; i < itemList.Length; i++)
			{
				Console.SetCursorPosition((i % m_nOptionsPerLine) * m_nSpacingPerLine, nStartTop + i / m_nOptionsPerLine);
				CLogger.LogInfo("Item {0}, Cursor position = {1}x{2}", i, Console.CursorTop, Console.CursorLeft);
				if(i == nCursorPosition)
					Console.ForegroundColor = ConsoleColor.Red;

				Console.WriteLine(itemList[i]);
				Console.ResetColor();
			}
		}

		/// <summary>
		/// Draw a list of options as a list
		/// </summary>
		/// <param name="nCursorPosition">Current position, which will be printed in a red colour</param>
		/// <param name="nStartTop">Offset from the top of the window</param>
		/// <param name="itemList">List of items to be displayed</param>
		protected void DrawListMenu(int nCursorPosition, int nStartTop, params string[] itemList)
		{
			for(int i = 0; i < itemList.Length; i++)
			{
				Console.SetCursorPosition(1, nStartTop + i);

				if(i == nCursorPosition)
					Console.ForegroundColor = ConsoleColor.Red;

				Console.WriteLine(itemList[i]);
				Console.ResetColor();
			}
		}

		/// <summary>
		/// Handle selection calculation when 'up' is selected
		/// </summary>
		/// <param name="nCurrentSelection">Reference to the current selection</param>
		protected virtual void HandleSelectionUp(ref int nCurrentSelection)
		{
			if(m_MenuType == MenuType.cType_Grid && nCurrentSelection >= m_nOptionsPerLine)
				nCurrentSelection -= m_nOptionsPerLine;

			else if(m_MenuType == MenuType.cType_List && nCurrentSelection >= 1)
				nCurrentSelection--;
		}

		/// <summary>
		/// Handle selection calculation when 'down' is selected
		/// </summary>
		/// <param name="nCurrentSelection">Reference to the current selection</param>
		/// <param name="nOptionCount">Number of items in the list</param>
		protected virtual void HandleSelectionDown(ref int nCurrentSelection, int nOptionCount)
		{
			if(m_MenuType == MenuType.cType_Grid && nCurrentSelection + m_nOptionsPerLine < nOptionCount)
				nCurrentSelection += m_nOptionsPerLine;

			else if(m_MenuType == MenuType.cType_List && nCurrentSelection + 1 < nOptionCount)
				nCurrentSelection++;
		}

		/// <summary>
		/// Handle selection calculation when 'left' is selected
		/// </summary>
		/// <param name="nCurrentSelection">Reference to the current selection</param>
		protected virtual void HandleSelectionLeft(ref int nCurrentSelection)
		{
			if(m_MenuType == MenuType.cType_Grid && nCurrentSelection > 0 && nCurrentSelection % m_nOptionsPerLine > 0)
				nCurrentSelection--;

			else if(m_MenuType == MenuType.cType_List && nCurrentSelection > 0)
				nCurrentSelection--;
		}

		/// <summary>
		/// Handle selection calculation when 'right' is selected
		/// </summary>
		/// <param name="nCurrentSelection">Reference to the current selection</param>
		/// <param name="nOptionCount">Numbers of items in the list</param>
		protected virtual void HandleSelectionRight(ref int nCurrentSelection, int nOptionCount)
		{
			if(m_MenuType == MenuType.cType_Grid && nCurrentSelection + 1 < nOptionCount && nCurrentSelection % m_nOptionsPerLine < m_nOptionsPerLine - 1)
				nCurrentSelection++;

			else if(m_MenuType == MenuType.cType_List && nCurrentSelection + 1 < nOptionCount && nCurrentSelection < 0)
				nCurrentSelection++;
		}
		
		/// <summary>
		/// Update the printed list of menu items by re-colouring only the changed items on the list
		/// </summary>
		/// <param name="nPreviousSelection">Index of the previously highlighted item</param>
		/// <param name="nCurrentSelection">Inted of the currently selected item</param>
		/// <param name="nStartY">Starting Y position (places from top)</param>
		/// <param name="strPreviousOption">String value of the previously selected option</param>
		/// <param name="strCurrentOption">String value of the currently selected option</param>
		protected virtual void UpdateMenu(int nPreviousSelection, int nCurrentSelection, int nStartY, string strPreviousOption, string strCurrentOption)
		{
			if(m_MenuType == MenuType.cType_List)
				Console.SetCursorPosition(1, nStartY + nCurrentSelection);

			else if(m_MenuType == MenuType.cType_Grid)
				Console.SetCursorPosition((nCurrentSelection % m_nOptionsPerLine) * m_nSpacingPerLine, nStartY + nCurrentSelection / m_nOptionsPerLine);

			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("{0}", strCurrentOption);

			if(m_MenuType == MenuType.cType_List)
				Console.SetCursorPosition(1, nStartY + nPreviousSelection);

			else if(m_MenuType == MenuType.cType_Grid)
				Console.SetCursorPosition((nPreviousSelection % m_nOptionsPerLine) * m_nSpacingPerLine, nStartY + nPreviousSelection / m_nOptionsPerLine);

			Console.ResetColor();
			Console.Write("{0}", strPreviousOption);
		}
	}
}