using DevExpress.Xpf.Docking;

namespace RfidBus.Service.Location.Manager
{
    internal class LayoutViewPanel : LayoutPanel
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the LayoutPanel class with default settings.
        ///     </para>
        /// </summary>
        public LayoutViewPanel(IViewPage page)
        {
            this.Page = page;

            this.AllowClose = false;
            this.AllowMaximize = true;
            this.AllowMinimize = true;
            this.AllowHide = true;

            this.Caption = page.Caption;
            this.Content = page;
        }

        public IViewPage Page { get; private set; }
    }
}