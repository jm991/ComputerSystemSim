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
    public sealed partial class UserGroup : UserControl, Updatable, INotifyPropertyChanged
    {
        #region Variables (private)

        private int curEventCooldown = 0;

        private ObservableCollection<Event> eventQueue;

        #endregion


        #region Properties (public)

        public ObservableCollection<Event> EventQueue
        {
            get { return eventQueue; }
            set 
            {
                eventQueue = value;
                OnPropertyChanged("EventQueue");
            }
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

        public int CurEventCooldown
        {
            get { return curEventCooldown; }
            set
            {
                curEventCooldown = value;
                OnPropertyChanged("CurEventCooldown");
            }
        }

        #endregion


        #region Dependency Properties

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register
        (
            "GroupName",
            typeof(string),
            typeof(UserGroup),
            new PropertyMetadata(0, new PropertyChangedCallback(ChangeText))
        );

        private static void ChangeText(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as UserGroup).UpdateText(e.NewValue.ToString());
        }

        private void UpdateText(string newText)
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


        public UserGroup()
        {
            EventQueue = new ObservableCollection<Event>();

            this.InitializeComponent();

            // TODO: move to external data class
            this.DataContext = this;
        }

        public Event GenerateArrival()
        {
            CurEventCooldown = (int)PseudoRandomGenerator.ExponentialRVG(InterarrivalERVGMean);
            Event newEvent = new Event(CurEventCooldown, this);

            return newEvent;
        }

        public void Update()
        {
            CurEventCooldown -= 1;

            // TODO: should be exactly zero; otherwise something wrong has happened
            if (CurEventCooldown <= 0)
            {
                EventQueue.Add(GenerateArrival());
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            object o = EventsListBox.SelectedItem;
            if (o != null)
            {
                Event selEvent = o as Event;
                
                int val;
                bool parse = int.TryParse(textBox1.Text, out val);
                if (parse)
                {
                    selEvent.ArrivalTime = val;
                }
            }
        }

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
    }
}
