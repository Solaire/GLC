using System;
using ConsoleUI.Base;
using ConsoleUI.Type;
using SqlDB;
using System.Data.SQLite;
using Logger;
using LibGLC;

namespace GLC
{
    class CAppFrame : CFrame
    {
        private CMinibuffer m_minibuffer;
        private CScannerDlg m_scannerDlg;

        private bool m_minibufferFocused;

        public CAppFrame(string title, ConsoleRect rect, int pageCount) : base(title, rect, pageCount)
        {
            ConsoleRect minibufRect = new ConsoleRect(0, rect.Bottom - 2, rect.width, 2);
            m_minibuffer    = new CMinibuffer(minibufRect);
            m_scannerDlg    = new CScannerDlg("Scanner", new ConsoleRect((m_rect.width / 2) - 20, (m_rect.height / 2) - 20, 40, 20));
        }

        public override void Initialise()
        {
            base.Initialise();

            bool scannerStart = false;

            // Initialise database, create database if necessary
            if(CSqlDB.Instance.Open(true) != SQLiteErrorCode.Ok)
            {
                m_minibuffer.SetStatus("ERROR: Could not open or create the database");
                CLogger.LogInfo("ERROR: Could not open or create the database");
                return;
            }
            if(CPlatform.GetPlatformCount() == 0 || CGame.GetGameCount(0) == 0)
            {
                scannerStart = true;
            }    

            // Load all database stuff

            ConsoleRect pageRect = new ConsoleRect(m_rect.x, m_rect.y + 1, m_rect.width, m_rect.height - 3);

            m_pages[0] = new CLibraryPage("Library", pageRect, 3, this);
            m_pages[0].Initialise();
            Draw(true);

            m_minibuffer.Initialise();

            if(scannerStart)
            {
                m_scannerDlg.DoModal();
                Draw(true);
            }
        }

        public override void WindowMain()
        {
            while(true)
            {
                Console.CursorVisible = (m_minibufferFocused);
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch(keyInfo.Key)
                {
                    case ConsoleKey.Oem1: // Colon
                    {
                        m_minibufferFocused = true;
                        m_minibuffer.KeyPressed(keyInfo);
                    }
                    break;

                    case ConsoleKey.Escape: // 
                    {
                        if(m_minibufferFocused)
                        {
                            m_minibufferFocused = false;
                            m_minibuffer.ClearBuffer();
                        }
                    }
                    break;

                    default:
                    {
                        if(m_minibufferFocused)
                        {
                            m_minibuffer.KeyPressed(keyInfo);
                        }
                        else
                        {
                            m_pages[m_activePageIndex].KeyPress(keyInfo);
                            m_pages[m_activePageIndex].Draw(false);
                        }
                    }
                    break;
                }
            }
        }

        public override void OnCommand(object sender, GenericEventArgs<string> e)
        {
            throw new System.NotImplementedException();
        }

        public override void KeyPress(ConsoleKeyInfo keyInfo)
        {
            throw new NotImplementedException();
        }
    }
}
