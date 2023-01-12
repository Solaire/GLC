using System;
using System.Reflection;
using System.Xml.Schema;
using Terminal.Gui;

namespace glc_2.UI.Dialog
{
    /// <summary>
    /// Implementation of <see cref="EditDlg{String}"/> for editing a single line of text.
    /// </summary>
    internal class EditStringDlg : EditDlg<string>
    {
        protected TextField m_textField;

        /// <summary>
        /// Construct the dialog and a single-line <see cref="TextField"/>
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="initialValue">Initial string value</param>
        public EditStringDlg(string title , string initialValue)
            : base(title, initialValue)
        {
            m_textField = new TextField()
            {
                X = 4,
                Y = 4,
                Width = Dim.Fill(),
                Text = initialValue,
                CursorPosition = initialValue.Length
            };
            Add(m_textField);
            m_textField.SetFocus();
        }

        /// <summary>
        /// Run the dialog and set the new string value if closed with the "okay" button.
        /// </summary>
        /// <returns>True if the new string value is different</returns>
        public override bool Run()
        {
            if(base.Run() && m_textField.Text.ToString() != InitialValue)
            {
                NewValue = m_textField.Text.ToString();
                return true;
            }
            return false;
        }
    }
}
