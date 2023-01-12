using System.Reflection;
using System.Runtime.Loader;
using Logger;
using core_2.DataAccess;
using core_2.Platform;
using core_2.Game;

namespace core_2
{
    public static class DataManager
    {
        private static Dictionary<int, Platform.Platform> m_platforms = new Dictionary<int, Platform.Platform>();
        private static Dictionary<string, List<CGame>> m_searchResults = new Dictionary<string, List<CGame>>();

        public static List<Platform.Platform> Platforms => m_platforms.Values.ToList();

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
            var testPlatformFactory = new CTestPlatformFactory();
            m_platforms[1] = testPlatformFactory.CreateFromDatabase(1, "Test ID 1", "ID1", "", true);
            m_platforms[2] = testPlatformFactory.CreateFromDatabase(2, "Test ID 2", "ID2", "", true);
            return true;

            /*
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
            */
        }

        public static bool GetBoolSetting(string key, bool defaultValue = true)
        {
            return defaultValue; // TODO
        }

        public static List<CTag> GetTagsForPlatform(int platformID)
        {
            return new List<CTag>()
            {
                CTag.CreateNew(1, "Installed", true, ""),
                CTag.CreateNew(2, "Not installed", true, "")
            };
        }

        public static void LoadPlatformGames(int platformID, bool reload = false)
        {
            if(!m_platforms.ContainsKey(platformID) || platformID <= 0)
            {
                return;
            }

            Platform.Platform platform = m_platforms[platformID];

            if(platform.IsLoaded && !reload)
            {
                return;
            }

            if(platform.IsLoaded && reload)
            {
                platform.m_games.Clear();
            }

            platform.m_games = LoadGames(platformID);
        }

        private static Dictionary<string, List<CGame>> LoadGames(int platformID)
        {
            if(platformID == 1)
            {
                return new Dictionary<string, List<CGame>>()
                {
                    {
                        "Installed", new List<CGame>()
                        {
                            CGame.CreateNew("Installed 1", 1, "Installed1", "Installed1", "", "Installed"),
                            CGame.CreateNew("Installed 2", 1, "Installed2", "Installed2", "", "Installed"),
                            CGame.CreateNew("Installed 3", 1, "Installed3", "Installed3", "", "Installed"),
                            CGame.CreateNew("Installed 4", 1, "Installed4", "Installed4", "", "Installed"),
                            CGame.CreateNew("Installed 5", 1, "Installed5", "Installed5", "", "Installed"),
                            CGame.CreateNew("Installed 6", 1, "Installed6", "Installed6", "", "Installed"),
                            CGame.CreateNew("Installed 7", 1, "Installed7", "Installed7", "", "Installed"),
                            CGame.CreateNew("Installed 8", 1, "Installed8", "Installed8", "", "Installed"),
                            CGame.CreateNew("Installed 9", 1, "Installed9", "Installed9", "", "Installed"),
                        }
                    },
                    {
                        "Not installed", new List<CGame>()
                        {
                            CGame.CreateNew("Deleted 1", 1, "Deleted1", "Deleted1", "", "Not installed"),
                            CGame.CreateNew("Deleted 2", 1, "Deleted2", "Deleted2", "", "Not installed"),
                            CGame.CreateNew("Deleted 3", 1, "Deleted3", "Deleted3", "", "Not installed"),
                            CGame.CreateNew("Deleted 4", 1, "Deleted4", "Deleted4", "", "Not installed"),
                            CGame.CreateNew("Deleted 5", 1, "Deleted5", "Deleted5", "", "Not installed"),
                            CGame.CreateNew("Deleted 6", 1, "Deleted6", "Deleted6", "", "Not installed"),
                            CGame.CreateNew("Deleted 7", 1, "Deleted7", "Deleted7", "", "Not installed"),
                            CGame.CreateNew("Deleted 8", 1, "Deleted8", "Deleted8", "", "Not installed"),
                            CGame.CreateNew("Deleted 9", 1, "Deleted9", "Deleted9", "", "Not installed"),
                        }
                    }
                };
            }

            return new Dictionary<string, List<CGame>>()
            {
                {
                    "Installed", new List<CGame>()
                    {
                        CGame.CreateNew("Installed 10", 2, "Installed10", "Installed10", "", "Installed"),
                        CGame.CreateNew("Installed 20", 2, "Installed20", "Installed20", "", "Installed"),
                        CGame.CreateNew("Installed 30", 2, "Installed30", "Installed30", "", "Installed"),
                        CGame.CreateNew("Installed 40", 2, "Installed40", "Installed40", "", "Installed"),
                        CGame.CreateNew("Installed 50", 2, "Installed50", "Installed50", "", "Installed"),
                        CGame.CreateNew("Installed 60", 2, "Installed60", "Installed60", "", "Installed"),
                        CGame.CreateNew("Installed 70", 1, "Installed70", "Installed70", "", "Installed"),
                        CGame.CreateNew("Installed 80", 1, "Installed80", "Installed80", "", "Installed"),
                        CGame.CreateNew("Installed 90", 1, "Installed90", "Installed90", "", "Installed"),
                    }
                },
                {
                    "Not installed", new List<CGame>()
                    {
                        CGame.CreateNew("Deleted 10", 2, "Deleted10", "Deleted10", "", "Not installed"),
                        CGame.CreateNew("Deleted 20", 2, "Deleted20", "Deleted20", "", "Not installed"),
                        CGame.CreateNew("Deleted 30", 2, "Deleted30", "Deleted30", "", "Not installed"),
                        CGame.CreateNew("Deleted 40", 2, "Deleted40", "Deleted40", "", "Not installed"),
                        CGame.CreateNew("Deleted 50", 2, "Deleted50", "Deleted50", "", "Not installed"),
                        CGame.CreateNew("Deleted 60", 2, "Deleted60", "Deleted60", "", "Not installed"),
                        CGame.CreateNew("Deleted 70", 1, "Deleted70", "Deleted70", "", "Not installed"),
                        CGame.CreateNew("Deleted 80", 1, "Deleted80", "Deleted80", "", "Not installed"),
                        CGame.CreateNew("Deleted 90", 1, "Deleted90", "Deleted90", "", "Not installed"),
                    }
                }
            };
        }

