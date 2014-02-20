using DevExpress.Xpf.Bars;

namespace RfidBus.Service.Location.Manager
{
    internal interface IViewPage
    {
        BarItem[] BarItems { get; }
        string Caption { get; }

        void OnLoaded();
    }
}