using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Oculus
	// [installed games only]
	public class PlatformOculus : IPlatform
	{
		public const GamePlatform ENUM			= GamePlatform.Oculus;
		public const string PROTOCOL			= "oculus://";
		//private const string OCULUS_UNREG		= "Oculus"; // HKLM64 Uninstall
		private const string OCULUS_LIBS		= @"SOFTWARE\Oculus VR, LLC\Oculus\Libraries"; // HKCU64
		private const string OCULUS_DB			= @"Oculus\sessions\_oaf\data.sqlite"; // AppData\Roaming
		private const string OCULUS_LIBPATH		= "OriginalPath"; // "Path" might be better, but may require converting "\\?\Volume{guid}\" to drive letter

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

        public static void Launch() => Process.Start(PROTOCOL);

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			// Get installed games
			Dictionary<ulong, string> titles = new();
			List<string> libPaths = new();
			string db = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), OCULUS_DB);

			try
			{
				using var con = new SQLiteConnection($"Data Source={db}");
				con.Open();

				// Get both installed and not-installed games

				using (var cmd = new SQLiteCommand(string.Format("SELECT hashkey, value FROM Objects WHERE typename = 'Application'"), con))
				using (SQLiteDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
                        _ = ulong.TryParse(rdr.GetString(0), out ulong id);
						//SQLiteBlob val = rdr.GetBlob(1, true);
						//val.Read(buffer, count, offset);
						//val.Close();
						byte[] val = new byte[rdr.GetBytes(1, 0, null, 0, int.MaxValue) - 1];
						rdr.GetBytes(1, 0, val, 0, val.Length);
						string strVal = System.Text.Encoding.Default.GetString(val);
						int i = strVal.IndexOf("display_name");
						int j = strVal.IndexOf("display_short_description");
						if (i > 0 && j > 0)
						{
							i += 22;
							j -= 5;
							if (j - i < 1)
								j = i + 1;
							titles[id] = strVal[i..j];
						}
					}
				}
			}
			catch (Exception e)
            {
				CLogger.LogError(e);
            }

			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(OCULUS_LIBS, RegistryKeyPermissionCheck.ReadSubTree))
			{
				if (key != null)
				{
					foreach (string lib in key.GetSubKeyNames())
					{
                        using RegistryKey key2 = Registry.CurrentUser.OpenSubKey(Path.Combine(OCULUS_LIBS, lib), RegistryKeyPermissionCheck.ReadSubTree);
                        libPaths.Add(GetRegStrVal(key2, OCULUS_LIBPATH));
                    }
				}
			}
			foreach (string lib in libPaths)
			{
				List<string> libFiles = new();
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
                            using JsonDocument document = JsonDocument.Parse(@strDocumentData, options);
                            CultureInfo ci = new("en-GB");
                            TextInfo ti = ci.TextInfo;

                            string strID = "";
                            string strTitle = "";
                            string strLaunch = "";
                            string strAlias = "";
                            string strPlatform = GetPlatformString(GamePlatform.Oculus);

							string name = GetStringProperty(document.RootElement, "canonicalName");
							if (ulong.TryParse(GetStringProperty(document.RootElement, "appId"), out ulong id))
								strID = "oculus_" + id;
							else
								strID = "oculus_" + name;
							string exePath = GetStringProperty(document.RootElement, "launchFile");
							if (!string.IsNullOrEmpty(exePath))
							{
								strTitle = titles[id];
								if (string.IsNullOrEmpty(strTitle))
									strTitle = ti.ToTitleCase(name.Replace('-', ' '));
								CLogger.LogDebug($"- {strTitle}");
								strLaunch = Path.Combine(lib, "Software", name, exePath);
								strAlias = GetAlias(Path.GetFileNameWithoutExtension(exePath));
								if (strAlias.Length > strTitle.Length)
									strAlias = GetAlias(strTitle);
								if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
									strAlias = "";
								gameDataList.Add(new ImportGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
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