using BasePlatformExtension;
using core;
using core.DataAccess;
using HtmlAgilityPack;
using Logger;
using Microsoft.Win32;
using System;
using System.Text.Json;
using System.Xml;

namespace Steam
{
    /// <summary>
    /// Steam scanner implementation
    /// </summary>
    public sealed class CSteamScanner : CBasePlatformScanner
    {
        private const string STEAM_PLATFORM      = "Steam";
        private const string PROTOCOL            = "steam://";
        private const string LAUNCH              = PROTOCOL + "open/games";
        private const string INSTALL_GAME        = PROTOCOL + "install/";
        private const string START_GAME          = PROTOCOL + "rungameid/";
        private const string UNINST_GAME         = PROTOCOL + "uninstall/";
        private const int STEAM_MAX_LIBS        = 64;
        private const string STEAM_GAME_PREFIX  = "Steam App ";
        private const string STEAM_PATH         = "steamapps";
        private const string STEAM_LIBFILE      = "libraryfolders.vdf";
        private const string STEAM_APPFILE      = "SteamAppData.vdf";
        private const string STEAM_USRFILE      = "loginusers.vdf";
        private const string STEAM_LIBARR       = "LibraryFolders";
        private const string STEAM_REG          = @"SOFTWARE\WOW6432Node\Valve\Steam"; // HKLM32
        //private const string STEAM_UNREG		= "Steam"; // HKLM32 Uninstall

        public CSteamScanner(int platformID)
            : base(platformID)
        {
        }


