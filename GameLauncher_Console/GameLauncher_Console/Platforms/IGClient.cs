﻿using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Indiegala Client
	// [owned and installed games]
	public class PlatformIGClient : IPlatform
	{
		public const GamePlatform ENUM		= GamePlatform.IGClient;
		public const string PROTOCOL		= "";
		private const string IG_REG			= @"SOFTWARE\6f4f090a-db12-53b6-ac44-9ecdb7703b4a"; // HKLM64
		//private const string IG_UNREG		= "6f4f090a-db12-53b6-ac44-9ecdb7703b4a"; // HKLM64 Uninstall
		private const string IG_JSON		= @"IGClient\storage\installed.json"; // AppData\Roaming
		private const string IG_OWN_JSON	= @"IGClient\config.json"; // AppData\Roaming

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

		public static void Launch()
		{
			if (OperatingSystem.IsWindows())
			{
                using RegistryKey key = Registry.LocalMachine.OpenSubKey(IG_REG, RegistryKeyPermissionCheck.ReadSubTree); // HKLM64
                Process igcProcess = new();
                string launcherPath = Path.Combine(GetRegStrVal(key, GAME_INSTALL_LOCATION), "IGClient.exe");
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

		public static void InstallGame(CGame game)
		{
			CDock.DeleteCustomImage(game.Title);
			Launch();
		}

        public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			List<string> igcIds = new();

			// Get installed games
			string file = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), IG_JSON);
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
                            string strID = "";
                            string strTitle = "";
                            string strLaunch = "";
                            string strAlias = "";
                            string strPlatform = GetPlatformString(ENUM);

                            element.TryGetProperty("target", out JsonElement target);
                            if (!target.Equals(null))
                            {
                                target.TryGetProperty("item_data", out JsonElement item);
                                if (!item.Equals(null))
                                {
                                    strID = GetStringProperty(item, "id_key_name");
                                    igcIds.Add(strID);
                                    strTitle = GetStringProperty(item, "name");
                                }
                            }
                            element.TryGetProperty("path", out JsonElement paths);
                            if (!paths.Equals(null))
                            {
                                foreach (JsonElement path in paths.EnumerateArray())
                                    strLaunch = CGameFinder.FindGameBinaryFile(path.ToString(), strTitle);
                            }

                            CLogger.LogDebug($"- {strTitle}");

                            if (!string.IsNullOrEmpty(strLaunch))
                            {
                                strAlias = GetAlias(Path.GetFileNameWithoutExtension(strLaunch));
                                if (strAlias.Length > strTitle.Length)
                                    strAlias = GetAlias(strTitle);
                                if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
                                    strAlias = "";
                                gameDataList.Add(new ImportGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
                            }
                        }
                    }
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
					}
				}
			}

			// Get not-installed games
			if (!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				file = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), IG_OWN_JSON);
				if (!File.Exists(file))
					CLogger.LogInfo("{0} not-installed games not found in AppData", _name.ToUpper());
				else
				{
					CLogger.LogDebug("{0} not-installed games:", _name.ToUpper());

					string strDocumentData = File.ReadAllText(file);

					if (string.IsNullOrEmpty(strDocumentData))
						CLogger.LogWarn(string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
					else
					{
						try
						{
                            using JsonDocument document = JsonDocument.Parse(@strDocumentData, jsonTrailingCommas);
                            bool exists = false;
                            bool found = false;
                            JsonElement coll = new();
                            document.RootElement.TryGetProperty("gala_data", out JsonElement gData);
                            if (!gData.Equals(null))
                            {
                                gData.TryGetProperty("data", out JsonElement data);
                                if (!data.Equals(null))
                                {
                                    data.TryGetProperty("showcase_content", out JsonElement sContent);
                                    if (!sContent.Equals(null))
                                    {
                                        sContent.TryGetProperty("content", out JsonElement content);
                                        if (!content.Equals(null))
                                        {
                                            content.TryGetProperty("user_collection", out coll);
                                            if (!coll.Equals(null))
                                                exists = true;
                                        }
                                    }
                                }
                            }

                            if (exists)
                            {
                                foreach (JsonElement prod in coll.EnumerateArray())
                                {
                                    string strID = GetStringProperty(prod, "prod_id_key_name");
                                    if (!string.IsNullOrEmpty(strID))
                                    {
                                        foreach (string id in igcIds)
                                        {
                                            if (id.Equals(strID))
                                                found = true;
                                        }
                                        if (!found)
                                        {
                                            string strTitle = GetStringProperty(prod, "prod_name");
                                            CLogger.LogDebug($"- *{strTitle}");
                                            string strPlatform = GetPlatformString(ENUM);
                                            gameDataList.Add(new ImportGameData(strID, strTitle, "", "", "", "", false, strPlatform));

                                            // Use prod_dev_image to download not-installed icons
                                            if (!(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
                                            {
                                                string devName = GetStringProperty(prod, "prod_dev_namespace");
                                                string image = GetStringProperty(prod, "prod_dev_image");
                                                if (!string.IsNullOrEmpty(devName) && !string.IsNullOrEmpty(image))
                                                {
                                                    string iconUrl = $"https://www.indiegalacdn.com/imgs/devs/{devName}/products/{strID}/prodmain/{image}";
                                                    CDock.DownloadCustomImage(strTitle, iconUrl);
                                                }
                                            }
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
			}
			CLogger.LogDebug("--------------------");
		}
	}
}