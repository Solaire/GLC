using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Arc
	// [installed games only]
	public class PlatformArc : IPlatform
	{
		public const GamePlatform ENUM			= GamePlatform.Arc;
		public const string PROTOCOL			= "arc://";
		private const string ARC_REG			= @"SOFTWARE\WOW6432Node\Perfect World Entertainment"; // HKLM32
		//private const string ARC_UNREG		= "{CED8E25B-122A-4E80-B612-7F99B93284B3}"; // HKLM32 Uninstall
		private const string ARC_GAMES			= "Core";
		private const string ARC_ID				= "APP_ABBR";
		private const string ARC_PATH			= "INSTALL_PATH";
		private const string ARC_EXEPATH		= "CLIENT_PATH";
		private const string ARC_INST			= "installed";
		//[strLaunch] CLIENT_PATH in e.g., HKLM\SOFTWARE\WOW6432Node\Perfect World Entertainment\Core\1400en
		//[strId?] APP_ABBR
		//[installed] installed

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

		string IPlatform.Description => GetPlatformString(ENUM);

		public static void Launch()
		{
			if (OperatingSystem.IsWindows())
				CDock.StartShellExecute(PROTOCOL);
			else
				Process.Start(PROTOCOL);
		}

		public static void InstallGame(CGame game) => throw new NotImplementedException();

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			List<RegistryKey> keyList = new();
			//string arcFolder = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), "Arc"); // AppData\Roaming
			/*
			string launcherPath = "";

			using (RegistryKey launcherKey = Registry.LocalMachine.OpenSubKey(Path.Combine(ARC_REG, "Arc"), RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (launcherKey == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", _name.ToUpper());
					return;
				}
				launcherPath = GetRegStrVal(launcherKey, "client");
			}
			*/

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Path.Combine(ARC_REG, ARC_GAMES), RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				foreach (string subKey in key.GetSubKeyNames()) // Add subkeys to search list
				{
					try
					{
						if (subKey.IndexOf("en") > -1)
							keyList.Add(key.OpenSubKey(subKey, RegistryKeyPermissionCheck.ReadSubTree));
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
				}
				CLogger.LogInfo("{0} {1} games found", keyList.Count, _name.ToUpper());
				foreach (var data in keyList)
                {
					string id = GetRegStrVal(data, ARC_ID);
					if (string.IsNullOrEmpty(id))
					{
						id = Path.GetFileName(data.Name);
						int idIndex = id.IndexOf("en");
						if (idIndex > -1)
							id = id.Substring(0, idIndex);
					}
					string name = Path.GetFileName(GetRegStrVal(data, ARC_PATH).Trim(new char[] { '"', '\\', '/' }));
					int nameIndex = name.IndexOf("_en");
					if (nameIndex > -1)
						name = name.Substring(0, nameIndex);
					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					string strAlias = "";
					bool bInstalled = true;
					string strPlatform = GetPlatformString(ENUM);

					try
					{
						strID = "arc_" + id;
						if (!string.IsNullOrEmpty(name))
							strTitle = name;
						else
							strTitle = id;
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = GetRegStrVal(data, ARC_EXEPATH);
						strAlias = GetAlias(Path.GetFileNameWithoutExtension(strLaunch));
						if (strAlias.Length > strTitle.Length)
							strAlias = GetAlias(strTitle);
						if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
							strAlias = "";
						int? installed = GetRegDWORDVal(data, ARC_INST);
						if (installed != null && installed == 0)
							bInstalled = false;
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
					if (!(string.IsNullOrEmpty(strLaunch)))
						gameDataList.Add(
							new ImportGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, bInstalled, strPlatform));
				}
			}
			CLogger.LogDebug("------------------------");
		}
	}
}