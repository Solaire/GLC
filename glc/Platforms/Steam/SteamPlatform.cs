using BasePlatformExtension;
using core.Platform;
using core;
using Logger;
using System.Diagnostics;
using System.IO;

namespace Steam
{
    /// <summary>
    /// Steam [Valve]
    /// [installed games + owned games if account is public]
	/// [NOTE: DLCs are currently listed as owned not-installed games]
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

            if(OperatingSystem.IsWindows())
            {
                StartShellExecute(game.Launch);
                return true;
            }
            Process.Start(game.Launch);
            return true;
        }

        /// <summary>
        /// Scan the key name and extract the Steam game id
        /// </summary>
        /// <param name="key">The game string</param>
        /// <returns>Steam game ID as string</returns>
        private string GetGameID(string key)
        {
            if(key.StartsWith("appmanifest_"))
            {
                return string.Empty;// Path.GetFileNameWithoutExtension(key[11..]);
            }
            return key;
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