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
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Windows.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ComputerSystemSim
{
    /// <summary>
    /// ViewModel code behind for UserGroup View.
    /// UserGroup is a visual representation of an entity that creates Jobs.
    /// </summary>
    public sealed partial class UserGroup : UserControl, INotifyPropertyChanged
    {
        #region Variables (private)

        /// <summary>
        /// Model data for UserGroup
        /// </summary>
        private UserGroupData data;

        #endregion


        #region Properties (public)

        public Color GroupColor
        {
            get { return (Color) GetValue(GroupColorProperty); }
            set
            {
                SetValue(GroupColorProperty, value);
            }
        }

        public Updatable Goal
        {
            get
            {
                return (Updatable)GetValue(GoalProperty);
            }
            set
            {
                SetValue(GoalProperty, value);
            }
        }

        public UserGroupData Data
        {
            get { return data; }
            set { data = value; }
        }

        public double InterarrivalERVGMean
        {
            get 
            { 
                double val = 0;
                bool parsed = double.TryParse(GetValue(InterarrivalProperty).ToString(), out val);
                if (parsed)
                {
                    return val;
                }
                else
                {
                    return -1;  // error val
                }
            }
            set { SetValue(InterarrivalProperty, value); }
        }

        public string GroupName
        {
            get { return (string) GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public Uri IconSource
        {
            get { return GetValue(IconSourceProperty) as Uri; }
            set 
            {
                SetValue(IconSourceProperty, value);
            }
        }

        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion


        #region Dependency Properties

        public static readonly DependencyProperty GroupColorProperty = DependencyProperty.Register
        (
            "GroupColor",
            typeof(Color),
            typeof(UserGroup),
            new PropertyMetadata(0, new PropertyChangedCallback(OnGroupColorChanged))
        );

        public static readonly DependencyProperty GoalProperty = DependencyProperty.Register
        (
            "Goal",
            typeof(Updatable),
            typeof(UserGroup),
            new PropertyMetadata(0, new PropertyChangedCallback(OnGoalChanged))
        );

        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register
        (
            "IconSource",
            typeof(Uri), 
            typeof(UserGroup),
            new PropertyMetadata(0, new PropertyChangedCallback(OnIconSourceChanged))
        );

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register
        (
            "GroupName",
            typeof(string),
            typeof(UserGroup),
            new PropertyMetadata(0, new PropertyChangedCallback(OnGroupNameChanged))
        );

        public static readonly DependencyProperty InterarrivalProperty = DependencyProperty.Register
        (
            "InterarrivalERVGMean",
            typeof(double),
            typeof(UserGroup),
            new PropertyMetadata(0, new PropertyChangedCallback(OnInterarrivalChanged))
        );

        #endregion  


        #region Event handlers

        private static void OnGroupColorChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as UserGroup).Data.GroupColor = (Color)e.NewValue;
        }

        private static void OnGoalChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as UserGroup).Data.Goal = (Updatable)e.NewValue;
        }

        private static void OnIconSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as UserGroup).GroupImage.Source = new BitmapImage((Uri)e.NewValue);
        }

        private static void OnGroupNameChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as UserGroup).NameBox.Text = e.NewValue.ToString();
        }

        private static void OnInterarrivalChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            double interarrival = (double)e.NewValue;
            (source as UserGroup).Data.InterarrivalERVGMean = interarrival;
        }

        #endregion


        #region Constructors        

        /// <summary>
        /// Initializes the ViewModel, setting the Model data.
        /// </summary>
        public UserGroup()
        {
            this.InitializeComponent();

            data = new UserGroupData(this);

            this.DataContext = data;
            this.GroupImage.DataContext = this;
        }

        #endregion


        #region Methods



        #endregion
    }
}
