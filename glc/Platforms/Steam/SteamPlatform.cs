using BasePlatformExtension;
using core.Platform;
using core;

namespace Steam
{
    /// <summary>
    /// Steam platform implementation
    /// </summary>
    public class CSteamPlatform : CBasePlatformExtension<CSteamScanner>
    {
        public CSteamPlatform(int id, string name, string description, string path, bool isActive)
            : base(id, name, description, path, isActive)
        {
        }

        public override bool GameLaunch(Game game)
        {
            System.Console.WriteLine($"Steam game {game.Title} launched");
            return true;
        }
    }

    public class CSteamFactory : CPlatformFactory<CPlatform>
    {
        public override CPlatform CreateDefault()
        {
            return new CSteamPlatform(-1, GetPlatformName(), "", "", true);
        }

        public override CPlatform CreateFromDatabase(int id, string name, string description, string path, bool isActive)
        {
            return new CSteamPlatform(id, name, description, path, isActive);
        }

        public override string GetPlatformName()
        {
            return "Steam";
        }
    }
}