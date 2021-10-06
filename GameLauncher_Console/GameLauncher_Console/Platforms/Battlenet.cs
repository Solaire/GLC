using Logger;
using Microsoft.Win32;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
//using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
    // Battle.net (Blizzard)
    // [installed games only]
    public class PlatformBattlenet : IPlatform
    {
        public const GamePlatform ENUM          = GamePlatform.Battlenet;
        public const string PROTOCOL            = "battlenet://";   // "blizzard://" works too [TODO: is one more compatible with older versions?]
        //private const string BATTLE_NET		= "Battle.net";
        //private const string BATTLE_NET_UNREG   = "Battle.net";       // HKLM32 Uninstall
        //private const string BATTLE_NET_REG	= @"SOFTWARE\WOW6432Node\Blizzard Entertainment\Battle.net"; // HKLM32
        private const string BATTLE_NET_CFG     = @"Battle.net\Battle.net.config";
        private const string BATTLE_NET_DB      = @"Battle.net\Agent\product.db";
        private const string BATTLE_NET_DATA    = @"Battle.net\Agent\data\cache";
        private const string BATTLE_NET_UNINST  = @"Battle.net\Agent\Blizzard Uninstaller.exe";

        private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

        GamePlatform IPlatform.Enum => ENUM;

        string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

        public static void Launch() => Process.Start(PROTOCOL);

        public static void InstallGame(CGame game) => throw new NotImplementedException();

        [SupportedOSPlatform("windows")]
        public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
        {
            string cfgFile = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), BATTLE_NET_CFG);
            string dbFile = Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), BATTLE_NET_DB);
            string dataPath = Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), BATTLE_NET_DATA);
            string uninstallExe = Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), BATTLE_NET_UNINST);

            if (!File.Exists(dbFile))
            {
                CLogger.LogInfo("{0} installed game database not found.", _name.ToUpper());
                return;
            }

            // grab config file for last run dates
            string strConfigData = File.ReadAllText(cfgFile);

            List<string> dataFiles = new();
            try
            {
                dataFiles = Directory.GetFiles(dataPath, "*.", SearchOption.AllDirectories).ToList();
                //CLogger.LogInfo("{0} {1} games found", dataFiles.Count > 3 ? dataFiles.Count - 3 : 0, _name.ToUpper());
            }
            catch (Exception e)
            {
                CLogger.LogError(e, string.Format("{0} directory read error: {1}", _name.ToUpper(), dataPath));
            }

            foreach (string dataFile in dataFiles)
            {
                string strDocumentData = File.ReadAllText(dataFile);

                if (!string.IsNullOrEmpty(strDocumentData))
                {
                    using JsonDocument document = JsonDocument.Parse(@strDocumentData, jsonTrailingCommas);
                    string all = GetStringProperty(document.RootElement, "all");
                    if (document.RootElement.TryGetProperty("all", out JsonElement allLocales) &&
                        allLocales.TryGetProperty("config", out JsonElement allConfig))
                    {
                        string code = GetStringProperty(allConfig, "product");
                        if (code.Equals("agent") || code.Equals("Bna")) // hide Agent and Battle.net
                            continue;

                        try
                        {
                            using var file = File.OpenRead(dbFile);
                            BnetDatabase db = Serializer.Deserialize<BnetDatabase>(file);
                            foreach (BnetProductInstall pi in db.productInstalls)
                            {
                                if (pi.productCode.Equals(code, CDock.IGNORE_CASE))
                                {
                                    string installPath = pi.Settings.installPath;
                                    if (!string.IsNullOrEmpty(installPath))
                                    {
                                        string lang = pi.Settings.selectedTextLanguage;
                                        if (string.IsNullOrEmpty(lang))
                                            lang = "enUS";
                                        /*
                                        string timestamp = "";
                                        foreach (BnetProductConfig pc in db.productConfigs)
                                        {
                                            if (code.Equals(pc.productCode, CDock.IGNORE_CASE))
                                            {
                                                timestamp = pc.Timestamp;
                                                break;
                                            }
                                        }
                                        */
                                        
                                        if (allConfig.TryGetProperty("shared_container_default_subfolder", out JsonElement sub))
                                        {
                                            installPath = Path.Combine(installPath, sub.GetString());
                                        }

                                        if (allConfig.TryGetProperty("supported_locales", out JsonElement locales))
                                        {
                                            bool found = false;
                                            foreach (JsonElement locale in locales.EnumerateArray())
                                            {
                                                if (locale.GetString().Equals("lang", CDock.IGNORE_CASE))
                                                    found = true;
                                            }
                                            if (!found) lang = "enus";
                                        }

                                        string strID = $"battlenet_{code}";
                                        string strTitle = code;

                                        if (document.RootElement.TryGetProperty(lang.ToLower(), out JsonElement selectedLocale) &&
                                            selectedLocale.TryGetProperty("config", out JsonElement localeConfig) &&
                                            localeConfig.TryGetProperty("install", out JsonElement install))
                                        {
                                            foreach (JsonElement item in install.EnumerateArray())
                                            {
                                                if (item.TryGetProperty("add_remove_programs_key", out JsonElement uninstall) &&
                                                    uninstall.TryGetProperty("display_name", out JsonElement name))
                                                    strTitle = name.GetString();
                                            }
                                        }
                                        CLogger.LogDebug($"- {strTitle}");
                                        if (document.RootElement.TryGetProperty("platform", out JsonElement platform) &&
                                            platform.TryGetProperty("win", out JsonElement windows))
                                        {
                                            if (windows.TryGetProperty("config", out JsonElement config) &&
                                                config.TryGetProperty("binaries", out JsonElement bins) &&
                                                bins.TryGetProperty("game", out JsonElement game))
                                            {
                                                string strLaunch = "";
                                                JsonElement exePath = new();
                                                if (game.TryGetProperty("relative_path_64", out exePath) || game.TryGetProperty("relative_path", out exePath))
                                                    strLaunch = Path.Combine(installPath, exePath.GetString());
                                                if (string.IsNullOrEmpty(strLaunch))
                                                    break;
                                                string strUninstall = $"\"{uninstallExe}\" --lang={lang} --uid={code} --displayname=\"{strTitle}\"";
                                                string strAlias = "";
                                                string strPlatform = GetPlatformString(GamePlatform.Battlenet);
                                                long lLastRun = 0;
                                                if (!string.IsNullOrEmpty(strConfigData))
                                                {
                                                    try
                                                    {
                                                        using JsonDocument document2 = JsonDocument.Parse(@strConfigData, jsonTrailingCommas);
                                                        if (document2.RootElement.TryGetProperty("Games", out JsonElement games) && 
                                                            games.TryGetProperty(code, out JsonElement cfgGame))
                                                        {
                                                            string strLastRun = "";
                                                            strLastRun = GetStringProperty(cfgGame, "LastPlayed");
                                                            //if (string.IsNullOrEmpty(strLastRun))
                                                            //    strLastRun = GetStringProperty(cfgGame, "LastActioned");
                                                            _ = long.TryParse(strLastRun, out lLastRun);
                                                            //CLogger.LogDebug("    LastPlayed: " + strLastRun + " -> " + DateTimeOffset.FromUnixTimeSeconds(lLastRun).UtcDateTime.ToShortDateString());
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        CLogger.LogError(e, string.Format("Malformed {0} configuration file: {1}", _name.ToUpper(), cfgFile));
                                                    }
                                                }
                                                DateTime lastRun = DateTimeOffset.FromUnixTimeSeconds(lLastRun).UtcDateTime;
                                                strAlias = GetAlias(strTitle);
                                                if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
                                                    strAlias = "";
                                                if (!(string.IsNullOrEmpty(strLaunch)))
                                                    gameDataList.Add(
                                                        new ImportGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform, dateLastRun: lastRun));
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            CLogger.LogError(e, string.Format("Malformed {0} data file: {1}", _name.ToUpper(), dbFile));
                        }
                    }
                }
            }
            
            /*
			List<RegistryKey> keyList;

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("HKLM32 Uninstall not found in the registry.", _name.ToUpper());
					return;
				}

				//keyList = FindGameKeys(key, BATTLE_NET, GAME_UNINSTALL_STRING, new string[] { BATTLE_NET_UNREG });
				keyList = FindGameKeys(key, Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), BATTLE_NET_UNINST), GAME_UNINSTALL_STRING, new string[] { BATTLE_NET_UNREG });

				CLogger.LogInfo("{0} {1} games found", keyList.Count, _name.ToUpper());
				foreach (var data in keyList)
				{
					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					//string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = GetPlatformString(GamePlatform.Battlenet);
					try
					{
						strID = Path.GetFileName(data.Name);
						strTitle = GetRegStrVal(data, GAME_DISPLAY_NAME);
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						strUninstall = GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
						strAlias = GetAlias(Path.GetFileNameWithoutExtension(GetRegStrVal(data, GAME_INSTALL_LOCATION).Trim(new char[] { ' ', '\'', '"' })));
						if (strAlias.Length > strTitle.Length)
							strAlias = GetAlias(strTitle);
						if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
							strAlias = "";
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
					if (!(string.IsNullOrEmpty(strLaunch)))
						gameDataList.Add(
							new ImportGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
				}
			}
            */
            CLogger.LogDebug("--------------------------");
        }
    }

// The below is used for reading the Battle.net ProtoBuf database, duplicated from:
// https://github.com/dafzor/bnetlauncher/blob/master/bnetlauncher/Utils/ProductDb.cs
// Copyright (C) 2016-2019 madalien.com

#pragma warning disable CS1591, CS0612, CS3021, IDE1006, CA1033
    [global::ProtoBuf.ProtoContract()]
    public partial class LanguageSetting : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"language")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Language
        {
            get { return __pbn__Language ?? ""; }
            set { __pbn__Language = value; }
        }
        public bool ShouldSerializeLanguage() => __pbn__Language != null;
        public void ResetLanguage() => __pbn__Language = null;
        private string __pbn__Language;

        [global::ProtoBuf.ProtoMember(2, Name = @"option")]
        [global::System.ComponentModel.DefaultValue(LanguageOption.LangoptionNone)]
        public LanguageOption Option
        {
            get { return __pbn__Option ?? LanguageOption.LangoptionNone; }
            set { __pbn__Option = value; }
        }
        public bool ShouldSerializeOption() => __pbn__Option != null;
        public void ResetOption() => __pbn__Option = null;
        private LanguageOption? __pbn__Option;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class UserSettings : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string installPath
        {
            get { return __pbn__installPath ?? ""; }
            set { __pbn__installPath = value; }
        }
        public bool ShouldSerializeinstallPath() => __pbn__installPath != null;
        public void ResetinstallPath() => __pbn__installPath = null;
        private string __pbn__installPath;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue("")]
        public string playRegion
        {
            get { return __pbn__playRegion ?? ""; }
            set { __pbn__playRegion = value; }
        }
        public bool ShouldSerializeplayRegion() => __pbn__playRegion != null;
        public void ResetplayRegion() => __pbn__playRegion = null;
        private string __pbn__playRegion;

        [global::ProtoBuf.ProtoMember(3)]
        [global::System.ComponentModel.DefaultValue(ShortcutOption.ShortcutNone)]
        public ShortcutOption desktopShortcut
        {
            get { return __pbn__desktopShortcut ?? ShortcutOption.ShortcutNone; }
            set { __pbn__desktopShortcut = value; }
        }
        public bool ShouldSerializedesktopShortcut() => __pbn__desktopShortcut != null;
        public void ResetdesktopShortcut() => __pbn__desktopShortcut = null;
        private ShortcutOption? __pbn__desktopShortcut;

        [global::ProtoBuf.ProtoMember(4)]
        [global::System.ComponentModel.DefaultValue(ShortcutOption.ShortcutNone)]
        public ShortcutOption startmenuShortcut
        {
            get { return __pbn__startmenuShortcut ?? ShortcutOption.ShortcutNone; }
            set { __pbn__startmenuShortcut = value; }
        }
        public bool ShouldSerializestartmenuShortcut() => __pbn__startmenuShortcut != null;
        public void ResetstartmenuShortcut() => __pbn__startmenuShortcut = null;
        private ShortcutOption? __pbn__startmenuShortcut;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue(LanguageSettingType.LangsettingNone)]
        public LanguageSettingType languageSettings
        {
            get { return __pbn__languageSettings ?? LanguageSettingType.LangsettingNone; }
            set { __pbn__languageSettings = value; }
        }
        public bool ShouldSerializelanguageSettings() => __pbn__languageSettings != null;
        public void ResetlanguageSettings() => __pbn__languageSettings = null;
        private LanguageSettingType? __pbn__languageSettings;

        [global::ProtoBuf.ProtoMember(6)]
        [global::System.ComponentModel.DefaultValue("")]
        public string selectedTextLanguage
        {
            get { return __pbn__selectedTextLanguage ?? ""; }
            set { __pbn__selectedTextLanguage = value; }
        }
        public bool ShouldSerializeselectedTextLanguage() => __pbn__selectedTextLanguage != null;
        public void ResetselectedTextLanguage() => __pbn__selectedTextLanguage = null;
        private string __pbn__selectedTextLanguage;

        [global::ProtoBuf.ProtoMember(7)]
        [global::System.ComponentModel.DefaultValue("")]
        public string selectedSpeechLanguage
        {
            get { return __pbn__selectedSpeechLanguage ?? ""; }
            set { __pbn__selectedSpeechLanguage = value; }
        }
        public bool ShouldSerializeselectedSpeechLanguage() => __pbn__selectedSpeechLanguage != null;
        public void ResetselectedSpeechLanguage() => __pbn__selectedSpeechLanguage = null;
        private string __pbn__selectedSpeechLanguage;

        [global::ProtoBuf.ProtoMember(8, Name = @"languages")]
        public global::System.Collections.Generic.List<LanguageSetting> Languages { get; } = new global::System.Collections.Generic.List<LanguageSetting>();

        [global::ProtoBuf.ProtoMember(9, Name = @"gfx_override_tags")]
        [global::System.ComponentModel.DefaultValue("")]
        public string GfxOverrideTags
        {
            get { return __pbn__GfxOverrideTags ?? ""; }
            set { __pbn__GfxOverrideTags = value; }
        }
        public bool ShouldSerializeGfxOverrideTags() => __pbn__GfxOverrideTags != null;
        public void ResetGfxOverrideTags() => __pbn__GfxOverrideTags = null;
        private string __pbn__GfxOverrideTags;

        [global::ProtoBuf.ProtoMember(10, Name = @"versionbranch")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Versionbranch
        {
            get { return __pbn__Versionbranch ?? ""; }
            set { __pbn__Versionbranch = value; }
        }
        public bool ShouldSerializeVersionbranch() => __pbn__Versionbranch != null;
        public void ResetVersionbranch() => __pbn__Versionbranch = null;
        private string __pbn__Versionbranch;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BnetInstallHandshake : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"product")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Product
        {
            get { return __pbn__Product ?? ""; }
            set { __pbn__Product = value; }
        }
        public bool ShouldSerializeProduct() => __pbn__Product != null;
        public void ResetProduct() => __pbn__Product = null;
        private string __pbn__Product;

        [global::ProtoBuf.ProtoMember(2, Name = @"uid")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Uid
        {
            get { return __pbn__Uid ?? ""; }
            set { __pbn__Uid = value; }
        }
        public bool ShouldSerializeUid() => __pbn__Uid != null;
        public void ResetUid() => __pbn__Uid = null;
        private string __pbn__Uid;

        [global::ProtoBuf.ProtoMember(3, Name = @"settings")]
        public UserSettings Settings { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BuildConfig : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"region")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Region
        {
            get { return __pbn__Region ?? ""; }
            set { __pbn__Region = value; }
        }
        public bool ShouldSerializeRegion() => __pbn__Region != null;
        public void ResetRegion() => __pbn__Region = null;
        private string __pbn__Region;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue("")]
        public string buildConfig
        {
            get { return __pbn__buildConfig ?? ""; }
            set { __pbn__buildConfig = value; }
        }
        public bool ShouldSerializebuildConfig() => __pbn__buildConfig != null;
        public void ResetbuildConfig() => __pbn__buildConfig = null;
        private string __pbn__buildConfig;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BaseProductState : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"installed")]
        public bool Installed
        {
            get { return __pbn__Installed.GetValueOrDefault(); }
            set { __pbn__Installed = value; }
        }
        public bool ShouldSerializeInstalled() => __pbn__Installed != null;
        public void ResetInstalled() => __pbn__Installed = null;
        private bool? __pbn__Installed;

        [global::ProtoBuf.ProtoMember(2, Name = @"playable")]
        public bool Playable
        {
            get { return __pbn__Playable.GetValueOrDefault(); }
            set { __pbn__Playable = value; }
        }
        public bool ShouldSerializePlayable() => __pbn__Playable != null;
        public void ResetPlayable() => __pbn__Playable = null;
        private bool? __pbn__Playable;

        [global::ProtoBuf.ProtoMember(3)]
        public bool updateComplete
        {
            get { return __pbn__updateComplete.GetValueOrDefault(); }
            set { __pbn__updateComplete = value; }
        }
        public bool ShouldSerializeupdateComplete() => __pbn__updateComplete != null;
        public void ResetupdateComplete() => __pbn__updateComplete = null;
        private bool? __pbn__updateComplete;

        [global::ProtoBuf.ProtoMember(4)]
        public bool backgroundDownloadAvailable
        {
            get { return __pbn__backgroundDownloadAvailable.GetValueOrDefault(); }
            set { __pbn__backgroundDownloadAvailable = value; }
        }
        public bool ShouldSerializebackgroundDownloadAvailable() => __pbn__backgroundDownloadAvailable != null;
        public void ResetbackgroundDownloadAvailable() => __pbn__backgroundDownloadAvailable = null;
        private bool? __pbn__backgroundDownloadAvailable;

        [global::ProtoBuf.ProtoMember(5)]
        public bool backgroundDownloadComplete
        {
            get { return __pbn__backgroundDownloadComplete.GetValueOrDefault(); }
            set { __pbn__backgroundDownloadComplete = value; }
        }
        public bool ShouldSerializebackgroundDownloadComplete() => __pbn__backgroundDownloadComplete != null;
        public void ResetbackgroundDownloadComplete() => __pbn__backgroundDownloadComplete = null;
        private bool? __pbn__backgroundDownloadComplete;

        [global::ProtoBuf.ProtoMember(6)]
        [global::System.ComponentModel.DefaultValue("")]
        public string currentVersion
        {
            get { return __pbn__currentVersion ?? ""; }
            set { __pbn__currentVersion = value; }
        }
        public bool ShouldSerializecurrentVersion() => __pbn__currentVersion != null;
        public void ResetcurrentVersion() => __pbn__currentVersion = null;
        private string __pbn__currentVersion;

        [global::ProtoBuf.ProtoMember(7)]
        [global::System.ComponentModel.DefaultValue("")]
        public string currentVersionStr
        {
            get { return __pbn__currentVersionStr ?? ""; }
            set { __pbn__currentVersionStr = value; }
        }
        public bool ShouldSerializecurrentVersionStr() => __pbn__currentVersionStr != null;
        public void ResetcurrentVersionStr() => __pbn__currentVersionStr = null;
        private string __pbn__currentVersionStr;

        [global::ProtoBuf.ProtoMember(8, Name = @"installedBuildConfig")]
        public global::System.Collections.Generic.List<BuildConfig> installedBuildConfigs { get; } = new global::System.Collections.Generic.List<BuildConfig>();

        [global::ProtoBuf.ProtoMember(9, Name = @"backgroundDownloadBuildConfig")]
        public global::System.Collections.Generic.List<BuildConfig> backgroundDownloadBuildConfigs { get; } = new global::System.Collections.Generic.List<BuildConfig>();

        [global::ProtoBuf.ProtoMember(10)]
        [global::System.ComponentModel.DefaultValue("")]
        public string decryptionKey
        {
            get { return __pbn__decryptionKey ?? ""; }
            set { __pbn__decryptionKey = value; }
        }
        public bool ShouldSerializedecryptionKey() => __pbn__decryptionKey != null;
        public void ResetdecryptionKey() => __pbn__decryptionKey = null;
        private string __pbn__decryptionKey;

        [global::ProtoBuf.ProtoMember(11)]
        public global::System.Collections.Generic.List<string> completedInstallActions { get; } = new global::System.Collections.Generic.List<string>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BackfillProgress : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"progress")]
        public double Progress
        {
            get { return __pbn__Progress.GetValueOrDefault(); }
            set { __pbn__Progress = value; }
        }
        public bool ShouldSerializeProgress() => __pbn__Progress != null;
        public void ResetProgress() => __pbn__Progress = null;
        private double? __pbn__Progress;

        [global::ProtoBuf.ProtoMember(2, Name = @"backgrounddownload")]
        public bool Backgrounddownload
        {
            get { return __pbn__Backgrounddownload.GetValueOrDefault(); }
            set { __pbn__Backgrounddownload = value; }
        }
        public bool ShouldSerializeBackgrounddownload() => __pbn__Backgrounddownload != null;
        public void ResetBackgrounddownload() => __pbn__Backgrounddownload = null;
        private bool? __pbn__Backgrounddownload;

        [global::ProtoBuf.ProtoMember(3, Name = @"paused")]
        public bool Paused
        {
            get { return __pbn__Paused.GetValueOrDefault(); }
            set { __pbn__Paused = value; }
        }
        public bool ShouldSerializePaused() => __pbn__Paused != null;
        public void ResetPaused() => __pbn__Paused = null;
        private bool? __pbn__Paused;

        [global::ProtoBuf.ProtoMember(4)]
        public ulong downloadLimit
        {
            get { return __pbn__downloadLimit.GetValueOrDefault(); }
            set { __pbn__downloadLimit = value; }
        }
        public bool ShouldSerializedownloadLimit() => __pbn__downloadLimit != null;
        public void ResetdownloadLimit() => __pbn__downloadLimit = null;
        private ulong? __pbn__downloadLimit;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class RepairProgress : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"progress")]
        public double Progress
        {
            get { return __pbn__Progress.GetValueOrDefault(); }
            set { __pbn__Progress = value; }
        }
        public bool ShouldSerializeProgress() => __pbn__Progress != null;
        public void ResetProgress() => __pbn__Progress = null;
        private double? __pbn__Progress;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class UpdateProgress : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string lastDiscSetUsed
        {
            get { return __pbn__lastDiscSetUsed ?? ""; }
            set { __pbn__lastDiscSetUsed = value; }
        }
        public bool ShouldSerializelastDiscSetUsed() => __pbn__lastDiscSetUsed != null;
        public void ResetlastDiscSetUsed() => __pbn__lastDiscSetUsed = null;
        private string __pbn__lastDiscSetUsed;

        [global::ProtoBuf.ProtoMember(2, Name = @"progress")]
        public double Progress
        {
            get { return __pbn__Progress.GetValueOrDefault(); }
            set { __pbn__Progress = value; }
        }
        public bool ShouldSerializeProgress() => __pbn__Progress != null;
        public void ResetProgress() => __pbn__Progress = null;
        private double? __pbn__Progress;

        [global::ProtoBuf.ProtoMember(3)]
        public bool discIgnored
        {
            get { return __pbn__discIgnored.GetValueOrDefault(); }
            set { __pbn__discIgnored = value; }
        }
        public bool ShouldSerializediscIgnored() => __pbn__discIgnored != null;
        public void ResetdiscIgnored() => __pbn__discIgnored = null;
        private bool? __pbn__discIgnored;

        [global::ProtoBuf.ProtoMember(4)]
        [global::System.ComponentModel.DefaultValue(0)]
        public ulong totalToDownload
        {
            get { return __pbn__totalToDownload ?? 0; }
            set { __pbn__totalToDownload = value; }
        }
        public bool ShouldSerializetotalToDownload() => __pbn__totalToDownload != null;
        public void ResettotalToDownload() => __pbn__totalToDownload = null;
        private ulong? __pbn__totalToDownload;

        [global::ProtoBuf.ProtoMember(5)]
        [global::System.ComponentModel.DefaultValue(0)]
        public ulong downloadRemaining
        {
            get { return __pbn__downloadRemaining ?? 0; }
            set { __pbn__downloadRemaining = value; }
        }
        public bool ShouldSerializedownloadRemaining() => __pbn__downloadRemaining != null;
        public void ResetdownloadRemaining() => __pbn__downloadRemaining = null;
        private ulong? __pbn__downloadRemaining;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class CachedProductState : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        public BaseProductState baseProductState { get; set; }

        [global::ProtoBuf.ProtoMember(2)]
        public BackfillProgress backfillProgress { get; set; }

        [global::ProtoBuf.ProtoMember(3)]
        public RepairProgress repairProgress { get; set; }

        [global::ProtoBuf.ProtoMember(4)]
        public UpdateProgress updateProgress { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class ProductOperations : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue(Operation.OpNone)]
        public Operation activeOperation
        {
            get { return __pbn__activeOperation ?? Operation.OpNone; }
            set { __pbn__activeOperation = value; }
        }
        public bool ShouldSerializeactiveOperation() => __pbn__activeOperation != null;
        public void ResetactiveOperation() => __pbn__activeOperation = null;
        private Operation? __pbn__activeOperation;

        [global::ProtoBuf.ProtoMember(2, Name = @"priority")]
        public ulong Priority
        {
            get { return __pbn__Priority.GetValueOrDefault(); }
            set { __pbn__Priority = value; }
        }
        public bool ShouldSerializePriority() => __pbn__Priority != null;
        public void ResetPriority() => __pbn__Priority = null;
        private ulong? __pbn__Priority;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BnetProductInstall : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"uid")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Uid
        {
            get { return __pbn__Uid ?? ""; }
            set { __pbn__Uid = value; }
        }
        public bool ShouldSerializeUid() => __pbn__Uid != null;
        public void ResetUid() => __pbn__Uid = null;
        private string __pbn__Uid;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue("")]
        public string productCode
        {
            get { return __pbn__productCode ?? ""; }
            set { __pbn__productCode = value; }
        }
        public bool ShouldSerializeproductCode() => __pbn__productCode != null;
        public void ResetproductCode() => __pbn__productCode = null;
        private string __pbn__productCode;

        [global::ProtoBuf.ProtoMember(3, Name = @"settings")]
        public UserSettings Settings { get; set; }

        [global::ProtoBuf.ProtoMember(4)]
        public CachedProductState cachedProductState { get; set; }

        [global::ProtoBuf.ProtoMember(5)]
        public ProductOperations productOperations { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BnetProductConfig : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string productCode
        {
            get { return __pbn__productCode ?? ""; }
            set { __pbn__productCode = value; }
        }
        public bool ShouldSerializeproductCode() => __pbn__productCode != null;
        public void ResetproductCode() => __pbn__productCode = null;
        private string __pbn__productCode;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue("")]
        public string metadataHash
        {
            get { return __pbn__metadataHash ?? ""; }
            set { __pbn__metadataHash = value; }
        }
        public bool ShouldSerializemetadataHash() => __pbn__metadataHash != null;
        public void ResetmetadataHash() => __pbn__metadataHash = null;
        private string __pbn__metadataHash;

        [global::ProtoBuf.ProtoMember(3, Name = @"timestamp")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Timestamp
        {
            get { return __pbn__Timestamp ?? ""; }
            set { __pbn__Timestamp = value; }
        }
        public bool ShouldSerializeTimestamp() => __pbn__Timestamp != null;
        public void ResetTimestamp() => __pbn__Timestamp = null;
        private string __pbn__Timestamp;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BnetActiveProcess : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue("")]
        public string processName
        {
            get { return __pbn__processName ?? ""; }
            set { __pbn__processName = value; }
        }
        public bool ShouldSerializeprocessName() => __pbn__processName != null;
        public void ResetprocessName() => __pbn__processName = null;
        private string __pbn__processName;

        [global::ProtoBuf.ProtoMember(2, Name = @"pid")]
        public int Pid
        {
            get { return __pbn__Pid.GetValueOrDefault(); }
            set { __pbn__Pid = value; }
        }
        public bool ShouldSerializePid() => __pbn__Pid != null;
        public void ResetPid() => __pbn__Pid = null;
        private int? __pbn__Pid;

        [global::ProtoBuf.ProtoMember(3, Name = @"uri")]
        public global::System.Collections.Generic.List<string> Uris { get; } = new global::System.Collections.Generic.List<string>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BnetDownloadSettings : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue(-1)]
        public int downloadLimit
        {
            get { return __pbn__downloadLimit ?? -1; }
            set { __pbn__downloadLimit = value; }
        }
        public bool ShouldSerializedownloadLimit() => __pbn__downloadLimit != null;
        public void ResetdownloadLimit() => __pbn__downloadLimit = null;
        private int? __pbn__downloadLimit;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue(-1)]
        public int backfillLimit
        {
            get { return __pbn__backfillLimit ?? -1; }
            set { __pbn__backfillLimit = value; }
        }
        public bool ShouldSerializebackfillLimit() => __pbn__backfillLimit != null;
        public void ResetbackfillLimit() => __pbn__backfillLimit = null;
        private int? __pbn__backfillLimit;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BnetDatabase : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"productInstall")]
        public global::System.Collections.Generic.List<BnetProductInstall> productInstalls { get; } = new global::System.Collections.Generic.List<BnetProductInstall>();

        [global::ProtoBuf.ProtoMember(2)]
        public global::System.Collections.Generic.List<BnetInstallHandshake> activeInstalls { get; } = new global::System.Collections.Generic.List<BnetInstallHandshake>();

        [global::ProtoBuf.ProtoMember(3)]
        public global::System.Collections.Generic.List<BnetActiveProcess> activeProcesses { get; } = new global::System.Collections.Generic.List<BnetActiveProcess>();

        [global::ProtoBuf.ProtoMember(4)]
        public global::System.Collections.Generic.List<BnetProductConfig> productConfigs { get; } = new global::System.Collections.Generic.List<BnetProductConfig>();

        [global::ProtoBuf.ProtoMember(5)]
        public BnetDownloadSettings downloadSettings { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public enum LanguageOption
    {
        [global::ProtoBuf.ProtoEnum(Name = @"LANGOPTION_NONE")]
        LangoptionNone = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"LANGOPTION_TEXT")]
        LangoptionText = 1,
        [global::ProtoBuf.ProtoEnum(Name = @"LANGOPTION_SPEECH")]
        LangoptionSpeech = 2,
        [global::ProtoBuf.ProtoEnum(Name = @"LANGOPTION_TEXT_AND_SPEECH")]
        LangoptionTextAndSpeech = 3,
    }

    [global::ProtoBuf.ProtoContract()]
    public enum LanguageSettingType
    {
        [global::ProtoBuf.ProtoEnum(Name = @"LANGSETTING_NONE")]
        LangsettingNone = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"LANGSETTING_SINGLE")]
        LangsettingSingle = 1,
        [global::ProtoBuf.ProtoEnum(Name = @"LANGSETTING_SIMPLE")]
        LangsettingSimple = 2,
        [global::ProtoBuf.ProtoEnum(Name = @"LANGSETTING_ADVANCED")]
        LangsettingAdvanced = 3,
    }

    [global::ProtoBuf.ProtoContract()]
    public enum ShortcutOption
    {
        [global::ProtoBuf.ProtoEnum(Name = @"SHORTCUT_NONE")]
        ShortcutNone = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"SHORTCUT_USER")]
        ShortcutUser = 1,
        [global::ProtoBuf.ProtoEnum(Name = @"SHORTCUT_ALL_USERS")]
        ShortcutAllUsers = 2,
    }

    [global::ProtoBuf.ProtoContract()]
    public enum Operation
    {
        [global::ProtoBuf.ProtoEnum(Name = @"OP_NONE")]
        OpNone = -1,
        [global::ProtoBuf.ProtoEnum(Name = @"OP_UPDATE")]
        OpUpdate = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"OP_BACKFILL")]
        OpBackfill = 1,
        [global::ProtoBuf.ProtoEnum(Name = @"OP_REPAIR")]
        OpRepair = 2,
    }
#pragma warning restore CS1591, CS0612, CS3021, IDE1006, CA1033
}