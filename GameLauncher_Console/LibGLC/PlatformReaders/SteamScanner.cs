using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Steam (Valve)
	/// </summary>
    public sealed class CSteamScanner : CBasePlatformScanner<CSteamScanner>
	{
		private const string STEAM_NAME         = "Steam";
		private const int STEAM_MAX_LIBS        = 64;
		private const string STEAM_GAME_FOLDER  = "Steam App ";
		private const string STEAM_LAUNCH       = "steam://rungameid/";
		private const string STEAM_UNINST       = "steam://uninstall/";
		private const string STEAM_PATH         = "steamapps";
		private const string STEAM_LIBFILE      = "libraryfolders.vdf";
		private const string STEAM_APPFILE      = "SteamAppData.vdf";
		private const string STEAM_USRFILE      = "loginusers.vdf";
		private const string STEAM_LIBARR       = "LibraryFolders";
		private const string STEAM_APPDATA      = "SteamAppData";
		private const string STEAM_APPARR       = "AppState";
		private const string STEAM_USRARR       = "users";
		private const string STEAM_REG          = @"SOFTWARE\WOW6432Node\Valve\Steam"; // HKLM32

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			int gameCount = 0;
			string strInstallPath = "";
			string strClientPath = "";

			// Find steam client in the registry
			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(STEAM_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if(key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", STEAM_NAME.ToUpper());
					return false;
				}

				strInstallPath = CRegHelper.GetRegStrVal(key, GAME_INSTALL_PATH);
				strClientPath = Path.Combine(strInstallPath, STEAM_PATH);
			}

			// Ensure that the client actually exist in the retrieved directory
			if(!Directory.Exists(strClientPath))
			{
				CLogger.LogInfo("{0} library not found: {1}", STEAM_NAME.ToUpper(), strClientPath);
				return false;
			}

			// TODO:
			string libFile = Path.Combine(strClientPath, STEAM_LIBFILE);
			List<string> libs = new List<string>
			{
				strClientPath
			};
			int nLibs = 1;

			try
			{
				if(File.Exists(libFile))
				{
					SteamWrapper document = new SteamWrapper(libFile);
					ACF_Struct documentData = document.ACFFileToStruct();
					ACF_Struct folders = new ACF_Struct();
					if(documentData.SubACF.ContainsKey(STEAM_LIBARR))
					{
						folders = documentData.SubACF[STEAM_LIBARR];
					}
					else if(documentData.SubACF.ContainsKey(STEAM_LIBARR.ToLower()))
					{
						folders = documentData.SubACF[STEAM_LIBARR.ToLower()];
					}
					for(; nLibs <= STEAM_MAX_LIBS; ++nLibs)
					{
						folders.SubItems.TryGetValue(nLibs.ToString(), out string library);
						if(string.IsNullOrEmpty(library))
						{
							if(folders.SubACF.ContainsKey(nLibs.ToString()))
							{
								folders.SubACF[nLibs.ToString()].SubItems.TryGetValue("path", out library);
							}
							if(string.IsNullOrEmpty(library))
							{
								nLibs--;
								break;
							}
						}
						library = Path.Combine(library, STEAM_PATH);
						if(!library.Equals(strClientPath) && Directory.Exists(library))
						{
							libs.Add(library);
						}
					}
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed {0} file: {1}", STEAM_NAME.ToUpper(), libFile));
				nLibs--;
			}

			int i = 0;
			List<string> allFiles = new List<string>();
			foreach(string lib in libs)
			{
				List<string> libFiles = new List<string>();
				try
				{
					libFiles = Directory.GetFiles(lib, "appmanifest_*.acf", SearchOption.TopDirectoryOnly).ToList();
					allFiles.AddRange(libFiles);
					CLogger.LogInfo("{0} {1} games found in library {2}", libFiles.Count, STEAM_NAME.ToUpper(), lib);
				}
				catch(Exception e)
				{
					CLogger.LogError(e, string.Format("{0} directory read error: ", STEAM_NAME.ToUpper(), lib));
					continue;
				}

				foreach(string file in libFiles)
				{
					try
					{
						SteamWrapper document = new SteamWrapper(file);
						ACF_Struct documentData = document.ACFFileToStruct();
						ACF_Struct app = documentData.SubACF[STEAM_APPARR];

						string id = app.SubItems["appid"];
						if(id.Equals("228980"))  // Steamworks Common Redistributables
						{
							continue;
						}

						string strID = Path.GetFileName(file);
						string strTitle = app.SubItems["name"];
						CLogger.LogDebug($"- {strTitle}");
						string strLaunch = STEAM_LAUNCH + id;
						string strIconPath = "";
						string strUninstall = "";
						string strAlias = "";
						string strPlatform = "Steam";// CGameData.GetPlatformString(CGameData.GamePlatform.Steam);

						if(!string.IsNullOrEmpty(strLaunch))
						{
							using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG + "\\" + STEAM_GAME_FOLDER + id, RegistryKeyPermissionCheck.ReadSubTree),  // HKLM64
											   key2 = Registry.LocalMachine.OpenSubKey(NODE32_REG + "\\" + STEAM_GAME_FOLDER + id, RegistryKeyPermissionCheck.ReadSubTree))  // HKLM32
							{
								if(key != null)
								{
									strIconPath = CRegHelper.GetRegStrVal(key, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
								}
								else if(key2 != null)
								{
									strIconPath = CRegHelper.GetRegStrVal(key2, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
								}
							}
							if(string.IsNullOrEmpty(strIconPath) && expensiveIcons)
							{
								strIconPath = CDirectoryHelper.FindGameBinaryFile(Path.Combine(Path.Combine(lib, "common"), app.SubItems["installdir"]), strTitle);
								strAlias = CRegHelper.GetAlias(Path.GetFileNameWithoutExtension(strIconPath));
							}
							else
							{
								strAlias = CRegHelper.GetAlias(strTitle);
							}
							if(strAlias.Length > strTitle.Length)
							{
								strAlias = CRegHelper.GetAlias(strTitle);
							}
							if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
							{
								strAlias = "";
							}
							strUninstall = STEAM_UNINST + id;

							//gameList.Add(new GameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
							NewGameFound(new RawGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
							gameCount++;
							//gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
						}
					}
					catch(Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} file: {1}", STEAM_NAME.ToUpper(), file));
					}
				}
				i++;
				if(i > nLibs)
				{
					CLogger.LogDebug("---------------------");
				}
			}
			return gameCount > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			/*
			if(getNonInstalled)
			{
				// First get Steam user ID
				ulong userId = (ulong)CConfig.GetConfigULong(CConfig.CFG_STEAMID);

				if(userId < 1)
				{
					try
					{
						ulong userIdTmp = 0;
						string userName = "";
						string userNameTmp = "";
						string strConfigPath = Path.Combine(strInstallPath, "config");
						string appFile = Path.Combine(strConfigPath, STEAM_APPFILE);

						if(File.Exists(appFile))
						{
							SteamWrapper appDoc = new SteamWrapper(appFile);
							ACF_Struct appDocData = appDoc.ACFFileToStruct();
							ACF_Struct appData = appDocData.SubACF[STEAM_APPDATA];

							appData.SubItems.TryGetValue("AutoLoginUser", out userName);

							SteamWrapper usrDoc = new SteamWrapper(Path.Combine(strConfigPath, STEAM_USRFILE));
							ACF_Struct usrDocData = usrDoc.ACFFileToStruct();
							ACF_Struct usrData = usrDocData.SubACF[STEAM_USRARR];

							foreach(KeyValuePair<string, ACF_Struct> user in usrData.SubACF)
							{
								ulong.TryParse(user.Key, out userIdTmp);

								foreach(KeyValuePair<string, string> userVal in user.Value.SubItems)
								{
									if(userVal.Key.Equals("AccountName"))
									{
										userNameTmp = userVal.Value;
										if(userNameTmp.Equals(userName))
											ulong.TryParse(user.Key, out userId);
									}
									if(userVal.Key.Equals("MostRecent") && userVal.Value.Equals("1") && string.IsNullOrEmpty(userName))
									{
										userId = userIdTmp;
										userName = userNameTmp;
										break;
									}
								}
							}
							if(userId < 1)
							{
								userId = userIdTmp;
								userName = userNameTmp;
							}
						}
						if(userId > 0)
						{
							CLogger.LogInfo("Setting default {0} user to {1} #{2}", STEAM_NAME.ToUpper(), userName, userId);
							CConfig.SetConfigValue(CConfig.CFG_STEAMID, userId);
							ExportConfig();
						}
					}
					catch(Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} file: {1} or {2}", STEAM_NAME.ToUpper(), STEAM_APPFILE, STEAM_USRFILE));
					}
				}

				if(userId > 0)
				{
					// Download game list from public user profile
					try
					{
						string url = string.Format("https://steamcommunity.com/profiles/{0}/games/?tab=all", userId);
						/*
						#if DEBUG
												string tmpfile = $"tmp_{STEAM_NAME}.html";
												if (!File.Exists(tmpfile))
												{
													using (var client = new WebClient())
													{
														client.DownloadFile(url, tmpfile);
													}
												}
												HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument
												{
													OptionUseIdAttribute = true
												};
												doc.Load(tmpfile);
						#else
						* /
						HtmlWeb web = new HtmlWeb();
						web.UseCookies = true;
						HtmlAgilityPack.HtmlDocument doc = web.Load(url);
						doc.OptionUseIdAttribute = true;
						//#endif
						HtmlNode gameList = doc.DocumentNode.SelectSingleNode("//script[@language='javascript']");
						if(gameList != null)
						{
							CLogger.LogDebug("{0} not-installed games (user #{1}):", STEAM_NAME.ToUpper(), userId);

							var options = new JsonDocumentOptions
							{
								AllowTrailingCommas = true
							};
							string rgGames = gameList.InnerText.Remove(0, gameList.InnerText.IndexOf('['));
							rgGames = rgGames.Remove(rgGames.IndexOf(';'));

							using(JsonDocument document = JsonDocument.Parse(@rgGames, options))
							{
								foreach(JsonElement game in document.RootElement.EnumerateArray())
								{
									ulong id = GetULongProperty(game, "appid");
									if(id > 0)
									{
										// Check if game is already installed
										string strID = $"appmanifest_{id}.acf";
										bool found = false;
										foreach(string file in allFiles)
										{
											if(file.EndsWith(strID))
												found = true;
										}
										if(!found)
										{
											string strTitle = GetStringProperty(game, "name");
											//string strIconPath = GetStringProperty(game, "logo");  // TODO: Use logo to download icon
											string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Steam);

											// Add not-installed games
											CLogger.LogDebug($"- *{strTitle}");
											gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
										}
									}
								}
							}
						}
						else
						{
							CLogger.LogInfo("Can't get not-installed {0} games. Profile may not be public.\n" +
											"To change this, go to <https://steamcommunity.com/my/edit/settings>.",
								STEAM_NAME.ToUpper());
						}
						/*
						#if DEBUG
												File.Delete(tmpfile);
						#endif
						* /
					}
					catch(Exception e)
					{
						CLogger.LogError(e);
					}
					CLogger.LogDebug("---------------------");
				}
			}
			*/
			return false;
		}
	}

	/// <summary>
	/// Steam acf File Reader
	/// https://stackoverflow.com/a/42876399/6754996
	/// </<summary>
	internal class SteamWrapper
	{
		public string FileLocation { get; private set; }

		public SteamWrapper(string FileLocation)
		{
			if(File.Exists(FileLocation))
            {
				this.FileLocation = FileLocation;
			}
			else
            {
				throw new FileNotFoundException("Error", FileLocation);
			}
		}

		public bool CheckIntegrity()
		{
			string Content = File.ReadAllText(FileLocation);
			int quote = Content.Count(x => x == '"');
			int braceleft = Content.Count(x => x == '{');
			int braceright = Content.Count(x => x == '}');

			return ((braceleft == braceright) && (quote % 2 == 0));
		}

		public ACF_Struct ACFFileToStruct()
		{
			return ACFFileToStruct(File.ReadAllText(FileLocation));
		}

		private ACF_Struct ACFFileToStruct(string RegionToReadIn)
		{
			ACF_Struct ACF = new ACF_Struct();
			int LengthOfRegion = RegionToReadIn.Length;
			int CurrentPos = 0;
			while(LengthOfRegion > CurrentPos)
			{
				int FirstItemStart = RegionToReadIn.IndexOf('"', CurrentPos);
				if(FirstItemStart == -1)
                {
					break;
				}
				int FirstItemEnd = RegionToReadIn.IndexOf('"', FirstItemStart + 1);
				CurrentPos = FirstItemEnd + 1;
				string FirstItem = RegionToReadIn.Substring(FirstItemStart + 1, FirstItemEnd - FirstItemStart - 1);

				int SecondItemStartQuote = RegionToReadIn.IndexOf('"', CurrentPos);
				int SecondItemStartBraceleft = RegionToReadIn.IndexOf('{', CurrentPos);
				if(SecondItemStartBraceleft == -1 || SecondItemStartQuote < SecondItemStartBraceleft)
				{
					int SecondItemEndQuote = RegionToReadIn.IndexOf('"', SecondItemStartQuote + 1);
					string SecondItem = RegionToReadIn.Substring(SecondItemStartQuote + 1, SecondItemEndQuote - SecondItemStartQuote - 1);
					CurrentPos = SecondItemEndQuote + 1;
					ACF.SubItems.Add(FirstItem, SecondItem);
				}
				else
				{
					int SecondItemEndBraceright = RegionToReadIn.NextEndOf('{', '}', SecondItemStartBraceleft + 1);
					ACF_Struct ACFS = ACFFileToStruct(RegionToReadIn.Substring(SecondItemStartBraceleft + 1, SecondItemEndBraceright - SecondItemStartBraceleft - 1));
					CurrentPos = SecondItemEndBraceright + 1;
					ACF.SubACF.Add(FirstItem, ACFS);
				}
			}

			return ACF;
		}
	}

	internal class ACF_Struct
	{
		public Dictionary<string, ACF_Struct> SubACF { get; private set; }
		public Dictionary<string, string> SubItems { get; private set; }

		public ACF_Struct()
		{
			SubACF = new Dictionary<string, ACF_Struct>();
			SubItems = new Dictionary<string, string>();
		}

		public override string ToString()
		{
			return ToString(0);
		}

		private string ToString(int Depth)
		{
			StringBuilder SB = new StringBuilder();
			foreach(KeyValuePair<string, string> item in SubItems)
			{
				SB.Append('\t', Depth);
				SB.AppendFormat("\"{0}\"\t\t\"{1}\"\r\n", item.Key, item.Value);
			}
			foreach(KeyValuePair<string, ACF_Struct> item in SubACF)
			{
				SB.Append('\t', Depth);
				SB.AppendFormat("\"{0}\"\n", item.Key);
				SB.Append('\t', Depth);
				SB.AppendLine("{");
				SB.Append(item.Value.ToString(Depth + 1));
				SB.Append('\t', Depth);
				SB.AppendLine("}");
			}
			return SB.ToString();
		}
	}

	internal static class Extension
	{
		public static int NextEndOf(this string str, char Open, char Close, int startIndex)
		{
			if(Open == Close)
            {
				throw new Exception("\"Open\" and \"Close\" char are equivalent!");
			}

			int OpenItem = 0;
			int CloseItem = 0;
			for(int i = startIndex; i < str.Length; i++)
			{
				if(str[i] == Open)
				{
					OpenItem++;
				}
				if(str[i] == Close)
				{
					CloseItem++;
					if(CloseItem > OpenItem)
                    {
						return i;
					}
				}
			}
			throw new Exception("Not enough closing characters!");
		}
	}
}
