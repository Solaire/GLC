using System;
using System.Collections.Generic;
using ConsoleUI.Base;
using ConsoleUI.Type;
using ConsoleUI.Helper;
using LibGLC;

namespace GLC
{
    class CScannerDlg : CDialogBox
    {
        private CScanner     m_scanner;
        private List<string> m_buffer;
        private string       m_targetPlatform;
        private bool         m_saveToDB;

        private int          m_totalGameCount;
        private int          m_installedGameCount;
        CGame.GameSet        m_discoveredGames;

        public CScannerDlg(string title, ConsoleRect rect, bool saveToDB) : base(title, rect)
        {
            m_scanner        = new CScanner(Scanner_NewPlatform, Scanner_NewGame, Scanner_Finished);
            m_buffer         = new List<string>();
            m_targetPlatform = "";
            m_saveToDB       = saveToDB;
            m_totalGameCount = 0;
            m_installedGameCount = 0;
            m_discoveredGames = new CGame.GameSet();
        }

        public void SetTarget(string target)
        {
            m_targetPlatform = target;
        }

        public bool SaveToDB
        {
            set { m_saveToDB = value; }
        }

        public override int DoModal()
        {
            int ret = 0;
            Draw(true);
            m_scanner.ScanForGames(m_targetPlatform);

            int bufferMax = Math.Max(0, m_buffer.Count - (m_rect.height - 3));
            int bufferPos = bufferMax;
            bool exitDlg  = false;

            while(!exitDlg)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch(key.Key)
                {
                    case ConsoleKey.Enter:
                    {
                        exitDlg = true;
                    }
                    break;

                    case ConsoleKey.UpArrow:
                    {
                        bufferPos = Math.Max(0, bufferPos - 1);
                        DrawBuffer(bufferPos);
                    }
                    break;

                    case ConsoleKey.DownArrow:
                    {
                        bufferPos = Math.Min(bufferMax, bufferPos + 1);
                        DrawBuffer(bufferPos);
                    }
                    break;

                    default:
                        // No op
                        break;
                }
            }

            return ret;
        }

        public override void KeyPress(ConsoleKeyInfo keyInfo)
        {
            // Do nothing
        }

        private void Scanner_NewPlatform(CNewPlatformEventArgs e)
        {
            WriteLine(string.Format("Scanning {0}", e.Value));
        }

        private void Scanner_NewGame(CNewGameFoundEventArgs e)
        {
            WriteLine(string.Format("   - {0}", e.Value.m_strTitle));
            m_totalGameCount++;
            if(e.Value.m_bInstalled)
            {
                m_installedGameCount++;
            }
            m_discoveredGames.Add(new CGame.GameObject(CPlatform.GetPlatformFromString(e.Value.m_strPlatform), e.Value.m_strID, e.Value.m_strTitle, e.Value.m_strAlias, e.Value.m_strLaunch, e.Value.m_strUninstall, e.Value.m_strIcon));
        }

        private void Scanner_Finished(EventArgs e)
        {
            // Load existing games from database
            // Perfrom an intersection, adding new games and removing deleted games
            if(m_saveToDB)
            {
                //TODO: Non-instlled games
                CGame.MergeGameSets(m_discoveredGames, true);
            }
            WriteLine(string.Format("Finished - Games found: {0}. Installed: {1}. Not installed: {2}", m_totalGameCount, m_installedGameCount, m_totalGameCount - m_installedGameCount));
            CConsoleDraw.WriteText(" [Close] ".PadCenter(m_rect.width - 2), m_rect.x + 1, m_rect.Bottom - 1, ConsoleColor.Black, ConsoleColor.White);
        }

        private void WriteLine(string line)
        {
            m_buffer.Add(line);

            int maxDrawHeight = m_rect.height - 3; // 2 for border, 1 for button

            if(m_buffer.Count >= maxDrawHeight)
            {
                int startY = m_rect.y + 1;
                for(int i = m_buffer.Count - maxDrawHeight; i < m_buffer.Count; i++)
                {
                    CConsoleDraw.WriteText(m_buffer[i].PadRight(m_rect.width - 2), m_rect.x + 1, startY++, ConsoleColor.Black, ConsoleColor.White);
                }
            }
            else
            {
                int startY = m_rect.y + m_buffer.Count;
                CConsoleDraw.WriteText(line.PadRight(m_rect.width - 2), m_rect.x + 1, startY, ConsoleColor.Black, ConsoleColor.White);
            }
        }

        private void DrawBuffer(int bufferPos)
        {
            for(int i = bufferPos, startY = m_rect.y + 1; startY < m_rect.Bottom - 2 && i < m_buffer.Count; i++, startY++)
            {
                CConsoleDraw.WriteText(m_buffer[i].PadRight(m_rect.width - 2), m_rect.x + 1, startY, ConsoleColor.Black, ConsoleColor.White);
            }
        }
    }
}
