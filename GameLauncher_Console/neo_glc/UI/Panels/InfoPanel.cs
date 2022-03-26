using NStack;
using System;
using System.Collections.Generic;
using Terminal.Gui;
using core;

namespace glc
{
    public class CGameInfoPanel
    {
        private FrameView  m_frameView;
        private GameObject m_gameObject;

        public FrameView FrameView { get { return m_frameView; } }

        public CGameInfoPanel(string name, Pos x, Pos y, Dim width, Dim height)
        {
            // FrameView construction
            m_frameView = new FrameView(name)
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                CanFocus = false
            };
            m_frameView.Title = $"{m_frameView.Title}";
            m_gameObject = new GameObject();
        }

        public void SwitchGameInfo(GameObject gameObject)
        {
            m_gameObject = gameObject;
            m_frameView.RemoveAll();

            int y = 0;
            AddLabel($"Alias: {m_gameObject.Alias}"             , 0, y++, Dim.Percent(50), 1, TextAlignment.Right);
            AddLabel($"Frequency: {m_gameObject.Frequency}"     , 0, y++, Dim.Percent(50), 1, TextAlignment.Right);
            AddLabel($"Favourite: {m_gameObject.IsFavourite}"   , 0, y++, Dim.Percent(50), 1, TextAlignment.Right);
            AddLabel($"Platforms: {m_gameObject.PlatformFK}"    , 0, y++, Dim.Percent(50), 1, TextAlignment.Right);
            AddLabel($"Tags: {m_gameObject.Tag}"                , 0, y++, Dim.Percent(50), 1, TextAlignment.Right);
        }

        private void AddLabel(string title, int x, int y, Dim width, Dim height, TextAlignment alignment)
        {
            Label label = new Label(title)
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                TextAlignment = alignment
            };
            m_frameView.Add(label);
        }
    }
}
