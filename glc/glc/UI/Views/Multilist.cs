using core.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using Terminal.Gui;

namespace glc.UI.Views
{
	public class AppendOnlyList<T> : IEnumerable<T>
    {
		private List<T> m_items = new List<T>();

		/// <summary>
		/// Add item to the list
		/// </summary>
		/// <param name="item">The item to be added</param>
		public void Append(T item)
		{
			m_items.Add(item);
		}

		///<inheritdoc/>
		public IEnumerator<T> GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

		///<inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
        {
            return m_items.GetEnumerator();
        }
    }

	/// <summary>
	/// Extension of <see cref="List{GameObject}"/> providing some additional properties
	/// </summary>
	public class CGameList : List<GameObject>
    {
		public CGameList(string listName, List<GameObject> games)
			: base(games)
        {
			ListName = listName;
			IsVisible = true; // TODO: Only show when have items
        }

		public CGameList(KeyValuePair<string, List<GameObject>> kv)
			: base(kv.Value)
        {
			ListName = kv.Key;
			IsVisible = true; // TODO: Only show when have items
		}

		/// <summary>
		/// Sublist name
		/// </summary>
		public string ListName
        {
			get;
			private set;
        }

		/// <summary>
		/// Flag controling list's visibility in the UI
		/// </summary>
		public bool IsVisible
        {
			get;
			set;
        }
    }

	/// <summary>
	/// Describes the data source interface used to populate <see cref="CMultilistView"/>.
	/// </summary>
	public interface IMultilistDataSource
    {
		/// <summary>
		/// List of sublist keys, preserving insert order
		/// </summary>
		List<string> SublistKeys { get; }

		/// <summary>
		/// Sublists keyed on the sublist name
		/// </summary>
        Dictionary<string, CGameList> Sublists { get; }

		/// <summary>
		/// Total count of all source items.
		/// </summary>
        int TotalCount { get; }

		/// <summary>
		/// Length of the largest item string.
		/// </summary>
		int Length { get; }

		/// <summary>
		/// Render the specified item.
		/// </summary>
		/// <param name="container">The multilist container</param>
		/// <param name="driver">The console driver</param>
		/// <param name="selected">Flag indicating if the rendered item is selected</param>
		/// <param name="item">The index of the item to render</param>
		/// <param name="col">The column number</param>
		/// <param name="line">the line number</param>
		/// <param name="width">The width of the render</param>
		/// <param name="start">The starting x position</param>
		void Render(CMultilistView container, ConsoleDriver driver, bool selected, string sublist, int itemIndex, int col, int line, int width, int start = 0);

		/// <summary>
		/// Check whether the specified sublist exists
		/// </summary>
		/// <param name="sublist">The sublist name</param>
		/// <returns>True of sublist exists in the data source</returns>
        bool SublistExists(string sublist);

		/// <summary>
		/// Return the count for the selected sublist
		/// </summary>
		/// <param name="sublist">The sublist name</param>
		/// <returns>Number of items in the sublist; if the sublist does not exist, should return -1</returns>
        int SublistCount(string sublist);

		/// <summary>
		/// Retrieve the item from the data source
		/// </summary>
		/// <param name="sublistIndex">The sublist index</param>
		/// <param name="itemIndex">The item index</param>
		/// <returns>Data source item; if the sublist doesn't exist, or the item index is out of bounds, should return null</returns>
		GameObject ? GetItem(string sublistName, int itemIndex);
	}

	/// <summary>
	/// The MultilistView is a hybrid of the ListView and TableView classes,
	/// rendering a scrollable list of data that is build from a string-list
	/// dictionary.
	///
	/// Remarks:
	/// MultilistView uses a string-list dictionary as the data source. This allows
	/// to render groups of items as well as rendering single lists in the `singleList`
	/// mode. This seems to be more efficient than loading new lists.
	/// </summary>
	public class CMultilistView : View
    {
		// Sublist and item indexes for...
        int		topSublist, topItem;			// ...topmost item in the UI
		int		selectedSublist, selectedItem;  // ...current selection
		int     lastSelectedItem;				// ...last selection (optimisation)
		bool	singleListMode;


