using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ComputerSystemSim
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// Some fields are static to emulate Singleton behaviour (i.e., accessible statically from any other class in the simulation).
    /// </summary>
    public sealed partial class MainPage : Page, Updatable, INotifyPropertyChanged
    {
        #region Variables (private)

        private const int totalJobs = 15000;

        private const int warmupJobs = 3000;

        private static int simClockTicks = 0;

        // Operating at tenth of second granularity
        private const int TICKS_PER_SECOND = 10;

        private int jobsInSystem = 0;

        #endregion


        #region Properties (public)

        public static int SimClockTicksStatic
        {
            get
            {
                return simClockTicks;
            }
        }

        public int SimClockTicks
        {
            get 
            { 
                return simClockTicks; 
            }
            set
            {
                simClockTicks = value;
                OnPropertyChanged("SimClockTicks");
            }
        }

        #endregion


        public MainPage()
        {
            this.InitializeComponent();
            
            // Should separate the data out into its own object type
            this.DataContext = this;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            foreach (var child in UserGroups.Children)
            {
                if (child is UserGroup)
                {
                    (child as UserGroup).Data.Goal = Mac.Data;
                }
            }
        }

        private void RandBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RandBox.Text = "rand: " + PseudoRandomGenerator.RandomNumberGenerator() + "\nexp: " + PseudoRandomGenerator.ExponentialRVG(3200);
        }

        public void Update()
        {
            SimClockTicks++;
        }

        private void TickBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Update();
            
            // Generate events from user groups
            foreach (var child in UserGroups.Children)
            {
                if (child is UserGroup)
                {
                    (child as UserGroup).Data.Update();
                }
            }

            // Generate events from components of system
            foreach (var child in SystemComponents.Children)
            {
                if (child is SystemComponent)
                {
                    (child as SystemComponent).Data.Update();
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
