using Logger;
using SqlDB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace core
{
    /// <summary>
    /// Main entry point into the glc application
    /// Contains all core initialisation logic for database, logging, etc.
    /// Should be inherited by the implementing app class
    /// </summary>
    public abstract class CApplicationCore
    {
        private const string DATA_SCHEMA_PATH = "CreateDB.sql";

        protected List<CPlatform> m_platforms;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected CApplicationCore()
        {
            m_platforms = new List<CPlatform>();
            Initialise();
        }

        /// <summary>
        /// Initialise the library's core functions
        /// </summary>
        /// <returns>True on initialise success</returns>
        protected virtual bool Initialise()
        {
            if(!InitialiseLogger())
            {
                return false;
            }
            if(!InitialiseDatabase())
            {
                CLogger.LogWarn("Error initialising the database: {0}", CSqlDB.Instance.Conn.LastError);
                return false;
            }
            LoadConfig();
            if(!InitialisePlatforms())
            {
                CLogger.LogWarn("Errors initialising platforms");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Initialise the logger
        /// </summary>
        /// <returns>True on initialise success</returns>
        private bool InitialiseLogger()
        {
#if DEBUG
            // Log unhandled exceptions in debug only
            System.AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(CLogger.ExceptionHandleEvent);
#endif // DEBUG

            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            CLogger.Configure(Path.Combine(currentPath,
                Path.GetFileNameWithoutExtension(System.Environment.GetCommandLineArgs()[0]) + ".log")); // Create a log file

            CLogger.LogInfo("{0} version {1}",
                Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title,
                Assembly.GetEntryAssembly().GetName().Version.ToString());
            CLogger.LogInfo("*************************");

            return true;
        }

        /// <summary>
        /// Initialise the database connection
        /// If database does not exist, create new and apply data schema
        /// </summary>
        /// <returns>True on initialise success</returns>
        private bool InitialiseDatabase()
        {
            SQLiteErrorCode err = CSqlDB.Instance.Open(true);
            if(err == SQLiteErrorCode.Ok)
            {
                // Success - no op.
            }
            else if(err == SQLiteErrorCode.Schema)
            {
                // New database, apply schema
                string script = "";
                try
                {
                    script = File.ReadAllText(DATA_SCHEMA_PATH);
                }
                catch(IOException e)
                {
                    CLogger.LogError(e);
                    err = SQLiteErrorCode.Unknown; // Use unknown for non-sql issues
                }
                if(script.Length > 0)
                {
                    err = CSqlDB.Instance.Conn.Execute(script);
                }
            }
            return err == SQLiteErrorCode.Ok;
        }

        /// <summary>
        /// Initialise the platforms using the database and plug-ins
        /// If a platform exists in the database, but the plug-in is missing, deactivate the platform.
        /// If a platform does not exist and the plug-in is loaded, create the platform object and add to database
        /// </summary>
        /// <returns>True on initialise success</returns>
        private bool InitialisePlatforms()
        {
            var pluginLoader = new GenericPluginLoader<IPlatformFactory<CPlatform>>();
            var plugins = pluginLoader.LoadAll(@"C:\dev\GameHub\GameLauncher_Console\neo_glc\bin\Debug\net5.0\platforms");
            Console.WriteLine($"Loaded {plugins.Count} plugin(s)");

            foreach(var plugin in plugins)
            {
                CPlatform platform = null;
                if(!CPlatformSQL.LoadPlatform(plugin, out platform))
                {
                    platform = plugin.CreateDefault();
                    CPlatformSQL.InsertPlatform(platform);
                }
                m_platforms.Add(platform);
            }

            return true;
        }

        protected abstract void LoadConfig();
    }

    internal class GenericPluginLoader<T> where T : class
    {
        private readonly List<GenericAssemblyLoadContext<T>> loadContexts = new List<GenericAssemblyLoadContext<T>>();

        public List<T> LoadAll(string pluginPath, string filter = "*.dll", params object[] constructorArgs)
        {
            List<T> plugins = new List<T>();
            foreach(var filePath in Directory.EnumerateFiles(pluginPath, filter, SearchOption.AllDirectories))
            {
                var plugin = Load(filePath, constructorArgs);
                if(plugin != null)
                {
                    plugins.Add(plugin);
                }
            }
            return plugins;
        }

        private T Load(string pluginPath, params object[] constructorArgs)
        {
            var loadContext = new GenericAssemblyLoadContext<T>(pluginPath);
            loadContexts.Add(loadContext);

            var assembly = loadContext.LoadFromAssemblyPath(pluginPath);
            var type = assembly.GetTypes().FirstOrDefault(t => typeof(T).IsAssignableFrom(t));
            if(type == null)
            {
                return null;
            }
            return (T)Activator.CreateInstance(type, constructorArgs);
        }

        public void UnloadAll()
        {
            foreach(var loadContext in loadContexts)
            {
                loadContext.Unload();
            }
        }
    }

    internal class GenericAssemblyLoadContext<T> : AssemblyLoadContext where T : class
    {
        private AssemblyDependencyResolver resolver;
        private HashSet<string> assembliesToNotLoadIntoContext;

        public GenericAssemblyLoadContext(string pluginPath) : base(isCollectible: true)
        {
            var pluginInterfaceAssembly = typeof(T).Assembly.FullName;
            assembliesToNotLoadIntoContext = GetReferencedAssemblyFullNames(pluginInterfaceAssembly);
            assembliesToNotLoadIntoContext.Add(pluginInterfaceAssembly);

            resolver = new AssemblyDependencyResolver(pluginPath);
        }

        private HashSet<string> GetReferencedAssemblyFullNames(string ReferencedBy)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies().FirstOrDefault(t => t.FullName == ReferencedBy)
                .GetReferencedAssemblies()
                .Select(t => t.FullName)
                .ToHashSet();
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            //Do not load the Plugin Interface DLL into the adapter's context
            //otherwise IsAssignableFrom is false.
            if(assembliesToNotLoadIntoContext.Contains(assemblyName.FullName))
            {
                return null;
            }

            string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if(assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if(libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
