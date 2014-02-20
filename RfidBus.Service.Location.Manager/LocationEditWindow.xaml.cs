using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using DevExpress.Xpo;

using RfidBus.Service.Location.Manager.database;

using RfidCenter.Basic;

namespace RfidBus.Service.Location.Manager
{
    /// <summary>
    ///     Interaction logic for LocationEditWindow.xaml
    /// </summary>
    public partial class LocationEditWindow
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location",
                                                                                                 typeof(database.Location),
                                                                                                 typeof(LocationEditWindow),
                                                                                                 new PropertyMetadata(null, LocationPropertyChanged));

        public static readonly DependencyProperty BusReadersProperty = DependencyProperty.Register("BusReaders",
                                                                                                   typeof(UiReaderRecord[]),
                                                                                                   typeof(LocationEditWindow),
                                                                                                   new PropertyMetadata(default(UiReaderRecord[])));

        public LocationEditWindow()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.RemoveReaderCommand = new RelayCommand(this.RemoveReaderCommandExecuted, this.CanRemoveReaderCommandExecute);
                this.OkCommand = new RelayCommand(this.OkCommandExecuted, this.CanOkCommandExecute);

                this.Loaded += this.OnLoaded;
            }

            this.InitializeComponent();
        }

        public UiReaderRecord[] BusReaders
        {
            get { return (UiReaderRecord[]) this.GetValue(BusReadersProperty); }
            set { this.SetValue(BusReadersProperty, value); }
        }

        public database.Location Location
        {
            get { return (database.Location) this.GetValue(LocationProperty); }
            set { this.SetValue(LocationProperty, value); }
        }

        public ICommand RemoveReaderCommand { get; private set; }
        public ICommand OkCommand { get; private set; }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.Location == null)
                this.Location = new database.Location(XpoDefault.Session);

            this.BusReaders = await ConnectionManager.Instance.GetBusReaders();
        }

        private void OkCommandExecuted(object obj)
        {
            if (this.Location == null)
                return;

            this.Location.Save();

            this.DialogResult = true;
            this.Close();
        }

        private bool CanOkCommandExecute(object obj)
        {
            return (this.Location != null)
                   && (!string.IsNullOrWhiteSpace(this.Location.Name));
        }

        private static void LocationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as LocationEditWindow;
            if (window == null)
                return;

            var newValue = e.NewValue as database.Location;
            if (newValue == null)
                window.Location = new database.Location(XpoDefault.Session);
        }

        private void RemoveReaderCommandExecuted(object obj)
        {
            var parameters = obj as LocationParameters;
            if (parameters == null)
                return;

            this.Location.LocationParameters.Remove(parameters);
        }

        private bool CanRemoveReaderCommandExecute(object obj)
        {
            var parameters = obj as LocationParameters;
            return parameters != null;
        }
    }
}