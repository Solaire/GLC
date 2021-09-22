using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CRegScanner;

namespace GameLauncher_Console
{
	// Bethesda.net Launcher
	// [installed games only]
	public class PlatformBethesda : IPlatform
    {
		public const GamePlatform ENUM	= GamePlatform.Bethesda;
		public const string PROTOCOL				= "bethesdanet://";
		private const string START_GAME				= PROTOCOL + "run";
		private const string BETHESDA_NET			= "bethesda.net";
		private const string BETHESDA_PATH			= "Path";
		private const string BETHESDA_CREATION_KIT	= "Creation Kit";
		private const string BETHESDA_PRODUCT_ID	= "ProductID";
		//private const string BETHESDA_REG			= @"SOFTWARE\WOW6432Node\Bethesda Softworks\Bethesda.net"; // HKLM32
		//private const string BETHESDA_UNREG		= "{3448917E-E4FE-4E30-9502-9FD52EABB6F5}_is1"; // HKLM32 Uninstall

		private static string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

        string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

        public static void Launch() => Process.Start(PROTOCOL);

		public static void InstallGame(CGame game) => throw new NotImplementedException();

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			List<RegistryKey> keyList; //= new List<RegistryKey>();

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", _name.ToUpper());
					return;
				}

				keyList = FindGameKeys(key, BETHESDA_NET, BETHESDA_PATH, new string[] { BETHESDA_CREATION_KIT });

				CLogger.LogInfo("{0} {1} games found", keyList.Count, _name.ToUpper());
				foreach (var data in keyList)
				{
					string loc = GetRegStrVal(data, BETHESDA_PATH);

					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = GetPlatformString(GamePlatform.Bethesda);
					try
					{
						strID = Path.GetFileName(data.Name);
						strTitle = GetRegStrVal(data, GAME_DISPLAY_NAME);
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = START_GAME + "/" + GetRegStrVal(data, BETHESDA_PRODUCT_ID);
						strIconPath = GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						if (string.IsNullOrEmpty(strIconPath))
							strIconPath = Path.Combine(loc.Trim(new char[] { ' ', '"' }), string.Concat(strTitle.Split(Path.GetInvalidFileNameChars())) + ".exe");
						strUninstall = GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
						strAlias = GetAlias(Path.GetFileNameWithoutExtension(loc.Trim(new char[] { ' ', '\'', '"' })));
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
							new ImportGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
				}
			}
			CLogger.LogDebug("------------------------");
		}
	}
}