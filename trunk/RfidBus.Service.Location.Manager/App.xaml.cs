using System.Globalization;
using System.Threading;
using System.Windows;

using DevExpress.Xpf.Core;

namespace RfidBus.Service.Location.Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ThemeManager.ApplicationThemeName = "Office2013";

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}
