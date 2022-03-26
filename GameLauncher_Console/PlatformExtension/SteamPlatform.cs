using core;
using System.Collections.Generic;

namespace PlatformExtension
{
    public class CSteamPlatform : CPlatform
    {
        public CSteamPlatform(int id, string name, string description, string path, bool isActive)
            : base(id, name, description, path, isActive)
        {

        }

        public override bool GameLaunch(GameObject game)
        {
            throw new System.NotImplementedException();
        }

        public override HashSet<GameObject> GameScanner()
        {
            //throw new System.NotImplementedException();
            System.Console.WriteLine("This is the gog platform");
            return null;
        }
    }

    public class CSteamFactory : CPlatformFactory<CPlatform>
    {
        public CPlatform CreateDefault()
        {
            return new CSteamPlatform(-1, GetPlatformName(), "", "", true);
        }

        public CPlatform CreateFromDatabase(int id, string name, string description, string path, bool isActive)
        {
            return new CSteamPlatform(id, name, description, path, isActive);
        }

        public string GetPlatformName()
        {
            return "Gog";
        }
    }
}
