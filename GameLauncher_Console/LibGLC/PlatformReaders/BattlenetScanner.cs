using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Battlenet (Blizzard)
	/// This scanner uses the Registry to access game data
	/// </summary>
    public sealed class CBattlenetScanner : CBasePlatformScanner<CBattlenetScanner>
    {
		private const string BATTLE_NET_REG = @"SOFTWARE\WOW6432Node\Blizzard Entertainment\Battle.net"; // HKLM32

		private CBattlenetScanner()
        {
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Battlenet);
		}

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			List<RegistryKey> keyList = new List<RegistryKey>();
			int gameCount = 0;

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if(key == null)
				{
					CLogger.LogInfo("{0}: Client not found in the registry.", m_platformName.ToUpper());
					return false;
				}

				keyList = CRegHelper.FindGameKeys(key, BATTLE_NET_REG, GAME_UNINSTALL_STRING, new string[] { BATTLE_NET_REG });

				CLogger.LogInfo("{0} games found", keyList.Count);
				foreach(var data in keyList)
				{
					string id = "";
					string title = "";
					string launch = "";
					//string iconPath = "";
					string uninstall = "";
					string alias = "";
					try
					{
						id = Path.GetFileName(data.Name);
						title = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_NAME);
						launch = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						uninstall = CRegHelper.GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
						alias = CRegHelper.GetAlias(Path.GetFileNameWithoutExtension(CRegHelper.GetRegStrVal(data, GAME_INSTALL_LOCATION).Trim(new char[] { ' ', '\'', '"' })));
						if(alias.Length > title.Length)
						{
							alias = CRegHelper.GetAlias(title);
						}
						if(alias.Equals(title, StringComparison.CurrentCultureIgnoreCase))
						{
							alias = "";
						}
					}
					catch(Exception e)
					{
						CLogger.LogError(e);
					}
					if(!string.IsNullOrEmpty(launch))
					{
						CEventDispatcher.OnGameFound(new RawGameData(id, title, launch, launch, uninstall, alias, true, m_platformName));
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
