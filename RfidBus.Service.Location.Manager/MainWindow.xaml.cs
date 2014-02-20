using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

using DevExpress.Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking.Base;
using DevExpress.Xpf.WindowsUI;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;

namespace RfidBus.Service.Location.Manager
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DXWindow
    {
        private readonly List<LayoutViewPanel> _panels = new List<LayoutViewPanel>();

        public MainWindow()
        {
            this.InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this._panels.Add(new LayoutViewPanel(new ViewHistory()));
                this._panels.Add(new LayoutViewPanel(new ViewLocations()));
                this._panels.Add(new LayoutViewPanel(new ViewObjects()));

                foreach (var item in this._panels)
                    this.MainGroup.Add(item);

                this.Loaded += this.OnLoaded;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.DelayedExecute(delegate
                                {
                                    try
                                    {
                                        ConnectionManager.Instance.Connect();

                                        XpoDefault.DataLayer = XpoDefault.GetDataLayer(Properties.Settings.Default.ConnectionString, AutoCreateOption.DatabaseAndSchema);
                                        XpoDefault.Session = new Session();

                                        foreach (var panel in this._panels)
                                            panel.Page.OnLoaded();
                                    }
                                    catch (Exception ex)
                                    {
                                        WinUIMessageBox.Show(this,
                                                             ex.Message,
                                                             ex.GetType().Name);
                                        Application.Current.Shutdown();
                                    }
                                });
        }

        private void DockLayoutManager_OnDockItemActivated(object sender, DockItemActivatedEventArgs ea)
        {
            this.ToolsBar.ItemLinks.Clear();

            var item = ea.Item as LayoutViewPanel;
            if (item == null)
                return;

            foreach (var barItem in item.Page.BarItems)
            {
                barItem.Manager = this.BarManager;
                this.ToolsBar.ItemLinks.Add(barItem);
            }
        }
    }
}