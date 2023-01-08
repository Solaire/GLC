using NStack;
using System.Collections.Generic;
using System;
using Terminal.Gui;

namespace glc_2.UI.Panels
{
    internal abstract class CBasePanel<T, U> where U : View
    {
        protected FrameView m_view;
        protected U m_containerView;
        protected int m_selectionIndex;

        public FrameView    View => m_view;
        public U ContainerView => m_containerView;

        public CBasePanel()
        {
            m_selectionIndex = 0;
        }

        protected void Initialise(string name, Square square, bool canFocus)
        {
            m_view = new FrameView(name)
            {
                X = square.x,
                Y = square.y,
                Width = square.w,
                Height = square.h,
                CanFocus = canFocus
            };
            m_view.Title = $"{m_view.Title}";

            CreateContainerView();
        }

        protected abstract void CreateContainerView();
    }

    public abstract class CGenericDataSource<T> : IListDataSource
    {
        private readonly int length;

        public List<T> ItemList { get; set; }

        public virtual bool IsMarked(int item) => false;

        public int Count => ItemList.Count;

        public int Length => length;

        public CGenericDataSource(List<T> itemList)
        {
            ItemList = itemList;
            length = GetMaxLengthItem();
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

        int GetMaxLengthItem()
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

        System.Collections.IList IListDataSource.ToList()
        {
            return ItemList;
        }

        protected abstract String ConstructString(int itemIndex);

        protected abstract string GetString(int itemIndex);
    }

    internal struct Square
    {
        public Pos x, y;
        public Dim w, h;

        public Square(Pos x, Pos y, Dim w, Dim h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }
    }
}
