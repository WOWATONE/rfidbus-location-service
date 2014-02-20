using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using DevExpress.Xpf.Bars;
using DevExpress.Xpo;

using RfidCenter.Basic;

namespace RfidBus.Service.Location.Manager
{
    /// <summary>
    ///     Interaction logic for ViewLocations.xaml
    /// </summary>
    public partial class ViewLocations : UserControl, IViewPage
    {
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register("Locations",
                                                                                                  typeof(XPCollection<database.Location>),
                                                                                                  typeof(ViewLocations),
                                                                                                  new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedLocationsProperty = DependencyProperty.Register("SelectedLocations",
                                                                                                          typeof(List<database.Location>),
                                                                                                          typeof(ViewLocations),
                                                                                                          new FrameworkPropertyMetadata(null,
                                                                                                                                        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private readonly Lazy<BarItem[]> _lazyBarItems;

        public ViewLocations()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.SelectedLocations = new List<database.Location>(5);

                this._lazyBarItems = new Lazy<BarItem[]>(() => new BarItem[]
                                                               {
                                                                   new BarButtonItem()
                                                                   {
                                                                       Content = Properties.Resources.Add,
                                                                       ToolTip = Properties.Resources.ViewLocations_Add_Tooltip,
                                                                       Command = new RelayCommand(this.AddCommandExecuted)
                                                                   },
                                                                   new BarButtonItem()
                                                                   {
                                                                       Content = Properties.Resources.Edit,
                                                                       ToolTip = Properties.Resources.ViewLocations_Edit_Tooltip,
                                                                       Command = new RelayCommand(this.EditCommandExecuted, this.CanEditCommandExecute)
                                                                   },
                                                                   new BarButtonItem()
                                                                   {
                                                                       Content = Properties.Resources.Delete,
                                                                       ToolTip = Properties.Resources.ViewLocations_Delete_Tooltip,
                                                                       Command = new RelayCommand(this.DeleteCommandExecuted, this.CanDeleteCommandExecute)
                                                                   }
                                                               });
            }

            this.InitializeComponent();
        }

        public List<database.Location> SelectedLocations
        {
            get { return (List<database.Location>) this.GetValue(SelectedLocationsProperty); }
            set { this.SetValue(SelectedLocationsProperty, value); }
        }

        public XPCollection<database.Location> Locations
        {
            get { return (XPCollection<database.Location>) this.GetValue(LocationsProperty); }
            set { this.SetValue(LocationsProperty, value); }
        }

        private void AddCommandExecuted(object obj)
        {
            var window = new LocationEditWindow()
                         {
                             Owner = Window.GetWindow(this)
                         };

            if (window.ShowDialog() == true)
                this.Locations.Reload();
        }

        private void EditCommandExecuted(object obj)
        {
            if (this.SelectedLocations.Count != 1)
                return;

            using (var session = new Session())
            {
                var window = new LocationEditWindow()
                             {
                                 Owner = Window.GetWindow(this),
                                 Location = Tools.GetUncachedXPObject(session, this.SelectedLocations[0])
                             };

                if (window.ShowDialog() == true)
                    this.Locations.Reload();
            }
        }

        private bool CanEditCommandExecute(object obj)
        {
            return this.SelectedLocations.Count == 1;
        }

        private void DeleteCommandExecuted(object obj)
        {
            if (this.SelectedLocations.Count == 0)
                return;

            if (DevExpress.Xpf.WindowsUI.WinUIMessageBox.Show(Window.GetWindow(this),
                                                              this.SelectedLocations.Count > 1
                                                                  ? Properties.Resources.DeleteSelectedLocations
                                                                  : Properties.Resources.DeleteSelectedLocation,
                                                              Properties.Resources.Warning,
                                                              MessageBoxButton.YesNo,
                                                              MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var selected = this.SelectedLocations.ToArray();

                foreach (var location in selected)
                    location.Delete();

                this.Locations.Reload();
            }
        }

        private bool CanDeleteCommandExecute(object obj)
        {
            return this.SelectedLocations.Count > 0;
        }

        private void GridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.EditCommandExecuted(this);
        }

        #region Implementation of IViewPage
        public BarItem[] BarItems
        {
            get { return this._lazyBarItems.Value; }
        }

        public string Caption
        {
            get { return Properties.Resources.Locations; }
        }

        public void OnLoaded()
        {
            this.Locations = new XPCollection<database.Location>();
        }
        #endregion
    }
}