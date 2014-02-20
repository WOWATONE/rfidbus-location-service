using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading.Tasks;

using RfidBus.Primitives;
using RfidBus.Primitives.Messages;
using RfidBus.Primitives.Messages.Readers;
using RfidBus.Primitives.Network;

using RfidCenter.Basic;

namespace RfidBus.Service.Location.Manager
{
    public sealed class ConnectionManager
    {
        private static readonly ConnectionManager _instance;
        private RfidBusClient _client;

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

        [ImportMany(typeof(IProtocolSerializer), AllowRecomposition = true)]
        public IEnumerable<IProtocolSerializer> Serializers { get; set; }

        public void Connect()
        {
            if (this.Serializers == null)
            {
                var catalog = new DirectoryCatalog(Tools.CurrentDirectory);
                var container = new CompositionContainer(catalog);

                container.ComposeParts(this);
            }

            this.CloseConnection();

            var serGuid = Guid.Parse(Properties.Settings.Default.BusSerializator);

            var serializer = this.Serializers.FirstOrDefault(ser => ser.Id == serGuid);
            if (serializer == null)
                throw new ArgumentException("Can't find specified serializator.");

            this._client = new RfidBusClient(Properties.Settings.Default.BusHost,
                                             Properties.Settings.Default.BusPort,
                                             serializer);

            if (!this._client.Authorize(Properties.Settings.Default.BusLogin, Properties.Settings.Default.BusPassword))
                throw new BaseException(RfidErrorCode.InvalidLoginAndPassword);
        }

        private void CloseConnection()
        {
            lock (this)
            {
                if (this._client == null)
                    return;

                this._client.Close();
                this._client = null;
            }
        }

        public async Task<UiReaderRecord[]> GetBusReaders()
        {
            var response = await this._client.SendRequestAsync(new GetReaders());
            
            return (response == null) || (response.Status != ResponseStatus.Ok)
                       ? new UiReaderRecord[0]
                       : (from record in response.Readers
                          select new UiReaderRecord(record)).ToArray();
        }
    }
}