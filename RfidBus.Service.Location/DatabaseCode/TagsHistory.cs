using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
namespace RfidBus.Service.Location.database
{

    public partial class TagsHistory
    {
        public TagsHistory(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
