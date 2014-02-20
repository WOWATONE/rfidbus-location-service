using DevExpress.Xpo;

namespace RfidBus.Service.Location.Manager.database
{

    public partial class TagsHistory
    {
        public TagsHistory() : base(Session.DefaultSession) { }
        public TagsHistory(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
