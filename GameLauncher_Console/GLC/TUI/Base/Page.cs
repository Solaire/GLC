using System;
using GLC_Structs;

namespace GLC
{
    public class CPage : CControl
    {
        private CWindow  m_parent;
        private string   m_title;
        private CPanel[] m_panels;
        private int      m_activePanel;

        public CPage(CWindow parent, string title)
        {
            m_parent      = parent;
            m_title       = title;
            m_activePanel = 0;

            m_rect.x = 0;
            m_rect.y = 0;
            m_rect.width = parent.m_rect.width;
            m_rect.height = parent.m_rect.height - 2; // Microbuffer

            m_colourPair = parent.m_defaultColours;
        }

        /// <summary>
        /// Initialise the panel
        /// </summary>
        public void Initialise(PanelType[] panelTypes)
        {
            m_panels = new CPanel[panelTypes.Length];
            for(int i = 0; i < panelTypes.Length; i++)
            {
                PanelData data = CConstants.PANEL_DATA[(int)panelTypes[i]];
                switch(panelTypes[i]) // TODO: Each panel has it's own class
                {
                    case PanelType.cPanel_Platforms:
                    {
                        m_panels[i] = new CPlatformPanel(data.percentWidth, data.percentHeight);
                    }
                    break;

                    case PanelType.cPanel_Games:
                    {
                        m_panels[i] = new CGamesPanel(data.percentWidth, data.percentHeight);
                    }
                    break;

                    case PanelType.cPanel_GameInfo:
                    {
                        //m_panels[i] = new CGameInfoPanel(data.percentWidth, data.percentHeight);
                    }
                    break;

                    case PanelType.cPanel_KeyConfig:
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
            Redraw();
        }

        public void CalculatePanelLayout()
        {
            int nextLeft = 0;
            int nextTop  = 0;

            for(int i = 0; i < m_panels.Length; i++)
            {
                int nextWidth  = Math.Min((int)(((float)m_rect.width  / 100f) * m_panels[i].GetPercentWidth()),  m_rect.width  - nextLeft);
                int nextHeight = Math.Min((int)(((float)m_rect.height / 100f) * m_panels[i].GetPercentHeight()), m_rect.height - nextTop);

                m_panels[i].SetPosition(nextLeft, nextTop);
                m_panels[i].SetSize(nextWidth, nextHeight);

                // Now adjust the next position for the next panel

                nextTop  += nextHeight;
                nextLeft += nextWidth;

                if(nextLeft >= m_rect.width && nextTop >= m_rect.height)
                {
                    m_panels[i].SetRightBorder(false);
                    m_panels[i].SetBottomBorder(false);
                    break; // No more space
                }
                else if(nextLeft >= m_rect.width) // 100% width - next panel below
                {
                    nextLeft = 0;
                    m_panels[i].SetRightBorder(false);
                }
                else if(nextTop >= m_rect.height) // 100% height - next panel to the right
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

        /// <summary>
        /// Redraw the page
        /// </summary>
        public override void Redraw()
        {
            // TODO: draw title
            foreach(CPanel p in m_panels)
            {
                p.Redraw();
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
    }
}
