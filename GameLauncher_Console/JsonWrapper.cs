using Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GameLauncher_Console
{
	/// <summary>
	/// Class for serializing and deserializing JSON data. 
	/// JSON data is stored and handled in a dynamically sized dictionary structure.
	/// </summary>
	static class CJsonWrapper
	{
		// Relative path to the games.json config file (the file should be in the same directory as the executable)
		private const string GAME_JSON_FILE			= @".\games.json";

		// JSON field names
		private const string GAMES_ARRAY			= "games";
		private const string GAMES_ARRAY_TITLE		= "title";
		private const string GAMES_ARRAY_LAUNCH		= "launch";
		private const string GAMES_ARRAY_PLATFORM	= "platform";
		private const string GAMES_ARRAY_FAVOURITE	= "favourite";

		/// <summary>
		/// Import games from the game.json config file
		/// </summary>
		/// <returns>True if successful, otherwise false</returns>
		public static bool ImportFromJSON()
		{
			int nGameCount = 0;
			if(!DoesFileExist())
			{
				CreateEmptyFile();
				return false;
			}
			ImportGames(ref nGameCount);

			if(nGameCount < 1)
			{
				CLogger.LogDebug("JSON file is empty - scanning for games...");
				CRegScanner.ScanGames();
			}
			return true;
		}

		/// <summary>
		/// Export game data from memory to the game.json config file
		/// NOTE: At the moment, the program will pretty pretty much create a brand new JSON file and override all of the content...
		/// ... I need to find a nice workaround as JsonDocument class is read-only.
		/// </summary>
		/// <returns>True is successful, otherwise false</returns>
		public static bool Export(List<CGameData.CGame> gameList)
		{
			var options = new JsonWriterOptions
			{
				Indented = true
			};

			try
			{
				using(var stream = new MemoryStream())
				{
					using(var writer = new Utf8JsonWriter(stream, options))
					{
						writer.WriteStartObject();
						writer.WriteStartArray(GAMES_ARRAY);
						for(int i = 0; i < gameList.Count; i++)
						{
							WriteGame(writer, stream, options, gameList[i]);
						}
						writer.WriteEndArray();
						writer.WriteEndObject();
					}

					string strJsonData = Encoding.UTF8.GetString(stream.ToArray());
					byte[] bytes = new UTF8Encoding(true).GetBytes(strJsonData);

					using(FileStream fs = File.Create(GAME_JSON_FILE))
					{
						fs.Write(bytes, 0, bytes.Length);
					}
				}
				
			}
			catch(Exception ex)
			{
				CLogger.LogError(ex);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Check if the games.json file exists.
		/// </summary>
		/// <returns>True if exists, otherwise false</returns>
		private static bool DoesFileExist()
		{
			return File.Exists(GAME_JSON_FILE);
		}

		/// <summary>
		/// Create empty games.json file with the empty array
		/// </summary>
		/// <returns>True if file created, otherwise false</returns>
		private static bool CreateEmptyFile()
		{
			var options = new JsonWriterOptions
			{
				Indented = true
			};

			try
			{
				using(var stream = new MemoryStream())
				{
					using(var writer = new Utf8JsonWriter(stream, options))
					{
						writer.WriteStartObject();
						writer.WriteStartArray(GAMES_ARRAY);
						writer.WriteEndArray();
						writer.WriteEndObject();
					}

					string strJsonData = Encoding.UTF8.GetString(stream.ToArray());
					byte[] bytes = new UTF8Encoding(true).GetBytes(strJsonData);

					using(FileStream fs = File.Create(GAME_JSON_FILE))
					{
						fs.Write(bytes, 0, bytes.Length);
					}
				}
			}
			catch(Exception ex)
			{
				CLogger.LogError(ex);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Import games from the json file and add them to the global game dictionary.
		/// </summary>
		private static void ImportGames(ref int nGameCount)
		{
			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			string strDocumentData = File.ReadAllText(GAME_JSON_FILE);

			using(JsonDocument document = JsonDocument.Parse(strDocumentData, options))
			{
				JsonElement jArrGames = document.RootElement.GetProperty(GAMES_ARRAY);

				foreach(JsonElement jElement in jArrGames.EnumerateArray())
				{
					string strTitle		= jElement.GetProperty(GAMES_ARRAY_TITLE).GetString();
					string strLaunch	= jElement.GetProperty(GAMES_ARRAY_LAUNCH).GetString();
					string strPlatform	= jElement.GetProperty(GAMES_ARRAY_PLATFORM).GetString();
					bool   bFavourite	= jElement.GetProperty(GAMES_ARRAY_FAVOURITE).GetBoolean();

					CGameData.AddGame(strTitle, strLaunch, bFavourite, strPlatform);
					nGameCount++;
				}
			}
		}

		/// <summary>
		/// Create a game JSON object and add write it with JsonWriter
		/// </summary>
		/// <param name="writer">JsonWriter object</param>
		/// <param name="stream">MemoryStream object</param>
		/// <param name="options">JsonWriter options struct</param>
		/// <param name="data">Game data</param>
		private static void WriteGame(Utf8JsonWriter writer, MemoryStream stream, JsonWriterOptions options, CGameData.CGame data)
		{
			writer.WriteStartObject();
			writer.WriteString(GAMES_ARRAY_TITLE		, data.Title);
			writer.WriteString(GAMES_ARRAY_LAUNCH		, data.Launch);
			writer.WriteString(GAMES_ARRAY_PLATFORM		, data.PlatformString);
			writer.WriteBoolean(GAMES_ARRAY_FAVOURITE	, data.IsFavourite);
			writer.WriteEndObject();
		}
	}
}
