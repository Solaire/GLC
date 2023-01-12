using Terminal.Gui;

namespace glc_2.UI
{
    /// <summary>
    /// Structure representing a box, used to determine the position and size
    /// of dynamic GUI elements (ones that rely on proportions or percentages)
    /// </summary>
    internal struct Box
    {
        #region Properties

        /// <summary>
        /// The X coordinate
        /// </summary>
        internal Pos X
        {
            get;
        }

        /// <summary>
        /// The Y coordinate
        /// </summary>
        internal Pos Y
        {
            get;
        }

        /// <summary>
        /// The width of the box
        /// </summary>
        internal Dim Width
        {
            get;
        }

        /// <summary>
        /// The height of the box
        /// </summary>
        internal Dim Height
        {
            get;
        }

        #endregion Properties

        /// <summary>
        /// Construct the box
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <param name="w">The width</param>
        /// <param name="h">the height</param>
        internal Box(Pos x, Pos y, Dim w, Dim h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }
    }
}
