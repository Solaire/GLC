using HtmlAgilityPack;
using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using static GameLauncher_Console.CRegScanner;

namespace GameLauncher_Console
{
	// Big Fish Games
	// [owned and installed games]
	public class PlatformBigFish : IPlatform
	{
		public const CGameData.GamePlatform ENUM	= CGameData.GamePlatform.BigFish;
		public const string NAME					= "BigFish";
		public const string DESCRIPTION				= "Big Fish";
		//private const string START_GAME			= "LaunchGame.bfg";
		private const string BIGFISH_GAME_FOLDER	= "BFG-";
		public const string BIGFISH_REG				= @"SOFTWARE\WOW6432Node\Big Fish Games\Client"; // HKLM32
		private const string BIGFISH_GAMES			= @"SOFTWARE\WOW6432Node\Big Fish Games\Persistence\GameDB"; // HKLM32
		private const string BIGFISH_ID				= "WrapID";
		private const string BIGFISH_PATH			= "ExecutablePath";
		//private const string BIGFISH_ACTIV		= "Activated";
		//private const string BIGFISH_DAYS			= "DaysLeft";
		//private const string BIGFISH_TIME			= "TimeLeft";
		private const string BIGFISH_CASINO_ID		= "F7315T1L1";

		CGameData.GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => NAME;

        string IPlatform.Description => DESCRIPTION;

        public static void Launch()
		{
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BIGFISH_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				string launcherPath = key.GetValue("InstallationPath") + "\\bfgclient.exe";
				if (File.Exists(launcherPath))
					Process.Start(launcherPath);
				else
				{
					//SetFgColour(cols.errorCC, cols.errorLtCC);
					CLogger.LogWarn("Cannot start {0} launcher.", NAME.ToUpper());
					Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
					//Console.ResetColor();
				}
			}
		}

		public static void InstallGame(CGameData.CGame game)
		{
			CDock.DeleteCustomImage(game.Title);
			Launch();
		}

		public void GetGames(List<RegistryGameData> gameDataList) => GetGames(gameDataList, false);

        public void GetGames(List<RegistryGameData> gameDataList, bool expensiveIcons)
		{
			List<RegistryKey> keyList;

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BIGFISH_GAMES, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", NAME.ToUpper());
					return;
				}

				keyList = FindGameFolders(key, "");

				CLogger.LogInfo("{0} {1} games found", keyList.Count, NAME.ToUpper());
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
					string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.BigFish);
					try
					{
						//bool found = false;
						strTitle = GetRegStrVal(data, "Name");

						// If this is an expired trial, count it as not-installed
						/*
						int activated = (int)GetRegDWORDVal(data, BIGFISH_ACTIV);
						int daysLeft = (int)GetRegDWORDVal(data, BIGFISH_DAYS);
						int timeLeft = (int)GetRegDWORDVal(data, BIGFISH_TIME);
						if (activated > 0 || timeLeft > 0 || daysLeft > 0)
						{
							found = true;
						*/
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
								strIconPath = CGameFinder.FindGameBinaryFile(Path.GetDirectoryName(strLaunch), strTitle);
							if (!(string.IsNullOrEmpty(strLaunch)))
								gameDataList.Add(
									new RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
						//}

						// Add not-installed/expired games
						/*
						if (!found)
						{
							CLogger.LogDebug($"- *{strTitle}");
							gameDataList.Add(new RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));

							// Use website to download not-installed icons
							if ((bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
							{
								int.TryParse(wrap.Substring(1, wrap.IndexOf('T') - 1), out int num);
								if (num > 0)
								{
									string url = $"https://www.bigfishgames.com/games/{num}/";
									/*
#if DEBUG
									// Don't re-download if file exists
									string tmpfile = $"tmp_{NAME}_{num}.html";
									if (!File.Exists(tmpfile))
									{
										using (var client = new WebClient())
										{
											client.DownloadFile(url, tmpfile);
										}
									}
									HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument
									{
										OptionUseIdAttribute = true
									};
									doc.Load(tmpfile);
#else
									*/
						/*
									HtmlWeb web = new HtmlWeb
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
											if (attr.Name == "src")
											{
												string iconUrl = attr.Value;
												if (CDock.DownloadCustomImage(strTitle, iconUrl))
													break;
											}
										}
									}
								}
							}
						}
						*/
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
				}
				CLogger.LogDebug("------------------------");
			}
		}
	}
}