using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameLauncherDock.Logic
{
	class CGameManager : CXML
	{
		private static readonly CGameManager m_defaultInstance = new CGameManager();

		private static List<CGameObject> m_gameObjectList;

		private CGameManager() : base("Game")
		{
			m_gameObjectList = new List<CGameObject>();

			if(!Exists(m_strDocumentPath))
			{
				Console.WriteLine("Game XML not found in {0}. Creaing new document", m_strDocumentPath);
				CreateEmptyXML();
				return;
			}
			m_xDoc = XDocument.Load(m_strDocumentPath);
			if(IsEmpty())
			{
				Console.WriteLine("Game XML loaded, but it is empty.");
				return;
			}
			LoadGameData();
		}

		/// <summary>
		/// Load games from the XML file into the list of games
		/// </summary>
		private void LoadGameData()
		{
			foreach(XElement element in m_xDoc.Descendants("Game"))
			{
				string strName			= element.Element("Name").Value;
				string strLaunchCommand = element.Element("LaunchCommand").Value;
				string strPlatform		= element.Element("Platform").Value;
				string strIcon			= element.Element("Icon").Value;
				string strFavourite		= element.Element("Faviourite").Value;
				string strExternal		= element.Element("External").Value;

				bool bFavourite = strFavourite[0] == '1';
				bool bExternal = strExternal[0] == '1';

				m_gameObjectList.Add(new CGameObject(strName, strLaunchCommand, strPlatform, bExternal, bFavourite, strIcon));
			}
		}

		/// <summary>
		/// Write many games to the XML file
		/// </summary>
		private void WriteManyGames()
		{
			for(int i = 0; i < m_gameObjectList.Count; i++)
			{
				string strName			= m_gameObjectList[i].GameTitle;
				string strLaunchCommand = m_gameObjectList[i].LaunchCommand;
				string strPlatform		= m_gameObjectList[i].Platform;
				string strIcon			= m_gameObjectList[i].IconPath;
				char charFavourite		= m_gameObjectList[i].External  ? '1' : '0';
				char charExternal		= m_gameObjectList[i].Favourite ? '1' : '0';

				XElement gameElement = m_xDoc.Element(m_strRootNode);
				XElement gameData    = new XElement("Game", 
											new XElement)
			}
		}

		public static CGameManager DefaultInstance()
		{
			return m_defaultInstance;
		}

		public static List<CGameObject> GameObjectList()
		{
			return m_gameObjectList;
		}
	}
}
