using System;
using System.Collections;
using System.Collections.Generic;

using core;
using core.Game;
using NStack;
using Terminal.Gui;

namespace glc.UI
{
	/// <summary>
	/// Base class for creating edit dialogs, inheriting from <see cref="Dialog"/> class.
	/// By default only contains the [ok] and [cancel] buttons
	/// </summary>
	public abstract class CEditDlg<T> : Dialog
	{
		protected bool	m_isOkayPressed;
		protected T     m_editValue;

		/// <summary>
		/// Constructor.
		/// Initialise the base dialog with [ok] and [cancel] buttons
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="height">The dialog width. Defaults to 40</param>
		/// <param name="width">The dialog height. Defaults to 10</param>
		public CEditDlg(string title, T editValue, int width = 40, int height = 10)
            : base(title, width, height)
        {
			m_isOkayPressed = false;
			m_editValue		= editValue;

			Button okButton = new Button("Ok");
			okButton.Clicked += () =>
			{
				if(ValidateValue())
				{
					m_isOkayPressed = true;
					Application.RequestStop();
				}
			};
			AddButton(okButton);

			Button cancelButton = new Button("Cancel");
			cancelButton.Clicked += () =>
			{
				m_isOkayPressed = false;
				Application.RequestStop();
			};
			AddButton(cancelButton);
		}

		/// <summary>
		/// Run the dialog until it is closed
		/// </summary>
		/// <returns><see langword="true"/> if [okay] is pressed; otherwise <see langword="false"/></returns>
		protected bool Run()
        {
			Application.Run(this);
			return m_isOkayPressed;
		}

		/// <summary>
		/// Version of <see cref="Run()"/> which accepts a generic field for modification.
		/// </summary>
		/// <param name="currentValue">Variable that will be used to seed the dialog.</param>
		/// <returns><see langword="true"/> if [okay] is pressed; otherwise <see langword="false"/></returns>
		public virtual bool Run(ref T currentValue)
		{
			return Run();
		}

		/// <summary>
		/// Validate values of internal members to make sure the dialog can be saved.
		/// Should be overwritten for things like numeric type validation, etc.
		/// </summary>
		/// <returns><see langword="true"/></returns>
		protected virtual bool ValidateValue()
        {
			return true;
		}

		public bool IsOkayPressed()
        {
			return m_isOkayPressed;
        }
    }

	/// <summary>
	/// Implementation of <see cref="CEditDlg"/> for handling <see langword="bool"/> type.
	/// </summary>
	public class CEditBoolDlg : CEditDlg<bool>
	{
		private CBinaryRadio m_radio;

		/// <summary>
		/// Constructor.
		/// Create a radio control with true/false values
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="initialValue">Initial boolean value</param>
		public CEditBoolDlg(string title, bool initialValue)
			: base(title, initialValue)
		{
			m_radio = new CBinaryRadio(2, 2, m_editValue);
			Add(m_radio);
		}

		/// <summary>
		/// Run dialog until
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="currentValue"></param>
		/// <returns></returns>
        public override bool Run(ref bool currentValue)
        {
			if(Run() && m_radio.BoolSelection != m_editValue)
            {
				currentValue = m_radio.BoolSelection;
				return true;
            }
			return false;
        }
	}

	/// <summary>
	/// Implementation of the CEditDlg for editing string values
	/// </summary>
	public class CEditStringDlg : CEditDlg<string>
	{
		protected TextField m_textEdit;

