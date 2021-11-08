using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LibGLC.PlatformReaders
{
    /// <summary>
    /// Singleton designed base class for platform scanners
    /// https://www.codeproject.com/articles/572263/a-reusable-base-class-for-the-singleton-pattern-in
    /// </summary>
    /// <typeparam name="T">Class type of the platform scanner</typeparam>
    public abstract class CBasePlatformScanner<T> where T : class
    {
		// ===== Singleton code ===== //
		#region Singleton code

		/// <summary>
		/// Static instance. Needs to use lambda expression
		/// to construct an instance (since constructor is private).
		/// </summary>
		private static readonly Lazy<T> sInstance = new Lazy<T>(() => CreateInstanceOf());

        /// <summary>
        /// Return instance
        /// </summary>
        public static T Instance { get { return sInstance.Value; } }

        /// <summary>
        /// Creates an instance of T via reflection since T's constructor is expected to be private.
        /// </summary>
        /// <returns>Instance of the singleton</returns>
        private static T CreateInstanceOf()
        {
            return Activator.CreateInstance(typeof(T), true) as T;
        }
		#endregion Singleton code

		// ===== Shared constants ===== //
		#region Shared constants

		protected const string NODE64_REG              = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
		protected const string NODE32_REG              = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
		protected const string GAME_DISPLAY_NAME       = "DisplayName";
		protected const string GAME_DISPLAY_ICON       = "DisplayIcon";
		protected const string GAME_INSTALL_PATH       = "InstallPath";
		protected const string GAME_INSTALL_LOCATION   = "InstallLocation";
		protected const string GAME_UNINSTALL_STRING   = "UninstallString";
		protected const string INSTALLSHIELD           = "_is1";

		#endregion Shared constants

		// ===== Event code ===== //
		#region Event code

        public static event Action<CNewPlatformEventArgs>  PlatformStarted;
        public static event Action<CNewGameFoundEventArgs> GameFound;

        /// <summary>
        /// Raise new platform event to subscriber
        /// </summary>
        /// <param name="platformName">Name of the platform</param>
        protected void NewPlatformStarted(string platformName)
        {
            if(PlatformStarted != null)
            {
                PlatformStarted.Invoke(new CNewPlatformEventArgs(platformName));
            }
        }

        /// <summary>
        /// Rase new game found event to subscriber
        /// </summary>
        /// <param name="gameData">The game data structure</param>
        protected void NewGameFound(RawGameData gameData)
        {
            if(GameFound != null)
            {
                GameFound.Invoke(new CNewGameFoundEventArgs(gameData));
            }
        }
		#endregion Event code

		// ===== Internal registry helper class ===== //
		#region Internal registry helper class

		/// <summary>
		/// Helper class for dealing with the windows registry
		/// </summary>
		protected internal static class CRegHelper
        {
			/// <summary>
			/// Get a value from the registry if it exists
			/// </summary>
			/// <param name="key">The registry key</param>
			/// <param name="valName">The registry value name</param>
			/// <returns>the value's string data</returns>
			public static string GetRegStrVal(RegistryKey key, string valName)
			{
				try
				{
					object valData = key.GetValue(valName);
					if(valData != null)
                    {
						return valData.ToString();
					}
				}
				catch(Exception e)
				{
					CLogger.LogError(e);
				}
				return String.Empty;
			}

			/// <summary>
			/// Simplify a string for use as a default alias
			/// </summary>
			/// <param name="title">The game's title</param>
			/// <returns>simplified string</returns>
			public static string GetAlias(string title)
			{
				string alias = title.ToLower();
				/*
				foreach (string prep in new List<string> { "for", "of", "to" })
				{
					if (alias.StartsWith(prep + " "))
						alias = alias.Substring(prep.Length + 1);
				}
				*/
				foreach(string art in CGame.ARTICLES)
				{
					if(alias.StartsWith(art + " "))
                    {
						alias = alias.Substring(art.Length + 1);
					}
				}
				alias = new string(alias.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c) && !char.IsSymbol(c)).ToArray());
				return alias;
			}

			/// <summary>
			/// Find game folders in the registry.
			/// This method will look for and return folders that match the input string.
			/// </summary>
			/// <param name="root">Root directory that will be scanned</param>
			/// <param name="strFolder">Target game folder</param>
			/// <returns>List of Reg keys with game folders</returns>
			public static List<RegistryKey> FindGameFolders(RegistryKey root, string strFolder)
			{
				LinkedList<RegistryKey> toCheck = new LinkedList<RegistryKey>();
				List<RegistryKey> gameKeys = new List<RegistryKey>();

				toCheck.AddLast(root);

				while(toCheck.Count > 0)
				{
					root = toCheck.First.Value;
					toCheck.RemoveFirst();

					if(root != null)
					{
						foreach(var sub in root.GetSubKeyNames())
						{
							if(!(sub.Equals("Microsoft")))
							{
								if(string.IsNullOrEmpty(strFolder) || sub.IndexOf(strFolder, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
									gameKeys.Add(root.OpenSubKey(sub, RegistryKeyPermissionCheck.ReadSubTree));
								}
							}
						}
					}
				}
				return gameKeys;
			}

			/// <summary>
			/// Find game keys in specified root
			/// Looks for a key-value pair inside the specified root.
			/// </summary>
			/// <param name="root">Root folder that will be scanned</param>
			/// <param name="strKey">The target key that should contain the target value</param>
			/// <param name="strValue">The target value in the key</param>
			/// <param name="ignore">Function will ignore these folders (used to ignore things like launchers)</param>
			/// <returns>List of game registry keys</returns>
			public static List<RegistryKey> FindGameKeys(RegistryKey root, string strKey, string strValue, string[] ignore)
			{
				LinkedList<RegistryKey> toCheck = new LinkedList<RegistryKey>();
				List<RegistryKey> gameKeys = new List<RegistryKey>();

				toCheck.AddLast(root);

				while(toCheck.Count > 0)
				{
					root = toCheck.First.Value;
					toCheck.RemoveFirst();

					if(root != null)
					{
						foreach(var name in root.GetValueNames())
						{
							if(root.GetValueKind(name) == RegistryValueKind.String && name == strValue)
							{
								if(((string)root.GetValue(name)).Contains(strKey))
								{
									gameKeys.Add(root);
									break;
								}
							}
						}

						foreach(var sub in root.GetSubKeyNames()) // Add subkeys to search list
						{
							if(!(sub.Equals("Microsoft"))) // Microsoft folder only contains system stuff and it doesn't need searching
							{
								foreach(var entry in ignore)
								{
									if(!(sub.Equals(entry.ToString())))
									{
										try
										{
											toCheck.AddLast(root.OpenSubKey(sub, RegistryKeyPermissionCheck.ReadSubTree));
										}
										catch(Exception e)
										{
											CLogger.LogError(e);
										}
									}
								}
							}
						}
					}
				}
				return gameKeys;
			}

			/// <summary>
			/// Get a value from the registry if it exists
			/// </summary>
			/// <param name="key">The registry key</param>
			/// <param name="valName">The registry value name</param>
			/// <returns>the value's DWORD (UInt32) data</returns>
			public static int? GetRegDWORDVal(RegistryKey key, string valName)
			{
				try
				{
					object valData = key.GetValue(valName);
					/*
					Type valType = valData.GetType();
					if (valData != null && valType == typeof(int))
					{
					*/
					if(int.TryParse(valData.ToString(), out int result))
                    {
						return result;
					}
					//}
				}
				catch(Exception e)
				{
					CLogger.LogError(e);
				}
				return null;
			}
		}
		#endregion Internal registry helper class

		// ===== Internal json helper class ===== //
		#region Internal json helper class

		/// <summary>
		/// Helper class for dealing with JSON files
		/// </summary>
		protected internal static class CJsonHelper
        {
			/// <summary>
			/// Retrieve string value from the JSON element
			/// </summary>
			/// <param name="strPropertyName">Name of the property</param>
			/// <param name="jElement">Source JSON element</param>
			/// <returns>Value of the property as a string or empty string if not found</returns>
			public static string GetStringProperty(JsonElement jElement, string strPropertyName)
			{
				try
				{
					if(jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
                    {
						return jValue.GetString();
					}
				}
				catch(Exception e)
				{
					CLogger.LogError(e);
				}
				return "";
			}
		}
		#endregion Internal json helper class

		// ===== Internal directory helper class ===== //
		#region Internal directory helper class

		/// <summary>
		/// Helper class for dealing with windows directories
		/// </summary>
		protected internal static class CDirectoryHelper
        {
			private const string IMAGE_FOLDER_NAME = "CustomImages";
			private const string GAME_FOLDER_NAME = "CustomGames";
			private const string CUSTOM_PLATFORM  = "Custom";

			/// <summary>
			/// Find the game's binary executable file
			/// </summary>
			/// <param name="strPath">Path to search</param>
			/// <param name="strTitle">Title of the game</param>
			/// <returns></returns>
			public static string FindGameBinaryFile(string strPath, string strTitle)
			{
				/* function flow
				 * Find all exe files in the directory. (Exclude files with words: "unins", "redist")
				 * Compare exe files with gameTitle
				 * If failed, compare with name of root directory
				 * Return "" if failed
				 */

				// Get our list of exe files in the game folder + all subfolders
				if(!Directory.Exists(strPath))
                {
					return "";
				}

				List<string> exeFiles = Directory.EnumerateFiles(strPath, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".exe")).ToList();

				// Big Fish Games may use .bfg for executables
				//exeFiles.AddRange(Directory.EnumerateFiles(strPath, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".bfg")).ToList());

				// If only 1 file has been found, return it.
				if(exeFiles.Count == 1)
				{
					return exeFiles[0];
				}

				// First, compare the files against the game title
				{
					string[] words = strTitle.Split(new char[] { ' ', ':', '-', '_' });
					string letters = "";

					foreach(string word in words)
					{
						if(!string.IsNullOrEmpty(word))
                        {
							letters += word[0];
						}
					}

					foreach(string file in exeFiles)
					{
						//CLogger.LogInfo("*** {0}", file);
						if(file.ToLower().Contains("redist")) // A lot of steam games contain C++ redistributables. Ignore them
                        {
							continue;
						}
						if(file.ToLower().Contains("unins"))
                        {
							continue;
						}


						string description = FileVersionInfo.GetVersionInfo(file).FileDescription ?? "";
						description.ToLower();

						FileInfo info = new FileInfo(file);
						string name = info.Name.Substring(0, info.Name.IndexOf('.')).ToLower();

						// Perform a check against the acronym
						if(letters.Length > 2)
						{
							if(letters.ToLower().Contains(name) || name.Contains(letters.ToLower()))
                            {
								return file;
							}
							else if(!string.IsNullOrEmpty(description) 
								&& (letters.ToLower().Contains(description) || description.Contains(letters.ToLower())))
                            {
								return file;
							}
						}

						foreach(string word in words)
						{
							if(string.IsNullOrEmpty(word) || word.Length < 3)
                            {
								continue;
							}
							if(name.Contains(word.ToLower()))
                            {
								return file;
							}
							if(!string.IsNullOrEmpty(description) && description.Contains(word.ToLower()))
                            {
								return file;
							}
						}
					}
				}

				// If search failed, we need to compare the exe files against the name of root directory
				{
					string[] words = { "" };
					string letters = "";

					int pathLen = strPath.LastIndexOf('\\');
					if(pathLen < 0)
					{
						pathLen = strPath.LastIndexOf('/');
						if(pathLen < 0)
						{
							pathLen = 0;
						}
						else
                        {
							words = strPath.Substring(strPath.LastIndexOf('/')).Split(new char[] { ' ', '-', '_', ':' });
						}
					}
					else
                    {
						words = strPath.Substring(strPath.LastIndexOf('\\')).Split(new char[] { ' ', '-', '_', ':' });
					}

					foreach(string word in words)
					{
						if(!string.IsNullOrEmpty(word))
                        {
							letters += word[0];
						}
					}

					foreach(string file in exeFiles)
					{
						//CLogger.LogInfo("*** {0}", file);
						if(file.ToLower().Contains("redist")) // A lot of steam games contain C++ redistributables. Ignore them
                        {
							continue;
						}
						if(file.ToLower().Contains("unins"))
						{
							continue;
						}

						string description = FileVersionInfo.GetVersionInfo(file).FileDescription ?? "";
						description.ToLower();

						FileInfo info = new FileInfo(file);
						string name = info.Name.Substring(0, info.Name.IndexOf('.')).ToLower();

						// Perform a check against the acronym
						if(letters.Length > 2)
						{
							if(letters.ToLower().Contains(name) || name.Contains(letters.ToLower()))
                            {
								return file;
							}
							else if(!string.IsNullOrEmpty(description) 
								&& (letters.ToLower().Contains(description) || description.Contains(letters.ToLower())))
                            {
								return file;
							}
						}

						foreach(string word in words)
						{
							if(string.IsNullOrEmpty(word) || word.Length < 3)
                            {
								continue;
							}
							if(name.Contains(word.ToLower()))
                            {
								return file;
							}
							if(!string.IsNullOrEmpty(description) && description.Contains(word.ToLower()))
                            {
								return file;
							}
						}
					}
				}
				return "";
			}
			/*
			/// <summary>
			/// Find and import games from the binaries found in the "CustomGames" directory
			/// </summary>
			public static void ImportFromFolder(ref CGameData.CTempGameSet tempGameSet)
			{
				CheckCustomFolder();
				FindCustomLinkFiles(ref tempGameSet);
				FindCustomBinaries(ref tempGameSet);
			}
			*/
			/*
			/// <summary>
			/// Search the "CustomGames" folder for file shortcuts (.lnk) to import.
			/// </summary>
			/// http://www.saunalahti.fi/janij/blog/2006-12.html#d6d9c7ee-82f9-4781-8594-152efecddae2
			private static void FindCustomLinkFiles(ref CGameData.CTempGameSet tempGameSet)
			{
				List<string> fileList = Directory.EnumerateFiles(GAME_FOLDER_NAME, "*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".lnk")).ToList();

				foreach(string file in fileList)
				{
					string strPathOnly      = Path.GetDirectoryName(file);
					strPathOnly = Path.GetFullPath(strPathOnly);
					string strFilenameOnly  = Path.GetFileName(file);

					Shell32.Shell shell = new Shell32.Shell();
					Shell32.Folder folder           = shell.NameSpace(strPathOnly);
					Shell32.FolderItem folderItem   = folder.ParseName(strFilenameOnly);
					if(folderItem != null)
					{
						Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
						string strID            = Path.GetFileNameWithoutExtension(file);
						string strTitle         = strID;
						CLogger.LogDebug($"- {strTitle}");
						string strLaunch        = link.Path;
						string strUninstall     = "";  // N/A
						string strAlias         = CRegScanner.GetAlias(strTitle);
						if(strAlias.Equals(strTitle, CDock.IGNORE_CASE))
							strAlias = "";
						tempGameSet.InsertGame(strID, strTitle, strLaunch, strLaunch, strUninstall, true, false, true, false, strAlias, CUSTOM_PLATFORM, 0f);
					}
				}
			}
			*/
			/*
			/// <summary>
			/// Search the "CustomGames" folder for binaries (.exe) files to import
			/// </summary>
			private static void FindCustomBinaries(ref CGameData.CTempGameSet tempGameSet)
			{
				List<string> fileList = Directory.EnumerateFiles(GAME_FOLDER_NAME, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".exe")).ToList();

				// Big Fish Games may use .bfg for executables
				//fileList.AddRange(Directory.EnumerateFiles(GAME_FOLDER_NAME, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".bfg")).ToList());

				foreach(string file in fileList)
				{
					string strID            = Path.GetFileNameWithoutExtension(file);
					string strTitle         = strID;
					CLogger.LogDebug($"- {strTitle}");
					string strLaunch        = Path.GetFullPath(file);
					string strUninstall     = ""; // N/A
					string strAlias         = CRegScanner.GetAlias(strTitle);
					if(strAlias.Equals(strTitle, CDock.IGNORE_CASE))
						strAlias = "";
					tempGameSet.InsertGame(strID, strTitle, strLaunch, strLaunch, strUninstall, true, false, true, false, strAlias, CUSTOM_PLATFORM, 0f);
				}
			}
			*/
			/// <summary>
			/// Check if the folders "CustomImages" and "CustomGames" exist in the same directory as the application - create if not found
			/// </summary>
			public static void CheckCustomFolder()
			{
				if(!Directory.Exists(IMAGE_FOLDER_NAME))
				{
					Directory.CreateDirectory(IMAGE_FOLDER_NAME);
				}
				if(!Directory.Exists(GAME_FOLDER_NAME))
				{
					Directory.CreateDirectory(GAME_FOLDER_NAME);
				}
			}
		}
		#endregion Internal directory helper class

		// ===== Game scanner methods ===== //
		#region Game scanner methods

		/// <summary>
		/// Scan the platform for games and populate the list
		/// </summary>
		/// <param name="getNonInstalled">If true, try to get non-installed games</param>
		/// <param name="expensiveIcons">If true, try to get expensive icons</param>
		/// <param name="gameList">Reference to the game list, where new games should be added</param>
		/// <returns>True if at least one game was found, otherwise false</returns>
		public virtual bool GetGames(bool getNonInstalled, bool expensiveIcons)
        {
			bool success = GetInstalledGames(expensiveIcons);
			if(getNonInstalled)
            {
				success = success && GetNonInstalledGames(expensiveIcons);
            }
			return success;
        }

        /// <summary>
        /// Scan platform for installed games
        /// </summary>
        /// <param name="gameList">Reference to the list that will hold the game data</param>
        /// <returns>True if at least one game was found, otherwise false</returns>
        protected abstract bool GetInstalledGames(bool expensiveIcons);

        /// <summary>
        /// Scan platform for non-installed games
        /// </summary>
        /// <param name="gameList">Reference to the list that will hold the game data</param>
        /// <returns>True if at least one game was found, otherwise false</returns>
        protected abstract bool GetNonInstalledGames(bool expensiveIcons);
		#endregion Game scanner methods
	}
}
