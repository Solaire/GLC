using System;
using System.Collections.Generic;
using System.Collections;
using ConsoleUI.Base;
using ConsoleUI.Type;
using ConsoleUI.Views;
using ConsoleUI.Helper;

using LibGLC;
using System.Linq;

namespace GLC
{
    class CLibraryPage : CPage
    {
        List<CPlatform.PlatformObject> m_platforms;

        private CListView m_platformListView;
        private CListView m_gameListView;
        private CInfoboxView m_gameInfoView;

        public CLibraryPage(string title, ConsoleRect rect, int componentCount, CFrame parent) : base(title, rect, componentCount, parent)
        {
            m_platforms = CPlatform.GetPlatforms(true);
            m_platforms.Add(new CPlatform.PlatformObject(-1, "All games", CGame.GetGameCount(-1), ""));
            m_platforms.Add(new CPlatform.PlatformObject(0, "Favourites", CGame.GetFavouriteCount(-1), ""));
            m_platforms.Sort((a, b) => a.PlatformID.CompareTo(b.PlatformID));

            CGame.GameSet allGames = CGame.GetAllGames();
            allGames.SortGames(CGame.GameSet.SortFlag.cSortFrequency, false, false);

            m_platformListView = new CListView(CConstants.PANEL_DATA[0], new CPlatformListDataSource(m_platforms), true, this);
            m_gameListView     = new CListView(CConstants.PANEL_DATA[1], new GameListDataSource(allGames.ToList()), false, this);
            m_gameInfoView     = new CInfoboxView(CConstants.PANEL_DATA[2], null, false, this);
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

            m_platformListView.SetFocusedIndex(0);
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
            CPlatform.PlatformObject selection = (CPlatform.PlatformObject)e.Value;
            GameListDataSource newSource = new GameListDataSource(null);

            switch(selection.PlatformID)
            {
                case -1: newSource.SetSource(CGame.GetAllGames().ToList());                 break;
                case 0:  newSource.SetSource(CGame.GetFavouriteGames().ToList());           break;
                default: newSource.SetSource(CGame.GetPlatformGames(selection.PlatformID)); break;
            }

            m_gameListView.SetSource(newSource);
            m_gameListView.Draw(true);
        }

        public void GameListView_SelectedChange(CListViewItemEventArgs e)
        {
            CGame.GameObject gameInfo = (CGame.GameObject)e.Value;
            m_gameInfoView.SetSource(new CGameInfoDataSource(gameInfo));
            m_gameInfoView.Draw(true);
        }

        /// <summary>
        /// Data source implementation for the platform list view
        /// </summary>
        internal class CPlatformListDataSource : IListDataSource
        {
            private List<CPlatform.PlatformObject> m_source;
            private int   m_focusIndex;

            public CPlatformListDataSource(List<CPlatform.PlatformObject> platformList)
            {
                if(platformList != null)
                {
                    m_focusIndex = -1;
                    m_source = platformList;
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
                    CConsoleDraw.WriteText(m_source[first + i].Name.PadRight(lengthX), startX, startY++, colourFG, colourBG);
                }
            }

            public void Render(int startX, int startY, int lengthX, int index, ConsoleColor colourFG, ConsoleColor colourBG)
            {
                if(m_source == null || (index < 0 || index >= Count))
                {
                    return;
                }

                CConsoleDraw.WriteText(m_source[index].Name.PadRight(lengthX), startX, startY, colourFG, colourBG);
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

        internal class GameListDataSource : IListDataSource
        {
            private List<CGame.GameObject> m_source;
            private int   m_focusIndex;

            public GameListDataSource(List<CGame.GameObject> platformList)
            {
                if(platformList != null)
                {
                    m_focusIndex = -1;
                    m_source = platformList;
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
                    CConsoleDraw.WriteText(m_source[first + i].Title.PadRight(lengthX), startX, startY++, colourFG, colourBG);
                }
            }

            public void Render(int startX, int startY, int lengthX, int index, ConsoleColor colourFG, ConsoleColor colourBG)
            {
                if(m_source == null || (index < 0 || index >= Count))
                {
                    return;
                }

                CConsoleDraw.WriteText(m_source[index].Title.PadRight(lengthX), startX, startY, colourFG, colourBG);
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

            public void SetSource(List<CGame.GameObject> newSource)
            {
                m_focusIndex = -1;
                m_source = newSource;
            }
        }

        internal class CGameInfoDataSource : IInfoboxDataSource
        {
            private CGame.GameObject m_game;

            public CGameInfoDataSource(CGame.GameObject game)
            {
                m_game = game;
            }

            public void Render(int startX, int startY, int lengthX, ConsoleColor colourFG, ConsoleColor colourBG)
            {
                CConsoleDraw.WriteText(m_game.Title.PadRight(lengthX), startX, startY, colourFG, colourBG);
            }
        }
    }
}
