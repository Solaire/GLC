using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
//using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Epic Games Launcher
	// [installed games only]
	public class PlatformEpic : IPlatform
	{
		public const GamePlatform ENUM			= GamePlatform.Epic;
		public const string PROTOCOL			= "com.epicgames.launcher://";
		//private const string EPIC_GAMES_UNREG	= "{A2FB1E1A-55D9-4511-A0BF-DEAD0493FBBC}"; // HKLM32 Uninstall
		//private const string EPIC_GAMES_UNREG	= "{A7BBC0A6-3DB0-41CC-BCED-DDFC5D4F3060}"; // HKLM32 Uninstall
		private const string EPIC_ITEMS 		= @"Epic\EpicGamesLauncher\Data\Manifests"; // ProgramData

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

		public static void InstallGame(CGame game) => throw new NotImplementedException();

		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			string dir = Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), EPIC_ITEMS);
			if (!Directory.Exists(dir))
			{
				CLogger.LogInfo("{0} games not found in ProgramData.", _name.ToUpper());
				return;
			}
			string[] files = Directory.GetFiles(dir, "*.item", SearchOption.TopDirectoryOnly);
			CLogger.LogInfo("{0} {1} games found", files.Length, _name.ToUpper());

			foreach (string file in files)
			{
				string strDocumentData = File.ReadAllText(file);

				if (string.IsNullOrEmpty(strDocumentData))
					continue;

				try
				{
                    using JsonDocument document = JsonDocument.Parse(@strDocumentData, jsonTrailingCommas);
                    string strID = Path.GetFileName(file);
                    string strTitle = GetStringProperty(document.RootElement, "DisplayName");
                    CLogger.LogDebug($"- {strTitle}");
                    string strLaunch = GetStringProperty(document.RootElement, "LaunchExecutable"); // DLCs won't have this set
                    string strAlias = "";
                    string strPlatform = GetPlatformString(GamePlatform.Epic);

                    if (!string.IsNullOrEmpty(strLaunch))
                    {
                        strLaunch = Path.Combine(GetStringProperty(document.RootElement, "InstallLocation"), strLaunch);
                        strAlias = GetAlias(GetStringProperty(document.RootElement, "MandatoryAppFolderName"));
                        if (strAlias.Length > strTitle.Length)
                            strAlias = GetAlias(strTitle);
                        if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
                            strAlias = "";
                        gameDataList.Add(new ImportGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
                    }
                }
				catch (Exception e)
				{
					CLogger.LogError(e, string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
				}
			}
			CLogger.LogDebug("--------------------");
		}
	}
}