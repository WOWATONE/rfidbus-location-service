using DevExpress.Xpo;

namespace RfidBus.Service.Location.Manager.database
{

    public partial class LocationParameters
    {
        public LocationParameters(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
