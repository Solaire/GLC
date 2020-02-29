using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLauncher_Console
{
	/// <summary>
	/// Contains the definition of the game data and its manipulation logic
	/// </summary>
	static public class CGameData
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
			Custom		= 9,
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
			"Custom"
		};


		/// <summary>
		/// Contains information about a game
		/// </summary>
		public class CGame
		{
			private readonly string		  m_strTitle;
			private readonly string		  m_strLaunch;
			private readonly bool		  m_bIsFavourite;
			private readonly GamePlatform m_platfrom;
			//private readonly string		m_strIcon; // Currently not in use

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="strTitle">Title of the game</param>
			/// <param name="strLaunchCommand">Game's launch command</param>
			/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
			/// <param name="enumPlatform">Game's platform enumerator</param>
			protected CGame(string strTitle, string strLaunchCommand, bool bIsFavourite, GamePlatform enumPlatform)
			{
				m_strTitle		= strTitle;
				m_strLaunch		= strLaunchCommand;
				m_bIsFavourite	= bIsFavourite;
				m_platfrom		= enumPlatform; 
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
			/// <param name="strLaunchCommand">Game's launch command</param>
			/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
			/// <param name="enumPlatform">Game's platform enumerator</param>
			public CGameInstance(string strTitle, string strLaunchCommand, bool bIsFavourite, GamePlatform enumPlatform) 
				: base(strTitle, strLaunchCommand, bIsFavourite, enumPlatform)
			{

			}
		}

		/// <summary>
		/// Create a new CGame instance
		/// </summary>
		/// <param name="strTitle">Title of the game</param>
		/// <param name="strLaunchCommand">Game's launch command</param>
		/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
		/// <param name="enumPlatform">Game's platform enumerator</param>
		/// <returns>Instance of CGame</returns>
		private static CGame CreateGameInstance(string strTitle, string strLaunchCommand, bool bIsFavourite, GamePlatform enumPlatform)
		{
			return new CGameInstance(strTitle, strLaunchCommand, bIsFavourite, enumPlatform);
		}

		private static Dictionary<GamePlatform, HashSet<CGame>> m_games = new Dictionary<GamePlatform, HashSet<CGame>>();

		/// <summary>
		/// Return the list of Game objects with specified platform
		/// </summary>
		/// <param name="enumPlatform">Platform enumerator</param>
		/// <returns>List of Game objects</returns>
		public static HashSet<CGame> GetGames(GamePlatform enumPlatform)
		{
			return m_games[enumPlatform];
		}

		/// <summary>
		/// Remove all games from memory
		/// </summary>
		/// <param name="bRemoveCustom">If true, will remove manually added games</param>
		public static void ClearGames(bool bRemoveCustom)
		{
			if(bRemoveCustom)
				m_games.Clear();

			else
			{
				foreach(KeyValuePair<GamePlatform, HashSet<CGame>> gameSet in m_games)
				{
					if(gameSet.Key != GamePlatform.Custom)
						gameSet.Value.Clear();
				}
			}
		}

		/// <summary>
		/// Return all games
		/// </summary>
		/// <returns>List of Game objects</returns>
		public static HashSet<CGame> GetAllGames()
		{
			List<CGame> allGames = new List<CGame>();

			foreach(KeyValuePair<GamePlatform, HashSet<CGame>> platform in m_games)
			{
				allGames.AddRange(platform.Value);
			}

			return new HashSet<CGame>(allGames);
		}

		/// <summary>
		/// Return titles of all games
		/// </summary>
		/// <returns>List of strings</returns>
		public static List<string> GetAllTitles()
		{
			List<string> allTitles = new List<string>();

			foreach(KeyValuePair<GamePlatform, HashSet<CGame>> keyValuePair in m_games)
			{
				foreach(CGame game in keyValuePair.Value)
				{
					allTitles.Add(game.Title);
				}
			}

			return allTitles;
		}

		/// <summary>
		/// Return all platform with at least 1 game along with the number of games per platform
		/// </summary>
		/// <returns>List of strings</returns>
		public static List<string> GetPlatformNames()
		{
			List<string> platforms = new List<string>();
			int nTotalCount = 0;

			foreach(KeyValuePair<GamePlatform, HashSet<CGame>> keyValuePair in m_games)
			{
				string strPlatform = m_strings[(int)keyValuePair.Key] + ": " + keyValuePair.Value.Count;
				platforms.Add(strPlatform);
				nTotalCount += keyValuePair.Value.Count;
			}
			string strAll = "All: " + nTotalCount;
			platforms.Add(strAll);
			return platforms;
		}

		/// <summary>
		/// Return titles of games for specified platform
		/// </summary>
		/// <param name="enumPlatform">Platform enumerator</param>
		/// <returns>List of strings</returns>
		public static List<string> GetPlatformTitles(GamePlatform enumPlatform)
		{
			List<string> platformTitles = new List<string>();
			
			if(enumPlatform == GamePlatform.All)
			{
				return GetAllTitles();
			}

			if(m_games.ContainsKey(enumPlatform))
			{
				foreach(CGame game in m_games[enumPlatform])
				{
					platformTitles.Add(game.Title);
				}
			}

			return platformTitles;
		}

		/// <summary>
		/// Add game to the dictionary
		/// </summary>
		/// <param name="strTitle">Title of the game</param>
		/// <param name="strLaunchCommand">Game's launch command</param>
		/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
		/// <param name="strPlatform">Game's platform as a string value</param>
		public static void AddGame(string strTitle, string strLaunchCommand, bool bIsFavourite, string strPlatform)
		{
			// If platform is incorrect or unsupported, default to custom.
			GamePlatform enumPlatform;
			if(!Enum.TryParse(strPlatform, true, out enumPlatform))
			{
				enumPlatform = GamePlatform.Custom;
			}

			// If this is the first entry in the key, we need to initialise the list
			if(!m_games.ContainsKey(enumPlatform))
			{
				m_games[enumPlatform] = new HashSet<CGame>();
			}

			m_games[enumPlatform].Add(CreateGameInstance(strTitle, strLaunchCommand, bIsFavourite, enumPlatform));
		}

		/// <summary>
		/// Resolve platform enum from a string
		/// </summary>
		/// <param name="strPlatformName">Platform as a string input</param>
		/// <returns>GamePlatform enumerator, cast to int type. -1 on failed resolution</returns>
		public static int GetPlatformEnum(string strPlatformName)
		{
			GamePlatform enumPlatform;
			if(!Enum.TryParse(strPlatformName, true, out enumPlatform))
			{
				return -1;
			}

			return (int)enumPlatform;
		}

		/// <summary>
		/// Get selected platform as a string
		/// </summary>
		/// <param name="enumPlatform">GamePlatform enumerator</param>
		/// <returns>String value of the platform</returns>
		public static string GetPlatformString(int enumPlatform)
		{
			if(enumPlatform > m_strings.Length)
			{
				return "";
			}

			return m_strings[enumPlatform];
		}

		public static CGame GetGame(int nPlatformEnum, int nGameIndex)
		{
			return m_games[(GamePlatform)nPlatformEnum].ElementAt(nGameIndex);
		}
	}
}