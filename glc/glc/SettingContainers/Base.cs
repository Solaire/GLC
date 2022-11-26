using System.Collections.Generic;
using System.ComponentModel;

using Terminal.Gui;

namespace glc.Settings
{
    /// <summary>
    /// Enum defining different setting categories, as well as their names and descriptions
    /// that will be displayed in the selection list
    /// </summary>
    public enum SettingCategory
    {
        [Category("General"),   Description("Main application settings")]
        cGeneral  = 0,
        [Category("Theme"),     Description("Change and edit colour themes")]
        cTheme    = 1,
        [Category("Platform"),  Description("Manage platforms extensions")]
        cPlatform = 2,
        [Category("Tags"),      Description("Edit game tags")]
        cTags     = 3,
    }

    /// <summary>
    /// Base class for a setting category, containing its own data source and way of
    /// editing/selecting data nodes.
    /// </summary>
    public abstract class CSettingContainer
    {
        public IListDataSource DataSource   { get; protected set; }

        public abstract void EditNode(int selectionIndex);
        public abstract void SelectNode(int selectionIndex);
    }

    /// <summary>
    /// Generic base class inheriting <see cref="CSettingContainer"/> while adding
    /// support for generic collections of data.
    /// </summary>
    /// <typeparam name="T">The type of the internal data list</typeparam>
    public abstract class CGenericSettingContainer<T> : CSettingContainer
    {
        protected List<T> DataList { get; set; }
    }
}
