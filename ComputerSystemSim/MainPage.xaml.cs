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

        private const int TOTAL_JOBS = 10000;

        private const int WARMUP_JOBS = 1000;

        private static int simClockTicks = 0;

        // Operating at tenth of second granularity
        private const int TICKS_PER_SECOND = 10;

        private int completedJobs = 0;

        private bool setPostWarmup = false;
        private double simClockPostWarmup = 0;
        private int printerJobs = 0;
        private double macPostWarmup = 0;
        private double nextPostWarmup = 0;
        private double printerPostWarmup = 0;
        private double totalJobTime = 0;
        private int jobsDone = 0;

        private double totalJobsInSystemArea = 0;
        private int prevJobsInSystemArea = 0;
        private double prevJobsTime = 0;
        private PriorityQueue<double, Job> jobQueue;
        private double simClock = 0;

        #endregion


        #region Properties (public)

        public Job.EventTypes EventType
        {
            get { throw new NotImplementedException(); }
        }

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

            // First random number seed produces bad results, so prime it before app starts
            PseudoRandomGenerator.RandomNumberGenerator();
            
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

            Mac.Data.Goal = NeXT.Data;

            NeXT.Data.Goal = Printer.Data;

            Printer.Data.Goal = ExitSystem.Data;
        }

        private void RandBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // RandBox.Text = "rand: " + PseudoRandomGenerator.RandomNumberGenerator();
            RandBox.Text = "rand: " + PseudoRandomGenerator.RandomNumberGenerator() + "\nexp: " + PseudoRandomGenerator.ExponentialRVG(3200);
        }

        public void Update()
        {
            SimClockTicks++;
        }
		
		public string GetName()
		{
			return "Computer System Printer Simulation";
		}

        private void TickBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SimulationTick();
        }

        private void FullSimBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            simClock = 0;
            completedJobs = 0;

            jobQueue = new PriorityQueue<double, Job>();

            // Create a 3 UserGroupData, 3 SystemComponentData, and ExitNodeData
            // Create a method in UserGroupData to spawn an event; call it for all user groups here: referred to as "Initializations"
            //      This process schedules a new events at sysclock + interarrivalERVG and adds to queue
            // Create a method in SystemComponentData that sends the job to the Goal and schedules end time for next Job from queue

            Job uG1Job = UserGroup1.Data.GenerateArrival(simClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG1Job.ArrivalTime, uG1Job));
            Job uG2Job = UserGroup2.Data.GenerateArrival(simClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG2Job.ArrivalTime, uG2Job));
            Job uG3Job = UserGroup3.Data.GenerateArrival(simClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG3Job.ArrivalTime, uG3Job));

            // Create an enum for Event types
            /* switch (eventType)
             * case UserGroupCooledDown:
             *      create a new UserGroupCooledDown event at SimClockTime + exponential
             *      if Mac idle,
             *          schedule a Finish event at timeAtWhichMacWillBeIdle
             *      else
             *          add to the MacQueue
             * case MacFinish:
             *      dequeue head of queue
             *      set timeAtWhichMacWillBeIdle to SimClockTime + exponential
             *      schedule a MacFinish event at timeAtWhichMacWillBeIdle
             * case MacFinish:
             *      enqueue a NeXTStart at timeAtWhichNeXTWillBeIdle
             */

            while (completedJobs < (WARMUP_JOBS + TOTAL_JOBS))
            {
                // Pop next event
                KeyValuePair<double, Job> curJob = jobQueue.Dequeue();

                // Advance simulation clock
                simClock = curJob.Value.ArrivalTime;
                if (completedJobs == WARMUP_JOBS && !setPostWarmup)
                {
                    setPostWarmup = true;
                    simClockPostWarmup = simClock;
                    macPostWarmup = Mac.Data.TotalTimeIdle;
                    nextPostWarmup = NeXT.Data.TotalTimeIdle;
                    printerPostWarmup = Printer.Data.TotalTimeIdle;
                }

                // Execute it and add new events back to eventQueue
                switch (curJob.Value.EventType)
                {
                    case Job.EventTypes.UG_FINISH: 

                        Job newUGJob = (curJob.Value.Creator as UserGroupData).GenerateArrival(simClock);

                        jobQueue.Add(new KeyValuePair<double, Job>(newUGJob.ArrivalTime, newUGJob));

                        jobQueue.Add(SystemComponentJobProcess(Mac.Data, curJob.Value));

                        UpdateJobsArea();

                        break;
                    case Job.EventTypes.MAC_FINISH:
                        //Mac.Data.CurState = SystemComponentData.State.IDLE;

                        jobQueue.Add(SystemComponentJobProcess(NeXT.Data, curJob.Value));

                        break;
                    case Job.EventTypes.NEXT_FINISH:
                        // NeXT.Data.CurState = SystemComponentData.State.IDLE;

                        if (printerJobs < 10)
                        {
                            jobQueue.Add(SystemComponentJobProcess(Printer.Data, curJob.Value));
                            printerJobs++;
                        }
                        else
                        {
                            JobExitSystem(curJob);
                        }

                        break;
                    case Job.EventTypes.LASERJET_FINISH:
                        // Printer.Data.CurState = SystemComponentData.State.IDLE;

                        // eventQueue.Add(SystemComponentJobProcess(ExitSystem.Data, curJob, simClock));
                        // TODO: make completedJobs databound to a loading bar in UI

                        JobExitSystem(curJob);

                        printerJobs--;

                        break;
                    default:
                        break;
                }
            }

            RandBox.Text = "Mac util time: " + (1 - ((Mac.Data.TotalTimeIdle - macPostWarmup) / (simClock - simClockPostWarmup)))
                + "\nNeXT util time: " + (1 - ((NeXT.Data.TotalTimeIdle - nextPostWarmup) / (simClock - simClockPostWarmup)))
                + "\nPrinter util time: " + (1 - ((Printer.Data.TotalTimeIdle - printerPostWarmup) / (simClock - simClockPostWarmup)))
                + "\nAvg time per job: " + (totalJobTime / TOTAL_JOBS) + " for " + jobsDone + " jobs"
                + "\nAvg jobs in system: " + (totalJobsInSystemArea / (simClock - simClockPostWarmup));
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

        private void JobExitSystem(KeyValuePair<double, Job> curJob)
        {
            completedJobs++;
            if (completedJobs > WARMUP_JOBS)
            {
                curJob.Value.SystemExitTime = simClock;

                totalJobTime += (curJob.Value.SystemExitTime - curJob.Value.SystemEntryTime);

                jobsDone++;
            }


            UpdateJobsArea();
        }

        private KeyValuePair<double, Job> SystemComponentJobProcess(SystemComponentData component, Job job)
        {
            // if (component.CurState == SystemComponentData.State.IDLE)
            //{
            //if (completedJobs > WARMUP_JOBS && (clock - component.TimeIdleAgain) > 0)
            //{
            //    component.TotalTimeIdle += (clock - component.TimeIdleAgain);
            //}
                //job.ArrivalTime = clock + PseudoRandomGenerator.ExponentialRVG(component.View.ProcessMean);
                //component.CurState = SystemComponentData.State.IN_USE;
            //}
            //else
            //{
            if (simClock < component.TimeIdleAgain)
            {
                // BUSY
                job.ArrivalTime = component.TimeIdleAgain + PseudoRandomGenerator.ExponentialRVG(component.View.ProcessMean);
                //}
            }
            else
            {
                // IDLE
                component.TotalTimeIdle += (simClock - component.TimeIdleAgain);
                job.ArrivalTime += PseudoRandomGenerator.ExponentialRVG(component.View.ProcessMean);
            }

            component.TimeIdleAgain = job.ArrivalTime;
            job.Creator = component;

            return new KeyValuePair<double,Job>(job.ArrivalTime, job);
        }

        private void SimulationTick()
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

            ExitSystem.Data.Update();
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
