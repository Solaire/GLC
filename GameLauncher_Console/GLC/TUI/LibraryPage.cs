using System;
using System.Collections.Generic;
using System.Collections;
using ConsoleUI.Base;
using ConsoleUI.Type;
using ConsoleUI.Views;
using ConsoleUI.Helper;

using LibGLC;

namespace GLC
{
    class CLibraryPage : CPage
    {
        private List<string> m_platforms;

        private CListView m_platformListView;
        private CListView m_gameListView;
        private CInfoboxView m_gameInfoView;

        private CGameObject     m_gameObject;
        private CPlatformObject m_platformObject;

        public CLibraryPage(string title, ConsoleRect rect, int componentCount, CFrame parent) : base(title, rect, componentCount, parent)
        {
            m_platforms = new List<string>();

            for(int i = 0; i < 15; i++)
            {
                m_platforms.Add(string.Format("Platform -> {0}", i));
            }

            m_platformListView = new CListView(CConstants.PANEL_DATA[0], new CListWrapper(m_platforms), true , this);
            m_gameListView     = new CListView(CConstants.PANEL_DATA[1], new CListWrapper(null)       , false, this);
            m_gameInfoView     = new CInfoboxView(CConstants.PANEL_DATA[2], null, false, this);

            m_gameObject      = new CGameObject();
            m_platformObject  = new CPlatformObject();
        }

        public override void Initialise()
        {
            // Add to array
            m_views[0] = m_platformListView;
            m_views[1] = m_gameListView;
            m_views[2] = m_gameInfoView;

            // Add any event handlers
            m_platformListView.SelectedItemChanged  += PlatformListView_SelectedChange;
            m_gameListView.SelectedItemChanged      += GameListView_SelectedChange;

            // Set the view sizes
            CalculateComponentPositions();
        }

        public override void KeyPress(ConsoleKeyInfo keyInfo)
        {
            if(keyInfo.Key == ConsoleKey.Tab)
            {
                m_views[m_focusedComponent].Focused = false;
                m_focusedComponent = (m_focusedComponent == 0) ? 1 : 0;
                m_views[m_focusedComponent].Focused = true;
            }
            else
            {
                m_views[m_focusedComponent].KeyPress(keyInfo);
            }
        }

        public void PlatformListView_SelectedChange(CListViewItemEventArgs e)
        {
            List<Game> newGameList;
            string newPlatform = e.Value.ToString();
            newGameList = m_gameObject.GetByPlatform(newPlatform); // Will handle all games
            m_gameListView.SetSource(new GameListDataSource(newGameList));
            m_gameListView.Draw(true);
            /*
            m_gameListView.Clear();
            for(int i = 0; i < 15; i++)
            {
                m_gameListView.Add(string.Format("Category {0}: Item {1}", e.Item, i));
            }
            m_gameListView.Draw(true);
            */
        }

        public void GameListView_SelectedChange(CListViewItemEventArgs e)
        {
            string selectedGame = e.Value.ToString();
            Game newGame = m_gameObject.GetByName(selectedGame);
            m_gameInfoView.SetSource(new CGameInfoDataSource(newGame));
            m_gameInfoView.Draw(true);
        }

        internal class GameListDataSource : IListDataSource
        {
            private List<Game> m_source;
            private int   m_focusIndex;

            public GameListDataSource(List<Game> gameList)
            {
                if(gameList != null)
                {
                    m_focusIndex = -1;
                    m_source = gameList;
                }
            }

            public int Count { get { return (m_source != null) ? m_source.Count : 0; } }

            public void Render(int startX, int startY, int lengthX, int first, int count, ConsoleColor colourFG, ConsoleColor colourBG)
            {
                if(m_source == null)
                {
                    return;
                }

                for(int i = 0; i < count && first + i < Count; i++)
                {
                    CConsoleDraw.WriteText(m_source[first + i].name.PadRight(lengthX), startX, startY++, colourFG, colourBG);
                }
            }

            public void Render(int startX, int startY, int lengthX, int index, ConsoleColor colourFG, ConsoleColor colourBG)
            {
                if(m_source == null || (index < 0 || index >= Count))
                {
                    return;
                }

                CConsoleDraw.WriteText(m_source[index].name.PadRight(lengthX), startX, startY, colourFG, colourBG);
            }

            public bool IsFocused(int index)
            {
                if(m_source == null || (index < 0 || index >= Count))
                {
                    return false;
                }
                return m_focusIndex == index;
            }

            public void SetFocused(int index)
            {
                if(m_source == null || (index < 0 || index >= Count))
                {
                    m_focusIndex = -1;
                }
                m_focusIndex = index;
            }

            public IList ToList()
            {
                return m_source;
            }
        }

        internal class CGameInfoDataSource : IInfoboxDataSource
        {
            private Game m_game;

            public CGameInfoDataSource(Game game)
            {
                m_game = game;
            }

            public void Render(int startX, int startY, int lengthX, ConsoleColor colourFG, ConsoleColor colourBG)
            {
                CConsoleDraw.WriteText(m_game.name.PadRight(lengthX), startX, startY, colourFG, colourBG);
            }
        }
    }
}
