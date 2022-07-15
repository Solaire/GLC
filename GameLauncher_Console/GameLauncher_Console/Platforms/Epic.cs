using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
//using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Epic Games Launcher
	// [installed games, and owned games with Legendary https://legendary.gl/github]
	public class PlatformEpic : IPlatform
	{
		public const GamePlatform ENUM			= GamePlatform.Epic;
		public const string PROTOCOL			= "com.epicgames.launcher://";
		private const string START_GAME			= PROTOCOL + @"apps/";
		private const string START_GAME_ARGS	= "?action=launch&silent=true";
		private const string INSTALL_GAME_ARGS	= "?action=install";
		//private const string EPIC_GAMES_UNREG	= "{A2FB1E1A-55D9-4511-A0BF-DEAD0493FBBC}"; // HKLM32 Uninstall
		//private const string EPIC_GAMES_UNREG	= "{A7BBC0A6-3DB0-41CC-BCED-DDFC5D4F3060}"; // HKLM32 Uninstall
		private const string EPIC_ITEMS 		= @"Epic\EpicGamesLauncher\Data\Manifests"; // ProgramData

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

		// return value
		// -1 = not implemented
		// 0 = failure
		// 1 = success
		public static int InstallGame(CGame game)
		{
			//CDock.DeleteCustomImage(game.Title);
			//bool useEGL = (bool)CConfig.GetConfigBool(CConfig.CFG_USEEGL);
			bool useLeg = (bool)CConfig.GetConfigBool(CConfig.CFG_USELEG);
			string pathLeg = CConfig.GetConfigString(CConfig.CFG_PATHLEG);
			if (string.IsNullOrEmpty(pathLeg))
				useLeg = false;
			if (!pathLeg.Contains(@"\") && !pathLeg.Contains("/")) // legendary.exe in current directory
				pathLeg = Path.Combine(Directory.GetCurrentDirectory(), pathLeg);

			if (useLeg && File.Exists(pathLeg))
			{
				if (OperatingSystem.IsWindows())
				{
					CLogger.LogInfo($"Launch: cmd.exe /c \"" + pathLeg + "\" -y install " + game.ID);
					CDock.StartAndRedirect("cmd.exe", "/c '\"" + pathLeg + "\" -y install " + game.ID);
				}
				else
				{
					CLogger.LogInfo($"Launch: " + pathLeg + " -y install " + game.ID);
					Process.Start(pathLeg, "-y install " + game.ID);
				}
				return 1;
			}
			else //if (useEGL)
			{
				if (OperatingSystem.IsWindows())
					CDock.StartShellExecute(START_GAME + game.ID + INSTALL_GAME_ARGS);
				else
					Process.Start(START_GAME + game.ID + INSTALL_GAME_ARGS);
				return 1;
			}
			//return 0;
		}

		public static void StartGame(CGame game)
        {
			bool useEGL = (bool)CConfig.GetConfigBool(CConfig.CFG_USEEGL);
			bool useLeg = (bool)CConfig.GetConfigBool(CConfig.CFG_USELEG);
			bool syncLeg = (bool)CConfig.GetConfigBool(CConfig.CFG_SYNCLEG);
			string pathLeg = CConfig.GetConfigString(CConfig.CFG_PATHLEG);
			if (string.IsNullOrEmpty(pathLeg))
				useLeg = false;
			if (!pathLeg.Contains(@"\") && !pathLeg.Contains("/")) // legendary.exe in current directory
				pathLeg = Path.Combine(Directory.GetCurrentDirectory(), pathLeg);

			if (useLeg && File.Exists(pathLeg))
			{
				if (OperatingSystem.IsWindows())
				{
					string cmdLine = "\"" + pathLeg + "\" -y launch " + game.ID;
					CLogger.LogInfo($"Launch: cmd.exe /c " + cmdLine);
					if (syncLeg)
						cmdLine = "\"" + pathLeg + "\" -y sync-saves " + game.ID + " & " + cmdLine + " & \"" + pathLeg + "\" -y sync-saves " + game.ID;
					CDock.StartAndRedirect("cmd.exe", "/c '" + cmdLine + " '");
				}
				else
				{
					CLogger.LogInfo($"Launch: " + pathLeg + " -y launch " + game.ID);
					if (syncLeg)
						Process.Start(pathLeg, "-y sync-saves " + game.ID);
					Process.Start(pathLeg, "-y launch " + game.ID);
					if (syncLeg)
						Process.Start(pathLeg, "-y sync-saves " + game.ID);
				}
			}
			else if (useEGL)
            {
				if (OperatingSystem.IsWindows())
					CDock.StartShellExecute(START_GAME + game.ID + START_GAME_ARGS);
				else
					Process.Start(PROTOCOL + START_GAME + game.ID + START_GAME_ARGS);
            }
			else
			{
				if (OperatingSystem.IsWindows())
					CDock.StartShellExecute(game.Launch);
				else
					Process.Start(game.Launch);
			}
		}

		// return value
		// -1 = not implemented
		// 0 = failure
		// 1 = success
		public static int UninstallGame(CGame game)
        {
			//bool useEGL = (bool)CConfig.GetConfigBool(CConfig.CFG_USEEGL);
			bool useLeg = (bool)CConfig.GetConfigBool(CConfig.CFG_USELEG);
			string pathLeg = CConfig.GetConfigString(CConfig.CFG_PATHLEG);
			if (string.IsNullOrEmpty(pathLeg))
				useLeg = false;
			if (!pathLeg.Contains(@"\") && !pathLeg.Contains("/")) // legendary.exe in current directory
				pathLeg = Path.Combine(Directory.GetCurrentDirectory(), pathLeg);
			if (useLeg && File.Exists(pathLeg))
			{
				//Process ps;
				if (OperatingSystem.IsWindows())
				{
					CLogger.LogInfo("Launch: cmd.exe /c \"" + pathLeg + "\" -y uninstall " + game.ID);
					CDock.StartAndRedirect("cmd.exe", "/c \"" + pathLeg + "\" -y uninstall " + game.ID);
				}
				else
				{
					CLogger.LogInfo("Launch: " + pathLeg + " -y uninstall " + game.ID);
					Process.Start(pathLeg, "-y uninstall " + game.ID);
				}
				/*
				ps.WaitForExit(30000);
				if (ps.ExitCode == 0)
				*/
					return 1;
			}
			/*
			else if (useEGL)
            {
				Launch();
				return -1;
			}
			*/
			else if (!string.IsNullOrEmpty(game.Uninstaller))
            {
				// delete Desktop icon
				File.Delete(Path.Combine(GetFolderPath(SpecialFolder.Desktop), game.Title + ".lnk"));
				string[] un = game.Uninstaller.Split(';');

				// delete manifest file
				if (un.Length > 1 && !string.IsNullOrEmpty(un[1]) && un[1].EndsWith(".item"))
					File.Delete(Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), EPIC_ITEMS, un[1]));

				if (un.Length > 0 && !string.IsNullOrEmpty(un[0]) && !un[0].EndsWith(".item"))
				{
					DirectoryInfo rootDir = new(un[0]);
					if (rootDir.Exists)
					{
						/*
						foreach (DirectoryInfo dir in rootDir.EnumerateDirectories())
							dir.Delete(true);
						foreach (FileInfo file in rootDir.EnumerateFiles())
							file.Delete();
						*/
						rootDir.Delete(true);
						return 1;
					}
				}
            }
			return 0;
		}

		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			List<string> epicIds = new();
			string strPlatform = GetPlatformString(ENUM);
			string dir = Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), EPIC_ITEMS);
			if (!Directory.Exists(dir))
			{
				CLogger.LogInfo("{0} games not found in ProgramData.", _name.ToUpper());
				return;
			}
			string[] files = Directory.GetFiles(dir, "*.item", SearchOption.TopDirectoryOnly);
			CLogger.LogInfo("{0} {1} games found", files.Length, _name.ToUpper());

			foreach (string file in files)
			{
				string strDocumentData = File.ReadAllText(file);

				if (string.IsNullOrEmpty(strDocumentData))
					continue;

				try
				{
					using JsonDocument document = JsonDocument.Parse(@strDocumentData, jsonTrailingCommas);
					string strID = GetStringProperty(document.RootElement, "AppName");
					if (string.IsNullOrEmpty(strID))
						strID = Path.GetFileName(file);
					string strTitle = GetStringProperty(document.RootElement, "DisplayName");
					CLogger.LogDebug($"- {strTitle}");
					string strLaunch = GetStringProperty(document.RootElement, "LaunchExecutable"); // DLCs won't have this set
					string strUninstall = "";
					string strAlias = "";

					if (!string.IsNullOrEmpty(strLaunch))
					{
						epicIds.Add(strID);
						strUninstall = GetStringProperty(document.RootElement, "InstallLocation");
						strLaunch = Path.Combine(strUninstall, strLaunch);
						strUninstall += ";" + Path.GetFileName(file);
						// rather than an uninstaller like most platforms, for Epic, strUninstall holds two fields: the install location and the manifest file
						strAlias = GetAlias(GetStringProperty(document.RootElement, "MandatoryAppFolderName"));
						if (strAlias.Length > strTitle.Length)
							strAlias = GetAlias(strTitle);
						if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
							strAlias = "";
						gameDataList.Add(new ImportGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
					}
				}
				catch (Exception e)
				{
					CLogger.LogError(e, string.Format("Malformed {0} file: {1}", _name.ToUpper(), file));
				}

			}

			// Get not-installed games (requires Legendary)
			if (!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				bool useLeg = (bool)CConfig.GetConfigBool(CConfig.CFG_USELEG);
				string pathLeg = CConfig.GetConfigString(CConfig.CFG_PATHLEG);
				if (string.IsNullOrEmpty(pathLeg))
					useLeg = false;
				if (!pathLeg.Contains(@"\") && !pathLeg.Contains("/")) // legendary.exe in current directory
					pathLeg = Path.Combine(Directory.GetCurrentDirectory(), pathLeg);

				if (useLeg && File.Exists(pathLeg))
				{
					try
					{
						Process ps;
						string tmpfile = $"tmp_{_name}.json";
						string errfile = $"tmp_{_name}.err";
						if (OperatingSystem.IsWindows())
						{
							CLogger.LogInfo("Launch: cmd.exe /c \"" + pathLeg + "\" list --json 1> " + tmpfile + " 2> " + errfile);
							ps = CDock.StartAndRedirect("cmd.exe", "/c \"" + pathLeg + "\" list --json 1> " + tmpfile + " 2> " + errfile);
						}
						else
						{
							CLogger.LogInfo("Launch: " + pathLeg + " list 1> " + tmpfile + " 2> " + errfile);
							ps = Process.Start(pathLeg, "list 1> " + tmpfile + " 2> " + errfile);
						}
						ps.WaitForExit(30000);
						if (File.Exists(tmpfile))
						{
							if (ps.ExitCode != 0)
							{
								if (File.Exists(errfile))
								{
									string strErrorData = File.ReadAllText(errfile);
									if (!string.IsNullOrEmpty(strErrorData))
									{
										using StreamReader reader = new(errfile);
										string line;
										while ((line = reader.ReadLine()) != null)
										{
											if (line.Trim().EndsWith("No saved credentials"))
											{
												CLogger.LogInfo("Error getting not-installed {0} games. Is Legendary authenticated?\n" +
															   $"To login, enter: \"{pathLeg}\" auth",
													_name.ToUpper());
											}
										}
									}
								}
							}

							string strDocumentData = File.ReadAllText(tmpfile);
							if (!string.IsNullOrEmpty(strDocumentData))
							{
								CLogger.LogDebug("{0} not-installed games:", _name.ToUpper());
								using JsonDocument document = JsonDocument.Parse(@strDocumentData, jsonTrailingCommas);
								foreach (JsonElement element in document.RootElement.EnumerateArray())
								{
									string strID = GetStringProperty(element, "app_name");
									if (epicIds.Contains(strID)) // Check if game is already installed
										continue;
									string strTitle = GetStringProperty(element, "app_title");
									CLogger.LogDebug($"- *{strTitle}");
									string strAlias = GetAlias(strTitle);
									if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
										strAlias = "";
									gameDataList.Add(new ImportGameData(strID, strTitle, "", "", "", strAlias, false, strPlatform));
								}
							}
#if !DEBUG
							File.Delete(tmpfile);
							File.Delete(errfile);
#endif

						}
						else
                        {
							CLogger.LogInfo("Can't get not-installed {0} games. Error running Legendary.",
								_name.ToUpper());
						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} output.", _name.ToUpper()));
					}
				}
				else
				{
					CLogger.LogInfo("Can't get not-installed {0} games. Legendary must be installed.\n" +
									"Go to <https://legendary.gl/release/latest>.",
						_name.ToUpper());
				}
			}

			CLogger.LogDebug("--------------------");
		}
	}
}