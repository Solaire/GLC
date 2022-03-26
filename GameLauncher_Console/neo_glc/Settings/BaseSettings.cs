using System.ComponentModel;

namespace glc.Settings
{
    /// <summary>
    /// Enum describing the type of settings in the category
    /// </summary>
    public enum SettingType
    {
        cToggle         = 0, // on/off toggle value (boolean).
        cValue          = 1, // Single value to write (string, number, etc)
        cSingleSelect   = 2, // Select one item from the list (multiselect = off)
        cObject         = 3, // Linked to a more advanced object (eg. colour theme). Open dialog
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
    public abstract class CSettings<T>
    {
        /// <summary>
        /// List of settings for the category
        /// </summary>
        public System.Collections.Generic.List<T> Settings { get; protected set; }

        /// <summary>
        /// Constructor.
        /// Load the settings
        /// </summary>
        public CSettings()
        {
            Load();
        }

        /// <summary>
        /// Load settings into the internal list
        /// </summary>
        protected abstract void Load();

        /// <summary>
        /// Save settings to database
        /// </summary>
        public abstract void Save(T node);
    }
}
