using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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
                {
                    return val;
                }
                else
                {
                    return -1;  // error val
                }
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
                Data.EventType = EventType;
            }
        }

        public Updatable Goal
        {
            get 
            {
                return (Updatable) GetValue(GoalProperty); 
            }
            set
            {
                SetValue(GoalProperty, value);
            }
        }

        #endregion


        #region Dependency Properties

        public static readonly DependencyProperty GoalProperty = DependencyProperty.Register
        (
            "Goal",
            typeof(Updatable),
            typeof(SystemComponent),
            new PropertyMetadata(new PropertyChangedCallback(OnGoalChanged))
        );

        public static readonly DependencyProperty EventTypeProperty = DependencyProperty.Register
        (
            "EventType",
            typeof(Job.EventTypes),
            typeof(SystemComponent),
            new PropertyMetadata(new PropertyChangedCallback(OnEventTypeChanged))
        );

        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register
        (
            "IconSource",
            typeof(Uri),
            typeof(SystemComponent),
            new PropertyMetadata(new PropertyChangedCallback(OnIconSourceChanged))
        );

        public static readonly DependencyProperty ComponentNameProperty = DependencyProperty.Register
        (
            "ComponentName",
            typeof(string),
            typeof(SystemComponent),
            new PropertyMetadata(0, new PropertyChangedCallback(ComponentNameChanged))
        );

        public static readonly DependencyProperty ProcessMeanProperty = DependencyProperty.Register
        (
            "ProcessMean",
            typeof(double),
            typeof(SystemComponent),
            new PropertyMetadata(0, new PropertyChangedCallback(OnProcessMeanChanged))
        );

        #endregion


        #region Event handlers

        private void EventsListBox_LayoutUpdated(object sender, object e)
        {
            CurJobViewer.DataContext = data.CurJob;
        }

        private void UserControl_LayoutUpdated_1(object sender, object e)
        {
            try
            {
                Data.Goal = Goal;
            }
            catch (InvalidCastException castE)
            {
                // Ignore; this happens because I'm waiting for the dependency property to register
            }
        }

        private static void OnGoalChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SystemComponent).Data.Goal = (Updatable)e.NewValue;
        }

        private static void OnEventTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SystemComponent).Data.EventType = (Job.EventTypes) e.NewValue;
        }

        private static void OnIconSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Uri iconSource = (Uri)e.NewValue;
            (sender as SystemComponent).GroupImage.Source = new BitmapImage(iconSource);
            (sender as SystemComponent).Data.IconSource = iconSource;
        }

        private static void ComponentNameChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            string componentName = e.NewValue.ToString();
            (source as SystemComponent).NameBox.Text = componentName;
            (source as SystemComponent).Data.Name = componentName;
        }

        private static void OnProcessMeanChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            double processMean = (double)e.NewValue;
            (source as SystemComponent).ProcessBox.Text = "" + processMean;
            (source as SystemComponent).Data.ProcessMean = processMean;
        }

        #endregion


        #region Constructors

        public SystemComponent()
        {
            this.InitializeComponent();

            data = new SystemComponentData();

            this.DataContext = data;
            this.GroupImage.DataContext = this;

            CurJobViewer.DataContext = data.CurJob;
        }

        #endregion


        #region Methods

        #endregion
    }
}
