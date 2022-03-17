﻿using NStack;
using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace glc
{
    public abstract class CFramePanel<T, U> where U : View
    {
		protected FrameView m_frameView;
        protected U         m_containerView;
        protected List<T>   m_contentList;
        protected int       m_listSelection;

		public FrameView FrameView		{ get { return m_frameView; } }
		public U         ContainerView	{ get { return m_containerView; } }
		public List<T>   ContentList
		{
			get { return m_contentList; }
			set { m_contentList = value; }
		}

		public CFramePanel(string name, Pos x, Pos y, Dim width, Dim height, bool canFocus, Key focusShortCut)
        {
			m_contentList = new List<T>();
			m_listSelection = 0;
		}

		protected void Initialise(string name, Pos x, Pos y, Dim width, Dim height, bool canFocus, Key focusShortCut)
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
			CreateContainerView();
		}

        public abstract void CreateContainerView();
    }

    public abstract class CGenericDataSource<T> : IListDataSource
    {
		private readonly int length;

		public List<T> ItemList { get; set; }

		public bool IsMarked(int item) => false;

		public int Count => ItemList.Count;

		public int Length => length;

		public CGenericDataSource(List<T> itemList)
		{
			ItemList = itemList;
			length = GetMaxLengthItem();
		}

		public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
		{
			container.Move(col, line);
			// Equivalent to an interpolated string like $"{Scenarios[item].Name, -widtestname}"; if such a thing were possible
			var s = ConstructString(item); //String.Format (String.Format ("{{0,{0}}}", 0), Games[item].Title);
			RenderUstr(driver, $"{s}", col, line, width, start);
		}
		public void SetMark(int item, bool value)
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
				var s = ConstructString(i); //String.Format (String.Format ("{{0,{0}}}", length), Games[i].Title);
				var sc = $"{s}  {GetString(i)}";//$"{s}  {Games[i].Title}";
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
}
