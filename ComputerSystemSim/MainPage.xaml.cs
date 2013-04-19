using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Structs
        
        private struct WarmUpValues
        {
            private bool setPostWarmup;
            private double simClockPostWarmup;
            private double macPostWarmup;
            private double neXTPostWarmup;
            private double printerPostWarmup;

            public bool SetPostWarmup { get { return setPostWarmup; } set { setPostWarmup = value; } }
            public double SimClockPostWarmup { get { return simClockPostWarmup; } set { simClockPostWarmup = value; } }
            public double MacPostWarmup { get { return macPostWarmup; } set { macPostWarmup = value; } }
            public double NeXTPostWarmup { get { return neXTPostWarmup; } set { neXTPostWarmup = value; } }
            public double PrinterPostWarmup { get { return printerPostWarmup; } set { printerPostWarmup = value; } }

            public void Init()
            {
                setPostWarmup = false;
                simClockPostWarmup = 0;
                macPostWarmup = 0;
                neXTPostWarmup = 0;
                printerPostWarmup = 0;
            }

            public void Set(bool setPostWarmup, double simClockPostWarmup, double macPostWarmup, double neXTPostWarmup, double printerPostWarmup)
            {
                this.setPostWarmup = setPostWarmup;
                this.simClockPostWarmup = simClockPostWarmup;
                this.macPostWarmup = macPostWarmup;
                this.neXTPostWarmup = neXTPostWarmup;
                this.printerPostWarmup = printerPostWarmup;
            }
        }

        #endregion


        #region Variables (private)

        private const int TOTAL_JOBS = 10000;
        private const int WARMUP_JOBS = 1000;
        private const int TRIALS = 30;
        private const int MAX_PRINTER_JOBS = 10;

        private PriorityQueue<double, Job> jobQueue;
        private double simClock = 0;
        private WarmUpValues warmUpValues;
        private int completedJobs = 0;
        private double totalJobTime = 0;
        private int jobsDone = 0;
        private int printerJobs = 0;
        private double totalJobsInSystemArea = 0;
        private int prevJobsInSystemArea = 0;
        private double prevJobsTime = 0;

        // TODO: move animation onto the same system as the full simulation
        private static int simClockTicks = 0;

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


        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();

            warmUpValues = new WarmUpValues();
            warmUpValues.Init();

            // First random number seed produces bad results, so prime it before app starts
            PseudoRandomGenerator.RandomNumberGenerator();
            
            // Should separate the data out into its own object type
            this.DataContext = this;
        }

        #endregion

        #region Event handlers

        private void RandBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RandBox.Text = "rand: " + PseudoRandomGenerator.RandomNumberGenerator() + "\nexp: " + PseudoRandomGenerator.ExponentialRVG(3200);
        }

        private void TickBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SimulationTick();
        }

        private async void FullSimBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ProgressBar.IsIndeterminate = true;

            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync( Windows.UI.Core.CoreDispatcherPriority.Normal, () => { ComputeSimulation(); } );

            // Update the UI with results
            ProgressBar.IsIndeterminate = false;
            RandBox.Text =
                    "Mac util time: " + (1 - ((Mac.Data.TotalTimeIdle - warmUpValues.MacPostWarmup) / (simClock - warmUpValues.SimClockPostWarmup)))
                + "\nNeXT util time: " + (1 - ((NeXT.Data.TotalTimeIdle - warmUpValues.NeXTPostWarmup) / (simClock - warmUpValues.SimClockPostWarmup)))
                + "\nPrinter util time: " + (1 - ((Printer.Data.TotalTimeIdle - warmUpValues.PrinterPostWarmup) / (simClock - warmUpValues.SimClockPostWarmup)))
                + "\nW Avg time per job: " + (totalJobTime / TOTAL_JOBS) + " for " + jobsDone + " jobs"
                + "\nL Avg jobs in system: " + (totalJobsInSystemArea / (simClock - warmUpValues.SimClockPostWarmup));
        }

        #endregion


        #region Methods

        private void ComputeSimulation()
        {
            simClock = 0;
            completedJobs = 0;
            jobQueue = new PriorityQueue<double, Job>();

            // Initializations
            Job uG1Job = UserGroup1.Data.GenerateArrival(simClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG1Job.ArrivalTime, uG1Job));
            Job uG2Job = UserGroup2.Data.GenerateArrival(simClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG2Job.ArrivalTime, uG2Job));
            Job uG3Job = UserGroup3.Data.GenerateArrival(simClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG3Job.ArrivalTime, uG3Job));

            while (completedJobs < (WARMUP_JOBS + TOTAL_JOBS))
            {
                // Pop next event
                KeyValuePair<double, Job> curJob = jobQueue.Dequeue();

                // Advance simulation clock
                simClock = curJob.Value.ArrivalTime;

                // Set necessary variables once warm up period finished
                if (completedJobs == WARMUP_JOBS && !warmUpValues.SetPostWarmup)
                {
                    warmUpValues.Set(true, simClock, Mac.Data.TotalTimeIdle, NeXT.Data.TotalTimeIdle, Printer.Data.TotalTimeIdle);
                }

                // Execute it and add new events back to eventQueue
                switch (curJob.Value.EventType)
                {
                    case Job.EventTypes.UG_FINISH:
                        // Spawn new UserGroup event 
                        Job newUGJob = (curJob.Value.Creator as UserGroupData).GenerateArrival(simClock);
                        jobQueue.Add(new KeyValuePair<double, Job>(newUGJob.ArrivalTime, newUGJob));

                        // Contributes to computation of L (average number of jobs in the whole system)
                        UpdateJobsArea();

                        // Send the current job down the system
                        jobQueue.Add(SystemComponentJobProcess(Mac.Data, curJob.Value));

                        break;
                    case Job.EventTypes.MAC_FINISH:
                        jobQueue.Add(SystemComponentJobProcess(NeXT.Data, curJob.Value));

                        break;
                    case Job.EventTypes.NEXT_FINISH:
                        if (printerJobs < MAX_PRINTER_JOBS)
                        {
                            jobQueue.Add(SystemComponentJobProcess(Printer.Data, curJob.Value));
                            printerJobs++;
                        }
                        else
                        {
                            JobExitSystem(curJob, false);
                        }

                        break;
                    case Job.EventTypes.LASERJET_FINISH:
                        JobExitSystem(curJob, true);
                        printerJobs--;

                        break;
                    default:
                        break;
                }
            }
        }

        private void UpdateJobsArea()
        {
            if (completedJobs > WARMUP_JOBS)
            {
                double area = prevJobsInSystemArea * (simClock - prevJobsTime);
                totalJobsInSystemArea += area;
                
                prevJobsInSystemArea = jobQueue.Count;
                prevJobsTime = simClock;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curJob"></param>
        /// <param name="includeInMetrics">Those jobs finding 10 or more at the printer will be excluded in the calculation of W.</param>
        private void JobExitSystem(KeyValuePair<double, Job> curJob, bool includeInMetrics)
        {
            completedJobs++;

            if (completedJobs > WARMUP_JOBS && includeInMetrics)
            {
                curJob.Value.SystemExitTime = simClock;

                totalJobTime += (curJob.Value.SystemExitTime - curJob.Value.SystemEntryTime);

                jobsDone++;
            }

            // Contributes to computation of L (average number of jobs in the whole system)
            UpdateJobsArea();
        }

        private KeyValuePair<double, Job> SystemComponentJobProcess(SystemComponentData component, Job job)
        {
            // BUSY
            if (simClock < component.TimeIdleAgain)
            {
                job.ArrivalTime = component.TimeIdleAgain + PseudoRandomGenerator.ExponentialRVG(component.ProcessMean);
            }
            // IDLE
            else
            {
                component.TotalTimeIdle += (simClock - component.TimeIdleAgain);
                job.ArrivalTime += PseudoRandomGenerator.ExponentialRVG(component.ProcessMean);
            }

            component.TimeIdleAgain = job.ArrivalTime;
            job.Creator = component;

            return new KeyValuePair<double,Job>(job.ArrivalTime, job);
        }

        private void SimulationTick()
        {
            SimClockTicks++;

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

            ExitSystem.Data.Update();
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
}
