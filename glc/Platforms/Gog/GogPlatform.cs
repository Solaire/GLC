using BasePlatformExtension;
using core.Platform;
using core;

namespace Gog
{
    /// <summary>
    /// Gog platform implementation
    /// </summary>
    public class CGogPlatform : CBasePlatformExtension<CGogScanner>
    {
        public CGogPlatform(int id, string name, string description, string path, bool isActive)
            : base(id, name, description, path, isActive)
        {
        }

        public override bool GameLaunch(Game game)
        {
            System.Console.WriteLine($"Gog game {game.Title} launched");
            return true;
        }
    }

    public class CGogFactory : CPlatformFactory<CPlatform>
    {
        public override CPlatform CreateDefault()
        {
            return new CGogPlatform(-1, GetPlatformName(), "", "", true);
        }

        public override CPlatform CreateFromDatabase(int id, string name, string description, string path, bool isActive)
        {
            return new CGogPlatform(id, name, description, path, isActive);
        }

        public override string GetPlatformName()
        {
            return "Gog";
        }
    }
}