using Terminal.Gui;
using core;
using System;
using System.Collections.Generic;

namespace glc
{
    public class CGamePanel : CFramePanel<GameObject, ListView>
    {
        public CGamePanel(List<GameObject> games, string name, Pos x, Pos y, Dim width, Dim height, bool canFocus)
            : base(name, x, y, width, height, canFocus)
        {
            m_contentList = games;
            Initialise(name, x, y, width, height, canFocus);
        }

        public override void CreateContainerView()
        {
            m_containerView = new ListView(new CGameDataSource(m_contentList))
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = true,
            };

            m_frameView.Add(m_containerView);
        }
    }

    internal class CGameDataSource : CGenericDataSource<GameObject>
    {
        public CGameDataSource(List<GameObject> itemList)
            : base(itemList)
        {

        }

        protected override string ConstructString(int itemIndex)
        {
            return String.Format(String.Format("{{0,{0}}}", 0), ItemList[itemIndex].Title);
        }

        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].Title;
        }
    }
}
