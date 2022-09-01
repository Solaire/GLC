using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Logger;
using System.Linq;
using BasePlatformExtension;
using core.Game;

namespace ReferencePlatform
{
	/// <summary>
	/// Reference scanner implementation
	/// </summary>
	public sealed class CReferenceScanner : CBasePlatformScanner
	{
		private const string REFERENCE_NAME      = "Reference Platform";

		public CReferenceScanner(int platformID)
			: base(platformID)
		{
		}

		/// <summary>
		/// Generate 10 installed game objects
		/// </summary>
		/// <param name="expensiveIcons">TODO: unused</param>
		/// <returns>HashSet with 10 generated game objects</returns>
		public override HashSet<GameObject> GetInstalledGames(bool expensiveIcons)
		{
			HashSet<GameObject> games = new HashSet<GameObject>();
			for(int i = 0; i < 10; i++)
            {
				string title  = $"Installed ref game {i}";
				string id     = $"ref_game_{i}";
				string alias  = $"IREF{i}";
				string launch = $"START_REF_{i}";

				games.Add(new GameObject(title, m_platformID, id, alias, launch, ""));
            }
			return games;
		}

		/// <summary>
		/// Generate 10 non-installed game objects
		/// </summary>
		/// <param name="expensiveIcons">TODO: unused</param>
		/// <returns>HashSet with 10 generated game objects</returns>
		public override HashSet<GameObject> GetNonInstalledGames(bool expensiveIcons)
		{
			HashSet<GameObject> games = new HashSet<GameObject>();
			for(int i = 0; i < 10; i++)
			{
				string title  = $"Non-installed ref game {i}";
				string id     = $"ref_game_{i}";

				games.Add(new GameObject(title, m_platformID, id, "", "", ""));
			}
			return games;
		}
	}
}
