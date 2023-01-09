using core_2.Game;
using Terminal.Gui;

namespace glc_2.UI.Panels
{
    internal class CGameInfoPanel
    {
        private FrameView m_frameView;

        public FrameView View => m_frameView;

        public CGameInfoPanel(Square square)
        {
            m_frameView = new FrameView()
            {
                Title = string.Empty,
                X = square.x,
                Y = square.y,
                Width = square.w,
                Height = square.h,
                CanFocus = false
            };
        }

        public void SetGameInfo(CGame game)
        {
            m_frameView.RemoveAll();
            m_frameView.Title = game.Name;

            int y = 0;
            AddLabel($"Alias:       {game.Alias}"      , new Square(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
            AddLabel($"Frequency:   {game.Frequency}"  , new Square(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
            AddLabel($"Favourite:   {game.IsFavourite}", new Square(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
            AddLabel($"Platforms:   {game.PlatformFK}" , new Square(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
            AddLabel($"Tags:        {game.Tag}"        , new Square(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
        }

        private void AddLabel(string title, Square square, TextAlignment align)
        {
            m_frameView.Add(new Label(title)
            {
                X = square.x,
                Y = square.y,
                Width = square.w,
                Height = square.h,
                TextAlignment = align
            });
        }
    }
}
