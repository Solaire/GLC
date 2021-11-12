using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
using Logger;
using System.Text.Json;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Paradox Store (Paradox Interactive)
	/// This scanner uses the Registry to access game data
	/// </summary>
	public sealed class CParadoxScanner : CBasePlatformScanner<CParadoxScanner>
    {
		private const string PARADOX_NAME = "Paradox";
		private const string PARADOX_REG = @"SOFTWARE\WOW6432Node\Paradox Interactive\Paradox Launcher\LauncherPath"; //HKLM32
		private const string PARADOX_PATH = "Path";
		private const string PARADOX_JSON_FOLDER = @"\Paradox Interactive\launcher";

		private CParadoxScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Paradox);
		}

		protected override bool GetInstalledGames(bool expensiveIcons)
        {
			List<string> dirs = new List<string>();
			int gameCount = 0;

			// Get installed games
			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(PARADOX_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if(key == null)
                {
					CLogger.LogInfo("Client not found in the registry.", m_platformName.ToUpper());
					return false;
				}

				string path = key.GetValue(PARADOX_PATH).ToString();
				try
				{
					if(!path.Equals(null) && Directory.Exists(path))
					{
						dirs.AddRange(Directory.GetDirectories(Directory.GetParent(Directory.GetParent(path).ToString()) + "\\games", "*.*", SearchOption.TopDirectoryOnly));
						foreach(string dir in dirs)
						{
							CultureInfo ci = new CultureInfo("en-GB");
							TextInfo ti = ci.TextInfo;

							string strID = Path.GetFileName(dir);
							string strTitle = "";
							string strLaunch = "";
							string strAlias = "";

							strTitle = ti.ToTitleCase(strID.Replace('_', ' '));
							CLogger.LogDebug($"- {strTitle}");
							strLaunch = CDirectoryHelper.FindGameBinaryFile(dir, strTitle);
							strAlias = CRegHelper.GetAlias(strLaunch);
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
								CEventDispatcher.OnGameFound(new RawGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, m_platformName));
								gameCount++;
							}
						}
					}
				}
				catch(Exception e)
				{
					CLogger.LogError(e);
				}
			}
			return gameCount > 0;
		}

		//TODO:
        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + PARADOX_JSON_FOLDER;
			if(!Directory.Exists(folder))
			{
				CLogger.LogInfo("Games not found in Local AppData.", PARADOX_NAME.ToUpper());
				return false;
			}

			int gameCount = 0;
			string[] files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			foreach(string file in files)
			{
				if(file.EndsWith("_installableGames.json") && !(file.StartsWith("_noUser")))
				{
					string strDocumentData = File.ReadAllText(file);
					if(string.IsNullOrEmpty(strDocumentData))
                    {
						continue;
					}

					try
					{
						using(JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
						{
							document.RootElement.TryGetProperty("content", out JsonElement content);
							if(content.Equals(null))
							{
								return false;
							}

							List<string> dirs = new List<string>();
							foreach(JsonElement game in content.EnumerateArray())
							{
								game.TryGetProperty("_name", out JsonElement id);

								// Check if game is already installed
								bool found = false;
								foreach(string dir in dirs)
								{
									if(id.ToString().Equals(Path.GetFileName(dir)))
                                    {
										found = true;
									}
								}
								if(!found)
								{
									game.TryGetProperty("_displayName", out JsonElement title);
									game.TryGetProperty("_owned", out JsonElement owned);
									if(!id.Equals(null) && !title.Equals(null) && owned.ToString().ToLower().Equals("true"))
									{
										string strID = id.ToString();
										string strTitle = title.ToString();
										CEventDispatcher.OnGameFound(new RawGameData(strID, strTitle, "", "", "", "", false, m_platformName));
										gameCount++;
									}
								}
							}
						}
					}
					catch(Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} file: {1}", PARADOX_NAME.ToUpper(), file));
					}
				}
			}
			return gameCount > 0;
		}
	}
}
