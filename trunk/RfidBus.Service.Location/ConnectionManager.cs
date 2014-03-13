using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NLog;

using RfidBus.Primitives;
using RfidBus.Primitives.Messages;
using RfidBus.Primitives.Messages.Bus;
using RfidBus.Primitives.Messages.Readers;
using RfidBus.Primitives.Network;
using RfidBus.Serializers.Pb;

using RfidCenter.Basic;
using RfidCenter.Basic.Arguments;

namespace RfidBus.Service.Location
{
    internal sealed class ConnectionManager
    {
        #region single instance
        private static readonly ConnectionManager _instance;

        static ConnectionManager()
        {
            _instance = new ConnectionManager();
        }

        private ConnectionManager()
        {
        }

        public static ConnectionManager Instance
        {
            get { return _instance; }
        }
        #endregion

        private CancellationTokenSource _cancellationTokenSource;
        private RfidBusClient _client;

        public event EventHandler<TranspondersEventArgs> TranspondersFound;
        public event EventHandler<TranspondersEventArgs> TranspondersLost;

        public void Initialize()
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(Properties.Settings.Default.BusHost), "BusHost has be specified.");
            Contract.Requires<ArgumentException>(Properties.Settings.Default.BusPort.IsInRange(1, 65535), "BustPort has be between 1 and 65535.");
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(Properties.Settings.Default.BusLogin), "BusLogin has be specified.");
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(Properties.Settings.Default.BusPassword), "BusLogin has be specified.");

            this.EnqueueConnectAction();
        }

        private void OpenClient()
        {
            this.CloseClient();

            lock (this)
            {
                var pbCommunication = new PbCommunicationDescription();
                var config = new ParametersValues(pbCommunication.GetClientConfiguration());
                config.SetValue(ConfigConstants.PARAMETER_HOST, Properties.Settings.Default.BusHost);
                config.SetValue(ConfigConstants.PARAMETER_PORT, Properties.Settings.Default.BusPort);

                this._client = new RfidBusClient(pbCommunication, config);
                if (!this._client.Authorize(Properties.Settings.Default.BusLogin, Properties.Settings.Default.BusPassword))
                    throw new BaseException(RfidErrorCode.InvalidLoginAndPassword);

                this.SubscribeToReaders();

                this._client.ReceivedEvent += this.ClientOnReceivedEvent;
                this._client.Reconnected += this.ClientOnReconnected;
                this._client.Disconnected += this.ClientOnDisconnected;
            }
        }

        public void Stop()
        {
            if (this._cancellationTokenSource != null)
                this._cancellationTokenSource.Cancel();

            this.CloseClient();
        }

        private void CloseClient()
        {
            lock (this)
            {
                if (this._client == null)
                    return;

                this._client.ReceivedEvent -= this.ClientOnReceivedEvent;
                this._client.Reconnected -= this.ClientOnReconnected;
                this._client.Disconnected -= this.ClientOnDisconnected;

                this._client.Close();
                this._client = null;
            }
        }

        private void EnqueueConnectAction()
        {
            this._cancellationTokenSource = new CancellationTokenSource();

            Task.Run(delegate
                     {
                         Task.Delay(Properties.Settings.Default.BusRestoreConnectionInterval, this._cancellationTokenSource.Token);

                         if (this._cancellationTokenSource.IsCancellationRequested)
                             return;

                         try
                         {
                             this.OpenClient();

                             this._cancellationTokenSource = null;
                         }
                         catch (Exception ex)
                         {
                             BaseTools.Log.Warn("Caught exception while openning connection to RFID Bus.");
                             BaseTools.LogException(ex, LogLevel.Warn);

                             this.EnqueueConnectAction();
                         }
                     },
                     this._cancellationTokenSource.Token);
        }

        private void SubscribeToReaders()
        {
            Contract.Requires<ArgumentException>(Properties.Settings.Default.ListenReaders != null, "ListenReaders has be not Null.");

            var guids = Properties.Settings.Default.ListenReaders.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(Guid.Parse)
                                  .Distinct()
                                  .ToArray();

            if (guids.Length == 0)
            {
                // subscribe to all readers
                var readersResponse = this._client.SendRequest(new GetReaders());
                if (readersResponse.Status != ResponseStatus.Ok)
                    throw new Exception("Can't get readers.");

                foreach (var record in readersResponse.Readers)
                    this.SubscribeAndStartReader(record.Id);

                this._client.SendRequest(new SubscribeToBusEventReaderAdded());
            }
            else
            {
                // subscribe to certain readers
                foreach (var guid in guids)
                    this.SubscribeAndStartReader(guid);
            }
        }

        private void SubscribeAndStartReader(Guid readerId)
        {
            if ((this._client == null) || (!this._client.IsConnected))
                return;

            this._client.SendRequest(new SubscribeToReader(readerId));
            this._client.SendRequest(new StartReading(readerId));
        }

        private void ClientOnReceivedEvent(object sender, ReceivedEventEventArgs args)
        {
            if (args.EventMessage is TransponderFoundEvent)
            {
                var message = (TransponderFoundEvent) args.EventMessage;

                this.OnTranspondersFound(new TranspondersEventArgs(message.ReaderRecord.Id.ToString(), message.Transponders));
            }
            else if (args.EventMessage is TransponderLostEvent)
            {
                var message = (TransponderLostEvent) args.EventMessage;

                this.OnTranspondersLost(new TranspondersEventArgs(message.ReaderRecord.Id.ToString(), message.Transponders));
            }
            else if (args.EventMessage is BusEventReaderAdded)
            {
                var message = (BusEventReaderAdded) args.EventMessage;

                Task.Run(() => this.SubscribeAndStartReader(message.Reader.Id));
            }
        }

        private void ClientOnDisconnected(object sender, EventArgs e)
        {
            BaseTools.Log.Warn("Connection to RFID Bus was terminated. Trying to reinitialize connection...");

            this.EnqueueConnectAction();
        }

        private void ClientOnReconnected(object sender, EventArgs e)
        {
            BaseTools.Log.Warn("Connection to RFID Bus restored.");
        }

        private void OnTranspondersFound(TranspondersEventArgs e)
        {
            var handler = this.TranspondersFound;
            if (handler != null)
                handler(this, e);
        }

        private void OnTranspondersLost(TranspondersEventArgs e)
        {
            var handler = this.TranspondersLost;
            if (handler != null)
                handler(this, e);
        }
    }
}