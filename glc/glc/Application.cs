using core;

namespace glc
{
    public class CApplication : CApplicationCore
    {
        public CApplication()
            : base()
        {

        }

        protected override bool Initialise()
        {
            bool isOk = base.Initialise();

            CAppWindow.Initialise(m_platforms);

            return isOk;
        }

        public void Run()
        {
            CAppWindow.Run();
        }

        protected override void LoadConfig()
        {
            //throw new System.NotImplementedException();
        }
    }
}
