using System.Reflection;
using System.Xml.Schema;
using Terminal.Gui;

namespace glc_2.UI.Dialog
{
    internal class CEditStringDlg : CEditDlg<string>
    {
        protected TextField m_textField;

        public CEditStringDlg(string title , string initialValue)
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

        public override bool Run(ref string currentValue)
        {
            if(Run() && m_textField.Text.ToString() != m_editValue)
            {
                currentValue = m_textField.Text.ToString();
                return true;
            }
            return false;
        }
    }
}
