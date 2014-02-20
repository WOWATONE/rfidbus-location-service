using DevExpress.Xpo;

namespace RfidBus.Service.Location.Manager.database
{

    public partial class Location
    {
        public Location(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