        public static Dictionary<string, List<CGame>> GetPlatformGames(int platformID)
        {
            if(platformID <= 0)
            {
                return m_searchResults;
            }
            return (m_platforms.ContainsKey(platformID)) ? m_platforms[platformID].m_games : new Dictionary<string, List<CGame>>();
        }

        public static void GameSearch(string searchTerm)
        {
            if(m_searchResults.ContainsKey(searchTerm))
            {
                return;
            }

            m_searchResults[searchTerm] = new List<CGame>();

            // TEMP
            foreach(Platform.Platform platform in m_platforms.Values)
            {
                m_searchResults[searchTerm].AddRange(platform.AllGames.Where(g => g.Name.Contains(searchTerm)));
            }
        }

        public static bool LaunchGame(CGame game)
        {
            Platform.Platform platform = Platforms.Single(p => p.ID == game.PlatformFK);
            return platform.GameLaunch(game);
        }
    }

    #region PluginLoader

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

    #endregion PluginLoader

    public class CTestPlatform : Platform.Platform
    {
        public CTestPlatform(int id, string name, string description, string path, bool isEnabled)
        {
            ID = id;
            Name = name;
            Description = description;
            Path = path;
            IsEnabled = isEnabled;
        }

        public override bool GameLaunch(CGame game)
        {
            System.Diagnostics.Debug.WriteLine($"Launching game: {game.Name}");
            return true;
        }

        public override HashSet<CGame> GetInstalledGames()
        {
            return new HashSet<CGame>();
        }

        public override HashSet<CGame> GetNonInstalledGames()
        {
            return new HashSet<CGame>();
        }
    }

    public class CTestPlatformFactory : CPlatformFactory<Platform.Platform>
    {
        public override Platform.Platform CreateDefault()
        {
            return new CTestPlatform(-1, GetPlatformName(), "", "", true);
        }

        public override Platform.Platform CreateFromDatabase(int id, string name, string description, string path, bool isActive)
        {
            return new CTestPlatform(id, name, description, path, isActive);
        }

        public override string GetPlatformName()
        {
            return "Steam";
        }
    }
}