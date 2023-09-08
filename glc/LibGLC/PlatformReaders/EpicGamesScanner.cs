using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Epic Games store
	/// This scanner uses a JSON file to access game data
	/// </summary>
	public sealed class CEpicGamesScanner : CBasePlatformScanner<CEpicGamesScanner>
    {
		private const string EPIC_ITEMS_FOLDER = @"\Epic\EpicGamesLauncher\Data\Manifests";

		private CEpicGamesScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Epic);
		}

		protected override bool GetInstalledGames(bool expensiveIcons)
        {
			int gameCount = 0;
			string dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + EPIC_ITEMS_FOLDER;

			if(!Directory.Exists(dir))
			{
				CLogger.LogInfo("{0}: Games not found in ProgramData.", m_platformName.ToUpper());
				return false;
			}
			string[] files = Directory.GetFiles(dir, "*.item", SearchOption.TopDirectoryOnly);
			CLogger.LogInfo("{0} games found", files.Count());

			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			foreach(string file in files)
			{
				string documentData = File.ReadAllText(file);

				if(string.IsNullOrEmpty(documentData))
				{
					continue;
				}

				try
				{
					using(JsonDocument document = JsonDocument.Parse(documentData, options))
					{
						string id = Path.GetFileName(file);
						string title = CJsonHelper.GetStringProperty(document.RootElement, "DisplayName");
						string launch = CJsonHelper.GetStringProperty(document.RootElement, "LaunchExecutable"); // DLCs won't have this set
						string alias = "";

						if(!string.IsNullOrEmpty(launch))
						{
							launch = Path.Combine(CJsonHelper.GetStringProperty(document.RootElement, "InstallLocation"), launch);
							alias = CRegHelper.GetAlias(CJsonHelper.GetStringProperty(document.RootElement, "MandatoryAppFolderName"));
							if(alias.Length > title.Length)
							{
								alias = CRegHelper.GetAlias(title);
							}
							if(alias.Equals(title, StringComparison.CurrentCultureIgnoreCase))
							{
								alias = "";
							}
							CEventDispatcher.OnGameFound(new RawGameData(id, title, launch, launch, "", alias, true, m_platformName));
							gameCount++;
						}
					}
				}
				catch(Exception e)
				{
					CLogger.LogError(e, string.Format("Malformed file: {0}", file));
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
