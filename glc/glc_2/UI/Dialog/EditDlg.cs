using Terminal.Gui;

namespace glc_2.UI.Dialog
{
    /// <summary>
    /// Base class for creating edit dialogs.
    /// </summary>
    internal abstract class EditDlg<T> : Terminal.Gui.Dialog
    {
        /// <summary>
        /// The initial value when the dialog is created.
        /// </summary>
        public T InitialValue
        {
            get;
        }

        /// <summary>
        /// New value set by the dialog
        /// </summary>
        public T NewValue
        {
            get;
            protected set;
        }

        /// <summary>
        /// Check if the "okay" button has been pressed.
        /// </summary>
        public bool OkPressed
        {
            get;
            protected set;
        }

        /// <summary>
        /// Construct the dialog with "okay" and "cancel" buttons.
        /// </summary>
        /// <param name="title">The dialog window title</param>
        /// <param name="initialValue">Initial value of <see cref="T"/></param>
        /// <param name="width">Width of the dialog (default 40)</param>
        /// <param name="height">Height of the dialog (default 10)</param>
        public EditDlg(string title, T initialValue, int width = 40, int height = 10)
            : base(title, width, height)
        {
            OkPressed = false;
            InitialValue = initialValue;

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
                Application.RequestStop();
            };
            AddButton(cancelButton);
        }

        /// <summary>
        /// Start the dialog window and keep focus until it is closed with a button
        /// </summary>
        /// <returns>Flag indicating if the dialog was closed with "okay" button</returns>
        public virtual bool Run()
        {
            Application.Run(this);
            return OkPressed;
        }

        /// <summary>
        /// Check if the value of <c>NewValue</c> is valid. The base implementation
        /// will return true by default.
        /// </summary>
        /// <returns>True if the value of NewValue is valid</returns>
        protected virtual bool ValidateValue()
        {
            return true;
        }
    }
}
