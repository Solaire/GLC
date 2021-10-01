using GLC_Structs;

namespace GLC
{
    /// <summary>
    /// Base class for providing mandatory members and functions to derived control en
    /// </summary>
    public abstract class CControl
    {
        protected ConsoleRect m_rect;
        protected ColourPair  m_colourPair;

        public abstract void Redraw();
        public abstract void OnEnter();
        public abstract void OnUpArrow();
        public abstract void OnDownArrow();
        public abstract void OnLeftArrow();
        public abstract void OnRightArrow();
    }
}
