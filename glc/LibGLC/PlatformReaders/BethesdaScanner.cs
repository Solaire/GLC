using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Bethesda.NET
	/// This scanner uses the Registry to access game data
	/// </summary>
	public sealed class CBethesdaScanner : CBasePlatformScanner<CBethesdaScanner>
    {
		private const string BETHESDA_NET           = "bethesda.net";
		private const string BETHESDA_PATH          = "Path";
		private const string BETHESDA_CREATION_KIT  = "Creation Kit";
		private const string BETHESDA_LAUNCH        = "bethesda://run/";
		private const string BETHESDA_PRODUCT_ID    = "ProductID";

		private CBethesdaScanner()
        {
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Bethesda);
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

				keyList = CRegHelper.FindGameKeys(key, BETHESDA_NET, BETHESDA_PATH, new string[] { BETHESDA_CREATION_KIT });

				CLogger.LogInfo("{0} games found", keyList.Count);
				foreach(var data in keyList)
				{
					string loc = CRegHelper.GetRegStrVal(data, BETHESDA_PATH);

					string id = "";
					string title = "";
					string launch = "";
					string iconPath = "";
					string uninstall = "";
					string alias = "";
					try
					{
						id = Path.GetFileName(data.Name);
						title = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_NAME);
						launch = BETHESDA_LAUNCH + CRegHelper.GetRegStrVal(data, BETHESDA_PRODUCT_ID);
						iconPath = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						if(string.IsNullOrEmpty(iconPath))
						{
							iconPath = Path.Combine(loc.Trim(new char[] { ' ', '"' }), string.Concat(title.Split(Path.GetInvalidFileNameChars())) + ".exe");
						}
						uninstall = CRegHelper.GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
						alias = CRegHelper.GetAlias(Path.GetFileNameWithoutExtension(loc.Trim(new char[] { ' ', '\'', '"' })));
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
						CEventDispatcher.OnGameFound(new RawGameData(id, title, launch, iconPath, uninstall, alias, true, m_platformName));
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
