using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Battle.net (Blizzard)
	// [installed games only]
	public class PlatformBattlenet : IPlatform
    {
		public const GamePlatform ENUM = GamePlatform.Battlenet;
		public const string PROTOCOL			= "battlenet://";   // "blizzard://" works too [TODO: is one more compatible with older versions?]
		//private const string BATTLE_NET		= "Battle.net";
		private const string BATTLE_NET_UNREG	= "Battle.net";		// HKLM32 Uninstall
		//private const string BATTLE_NET_REG	= @"SOFTWARE\WOW6432Node\Blizzard Entertainment\Battle.net"; // HKLM32
		private const string BATTLE_NET_UNINST	= @"Battle.net\Agent\Blizzard Uninstaller.exe";

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

        public static void Launch() => Process.Start(PROTOCOL);

		public static void InstallGame(CGame game) => throw new NotImplementedException();

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
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
			CLogger.LogDebug("--------------------------");
		}
	}
}