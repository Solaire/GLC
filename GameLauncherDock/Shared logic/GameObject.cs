using IconExtractor;
using System.Diagnostics;
using System.Drawing;

namespace GameLauncherDock
{
	public class CGameObject
	{
		private string m_strTitle;
		private string m_strCommand;
		private string m_strPlatform;
		private string m_strIconPath;
		private bool m_bisExternal;
		private bool m_bIsFavourite;
		private Icon m_icon;

		/// <summary>
		/// Constructor.
		/// Set up member variables
		/// </summary>
		public CGameObject(string strTitle, string strcommand, string strPlatform, bool bIsExternal, bool bIsFavourite, string strIconPath)
		{
			m_strTitle		= strTitle;
			m_strCommand	= strcommand;
			m_strPlatform	= strPlatform;
			m_bisExternal	= bIsExternal;
			m_bIsFavourite	= bIsFavourite;
			m_strIconPath	= strIconPath;
			m_icon			= SetIcon(m_strIconPath);
		}

		#region Getters / setters

		/// <summary>
		/// Reset member variables to their defulat values
		/// </summary>
		public void Reset()
		{
			m_strTitle		= "";
			m_strCommand	= "";
			m_strPlatform	= "";
			m_bisExternal	= false;
			m_bIsFavourite	= false;
			m_icon			= null;
		}

		/// <summary>
		/// Get or set the game title
		/// </summary>
		public string GameTitle
		{
			get
			{
				return m_strTitle;
			}
			set
			{
				m_strTitle = value;
			}
		}

		/// <summary>
		/// Get or set the game's launch command
		/// </summary>
		public string LaunchCommand
		{
			get
			{
				return m_strCommand;
			}
			set
			{
				m_strCommand = value;
			}
		}

		/// <summary>
		/// Get or set the game's platform
		/// </summary>
		public string Platform
		{
			get
			{
				return m_strPlatform;
			}
			set
			{
				m_strPlatform = value;
			}
		}

		/// <summary>
		/// Get or set the games icon path
		/// </summary>
		public string IconPath
		{
			get
			{
				return m_strIconPath;
			}
			set
			{
				m_strIconPath = value;
			}
		}

		/// <summary>
		/// Get or set the flag which will marks the game as 'manually added'.
		/// Games with this flag will need to be manually removed from the game list
		/// </summary>
		public bool External
		{
			get
			{
				return m_bisExternal;
			}
			set
			{
				m_bisExternal = value;
			}
		}

		/// <summary>
		/// Get or set the flag which marks the game as 'favourite'.
		/// Games with this flag will appear inthe 'Faviourites' tab.
		/// </summary>
		public bool Favourite
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
		/// Get the game icon
		/// </summary>
		public Icon GameIcon
		{
			get
			{
				return m_icon;
			}
		}
		#endregion

		#region Public functions

		/// <summary>
		/// Attempt to start the game and close this program.
		/// </summary>
		public void StartGame()
		{
			if(!IsGogGame())
			{
				Process.Start(m_strCommand);
			}
			else // For online games (e.g. Gwent), the client needs to be launched first. Otherwise the game might produce a 'no connection error'
			{
				ProcessStartInfo gogProcess = new ProcessStartInfo();
				string clientPath = m_strCommand.Substring(0, m_strCommand.IndexOf('.') + 4);
				string arguments = m_strCommand.Substring(m_strCommand.IndexOf('.') + 4);
				gogProcess.FileName = clientPath;
				gogProcess.Arguments = arguments;
				Process.Start(gogProcess);
			}

			System.Windows.Application.Current.Shutdown(); // Close the program when launching a game
		}

		#endregion

		#region Private functions

		/// <summary>
		/// Extract the best icon from the path and return it
		/// </summary>
		private Icon SetIcon(string strIconPath)
		{
			IconExtractor.IconExtractor extractor = new IconExtractor.IconExtractor(strIconPath);
			Icon icon = extractor.GetIcon(0);
			Icon[] allIcons = IconUtilities.Split(icon);

			int largestIcon = 0;

			for(int i = 0; i <allIcons.Length; i++)
			{
				if(allIcons[largestIcon].Height < allIcons[i].Height)
				{
					largestIcon = i;
				}
				else if(allIcons[largestIcon].Height == allIcons[i].Height)
				{
					int largestIconbits = IconUtilities.GetIconData(allIcons[largestIcon]).Length;
					int indexIconBits = IconUtilities.GetIconData(allIcons[i]).Length;

					if(largestIconbits < indexIconBits)
					{
						largestIcon = i;
					}
				}
			}

			return allIcons[largestIcon];
		}

		/// <summary>
		/// check if the game's platform is GOG
		/// </summary>
		/// <returns>True if platform is 'gog', otherwise false</returns>
		private bool IsGogGame()
		{
			return m_strPlatform.ToLower() == "gog";
		}

		#endregion
	}
}
