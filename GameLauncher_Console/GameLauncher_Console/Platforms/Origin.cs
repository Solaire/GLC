//using HtmlAgilityPack;
using Logger;
//using Microsoft.Web.WebView2;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
//using System.Net;
//using System.Windows.Forms;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Origin [soon to be EA Desktop]
	// [installed games only]
	public class PlatformOrigin : IPlatform
	{
		public const GamePlatform ENUM			= GamePlatform.Origin;
		public const string PROTOCOL			= "origin://";	//"eadm://" was added by EA Desktop, but "origin://" and "origin2://" still work with it (for now)
		private const string ORIGIN_CONTENT		= @"Origin\LocalContent"; // ProgramData
		private const string ORIGIN_PATH		= "dipinstallpath=";
		/*
		private const string ORIGIN_GAMES		= "Origin Games";
		private const string EA_GAMES			= "EA Games";
		private const string ORIGIN_UNREG		= "Origin"; // HKLM32 Uninstall
		private const string ORIGIN_REG			= @"SOFTWARE\WOW6432Node\Origin"; // HKLM32
		*/

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

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			List<RegistryKey> keyList = new();
			List<string> dirs = new();
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

				string strID = Path.GetFileName(dir);
				string strTitle = strID;
				string strLaunch = "";
				//string strIconPath = "";
				string strUninstall = "";
				string strAlias = "";
				string strPlatform = GetPlatformString(GamePlatform.Origin);

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
					try
					{
						string strDocumentData = File.ReadAllText(file);
						string[] subs = strDocumentData.Split('&');
						foreach (string sub in subs)
						{
							if (sub.StartsWith(ORIGIN_PATH))
								install = sub[15..];
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
							keyList = FindGameKeys(key, install, GAME_INSTALL_LOCATION, new string[] { "Origin" });
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
						gameDataList.Add(
							new ImportGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
				}
			}
			CLogger.LogDebug("----------------------");
		}

		public void SignIn()
        {
/*
			string url = "https://accounts.ea.com/connect/auth?response_type=code&client_id=ORIGIN_SPA_ID&display=originXWeb%2Flogin&locale=en_US&release_type=prod&redirect_uri=https%3A%2F%2Fwww.origin.com%2Fviews%2Flogin.html";

#if DEBUG
			// Don't re-download if file exists
			CookieContainer cookies = new CookieContainer();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "GET";
			request.CookieContainer = cookies;
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			var stream = response.GetResponseStream();
*/
			/*
			string tmpfile = $"tmp_{_name}.html";
			if (!File.Exists(tmpfile))
			{
				using (var client = new WebClient());
				client.DownloadFile(url, tmpfile);
			}
			*/
/*
			using (var reader = new StreamReader(stream));
			{
				string html = reader.ReadToEnd();
				HtmlDocument doc = new HtmlDocument
				{
					OptionUseIdAttribute = true
				};
				doc.Load(html);
			}
#else

			HtmlWeb web = new()
			{
				UseCookies = true
			};
			HtmlDocument doc = web.Load(url);
			doc.OptionUseIdAttribute = true;
			
#endif
			*/
			
			/*
			HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@class='rr-game-image']");
			foreach (HtmlNode child in node.ChildNodes)
			{
				foreach (HtmlAttribute attr in child.Attributes)
				{
					if (attr.Name.Equals("src", CDock.IGNORE_CASE))
					{
						return attr.Value;
					}
				}
			}
			*/
		}
	}
}