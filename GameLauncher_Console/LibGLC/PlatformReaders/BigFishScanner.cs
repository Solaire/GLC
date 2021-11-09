using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for BigFish games
	/// </summary>
    public sealed class CBigFishScanner : CBasePlatformScanner<CBigFishScanner>
    {
		private const string BIGFISH_NAME            = "BigFish";
		private const string BIGFISH_NAME_LONG       = "Big Fish";
		private const string BIGFISH_GAME_FOLDER    = "BFG-";
		private const string BIGFISH_REG             = @"SOFTWARE\WOW6432Node\Big Fish Games\Client"; // HKLM32
		private const string BIGFISH_GAMES          = @"SOFTWARE\WOW6432Node\Big Fish Games\Persistence\GameDB"; // HKLM32
		private const string BIGFISH_ID             = "WrapID";
		private const string BIGFISH_PATH           = "ExecutablePath";
		private const string BIGFISH_ACTIV          = "Activated";
		private const string BIGFISH_DAYS           = "DaysLeft";
		private const string BIGFISH_TIME           = "TimeLeft";

		private CBigFishScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.BigFish);
		}

		protected override bool GetInstalledGames(bool expensiveIcons)
        {
			List<RegistryKey> keyList;
			int gameCount = 0;

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(BIGFISH_GAMES, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if(key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", m_platformName.ToUpper());
					return false;
				}

				keyList = CRegHelper.FindGameFolders(key, "");

				CLogger.LogInfo("{0} {1} games found", keyList.Count, m_platformName.ToUpper());
				foreach(var data in keyList)
				{
					string wrap = Path.GetFileName(data.Name);
					if(wrap.Equals("F7315T1L1"))  // Big Fish Casino
					{
						continue;
					}

					string strID = "bfg_" + wrap;
					string strTitle = "";
					string strLaunch = "";
					string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = m_platformName;
					try
					{
						bool found = false;
						strTitle = CRegHelper.GetRegStrVal(data, "Name");

						// If this is an expired trial, count it as not-installed
						int activated = (int)CRegHelper.GetRegDWORDVal(data, BIGFISH_ACTIV);
						int daysLeft = (int)CRegHelper.GetRegDWORDVal(data, BIGFISH_DAYS);
						int timeLeft = (int)CRegHelper.GetRegDWORDVal(data, BIGFISH_TIME);
						if(activated > 0 || timeLeft > 0 || daysLeft > 0)
						{
							found = true;
							CLogger.LogDebug($"- {strTitle}");
							strLaunch = CRegHelper.GetRegStrVal(data, BIGFISH_PATH);
							strAlias = CRegHelper.GetAlias(strTitle);
							if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
							{
								strAlias = "";
							}

							List<RegistryKey> unKeyList;
							using(RegistryKey key2 = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
							{
								if(key2 != null)
								{
									unKeyList = CRegHelper.FindGameFolders(key2, BIGFISH_GAME_FOLDER);
									foreach(var data2 in unKeyList)
									{
										if(CRegHelper.GetRegStrVal(data2, BIGFISH_ID).Equals(wrap))
										{
											strIconPath = CRegHelper.GetRegStrVal(data2, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
											strUninstall = CRegHelper.GetRegStrVal(data2, GAME_UNINSTALL_STRING).Trim(new char[] { ' ', '"' });
										}
									}
								}
							}
							if(string.IsNullOrEmpty(strIconPath) && expensiveIcons)
							{
								strIconPath = CDirectoryHelper.FindGameBinaryFile(Path.GetDirectoryName(strLaunch), strTitle);
							}
							if(!(string.IsNullOrEmpty(strLaunch)))
							{
								CEventDispatcher.OnGameFound(new RawGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
								gameCount++;
							}
						}

						// Add not-installed games
						/*
						if(!found)
						{
							CLogger.LogDebug($"- *{strTitle}");
							gameList.Add(new GameData(strID, strTitle, "", "", "", "", false, strPlatform));
						}
						*/
					}
					catch(Exception e)
					{
						CLogger.LogError(e);
					}
				}
			}
			return gameCount > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
            throw new NotImplementedException();
        }
    }
}
