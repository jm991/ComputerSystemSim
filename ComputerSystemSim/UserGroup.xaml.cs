﻿using System;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ComputerSystemSim
{
    public sealed partial class UserGroup : UserControl, INotifyPropertyChanged
    {
        #region Variables (private)

        private UserGroupData data;

        #endregion


        #region Properties (public)

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
                    return val;
                else
                    return -1;  // error val
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

        public static readonly DependencyProperty GoalProperty = DependencyProperty.Register
        (
            "Goal",
            typeof(Updatable),
            typeof(UserGroup),
            new PropertyMetadata(new PropertyChangedCallback(OnGoalChanged))
        );

        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register
        (
            "IconSource",
            typeof(Uri), 
            typeof(UserGroup),
            new PropertyMetadata(new PropertyChangedCallback(OnIconSourceChanged))
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
            (source as UserGroup).InterarrivalBox.Text = "" + e.NewValue;
        }

        #endregion


        #region Constructors 

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
