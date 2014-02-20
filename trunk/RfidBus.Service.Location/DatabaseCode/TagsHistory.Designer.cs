//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace RfidBus.Service.Location.database
{

    [OptimisticLocking(false)]
    [DeferredDeletion(false)]
    public partial class TagsHistory : XPObject
    {
        string fTid;
        [Indexed(Name = @"TidIndex")]
        public string Tid
        {
            get { return fTid; }
            set { SetPropertyValue<string>("Tid", ref fTid, value); }
        }
        string fReader;
        [Indexed(@"Antenna", Name = @"ReaderAntennaIndex")]
        public string Reader
        {
            get { return fReader; }
            set { SetPropertyValue<string>("Reader", ref fReader, value); }
        }
        int? fAntenna;
        public int? Antenna
        {
            get { return fAntenna; }
            set { SetPropertyValue<int?>("Antenna", ref fAntenna, value); }
        }
        DateTime fEntryTime;
        public DateTime EntryTime
        {
            get { return fEntryTime; }
            set { SetPropertyValue<DateTime>("EntryTime", ref fEntryTime, value); }
        }
        DateTime? fLeaveTime;
        public DateTime? LeaveTime
        {
            get { return fLeaveTime; }
            set { SetPropertyValue<DateTime?>("LeaveTime", ref fLeaveTime, value); }
        }
    }

}
