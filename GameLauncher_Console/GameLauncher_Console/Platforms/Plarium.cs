using Logger;
//using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CGameFinder;
using static GameLauncher_Console.CJsonWrapper;
//using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Plarium Play
	// [installed games only]
	public class PlatformPlarium : IPlatform
	{
		public const GamePlatform ENUM			= GamePlatform.Plarium;
		public const string PROTOCOL			= "plariumplay://";
		//private const string PLARIUM_FOLDER	= "InstallFolder";
		//private const string PLARIUM_REG		= "PlariumPlayInstaller"; //HKCU64
		//private const string PLARIUM_UNREG	= "{970D6975-3C2A-4AF9-B190-12AF8837331F}";	// HKLM32 Uninstall
		private const string PLARIUM_DB			= @"Plarium\PlariumPlay\gamestorage.gsfn";	// AppData\Local
		private const string PLARIUM_GAMES		= @"Plarium\PlariumPlay\StandAloneApps";	// AppData\Local

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

		string IPlatform.Description => GetPlatformString(ENUM);

		public static void Launch()
		{
			if (OperatingSystem.IsWindows())
				CDock.StartShellExecute(PROTOCOL);
			else
				Process.Start(PROTOCOL);
		}

		// return value
		// -1 = not implemented
		// 0 = failure
		// 1 = success
		public static int InstallGame(CGame game)
		{
			//CDock.DeleteCustomImage(game.Title);
			Launch();
			return -1;
		}

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			bool error = false;
			string strPlatform = GetPlatformString(ENUM);
			string file = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), PLARIUM_DB);
			/*
			string launcherPath = "";
			int? lang = 1;

			using (RegistryKey launcherKey = Registry.CurrentUser.OpenSubKey(PLARIUM_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKCU64
			{
				if (launcherKey == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", _name.ToUpper());
					return;
				}
				launcherPath = GetRegStrVal(launcherKey, PLARIUM_FOLDER);
				lang = GetRegDWORDVal(launcherKey, "Language");
			}
			*/


			if (!File.Exists(file))
			{
				CLogger.LogInfo("{0} database not found.", _name.ToUpper());
				return;
			}
			try
			{
				while (true)
				{
					string strDocumentData = File.ReadAllText(file);

					if (string.IsNullOrEmpty(strDocumentData))
					{
						error = true;
						break;
					}

					using JsonDocument document = JsonDocument.Parse(@strDocumentData, jsonTrailingCommas);
					document.RootElement.TryGetProperty("InstalledGames", out JsonElement games);
					if (games.Equals(null))
					{
						error = true;
						break;
					}

					foreach (JsonProperty game in games.EnumerateObject())
					{
						CultureInfo ci = new("en-GB");
						TextInfo ti = ci.TextInfo;

						string strID = "";
						string strTitle = "";
						string strLaunch = "";
						string strAlias = "";

						foreach (JsonProperty gameVal in game.Value.EnumerateObject())
						{
							if (gameVal.Name.Equals("InsalledGames", CDock.IGNORE_CASE))
							{
								foreach (JsonProperty install in gameVal.Value.EnumerateObject())
								{
									string num = install.Value.GetString();
									string id = install.Name;
									strID = "plarium_" + id;
									string path = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), PLARIUM_GAMES, id, num);
									strLaunch = FindGameBinaryFile(path, id);
									string infoPath = Path.Combine(path, id + "_Data", "app.info");
									string jsonPath = Path.Combine(path, "settings.json");
									if (File.Exists(infoPath))
									{
										int i = 0;
										foreach (string line in File.ReadLines(infoPath))
										{
											if (i == 1)
											{
												strTitle = line.Trim();
												break;
											}
											i++;
										}
									}
									if (string.IsNullOrEmpty(strTitle) && File.Exists(jsonPath))
									{
										string strDocumentData2 = File.ReadAllText(jsonPath);
										if (!string.IsNullOrEmpty(strDocumentData2))
										{
											using JsonDocument document2 = JsonDocument.Parse(@strDocumentData2, jsonTrailingCommas);
											strTitle = GetStringProperty(document2.RootElement, "productName");
										}
									}
									if (string.IsNullOrEmpty(strTitle))
										strTitle = ti.ToTitleCase(strID);
									CLogger.LogDebug($"- {strTitle}");
									strAlias = GetAlias(Path.GetFileNameWithoutExtension(strLaunch));
									if (strAlias.Length > strTitle.Length)
										strAlias = GetAlias(strTitle);
									if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
										strAlias = "";
									if (!string.IsNullOrEmpty(strLaunch))
										gameDataList.Add(new ImportGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
								}
							}
						}
					}
					break;
				}
			}
			catch (Exception e)
            {
				CLogger.LogError(e, string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
			}

			if (error)
			{
				CLogger.LogInfo("Malformed {0} file: {1}", _name.ToUpper(), file);
				return;
			}
			CLogger.LogDebug("------------------------");
		}
	}
}