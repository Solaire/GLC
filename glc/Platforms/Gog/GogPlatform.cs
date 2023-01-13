using BasePlatformExtension;
using Logger;
using System.Diagnostics;
using core_2.Game;
using core_2.Platform;

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
            System.Console.WriteLine($"Gog game {game.Name} launched");
            return true;

            //if((bool)CConfig.GetConfigBool(CConfig.CFG_USEGAL))
            if(true)
            {
                //CLogger.LogInfo("Setting up a {0} game...", GOG.NAME_LONG);
                ProcessStartInfo gogProcess = new();
                string gogClientPath = game.Launch.Contains(".") ? game.Launch.Substring(0, game.Launch.IndexOf('.') + 4) : game.Launch;
                string gogArguments = game.Launch.Contains(".") ? game.Launch[(game.Launch.IndexOf('.') + 4)..] : string.Empty;
                CLogger.LogInfo($"Launch: \"{gogClientPath}\" {gogArguments}");
                gogProcess.FileName = gogClientPath;
                gogProcess.Arguments = gogArguments;
                Process.Start(gogProcess);
                if(OperatingSystem.IsWindows())
                {
                    Thread.Sleep(4000);
                    Process[] procs = Process.GetProcessesByName("GalaxyClient");
                    foreach(Process proc in procs)
                    {
                        //CDock.WindowMessage.ShowWindowAsync(procs[0].MainWindowHandle, CDock.WindowMessage.SW_FORCEMINIMIZE);
                    }
                }
                return true;
            }

            CLogger.LogInfo($"Launch: {game.Icon}");
            if(OperatingSystem.IsWindows())
            {
                StartShellExecute(game.Icon);
                return true;
            }
            Process.Start(game.Icon);
            return true;
        }
    }

    public class CGogFactory : CPlatformFactory<BasePlatform>
    {
        public override BasePlatform CreateDefault()
        {
            return new CGogPlatform(-1, GetPlatformName(), "", "", true);
        }

        public override BasePlatform CreateFromDatabase(int id, string name, string description, string path, bool isActive)
        {
            return new CGogPlatform(id, name, description, path, isActive);
        }

        public override string GetPlatformName()
        {
            return "Gog";
        }
    }
}