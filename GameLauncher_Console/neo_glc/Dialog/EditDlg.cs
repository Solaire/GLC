using NStack;
using System;
using System.Collections.Generic;
using Terminal.Gui;
using static core.CSystemAttributeSQL;

namespace glc
{
	/// <summary>
	/// Base class for creating edit dialogs
	/// Only controls are the ok and cancel buttons
	/// </summary>
	public abstract class CEditDlg : Dialog
	{
		protected bool m_isOkay;

		/// <summary>
		/// Base constructor.
		/// </summary>
		/// <param name="title">The dialog title</param>
		public CEditDlg(string title)
            : base(title, 40, 10)
        {
			m_isOkay = false;

			Button okButton = new Button("Ok");
			okButton.Clicked += () =>
			{
				if(ValidateValue())
				{
					m_isOkay = true;
					Application.RequestStop();
				}
			};
			AddButton(okButton);

			Button cancelButton = new Button("Cancel");
			cancelButton.Clicked += () =>
			{
				m_isOkay = false;
				Application.RequestStop();
			};
			AddButton(cancelButton);
		}

		/// <summary>
		/// Run the dialog until it's closed.
		/// </summary>
		/// <param name="node">Refernece to a system attribute node</param>
		/// <returns>True if 'ok' button was pressed; otherwise false</returns>
		public virtual bool Run(ref SystemAttributeNode node)
        {
			Application.Run(this);
			return m_isOkay;
		}

		/// <summary>
		/// Check the new value of the setting to ensure it is allowed
		/// </summary>
		/// <returns>Always true</returns>
		protected virtual bool ValidateValue()
        {
			return true;
		}
    }

	/// <summary>
	/// Implementation of the CEditDlg for editing boolean values
	/// </summary>
	public class CEditBoolDlg : CEditDlg
	{
		private bool m_isTrue;
		private CBoolRadio m_radio;

		/// <summary>
		/// Constructor.
		/// Create a radio control with true/false values
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="isInitialTrue">Initial boolean value</param>
		public CEditBoolDlg(string title, bool isInitialTrue)
			: base(title)
		{
			m_isTrue = isInitialTrue;
			m_radio = new CBoolRadio(2, 2, m_isTrue);
			Add(m_radio);
		}

		/// <summary>
		/// Function override.
		/// If ok button was pressed, modify the node value with the new value
		/// </summary>
		/// <param name="node">Refernece to a system attribute node</param>
		/// <returns>True if ok button was pressed</returns>
		public override bool Run(ref SystemAttributeNode node)
        {
			if(base.Run(ref node))
            {
				node.SetTrue(m_radio.SelectedItem == 0);
				return true;
            }
			return false;
		}

		/// <summary>
		/// Child of RadioGroup, with pre-set configuration and extended key handler
		/// </summary>
		private class CBoolRadio : RadioGroup
        {
			/// <summary>
			/// Constructor.
			/// Create RadioGroup control with true/false values and set to horizontal view
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <param name="isInitialTrue">Initial boolean value to select</param>
			public CBoolRadio(int x, int y, bool isInitialTrue)
				: base(x, y, new ustring[] { "True", "False" }, (isInitialTrue) ? 0 : 1)
            {
				DisplayMode = DisplayModeLayout.Horizontal;
				HorizontalSpace = 5;
            }

			/// <summary>
			/// Fundtion override.
			/// Handle left, right and enter buttons
			/// </summary>
			/// <param name="kb">The key event</param>
			/// <returns>Result of base.ProcessKey()</returns>
            public override bool ProcessKey(KeyEvent kb)
            {
				// The base class members are set to private, so we can't actually directly modify them
				// We have to resort to a bit of a hack and treat the new keys as the keys already handled
				// Luckily for our purposes this is all we need
				switch(kb.Key)
				{
					case Key.CursorLeft:	return base.ProcessKey(new KeyEvent(Key.CursorUp,	new KeyModifiers()));
					case Key.CursorRight:	return base.ProcessKey(new KeyEvent(Key.CursorDown, new KeyModifiers()));
					case Key.Enter:			return base.ProcessKey(new KeyEvent(Key.Space,		new KeyModifiers()));
				}
				return base.ProcessKey(kb);
			}
        }
	}

	/// <summary>
	/// Implementation of the CEditDlg for editing string values
	/// </summary>
	public class CEditStringDlg : CEditDlg
	{
		protected TextField m_textEdit;

		/// <summary>
		/// Constructor.
		/// Create the text edit box
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="initialText">Initial text value</param>
		public CEditStringDlg(string title, string initialText)
			: base(title)
		{
			m_textEdit = new TextField()
			{
				X = 4,
				Y = 4,
				Width = Dim.Fill(),
				Text = initialText,
				CursorPosition = initialText.Length,
			};
			Add(m_textEdit);
			m_textEdit.SetFocus();
		}

		/// <summary>
		/// Function override.
		/// If ok button was pressed, modify the node value with the new value
		/// </summary>
		/// <param name="node">Refernece to a system attribute node</param>
		/// <returns>True if ok button was pressed</returns>
		public override bool Run(ref SystemAttributeNode node)
        {
			if(base.Run(ref node))
			{
				node.AttributeValue = m_textEdit.Text.ToString();
				return true;
			}
			return false;
		}
    }

	/// <summary>
	/// Implementation of the CEditStringDlg for editing integer values.
	/// Functionally the same as CEditStringDlg, but will validate the text to ensure it is numeric
	/// </summary>
	public class CEditIntDlg : CEditStringDlg
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="initialValue">Initial value as string</param>
		public CEditIntDlg(string title, string initialValue)
			: base(title, initialValue)
		{
			Int32.TryParse(initialValue, out int initIntValue);
			m_textEdit.Text = initIntValue.ToString();
			m_textEdit.CursorPosition = m_textEdit.Text.Length;
		}

		/// <summary>
		/// Function override.
		/// If ok button was pressed, modify the node value with the new value
		/// </summary>
		/// <param name="node">Refernece to a system attribute node</param>
		/// <returns>True if ok button was pressed</returns>
		public override bool Run(ref SystemAttributeNode node)
		{
			if(base.Run(ref node))
			{
				node.AttributeValue = m_textEdit.Text.ToString();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Function override.
		/// Check if value is a valid integer by converting it to Int32
		/// If conversion fails, show a MessageBox with an error message.
		/// </summary>
		/// <returns>True if value can be converted; otherwise false</returns>
        protected override bool ValidateValue()
        {
            if(!Int32.TryParse(m_textEdit.Text.ToString(), out _))
            {
				MessageBox.ErrorQuery(30, 5, "Invalid value", "Value must be a valid integer", new ustring[] { "Okay" });
				return false;
			}
			return true;
		}
	}
}
