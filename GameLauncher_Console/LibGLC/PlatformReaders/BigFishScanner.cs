using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Logger;
using System.Linq;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for BigFish games
	/// This scanner uses the Registry to access game data
	/// </summary>
	public sealed class CBigFishScanner : CBasePlatformScanner<CBigFishScanner>
    {
		private const string BIGFISH_GAME_FOLDER    = "BFG-";
		private const string BIGFISH_GAMES          = @"SOFTWARE\WOW6432Node\Big Fish Games\Persistence\GameDB"; // HKLM32
		private const string BIGFISH_ID             = "WrapID";
		private const string BIGFISH_PATH           = "ExecutablePath";
		private const string BIGFISH_ACTIV          = "Activated";
		private const string BIGFISH_DAYS           = "DaysLeft";
		private const string BIGFISH_TIME           = "TimeLeft";

		private readonly string[] IGNORE =
		{
			"F7315T1L1" // Big Fish casino
		};

		private CBigFishScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.BigFish);
		}

		/// <summary>
		/// Override.
		/// No easy way of splitting the function into installed and non-installed
		/// </summary>
		/// <param name="getNonInstalled">If true, try to get non-installed games</param>
		/// <param name="expensiveIcons">If true, try to get expensive icons</param>
		/// <returns></returns>
		public override bool GetGames(bool getNonInstalled, bool expensiveIcons)
        {
			CEventDispatcher.OnPlatformStarted(m_platformName);

			List<RegistryKey> keyList = new List<RegistryKey>();
			int gameCount = 0;

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(BIGFISH_GAMES, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if(key == null)
				{
					CLogger.LogInfo("{0}: Client not found in the registry.", m_platformName.ToUpper());
					return false;
				}

				keyList = CRegHelper.FindGameFolders(key, "");

				CLogger.LogInfo("{0} games found", keyList.Count);
				foreach(var data in keyList)
				{
					string wrap = Path.GetFileName(data.Name);
					if(IGNORE.Contains(wrap))
					{
						continue;
					}

					string id = "bfg_" + wrap;
					string title = "";
					string launch = "";
					string iconPath = "";
					string uninstall = "";
					string alias = "";
					try
					{
						bool found = false;
						title = CRegHelper.GetRegStrVal(data, "Name");

						// If this is an expired trial, count it as not-installed
						int activated = (int)CRegHelper.GetRegDWORDVal(data, BIGFISH_ACTIV);
						int daysLeft = (int)CRegHelper.GetRegDWORDVal(data, BIGFISH_DAYS);
						int timeLeft = (int)CRegHelper.GetRegDWORDVal(data, BIGFISH_TIME);
						if(activated > 0 || timeLeft > 0 || daysLeft > 0)
						{
							found = true;
							launch = CRegHelper.GetRegStrVal(data, BIGFISH_PATH);
							alias = CRegHelper.GetAlias(title);
							if(alias.Equals(title, StringComparison.CurrentCultureIgnoreCase))
							{
								alias = "";
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
											iconPath = CRegHelper.GetRegStrVal(data2, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
											uninstall = CRegHelper.GetRegStrVal(data2, GAME_UNINSTALL_STRING).Trim(new char[] { ' ', '"' });
										}
									}
								}
							}
							if(string.IsNullOrEmpty(iconPath) && expensiveIcons)
							{
								iconPath = CDirectoryHelper.FindGameBinaryFile(Path.GetDirectoryName(launch), title);
							}
							if(!(string.IsNullOrEmpty(launch)))
							{
								CEventDispatcher.OnGameFound(new RawGameData(id, title, launch, iconPath, uninstall, alias, true, m_platformName));
								gameCount++;
							}
						}

						// Add not-installed games
						if(getNonInstalled && !found)
						{
							CEventDispatcher.OnGameFound(new RawGameData(id, title, "", "", "", "", false, m_platformName));
							gameCount++;
						}
					}
					catch(Exception e)
					{
						CLogger.LogError(e);
					}
				}
			}
			return gameCount > 0;
		}

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			return false;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			return false;
        }
    }
}
