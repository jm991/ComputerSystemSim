using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ComputerSystemSim
{
    /// <summary>
    /// Data class to hold simulation output.
    /// </summary>
    public class Output : INotifyPropertyChanged
    {
        #region Variables (private)

        /// <summary>
        /// Utilization of Mac
        /// </summary>
        private double macUtil;

        /// <summary>
        /// Utilization of NeXTStation
        /// </summary>
        private double nextUtil;

        /// <summary>
        /// Utilization of LaserJet
        /// </summary>
        private double printerUtil;

        /// <summary>
        /// Average time a job spends in the whole system
        /// Those jobs finding 10 or more at the printer will be excluded
        /// </summary>
        private double w;

        /// <summary>
        /// Average number of jobs in the whole system
        /// Area_j = (Previous N at t_(j-1)) * [(The current time t_j at which N is changed) – (The last time t_(j-1) at which N was changed)]
        /// Total Area = Sum of Area_j during the course of simulation in steady state
        /// Average Number in the System = L =  T otal Area / Simulation Duration in Steady State
        /// </summary>
        private double l;

        #endregion


        #region Properties (public) 

        public double MacUtil
        {
            get { return macUtil; }
            set
            {
                OnPropertyChanged("MacUtil");
            }
        }

        public double NextUtil
        {
            get { return nextUtil; }
            set
            {
                OnPropertyChanged("NextUtil");
            }
        }

        public double PrinterUtil
        {
            get { return printerUtil; }
            set
            {
                OnPropertyChanged("PrinterUtil");
            }
        }

        public double W
        {
            get { return w; }
            set
            {
                OnPropertyChanged("W");
            }
        }

        public double L
        {
            get { return l; }
            set
            {
                OnPropertyChanged("L");
            }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// Instantiate a new Output with given params.
        /// </summary>
        /// <param name="macUtil">Input for macUtil</param>
        /// <param name="nextUtil">Input for nextUtil</param>
        /// <param name="printerUtil">Input for printerUtil</param>
        /// <param name="w">Input for w</param>
        /// <param name="l">input for l</param>
        public Output(double macUtil, double nextUtil, double printerUtil, double w, double l)
        {
            this.macUtil = macUtil;
            this.nextUtil = nextUtil;
            this.printerUtil = printerUtil;
            this.w = w;
            this.l = l;
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
    }

    /// <summary>
    /// Output display page, indicating results of all 30 trials and the average.
    /// Serves as both a ViewModel/Model since there is no data to store in this page.
    /// 
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class OutputPage : ComputerSystemSim.Common.LayoutAwarePage, INotifyPropertyChanged
    {
        #region Variables (private)

        /// <summary>
        /// Data for ListBox output
        /// </summary>
        private ObservableCollection<Output> outputs;

        /// <summary>
        /// Average for Mac Util
        /// </summary>
        private double macUtilAvg;

        /// <summary>
        /// Average for NeXT Util
        /// </summary>
        private double nextUtilAvg;

        /// <summary>
        /// Average for Printer Util
        /// </summary>
        private double printerUtilAvg;

        /// <summary>
        /// Average length of time job spent in system
        /// </summary>
        private double wAvg;

        /// <summary>
        /// Average jobs in system
        /// </summary>
        private double lAvg;

        #endregion


        #region Properties (public)

        public double MacUtilAvg
        {
            get
            {
                return macUtilAvg;
            }
            set
            {
                macUtilAvg = value;
                OnPropertyChanged("MacUtilAvg");
            }
        }

        public double NextUtilAvg
        {
            get { return nextUtilAvg; }
            set
            {
                nextUtilAvg = value;
                OnPropertyChanged("NextUtilAvg");
            }
        }

        public double PrinterUtilAvg
        {
            get { return printerUtilAvg; }
            set
            {
                printerUtilAvg = value;
                OnPropertyChanged("PrinterUtilAvg");
            }
        }

        public double WAvg
        {
            get { return wAvg; }
            set
            {
                wAvg = value;
                OnPropertyChanged("WAvg");
            }
        }

        public double LAvg
        {
            get { return lAvg; }
            set
            {
                lAvg = value;
                OnPropertyChanged("LAvg");
            }
        }

        public ObservableCollection<Output> Outputs
        {
            get
            {
                return outputs;
            }
            set
            {
                outputs = value;
                OnPropertyChanged("Outputs");
            }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// Instantiate the page and set the Model data to itself.
        /// </summary>
        public OutputPage()
        {
            this.InitializeComponent();

            outputs = new ObservableCollection<Output>();

            this.DataContext = this;
        }

        #endregion


        #region Event Handlers

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (navigationParameter is ObservableCollection<Output>)
            {
                Outputs = (navigationParameter as ObservableCollection<Output>);

                double macUtilSum = 0;
                double nextUtilSum = 0;
                double printerUtilSum = 0;
                double wSum = 0;
                double lSum = 0;

                foreach (Output curOutput in Outputs)
                {
                    macUtilSum += curOutput.MacUtil;
                    nextUtilSum += curOutput.NextUtil;
                    printerUtilSum += curOutput.PrinterUtil;
                    wSum += curOutput.W;
                    lSum += curOutput.L;

                    Debug.WriteLine("" + curOutput.MacUtil + "\t" + curOutput.NextUtil + "\t" + curOutput.PrinterUtil + "\t" + curOutput.W + "\t" + curOutput.L);
                }

                MacUtilAvg = macUtilSum / Outputs.Count;
                NextUtilAvg = nextUtilSum / Outputs.Count;
                PrinterUtilAvg = printerUtilSum / Outputs.Count;
                WAvg = wSum / Outputs.Count;
                LAvg = lSum / Outputs.Count;

                Debug.WriteLine("Average\n" + MacUtilAvg + "\t" + NextUtilAvg + "\t" + PrinterUtilAvg + "\t" + WAvg + "\t" + LAvg);
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
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
    }
}
