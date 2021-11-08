using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Origin (EA games)
	/// </summary>
    public sealed class COriginScanner : CBasePlatformScanner<COriginScanner>
    {
		private const string ORIGIN_NAME      = "Origin"; //"EA"
		private const string ORIGIN_NAME_LONG = "Origin"; //"EA Desktop";
		private const string ORIGIN_CONTENT   = @"\Origin\LocalContent";
		private const string ORIGIN_PATH      = "dipinstallpath=";

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			int gameCount = 0;
			List<RegistryKey> keyList = new List<RegistryKey>();
			List<string> dirs = new List<string>();
			string path = "";
			try
			{
				path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + ORIGIN_CONTENT;
				if(Directory.Exists(path))
				{
					dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly));
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e, string.Format("{0} directory read error: {1}", ORIGIN_NAME.ToUpper(), path));
			}

			CLogger.LogInfo("{0} {1} games found", dirs.Count, ORIGIN_NAME.ToUpper());
			foreach(string dir in dirs)
			{
				string[] files = { };
				string install = "";

				string strID = Path.GetFileName(dir);
				string strTitle = strID;
				string strLaunch = "";
				//string strIconPath = "";
				string strUninstall = "";
				string strAlias = "";
				string strPlatform = "Origin";//CGameData.GetPlatformString(CGameData.GamePlatform.Origin);

				try
				{
					files = Directory.GetFiles(dir, "*.mfst", SearchOption.TopDirectoryOnly);
				}
				catch(Exception e)
				{
					CLogger.LogError(e);
				}

				foreach(string file in files)
				{
					try
					{
						string strDocumentData = File.ReadAllText(file);
						string[] subs = strDocumentData.Split('&');
						foreach(string sub in subs)
						{
							if(sub.StartsWith(ORIGIN_PATH))
								install = sub.Substring(15);
						}
					}
					catch(Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} file: {1}", ORIGIN_NAME.ToUpper(), file));
					}
				}

				if(!string.IsNullOrEmpty(install))
				{
					install = Uri.UnescapeDataString(install);

					using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
					{
						if(key != null)
						{
							keyList = CRegHelper.FindGameKeys(key, install, GAME_INSTALL_LOCATION, new string[] { ORIGIN_NAME });
							foreach(var data in keyList)
							{
								strTitle = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_NAME);
								strLaunch = CRegHelper.GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
								strUninstall = CRegHelper.GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
							}
						}
					}

					CLogger.LogDebug($"- {strTitle}");
					if(string.IsNullOrEmpty(strLaunch))
					{
						strLaunch = CDirectoryHelper.FindGameBinaryFile(install, strTitle);
					}
					strAlias = CRegHelper.GetAlias(Path.GetFileNameWithoutExtension(install));
					if(strAlias.Length > strTitle.Length)
					{
						strAlias = CRegHelper.GetAlias(strTitle);
					}
					if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
					{
						strAlias = "";
					}

					if(!(string.IsNullOrEmpty(strLaunch)))
					{
						//gameList.Add(new GameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
						NewGameFound(new RawGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
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
