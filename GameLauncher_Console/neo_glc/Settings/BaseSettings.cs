using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Terminal.Gui;
using static core.CSystemAttributeSQL;

namespace glc.Settings
{
    /// <summary>
    /// Enum describing the type of settings in the category
    /// </summary>
    public enum SettingType
    {
        cValue          = 0, // Single value to write (string, number, bool, etc)
        cSingleSelect   = 1, // Select one item from the list (multiselect = off)
        cObject         = 2, // Linked to a more advanced object (eg. colour theme).
    }

    /// <summary>
    /// Custom attribute for SettingType
    /// </summary>
    public class CSettingType : System.Attribute
    {
        public SettingType Type { get; private set; }

        public CSettingType(SettingType type)
        {
            this.Type = type;
        }
    }

    /// <summary>
    /// Setting category enum, with title, description and value types
    /// </summary>
    public enum SettingCategory
    {
        [Category("General"),   Description("Main application settings"),       CSettingType(SettingType.cValue)]
        cGeneral  = 0,
        [Category("Theme"),     Description("Change and edit colour themes"),   CSettingType(SettingType.cSingleSelect)]
        cTheme    = 1,
        [Category("Platform"),  Description("Manage platforms extensions"),     CSettingType(SettingType.cObject)]
        cPlatform = 2,
        [Category("Tags"),      Description("Edit game tags"),                  CSettingType(SettingType.cObject)]
        cTags     = 3,
    }

    /// <summary>
    /// Base class for managing different types of settings, from simple on/off to more complex obejcts
    /// </summary>
    /// <typeparam name="T">Type T to be used in the list of settings</typeparam>
    public abstract class CSettingContainer
    {
        public IListDataSource DataSource   { get; protected set; }

        public abstract void EditNode(int selectionIndex);
        public abstract void SelectNode(int selectionIndex);
    }

    public abstract class CGenericSettingContainer<T> : CSettingContainer
    {
        protected List<T> DataList { get; set; }
    }
}
