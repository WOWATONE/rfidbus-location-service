using System;
using System.Configuration.Install;
using System.IO;
using System.Reflection;

namespace RfidBus.Service.Location
{
    internal static class Tools
    {
        static Tools()
        {
            CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public static string CurrentDirectory { get; private set; }

        public static bool ArgsContainsParameter(string parameter)
        {
            return Array.Exists(Environment.GetCommandLineArgs(), arg => string.Equals(arg, parameter, StringComparison.InvariantCultureIgnoreCase));
        }

        public static void RegisterService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] {Assembly.GetEntryAssembly().Location});
            }
            catch (Exception)
            {
            }
        }

        public static void UnregisterService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] {"/u", Assembly.GetEntryAssembly().Location});
            }
            catch (Exception)
            {
            }
        }
    }
}