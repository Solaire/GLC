using SqlDB;

namespace core
{
    /// <summary>
    /// Base configuration class
    /// Basic configuration will read/write to "SystemAttribute" table.
    /// </summary>
    public abstract class CConfig
    {
        // List of supported core system attributes
        public const string A_SHOW_HIDDEN_GAMES = "SHOW_HIDDEN_GAMES";

        private CDbAttribute m_systemAttribute;

        /// <summary>
        /// Getter for system attribute
        /// </summary>
        public CDbAttribute SystemAttribute { get { return m_systemAttribute; } }

        /// <summary>
        /// Constructor.
        /// Create the attribute object and set the ID to 1
        /// </summary>
        protected CConfig()
        {
            m_systemAttribute = new CDbAttribute("System");
        }
    }
}
