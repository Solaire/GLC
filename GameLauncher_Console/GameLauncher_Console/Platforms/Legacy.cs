using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Legacy Games
	// [installed games only]
	public class PlatformLegacy : IPlatform
	{
		public const GamePlatform ENUM		= GamePlatform.Legacy;
		public const string PROTOCOL		= "";
		private const string LEG_REG		= @"SOFTWARE\Legacy Games"; // HKCU64
		//private const string LEG_UNREG	= "da414c81-a9fd-5732-bd5e-8acced116298da414c81-a9fd-5732-bd5e-8acced116298"; // HKLM64 Uninstall
		private const string LEG_JSON		= @"legacy-games-launcher\app-state.json"; // AppData\Roaming

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

		public static void Launch()
		{
			if (OperatingSystem.IsWindows())
			{
                using RegistryKey key = Registry.LocalMachine.OpenSubKey(LEG_REG, RegistryKeyPermissionCheck.ReadSubTree); // HKLM64
                Process legacyProcess = new();
                string launcherPath = Path.Combine(Path.GetDirectoryName(GetRegStrVal(key, GAME_DISPLAY_ICON)), "Legacy Games Launcher.exe");
                if (File.Exists(launcherPath))
                    CDock.StartAndRedirect(launcherPath);
                else
                {
                    //SetFgColour(cols.errorCC, cols.errorLtCC);
                    CLogger.LogWarn("Cannot start {0} launcher.", _name.ToUpper());
                    Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
                    //Console.ResetColor();
                }
            }
		}

        public static void InstallGame(CGame game) => throw new NotImplementedException();

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			List<RegistryKey> keyList = new();

            // Get installed games
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(LEG_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM64
			{
				keyList = FindGameFolders(key, "");

				CLogger.LogInfo("{0} {1} games found", keyList.Count, _name.ToUpper());
				foreach (var data in keyList)
				{
					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = GetPlatformString(ENUM);
					try
					{
						strID = GetRegStrVal(data, "InstallerUUID");
						if (string.IsNullOrEmpty(strID))
							strID = data.Name;
						strTitle = GetRegStrVal(data, "ProductName");
						if (string.IsNullOrEmpty(strTitle))
							strTitle = data.Name;
						CLogger.LogDebug($"- {strTitle}");
						string exeFile = GetRegStrVal(data, "GameExe");
						strLaunch = Path.Combine(GetRegStrVal(data, "InstDir").Trim(new char[] { ' ', '"' }), exeFile);
						if (expensiveIcons)
						{
                            using RegistryKey key2 = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree), // HKLM32
                                              key3 = Registry.LocalMachine.OpenSubKey(NODE64_REG, RegistryKeyPermissionCheck.ReadSubTree); // HKLM64
                            List<RegistryKey> unList = FindGameKeys(key2, GAME_DISPLAY_NAME, strTitle, new string[] { "Legacy Games Launcher" });
                            if (unList.Count <= 0)
                                unList = FindGameKeys(key3, GAME_DISPLAY_NAME, strTitle, new string[] { "Legacy Games Launcher" });
                            foreach (RegistryKey unKey in unList)
                            {
                                strUninstall = GetRegStrVal(unKey, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
                                strIconPath = GetRegStrVal(unKey, GAME_DISPLAY_ICON); //.Trim(new char[] { ' ', '"' });
                                break;
                            }
                        }
						if (string.IsNullOrEmpty(strIconPath))
							strIconPath = strLaunch;
						strAlias = GetAlias(Path.GetFileNameWithoutExtension(exeFile));
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
							new ImportGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
				}
			}

			if (expensiveIcons && keyList.Count <= 0)
			{
				// if no games found in registry, check manually

				List<string> libPaths = new();

				string file = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), LEG_JSON);
				if (!File.Exists(file))
				{
					CLogger.LogInfo("{0} installed games not found in AppData", _name.ToUpper());
					return;
				}
				else
				{
					string strDocumentData = File.ReadAllText(file);

					if (string.IsNullOrEmpty(strDocumentData))
						CLogger.LogWarn(string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
					else
					{
						try
						{
							using JsonDocument document = JsonDocument.Parse(@strDocumentData, jsonTrailingCommas);
							foreach (JsonElement element in document.RootElement.EnumerateArray())
							{

								element.TryGetProperty("settings", out JsonElement settings);
								if (!settings.Equals(null))
								{
									settings.TryGetProperty("gameLibraryPath", out JsonElement paths);
									if (!paths.Equals(null))
									{
										foreach (JsonElement path in paths.EnumerateArray())
										{
											if (!path.Equals(null))
												libPaths.Add(path.ToString());
										}
									}
								}
							}
						}
						catch (Exception e)
						{
							CLogger.LogError(e, string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
						}
					}
				}

				foreach (string path in libPaths)
				{
					List<string> dirs = new();

					try
					{
						if (!path.Equals(null) && Directory.Exists(path))
						{
							dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly));
							foreach (string dir in dirs)
							{
								string strID = Path.GetFileName(dir);
								string strLaunch = "";
								string strAlias = "";
								string strPlatform = GetPlatformString(ENUM);

								CLogger.LogDebug($"- {strID}");
								strLaunch = CGameFinder.FindGameBinaryFile(dir, strID);
								strAlias = GetAlias(Path.GetFileNameWithoutExtension(strLaunch));
								if (strAlias.Length > strID.Length)
									strAlias = GetAlias(strID);
								if (strAlias.Equals(strID, CDock.IGNORE_CASE))
									strAlias = "";
								if (!(string.IsNullOrEmpty(strLaunch)))
									gameDataList.Add(
										new ImportGameData(strID, strID, strLaunch, strLaunch, "", strAlias, true, strPlatform));
							}

						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
				}
			}
			CLogger.LogDebug("--------------------");
		}
	}
}