		IMultilistDataSource source;

		/// <summary>
		/// The data source.
		/// </summary>
        public IMultilistDataSource Source
        {
            get => source;
            set
            {
                topSublist = 0;
                topItem = 0;
				selectedSublist = 0;
				selectedItem = 0;
				lastSelectedItem = -1;
				singleListMode = false;
                source = value;
                SetNeedsDisplay();
            }
        }

		/// <summary>
		/// Gets the widest item.
		/// </summary>
		public int Maxlength => (source?.Length) ?? 0;

		/// <summary>
		/// Initializes a new instance of <see cref="ListView"/> that will display the provided data source, using relative positioning.
		/// </summary>
		/// <param name="source"><see cref="IListDataSource"/> object that provides a mechanism to render the data.
		/// The number of elements on the collection should not change, if you must change, set
		/// the "Source" property to reset the internal settings of the ListView.</param>
		public CMultilistView(IMultilistDataSource source) : base()
		{
			this.source = source;
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of <see cref="ListView"/>. Set the <see cref="Source"/> property to display something.
		/// </summary>
		public CMultilistView() : base()
		{
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of <see cref="ListView"/> with the provided data source and an absolute position
		/// </summary>
		/// <param name="rect">Frame for the listview.</param>
		/// <param name="source">IListDataSource object that provides a mechanism to render the data. The number of elements on the collection should not change, if you must change, set the "Source" property to reset the internal settings of the ListView.</param>
		public CMultilistView(Rect rect, IMultilistDataSource source) : base(rect)
		{
			this.source = source;
			Initialize();
		}

		// TODO: unnecessary ?
		/// <summary>
		/// Initialise the multilist view variables
		/// </summary>
		void Initialize()
		{
			singleListMode = false;
			Source = source;
			CanFocus = true;
		}

		/// <summary>
		/// Calculate and return the global selection index, accounting
		/// for the sublist headers.
		/// </summary>
		/// <returns>The global selection index.</returns>
		int CalculateGlobalSelection()
        {
			return CalculateGlobalSelection(selectedSublist, selectedItem);
		}

		int CalculateGlobalSelection(int sublistIndex, int itemIndex)
        {
			if(sublistIndex >= source.SublistKeys.Count)
			{
				return 0;
			}

			int globalSelection = 0;
			for(int i = 0; i < sublistIndex; ++i)
			{
				globalSelection += source.SublistCount(source.SublistKeys[i]);
			}
			globalSelection += itemIndex + sublistIndex + 1; // Account for headings
			return globalSelection;
		}

		// TODO: Simplify and test with 3-5 sublists
		void NormaliseTopItems()
        {
			int globalSelection = CalculateGlobalSelection();
			int globalTop		= CalculateGlobalSelection(topSublist, topItem);
			System.Diagnostics.Debug.WriteLine($"Global selection {globalSelection}, top: {globalTop}");

			// Handle moving up
			if(globalSelection < globalTop)
            {
				globalTop -= (globalTop - globalSelection);
				globalTop -= (1 + topSublist);

				for(int i = 0; i < source.SublistKeys.Count && globalTop > 0; ++i)
				{
					if(globalTop >= Source.SublistCount(source.SublistKeys[i]))
                    {
						globalTop -= Source.SublistCount(source.SublistKeys[i]);
						continue;
					}
					topSublist = i;
					break;
				}
				topItem = globalTop;
				return;
			}

			// Handle moving down
			if(globalSelection + 1 >= globalTop + Frame.Height)
            {
				topItem += ((globalSelection + 1) - (globalTop + Frame.Height) + 1);
				while(topItem >= source.SublistCount(source.SublistKeys[topSublist]))
                {
					if(topSublist + 1 >= source.SublistKeys.Count)
                    {
						topItem = source.SublistCount(source.SublistKeys[topSublist]) - Frame.Height;
						return;
					}

					int delta = source.SublistCount(source.SublistKeys[topSublist]) - topItem;
					topSublist++;
					topItem = delta;
				}
			}
		}

		///<inheritdoc/>
		public override void Redraw(Rect bounds)
		{
			// TODO: Refactor

			var current = ColorScheme.Focus;
			Driver.SetAttribute(current);
			Move(0, 0);
			var f = Frame;
			bool focused = HasFocus;
			int col = 0;
			int row = 0;

			int sublist = topSublist;
			int item	= topItem;
			bool drawHeading = true;

			for(; row < f.Height && sublist < Source.SublistKeys.Count; row++)
            {
				bool isSelected = (!drawHeading && (sublist == selectedSublist && item == selectedItem));
				var newcolor = focused ? (isSelected ? ColorScheme.Focus : GetNormalColor ())
							   : (isSelected ? ColorScheme.HotNormal : GetNormalColor ());

				if(newcolor != current)
				{
					Driver.SetAttribute(newcolor);
					current = newcolor;
				}

				Move(0, row);
				if(source == null || item >= source.TotalCount)
				{
					for(int c = 0; c < f.Width; c++)
					{
						Driver.AddRune(' ');
					}
				}
				else if(drawHeading)
				{
					Driver.AddStr(Source.SublistKeys[sublist] + " ".PadRight(f.Width, '─'));
				}
				else
				{
					Source.Render(this, Driver, isSelected, Source.SublistKeys[sublist], item, col, row, f.Width - col);
					item++;

					bool nextSublist = (item >= source.SublistCount(source.SublistKeys[sublist]));
					if(singleListMode && nextSublist)
                    {
						break;
                    }
					else if(nextSublist)
                    {
						drawHeading = true;
						sublist++;
						item = 0;
						continue;
                    }
				}
				drawHeading = false;
			}
		}

		/// <summary>
		/// This event is raised when the selected item in the <see cref="ListView"/> has changed.
		/// </summary>
		public event Action<MultilistViewItemEventArgs> SelectedItemChanged;

		/// <summary>
		/// This event is raised when the user Double Clicks on an item or presses ENTER to open the selected item.
		/// </summary>
		public event Action<MultilistViewItemEventArgs> OpenSelectedItem;

		///<inheritdoc/>
		public override bool ProcessKey(KeyEvent kb)
		{
			if(source == null)
				return base.ProcessKey(kb);

			switch(kb.Key)
			{
				case Key.CursorUp:
				case Key.P | Key.CtrlMask:
					return MoveUp();

				case Key.CursorDown:
				case Key.N | Key.CtrlMask:
					return MoveDown();

				case Key.CursorRight:
				case Key.PageDown:
					return NextSublist();

				case Key.CursorLeft:
				case Key.PageUp:
					return PreviousSublist();

				/*
				case Key.Space:
					if(MarkUnmarkRow())
						return true;
					else
						break;
				*/

				case Key.Enter:
					return OnOpenSelectedItem();

				case Key.Home:
					return MoveHome();

				case Key.End:
					return MoveEnd();

				default:
					return false;
			}

			return true;
		}

		/// <summary>
		/// Move to the first element of the next sublist
		/// </summary>
		/// <returns>True</returns>
		public virtual bool NextSublist()
		{
			if(selectedSublist >= source.SublistKeys.Count - 1) // Can't move
			{
				return true;
			}
			else if(singleListMode)
			{
				selectedItem = source.Sublists[source.SublistKeys[selectedSublist]].Count - 1;
			}

			selectedSublist++;
			selectedItem = 0;

			NormaliseTopItems();
			OnSelectedChanged();
			SetNeedsDisplay();

			return true;
		}

		/// <summary>
		/// Move to the first element of the current sublist.
		/// If already there, move to the first element of the previous sublist
		/// </summary>
		/// <returns>True</returns>
		public virtual bool PreviousSublist()
		{
			if(selectedItem != 0 || singleListMode) // First item of current sublist
            {
				selectedItem = 0;
			}
			else if(selectedSublist - 1 >= 0) // First item of previous sublist
            {
				selectedSublist--;
			}
			else // Can't move
            {
				return true;
            }

			NormaliseTopItems();
			OnSelectedChanged();
			SetNeedsDisplay();

			return true;
		}

		/// <summary>
		/// Moves the selected item index to the previous row.
		/// </summary>
		/// <returns></returns>
		public virtual bool MoveUp()
		{
			// TODO: Refactor

			if(source.TotalCount == 0)
			{
				// Do we set lastSelectedItem to -1 here?
				return true; //Nothing for us to move to
			}

			if(selectedItem - 1 >= 0) // Move to the previous item on the current sublist
			{
				selectedItem--;
			}
			else if(!singleListMode && selectedSublist - 1 >= 0) // Move to the last element of the previous sublist
			{
				selectedSublist--;
				selectedItem = source.SublistCount(Source.SublistKeys[selectedSublist]) - 1;
			}
			else // Can't do anything
			{
				return true;
			}

			NormaliseTopItems();
			OnSelectedChanged();
			SetNeedsDisplay();
			return true;
		}

		/// <summary>
		/// Moves the selected item index to the next row.
		/// </summary>
		/// <returns></returns>
		public virtual bool MoveDown()
		{
			// TODO: Refactor

			if(source.TotalCount == 0)
			{
				// Do we set lastSelectedItem to -1 here?
				return true; //Nothing for us to move to
			}

			if(selectedItem + 1 < source.SublistCount(Source.SublistKeys[selectedSublist])) // Next item on the current sublist
			{
				selectedItem++;
			}
			else if(!singleListMode && selectedSublist + 1 < source.SublistKeys.Count) // Move to the first element on the next sublist
			{
				selectedSublist++;
				selectedItem = 0;
			}
			else // Can't do anything
			{
				return true;
			}

			NormaliseTopItems();
			OnSelectedChanged();
			SetNeedsDisplay();
			return true;
		}

		/// <summary>
		/// Moves the selected item index to the first row.
		/// </summary>
		/// <returns></returns>
		public virtual bool MoveHome()
		{
			selectedSublist = 0;
			selectedItem = 0;

			NormaliseTopItems();
			OnSelectedChanged();
			SetNeedsDisplay();

			return true;
		}

		/// <summary>
		/// Moves the selected item index to the last row.
		/// </summary>
		/// <returns></returns>
		public virtual bool MoveEnd()
		{
			selectedSublist = source.SublistKeys.Count - 1;
			selectedItem = source.SublistCount(Source.SublistKeys[selectedSublist]) - 1;

			NormaliseTopItems();
			OnSelectedChanged();
			SetNeedsDisplay();

			return true;
		}

		/// <summary>
		/// Scrolls the view down.
		/// </summary>
		/// <param name="lines">Number of lines to scroll down.</param>
		public virtual void ScrollDown(int lines)
		{
			//top = Math.Max(Math.Min(top + lines, source.TotalCount - 1), 0);
			SetNeedsDisplay();
		}

		/// <summary>
		/// Scrolls the view up.
		/// </summary>
		/// <param name="lines">Number of lines to scroll up.</param>
		public virtual void ScrollUp(int lines)
		{
			//top = Math.Max(top - lines, 0);
			SetNeedsDisplay();
		}

		/// <summary>
		/// Invokes the SelectedChanged event if it is defined.
		/// </summary>
		/// <returns></returns>
		public virtual bool OnSelectedChanged()
		{
			int globalSelection = CalculateGlobalSelection();
			if(globalSelection == lastSelectedItem)
            {
				return true;
            }

			if(source.TotalCount == 0)
            {
				return true;
            }

			GameObject value = (GameObject)source.GetItem(Source.SublistKeys[selectedSublist], selectedItem);
			SelectedItemChanged?.Invoke(new MultilistViewItemEventArgs(globalSelection, value));
			if(HasFocus)
            {
				lastSelectedItem = globalSelection;
            }
			return true;
		}

		/// <summary>
		/// Invokes the OnOpenSelectedItem event if it is defined.
		/// </summary>
		/// <returns></returns>
		public virtual bool OnOpenSelectedItem()
		{
			if(source.TotalCount == 0)
			{
				return true;
			}

			if(OpenSelectedItem == null)
            {
				return true;
            }

			GameObject value = (GameObject)source.GetItem(Source.SublistKeys[selectedSublist], selectedItem);

			OpenSelectedItem?.Invoke(new MultilistViewItemEventArgs(CalculateGlobalSelection(), value));

			return true;
		}

		///<inheritdoc/>
		public override bool OnEnter(View view)
		{
			Application.Driver.SetCursorVisibility(CursorVisibility.Invisible);

			if(lastSelectedItem == -1)
			{
				EnsuresVisibilitySelectedItem();
				OnSelectedChanged();
				return true;
			}

			return base.OnEnter(view);
		}

		///<inheritdoc/>
		public override bool OnLeave(View view)
		{
			if(lastSelectedItem > -1)
			{
				lastSelectedItem = -1;
				return true;
			}

			return false;
		}

		void EnsuresVisibilitySelectedItem()
		{
			/*
			SuperView?.LayoutSubviews();
			int globalSelection = CalculateGlobalSelection();
			if(globalSelection < top)
			{
				top = globalSelection;
			}
			else if(Frame.Height > 0 && globalSelection >= top + Frame.Height)
			{
				top = Math.Max(globalSelection - Frame.Height + 2, 0);
			}
			*/
		}

		///<inheritdoc/>
		public override void PositionCursor()
		{
			//Move(Bounds.Width - 1, CalculateGlobalSelection() - top);
		}

		///<inheritdoc/>
		public override bool MouseEvent(MouseEvent me)
		{
			return true; // TODO: mouse disabled

			if(!me.Flags.HasFlag(MouseFlags.Button1Clicked) && !me.Flags.HasFlag(MouseFlags.Button1DoubleClicked) &&
				me.Flags != MouseFlags.WheeledDown && me.Flags != MouseFlags.WheeledUp &&
				me.Flags != MouseFlags.WheeledRight && me.Flags != MouseFlags.WheeledLeft)
				return false;

			if(!HasFocus && CanFocus)
			{
				SetFocus();
			}

			if(source == null)
			{
				return false;
			}

			if(me.Flags == MouseFlags.WheeledDown)
			{
				ScrollDown(1);
				return true;
			}
			else if(me.Flags == MouseFlags.WheeledUp)
			{
				ScrollUp(1);
				return true;
			}

			/*
			if(me.Y + top >= source.TotalCount)
			{
				return true;
			}
			*/

			//globalSelection = top + me.Y;

			OnSelectedChanged();
			SetNeedsDisplay();
			if(me.Flags == MouseFlags.Button1DoubleClicked)
			{
				OnOpenSelectedItem();
			}

			return true;
		}

		public void SingleListMode(string sublistName)
        {
			int index = -1;
			for(int i = 0; i < Source.SublistKeys.Count; ++i)
            {
                if(Source.SublistKeys[i] == sublistName)
                {
					index = i;
					break;
                }
            }

			if(index >= 0)
            {
				singleListMode = true;
				selectedItem = 0;
				selectedSublist = index;
				topItem = 0;
				topSublist = index;
            }

			// TODO: fix
			/*
			if(Source.SublistExists(sublistName))
            {
				singleListMode = true;
				selectedItem = 0;
				selectedSublist = 0;
            }
			*/
        }
	}

	/// <summary>
	/// <see cref="EventArgs"/> for <see cref="ListView"/> events.
	/// </summary>
	public class MultilistViewItemEventArgs : EventArgs
	{
		/// <summary>
		/// The index of the <see cref="ListView"/> item.
		/// </summary>
		public int Item { get; }
		/// <summary>
		/// The <see cref="ListView"/> item.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="ListViewItemEventArgs"/>
		/// </summary>
		/// <param name="item">The index of the <see cref="ListView"/> item.</param>
		/// <param name="value">The <see cref="ListView"/> item</param>
		public MultilistViewItemEventArgs(int item, object value)
		{
			Item = item;
			Value = value;
		}
	}
}
