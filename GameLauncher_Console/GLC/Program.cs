using System;
using ConsoleUI.Type;

namespace GLC
{
    class Program
    {
        static void Main(string[] args)
        {


            // Main program start
            ConsoleRect rect = new ConsoleRect(0, 0, 100, 48);
            const int PAGE_COUNT = 1;

            CAppFrame window = new CAppFrame("GLC 2.0 test", rect, PAGE_COUNT);
            window.Initialise();
            window.WindowMain();
        }
    }
}
