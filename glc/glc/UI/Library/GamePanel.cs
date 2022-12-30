using System;
using System.Collections.Generic;
using System.Linq;
using core;
using glc.UI.Views;
using NStack;
using Terminal.Gui;

namespace glc.UI.Library
{
    public class CGamePanel : CFramePanel<Game, CMultilistView>
    {
        public Dictionary<string, CGameList> m_contentDictionary;
        public string singleSublist;

        public CGamePanel(Dictionary<string, CGameList> games, string name, Pos x, Pos y, Dim width, Dim height, bool canFocus)
            : base(name, x, y, width, height, canFocus)
        {
            m_contentDictionary = games;
            Initialise(name, x, y, width, height, canFocus);
        }

        public override void CreateContainerView()
        {
            m_containerView = new CMultilistView(new CGameDataMultilistSource(m_contentDictionary))
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                CanFocus = true,
            };

            m_frameView.Add(m_containerView);
        }

        public void NewMultilistSource(Dictionary<string, CGameList> multilist)
        {
            m_containerView.Source = new CGameDataMultilistSource(multilist);
        }

        public void SingleListMode(string sublistName)
        {
            m_containerView.SingleListMode(sublistName);
        }
    }

    internal class CGameDataSource : CGenericDataSource<Game>
    {
        public CGameDataSource(List<Game> itemList)
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

    internal class CGameDataMultilistSource : IMultilistDataSource
    {
        public AppendOnlyList<string> SublistKeys { get; }
        public Dictionary<string, CGameList> Sublists { get; }
        public List<int> HeadingIndexes { get; }

        public int TotalCount
        {
            get
            {
                int c = 0;
                foreach(KeyValuePair<string, CGameList> kv in Sublists)
                {
                    c += kv.Value.Count;
                }
                return c;
            }
        }
        public int Length => GetMaxLengthItem();

        public bool SublistExists(string sublist)
        {
            return Sublists.ContainsKey(sublist);
        }

        public int SublistCount(string sublist)
        {
            if(!SublistExists(sublist))
            {
                return 0;
            }
            return Sublists[sublist].Count;
        }

        public int SublistCount(int sublistIndex)
        {
            return (sublistIndex >= 0 && sublistIndex < SublistKeys.Count) ? SublistCount(SublistKeys[sublistIndex]) : 0;
        }

        public Game? GetItem(string sublist, int itemIndex)
        {
            if(!Sublists.ContainsKey(sublist))
            {
                return null;
            }
            if(itemIndex < 0 || itemIndex >= SublistCount(sublist))
            {
                return null;
            }
            return Sublists[sublist][itemIndex];
        }

        public CGameDataMultilistSource(Dictionary<string, CGameList> dataSource)
        {
            Sublists = dataSource;
            SublistKeys = new AppendOnlyList<string>(Sublists.Keys.ToList());

            HeadingIndexes = new List<int>() { 0 };
            for(int i = 0, j = 0; i < SublistKeys.Count - 1; i++)
            {
                j += Sublists[SublistKeys[i]].Count;
                HeadingIndexes.Add(j + 1);
            }
        }

        public virtual void Render(CMultilistView container, ConsoleDriver driver, bool selected, string sublist, int itemIndex, int col, int line, int width, int start = 0)
        {
            container.Move(col, line);
            // Equivalent to an interpolated string like $"{Scenarios[item].Name, -widtestname}"; if such a thing were possible
            var s = ConstructString(sublist, itemIndex);
            RenderUstr(driver, $"{s}", col, line, width, start);
            //System.Diagnostics.Debug.WriteLine(s);
        }

        int GetMaxLengthItem()
        {
            int maxLength = 0;
            foreach(KeyValuePair<string, CGameList> kv in Sublists)
            {
                for(int i = 0; i < kv.Value.Count; ++i)
                {
                    var s = ConstructString(kv.Key, i);
                    var sc = $"{s}  {ConstructString(kv.Key, i)}";
                    var l = sc.Length;
                    if(l > maxLength)
                    {
                        maxLength = l;
                    }
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

        protected string ConstructString(string sublist, int itemIndex)
        {
            if(!Sublists.ContainsKey(sublist) || itemIndex < 0 || itemIndex >= Sublists[sublist].Count)
            {
                return "";
            }
            return String.Format(String.Format("  {{0,{0}}}", 0), Sublists[sublist][itemIndex].Title);
        }
    }
}