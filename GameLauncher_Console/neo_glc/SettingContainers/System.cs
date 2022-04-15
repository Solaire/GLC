using System;
using System.Collections.Generic;

using core.SystemAttribute;
using glc.UI;

namespace glc.Settings
{
    /// <summary>
    /// Implementation of the <see cref="CGenericSettingContainer"/> class for managing
    /// system (application) settings.
    /// </summary>
    public class CSystemSettingsContainer : CGenericSettingContainer<SystemAttributeNode>
    {
        /// <summary>
        /// Constructor.
        /// Load the system attributes and create the data source.
        /// </summary>
        public CSystemSettingsContainer()
        {
            DataList    = CSystemAttributeSQL.GetAllNodes();
            DataSource  = new CSystemSettingsDataSource(DataList);
        }

        /// <summary>
        /// Load the selected setting into an edit dialog and display to user.
        /// If used closes the dialog with 'OK', update the setting and data source.
        /// </summary>
        /// <param name="selectionIndex"></param>
        public override void EditNode(int selectionIndex)
        {
            SystemAttributeNode selected = DataList[selectionIndex];
            bool updateDB = false;

            if(selected.AttributeType == CSystemAttributeSQL.AttributeType.cTypeInteger)
            {
                string value = selected.AttributeValue;
                updateDB = EditNode(ref value, new CEditIntDlg(selected.AttributeDescription, selected.AttributeValue));
                selected.AttributeValue = value;
            }
            else if(selected.AttributeType == CSystemAttributeSQL.AttributeType.cTypeBool)
            {
                bool value = selected.IsTrue();
                updateDB = EditNode(ref value, new CEditBoolDlg(selected.AttributeDescription, selected.IsTrue()));
                selected.SetBool(value);
            }
            else
            {
                string value = selected.AttributeValue;
                updateDB = EditNode(ref value, new CEditStringDlg(selected.AttributeDescription, selected.AttributeValue));
                selected.AttributeValue = value;
            }

            if(updateDB)
            {
                DataList[selectionIndex] = selected;
                DataSource.ToList()[selectionIndex] = selected;

                CSystemAttributeSQL.UpdateNode(selected);
            }
        }

        public override void SelectNode(int selectionIndex)
        {
            EditNode(selectionIndex);
        }

        private bool EditNode<T>(ref T currentValue, CEditDlg<T> editDlg)
        {
            if(editDlg.Run(ref currentValue))
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Implementation of the <see cref="CGenericDataSource"/> class for displaying
    /// the system settings in the list container
    /// </summary>
    internal class CSystemSettingsDataSource : CGenericDataSource<SystemAttributeNode>
    {
        private readonly long m_maxDescLength;

        /// <summary>
        /// Constructor.
        /// Determine the maximum length of setting description, for list formatting.
        /// </summary>
        /// <param name="itemList">The list of <see cref="SystemAttributeNode"/> nodes</param>
        public CSystemSettingsDataSource(List<SystemAttributeNode> itemList)
            : base(itemList)
        {
            for(int i = 0; i < itemList.Count; i++)
            {
                if(ItemList[i].AttributeDescription.Length > m_maxDescLength)
                {
                    m_maxDescLength = ItemList[i].AttributeDescription.Length;
                }
            }
        }

        /// <summary>
        /// Construct the string that will be displayed in the container list
        /// </summary>
        /// <param name="itemIndex">The index of the item list</param>
        /// <returns>Formatted string</returns>
        protected override string ConstructString(int itemIndex)
        {
            string description = ItemList[itemIndex].AttributeDescription;
            string value       = ItemList[itemIndex].AttributeValue;
            String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxDescLength), description);
            return $"{s1}  {value}";
        }

        /// <summary>
        /// Get the system attribute description string.
        /// </summary>
        /// <param name="itemIndex">The index of the item list</param>
        /// <returns>system attribute description</returns>
        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].AttributeDescription;
        }
    }
}
