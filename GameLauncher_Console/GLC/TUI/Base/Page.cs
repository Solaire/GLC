using System;
using GLC_Structs;

namespace GLC
{
    public class CPage : CControl
    {
        protected CWindow  m_parent;
        protected string   m_title;
        protected CPanel[] m_panels;
        protected int      m_activePanel;

        public CPage(CWindow parent, string title)
        {
            m_parent      = parent;
            m_title       = title;
            m_activePanel = 0;

            m_area.x = 0;
            m_area.y = 0;
            m_area.width = parent.m_rect.width;
            m_area.height = parent.m_rect.height - 2; // Microbuffer
        }

        public virtual void Initialise(PanelType[] panelTypes)
        {
            // No op. Needs to be implemented
            throw new NotImplementedException("Needs to be overridden");
        }

        /// <summary>
        /// Initialise the panel
        /// </summary>
        /*
        public virtual void Initialise(PanelType[] panelTypes)
        {
            m_panels = new CPanel[panelTypes.Length];
            for(int i = 0; i < panelTypes.Length; i++)
            {
                PanelData data = CConstants.PANEL_DATA[(int)panelTypes[i]];
                switch(panelTypes[i]) // TODO: Each panel has it's own class
                {
                    case PanelType.cPlatforms:
                    {
                        m_panels[i] = new CPlatformPanel(data.percentWidth, data.percentHeight, this);
                    }
                    break;

                    case PanelType.cGames:
                    {
                        m_panels[i] = new CGamesPanel(data.percentWidth, data.percentHeight, this);
                    }
                    break;

                    case PanelType.cGameInfo:
                    {
                        //m_panels[i] = new CGameInfoPanel(data.percentWidth, data.percentHeight);
                    }
                    break;

                    case PanelType.cKeyConfig:
                    {
                        //m_panels[i] = new CKeyConfigPanel(data.percentWidth, data.percentHeight);
                    }
                    break;

                    default:
                    {
                        throw new NotSupportedException("Unknown panel type");
                    }
                }
            }
            
            CalculatePanelLayout();
            Redraw(true);
        }
        */

        public virtual void Update()
        {
            foreach(CPanel panel in m_panels)
            {
                panel.Update();
            }
        }

        public void CalculatePanelLayout()
        {
            int nextLeft = 0;
            int nextTop  = 0;

            for(int i = 0; i < m_panels.Length; i++)
            {
                int nextWidth  = Math.Min((int)(((float)m_area.width  / 100f) * m_panels[i].GetPercentWidth()),  m_area.width  - nextLeft);
                int nextHeight = Math.Min((int)(((float)m_area.height / 100f) * m_panels[i].GetPercentHeight()), m_area.height - nextTop);

                m_panels[i].SetPosition(nextLeft, nextTop);
                m_panels[i].SetSize(nextWidth, nextHeight);

                // Now adjust the next position for the next panel

                nextTop  += nextHeight;
                nextLeft += nextWidth;

                if(nextLeft >= m_area.width && nextTop >= m_area.height)
                {
                    m_panels[i].SetRightBorder(false);
                    m_panels[i].SetBottomBorder(false);
                    break; // No more space
                }
                else if(nextLeft >= m_area.width) // 100% width - next panel below
                {
                    nextLeft = 0;
                    m_panels[i].SetRightBorder(false);
                }
                else if(nextTop >= m_area.height) // 100% height - next panel to the right
                {
                    nextTop = 0;
                    m_panels[i].SetBottomBorder(false);
                }
                else // Panel doesn't fit entire witdh and height - next panel to the right
                {
                    nextTop = 0;
                }
            }
        }

        public ConsoleColor GetColour(ColourThemeIndex i)
        {
            if(m_parent == null)
            {
                return (((int)i & 1) == 0) ? ConsoleColor.Black : ConsoleColor.White;
            }
            return m_parent.m_colours[i];
        }

        /// <summary>
        /// Redraw the page
        /// </summary>
        public override void Redraw(bool fullRedraw)
        {
            // TODO: draw title
            for(int i = 0; i < m_panels.Length; i++)
            {
                if(m_panels[i] != null && m_activePanel == i)
                {
                    m_panels[i].Redraw(fullRedraw);
                }
            }
        }

        public override void OnEnter()
        {
            m_panels[m_activePanel].OnEnter();
        }

        public override void OnUpArrow()
        {
            m_panels[m_activePanel].OnUpArrow();
        }

        public override void OnDownArrow()
        {
            m_panels[m_activePanel].OnDownArrow();
        }

        public override void OnLeftArrow()
        {
            m_panels[m_activePanel].OnLeftArrow();
        }

        public override void OnRightArrow()
        {
            m_panels[m_activePanel].OnRightArrow();
        }

        public override void OnTab()
        {
            throw new NotImplementedException("Override in child class");
        }
    }
}
