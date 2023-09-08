using System;
using System.IO;
using System.Text.Json;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Indiegala
	/// This scanner uses a JSON file to access game data
	/// </summary>
	public sealed class CIndiegalaScanner : CBasePlatformScanner<CIndiegalaScanner>
    {
		private const string IG_JSON_FILE       = @"\IGClient\storage\installed.json";
		private const string IG_OWN_JSON_FILE   = @"\IGClient\config.json";

		private CIndiegalaScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.IGClient);
		}

		protected override bool GetInstalledGames(bool expensiveIcons)
        {
			int found = 0;

			// Get installed games
			string file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + IG_JSON_FILE;
			if(!File.Exists(file))
			{
				CLogger.LogInfo("{0}: Installed games not found in AppData", m_platformName.ToUpper());
				return false;
			}

			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			string strDocumentData = File.ReadAllText(file);
			if(string.IsNullOrEmpty(strDocumentData))
			{
				CLogger.LogWarn(string.Format("Malformed file: {0}", file));
				return false;
			}

			try
			{
				using(JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					foreach(JsonElement element in document.RootElement.EnumerateArray())
					{
						string id = "";
						string title = "";
						string launch = "";
						string alias = "";

						element.TryGetProperty("target", out JsonElement target);
						if(!target.Equals(null))
						{
							target.TryGetProperty("item_data", out JsonElement item);
							if(!item.Equals(null))
							{
								id = CJsonHelper.GetStringProperty(item, "id_key_name");
								title = CJsonHelper.GetStringProperty(item, "name");
							}
						}
						element.TryGetProperty("path", out JsonElement paths);
						if(!paths.Equals(null))
						{
							foreach(JsonElement path in paths.EnumerateArray())
							{
								launch = CDirectoryHelper.FindGameBinaryFile(path.ToString(), title);
							}
						}

						if(!string.IsNullOrEmpty(launch))
						{
							alias = CRegHelper.GetAlias(Path.GetFileNameWithoutExtension(launch));
							if(alias.Length > title.Length)
							{
								alias = CRegHelper.GetAlias(title);
							}
							if(alias.Equals(title, StringComparison.CurrentCultureIgnoreCase))
							{
								alias = "";
							}
							CEventDispatcher.OnGameFound(new RawGameData(id, title, launch, launch, "", alias, true, m_platformName));
							found++;
						}
					}
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed file: {1}", file));
			}

			return found > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			string file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + IG_OWN_JSON_FILE;
			if(!File.Exists(file))
            {
				CLogger.LogInfo("{0}: not-installed games not found in AppData", m_platformName);
				return false;
			}

			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			string strDocumentData = File.ReadAllText(file);

			if(string.IsNullOrEmpty(strDocumentData))
            {
				CLogger.LogWarn(string.Format("Malformed file: {1}", file));
				return false;
			}

			int gameCount = 0;
			try
			{
				using(JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					JsonElement coll = new JsonElement();
					document.RootElement.TryGetProperty("gala_data", out JsonElement gData);
					if(gData.Equals(null))
                    {
						return false;
                    }
					gData.TryGetProperty("data", out JsonElement data);
					if(data.Equals(null))
                    {
						return false;
                    }
					data.TryGetProperty("showcase_content", out JsonElement sContent);
					if(sContent.Equals(null))
                    {
						return false;
                    }
					sContent.TryGetProperty("content", out JsonElement content);
					if(content.Equals(null))
                    {
						return false;
                    }
					content.TryGetProperty("user_collection", out coll);
					if(coll.Equals(null))
                    {
						return false;
                    }

					foreach(JsonElement prod in coll.EnumerateArray())
					{
						string id = CJsonHelper.GetStringProperty(prod, "prod_id_key_name");
						if(!string.IsNullOrEmpty(id))
						{
							string title = CJsonHelper.GetStringProperty(prod, "prod_name");
							//string strIconPath = GetStringProperty(prod, "prod_dev_image");  // TODO: Use prod_dev_image to download icon 
							CEventDispatcher.OnGameFound(new RawGameData(id, title, "", "", "", "", false, m_platformName));
						}
					}
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed file: {1}", file));
			}
			return gameCount > 0;
		}
	}
}
