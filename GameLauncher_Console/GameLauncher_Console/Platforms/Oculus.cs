using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
using static GameLauncher_Console.CRegScanner;

namespace GameLauncher_Console
{
	// Oculus
	// [installed games only]
	public class PlatformOculus : IPlatform
	{
		public const GamePlatform ENUM = GamePlatform.Oculus;
		public const string PROTOCOL			= "oculus://";
		//private const string OCULUS_UNREG		= "Oculus"; // HKLM64 Uninstall
		private const string OCULUS_LIBS		= @"SOFTWARE\Oculus VR, LLC\Oculus\Libraries"; // HKCU64
		private const string OCULUS_LIBPATH		= "OriginalPath"; // "Path" might be better, but may require converting "\\?\Volume{guid}\" to drive letter

		private static string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

        public static void Launch() => Process.Start(PROTOCOL);

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			// Get installed games
			List<string> libPaths = new List<string>();

			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(OCULUS_LIBS, RegistryKeyPermissionCheck.ReadSubTree))
			{
				if (key != null)
				{
					foreach (string lib in key.GetSubKeyNames())
					{
						using (RegistryKey key2 = Registry.CurrentUser.OpenSubKey(Path.Combine(OCULUS_LIBS, lib), RegistryKeyPermissionCheck.ReadSubTree))
						{
							libPaths.Add(GetRegStrVal(key2, OCULUS_LIBPATH));
						}
					}
				}
			}
			foreach (string lib in libPaths)
			{
				List<string> libFiles = new List<string>();
				try
				{
					string manifestPath = Path.Combine(lib, "Manifests");
					libFiles = Directory.GetFiles(manifestPath, "*.json.mini", SearchOption.TopDirectoryOnly).ToList();
					CLogger.LogInfo("{0} {1} games found in library {2}", libFiles.Count, _name.ToUpper(), lib);
				}
				catch (Exception e)
				{
					CLogger.LogError(e, string.Format("{0} directory read error: {1}", _name.ToUpper(), lib));
					continue;
				}

				foreach (string file in libFiles)
				{
					try
					{
						var options = new JsonDocumentOptions
						{
							AllowTrailingCommas = true
						};

						string strDocumentData = File.ReadAllText(file);

						if (string.IsNullOrEmpty(strDocumentData))
							CLogger.LogWarn(string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
						else
						{
							using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
							{
								CultureInfo ci = new CultureInfo("en-GB");
								TextInfo ti = ci.TextInfo;

								string strID = "";
								string strTitle = "";
								string strLaunch = "";
								string strAlias = "";
								string strPlatform = GetPlatformString(GamePlatform.Oculus);

								//ulong id = GetULongProperty(document.RootElement, "appId");
								string name = GetStringProperty(document.RootElement, "canonicalName");
								//if (id > 0)
								if (!string.IsNullOrEmpty(name))
								{
									strID = name + ".json";
									string exefile = GetStringProperty(document.RootElement, "launchFile");
									strTitle = ti.ToTitleCase(name.Replace('-', ' '));
									CLogger.LogDebug($"- {strTitle}");
									strLaunch = Path.Combine(lib, "Software", name, exefile);
									strAlias = GetAlias(Path.GetFileNameWithoutExtension(exefile));
									if (strAlias.Length > strTitle.Length)
										strAlias = GetAlias(strTitle);
									if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
										strAlias = "";
									gameDataList.Add(new ImportGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
								}
							}
						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
					}
				}
			}
			CLogger.LogDebug("--------------------");
		}
	}
}