using Terminal.Gui;

namespace glc_2.UI.Dialog
{
    internal abstract class CEditDlg<T> : Terminal.Gui.Dialog
    {
        protected T     m_editValue;

        public bool OkPressed
        {
            get;
            protected set;
        }

        public CEditDlg(string title, T initialValue, int width = 40, int height = 10)
            : base(title, width, height)
        {
            OkPressed = false;
            m_editValue = initialValue;

            Button okButton = new Button("Ok");
            okButton.Clicked += () =>
            {
                if(ValidateValue())
                {
                    OkPressed = true;
                    Application.RequestStop();
                }
            };
            AddButton(okButton);

            Button cancelButton = new Button("Cancel");
            cancelButton.Clicked += () =>
            {
                OkPressed = false;
                Application.RequestStop();
            };
            AddButton(cancelButton);
        }

        public bool Run()
        {
            Application.Run(this);
            return OkPressed;
        }

        public virtual bool Run(ref T currentValue)
        {
            return Run();
        }

        protected virtual bool ValidateValue()
        {
            return true;
        }
    }
}
