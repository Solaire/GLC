using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GameHub_Console
{
	/// <summary>
	/// Class for finding game binary (.exe) and link/shortcus (.lnk) files inside directories.
	/// </summary>
	public static class CGameFinder
	{
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
			 * Find all exe files in the directory. (Exclude files with words: "redist", "x86", "x64")
			 * Compare exe files with gameTitle
			 * If failed, compare with name of root directory
			 * Return "" if failed
			 */

			// Get our list of exe files in the game folder + all subfolders
			List<string> exeFiles = Directory.EnumerateFiles(strPath, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".exe")).ToList();

			// If only 1 file has been found, return it.
			if(exeFiles.Count == 1)
			{
				return exeFiles[0];
			}

			// First, compare the files against the game title
			{
				string[] words = strTitle.Split(new char[] { ' ', ':', '-', '_' });
				string letters = "";

				foreach(string word in words)
				{
					if(word != "")
						letters += word[0];
				}

				foreach(string file in exeFiles)
				{
					if(file.ToLower().Contains("redist")) // A lot of steam games contain C++ redistributables. Ignore them
						continue;


					string description = FileVersionInfo.GetVersionInfo(file).FileDescription ?? "";
					description.ToLower();

					FileInfo info = new FileInfo(file);
					string name = info.Name.Substring(0, info.Name.IndexOf('.')).ToLower();

					// Perform a check againt the acronym
					if(letters.Length > 2)
					{
						if(letters.ToLower().Contains(name) || name.Contains(letters.ToLower()))
							return file;

						else if((description != null && description != "") &&
						(letters.ToLower().Contains(description) || description.Contains(letters.ToLower())))
							return file;
					}

					foreach(string word in words)
					{
						if(word == "" && word.Length < 3)
							continue;

						if(name.Contains(word.ToLower()))
							return file;

						if((description != null && description != "") && description.Contains(word.ToLower()))
							return file;
					}
				}
			}

			// If search failed, we need to compare the exe files against the name of root directory
			{
				string[] words = strPath.Substring(strPath.LastIndexOf('\\')).Split(new char[] { ' ', '-', '_', ':' });
				string letters = "";

				foreach(string word in words)
				{
					if(word != "")
						letters += word[0];
				}

				foreach(string file in exeFiles)
				{
					if(file.ToLower().Contains("redist")) // A lot of steam games contain C++ redistributables. Ignore them
						continue;

					string description = FileVersionInfo.GetVersionInfo(file).FileDescription ?? "";
					description.ToLower();

					FileInfo info = new FileInfo(file);
					string name = info.Name.Substring(0, info.Name.IndexOf('.')).ToLower();

					// Perform a check againt the acronym
					if(letters.Length > 2)
					{
						if(letters.ToLower().Contains(name) || name.Contains(letters.ToLower()))
							return file;

						else if((description != null && description != "") &&
						(letters.ToLower().Contains(description) || description.Contains(letters.ToLower())))
							return file;
					}

					foreach(string word in words)
					{
						if(word == "" && word.Length < 3)
							continue;

						if(name.Contains(word.ToLower()))
							return file;

						if((description != null && description != "") && description.Contains(word.ToLower()))
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

			foreach(string file in fileList)
			{
				string strPathOnly		= Path.GetDirectoryName(file);
				strPathOnly				= Path.GetFullPath(strPathOnly);
				string strFilenameOnly	= Path.GetFileName(file);

				Shell32.Shell shell = new Shell32.Shell();
				Shell32.Folder folder			= shell.NameSpace(strPathOnly);
				Shell32.FolderItem folderItem	= folder.ParseName(strFilenameOnly);
				if(folderItem != null)
				{
					Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
					string strTitle  = Path.GetFileNameWithoutExtension(file);
					string strLaunch = link.Path;
					tempGameSet.InsertGame(strTitle, strLaunch, false, CUSTOM_PLATFORM, 0f);
				}
			}
		}

		/// <summary>
		/// Search the "CustomGames" folder for binaries (.exe) files to import
		/// </summary>
		private static void FindCustomBinaries(ref CGameData.CTempGameSet tempGameSet)
		{
			List<string> fileList = Directory.EnumerateFiles(GAME_FOLDER_NAME, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".exe")).ToList();

			foreach(string file in fileList)
			{
				string strTitle  = Path.GetFileNameWithoutExtension(file);
				string strLaunch = Path.GetFullPath(file);
				tempGameSet.InsertGame(strTitle, strLaunch, false, CUSTOM_PLATFORM, 0f);
			}
		}

		/// <summary>
		/// Check if the folder "CustomGames" exists in the same directory as the application - create if not found
		/// </summary>
		public static void CheckCustomFolder()
		{
			if(!Directory.Exists(GAME_FOLDER_NAME))
			{
				Directory.CreateDirectory(GAME_FOLDER_NAME);
			}
		}
	}
}
