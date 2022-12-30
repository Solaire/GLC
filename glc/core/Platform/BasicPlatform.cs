using core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.Platform
{
    public class CBasicPlatform : IDataNode
    {
        protected readonly string m_name;
        protected readonly string m_description;
        protected readonly string m_path;

        protected int    m_id;
        protected bool   m_isEnabled;

        /// <summary>
        /// Platform unique name
        /// </summary>
        public string Name { get { return m_name; } }

        /// <summary>
        /// Platform description
        /// </summary>
        public string Description { get { return m_description; } }

        /// <summary>
        /// Path to the platform root directory
        /// </summary>
        public string Path { get { return m_path; } }

        /// <summary>
        /// Check if this is a special platform
        /// </summary>
        public bool IsSpecialPlatform { get { return m_id < 0; } }

        /// <summary>
        /// The PlatformID database primary key getter and setter
        /// </summary>
        public int PrimaryKey
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// IsActive flag getter and setter
        /// </summary>
        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set { m_isEnabled = value; }
        }

        public CBasicPlatform(int id, string name, string description, string path, bool isEnabled)
        {
            m_id = id;
            m_name = name;
            m_description = description;
            m_path = path;
            m_isEnabled = isEnabled;
        }

        public CBasicPlatform(CPlatformSQL.CQryReadPlatform qry)
        {
            m_id = qry.PlatformID;
            m_name = qry.Name;
            m_description = qry.Description;
            m_path = qry.Path;
            m_isEnabled = qry.IsActive;
        }

        /// <summary>
        /// Comparison function.
        /// Compare this platform with another, based on ID property
        /// </summary>
        /// <param name="other">The other platform object</param>
        /// <returns>True if this.ID > other.ID</returns>
        public bool SortByID(CPlatform other)
        {
            return this.PrimaryKey > other.PrimaryKey;
        }
    }
}
