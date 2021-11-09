using System;
using System.IO;
using System.Text.Json;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Indiegala
	/// </summary>
    public sealed class CIndiegalaScanner : CBasePlatformScanner<CIndiegalaScanner>
    {
		private const string IG_NAME            = "IGClient";
		private const string IG_JSON_FILE       = @"\IGClient\storage\installed.json";
		private const string IG_OWN_JSON_FILE   = @"\IGClient\config.json";

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			int found = 0;

			// Get installed games
			string file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + IG_JSON_FILE;
			if(!File.Exists(file))
			{
				CLogger.LogInfo("{0} installed games not found in AppData", IG_NAME.ToUpper());
				return false;
			}
			else
			{
				var options = new JsonDocumentOptions
				{
					AllowTrailingCommas = true
				};

				string strDocumentData = File.ReadAllText(file);

				if(string.IsNullOrEmpty(strDocumentData))
				{
					CLogger.LogWarn(string.Format("Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
				}
				else
				{
					try
					{
						using(JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
						{
							foreach(JsonElement element in document.RootElement.EnumerateArray())
							{
								string strID = "";
								string strTitle = "";
								string strLaunch = "";
								string strAlias = "";
								string strPlatform = "Indiegala";// CGameData.GetPlatformString(CGameData.GamePlatform.IGClient);

								element.TryGetProperty("target", out JsonElement target);
								if(!target.Equals(null))
								{
									target.TryGetProperty("item_data", out JsonElement item);
									if(!item.Equals(null))
									{
										strID = CJsonHelper.GetStringProperty(item, "id_key_name");
										strTitle = CJsonHelper.GetStringProperty(item, "name");
									}
								}
								element.TryGetProperty("path", out JsonElement paths);
								if(!paths.Equals(null))
								{
									foreach(JsonElement path in paths.EnumerateArray())
									{
										strLaunch = CDirectoryHelper.FindGameBinaryFile(path.ToString(), strTitle);
									}
								}

								CLogger.LogDebug($"- {strTitle}");

								if(!string.IsNullOrEmpty(strLaunch))
								{
									strAlias = CRegHelper.GetAlias(Path.GetFileNameWithoutExtension(strLaunch));
									if(strAlias.Length > strTitle.Length)
									{
										strAlias = CRegHelper.GetAlias(strTitle);
									}
									if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
									{
										strAlias = "";
									}
									//gameList.Add(new GameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
									CEventDispatcher.NewGameFound(new RawGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
									found++;
								}
							}
						}
					}
					catch(Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
					}
				}
			}
			return found > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			/*
			if(!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				file = GetFolderPath(SpecialFolder.ApplicationData) + IG_OWN_JSON_FILE;
				if(!File.Exists(file))
					CLogger.LogInfo("{0} not-installed games not found in AppData", IG_NAME.ToUpper());
				else
				{
					CLogger.LogDebug("{0} not-installed games:", IG_NAME.ToUpper());
					var options = new JsonDocumentOptions
					{
						AllowTrailingCommas = true
					};

					string strDocumentData = File.ReadAllText(file);

					if(string.IsNullOrEmpty(strDocumentData))
						CLogger.LogWarn(string.Format("Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
					else
					{
						try
						{
							using(JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
							{
								bool found = false;
								JsonElement coll = new JsonElement();
								document.RootElement.TryGetProperty("gala_data", out JsonElement gData);
								if(!gData.Equals(null))
								{
									gData.TryGetProperty("data", out JsonElement data);
									if(!data.Equals(null))
									{
										data.TryGetProperty("showcase_content", out JsonElement sContent);
										if(!sContent.Equals(null))
										{
											sContent.TryGetProperty("content", out JsonElement content);
											if(!content.Equals(null))
											{
												content.TryGetProperty("user_collection", out coll);
												if(!coll.Equals(null))
													found = true;
											}
										}
									}
								}

								if(found)
								{
									foreach(JsonElement prod in coll.EnumerateArray())
									{
										string strID = GetStringProperty(prod, "prod_id_key_name");
										if(!string.IsNullOrEmpty(strID))
										{
											string strTitle = GetStringProperty(prod, "prod_name");
											//string strIconPath = GetStringProperty(prod, "prod_dev_image");  // TODO: Use prod_dev_image to download icon 
											CLogger.LogDebug($"- *{strTitle}");
											string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.IGClient);
											gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
										}
									}
								}
							}
						}
						catch(Exception e)
						{
							CLogger.LogError(e, string.Format("Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
						}
					}
				}
			}
			*/
			return false;
		}
	}
}
