using NStack;
using System.Collections.Generic;
using System;
using Terminal.Gui;

namespace glc_2.UI.Panels
{
    /// <summary>
    /// Base class for all GUI panels (not related to <see cref="Terminal.Gui.PanelView"/>). A panel
    /// is a wrapper which encapsulates data, logic and an implementation of <see cref="Terminal.Gui.View"/>.
    /// logic.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U">Implementation of <see cref="Terminal.Gui.View"/></typeparam>
    internal abstract class BasePanel<T, U> where U : View
    {
        protected FrameView m_view;
        protected U m_containerView;
        protected int m_selectionIndex;

        internal FrameView View => m_view;
        internal U ContainerView => m_containerView;

        /// <summary>
        /// Create the main panel frame and call data container initialisation logic
        /// </summary>
        /// <param name="title">The panel title</param>
        /// <param name="box">The position and size of the panel frame</param>
        /// <param name="canFocus">Flag determining if the panel can be focused</param>
        protected void Initialise(string title, Box box, bool canFocus = true)
        {
            m_view = new FrameView(title)
            {
                X = box.X,
                Y = box.Y,
                Width = box.Width,
                Height = box.Height,
                CanFocus = canFocus
            };
            m_view.Title = $"{m_view.Title}";

            CreateContainerView();
            m_selectionIndex = 0;
        }

        /// <summary>
        /// Initialise data container view and add it to the panel frame.
        /// </summary>
        protected abstract void CreateContainerView();
    }

    /// <summary>
    /// Generic base class for <see cref="IListDataSource"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class ListDataSource<T> : IListDataSource
    {
        private readonly int length;

        #region IListDataSource

        public int Count => ItemList.Count;

        public int Length => length;

        public virtual bool IsMarked(int item)
        {
            return false;
        }

        public virtual void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
        {
            container.Move(col, line);
            // Equivalent to an interpolated string like $"{Scenarios[item].Name, -widtestname}"; if such a thing were possible
            var s = ConstructString(item);
            RenderUstr(driver, $"{s}", col, line, width, start);
        }

        public virtual void SetMark(int item, bool value)
        {
        }

        System.Collections.IList IListDataSource.ToList()
        {
            return ItemList;
        }

        #endregion IListDataSource

        internal List<T> ItemList { get; set; }

        /// <summary>
        /// Set the data source and calculate longest item
        /// </summary>
        /// <param name="itemList">the data list</param>
        internal ListDataSource(List<T> itemList)
        {
            ItemList = itemList;
            length = GetMaxLengthItem();
        }

        /// <summary>
        /// Calculate the longest string from the data source.
        /// </summary>
        /// <returns>Length of the longest string</returns>
        private int GetMaxLengthItem()
        {
            if(ItemList?.Count == 0)
            {
                return 0;
            }

            int maxLength = 0;
            for(int i = 0; i < ItemList.Count; i++)
            {
                var s = ConstructString(i);
                var sc = $"{s}  {GetString(i)}";
                var l = sc.Length;
                if(l > maxLength)
                {
                    maxLength = l;
                }
            }

            return maxLength;
        }

        // A slightly adapted method from: https://github.com/migueldeicaza/gui.cs/blob/fc1faba7452ccbdf49028ac49f0c9f0f42bbae91/Terminal.Gui/Views/ListView.cs#L433-L461
        private void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width, int start = 0)
        {
            int used = 0;
            int index = start;
            while(index < ustr.Length)
            {
                (var rune, var size) = Utf8.DecodeRune(ustr, index, index - ustr.Length);
                var count = Rune.ColumnWidth (rune);
                if(used + count >= width) break;
                driver.AddRune(rune);
                used += count;
                index += size;
            }

            while(used < width)
            {
                driver.AddRune(' ');
                used++;
            }
        }

        /// <summary>
        /// Construct a string from the specified item.
        /// </summary>
        /// <param name="itemIndex">The index of the selected item</param>
        /// <returns>A string representing the specified item</returns>
        protected abstract String ConstructString(int itemIndex);

        // TODO: might not be necessary
        /// <summary>
        /// Get the string of the specified item.
        /// </summary>
        /// <param name="itemIndex">The index of the selected item</param>
        /// <returns>String from the specified item</returns>
        protected abstract string GetString(int itemIndex);
    }
}
