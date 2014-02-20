using System;
using System.ServiceProcess;

using NLog;

using RfidCenter.Basic;

namespace RfidBus.Service.Location
{
    public partial class LocationService : ServiceBase
    {
        public LocationService()
        {
            this.InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ConnectionManager.Instance.Initialize();
                DatabaseManager.Instance.Initialize();

                ConnectionManager.Instance.TranspondersFound += this.ConnectionManagerOnTranspondersFound;
                ConnectionManager.Instance.TranspondersLost += this.ConnectionManagerOnTranspondersLost;
            }
            catch (Exception ex)
            {
                BaseTools.LogException(ex, LogLevel.Error);

                this.Stop();
            }
        }

        private void ConnectionManagerOnTranspondersFound(object sender, TranspondersEventArgs args)
        {
            DatabaseManager.Instance.FoundTransponders(args.ReaderId, args.Transponders);
        }

        private void ConnectionManagerOnTranspondersLost(object sender, TranspondersEventArgs args)
        {
            DatabaseManager.Instance.LostTransponders(args.ReaderId, args.Transponders);
        }

        protected override void OnStop()
        {
            try
            {
                ConnectionManager.Instance.Stop();
            }
            catch (Exception ex)
            {
                BaseTools.LogException(ex, LogLevel.Fatal);
            }
        }

        public void Start()
        {
            this.OnStart(null);
        }
    }
}