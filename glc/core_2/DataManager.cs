using System.Reflection;
using System.Runtime.Loader;
using Logger;
using core_2.DataAccess;
using core_2.Platform;

namespace core_2
{
    public static class CDataManager
    {
        private static List<CPlatform> m_platforms = new List<CPlatform>();

        public static bool Initialise()
        {
            return InitialisePlatforms();
        }

        /// <summary>
        /// Load the plugins found in the "platforms" folder.
        /// Either load the plaftorms from the database (if exist) or
        /// create default instance and insert into the database
        /// </summary>
        /// <returns>True on initialise success</returns>
        private static bool InitialisePlatforms()
        {
            var pluginLoader = new PluginLoader<CPlatformFactory<CPlatform>>();
            var plugins = pluginLoader.LoadAll(@"C:\dev\GameHub\glc\glc\bin\Debug\net6.0\platforms"); // TODO: path
            CLogger.LogInfo($"Loaded {plugins.Count} plugin(s)");

            foreach(var plugin in plugins)
            {
                CLogger.LogInfo($"Loaded plugin: {plugin.GetPlatformName()}");
                CPlatform platform = null;
                if(!CPlatformSQL.LoadPlatform(plugin, out platform))
                {
                    platform = plugin.CreateDefault();
                    CPlatformSQL.InsertPlatform(platform);
                    CLogger.LogInfo($"Added new platform: {platform.Name}");
                }
                m_platforms.Add(platform);
            }

            // We've loaded all data, no longer need to keep the DLLs loaded.
            pluginLoader.UnloadAll();
            return true;
        }
    }

    /// <summary>
    /// Generic plugin loader class
    /// </summary>
    /// <typeparam name="T">Generic type T</typeparam>
    internal class PluginLoader<T> where T : class
    {
        private readonly List<PluginAssemblyLoadContext<T>> loadContexts = new List<PluginAssemblyLoadContext<T>>();

        /// <summary>
        /// Load all available plugins found in the specified folder
        /// Plugins must match the generic type T
        /// </summary>
        /// <param name="pluginFolder">The plugin folder absolute path</param>
        /// <returns>List of objects of generic type T</returns>
        public List<T> LoadAll(string pluginFolder)
        {
            List<T> plugins = new List<T>();
            foreach(var filePath in Directory.EnumerateFiles(pluginFolder, "*.dll", SearchOption.AllDirectories))
            {
                // TEMP
                if(filePath.Contains("BasePlatform.dll"))
                {
                    Assembly.LoadFrom(filePath);
                    continue;
                }

                T plugin = Load(filePath);
                if(plugin != null)
                {
                    plugins.Add(plugin);
                }
            }
            return plugins;
        }

        /// <summary>
        /// Load plugin from dll file, adding its context to the internal list
        /// </summary>
        /// <param name="pluginPath">The absolute path to DLL file</param>
        /// <returns>Generic instance of type T</returns>
        private T Load(string pluginPath)
        {
            PluginAssemblyLoadContext<T> loadContext = new PluginAssemblyLoadContext<T>(pluginPath);
            loadContexts.Add(loadContext);

            Assembly assembly = loadContext.LoadFromAssemblyPath(pluginPath);
            var type = assembly.GetTypes().FirstOrDefault(t => typeof(T).IsAssignableFrom(t));
            if(type == null)
            {
                return null;
            }
            return (T)Activator.CreateInstance(type);
        }

        /// <summary>
        /// Unload all plugins from internal list
        /// </summary>
        public void UnloadAll()
        {
            foreach(PluginAssemblyLoadContext<T> loadContext in loadContexts)
            {
                loadContext.Unload();
            }
        }
    }

    /// <summary>
    /// Generic plugin context class
    /// </summary>
    /// <typeparam name="T">Generic type T</typeparam>
    internal class PluginAssemblyLoadContext<T> : AssemblyLoadContext where T : class
    {
        private AssemblyDependencyResolver resolver;
        private HashSet<string> assembliesToNotLoadIntoContext;

        /// <summary>
        /// Constructor.
        /// Creatre plugin context from plugin file
        /// </summary>
        /// <param name="pluginPath">The absolute path to the plugin DLL file</param>
        public PluginAssemblyLoadContext(string pluginPath)
            : base(isCollectible: true)
        {
            var pluginInterfaceAssembly = typeof(T).Assembly.FullName;
            assembliesToNotLoadIntoContext = GetReferencedAssemblyFullNames(pluginInterfaceAssembly);
            assembliesToNotLoadIntoContext.Add(pluginInterfaceAssembly);

            resolver = new AssemblyDependencyResolver(pluginPath);
        }

        /// <summary>
        /// Get the hashset of referenced assembly names
        /// </summary>
        /// <param name="ReferencedBy">The string describing the referer of the target assemblies</param>
        /// <returns>String hashset</returns>
        private HashSet<string> GetReferencedAssemblyFullNames(string ReferencedBy)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies().FirstOrDefault(t => t.FullName == ReferencedBy)
                .GetReferencedAssemblies()
                .Select(t => t.FullName)
                .ToHashSet();
        }
    }
}
