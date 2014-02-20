using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using DevExpress.Data.Filtering;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpo;

using RfidCenter.Basic;

namespace RfidBus.Service.Location.Manager
{
    /// <summary>
    ///     Interaction logic for ViewHistory.xaml
    /// </summary>
    public partial class ViewHistory : UserControl, IViewPage
    {
        public static readonly DependencyProperty HistorySourceProperty = DependencyProperty.Register("HistorySource",
                                                                                                      typeof(XPView),
                                                                                                      typeof(ViewHistory),
                                                                                                      new PropertyMetadata(default(XPView)));

        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register("Locations",
                                                                                                  typeof(XPCollection<database.Location>),
                                                                                                  typeof(ViewHistory),
                                                                                                  new PropertyMetadata(default(XPCollection<database.Location>)));

        public static readonly DependencyProperty SelectedLocationProperty = DependencyProperty.Register("SelectedLocation",
                                                                                                         typeof(database.Location),
                                                                                                         typeof(ViewHistory),
                                                                                                         new FrameworkPropertyMetadata(null,
                                                                                                                                       FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ObjectsProperty = DependencyProperty.Register("Objects",
                                                                                                typeof(XPCollection<database.Object>),
                                                                                                typeof(ViewHistory),
                                                                                                new PropertyMetadata(default(XPCollection<Object>)));

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register("SelectedObject",
                                                                                                       typeof(database.Object),
                                                                                                       typeof(ViewHistory),
                                                                                                       new FrameworkPropertyMetadata(null,
                                                                                                                                     FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectFromDateProperty = DependencyProperty.Register("SelectFromDate",
                                                                                                       typeof(DateTime?),
                                                                                                       typeof(ViewHistory),
                                                                                                       new FrameworkPropertyMetadata(null,
                                                                                                                                     FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectTillDateProperty = DependencyProperty.Register("SelectTillDate",
                                                                                                       typeof(DateTime?),
                                                                                                       typeof(ViewHistory),
                                                                                                       new FrameworkPropertyMetadata(null,
                                                                                                                                     FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ViewHistory()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.ApplyFiltersCommand = new RelayCommand(this.ApplyFiltersCommandExecuted, this.CanApplyFiltersCommandExecute);

                this.SelectFromDate = DateTime.Today;
                this.SelectTillDate = DateTime.Today.AddDays(1.0);
            }

            this.InitializeComponent();
        }

        public RelayCommand ApplyFiltersCommand { get; private set; }

        public DateTime? SelectTillDate
        {
            get { return (DateTime?) this.GetValue(SelectTillDateProperty); }
            set { this.SetValue(SelectTillDateProperty, value); }
        }

        public DateTime? SelectFromDate
        {
            get { return (DateTime?) this.GetValue(SelectFromDateProperty); }
            set { this.SetValue(SelectFromDateProperty, value); }
        }

        public XPCollection<database.Object> Objects
        {
            get { return (XPCollection<database.Object>)this.GetValue(ObjectsProperty); }
            set { this.SetValue(ObjectsProperty, value); }
        }

        public database.Object SelectedObject
        {
            get { return (database.Object)this.GetValue(SelectedObjectProperty); }
            set { this.SetValue(SelectedObjectProperty, value); }
        }

        public XPCollection<database.Location> Locations
        {
            get { return (XPCollection<database.Location>) this.GetValue(LocationsProperty); }
            set { this.SetValue(LocationsProperty, value); }
        }

        public database.Location SelectedLocation
        {
            get { return (database.Location) this.GetValue(SelectedLocationProperty); }
            set { this.SetValue(SelectedLocationProperty, value); }
        }

        public XPView HistorySource
        {
            get { return (XPView) this.GetValue(HistorySourceProperty); }
            set { this.SetValue(HistorySourceProperty, value); }
        }

        private void ApplyFiltersCommandExecuted(object obj)
        {
            var criteria = new GroupOperator(GroupOperatorType.And);
            if (this.SelectedObject != null)
                criteria.Operands.Add(new BinaryOperator("Tid", this.SelectedObject.Tid, BinaryOperatorType.Equal));

            if (this.SelectedLocation != null)
            {
                var grp = new GroupOperator(GroupOperatorType.Or);
                foreach (var locationParameter in this.SelectedLocation.LocationParameters)
                {
                    if (locationParameter.Antenna == null)
                        grp.Operands.Add(new BinaryOperator("Reader", locationParameter.Reader, BinaryOperatorType.Equal));
                    else
                    {
                        grp.Operands.Add(new GroupOperator(GroupOperatorType.And,
                                                           new BinaryOperator("Reader", locationParameter.Reader, BinaryOperatorType.Equal),
                                                           new BinaryOperator("Antenna", locationParameter.Antenna.Value, BinaryOperatorType.Equal)));
                    }
                }
                criteria.Operands.Add(grp);
            }

            if (this.SelectFromDate != null)
                criteria.Operands.Add(new BinaryOperator("EntryTime", this.SelectFromDate.Value, BinaryOperatorType.GreaterOrEqual));

            if (this.SelectTillDate != null)
                criteria.Operands.Add(new BinaryOperator("LeaveTime", this.SelectTillDate.Value, BinaryOperatorType.LessOrEqual));

            this.HistorySource.Filter = criteria.Operands.Count == 0 ? null : criteria;

            this.HistorySource.Reload();
            this.Locations.Reload();
            this.Objects.Reload();
        }

        private bool CanApplyFiltersCommandExecute(object obj)
        {
            return (this.SelectFromDate == null)
                   || (this.SelectTillDate == null)
                   || (this.SelectFromDate.Value < this.SelectTillDate.Value);
        }

        private void OnCustomUnboundColumnData(object sender, GridColumnDataEventArgs e)
        {
            if (e.IsGetData)
            {
                if (e.Column.FieldName == "Obj")
                {
                    var record = this.HistorySource[e.ListSourceRowIndex];
                    var tid = Convert.ToString(record["Tid"]);

                    var obj = this.Objects.FirstOrDefault(rec => string.Equals(tid, rec.Tid, StringComparison.InvariantCultureIgnoreCase));
                    e.Value = obj == null ? "" : obj.Name;
                }
                else if (e.Column.FieldName == "Loc")
                {
                    var record = this.HistorySource[e.ListSourceRowIndex];
                    var reader = Convert.ToString(record["Reader"]);
                    var antenna = Convert.ToInt32(record["Antenna"]);

                    var loc = (from location in this.Locations
                               where location.LocationParameters.Any(parameter => parameter.Reader == reader
                                                                                  && (parameter.Antenna == antenna || parameter.Antenna == null))
                               select location).FirstOrDefault();

                    e.Value = loc == null ? "" : loc.Name;
                }
            }
        }

        #region Implementation of IViewPage
        public BarItem[] BarItems
        {
            get { return new BarItem[0]; }
        }

        public string Caption
        {
            get { return Properties.Resources.ViewHistory_Caption; }
        }

        public void OnLoaded()
        {
            this.Objects = new XPCollection<database.Object>();
            this.Locations = new XPCollection<database.Location>();

            var view = new XPView(XpoDefault.Session, typeof(database.TagsHistory));
            view.AddProperty("Tid");
            view.AddProperty("Reader");
            view.AddProperty("Antenna");
            view.AddProperty("EntryTime");
            view.AddProperty("LeaveTime");

            this.HistorySource = view;

            this.ApplyFiltersCommandExecuted(null);
        }
        #endregion
    }
}