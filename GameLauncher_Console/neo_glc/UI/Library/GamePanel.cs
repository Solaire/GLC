#define TABLE_TEST_

#if TABLE_TEST

using System;
using System.Collections.Generic;
using System.Data;
using core.Game;
using Terminal.Gui;

namespace glc.UI.Library
{
    public class CGamePanel : CFramePanel<GameObject, TableView>
    {
        public CGamePanel(List<GameObject> games, string name, Pos x, Pos y, Dim width, Dim height, bool canFocus)
            : base(name, x, y, width, height, canFocus)
        {
            m_contentList = games;
            Initialise(name, x, y, width, height, canFocus);
        }

        public override void CreateContainerView()
        {
            m_containerView = new TableView(new CGameDataTableSource(m_contentList))
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                CanFocus = true,
                FullRowSelect = true,
                MultiSelect = false
            };
            m_containerView.Style.AlwaysShowHeaders = true;
            m_containerView.Style.ShowVerticalCellLines = false;
            m_containerView.Style.ExpandLastColumn = false;
            m_frameView.Add(m_containerView);
        }

        public void UpdateTable()
        {
            m_containerView.Table = new CGameDataTableSource(m_contentList);
        }
    }

    internal class CGameDataTableSource : DataTable
    {
        public CGameDataTableSource(List<GameObject> itemList)
            : base()
        {
            Columns.Add(new DataColumn("Title", typeof(string)));
            Columns.Add(new DataColumn("Alias", typeof(string)));
            Columns.Add(new DataColumn("Fav",   typeof(bool)));
            Columns.Add(new DataColumn("Rating",typeof(int)));

            for(int i = 0; i < itemList.Count; ++i)
            {
                List<object> row = new List<object>()
                {
                    itemList[i].Title,
                    itemList[i].Alias,
                    itemList[i].IsFavourite,
                    itemList[i].ID, // TODO: replace
                };
                Rows.Add(row.ToArray());
            }
        }
    }
}

#else

using System;
using System.Collections;
using System.Collections.Generic;
using core.Game;
using glc.UI.Views;
using NStack;
using Terminal.Gui;

namespace glc.UI.Library
{
    public class CGamePanel : CFramePanel<GameObject, CCMultilistView>
    {
        public Dictionary<string, List<GameObject>> m_contentDictionary;
        public string singleSublist;

        //public CGamePanel(List<GameObject> games, string name, Pos x, Pos y, Dim width, Dim height, bool canFocus)
        public CGamePanel(Dictionary<string, List<GameObject>> games, string name, Pos x, Pos y, Dim width, Dim height, bool canFocus)
            : base(name, x, y, width, height, canFocus)
        {
            //m_contentList = games;
            m_contentDictionary = games;
            Initialise(name, x, y, width, height, canFocus);
        }

        public override void CreateContainerView()
        {
            m_containerView = new CCMultilistView(new CGameDataMultilistSource(m_contentDictionary))
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                CanFocus = true,
            };

            m_frameView.Add(m_containerView);
        }

        public void NewMultilistSource(Dictionary<string, List<GameObject>> multilist)
        {
            m_containerView.Source = new CGameDataMultilistSource(multilist);
        }

        public void SingleListMode(string sublistName)
        {
            m_containerView.SingleListMode(sublistName);
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

    internal class CGameDataMultilistSource : IIMultilistDataSource
    {
        public List<string> SublistHeaders { get; }
        public Dictionary<string, List<GameObject>> Source { get; }
        public List<int> HeadingIndexes { get; }

        public int TotalItemCount
        {
            get
            {
                int c = 0;
                foreach(KeyValuePair<string, List<GameObject>> kv in Source)
                {
                    c += kv.Value.Count;
                }
                return c;
            }
        }
        public int Length => GetMaxLengthItem();

        public bool SublistExists(string sublist)
        {
            return Source.ContainsKey(sublist);
        }

        public int SublistCount(string sublist)
        {
            if(!SublistExists(sublist))
            {
                return 0;
            }
            return Source[sublist].Count;
        }

        public int SublistCount(int sublistIndex)
        {
            if(sublistIndex < 0 || sublistIndex >= Source.Count)
            {
                return 0;
            }
            return Source[SublistHeaders[sublistIndex]].Count;
        }

        public GameObject ? GetItem(int sublistIndex, int itemIndex)
        {
            if(sublistIndex < 0 || sublistIndex >= Source.Count)
            {
                return null;
            }
            if(itemIndex < 0 || itemIndex >= Source[SublistHeaders[sublistIndex]].Count)
            {
                return null;
            }
            return Source[SublistHeaders[sublistIndex]][itemIndex];
        }

        public CGameDataMultilistSource(Dictionary<string, List<GameObject>> dataSource)
        {
            Source = dataSource;
            SublistHeaders = new List<string>(Source.Keys);

            HeadingIndexes = new List<int>() { 0 };
            for(int i = 0, j = 0; i < SublistHeaders.Count - 1; i++)
            {
                j += Source[SublistHeaders[i]].Count;
                HeadingIndexes.Add(j + 1);
            }
        }

        public virtual void Render(CCMultilistView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
        {
            container.Move(col, line);
            // Equivalent to an interpolated string like $"{Scenarios[item].Name, -widtestname}"; if such a thing were possible
            var s = ConstructString(item);
            RenderUstr(driver, $"{s}", col, line, width, start);
            //System.Diagnostics.Debug.WriteLine(s);
        }

        int GetMaxLengthItem()
        {
            int maxLength = 0;
            foreach(KeyValuePair<string, List<GameObject>> kv in Source)
            {
                for(int i = 0; i < kv.Value.Count; ++i)
                {
                    var s = ConstructString(i);
                    var sc = $"{s}  {GetString(i)}";
                    var l = sc.Length;
                    if(l > maxLength)
                    {
                        maxLength = l;
                    }
                }
            }

            /*
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
            */

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

        protected string ConstructString(int globalIndex)
        {
            //return String.Format(String.Format("{{0,{0}}}", 0), GetString(globalIndex));
            return String.Format(String.Format("  {{0,{0}}}", 0), GetString(globalIndex));
        }

        protected string GetString(int globalIndex)
        {
            int sublistIndex = 0;
            int itemIntex = 0;

            for(int i = 0; i < SublistHeaders.Count; ++i)
            {
                if(globalIndex >= Source[SublistHeaders[i]].Count)
                {
                    globalIndex -= Source[SublistHeaders[i]].Count;
                }
                else
                {
                    sublistIndex = i;
                    itemIntex = globalIndex;
                    break;
                }
            }

            return Source[SublistHeaders[sublistIndex]][itemIntex].Title;
        }
    }
}

#endif
