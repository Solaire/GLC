using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Ubisoft Connect (formerly Uplay)
	/// This scanner uses the Registry to access game data
	/// </summary>
	public sealed class CUbisoftScanner : CBasePlatformScanner<CUbisoftScanner>
    {
		private const string UPLAY_INSTALL   = "Uplay Install ";
		private const string UPLAY_LAUNCH    = "uplay://launch/";

		private CUbisoftScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Uplay);
		}

		protected override bool GetInstalledGames(bool expensiveIcons)
        {
			int gameCount = 0;
			List<RegistryKey> keyList = new List<RegistryKey>();
			List<string> uplayIds	  = new List<string>();
			List<string> uplayIcons   = new List<string>();
			string uplayLoc = "";

			using(RegistryKey uplayKey = Registry.LocalMachine.OpenSubKey(NODE32_REG + "\\Uplay", RegistryKeyPermissionCheck.ReadSubTree))
			{
				if(uplayKey == null)
				{
					CLogger.LogInfo("Client not found in the registry.");
					return false;
				}
				uplayLoc = CRegHelper.GetRegStrVal(uplayKey, GAME_INSTALL_LOCATION);
			}

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				keyList = CRegHelper.FindGameFolders(key, UPLAY_INSTALL);
				CLogger.LogInfo("{0} games found", keyList.Count);

				foreach(var data in keyList)
				{
					string loc = CRegHelper.GetRegStrVal(data, GAME_INSTALL_LOCATION);

					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";

					try
					{
						strID = Path.GetFileName(data.Name);
						uplayIds.Add(strID);
						strTitle = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_NAME);
						strLaunch = UPLAY_LAUNCH + GetUplayGameID(strID);
						strIconPath = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						uplayIcons.Add(strIconPath);
						if(string.IsNullOrEmpty(strIconPath) && expensiveIcons)
						{
							strIconPath = CDirectoryHelper.FindGameBinaryFile(loc, strTitle);
						}
						strUninstall = CRegHelper.GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
						strAlias = CRegHelper.GetAlias(Path.GetFileNameWithoutExtension(loc.Trim(new char[] { ' ', '\'', '"' })));
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
						CEventDispatcher.OnGameFound(new RawGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, m_platformName));
						gameCount++;
					}
				}
			}
			return gameCount > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			string uplayLoc = "";
			int gameCount = 0;

			using(RegistryKey uplayKey = Registry.LocalMachine.OpenSubKey(NODE32_REG + "\\Uplay", RegistryKeyPermissionCheck.ReadSubTree))
			{
				if(uplayKey == null)
				{
					CLogger.LogInfo("Client not found in the registry.", m_platformName.ToUpper());
					return false;
				}
				uplayLoc = CRegHelper.GetRegStrVal(uplayKey, GAME_INSTALL_LOCATION);
			}

			string uplayCfgFile = Path.Combine(uplayLoc, @"cache\configuration\configurations");
			if(!File.Exists(uplayCfgFile))
			{
				return false;
			}

			try
			{
				char[] trimChars = { ' ', '\'', '"' };
				List<string> uplayCfg	= new List<string>();
				List<string> uplayIds	= new List<string>();
				List<string> uplayIcons = new List<string>();
				uplayCfg.AddRange(File.ReadAllLines(uplayCfgFile));

				int nGames = 0;
				bool dlc = false;
				string strID = "";
				string strTitle = "";
				string strIconPath = "";

				foreach(string line in uplayCfg)  // Note if the last game is valid, this method won't catch it; but that appears very unlikely
				{
					if(line.Trim().StartsWith("root:"))
					{
						if(nGames > 0 && !(string.IsNullOrEmpty(strID)) && dlc == false)
						{
							bool found = false;
							foreach(string id in uplayIds)
							{
								if(id.Equals(strID))
                                {
									found = true;
								}
							}
							if(!found)
							{
								// The Uplay ID is not always available, so this is an alternative test
								// However, if user has e.g., a demo and the full game installed, this might get a false negative
								foreach(string icon in uplayIcons)
								{
									if(Path.GetFileName(icon).Equals(Path.GetFileName(strIconPath)))
                                    {
										found = true;
									}
								}
							}
							if(!found)
							{
								strTitle = strTitle.Replace("''", "'");
								CEventDispatcher.OnGameFound(new RawGameData(strID, strTitle, "", strIconPath, "", "", false, m_platformName));
								gameCount++;
							}
						}

						nGames++;
						dlc = false;
						strID = "";
						strTitle = "";
						strIconPath = "";
					}

					if(dlc == true)
                    {
						continue;
					}
					else if(line.Trim().StartsWith("is_dlc: yes"))
					{
						dlc = true;
						continue;
					}
					else if(string.IsNullOrEmpty(strTitle) && line.Trim().StartsWith("name: "))
                    {
						strTitle = line.Substring(line.IndexOf("name:") + 6).Trim(trimChars);
					}
					else if(line.Trim().StartsWith("display_name: "))  // replace "name:" if it exists
                    {
						strTitle = line.Substring(line.IndexOf("display_name:") + 14).Trim(trimChars);
					}
					else if(strTitle.Equals("NAME") && line.Trim().StartsWith("NAME: "))
                    {
						strTitle = line.Substring(line.IndexOf("NAME:") + 6).Trim(trimChars);
					}
					else if(strTitle.Equals("GAMENAME") && line.Trim().StartsWith("GAMENAME: "))
                    {
						strTitle = line.Substring(line.IndexOf("GAMENAME:") + 10).Trim(trimChars);
					}
					else if(strTitle.Equals("l1") && line.Trim().StartsWith("l1: "))
                    {
						strTitle = line.Substring(line.IndexOf("l1:") + 4).Trim(trimChars);
					}

					else if(line.Trim().StartsWith("icon_image: ") && line.Trim().EndsWith(".ico"))
                    {
						strIconPath = Path.Combine(Path.Combine(uplayLoc, "data\\games"), line.Substring(line.IndexOf("icon_image:") + 12).Trim());
					}
					else if(string.IsNullOrEmpty(strID))
					{
						if(line.Trim().StartsWith("game_code: ") && !(strID.StartsWith(UPLAY_INSTALL)))
                        {
							strID = "uplay_" + line.Substring(line.IndexOf("game_code:") + 11).Trim();
						}
						else if(line.Trim().StartsWith(@"register: HKEY_LOCAL_MACHINE\SOFTWARE\Ubisoft\Launcher\Installs\"))
						{
							strID = line.Substring(0, line.LastIndexOf("\\")).Trim();
							strID = UPLAY_INSTALL + strID.Substring(strID.LastIndexOf("\\") + 1);
						}
					}
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed file: {1}", uplayCfgFile));
			}

			return gameCount > 0;
		}

		/// <summary>
		/// Retrieve Ubisoft game ID from a game key string
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>String representing the Ubisoft game ID</returns>
		private string GetUplayGameID(string key)
		{
			int index = 0;
			for(int i = key.Length - 1; i > -1; i--)
			{
				if(char.IsDigit(key[i]))
				{
					index = i;
					continue;
				}
				break;
			}
			return key.Substring(index);
		}
	}
}
