using core_2.Game;
using Terminal.Gui;

namespace glc_2.UI.Panels
{
    internal class CGameInfoPanel
    {
        private FrameView m_frameView;

        public FrameView View => m_frameView;

        public CGameInfoPanel(Box box)
        {
            m_frameView = new FrameView()
            {
                Title = string.Empty,
                X = box.X,
                Y = box.Y,
                Width = box.Width,
                Height = box.Height,
                CanFocus = false
            };
        }

        public void SetGameInfo(Game game)
        {
            m_frameView.RemoveAll();
            m_frameView.Title = game.Name;

            int y = 0;
            AddLabel($"Alias:       {game.Alias}"      , new Box(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
            AddLabel($"Frequency:   {game.Frequency}"  , new Box(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
            AddLabel($"Favourite:   {game.IsFavourite}", new Box(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
            AddLabel($"Platforms:   {game.PlatformFK}" , new Box(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
            AddLabel($"Tags:        {game.Tag}"        , new Box(0, y++, Dim.Percent(50), 1), TextAlignment.Left);
        }

        private void AddLabel(string title, Box box, TextAlignment align)
        {
            m_frameView.Add(new Label(title)
            {
                X = box.X,
                Y = box.Y,
                Width = box.Width,
                Height = box.Height,
                TextAlignment = align
            });
        }
    }
}
