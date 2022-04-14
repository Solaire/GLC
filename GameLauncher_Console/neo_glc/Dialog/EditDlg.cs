using core;
using glc.Settings;
using NStack;
using System;
using System.Collections;
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
		public CEditDlg(string title, int width = 40, int height = 10)
            : base(title, width, height)
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

	public class CEditPlatformDlg : CEditDlg
    {
		private Label m_isActiveLabel;

		private bool m_isTrue;
		private CBoolRadio m_radio;

		private CTagPanel m_tagPanel;

		public List<TagObject> Tags
        {
			get { return m_tagPanel.ContentList; }
        }

		/// <summary>
		/// Constructor.
		/// Create a radio control with true/false values
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="isInitialTrue">Initial boolean value</param>
		public CEditPlatformDlg(CBasicPlatform platform)
			: base(platform.Name, 40, 25)
		{
			m_isActiveLabel = new Label()
			{
				X = 3,
				Y = 1,
				Width = Dim.Fill(),
				Text = "Enabled ",
			};

			m_tagPanel = new CTagPanel(platform.ID, platform.IsActive);

			m_isTrue = platform.IsActive;
			m_radio = new CBoolRadio(m_isActiveLabel.Text.Length + 5, 1, m_isTrue);
			Add(m_isActiveLabel);
			Add(m_radio);
			Add(m_tagPanel.FrameView);

            m_radio.SelectedItemChanged += M_radio_SelectedItemChanged;
		}

        private void M_radio_SelectedItemChanged(RadioGroup.SelectedItemChangedArgs obj)
        {
			bool enabled = (obj.SelectedItem == 0);
			m_tagPanel.FrameView.Enabled = enabled;
			m_tagPanel.FrameView.Visible = enabled;

			SetNeedsDisplay();
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
					case Key.CursorLeft:	return base.ProcessKey(new KeyEvent(Key.CursorUp, new KeyModifiers()));
					case Key.CursorRight:	return base.ProcessKey(new KeyEvent(Key.CursorDown, new KeyModifiers()));
					case Key.Enter:			return base.ProcessKey(new KeyEvent(Key.Space, new KeyModifiers()));
				}
				return base.ProcessKey(kb);
			}
		}

		private class CTagPanel : CFramePanel<TagObject, ListView>
        {
			public CTagPanel(int platformID, bool isActive)
				: base("Tags", 3, 4, Dim.Fill(), Dim.Fill(), true, Key.CtrlMask | Key.C)
            {
				m_contentList = CTagSQL.GetTagsforPlatform(platformID);

				Initialise("Tags", 0, 4, Dim.Fill(), Dim.Fill(3), true, Key.CtrlMask | Key.C);

				m_frameView.Enabled = isActive;
				m_frameView.Visible = isActive;
			}

			public override void CreateContainerView()
			{
				m_containerView = new ListView(new CTagsDataSource(m_contentList))
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
		}

		internal class CTagsDataSource : CGenericDataSource<TagObject>
		{
			private BitArray marks;

			public CTagsDataSource(List<TagObject> itemList)
				: base(itemList)
			{
				marks = new BitArray(Count);
				for(int i = 0; i < itemList.Count; i++)
                {
					marks[i] = (itemList[i].isActive);
                }
			}

			protected override string ConstructString(int itemIndex)
			{
				return ItemList[itemIndex].name;
			}

			protected override string GetString(int itemIndex)
			{
				return ItemList[itemIndex].name;
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

					TagObject copy = ItemList[item];
					copy.isActive = value;
					ItemList[item] = copy;
				}
			}
		}
	}

	/// <summary>
	///
	/// </summary>
	public class CEditTagDlg : CEditDlg
	{
		private Label m_isActiveLabel;
		private Label m_nameLabel;
		private Label m_descriptionLabel;

		private bool m_isTrue;
		private CBoolRadio m_radio;

		private CPlatformPanel m_platformPanel;

		protected TextField m_textEditName;
		protected TextField m_textEditDescription;

		public List<CBasicPlatform> Platforms
		{
			get { return m_platformPanel.ContentList; }
		}

		public string TagName
        {
			get { return m_textEditName.Text.ToString(); }
        }

		public string TagDescription
		{
			get { return m_textEditDescription.Text.ToString(); }
		}

		/// <summary>
		/// Constructor.
		/// Create a radio control with true/false values
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="isInitialTrue">Initial boolean value</param>
		public CEditTagDlg(TagObject tag)
			: base(tag.name, 40, 30)
		{
			m_isActiveLabel = new Label()
			{
				X = 1,
				Y = 1,
				Width = Dim.Fill(),
				Text = "Enabled: ",
			};
			m_nameLabel = new Label()
			{
				X = 1,
				Y = 2,
				Width = Dim.Fill(),
				Text = "Name: ",
			};
			m_descriptionLabel = new Label()
			{
				X = 1,
				Y = 5,
				Width = Dim.Fill(),
				Text = "Description: ",
			};

			m_textEditName = new TextField()
			{
				X = 1,
				Y = 3,
				Width = Dim.Fill(),
				Text = tag.name,
				CursorPosition = tag.name.Length,
				Enabled = !tag.isInternal,
				CanFocus = !tag.isInternal,
			};

			m_textEditDescription = new TextField()
			{
				X = 1,
				Y = 6,
				Width = Dim.Fill(),
				Text = tag.description,
				CursorPosition = tag.description.Length,
				Enabled = !tag.isInternal,
				CanFocus = !tag.isInternal,
			};

			m_isTrue = tag.isActive;
			m_radio = new CBoolRadio(m_isActiveLabel.Text.Length + 5, 1, m_isTrue);

			m_platformPanel = new CPlatformPanel(tag.tagID, tag.isActive);

			Add(m_isActiveLabel);
			Add(m_radio);
			Add(m_nameLabel);
			Add(m_descriptionLabel);
			Add(m_textEditName);
			Add(m_textEditDescription);
			Add(m_platformPanel.FrameView);

			m_radio.SelectedItemChanged += M_radio_SelectedItemChanged;
		}

		private void M_radio_SelectedItemChanged(RadioGroup.SelectedItemChangedArgs obj)
		{
			bool enabled = (obj.SelectedItem == 0);
			m_platformPanel.FrameView.Enabled = enabled;
			m_platformPanel.FrameView.Visible = enabled;

			SetNeedsDisplay();
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
					case Key.CursorLeft: return base.ProcessKey(new KeyEvent(Key.CursorUp, new KeyModifiers()));
					case Key.CursorRight: return base.ProcessKey(new KeyEvent(Key.CursorDown, new KeyModifiers()));
					case Key.Enter: return base.ProcessKey(new KeyEvent(Key.Space, new KeyModifiers()));
				}
				return base.ProcessKey(kb);
			}
		}

		private class CPlatformPanel : CFramePanel<CBasicPlatform, ListView>
		{
			public CPlatformPanel(int tagID, bool isActive)
				: base("Platforms", 3, 10, Dim.Fill(), Dim.Fill(), true, Key.CtrlMask | Key.C)
			{
				m_contentList = CTagSQL.GetPlatformsForTag(tagID);

				Initialise("Platforms", 0, 10, Dim.Fill(), Dim.Fill(3), true, Key.CtrlMask | Key.C);

				m_frameView.Enabled = isActive;
				m_frameView.Visible = isActive;
			}

			public override void CreateContainerView()
			{
				m_containerView = new ListView(new CPlatformDataSourceInternal(m_contentList))
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
		}

		internal class CPlatformDataSourceInternal : CGenericDataSource<CBasicPlatform>
		{
			private BitArray marks;

			public CPlatformDataSourceInternal(List<CBasicPlatform> itemList)
				: base(itemList)
			{
				marks = new BitArray(Count);
				for(int i = 0; i < itemList.Count; i++)
				{
					marks[i] = (itemList[i].IsActive);
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

					CBasicPlatform copy = ItemList[item];
					copy.IsActive = value;
					ItemList[item] = copy;
				}
			}
		}
	}

	// TODO: Implement
	/*
	/// <summary>
	/// Implementation of the CEditDlg for editing colour themes
	/// </summary>
	public class CEditColourDlg : CEditDlg
    {
		protected Label			m_colourNormal;
		protected Label			m_colourFocus;
		protected Label			m_colourHotNormal;
		protected Label         m_colourHotFocus;
		protected Label			m_colourDisabled;

		protected ColorScheme	m_colourScheme;

		/// <summary>
		/// Constructor.
		/// Create the text edit box
		/// </summary>
		/// <param name="title">The dialog title</param>
		/// <param name="initialScheme">Initial colour scheme</param>
		public CEditColourDlg(string title, ColorScheme initialScheme)
			: base(title)
		{
			m_colourScheme = initialScheme;

			m_colourNormal = new Label()
			{
				X = 4,
				Y = 4,
				Width = Dim.Fill(),
				Text = $"Normal Bg: {m_colourScheme.Normal.Background} | Fg: {m_colourScheme.Normal.Foreground}",
			};
			m_colourFocus = new Label()
			{
				X = 4,
				Y = 5,
				Width = Dim.Fill(),
				Text = $"Focus Bg: {m_colourScheme.Focus.Background} | Fg: {m_colourScheme.Focus.Foreground}",
			};
			m_colourHotNormal = new Label()
			{
				X = 4,
				Y = 6,
				Width = Dim.Fill(),
				Text = $"Hot Normal Bg: {m_colourScheme.HotNormal.Background} | Fg: {m_colourScheme.HotNormal.Foreground}",
			};
			m_colourHotFocus = new Label()
			{
				X = 4,
				Y = 7,
				Width = Dim.Fill(),
				Text = $"Hot Focus Bg: {m_colourScheme.HotFocus.Background} | Fg: {m_colourScheme.HotFocus.Foreground}",
			};
			m_colourDisabled = new Label()
			{
				X = 4,
				Y = 8,
				Width = Dim.Fill(),
				Text = $"Disabled Bg: {m_colourScheme.Disabled.Background} | Fg: {m_colourScheme.Disabled.Foreground}",
			};

			Add(m_colourNormal);
			Add(m_colourFocus);
			Add(m_colourHotNormal);
			Add(m_colourHotFocus);
			Add(m_colourDisabled);
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
				//node.AttributeValue = m_textEdit.Text.ToString();
				return true;
			}
			return false;
		}
	}
	*/
}
