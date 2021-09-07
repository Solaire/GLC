using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using static GameLauncher_Console.CJsonWrapper;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Epic Games Launcher
	// [installed games only]
	public class PlatformEpic : IPlatform
	{
		public const CGameData.GamePlatform ENUM = CGameData.GamePlatform.Epic;
		public const string NAME				= "Epic";
		public const string DESCRIPTION			= "Epic Games Launcher";
		public const string PROTOCOL			= "com.epicgames.launcher://";
		//private const string EPIC_GAMES_UNREG	= "{A2FB1E1A-55D9-4511-A0BF-DEAD0493FBBC}"; // HKLM32 Uninstall
		//private const string EPIC_GAMES_UNREG	= "{A7BBC0A6-3DB0-41CC-BCED-DDFC5D4F3060}"; // HKLM32 Uninstall
		private const string EPIC_ITEMS_FOLDER	= @"\Epic\EpicGamesLauncher\Data\Manifests";

		CGameData.GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => NAME;

        string IPlatform.Description => DESCRIPTION;

        public static void Launch() => Process.Start(PROTOCOL);

		public static void InstallGame() => throw new NotImplementedException();

		public void GetGames(List<RegistryGameData> gameDataList)
		{
			string dir = GetFolderPath(SpecialFolder.CommonApplicationData) + EPIC_ITEMS_FOLDER;
			if (!Directory.Exists(dir))
			{
				CLogger.LogInfo("{0} games not found in ProgramData.", NAME.ToUpper());
				return;
			}
			string[] files = Directory.GetFiles(dir, "*.item", SearchOption.TopDirectoryOnly);
			CLogger.LogInfo("{0} {1} games found", files.Count(), NAME.ToUpper());

			foreach (string file in files)
			{
				string strDocumentData = File.ReadAllText(file);

				if (string.IsNullOrEmpty(strDocumentData))
					continue;

				try
				{
					using (JsonDocument document = JsonDocument.Parse(@strDocumentData, jsonTrailingCommas))
					{
						string strID = Path.GetFileName(file);
						string strTitle = GetStringProperty(document.RootElement, GAME_DISPLAY_NAME);
						CLogger.LogDebug($"- {strTitle}");
						string strLaunch = GetStringProperty(document.RootElement, "LaunchExecutable"); // DLCs won't have this set
						string strAlias = "";
						string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Epic);

						if (!string.IsNullOrEmpty(strLaunch))
						{
							strLaunch = Path.Combine(GetStringProperty(document.RootElement, GAME_INSTALL_LOCATION), strLaunch);
							strAlias = GetAlias(GetStringProperty(document.RootElement, "MandatoryAppFolderName"));
							if (strAlias.Length > strTitle.Length)
								strAlias = GetAlias(strTitle);
							if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
								strAlias = "";
							gameDataList.Add(new RegistryGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
						}
					}
				}
				catch (Exception e)
				{
					CLogger.LogError(e, string.Format("Malformed {0} file: {1}", NAME.ToUpper(), file));
				}
			}
			CLogger.LogDebug("--------------------");
		}

		public void GetGames(List<RegistryGameData> gameDataList, bool expensiveIcons)
		{
			GetGames(gameDataList);
		}
	}
}