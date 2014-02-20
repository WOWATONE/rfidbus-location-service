using DevExpress.Xpo;

namespace RfidBus.Service.Location.Manager.database
{

    public partial class Object
    {
        public Object(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