		/// <summary>
		/// Constructor.
		/// Create the text edit box
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="initialText">Initial text value</param>
		public CEditStringDlg(string title, string initialString)
			: base(title, initialString)
		{
			m_textEdit = new TextField()
			{
				X = 4,
				Y = 4,
				Width = Dim.Fill(),
				Text = initialString,
				CursorPosition = initialString.Length,
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
		public override bool Run(ref string currentValue)
        {
			if(Run() && m_textEdit.Text.ToString() != m_editValue)
            {
				currentValue = m_textEdit.Text.ToString();
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
		public override bool Run(ref string currentValue)
		{
			return base.Run(ref currentValue);
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

	/// <summary>
	/// Child of RadioGroup, with pre-set configuration and extended key handler
	/// </summary>
	public class CBinaryRadio : RadioGroup
	{
		public bool BoolSelection
		{
			get { return SelectedItem == 0; }
		}

		/// <summary>
		/// Constructor.
		/// Create RadioGroup control with true/false values and set to horizontal view
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="isInitialTrue">Initial boolean value to select</param>
		public CBinaryRadio(int x, int y, bool isInitialTrue)
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
				case Key.CursorLeft: return base.ProcessKey(new KeyEvent(Key.CursorUp, new KeyModifiers()));
				case Key.CursorRight: return base.ProcessKey(new KeyEvent(Key.CursorDown, new KeyModifiers()));
				case Key.Enter: return base.ProcessKey(new KeyEvent(Key.Space, new KeyModifiers()));
			}
			return base.ProcessKey(kb);
		}
	}

	public class CDialogSelectionPanel : CFramePanel<IDataNode, ListView>
    {
		private List<IDataNode> m_originalNodes;

		public CDialogSelectionPanel(string title, int x, int y, List<IDataNode> nodes, bool isVisible)
			: base(title, x, y, Dim.Fill(), Dim.Fill(), true)
        {
			m_contentList = nodes;
			m_originalNodes = new List<IDataNode>(nodes);

			Initialise(title, x, y, Dim.Fill(), Dim.Fill(3), true);

			m_frameView.Enabled = isVisible;
			m_frameView.Visible = isVisible;
		}

		public override void CreateContainerView()
		{
			m_containerView = new ListView(new CDialogSelectionDataSource(m_contentList))
			{
				X = 0,
				Y = 0,
				Width = Dim.Fill(0),
				Height = Dim.Fill(0),
				AllowsMarking = true,
				AllowsMultipleSelection = true,
				CanFocus = true
			};

			m_frameView.Add(m_containerView);
		}

		public bool IsSelectionDirty()
        {
			return m_originalNodes != m_contentList;
        }

		internal class CDialogSelectionDataSource : CGenericDataSource<IDataNode>
		{
			private BitArray marks;

			public CDialogSelectionDataSource(List<IDataNode> itemList)
				: base(itemList)
			{
				marks = new BitArray(Count);

				for(int i = 0; i < itemList.Count; i++)
				{
					marks[i] = (itemList[i].IsEnabled);
				}
			}

			protected override string ConstructString(int itemIndex)
			{
				return ItemList[itemIndex].Name;
			}

			protected override string GetString(int itemIndex)
			{
				return ItemList[itemIndex].Name;
			}

			public override bool IsMarked(int item)
			{
				if(item >= 0 && item < Count)
				{
					return marks[item];
				}
				return false;
			}

			public override void SetMark(int item, bool value)
			{
				if(item >= 0 && item < Count)
				{
					marks[item] = value;

					IDataNode copy = ItemList[item];
					copy.IsEnabled = value;
					ItemList[item] = copy;
				}
			}
		}
	}

	public class CDataNodeDataSource<T> : CGenericDataSource<T> where T : IDataNode
    {
		protected long m_maxNameLength;
		protected long m_maxDescLength;

		public CDataNodeDataSource(List<T> itemList)
			: base(itemList)
		{
			for(int i = 0; i < itemList.Count; i++)
			{
				if(itemList[i].Name.Length > m_maxNameLength)
				{
					m_maxNameLength = itemList[i].Name.Length;
				}
				if(itemList[i].Description.Length > m_maxDescLength)
				{
					m_maxDescLength = itemList[i].Description.Length;
				}
			}
		}

		protected override string ConstructString(int itemIndex)
		{
			String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxNameLength), ItemList[itemIndex].Name);
			String s2 = String.Format(String.Format("{{0,{0}}}", -m_maxDescLength), ItemList[itemIndex].Description);
			string enabled = (ItemList[itemIndex].IsEnabled) ? "Enabled" : "Disabled";

			return $"{s1}  {s2}  {enabled}";
		}

		protected override string GetString(int itemIndex)
		{
			return ItemList[itemIndex].Name;
		}
	}

	public class CEditSelectionDlg<T> : CEditDlg<T> where T : IDataNode
    {
		protected CBinaryRadio m_radio;
		protected CDialogSelectionPanel m_selectionPanel;

		/// <summary>
		/// Constructor.
		/// Create a radio control with true/false values
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="isInitialTrue">Initial boolean value</param>
		public CEditSelectionDlg(T node, List<IDataNode> selectionDataList)
			: base(node.Name, node, 40, 25)
		{
			Label isActiveLabel = new Label()
			{
				X = 3,
				Y = 1,
				Width = Dim.Fill(),
				Text = "Enabled: ",
			};
			Add(isActiveLabel);

			m_radio = new CBinaryRadio(isActiveLabel.Text.Length + 5, 1, node.IsEnabled);
			m_radio.SelectedItemChanged += M_radio_SelectedItemChanged;
			Add(m_radio);

			//m_tagPanel = new CDialogSelectionPanel(platform.ID, platform.IsActive);
			m_selectionPanel = new CDialogSelectionPanel("Tags", 0, 4, selectionDataList, node.IsEnabled);
			Add(m_selectionPanel.FrameView);
		}

		private void M_radio_SelectedItemChanged(RadioGroup.SelectedItemChangedArgs obj)
		{
			bool enabled = (obj.SelectedItem == 0);
			m_selectionPanel.FrameView.Enabled = enabled;
			m_selectionPanel.FrameView.Visible = enabled;

			SetNeedsDisplay();
		}

		/// <summary>
		/// Function override.
		/// If ok button was pressed, modify the node value with the new value
		/// </summary>
		/// <param name="node">Refernece to a system attribute node</param>
		/// <returns>True if ok button was pressed</returns>
		public override bool Run(ref T currentValue)
		{
			if(base.Run(ref currentValue) && m_radio.BoolSelection != m_editValue.IsEnabled)
			{
				currentValue.IsEnabled = m_radio.BoolSelection;
				return true;
			}
			return false;
		}

		public bool IsSelectionDirty()
		{
			return m_selectionPanel.IsSelectionDirty();
		}
	}

	/// <summary>
	/// Implementation of the CEditDlg for editing game rating
	/// </summary>
	public class CEditRatingDlg : CEditDlg<int>
	{
		protected TextField m_textEdit;
		private const char m_ratingSymbol = '*';

		/// <summary>
		/// Constructor.
		/// Create the text edit box
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="initialText">Initial text value</param>
		public CEditRatingDlg(string title, int initialRating)
			: base(title, initialRating)
		{
			m_textEdit = new TextField()
			{
				X = Pos.Center(),
				Y = 3,
				Width = 5,
				Text = new string(m_ratingSymbol, initialRating),
				CanFocus = false,
			};
			Add(m_textEdit);
		}

		/// <summary>
		/// Function override.
		/// If ok button was pressed, modify the node value with the new value
		/// </summary>
		/// <param name="node">Refernece to a system attribute node</param>
		/// <returns>True if ok button was pressed</returns>
		public override bool Run(ref int currentRating)
		{
			if(Run() && m_textEdit.Text.Length != m_editValue)
			{
				currentRating = m_textEdit.Text.Length;
				return true;
			}
			return false;
		}


		/// <summary>
		/// Fundtion override.
		/// Handle left, right and enter buttons
		/// </summary>
		/// <param name="kb">The key event</param>
		/// <returns>Result of base.ProcessKey()</returns>
		public override bool ProcessKey(KeyEvent kb)
		{
			if(kb.Key == Key.CursorLeft)
			{
				if(m_textEdit.Text.Length > 1)
                {
					m_textEdit.Text = new string(m_ratingSymbol, m_textEdit.Text.Length - 1);
				}
				return true;
			}
			else if(kb.Key == Key.CursorRight)
			{
				if(m_textEdit.Text.Length < 5)
                {
					m_textEdit.Text = new string(m_ratingSymbol, m_textEdit.Text.Length + 1);
                }
				return true;
			}
			return base.ProcessKey(kb);
		}
	}

	// TODO: Fixup and simplify (can be made into a shared class or use one)
	public class CEditGameInfoDlg : CEditDlg<GameObject>
	{
		protected CBinaryRadio m_radio;
		private TextField m_textEditAlias;
		private TextField m_textEditRating;
		protected CDialogSelectionPanel m_selectionPanel;

		public CEditGameInfoDlg(GameObject game, List<IDataNode> availableTags)
			: base(game.Title, game, 40, 25)
		{
			// Remove to fix the tab order. Re-add at the bottom
			Label nameLabel = new Label()
			{
				X = 1,
				Y = 2,
				Width = Dim.Fill(),
				Text = $"Title: {game.Title}",
			};
			Add(nameLabel);

			Label ratingLabel = new Label()
			{
				X = 1,
				Y = 4,
				Width = Dim.Fill(),
				Text = $"Rating:",
			};
			Add(nameLabel);

			m_textEditAlias = new TextField()
			{
				X = 1,
				Y = 3,
				Width = Dim.Fill(),
				Text = game.Alias,
				CursorPosition = game.Alias.Length,
				Enabled = true,
				CanFocus = true,
			};
			Add(m_textEditAlias);

			m_textEditRating = new TextField()
			{
				X = 1,
				Y = 4,
				Width = Dim.Fill(),
				Text = new string('*', 3), // TODO: Add game rating
				CursorPosition = game.Alias.Length,
				Enabled = true,
				CanFocus = true,
				ReadOnly = true,
			};
			m_textEditRating.KeyDown += M_textEditAlias_KeyDown;
			Add(m_textEditRating);

			Label isActiveLabel = new Label()
			{
				X = 3,
				Y = 5,
				Width = Dim.Fill(),
				Text = "Favourite: ",
			};
			Add(isActiveLabel);

			m_radio = new CBinaryRadio(isActiveLabel.Text.Length + 5, 5, game.IsFavourite);
			Add(m_radio);

			m_selectionPanel = new CDialogSelectionPanel("Tags", 0, 7, availableTags, true);
			m_selectionPanel.FrameView.Y = 10; // Update the platform selection box
			Add(m_selectionPanel.FrameView);


		}

        private void M_textEditAlias_KeyDown(KeyEventEventArgs kb)
        {
			if(kb.KeyEvent.Key == Key.CursorLeft && m_textEditRating.Text.Length > 1)
			{
				m_textEditRating.Text = new string('*', m_textEditRating.Text.Length - 1);
			}
			else if(kb.KeyEvent.Key == Key.CursorRight && m_textEditRating.Text.Length < 5)
			{
				m_textEditRating.Text = new string('*', m_textEditRating.Text.Length + 1);
			}
		}

        /// <summary>
        /// Function override.
        /// If ok button was pressed, modify the node value with the new value
        /// </summary>
        /// <param name="node">Refernece to a system attribute node</param>
        /// <returns>True if ok button was pressed</returns>
        public override bool Run(ref GameObject currentValue)
		{
			if(!Run())
			{
				return false;
			}
			bool isDirty = false;

			if(m_radio.BoolSelection != m_editValue.IsFavourite)
			{
				//currentValue.IsFavourite = m_radio.BoolSelection;
				isDirty = true;
			}

			if(m_textEditAlias.Text != m_editValue.Alias)
            {
				//currentValue.Alias = m_textEditAlias.Text;
				isDirty = true;
			}

			/*
			if(m_textEditRating.Text.Length != m_editValue.Rating)
			{
				//currentValue.Rating = m_textEditRating.Text.Length;
				isDirty = true;
			}
			*/

			// TODO: Tag support

			return isDirty;
		}
	}
}
