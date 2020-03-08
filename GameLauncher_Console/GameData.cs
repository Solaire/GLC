using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLauncher_Console
{
	/// <summary>
	/// Contains the definition of the game data and its manipulation logic
	/// </summary>
	public static class CGameData
	{
		/// <summary>
		/// Enumerator containing currently supported game platforms
		/// </summary>
		public enum GamePlatform
		{
			Steam		= 0,
			GOG			= 1,
			Uplay		= 2, 
			Origin		= 3,
			Epic		= 4,
			Bethesda	= 5,
			Battlenet	= 6,
			Rockstar	= 7,
			All			= 8,
			Favourites  = 9,
			Custom		= 10,
		}

		private static readonly string[] m_strings =
		{
			"Steam",
			"GOG",
			"Uplay",
			"Origin",
			"Epic",
			"Bethesda",
			"Battlenet",
			"Rockstar",
			"All",
			"Favourites",
			"Custom"
		};


		/// <summary>
		/// Contains information about a game
		/// </summary>
		public class CGame
		{
			private readonly string		  m_strTitle;
			private readonly string		  m_strLaunch;
			private bool				  m_bIsFavourite;
			private readonly GamePlatform m_platfrom;
			//private readonly string	  m_strIcon; // Currently not in use

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="strTitle">Title of the game</param>
			/// <param name="strLaunch">Game's launch command</param>
			/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
			/// <param name="platformEnum">Game's platform enumerator</param>
			protected CGame(string strTitle, string strLaunch, bool bIsFavourite, GamePlatform platformEnum/*, string strIconPath*/)
			{
				m_strTitle		= strTitle;
				m_strLaunch		= strLaunch;
				m_bIsFavourite	= bIsFavourite;
				m_platfrom		= platformEnum; 
				//m_strIcon = strIconPath;
			}

			/// <summary>
			/// Equals override for HashSet comparison.
			/// </summary>
			/// <param name="other">Object to compare against</param>
			/// <returns>True is other is not null and the titles are matching</returns>
			public override bool Equals(object other)
			{
				// We're only interested in comparing the titles
				CGame game = other as CGame;
				return(game != null && this.m_strTitle == game.m_strTitle);
			}

			/// <summary>
			/// Return the hash code of this object's title variable
			/// </summary>
			/// <returns>Hash code</returns>
			public override int GetHashCode()
			{
				return this.m_strTitle.GetHashCode();
			}

			/// <summary>
			/// Title getter
			/// </summary>
			public string Title
			{
				get
				{
					return m_strTitle;
				}
			}

			/// <summary>
			/// Launch command getter
			/// </summary>
			public string Launch
			{
				get
				{
					return m_strLaunch;
				}
			}

			/// <summary>
			/// Favourite flag getter
			/// </summary>
			public bool IsFavourite
			{
				get
				{
					return m_bIsFavourite;
				}
				set
				{
					m_bIsFavourite = value;
				}
			}

			/// <summary>
			/// Platform enumerator getter
			/// </summary>
			public GamePlatform Platform
			{
				get
				{
					return m_platfrom;
				}
			}

			/// <summary>
			/// Platform string getter
			/// </summary>
			public string PlatformString
			{
				get
				{
					return m_strings[(int)m_platfrom];
				}
			}
		}
		
		/// <summary>
		/// Wrapper class for the Game class
		/// The goal is to make the Game class visible to the rest of the client, but make it impossible to create new instances outside of the AddGame() function
		/// </summary>
		private class CGameInstance : CGame
		{
			/// <summary>
			/// Constructor.
			/// Call constructor of base class
			/// </summary>
			/// <param name="strTitle">Title of the game</param>
			/// <param name="strLaunch">Game's launch command</param>
			/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
			/// <param name="platformEnum">Game's platform enumerator</param>
			public CGameInstance(string strTitle, string strLaunch, bool bIsFavourite, GamePlatform platformEnum) 
				: base(strTitle, strLaunch, bIsFavourite, platformEnum)
			{

			}
		}

		/// <summary>
		/// Child class of HashSet which stores CGame objects.
		/// Designed to be used to temporarily store the game objects
		/// </summary>
		public class CTempGameSet : HashSet<CGame>
		{
			/// <summary>
			/// Default constructor
			/// </summary>
			public CTempGameSet()
			{

			}

			/// <summary>
			/// Instert CGame object into the HashSet
			/// </summary>
			/// <param name="strTitle">Game title</param>
			/// <param name="strLaunch">Game launch command</param>
			/// <param name="bIsFavourite">Flag indicating if the game is favourite</param>
			/// <param name="strPlatform">Game platform string</param>
			public void InsertGame(string strTitle, string strLaunch, bool bIsFavourite, string strPlatform)
			{
				GamePlatform platformEnum;
				if(!Enum.TryParse(strPlatform, true, out platformEnum))
					platformEnum = GamePlatform.Custom;

				this.Add(CreateGameInstance(strTitle, strLaunch, bIsFavourite, platformEnum));
			}
		}

		/// <summary>
		/// Create a new CGame instance
		/// </summary>
		/// <param name="strTitle">Title of the game</param>
		/// <param name="strLaunch">Game's launch command</param>
		/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
		/// <param name="platformEnum">Game's platform enumerator</param>
		/// <returns>Instance of CGame</returns>
		private static CGame CreateGameInstance(string strTitle, string strLaunch, bool bIsFavourite, GamePlatform platformEnum)
		{
			return new CGameInstance(strTitle, strLaunch, bIsFavourite, platformEnum);
		}

		private static Dictionary<GamePlatform, HashSet<CGame>> m_gameDictionary = new Dictionary<GamePlatform, HashSet<CGame>>();
		private static HashSet<CGame> m_allGames   = new HashSet<CGame>();
		private static HashSet<CGame> m_favourites = new HashSet<CGame>();

		/// <summary>
		/// Return the list of Game objects with specified platform
		/// </summary>
		/// <param name="platformEnum">Platform enumerator</param>
		/// <returns>List of Game objects</returns>
		public static HashSet<CGame> GetPlatformGameList(GamePlatform platformEnum)
		{
			if(platformEnum == GamePlatform.All)
				return m_allGames;

			else if(platformEnum == GamePlatform.Favourites)
				return m_favourites;

			else
				return m_gameDictionary[platformEnum];
		}

		/// <summary>
		/// Remove all games from memory
		/// </summary>
		/// <param name="bRemoveCustom">If true, will remove manually added games</param>
		public static void ClearGames(bool bRemoveCustom)
		{
			if(bRemoveCustom)
				m_gameDictionary.Clear();

			else
			{
				foreach(KeyValuePair<GamePlatform, HashSet<CGame>> gameSet in m_gameDictionary)
				{
					if(gameSet.Key != GamePlatform.Custom)
						gameSet.Value.Clear();
				}
			}

			m_allGames.Clear();
		}

		/// <summary>
		/// Return all platform with at least 1 game along with the number of games per platform
		/// </summary>
		/// <returns>List of strings</returns>
		public static List<string> GetPlatformNames()
		{
			List<string> platformList = new List<string>();
			foreach(KeyValuePair<GamePlatform, HashSet<CGame>> platform in m_gameDictionary)
			{
				string strPlatform = m_strings[(int)platform.Key] + ": " + platform.Value.Count;
				platformList.Add(strPlatform);
			}
			platformList.Add("All: " + m_allGames.Count);

			if(m_favourites.Count > 0)
				platformList.Add("Favourites: " + m_favourites.Count);

			return platformList;
		}

		/// <summary>
		/// Return titles of games for specified platform
		/// </summary>
		/// <param name="platformEnum">Platform enumerator</param>
		/// <returns>List of strings</returns>
		public static List<string> GetPlatformTitles(GamePlatform platformEnum)
		{
			List<string> platformTitles = new List<string>();
			
			if(platformEnum == GamePlatform.All)
			{
				foreach(CGame game in m_allGames)
				{
					string strTitle = game.Title;
					if(game.IsFavourite)
						strTitle += " [F]";

					platformTitles.Add(strTitle);
				}
			}

			else if(platformEnum == GamePlatform.Favourites)
			{
				foreach(CGame game in m_favourites)
				{
					platformTitles.Add(game.Title);
				}
				return platformTitles;
			}

			else if(m_gameDictionary.ContainsKey(platformEnum))
			{
				foreach(CGame game in m_gameDictionary[platformEnum])
				{
					string strTitle = game.Title;
					if(game.IsFavourite)
						strTitle += " [F]";

					platformTitles.Add(strTitle);
				}
			}

			return platformTitles;
		}

		/// <summary>
		/// Add game to the dictionary
		/// </summary>
		/// <param name="strTitle">Title of the game</param>
		/// <param name="strLaunch">Game's launch command</param>
		/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
		/// <param name="strPlatform">Game's platform as a string value</param>
		public static void AddGame(string strTitle, string strLaunch, bool bIsFavourite, string strPlatform)
		{
			// If platform is incorrect or unsupported, default to custom.
			GamePlatform platformEnum;
			if(!Enum.TryParse(strPlatform, true, out platformEnum))
				platformEnum = GamePlatform.Custom;

			// If this is the first entry in the key, we need to initialise the list
			if(!m_gameDictionary.ContainsKey(platformEnum))
				m_gameDictionary[platformEnum] = new HashSet<CGame>();

			CGame game = CreateGameInstance(strTitle, strLaunch, bIsFavourite, platformEnum);
			m_gameDictionary[platformEnum].Add(game);
			m_allGames.Add(game);

			if(game.IsFavourite)
				m_favourites.Add(game);
		}

		/// <summary>
		/// Function overload
		/// Add game object from all game set to the dictionary
		/// </summary>
		/// <param name="game">instance of CGame to add</param>
		private static void AddGame(CGame game)
		{
			if(!m_gameDictionary.ContainsKey(game.Platform))
				m_gameDictionary[game.Platform] = new HashSet<CGame>();

			m_gameDictionary[game.Platform].Add(game);

			if(game.IsFavourite)
				m_favourites.Add(game);
		}

		/// <summary>
		/// Resolve platform enum from a string
		/// </summary>
		/// <param name="strPlatformName">Platform as a string input</param>
		/// <returns>GamePlatform enumerator, cast to int type. -1 on failed resolution</returns>
		public static int GetPlatformEnum(string strPlatformName)
		{
			GamePlatform platformEnum;
			if(!Enum.TryParse(strPlatformName, true, out platformEnum))
				return -1;

			return (int)platformEnum;
		}

		/// <summary>
		/// Get selected platform as a string
		/// </summary>
		/// <param name="platformEnum">GamePlatform enumerator</param>
		/// <returns>String value of the platform</returns>
		public static string GetPlatformString(int platformEnum)
		{
			if(platformEnum > m_strings.Length)
				return "";

			return m_strings[platformEnum];
		}

		/// <summary>
		/// Return game object for the specified platform
		/// </summary>
		/// <param name="platformEnum">Game's platform index</param>
		/// <param name="nGameIndex">Index of the game list</param>
		/// <returns></returns>
		public static CGame GetPlatformGame(GamePlatform platformEnum, int nGameIndex)
		{
			if(platformEnum == GamePlatform.All)
				return m_allGames.ElementAt(nGameIndex);

			else if(platformEnum == GamePlatform.Favourites)
				return m_favourites.ElementAt(nGameIndex);

			else
				return m_gameDictionary[platformEnum].ElementAt(nGameIndex);
		}

		/// <summary>
		/// Toggle the specified game's favourite flag
		/// </summary>
		/// <param name="platformEnum">Game's platform enumerator</param>
		/// <param name="nGameIndex">Index of the game list</param>
		public static void ToggleFavourite(GamePlatform platformEnum, int nGameIndex)
		{
			CGame gameCopy;
			if(platformEnum == GamePlatform.Favourites)
				gameCopy = m_favourites.ElementAt(nGameIndex);

			else if(platformEnum == GamePlatform.All)
				gameCopy = m_allGames.ElementAt(nGameIndex);

			else
				gameCopy = m_gameDictionary[platformEnum].ElementAt(nGameIndex);

			if(gameCopy.IsFavourite)
			{
				gameCopy.IsFavourite = false;
				m_favourites.Remove(gameCopy);
			}
			else
			{
				gameCopy.IsFavourite = true;
				m_favourites.Add(gameCopy);
			}
		}

		/// <summary>
		/// Remove selected game from memory
		/// </summary>
		/// <param name="game">Game object to remove</param>
		public static void RemoveGame(CGame game)
		{
			if(game.IsFavourite)
				m_favourites.Remove(game);

			m_allGames.Remove(game);
			m_gameDictionary[game.Platform].Remove(game);
		}

		/// <summary>
		/// Merge the temporary game set with the main game set.
		/// Add new games to the main set and remove missing games from the main set.
		/// </summary>
		/// <param name="tempGameSet">Instance of CTempGameSet containing new games</param>
		public static void MergeGameSets(CTempGameSet tempGameSet)
		{
			m_gameDictionary.Clear();
			m_favourites.Clear();

			// Find games that are missing from tempGameSet and remove them from m_allGames
			// Find games that are missing from m_allGames and add them to m_allGames

			HashSet<CGame> newGames = new HashSet<CGame>(tempGameSet);
			newGames.ExceptWith(m_allGames);

			HashSet<CGame> gamesToRemove = new HashSet<CGame>(m_allGames);
			gamesToRemove.ExceptWith(tempGameSet);

			foreach(CGame game in gamesToRemove)
			{
				m_allGames.Remove(game);
			}

			foreach(CGame game in newGames)
			{
				m_allGames.Add(game);
			}

			foreach(CGame game in m_allGames)
			{
				AddGame(game);
			}
		}
	}
}