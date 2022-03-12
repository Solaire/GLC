using System.Collections.Generic;
using Terminal.Gui;

namespace glc
{
    public abstract class CFramePanel<T, U> where U : View
    {
        private FrameView m_frameView;
        private U         m_containerView;
        private List<T>   m_contentList;
        private int       m_listSelection;

        public CFramePanel(string name, Pos x, Pos y, Dim width, Dim height, bool canFocus, Key focusShortCut)
        {
            // FrameView construction
            m_frameView = new FrameView(name)
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                CanFocus = canFocus,
                Shortcut = focusShortCut
            };
            m_frameView.Title = $"{m_frameView.Title} ({m_frameView.ShortcutTag})";
            m_frameView.ShortcutAction = () => m_frameView.SetFocus();

            // Treeview construction
            m_containerView = CreateContainerView();

            SetEventTriggers();
            m_frameView.Add(m_containerView);

            m_contentList = new List<T>();
            m_listSelection = 0;
        }

        public abstract U CreateContainerView();

        public abstract void SetEventTriggers();
    }
}
