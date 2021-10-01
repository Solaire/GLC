﻿using HtmlAgilityPack;
using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
//using System.Net;
using System.Xml;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CRegScanner;

namespace GameLauncher_Console
{
	// Big Fish Games
	// [owned and installed games]
	public class PlatformBigFish : IPlatform
	{
		public const GamePlatform ENUM	= GamePlatform.BigFish;
		//private const string START_GAME			= "LaunchGame.bfg";
		private const string BIGFISH_GAME_FOLDER	= "BFG-";
		public const string BIGFISH_REG				= @"SOFTWARE\WOW6432Node\Big Fish Games\Client"; // HKLM32
		private const string BIGFISH_GAMES			= @"SOFTWARE\WOW6432Node\Big Fish Games\Persistence\GameDB"; // HKLM32
		private const string BIGFISH_ID				= "WrapID";
		private const string BIGFISH_PATH			= "ExecutablePath";
		private const string BIGFISH_ACTIV			= "Activated";
		private const string BIGFISH_DAYS			= "DaysLeft";
		private const string BIGFISH_TIME			= "TimeLeft";
		private const string BIGFISH_CASINO_ID		= "F7315T1L1";

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

		public static void Launch()
		{
			if (OperatingSystem.IsWindows())
			{
                using RegistryKey key = Registry.LocalMachine.OpenSubKey(BIGFISH_REG, RegistryKeyPermissionCheck.ReadSubTree); // HKLM32
                string launcherPath = Path.Combine(GetRegStrVal(key, "InstallationPath"), "bfgclient.exe");
                if (File.Exists(launcherPath))
                    Process.Start(launcherPath);
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

		public string GetIconUrl(CGame game)
        {
			return GetIconUrl(game.ID, game.Title);
        }

		public string GetIconUrl(string id, string title)
		{
			string num = GetGameID(id);
			if (!string.IsNullOrEmpty(num))
			{
				string url = $"https://www.bigfishgames.com/games/{num}/";
				/*
#if DEBUG
				// Don't re-download if file exists
				string tmpfile = $"tmp_{_name}_{num}.html";
				if (!File.Exists(tmpfile))
				{
					using (var client = new WebClient())
					{
						client.DownloadFile(url, tmpfile);
					}
				}
                HtmlDocument doc = new HtmlDocument
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
				HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@class='rr-game-image']");
				foreach (HtmlNode child in node.ChildNodes)
				{
					foreach (HtmlAttribute attr in child.Attributes)
					{
						if (attr.Name.Equals("src", CDock.IGNORE_CASE))
						{
							return attr.Value;
						}
					}
				}
			}
			return null;
		}

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			List<RegistryKey> keyList;

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BIGFISH_GAMES, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", _name.ToUpper());
					return;
				}

				keyList = FindGameFolders(key, "");

				CLogger.LogInfo("{0} {1} games found", keyList.Count, _name.ToUpper());
				foreach (var data in keyList)
				{
					string wrap = Path.GetFileName(data.Name);
					if (wrap.Equals(BIGFISH_CASINO_ID))  // hide Big Fish Casino Activator
						continue;

					string strID = "bfg_" + wrap;
					string strTitle = "";
					string strLaunch = "";
					string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = GetPlatformString(GamePlatform.BigFish);
					try
					{
						//found = true;
						bool isInstalled = false;
						strTitle = GetRegStrVal(data, "Name");

						// If this is an expired trial, count it as not-installed
						int activated = (int)GetRegDWORDVal(data, BIGFISH_ACTIV);
						int daysLeft = (int)GetRegDWORDVal(data, BIGFISH_DAYS);
						int timeLeft = (int)GetRegDWORDVal(data, BIGFISH_TIME);
						if (activated > 0 || timeLeft > 0 || daysLeft > 0)
						{
							isInstalled = true;
						}
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = GetRegStrVal(data, BIGFISH_PATH);
						strAlias = GetAlias(strTitle);
						if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
							strAlias = "";

						List<RegistryKey> unKeyList;
						using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
						{
							if (key2 != null)
							{
								unKeyList = FindGameFolders(key2, BIGFISH_GAME_FOLDER);
								foreach (var data2 in unKeyList)
								{
									if (GetRegStrVal(data2, BIGFISH_ID).Equals(wrap))
									{
										strIconPath = GetRegStrVal(data2, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
										strUninstall = GetRegStrVal(data2, GAME_UNINSTALL_STRING).Trim(new char[] { ' ', '"' });
									}
								}
							}
						}
						if (string.IsNullOrEmpty(strIconPath) && expensiveIcons)
						{
							bool success = false;
							string dir = Path.GetDirectoryName(strLaunch);
							string xmlPath = Path.Combine(dir, "bfgstate.xml");
							try
							{
								if (File.Exists(xmlPath))
								{
									XmlDocument doc = new();
									doc.Load(xmlPath);
									XmlNode node = doc.DocumentElement.SelectSingleNode("thumbnail");
									if (node != null)
									{
										strIconPath = node.InnerText;
										if (File.Exists(strIconPath))
											success = true;
										else
										{
											// Use website to download missing icons
											if (!(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
											{
												if (CDock.DownloadCustomImage(strTitle, GetIconUrl(GetGameID(strID), strTitle)))
													success = true;
											}
										}
									}
								}
							}
							catch (Exception e)
							{
								CLogger.LogError(e);
							}
							if (!success)
							{
								strIconPath = CGameFinder.FindGameBinaryFile(dir, strTitle);
							}
						}
						if (!(string.IsNullOrEmpty(strLaunch)))
							gameDataList.Add(
								new ImportGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, isInstalled, strPlatform));

						// Add not-installed games [not used]
						/*
						if (!found)
						{
							CLogger.LogDebug($"- *{strTitle}");
							gameDataList.Add(new ImportGameData(strID, strTitle, "", "", "", "", false, strPlatform));

							// Use website to download not-installed icons
							if (!(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
							{
								CDock.DownloadCustomImage(strTitle, GetIconUrl(GetGameID(strID), strTitle));
							}
						}
						*/
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
				}
			}
			CLogger.LogDebug("------------------------");
		}

		/// <summary>
		/// Scan the key name and extract the Big Fish game id
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>Big Fish game ID as string</returns>
		public static string GetGameID(string key)
		{
			if (int.TryParse(key.Substring(5, key.IndexOf('T') - 1), out int num) && num > 0)
				return num.ToString();
			else
				return null;
		}
	}
}