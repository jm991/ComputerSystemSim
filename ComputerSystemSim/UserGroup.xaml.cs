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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ComputerSystemSim
{
    public sealed partial class UserGroup : UserControl, INotifyPropertyChanged
    {
        #region Variables (private)

        private UserGroupData data;

        #endregion


        #region Properties (public)

        public int Count
        {
            get { return data.CurEventCooldown; }
            set
            {
                data.CurEventCooldown = value;
                OnPropertyChanged("Count");
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

        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion


        #region Dependency Properties

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register
        (
            "GroupName",
            typeof(string),
            typeof(UserGroup),
            new PropertyMetadata(0, new PropertyChangedCallback(ChangeGroupName))
        );

        private static void ChangeGroupName(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as UserGroup).UpdateGroupName(e.NewValue.ToString());
        }

        private void UpdateGroupName(string newText)
        {
            NameBox.Text = newText;
        }

        public static readonly DependencyProperty InterarrivalProperty = DependencyProperty.Register
        (
            "InterarrivalERVGMean",
            typeof(double),
            typeof(UserGroup),
            new PropertyMetadata(0, new PropertyChangedCallback(ChangeInterarrival))
        );

        private static void ChangeInterarrival(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as UserGroup).UpdateInterarrival(e.NewValue);
        }

        private void UpdateInterarrival(Object newDouble)
        {
            InterarrivalBox.Text = "" + newDouble.ToString();
        } 

        #endregion


        #region Constructors 

        public UserGroup()
        {
            this.InitializeComponent();

            data = new UserGroupData(this);

            this.DataContext = data;
        }

        #endregion


        #region Methods



        #endregion
    }
}
