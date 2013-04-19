using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ComputerSystemSim
{
    public sealed partial class SystemComponent : UserControl
    {
        #region Variables (private)

        private SystemComponentData data;

        #endregion


        #region Properties (public)

        public SystemComponentData Data
        {
            get { return data; }
            set { data = value; }
        }

        public double ProcessMean
        {
            get
            {
                double val = 0;
                bool parsed = double.TryParse(GetValue(ProcessMeanProperty).ToString(), out val);
                if (parsed)
                    return val;
                else
                    return -1;  // error val
            }
            set { SetValue(ProcessMeanProperty, value); }
        }

        public string ComponentName
        {
            get { return (string)GetValue(ComponentNameProperty); }
            set { SetValue(ComponentNameProperty, value); }
        }

        public Uri IconSource
        {
            get { return GetValue(IconSourceProperty) as Uri; }
            set
            {
                SetValue(IconSourceProperty, value);
            }
        }

        public Job.EventTypes EventType
        {
            get { return (Job.EventTypes) GetValue(EventTypeProperty); }
            set
            {
                SetValue(EventTypeProperty, value);
            }
        }

        #endregion


        #region Dependency Properties

        public static readonly DependencyProperty EventTypeProperty = DependencyProperty.Register
        (
            "EventType",
            typeof(Job.EventTypes),
            typeof(SystemComponent),
            new PropertyMetadata(new PropertyChangedCallback(OnEventTypeChanged))
        );

        private static void OnEventTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SystemComponent).Data.EventType = (Job.EventTypes) e.NewValue;
        }

        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register
        (
            "IconSource",
            typeof(Uri),
            typeof(SystemComponent),
            new PropertyMetadata(new PropertyChangedCallback(OnIconSourceChanged))
        );

        private static void OnIconSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SystemComponent).GroupImage.Source = new BitmapImage((Uri) e.NewValue);
        }

        public static readonly DependencyProperty ComponentNameProperty = DependencyProperty.Register
        (
            "ComponentName",
            typeof(string),
            typeof(SystemComponent),
            new PropertyMetadata(0, new PropertyChangedCallback(ComponentNameChanged))
        );

        private static void ComponentNameChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as SystemComponent).NameBox.Text = e.NewValue.ToString();
        }

        public static readonly DependencyProperty ProcessMeanProperty = DependencyProperty.Register
        (
            "ProcessMean",
            typeof(double),
            typeof(SystemComponent),
            new PropertyMetadata(0, new PropertyChangedCallback(OnProcessMeanChanged))
        );

        private static void OnProcessMeanChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as SystemComponent).ProcessBox.Text = "" + e.NewValue;
        }

        #endregion


        #region Constructors

        public SystemComponent()
        {
            this.InitializeComponent();

            data = new SystemComponentData(this);
            
            this.DataContext = data;
            this.GroupImage.DataContext = this;
        }

        #endregion


        #region Methods

        public void SetCurJobViewer(Job job)
        {
            CurJobViewer.DataContext = job;
        }

        #endregion
    }
}
