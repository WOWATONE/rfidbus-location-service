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
namespace RfidBus.Service.Location.Manager.database
{

    [OptimisticLocking(false)]
    [DeferredDeletion(false)]
    public partial class Object : XPObject
    {
        string fName;
        public string Name
        {
            get { return fName; }
            set { SetPropertyValue<string>("Name", ref fName, value); }
        }
        string fDescription;
        public string Description
        {
            get { return fDescription; }
            set { SetPropertyValue<string>("Description", ref fDescription, value); }
        }
        string fTid;
        public string Tid
        {
            get { return fTid; }
            set { SetPropertyValue<string>("Tid", ref fTid, value); }
        }
    }

}
