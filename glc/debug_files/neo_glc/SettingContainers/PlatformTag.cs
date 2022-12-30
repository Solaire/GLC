using System;
using System.Collections.Generic;
using System.Linq;

using core;
using core.Tag;
using core.Platform;
using Terminal.Gui;
using glc.UI;

namespace glc.Settings
{
    // Contailer begin
    public class CTagsSettingsContainer : CGenericSettingContainer<TagObject>
    {
        public CTagsSettingsContainer()
        {
            DataList = CTagSQL.GetTags();
            DataSource = new CTagSettingDataSource(DataList);
        }

        public override void EditNode(int selectionIndex)
        {
            TagObject selected = DataList[selectionIndex];
            List<CBasicPlatform> tempPlatforms = CTagSQL.GetPlatformsForTag(selected.tagID);
            List<IDataNode> currentPlatforms = new List<IDataNode>(tempPlatforms.Cast<IDataNode>());

            CEditTagDlg dlg = new CEditTagDlg(selected, currentPlatforms);

            if(dlg.Run(ref selected))
            {
                CTagSQL.ToggleActive(selected.tagID, selected.IsEnabled);

                if(!selected.isInternal)
                {
                    selected.name = dlg.TagName;
                    selected.description = dlg.TagDescription;
                    CTagSQL.UpdateNameDescription(selected);
                }

                DataList[selectionIndex] = selected;
                DataSource.ToList()[selectionIndex] = selected;
            }

            if(dlg.IsOkayPressed() && dlg.IsSelectionDirty())
            {
                foreach(IDataNode platform in currentPlatforms)
                {
                    CPlatformSQL.SetTag(platform.PrimaryKey, selected.tagID, platform.IsEnabled);
                }
            }
        }

        public override void SelectNode(int selectionIndex)
        {
            EditNode(selectionIndex);
        }
    }

    internal class CTagSettingDataSource : CDataNodeDataSource<TagObject>
    {
        public CTagSettingDataSource(List<TagObject> tags)
            : base(tags)
        {

        }

        protected override string ConstructString(int itemIndex)
        {
            string strEnabled  = (ItemList[itemIndex].IsEnabled)  ? "Enabled"  : "Disabled";
            string strInternal = (ItemList[itemIndex].isInternal) ? "INTERNAL" : "";

            String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxNameLength),  ItemList[itemIndex].name);
            String s2 = String.Format(String.Format("{{0,{0}}}", -m_maxDescLength),  ItemList[itemIndex].description);
            String s3 = String.Format(String.Format("{{0,{0}}}", "Disabled".Length), strEnabled);
            String s4 = String.Format(String.Format("{{0,{0}}}", 0), strInternal);

            return $"{s1}  {s2}  {s3}  {s4}";
        }
    }

    public class CEditTagDlg : CEditSelectionDlg<TagObject>
    {
        private TextField m_textEditName;
        private TextField m_textEditDescription;

        public string TagName
        {
            get { return m_textEditName.Text.ToString(); }
        }

        public string TagDescription
        {
            get { return m_textEditDescription.Text.ToString(); }
        }

        public CEditTagDlg(TagObject tag, List<IDataNode> platforms)
            : base(tag, platforms)
        {
            // Remove to fix the tab order. Re-add at the bottom
            Remove(m_selectionPanel.FrameView);

            Label nameLabel = new Label()
            {
                X = 1,
                Y = 2,
                Width = Dim.Fill(),
                Text = "Name: ",
            };
            Add(nameLabel);

            Label descriptionLabel = new Label()
            {
                X = 1,
                Y = 5,
                Width = Dim.Fill(),
                Text = "Description: ",
            };
            Add(descriptionLabel);

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
            Add(m_textEditName);

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
            Add(m_textEditDescription);

            m_selectionPanel.FrameView.Y = 10; // Update the platform selection box
            Add(m_selectionPanel.FrameView);
        }

        /// <summary>
        /// Function override.
        /// If ok button was pressed, modify the node value with the new value
        /// </summary>
        /// <param name="node">Refernece to a system attribute node</param>
        /// <returns>True if ok button was pressed</returns>
        public override bool Run(ref TagObject currentValue)
        {
            if(!Run())
            {
                return false;
            }
            bool isDirty = false;

            if(m_radio.BoolSelection != m_editValue.isEnabled)
            {
                currentValue.isEnabled = m_radio.BoolSelection;
                isDirty = true;
            }

            if(!m_editValue.isInternal
                && (m_editValue.name != m_textEditName.Text.ToString() || m_editValue.description != m_textEditDescription.Text.ToString()))
            {
                currentValue.name = m_textEditName.Text.ToString();
                currentValue.description = m_textEditDescription.Text.ToString();
                isDirty = true;
            }

            return isDirty;
        }
    }
}
