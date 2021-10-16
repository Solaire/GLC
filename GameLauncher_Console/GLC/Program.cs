using ConsoleUI.Structs;

namespace GLC
{
    class Program
    {
        static void Main(string[] args)
        {


            // Main program start
            ConsoleRect rect = new ConsoleRect(0, 0, 100, 48);
            const int PAGE_COUNT = 1;

            CAppWindow window = new CAppWindow("GLC 2.0 test", rect, CConstants.DEFAULT_THEME, PAGE_COUNT);
            window.Initialise();
            window.WindowMain();
        }
    }
}
