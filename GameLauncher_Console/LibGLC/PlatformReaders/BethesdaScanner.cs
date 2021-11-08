using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Bethesda.NET
	/// </summary>
    public sealed class CBethesdaScanner : CBasePlatformScanner<CBethesdaScanner>
    {
		private const string BETHESDA_NAME           = "Bethesda";
		private const string BETHESDA_NAME_LONG      = "Bethesda.net Launcher";
		private const string BETHESDA_NET           = "bethesda.net";
		private const string BETHESDA_PATH          = "Path";
		private const string BETHESDA_CREATION_KIT  = "Creation Kit";
		private const string BETHESDA_LAUNCH        = "bethesda://run/";
		private const string BETHESDA_PRODUCT_ID    = "ProductID";
		//private const string BETHESDA_UNREG			= "{3448917E-E4FE-4E30-9502-9FD52EABB6F5}_is1"; // HKLM32 Uninstall
		//private const string BETHESDA_REG			= @"SOFTWARE\WOW6432Node\Bethesda Softworks\Bethesda.net"; // HKLM32

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			List<RegistryKey> keyList; //= new List<RegistryKey>();
			int gameCount = 0;

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if(key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", BETHESDA_NAME.ToUpper());
					return false;
				}

				keyList = CRegHelper.FindGameKeys(key, BETHESDA_NET, BETHESDA_PATH, new string[] { BETHESDA_CREATION_KIT });

				CLogger.LogInfo("{0} {1} games found", keyList.Count, BETHESDA_NAME.ToUpper());
				foreach(var data in keyList)
				{
					string loc = CRegHelper.GetRegStrVal(data, BETHESDA_PATH);

					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = "Bethesda";// CGameData.GetPlatformString(CGameData.GamePlatform.Bethesda);
					try
					{
						strID = Path.GetFileName(data.Name);
						strTitle = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_NAME);
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = BETHESDA_LAUNCH + CRegHelper.GetRegStrVal(data, BETHESDA_PRODUCT_ID);
						strIconPath = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						if(string.IsNullOrEmpty(strIconPath))
						{
							strIconPath = Path.Combine(loc.Trim(new char[] { ' ', '"' }), string.Concat(strTitle.Split(Path.GetInvalidFileNameChars())) + ".exe");
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
						//gameList.Add(new GameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
						NewGameFound(new RawGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
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
