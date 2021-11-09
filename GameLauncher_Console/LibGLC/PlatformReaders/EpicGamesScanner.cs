using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Epic Games store
	/// </summary>
    public sealed class CEpicGamesScanner : CBasePlatformScanner<CEpicGamesScanner>
    {
		private const string EPIC_NAME = "Epic";
		private const string EPIC_ITEMS_FOLDER = @"\Epic\EpicGamesLauncher\Data\Manifests";

		private CEpicGamesScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Epic);
		}

		protected override bool GetInstalledGames(bool expensiveIcons)
        {
			int found = 0;
			string dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + EPIC_ITEMS_FOLDER;
			if(!Directory.Exists(dir))
			{
				CLogger.LogInfo("{0} games not found in ProgramData.", m_platformName.ToUpper());
				return false;
			}
			string[] files = Directory.GetFiles(dir, "*.item", SearchOption.TopDirectoryOnly);
			CLogger.LogInfo("{0} {1} games found", files.Count(), m_platformName.ToUpper());

			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			foreach(string file in files)
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
						string strID = Path.GetFileName(file);
						string strTitle = CJsonHelper.GetStringProperty(document.RootElement, "DisplayName");
						CLogger.LogDebug($"- {strTitle}");
						string strLaunch = CJsonHelper.GetStringProperty(document.RootElement, "LaunchExecutable"); // DLCs won't have this set
						string strAlias = "";
						string strPlatform = m_platformName;

						if(!string.IsNullOrEmpty(strLaunch))
						{
							strLaunch = Path.Combine(CJsonHelper.GetStringProperty(document.RootElement, "InstallLocation"), strLaunch);
							strAlias = CRegHelper.GetAlias(CJsonHelper.GetStringProperty(document.RootElement, "MandatoryAppFolderName"));
							if(strAlias.Length > strTitle.Length)
							{
								strAlias = CRegHelper.GetAlias(strTitle);
							}
							if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
							{
								strAlias = "";
							}
							CEventDispatcher.OnGameFound(new RawGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
							found++;
						}
					}
				}
				catch(Exception e)
				{
					CLogger.LogError(e, string.Format("Malformed {0} file: {1}", m_platformName.ToUpper(), file));
				}
			}
			return found > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			return false;
        }
    }
}
