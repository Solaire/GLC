using System;
using System.Collections.Generic;
using System.Linq;
using GLC_Structs;

namespace GLC
{
    /// <summary>
    /// Container for other interactive controls and data
    /// Designed to be special-purpose so this class must be inherited
    /// 
    /// Size is calculated from the percentage width/height members and the parent window size (CPage)
    /// Layout and position is determined by the order in the array
    /// 
    /// Instance of CPage -essentially the parent control- should be in charge of tracking the size and layout when initialising/drawing
    /// </summary>
    public abstract class CPanel : CControl
    {
        protected string    m_title;
        protected PanelType m_panelType;
        protected int       m_percentWidth;
        protected int       m_percentHeight;

        protected int    m_hoveredItemIndex;

        protected bool m_rightBorder;
        protected bool m_bottomBorder;

        protected CPage m_parentPage;

        protected bool m_isFocused;

        public delegate void OnDirtyEventHandler(object sender, GenericEventArgs<string> e);
        public event OnDirtyEventHandler OnDirty;

        public CPanel(string title, PanelType type, int percentWidth, int percentHeight, CPage parentPage)
        {
            m_title         = title;
            m_panelType     = type;
            m_percentWidth  = percentWidth;
            m_percentHeight = percentHeight;

            m_hoveredItemIndex = 0;


            m_rightBorder  = true;
            m_bottomBorder = true;

            m_parentPage = parentPage;

            m_isFocused = false;
        }

        public int GetPercentWidth()
        {
            return m_percentWidth;
        }

        public int GetPercentHeight()
        {
            return m_percentHeight;
        }

        public void SetPosition(int x, int y)
        {
            m_area.x = x;
            m_area.y = y;
        }

        public void SetSize(int width, int height)
        {
            m_area.width  = width;
            m_area.height = height;
        }

        public void SetRightBorder(bool enable)
        {
            m_rightBorder = enable;
        }

        public void SetBottomBorder(bool enable)
        {
            m_bottomBorder = enable;
        }

        protected void FireDirtyEvent(string eventData)
        {
            OnDirty.Invoke(this, new GenericEventArgs<string>(eventData));
        }

        protected abstract bool LoadContent();
        protected abstract void ReloadContent();
        protected abstract void DrawHighlighted(bool isFocused);
        public abstract void OnKeyInfo(ConsoleKeyInfo keyInfo);
        public abstract bool Update();
        public abstract void OnUpdateData(object sender, GenericEventArgs<string> e);
        public abstract void OnSetFocus(object sender, GenericEventArgs<int> e);
    }

    public class GenericEventArgs<T> : EventArgs
    {
        private readonly T m_eventData;

        public GenericEventArgs(T eventData)
        {
            m_eventData = eventData;
        }

        public T Data
        {
            get { return m_eventData; }
        }
    }
}
