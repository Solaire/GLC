using core_2;
using System.Security.Cryptography.X509Certificates;

namespace glc_2
{
    internal class CApplication : CCore
    {
        public override bool Initialise()
        {
            if(!base.Initialise())
            {
                return false;
            }
            CWindow.Initialise();
            return true;
        }

        public void Run()
        {
            CWindow.Run();
        }
    }
}
