using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Battlenet (Blizzard)
	/// </summary>
    public sealed class CBattlenetScanner : CBasePlatformScanner<CBattlenetScanner>
    {
		private const string BATTLENET_NAME          = "Battlenet";
		private const string BATTLENET_NAME_LONG     = "Battle.net";
		//private const string BATTLE_NET_UNREG		= "Battle.net"; // HKLM32 Uninstall
		private const string BATTLE_NET_REG         = @"SOFTWARE\WOW6432Node\Blizzard Entertainment\Battle.net"; // HKLM32

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			List<RegistryKey> keyList; //= new List<RegistryKey>();
			int gameCount = 0;

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if(key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", BATTLENET_NAME.ToUpper());
					return false;
				}

				keyList = CRegHelper.FindGameKeys(key, BATTLE_NET_REG, GAME_UNINSTALL_STRING, new string[] { BATTLE_NET_REG });

				CLogger.LogInfo("{0} {1} games found", keyList.Count, BATTLENET_NAME.ToUpper());
				foreach(var data in keyList)
				{
					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					//string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = "Battlenet";// CGameData.GetPlatformString(CGameData.GamePlatform.Battlenet);
					try
					{
						strID = Path.GetFileName(data.Name);
						strTitle = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_NAME);
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						strUninstall = CRegHelper.GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
						strAlias = CRegHelper.GetAlias(Path.GetFileNameWithoutExtension(CRegHelper.GetRegStrVal(data, GAME_INSTALL_LOCATION).Trim(new char[] { ' ', '\'', '"' })));
						if(strAlias.Length > strTitle.Length)
						{
							strAlias = CRegHelper.GetAlias(strTitle);
						}
						if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
						{
							strAlias = "";
						}
					}
					catch(Exception e)
					{
						CLogger.LogError(e);
					}
					if(!(string.IsNullOrEmpty(strLaunch)))
					{
						//gameList.Add(new GameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
						CEventDispatcher.NewGameFound(new RawGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
						gameCount++;
					}
				}
			}
			return gameCount > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			return false;
		}
    }
}
