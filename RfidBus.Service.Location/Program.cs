using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace RfidBus.Service.Location
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            if (Tools.ArgsContainsParameter("--install"))
            {
                var exeFileName = Path.GetFullPath(Assembly.GetEntryAssembly().Location);
                var processStartInfo = new ProcessStartInfo(exeFileName) {Verb = "runas", Arguments = "--register_service", UseShellExecute = true,};
                Process.Start(processStartInfo);
            }
            else if (Tools.ArgsContainsParameter("--uninstall"))
            {
                var exeFileName = Path.GetFullPath(Assembly.GetEntryAssembly().Location);
                var processStartInfo = new ProcessStartInfo(exeFileName) {Verb = "runas", Arguments = "--unregister_service", UseShellExecute = true,};
                Process.Start(processStartInfo);
            }
            else if (Tools.ArgsContainsParameter("--register_service"))
                Tools.RegisterService();
            else if (Tools.ArgsContainsParameter("--unregister_service"))
                Tools.UnregisterService();
            else
            {
#if DEBUG
                if (Environment.UserInteractive)
                {
                    var service = new LocationService();
                    service.Start();

                    Thread.Sleep(Timeout.Infinite);

                    return;
                }
#endif
                var servicesToRun = new ServiceBase[]
                                    {
                                        new LocationService()
                                    };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}