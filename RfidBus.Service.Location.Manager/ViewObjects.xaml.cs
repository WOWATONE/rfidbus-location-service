using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using DevExpress.Xpf.Bars;
using DevExpress.Xpf.WindowsUI;
using DevExpress.Xpo;

using RfidCenter.Basic;

namespace RfidBus.Service.Location.Manager
{
    /// <summary>
    ///     Interaction logic for ViewObjects.xaml
    /// </summary>
    public partial class ViewObjects : UserControl, IViewPage
    {
        public static readonly DependencyProperty ObjectsProperty = DependencyProperty.Register("Objects",
                                                                                                typeof(XPCollection<database.Object>),
                                                                                                typeof(ViewObjects),
                                                                                                new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedObjectsProperty = DependencyProperty.Register("SelectedObjects",
                                                                                                        typeof(List<database.Object>),
                                                                                                        typeof(ViewObjects),
                                                                                                        new FrameworkPropertyMetadata(null,
                                                                                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private readonly Lazy<BarItem[]> _lazyBarItems;

        public ViewObjects()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.SelectedObjects = new List<database.Object>();
                this._lazyBarItems = new Lazy<BarItem[]>(() => new BarItem[]
                                                               {
                                                                   new BarButtonItem()
                                                                   {
                                                                       Content = Properties.Resources.Add,
                                                                       ToolTip = Properties.Resources.ViewObjects_Add_Tooltip,
                                                                       Command = new RelayCommand(this.AddCommandExecuted)
                                                                   },
                                                                   new BarButtonItem()
                                                                   {
                                                                       Content = Properties.Resources.Edit,
                                                                       ToolTip = Properties.Resources.ViewObjects_Edit_Tooltip,
                                                                       Command = new RelayCommand(this.EditCommandExecuted, this.CanEditCommandExecute)
                                                                   },
                                                                   new BarButtonItem()
                                                                   {
                                                                       Content = Properties.Resources.Delete,
                                                                       ToolTip = Properties.Resources.ViewObjects_Delete_Tooltip,
                                                                       Command = new RelayCommand(this.DeleteCommandExecuted, this.CanDeleteCommandExecute)
                                                                   }
                                                               });
            }

            this.InitializeComponent();
        }

        public List<database.Object> SelectedObjects
        {
            get { return (List<database.Object>)this.GetValue(SelectedObjectsProperty); }
            set { this.SetValue(SelectedObjectsProperty, value); }
        }

        public XPCollection<database.Object> Objects
        {
            get { return (XPCollection<database.Object>) this.GetValue(ObjectsProperty); }
            set { this.SetValue(ObjectsProperty, value); }
        }

        private void AddCommandExecuted(object obj)
        {
            var window = new ObjectEditWindow() {Owner = Window.GetWindow(this)};
            if (window.ShowDialog() == true)
                this.Objects.Reload();
        }

        private void EditCommandExecuted(object obj)
        {
            if (this.SelectedObjects.Count != 1)
                return;

            using (var session = new Session())
            {
                var window = new ObjectEditWindow()
                             {
                                 Owner = Window.GetWindow(this),
                                 Object = Tools.GetUncachedXPObject(session, this.SelectedObjects[0])
                             };

                if (window.ShowDialog() == true)
                    this.Objects.Reload();
            }
        }

        private bool CanEditCommandExecute(object obj)
        {
            return this.SelectedObjects.Count == 1;
        }

        private void DeleteCommandExecuted(object obj)
        {
            if (this.SelectedObjects.Count == 0)
                return;

            if (WinUIMessageBox.Show(Window.GetWindow(this),
                                     this.SelectedObjects.Count > 1
                                         ? Properties.Resources.DeleteSelectedObjects
                                         : Properties.Resources.DeleteSelectedObject,
                                     Properties.Resources.Warning,
                                     MessageBoxButton.YesNo,
                                     MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var selected = this.SelectedObjects.ToArray();

                foreach (var selectedObject in selected)
                    selectedObject.Delete();

                this.Objects.Reload();
            }
        }

        private bool CanDeleteCommandExecute(object obj)
        {
            return this.SelectedObjects.Count > 0;
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
            get { return Properties.Resources.Objects; }
        }

        public void OnLoaded()
        {
            this.Objects = new XPCollection<database.Object>();
        }
        #endregion
    }
}