using core_2.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core_2.Platform
{
    public class InternalPlatform : IData
    {
        #region IData

        public int ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            private set;
        }

        public bool IsEnabled
        {
            get;
            private set;
        }

        #endregion IData

        #region Properties

        /// <summary>
        /// Platform description
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Path to the platform root directory
        /// </summary>
        public string Path
        {
            get;
            private set;
        }

        #endregion Properties

        /// <summary>
        /// Check if this is a special platform
        /// </summary>
        // public bool IsSpecialPlatform { get { return m_id < 0; } }

        protected InternalPlatform() { }

        public static InternalPlatform CreateNew(int id, string name, string description, string path, bool isEnabled)
        {
            return new InternalPlatform()
            {
                ID = id,
                Name = name,
                Description = description,
                Path = path,
                IsEnabled = isEnabled
            };
        }
        internal static InternalPlatform CreateFromDB(PlatformSQL.QryReadPlatform qry)
        {
            return new InternalPlatform()
            {
                ID = qry.PlatformID,
                Name = qry.Name,
                Description = qry.Description,
                Path = qry.Path,
                IsEnabled = qry.IsActive
            };
        }
    }
}
