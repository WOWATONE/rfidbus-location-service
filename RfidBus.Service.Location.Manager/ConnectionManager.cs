using System.Linq;
using System.Threading.Tasks;

using RfidBus.Primitives.Messages;
using RfidBus.Primitives.Messages.Readers;
using RfidBus.Primitives.Network;
using RfidBus.Serializers.Pb;

using RfidCenter.Basic;
using RfidCenter.Basic.Arguments;

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

        public void Connect()
        {
            this.CloseConnection();

            var pbCommunication = new PbCommunicationDescription();
            var config = new ParametersValues(pbCommunication.GetClientConfiguration());
            config.SetValue(ConfigConstants.PARAMETER_HOST, Properties.Settings.Default.BusHost);
            config.SetValue(ConfigConstants.PARAMETER_PORT, Properties.Settings.Default.BusPort);

            this._client = new RfidBusClient(pbCommunication, config);

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