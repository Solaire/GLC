using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Logger;

namespace GameLauncher_Console
{
	/// <summary>
	/// Class for finding game binary (.exe) and link/shortcus (.lnk) files inside directories.
	/// </summary>
	public static class CGameFinder
	{
		private const string IMAGE_FOLDER_NAME = "CustomImages";
		private const string GAME_FOLDER_NAME = "CustomGames";
		private const string CUSTOM_PLATFORM  = "Custom";

		/// <summary>
		/// Find the game's binary executable file
		/// </summary>
		/// <param name="strPath">Path to search</param>
		/// <param name="strTitle">Title of the game</param>
		/// <returns></returns>
		public static string FindGameBinaryFile(string strPath, string strTitle)
		{
			/* function flow
			 * Find all exe files in the directory. (Exclude files with words: "unins", "redist")
			 * Compare exe files with gameTitle
			 * If failed, compare with name of root directory
			 * Return "" if failed
			 */

			// Get our list of exe files in the game folder + all subfolders
			if (!Directory.Exists(strPath))
				return "";
			
			List<string> exeFiles = Directory.EnumerateFiles(strPath, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".exe")).ToList();
			
			// Big Fish Games may use .bfg for executables
			//exeFiles.AddRange(Directory.EnumerateFiles(strPath, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".bfg")).ToList());

			// If only 1 file has been found, return it.
			if (exeFiles.Count == 1)
			{
				return exeFiles[0];
			}

			// First, compare the files against the game title
			{
				string[] words = strTitle.Split(new char[] { ' ', ':', '-', '_' });
				string letters = "";

				foreach (string word in words)
				{
					if (!string.IsNullOrEmpty(word))
						letters += word[0];
				}

				foreach (string file in exeFiles)
				{
					//CLogger.LogInfo("*** {0}", file);
					if (file.ToLower().Contains("redist")) // A lot of steam games contain C++ redistributables. Ignore them
						continue;
					if (file.ToLower().Contains("unins"))
						continue;


					string description = FileVersionInfo.GetVersionInfo(file).FileDescription ?? "";
					description.ToLower();

					FileInfo info = new FileInfo(file);
					string name = info.Name.Substring(0, info.Name.IndexOf('.')).ToLower();

					// Perform a check against the acronym
					if (letters.Length > 2)
					{
						if (letters.ToLower().Contains(name) || name.Contains(letters.ToLower()))
							return file;

						else if (!string.IsNullOrEmpty(description) &&
							(letters.ToLower().Contains(description) || description.Contains(letters.ToLower())))
							return file;
					}

					foreach (string word in words)
					{
						if (string.IsNullOrEmpty(word) || word.Length < 3)
							continue;

						if (name.Contains(word.ToLower()))
							return file;

						if (!string.IsNullOrEmpty(description) && description.Contains(word.ToLower()))
							return file;
					}
				}
			}

			// If search failed, we need to compare the exe files against the name of root directory
			{
				string[] words = { "" };
				string letters = "";

				int pathLen = strPath.LastIndexOf('\\');
				if (pathLen < 0)
				{
					pathLen = strPath.LastIndexOf('/');
					if (pathLen < 0)
					{
						pathLen = 0;
					}
					else
						words = strPath.Substring(strPath.LastIndexOf('/')).Split(new char[] { ' ', '-', '_', ':' });
				}
				else
					words = strPath.Substring(strPath.LastIndexOf('\\')).Split(new char[] { ' ', '-', '_', ':' });
				
				foreach (string word in words)
				{
					if (!string.IsNullOrEmpty(word))
						letters += word[0];
				}

				foreach (string file in exeFiles)
				{
					//CLogger.LogInfo("*** {0}", file);
					if (file.ToLower().Contains("redist")) // A lot of steam games contain C++ redistributables. Ignore them
						continue;
					if (file.ToLower().Contains("unins"))
						continue;

					string description = FileVersionInfo.GetVersionInfo(file).FileDescription ?? "";
					description.ToLower();

					FileInfo info = new FileInfo(file);
					string name = info.Name.Substring(0, info.Name.IndexOf('.')).ToLower();

					// Perform a check against the acronym
					if (letters.Length > 2)
					{
						if (letters.ToLower().Contains(name) || name.Contains(letters.ToLower()))
							return file;

						else if (!string.IsNullOrEmpty(description) &&
							(letters.ToLower().Contains(description) || description.Contains(letters.ToLower())))
							return file;
					}

					foreach (string word in words)
					{
						if (string.IsNullOrEmpty(word) || word.Length < 3)
							continue;

						if (name.Contains(word.ToLower()))
							return file;

						if (!string.IsNullOrEmpty(description) && description.Contains(word.ToLower()))
							return file;
					}
				}
			}
			return "";
		}

