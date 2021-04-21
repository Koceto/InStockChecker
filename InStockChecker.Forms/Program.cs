using InStockChecker.Services;
using System;

namespace InStockChecker.Forms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            MainService mainService = new MainService();

#if DEBUG
            mainService.Run(10000);
#else
            mainService.Run();
#endif
        }
    }
}