using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static GameLauncher_Console.CRegScanner;

namespace GameLauncher_Console
{
	// Battle.net (Blizzard)
	// [installed games only]
	public class PlatformBattlenet : IPlatform
    {
		public const CGameData.GamePlatform ENUM = CGameData.GamePlatform.Battlenet;
		public const string NAME				= "Battlenet";
		public const string DESCRIPTION			= "Battle.net";
		public const string PROTOCOL			= "battlenet://";	// "blizzard://" works too [TODO: is one more compatible with older versions?]
		//private const string BATTLE_NET_UNREG	= "Battle.net"; // HKLM32 Uninstall
		private const string BATTLE_NET_REG		= @"SOFTWARE\WOW6432Node\Blizzard Entertainment\Battle.net"; // HKLM32

		CGameData.GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => NAME;

        string IPlatform.Description => DESCRIPTION;

        public static void Launch() => Process.Start(PROTOCOL);

		public static void InstallGame(CGameData.CGame game) => throw new NotImplementedException();

		public void GetGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList; //= new List<RegistryKey>();

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", NAME.ToUpper());
					return;
				}

				keyList = FindGameKeys(key, BATTLE_NET_REG, GAME_UNINSTALL_STRING, new string[] { BATTLE_NET_REG });

				CLogger.LogInfo("{0} {1} games found", keyList.Count, NAME.ToUpper());
				foreach (var data in keyList)
				{
					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					//string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Battlenet);
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
							new RegistryGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
				}
			}
			CLogger.LogDebug("--------------------------");
		}

		public void GetGames(List<RegistryGameData> gameDataList, bool expensiveIcons)
		{
			GetGames(gameDataList);
		}
	}
}