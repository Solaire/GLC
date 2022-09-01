using BasePlatformExtension;
using core.Game;
using core.Platform;

namespace ReferencePlatform
{
    /// <summary>
    /// Reference platform for testing purposes
    /// </summary>
    public class CReferencePlatform : CBasePlatformExtension<CReferenceScanner>
    {
        public CReferencePlatform(int id, string name, string description, string path, bool isActive)
            : base(id, name, description, path, isActive)
        {
        }

        public override bool GameLaunch(GameObject game)
        {
            System.Console.WriteLine($"Reference game {game.Title} launched");
            return true;
        }
    }

    public class CReferenceFactory : CPlatformFactory<CPlatform>
    {
        public override CPlatform CreateDefault()
        {
            return new CReferencePlatform(-1, GetPlatformName(), "", "", true);
        }

        public override CPlatform CreateFromDatabase(int id, string name, string description, string path, bool isActive)
        {
            return new CReferencePlatform(id, name, description, path, isActive);
        }

        public override string GetPlatformName()
        {
            return "Reference";
        }
    }
}