        public override HashSet<Game> GetInstalledGames(bool expensiveIcons)
        {
            HashSet<Game> games = new HashSet<Game>();

            string strInstallPath = "";
            string strClientPath = "";

            using(RegistryKey key = Registry.LocalMachine.OpenSubKey(STEAM_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
            {
                if(key == null)
                {
                    CLogger.LogInfo("{0} client not found in the registry.", STEAM_PLATFORM.ToUpper());
                    return new HashSet<Game>();
                }

                strInstallPath = CRegistryHelper.GetRegStrVal(key, GAME_INSTALL_PATH);
                strClientPath = Path.Combine(strInstallPath, STEAM_PATH);
            }

            if(!Directory.Exists(strClientPath))
            {
                CLogger.LogInfo("{0} library not found: {1}", STEAM_PLATFORM.ToUpper(), strClientPath);
                return new HashSet<Game>();
            }

            string libFile = Path.Combine(strClientPath, STEAM_LIBFILE);
            List<string> libs = new()
            {
                strClientPath
            };
            int nLibs = 1;

            try
            {
                if(File.Exists(libFile))
                {
                    SteamWrapper document = new(libFile);
                    ACF_Struct documentData = document.ACFFileToStruct();
                    ACF_Struct folders = new();
                    if(documentData.SubACF.ContainsKey(STEAM_LIBARR))
                        folders = documentData.SubACF[STEAM_LIBARR];
                    else if(documentData.SubACF.ContainsKey(STEAM_LIBARR.ToLower()))
                        folders = documentData.SubACF[STEAM_LIBARR.ToLower()];
                    for(; nLibs <= STEAM_MAX_LIBS; ++nLibs)
                    {
                        folders.SubItems.TryGetValue(nLibs.ToString(), out string library);
                        if(string.IsNullOrEmpty(library))
                        {
                            if(folders.SubACF.ContainsKey(nLibs.ToString()))
                                folders.SubACF[nLibs.ToString()].SubItems.TryGetValue("path", out library);
                            if(string.IsNullOrEmpty(library))
                            {
                                nLibs--;
                                break;
                            }
                        }
                        library = Path.Combine(library, STEAM_PATH);
                        if(!library.Equals(strClientPath) && Directory.Exists(library))
                            libs.Add(library);
                    }
                }
            }
            catch(Exception e)
            {
                CLogger.LogError(e, string.Format("Malformed {0} file: {1}", STEAM_PLATFORM.ToUpper(), libFile));
                nLibs--;
            }

            int i = 0;
            List<string> allFiles = new();
            foreach(string lib in libs)
            {
                List<string> libFiles = new();
                try
                {
                    libFiles = Directory.GetFiles(lib, "appmanifest_*.acf", SearchOption.TopDirectoryOnly).ToList();
                    allFiles.AddRange(libFiles);
                    CLogger.LogInfo("{0} {1} games found in library {2}", libFiles.Count, STEAM_PLATFORM.ToUpper(), lib);
                }
                catch(Exception e)
                {
                    CLogger.LogError(e, string.Format("{0} directory read error: {1}", STEAM_PLATFORM.ToUpper(), lib));
                    continue;
                }

                foreach(string file in libFiles)
                {
                    try
                    {
                        SteamWrapper document = new(file);
                        ACF_Struct documentData = document.ACFFileToStruct();
                        ACF_Struct app = documentData.SubACF["AppState"];

                        string id = app.SubItems["appid"];
                        if(id.Equals("228980"))  // Steamworks Common Redistributables
                            continue;

                        string strID = Path.GetFileName(file);
                        string strTitle = app.SubItems["name"];
                        CLogger.LogDebug($"- {strTitle}");
                        string strLaunch = START_GAME + id;
                        string strIconPath = "";
                        string strUninstall = "";
                        string strAlias = "";

                        strAlias = CRegistryHelper.GetAlias(strTitle);
                        if(!string.IsNullOrEmpty(strLaunch))
                        {
                            using(RegistryKey key = Registry.LocalMachine.OpenSubKey(Path.Combine(NODE64_REG, STEAM_GAME_PREFIX + id), RegistryKeyPermissionCheck.ReadSubTree),  // HKLM64
                                               key2 = Registry.LocalMachine.OpenSubKey(Path.Combine(NODE32_REG, STEAM_GAME_PREFIX + id), RegistryKeyPermissionCheck.ReadSubTree))  // HKLM32
                            {
                                if(key != null)
                                    strIconPath = CRegistryHelper.GetRegStrVal(key, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
                                else if(key2 != null)
                                    strIconPath = CRegistryHelper.GetRegStrVal(key2, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
                            }
                            if(string.IsNullOrEmpty(strIconPath) && expensiveIcons)
                            {
                                bool success = false;

                                // Search for an .exe to use as icon
                                strIconPath = CDirectoryHelper.FindGameBinaryFile(Path.Combine(lib, "common", app.SubItems["installdir"]), strTitle);
                                if(!string.IsNullOrEmpty(strIconPath))
                                {
                                    success = true;
                                    strAlias = CRegistryHelper.GetAlias(Path.GetFileNameWithoutExtension(strIconPath));
                                    if(strAlias.Length > strTitle.Length)
                                        strAlias = CRegistryHelper.GetAlias(strTitle);
                                }

                                /*
                                if(!success && !(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
                                {
                                    // Download missing icons
                                    string iconUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{id}/capsule_184x69.jpg";
                                    if(CDock.DownloadCustomImage(strTitle, iconUrl))
                                        success = true;
                                }
                                */
                            }
                            if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
                                strAlias = "";
                            strUninstall = UNINST_GAME + id;
                            //games.Add(new ImportGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
                            games.Add(Game.FromScanner(0, strID, strTitle, strLaunch, strAlias, "Installed"));
                        }
                    }
                    catch(Exception e)
                    {
                        CLogger.LogError(e, string.Format("Malformed {0} file: {1}", STEAM_PLATFORM.ToUpper(), file));
                    }
                }
                i++;
                /*
				if (i > nLibs)
					CLogger.LogDebug("---------------------");
				*/
            }

            return games;
        }

        /// <summary>
        /// Generate 10 non-installed game objects
        /// </summary>
        /// <param name="expensiveIcons">TODO: unused</param>
        /// <returns>HashSet with 10 generated game objects</returns>
        public override HashSet<Game> GetNonInstalledGames(bool expensiveIcons)
        {
            HashSet<Game> games = new HashSet<Game>();

            // First get Steam user ID
            ulong userId = 0; // (ulong)CConfig.GetConfigULong(CConfig.CFG_STEAMID);

            string strInstallPath = "";
            string strClientPath = "";

            using(RegistryKey key = Registry.LocalMachine.OpenSubKey(STEAM_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
            {
                if(key == null)
                {
                    CLogger.LogInfo("{0} client not found in the registry.", STEAM_PLATFORM.ToUpper());
                    return new HashSet<Game>();
                }

                strInstallPath = CRegistryHelper.GetRegStrVal(key, GAME_INSTALL_PATH);
                strClientPath = Path.Combine(strInstallPath, STEAM_PATH);
            }

            if(!Directory.Exists(strClientPath))
            {
                CLogger.LogInfo("{0} library not found: {1}", STEAM_PLATFORM.ToUpper(), strClientPath);
                return new HashSet<Game>();
            }

            string libFile = Path.Combine(strClientPath, STEAM_LIBFILE);
            List<string> libs = new()
            {
                strClientPath
            };
            int nLibs = 1;

            try
            {
                if(File.Exists(libFile))
                {
                    SteamWrapper document = new(libFile);
                    ACF_Struct documentData = document.ACFFileToStruct();
                    ACF_Struct folders = new();
                    if(documentData.SubACF.ContainsKey(STEAM_LIBARR))
                        folders = documentData.SubACF[STEAM_LIBARR];
                    else if(documentData.SubACF.ContainsKey(STEAM_LIBARR.ToLower()))
                        folders = documentData.SubACF[STEAM_LIBARR.ToLower()];
                    for(; nLibs <= STEAM_MAX_LIBS; ++nLibs)
                    {
                        folders.SubItems.TryGetValue(nLibs.ToString(), out string library);
                        if(string.IsNullOrEmpty(library))
                        {
                            if(folders.SubACF.ContainsKey(nLibs.ToString()))
                                folders.SubACF[nLibs.ToString()].SubItems.TryGetValue("path", out library);
                            if(string.IsNullOrEmpty(library))
                            {
                                nLibs--;
                                break;
                            }
                        }
                        library = Path.Combine(library, STEAM_PATH);
                        if(!library.Equals(strClientPath) && Directory.Exists(library))
                            libs.Add(library);
                    }
                }
            }
            catch(Exception e)
            {
                CLogger.LogError(e, string.Format("Malformed {0} file: {1}", STEAM_PLATFORM.ToUpper(), libFile));
                nLibs--;
            }

            int i = 0;
            List<string> allFiles = new();
            foreach(string lib in libs)
            {
                List<string> libFiles = new();
                try
                {
                    libFiles = Directory.GetFiles(lib, "appmanifest_*.acf", SearchOption.TopDirectoryOnly).ToList();
                    allFiles.AddRange(libFiles);
                    CLogger.LogInfo("{0} {1} games found in library {2}", libFiles.Count, STEAM_PLATFORM.ToUpper(), lib);
                }
                catch(Exception e)
                {
                    CLogger.LogError(e, string.Format("{0} directory read error: {1}", STEAM_PLATFORM.ToUpper(), lib));
                    continue;
                }
            }

            if(userId < 1)
            {
                try
                {
                    ulong userIdTmp = 0;
                    string userName = "";
                    string userNameTmp = "";
                    string strConfigPath = Path.Combine(strInstallPath, "config");
                    string appFile = Path.Combine(strConfigPath, STEAM_APPFILE);

                    if(File.Exists(appFile))
                    {
                        SteamWrapper appDoc = new(appFile);
                        ACF_Struct appDocData = appDoc.ACFFileToStruct();
                        ACF_Struct appData = appDocData.SubACF["SteamAppData"];

                        appData.SubItems.TryGetValue("AutoLoginUser", out userName);

                        SteamWrapper usrDoc = new(Path.Combine(strConfigPath, STEAM_USRFILE));
                        ACF_Struct usrDocData = usrDoc.ACFFileToStruct();
                        ACF_Struct usrData = usrDocData.SubACF["users"];

                        foreach(KeyValuePair<string, ACF_Struct> user in usrData.SubACF)
                        {
                            if(!ulong.TryParse(user.Key, out userIdTmp))
                                userIdTmp = 0;

                            foreach(KeyValuePair<string, string> userVal in user.Value.SubItems)
                            {
                                if(userVal.Key.Equals("AccountName"))
                                {
                                    userNameTmp = userVal.Value;
                                    if(userNameTmp.Equals(userName))
                                    {
                                        if(!ulong.TryParse(user.Key, out userId))
                                            userId = 0;
                                    }
                                }
                                if(userVal.Key.Equals("MostRecent") && userVal.Value.Equals("1") && string.IsNullOrEmpty(userName))
                                {
                                    userId = userIdTmp;
                                    userName = userNameTmp;
                                    break;
                                }
                            }
                        }
                        if(userId < 1)
                        {
                            userId = userIdTmp;
                            userName = userNameTmp;
                        }
                    }
                    if(userId > 0)
                    {
                        CLogger.LogInfo("Setting default {0} user to {1} #{2}", STEAM_PLATFORM.ToUpper(), userName, userId);
                        //CConfig.SetConfigValue(CConfig.CFG_STEAMID, userId);
                        //ExportConfig();
                    }
                }
                catch(Exception e)
                {
                    CLogger.LogError(e, string.Format("Malformed {0} file: {1} or {2}", STEAM_PLATFORM.ToUpper(), STEAM_APPFILE, STEAM_USRFILE));
                }
            }

            if(userId > 0)
            {
                // Download game list from public user profile
                try
                {
                    string url = string.Format("https://steamcommunity.com/profiles/{0}/games/?tab=all", userId);
                    /*
#if DEBUG
                    // Don't re-download if file exists
                    string tmpfile = $"tmp_{STEAM_PLATFORM}.html";
                    if (!File.Exists(tmpfile))
                    {
                        using WebClient client = new();
                        client.DownloadFile(url, tmpfile);
                    }
                    HtmlDocument doc = new()
                    {
                        OptionUseIdAttribute = true
                    };
                    doc.Load(tmpfile);
#else
                    */
                    HtmlWeb web = new()
                    {
                        UseCookies = true
                    };
                    HtmlDocument doc = web.Load(url);
                    doc.OptionUseIdAttribute = true;
                    //#endif
                    HtmlNode gameList = doc.DocumentNode.SelectSingleNode("//script[@language='javascript']");
                    if(gameList == null)
                    {
                        CLogger.LogInfo("Can't get not-installed {0} games. Profile may not be public.\n" +
                                        "To change this, go to <https://steamcommunity.com/my/edit/settings>.",
                            STEAM_PLATFORM.ToUpper());
                    }
                    else
                    {
                        CLogger.LogDebug("{0} not-installed games (user #{1}):", STEAM_PLATFORM.ToUpper(), userId);

                        var options = new JsonDocumentOptions
                        {
                            AllowTrailingCommas = true
                        };
                        string rgGames = gameList.InnerText.Remove(0, gameList.InnerText.IndexOf('['));
                        rgGames = rgGames.Remove(rgGames.IndexOf(';'));
#if DEBUG
                        File.WriteAllText($"tmp_{STEAM_PLATFORM}.json", rgGames);
#endif

                        using JsonDocument document = JsonDocument.Parse(@rgGames, options);
                        foreach(JsonElement game in document.RootElement.EnumerateArray())
                        {
                            ulong id = CJsonHelper.GetULongProperty(game, "appid");
                            if(id > 0)
                            {
                                // Check if game is already installed
                                string strID = $"appmanifest_{id}.acf";
                                bool found = false;
                                foreach(string file in allFiles)
                                {
                                    if(file.EndsWith(strID))
                                        found = true;
                                }
                                if(!found)
                                {
                                    string strTitle = CJsonHelper.GetStringProperty(game, "name");

                                    // Add not-installed games
                                    CLogger.LogDebug($"- *{strTitle}");
                                    //gameDataList.Add(new ImportGameData(strID, strTitle, "", "", "", "", false, STEAM_PLATFORM));
                                    games.Add(Game.FromScanner(0, strID, strTitle, "", "", "Not installed"));

                                    // Use logo to download not-installed icons
                                    /*
                                    if(!(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
                                    {
                                        string iconUrl = GetStringProperty(game, "logo");
                                        CDock.DownloadCustomImage(strTitle, iconUrl);
                                    }
                                    */
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    CLogger.LogError(e);
                }

                CLogger.LogDebug("---------------------");
            }

            return games;
        }
    }
}
