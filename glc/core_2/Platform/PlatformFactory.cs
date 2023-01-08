using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core_2.Platform
{
    /// <summary>
    /// Platform factory interface for creating instances of T (derived classes)
    /// </summary>
    /// <typeparam name="T">Type that inherits from CPlatform abstract class</typeparam>
    public abstract class CPlatformFactory<T> where T : CPlatform
    {
        /// <summary>
        /// Create instance of child CPlatform with exisitng data
        /// </summary>
        /// <param name="id">The platform ID</param>
        /// <param name="name">The platform name</param>
        /// <param name="description">The platform description</param>
        /// <param name="path">The path to platform directory</param>
        /// <param name="isActive">IsActive flag</param>
        /// <returns>Instance of T (child of CPlatform)</returns>
        public abstract T CreateFromDatabase(int id, string name, string description, string path, bool isActive);

        /// <summary>
        /// Create default instance of child CPlatform
        /// </summary>
        /// <returns>Instance of T (child of CPlatform)</returns>
        public abstract T CreateDefault();

        /// <summary>
        /// Return the name of the platform.
        /// </summary>
        /// <returns>Platform name string</returns>
        public abstract string GetPlatformName();
    }
}
