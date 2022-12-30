using core;

namespace glc
{
    public class CApplication : CApplicationCore
    {
        public CApplication()
            : base()
        {

        }

        public override bool Initialise()
        {
            if(!base.Initialise())
            {
                return false;
            }

            CAppWindow.Initialise(m_platforms);
            return true;
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
