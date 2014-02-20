using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using DevExpress.Xpf.Core;
using DevExpress.Xpo;

using RfidCenter.Basic;

namespace RfidBus.Service.Location.Manager
{
    /// <summary>
    ///     Interaction logic for PersonaEditWindow.xaml
    /// </summary>
    public partial class ObjectEditWindow : DXWindow
    {
        public static readonly DependencyProperty ObjectProperty = DependencyProperty.Register("Object",
                                                                                               typeof(database.Object),
                                                                                               typeof(ObjectEditWindow),
                                                                                               new FrameworkPropertyMetadata(null,
                                                                                                                             FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                                                                             PersonaPropertyChanged));

        public ObjectEditWindow()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.OkCommand = new RelayCommand(this.OkCommandExecuted, this.CanOkCommandExecute);

                this.Loaded += this.OnLoaded;
            }

            this.InitializeComponent();
        }

        public database.Object Object
        {
            get { return (database.Object)this.GetValue(ObjectProperty); }
            set { this.SetValue(ObjectProperty, value); }
        }

        public ICommand OkCommand { get; private set; }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.Object == null)
                this.Object = new database.Object(XpoDefault.Session);
        }

        private void OkCommandExecuted(object obj)
        {
            if (this.Object == null)
                return;

            this.Object.Save();

            this.DialogResult = true;
            this.Close();
        }

        private bool CanOkCommandExecute(object obj)
        {
            return (this.Object != null)
                   && (!string.IsNullOrWhiteSpace(this.Object.Name));
        }

        private static void PersonaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as ObjectEditWindow;
            if (window == null)
                return;

            var newValue = e.NewValue as database.Object;
            if (newValue == null)
                window.Object = new database.Object(XpoDefault.Session);
        }
    }
}