		/// <summary>
		/// Find and import games from the binaries found in the "CustomGames" directory
		/// </summary>
		public static void ImportFromFolder(ref CGameData.CTempGameSet tempGameSet)
		{
			CheckCustomFolder();
			FindCustomLinkFiles(ref tempGameSet);
			FindCustomBinaries(ref tempGameSet);
		}

		/// <summary>
		/// Search the "CustomGames" folder for file shortcuts (.lnk) to import.
		/// </summary>
		/// http://www.saunalahti.fi/janij/blog/2006-12.html#d6d9c7ee-82f9-4781-8594-152efecddae2
		private static void FindCustomLinkFiles(ref CGameData.CTempGameSet tempGameSet)
		{
			List<string> fileList = Directory.EnumerateFiles(GAME_FOLDER_NAME, "*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".lnk")).ToList();

			foreach (string file in fileList)
			{
				string strPathOnly		= Path.GetDirectoryName(file);
				strPathOnly				= Path.GetFullPath(strPathOnly);
				string strFilenameOnly	= Path.GetFileName(file);

				Shell32.Shell shell = new Shell32.Shell();
				Shell32.Folder folder			= shell.NameSpace(strPathOnly);
				Shell32.FolderItem folderItem	= folder.ParseName(strFilenameOnly);
				if (folderItem != null)
				{
					Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
					string strID			= Path.GetFileNameWithoutExtension(file);
					string strTitle			= strID;
					CLogger.LogDebug($"- {strTitle}");
					string strLaunch		= link.Path;
					string strUninstall		= "";  // N/A
					string strAlias			= CRegScanner.GetAlias(strTitle);
					if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
						strAlias = "";
					tempGameSet.InsertGame(strID, strTitle, strLaunch, strLaunch, strUninstall, true, false, true, false, strAlias, CUSTOM_PLATFORM, 0f);
				}
			}
		}

		/// <summary>
		/// Search the "CustomGames" folder for binaries (.exe) files to import
		/// </summary>
		private static void FindCustomBinaries(ref CGameData.CTempGameSet tempGameSet)
		{
			List<string> fileList = Directory.EnumerateFiles(GAME_FOLDER_NAME, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".exe")).ToList();

			// Big Fish Games may use .bfg for executables
			//fileList.AddRange(Directory.EnumerateFiles(GAME_FOLDER_NAME, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".bfg")).ToList());

			foreach (string file in fileList)
			{
				string strID			= Path.GetFileNameWithoutExtension(file);
				string strTitle			= strID;
				CLogger.LogDebug($"- {strTitle}");
				string strLaunch		= Path.GetFullPath(file);
				string strUninstall		= ""; // N/A
				string strAlias			= CRegScanner.GetAlias(strTitle);
				if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
					strAlias = "";
				tempGameSet.InsertGame(strID, strTitle, strLaunch, strLaunch, strUninstall, true, false, true, false, strAlias, CUSTOM_PLATFORM, 0f);
			}
		}

		/// <summary>
		/// Check if the folders "CustomImages" and "CustomGames" exist in the same directory as the application - create if not found
		/// </summary>
		public static void CheckCustomFolder()
		{
			if (!Directory.Exists(IMAGE_FOLDER_NAME))
			{
				Directory.CreateDirectory(IMAGE_FOLDER_NAME);
			}
			if (!Directory.Exists(GAME_FOLDER_NAME))
			{
				Directory.CreateDirectory(GAME_FOLDER_NAME);
			}
		}
	}
}
