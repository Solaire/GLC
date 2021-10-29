using System;
using System.Collections.Generic;
using System.Threading;

namespace LibGLC
{
    public class CScannerGameReadEventArgs : EventArgs
    {
        public int Count { get; }
        public CGame.GameObject Game { get; }

        public CScannerGameReadEventArgs(int count, CGame.GameObject game)
        {
            Count = count;
            Game = game;
        }
    }

    public class CScannerPlatformEventArgs : EventArgs
    {
        public int Count { get; }
        public string Platform { get; }

        public CScannerPlatformEventArgs(int count, string platform)
        {
            Count = count;
            Platform = platform;
        }
    }

    public class CScanner
    {
        private bool m_hasStarted;
        private bool m_hasFinished;

        public event Action<CScannerGameReadEventArgs> NewGameRead;
        public event Action<CScannerPlatformEventArgs> PlatformStarted;
        public event Action<EventArgs> ScannerFinished;

        public bool   HasFinished { get { return m_hasFinished; } }

        public CScanner()
        {
            m_hasStarted  = false;
            m_hasFinished = false;
        }

        public void StartScanner(string target)
        {
            ScanForGames();
        }

        private void ScanForGames()
        {
            string[] test = new string[]
            {
                "Scanning, Steam",
                "Found - Dark souls 3",
                "Found - Divinity 2",
                "Found - Tekken 7",
                "Found - Civ V",
                "Found - MK 11",
                "Found - Overcooked 2",
                "Found - Quake",

                "Scanning, GOG",
                "Found - Gothic 2 NK",
                "Found - Heroes 3",
                "Found - Witcher 3",
                "Found - Baldur's gate",
                "Found - Project warlock",

                "Scanning, Uplay",
                "Found - Assassin's Creed",
                "Found - Far Cry 4",
                "Found - Ghost Recon - wildlands",
                "Found - Settlers",
                "Found - Heroes of might and magic",
            };

            int platformsFound = 0;
            int gamesFound = 0;

            for(int i = 0; i < test.Length; i++)
            {
                if(test[i].Contains("Scanning, "))
                {
                    OnPlatformStart(++platformsFound, test[i]);
                }
                else if(test[i].Contains("Found - "))
                {
                    OnNewGameFound(++gamesFound, new CGame.GameObject(0, "test", test[i], "test", "test", "test"));
                }
                Thread.Sleep(150);
            }

            OnScannerFinish();

            /*
            int platformCount   = 0;
            while(m_active)
            {
                foreach(string s in platforms)
                {
                    OnPlatformStart(platformCount++, s);
                    if(games.ContainsKey(s) && games[s] != null)
                    {
                        for(int i = 0; i < games[s].Count; i++)
                        {
                            OnNewGameFound(i + 1, games[s][i]);
                        }
                    }
                }
                OnScannerFinish();
            }
            */
        }

        private bool OnNewGameFound(int totalCount, CGame.GameObject game)
        {
            if(NewGameRead != null)
            {
                NewGameRead.Invoke(new CScannerGameReadEventArgs(totalCount, game));
                return true;
            }
            return false;
        }

        private bool OnPlatformStart(int totalCount, string platformName)
        {
            if(PlatformStarted != null)
            {
                PlatformStarted.Invoke(new CScannerPlatformEventArgs(totalCount, platformName));
                return true;
            }
            return false;
        }

        private bool OnScannerFinish()
        {
            if(ScannerFinished != null)
            {
                ScannerFinished.Invoke(new EventArgs());
                m_hasFinished = true;
                m_hasStarted  = false;
                return true;
            }
            return false;
        }
    }
}