using Logger;
using Microsoft.Win32;
using PureOrigin.API;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
    // EA (formerly Origin)
    // [installed games + owned games if login is provided]
    public class PlatformEA : IPlatform
	{
		public const GamePlatform ENUM = GamePlatform.EA;
		public const string PROTOCOL			= "origin2://"; //"eadm://" and "ealink://" added after move to EA branding, but "origin://" or "origin2://" still seem to be the correct ones
		public const string LAUNCH				= PROTOCOL + "library/open";
		//public const string INSTALL_GAME		= PROTOCOL + "";
		public const string START_GAME			= PROTOCOL + "game/launch?offerIds=";
		private const string ORIGIN_CONTENT		= @"Origin\LocalContent"; // ProgramData
		private const string ORIGIN_PATH		= "dipinstallpath=";
		/*
		private const string ORIGIN_GAMES		= "Origin Games";
		private const string EA_GAMES			= "EA Games";
		private const string ORIGIN_UNREG		= "Origin"; // HKLM32 Uninstall
		private const string EA_UNREG			= "{9d365a2c-801c-4d99-a902-f17f2dc03510}"; // HKLM32 Uninstall
		private const string EA_REG				= @"SOFTWARE\WOW6432Node\Electronic Arts\EA Desktop"; // HKLM32
		*/
		private const string ORIGIN_CTRYDEF		= "US";
		private const string ORIGIN_LANGDEF		= "en_US";

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

		string IPlatform.Description => GetPlatformString(ENUM);

		public static void Launch()
		{
			if (OperatingSystem.IsWindows())
				_ = CDock.StartShellExecute(LAUNCH);
			else
				_ = Process.Start(LAUNCH);
		}

		// return value
		// -1 = not implemented
		// 0 = failure
		// 1 = success
		public static int InstallGame(CGame game)
		{
			CDock.DeleteCustomImage(game.Title, false);
			Launch();
			/*
			if (OperatingSystem.IsWindows())
                _ = CDock.StartShellExecute(INSTALL_GAME + GetGameID(game.ID));
			else
				_ = Process.Start(INSTALL_GAME + GetGameID(game.ID));
			*/
			return 1;
		}

		public static void StartGame(CGame game)
		{
			CLogger.LogInfo($"Launch: {game.Launch}");
			if (OperatingSystem.IsWindows())
				_ = CDock.StartShellExecute(game.Launch);
			else
				_ = Process.Start(game.Launch);
		}

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			List<RegistryKey> keyList = new();
			List<string> dirs = new();
			List<string> eaIds = new();
			string strPlatform = GetPlatformString(ENUM);
			string path = "";
			try
			{
				path = Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), ORIGIN_CONTENT);
				if (Directory.Exists(path))
				{
					dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly));
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format("{0} directory read error: {1}", _name.ToUpper(), path));
			}

			CLogger.LogInfo("{0} {1} games found", dirs.Count, _name.ToUpper());
			foreach (string dir in dirs)
			{
				string[] files = Array.Empty<string>();
				string install = "";

				string strID = "";
				string strTitle = strTitle = Path.GetFileName(dir);
				string strLaunch = "";
				//string strIconPath = "";
				string strUninstall = "";
				string strAlias = "";

				try
				{
					files = Directory.GetFiles(dir, "*.mfst", SearchOption.TopDirectoryOnly);
				}
				catch (Exception e)
				{
					CLogger.LogError(e);
				}

				foreach (string file in files)
				{
					strID = Path.GetFileName(file);
					try
					{
						string strDocumentData = File.ReadAllText(file);
						string[] subs = strDocumentData.Split('&');
						foreach (string sub in subs)
						{
							if (sub.StartsWith(ORIGIN_PATH))
							{
								install = sub[15..];
								break;
							}
						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
					}
				}

				if (!string.IsNullOrEmpty(install))
				{
					install = Uri.UnescapeDataString(install);

					using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
					{
						if (key != null)
						{
							keyList = FindGameKeys(key, install, GAME_INSTALL_LOCATION, new string[] { "EA Desktop" });
							foreach (var data in keyList)
							{
								strTitle = GetRegStrVal(data, GAME_DISPLAY_NAME);
								strLaunch = GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
								strUninstall = GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
							}
						}
					}

					CLogger.LogDebug($"- {strTitle}");
					if (string.IsNullOrEmpty(strLaunch))
						strLaunch = CGameFinder.FindGameBinaryFile(install, strTitle);
					strAlias = GetAlias(Path.GetFileNameWithoutExtension(install));
					if (strAlias.Length > strTitle.Length)
						strAlias = GetAlias(strTitle);
					if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
						strAlias = "";

					if (!(string.IsNullOrEmpty(strLaunch)))
					{
						gameDataList.Add(
							new ImportGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
						eaIds.Add(GetGameID(strID));
					}
				}
			}

			// Get not-installed games
			if (!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				if (GetLogin(out string email, out SecureString password) && (!email.Equals("skipped") && !email.Equals("invalid") && !password.Equals(null) && password.Length > 0))
				{
					List<string[]> games = GetOwnedGames(email, password).Result;
					password.Dispose();
					if (games.Count <= 0)
					{
						CLogger.LogInfo("Can't get not-installed {0} games. Profile may not be public.\n" +
										"To change this, go to <https://myaccount.ea.com/cp-ui/privacy/index>.",
							_name.ToUpper());
					}
					else
					{
						foreach (string[] game in games)
						{
							if (game.Length > 2)
							{
								string strID = game[0];
								if (eaIds.Contains(strID))
									continue;
								string strTitle = game[1];
								CLogger.LogDebug($"- *{strTitle}");
								// TODO: metadata description
								//if (game.Length > 3)
								//	string strDescription = game[2];
								DateTime lastRun;
								if (!(game.Length > 4 && DateTime.TryParse(game[3], out lastRun)))
									lastRun = DateTime.MinValue;
								gameDataList.Add(new ImportGameData(strID, strTitle, "", "", "", "", false, strPlatform, dateLastRun: lastRun));
								// Use website to download missing icons
								if (game.Length > 4 && !(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
									CDock.DownloadCustomImage(strTitle, game[4]);
							}
						}
					}
				}
			}

			CLogger.LogDebug("----------------------");
		}

		public static string GetIconUrl(CGame game)
		{
			if (GetLogin(out string email, out SecureString password) && (!email.Equals("skipped") && !email.Equals("invalid") && !password.Equals(null) && password.Length > 0))
			{
				List<string[]> games = GetOwnedGames(email, password, GetGameID(game.ID)).Result;
				password.Dispose();
				if (games.Count <= 0)
				{
					CLogger.LogInfo("Can't get {0} game list. Profile may not be public.\n" +
									"To change this, go to <https://myaccount.ea.com/cp-ui/privacy/index>.",
						_name.ToUpper());
				}
				else
				{
					foreach (string[] gameArray in games)
					{
						if (gameArray.Length > 4 && !(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
							CDock.DownloadCustomImage(game.Title, gameArray[4]);
						break;
					}
				}
			}

			return "";
		}

		/// <summary>
		/// Scan the key name and extract the Origin game id
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>Origin game ID as string</returns>
		public static string GetGameID(string key)
		{
			if (key.EndsWith(".mfst"))
			{
				return Path.GetFileNameWithoutExtension(key);
			}
			return key;
		}

		private static bool GetLogin(out string email, out SecureString password)
        {
			email = CConfig.GetConfigString(CConfig.CFG_ORIGINID);
			password = new();
			try
			{
				string encrypted = CConfig.GetConfigString(CConfig.CFG_ORIGINPW);
				if (!string.IsNullOrEmpty(encrypted))
				{
					Span<byte> byteSpan = Convert.FromBase64String(encrypted);
					if (OperatingSystem.IsWindows())
					{
						Array.ForEach(Encoding.UTF8.GetString(ProtectedData.Unprotect(byteSpan.ToArray(), null, DataProtectionScope.CurrentUser)).ToArray(), password.AppendChar);
					}
					else  // If we ever support other OSes, we will need to have a POSIX method to encrypt the password
						Array.ForEach(Encoding.UTF8.GetString(byteSpan).ToArray(), password.AppendChar);
					password.MakeReadOnly();
				}

				if (string.IsNullOrEmpty(email))
				{
					email = CDock.InputPrompt(_name + " e-mail >>> ", new());
					CDock.ClearInputLine(new());
				}

				if (string.IsNullOrEmpty(email))
					CConfig.SetConfigValue(CConfig.CFG_ORIGINID, "skipped");
				else if (!email.Contains('@') || !email.Contains('.'))
					CConfig.SetConfigValue(CConfig.CFG_ORIGINID, "invalid");
				else
				{
					CConfig.SetConfigValue(CConfig.CFG_ORIGINID, email);

					if (string.IsNullOrEmpty(encrypted))
					{
						string strPwd = CDock.InputPassword(_name + " password >>> ", new());
						if (!string.IsNullOrEmpty(strPwd))
						{
							Span<byte> byteSpan = new();
							if (OperatingSystem.IsWindows())
								byteSpan = ProtectedData.Protect(Encoding.UTF8.GetBytes(strPwd), null, DataProtectionScope.CurrentUser);
							else  // If we ever support other OSes, we will need to have a POSIX method to encrypt the password
								byteSpan = Encoding.UTF8.GetBytes(strPwd);
							password = new NetworkCredential("", strPwd).SecurePassword;
							password.MakeReadOnly();
							encrypted = Convert.ToBase64String(byteSpan);
							CConfig.SetConfigValue(CConfig.CFG_ORIGINPW, encrypted);
							byteSpan = null;
						}
						strPwd = null;
						CDock.ClearInputLine(new());
					}
				}
				encrypted = null;
				if (!string.IsNullOrEmpty(email) && password.Length > 0)
					return true;
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			
			return false;
		}

		private static async Task<List<string[]>> GetOwnedGames(string email, SecureString password, string id = "")
		{
			List<string[]> games = new();
			if (!email.Equals("skipped") && !email.Equals("invalid") && !password.Equals(null) && password.Length > 0)
			{
				bool success = false;
				XmlDocument doc = new();
				ulong userid = 0;
				string country = "";
				//string language = "";
				string locale = "";
				try
				{
					OriginAPI originApi = new(email, new NetworkCredential("", password).Password);
					bool result = await originApi.LoginAsync();
					if (!result)
					{
						CLogger.LogWarn("Login error for {0} not-installed games.", _name.ToUpper());
					}
					else
					{
						userid = originApi.InternalUser.UserId;
						country = originApi.InternalUser.Country;		// "US"
						if (string.IsNullOrEmpty(country))
							country = ORIGIN_CTRYDEF;
						//language = originApi.InternalUser.Language;	// "en"
						locale = originApi.InternalUser.Locale;			// "en_US"
						if (string.IsNullOrEmpty(locale))
							locale = ORIGIN_LANGDEF;

						/*
						OriginUser user = await originApi.GetUserAsync(userid);
						CLogger.LogDebug("persona: {0}, name: {1}, avatar: {2}", user.PersonaId.ToString(), user.Username, await user.GetAvatarUrlAsync());
						*/
						/*
						//Random rand = new Random();
						//rand.Next(1, 5);
						//HttpRequestMessage request4 = BaseAPIManager.CreateRequest(HttpMethod.Get, $"https://api1.origin.com/atom/users?userIds={userid}");
						//HttpRequestMessage request = BaseAPIManager.CreateRequest(HttpMethod.Get, $"https://api1.origin.com/atom/users/{userid}/appSettings");
						HttpRequestMessage request = BaseAPIManager.CreateRequest(HttpMethod.Get, $"https://api4.origin.com/atom/users/{userid}/privacySettings");
						HttpResponseMessage response = await BaseAPIManager.SendAsync(request);
						if (response.StatusCode != HttpStatusCode.OK)
							CLogger.LogWarn("Communication error for {0} privacy settings: {1}", _name.ToUpper(), response.StatusCode);
						else
						{
							string content = await response.Content.ReadAsStringAsync();
							CLogger.LogDebug("  {0} 1 content:\n{1}", _name, content);
						}
						*/
/*
#if DEBUG
						// Don't re-download if file exists
						string tmpfile = $"tmp_{_name}.xml";
						if (!File.Exists(tmpfile))
						{
#endif
*/
						//rand.Next(1, 5);
						HttpRequestMessage request2 = BaseAPIManager.CreateRequest(HttpMethod.Get, $"https://api2.origin.com/ecommerce2/consolidatedentitlements/{userid}?machine_hash=1");
						request2.Headers.Add("accept", new List<string> { "application/vnd.origin.v3+json", "x-cache/force-write" });
						HttpResponseMessage response2 = await BaseAPIManager.SendAsync(request2);
						if (response2.StatusCode != HttpStatusCode.OK)
							CLogger.LogWarn("Communication error for {0} not-installed games: {1}", _name.ToUpper(), response2.StatusCode);
						else
						{
							success = true;
							StreamReader content2stream = new(await response2.Content.ReadAsStreamAsync()); //Transfer-Encoding: chunked
							byte[] content2byte = Encoding.UTF8.GetBytes(content2stream.ReadToEnd());
							//CLogger.LogDebug("  {0} 2 content:\n{1}", _name, Encoding.UTF8.GetString(content2byte.ToArray()));
							doc.LoadXml(Encoding.UTF8.GetString(content2byte.ToArray()));
/*
#if DEBUG
								File.WriteAllBytes(tmpfile, content2byte);
							}
						}
						else
						{
							success = true;
							doc.Load(tmpfile);
#endif
*/
						}
					}
				}
				catch (Exception e)
				{
					CLogger.LogError(e, string.Format("API error for {0} not-installed games.", _name.ToUpper()));
				}

				if (!success)
					return new();
				
				/*
				//rand.Next(1, 5);
				HttpRequestMessage request3 = BaseAPIManager.CreateRequest(HttpMethod.Get, $"https://api1.origin.com/ecommerce2/entitlements/{userid}/associations/pending");
				HttpResponseMessage response3 = await BaseAPIManager.SendAsync(request3);
				if (response3.StatusCode != HttpStatusCode.OK)
					CLogger.LogWarn("Communication error for {0} not-installed games: {1}", _name.ToUpper(), response3.StatusCode);
				else
				{
					string content3 = await response3.Content.ReadAsStringAsync();
					CLogger.LogDebug("  {0} 3 content:\n{1}", _name, content3);
				}
				*/

				XmlNodeList gameList = doc.DocumentElement.SelectNodes("/entitlements/entitlement");
				if (gameList == null || gameList.Count <= 0)
					return new();

				CLogger.LogDebug("{0} not-installed games (user #{1}):", _name.ToUpper(), userid);

				string strID = "";
				string strTitle = "";
				string strDescription = "";
				string iconUrl = "";
				//string mdmId = "";
				string lastRun = "";

				foreach (XmlNode game in gameList)
				{
					try
					{
						strID = game.SelectSingleNode("offerId").InnerText;
						if (!string.IsNullOrEmpty(id) && !strID.Equals(id))  // used when looking for a particular game ID
							continue;
						//if (!game.SelectSingleNode("originDisplayType").InnerText.Equals("Full Game"))	// DLCs will be "Expansion" or "Addon"
						if (!game.SelectSingleNode("entitlementTag").InnerText.Equals("ORIGIN_DOWNLOAD"))   // DLCs will be XPACK{#}_ACCESS
							continue;

						iconUrl = game.SelectSingleNode("offer/customAttributes/imageServer").InnerText;

						XmlNode node3 = game.SelectSingleNode("offer/localizableAttributes");
						if (node3 == null)
							continue;

						XmlNode descNode = node3.SelectSingleNode("shortDescription");
						if (descNode != null)
							strDescription = descNode.InnerText;
						XmlNode titleNode = node3.SelectSingleNode("displayName");
						if (titleNode != null)
							strTitle = titleNode.InnerText;
						XmlNode imageNode = node3.SelectSingleNode("packArtLarge");
						if (imageNode != null)
							iconUrl += imageNode.InnerText;

						/*
						XmlNode node4 = game.SelectSingleNode("offer/mdmHierarchies");
						XmlNodeList mdmList = node4.SelectNodes("mdmHierarchy");
						foreach (XmlNode mdm in node4)
						{
							foreach (XmlAttribute mdmAttrib in mdm.Attributes)
							{
								if (mdmAttrib.LocalName.Equals("type") && mdmAttrib.Value.Equals("Primary"))
								{
									foreach (XmlAttribute titleAttrib in mdm.SelectSingleNode("mdmMasterTitle").Attributes)
									{
										if (titleAttrib.LocalName.Equals("masterTitleId"))
										{
											mdmId = titleAttrib.Value;

											//rand.Next(1, 5);
											HttpRequestMessage request4 = BaseAPIManager.CreateRequest(HttpMethod.Get, $"https://api1.origin.com/atom/users/{userid}/games/{mdmId}/usage");
											request4.Headers.Add("X-Origin-Platform", "PCWIN");
											HttpResponseMessage response4 = await BaseAPIManager.SendAsync(request4);
											if (response4.StatusCode != HttpStatusCode.OK)
												CLogger.LogWarn("Communication error for {0} game usage: {1}", _name.ToUpper(), response4.StatusCode);
											else
											{
												string content4 = await response4.Content.ReadAsStringAsync();
												CLogger.LogDebug("  {0} 4 content:\n{1}", _name, content4);
												XmlDocument doc2 = new();
												doc2.LoadXml(content4);
												XmlNode usage = doc2.DocumentElement.SelectSingleNode("/usage");
												if (!usage.SelectSingleNode("gameId").InnerText.Equals(mdmId))
													continue;
						*/
/*
#if DEBUG
												File.WriteAllText($"tmp_{_name}_usage{mdmId}.xml", content4);
#endif
*/
						/*
												lastRun = usage.SelectSingleNode("lastSessionEndTimeStamp").InnerText;
												CLogger.LogDebug($"lastRun: {lastRun}");
											}
											break;
										}
									}
								}
							}
						}
						*/

						/*
						//rand.Next(1, 5);
						HttpRequestMessage request5 = BaseAPIManager.CreateRequest(HttpMethod.Get, $"https://api4.origin.com/ecommerce2/offerUpdatedDate?offerIds={game}");
						HttpResponseMessage response5 = await BaseAPIManager.SendAsync(request5);
						if (response5.StatusCode != HttpStatusCode.OK)
							CLogger.LogWarn("Communication error for {0} not-installed games: {1}", _name.ToUpper(), response5.StatusCode);
						else
						{
							string content5 = await response5.Content.ReadAsStringAsync();
							CLogger.LogDebug("  {0} 5 content:\n{1}", _name, content5);
						}
						*/

						games.Add(new string[] { strID, strTitle, strDescription, lastRun, iconUrl });
					}
					catch (Exception e)
                    {
						CLogger.LogError(e, string.Format("API error for {0} not-installed games.", _name.ToUpper()));
					}
				}

				/*
				//rand.Next(1, 5);
				string file = $"supercat-PCWIN_MAC-{country}-{locale}.json.gz";
				if (!File.Exists(file))
				{
					HttpRequestMessage request6 = BaseAPIManager.CreateRequest(HttpMethod.Get, $"https://api2.origin.com/supercat/${country}/{locale}/{file}");
					HttpResponseMessage response6 = await BaseAPIManager.SendAsync(request6);
					if (response6.StatusCode != HttpStatusCode.OK)
						CLogger.LogWarn("Communication error for {0} not-installed games: {1}", _name.ToUpper(), response6.StatusCode);
					else
					{
						string content6 = await response6.Content.ReadAsStringAsync();
						CLogger.LogDebug("  {0} 6 content:\n{1}", _name, content6);
					}
				}
				*/

				return games;
			}

			return new();
		}
	}

	public class OriginResponse
	{
		public class Entitlement
		{
			public class OriginEntitlement
			{
				public long entitlementId;
				public string offerId;
				public string offerPath;
				public string status;
				public string offerType;
				public string originDisplayType;
				public string masterTitleId;
				public string gameDistributionSubType;
			}

			public string error;
			public List<OriginEntitlement> entitlements;
		}
	}
}