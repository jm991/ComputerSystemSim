using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ComputerSystemSim
{
    /// <summary>
    /// Simulation visualization page where the user can initiate a simulation and 
    /// watch as the data flows through the system.
    /// TODO: need to split out Model from this ViewModel
    /// 
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SimulationPage : ComputerSystemSim.Common.LayoutAwarePage, INotifyPropertyChanged
    {
        #region Structs
        
        /// <summary>
        /// Struct that holds data once warmup period is reached
        /// </summary>
        private struct WarmUpValues
        {
            #region Variables (private)

            private bool setPostWarmup;
            private double simClockPostWarmup;
            private double macPostWarmup;
            private double neXTPostWarmup;
            private double printerPostWarmup;

            #endregion


            #region Properties

            public bool SetPostWarmup { get { return setPostWarmup; } set { setPostWarmup = value; } }
            public double SimClockPostWarmup { get { return simClockPostWarmup; } set { simClockPostWarmup = value; } }
            public double MacPostWarmup { get { return macPostWarmup; } set { macPostWarmup = value; } }
            public double NeXTPostWarmup { get { return neXTPostWarmup; } set { neXTPostWarmup = value; } }
            public double PrinterPostWarmup { get { return printerPostWarmup; } set { printerPostWarmup = value; } }

            #endregion


            #region Methods

            /// <summary>
            /// Initialization method
            /// </summary>
            public void Init()
            {
                setPostWarmup = false;
                simClockPostWarmup = 0;
                macPostWarmup = 0;
                neXTPostWarmup = 0;
                printerPostWarmup = 0;
            }

            /// <summary>
            /// Sets all variables in struct
            /// </summary>
            /// <param name="setPostWarmup"></param>
            /// <param name="simClockPostWarmup"></param>
            /// <param name="macPostWarmup"></param>
            /// <param name="neXTPostWarmup"></param>
            /// <param name="printerPostWarmup"></param>
            public void Set(bool setPostWarmup, double simClockPostWarmup, double macPostWarmup, double neXTPostWarmup, double printerPostWarmup)
            {
                this.setPostWarmup = setPostWarmup;
                this.simClockPostWarmup = simClockPostWarmup;
                this.macPostWarmup = macPostWarmup;
                this.neXTPostWarmup = neXTPostWarmup;
                this.printerPostWarmup = printerPostWarmup;
            }

            #endregion
        }

        #endregion


        #region Variables (private)

        /// <summary>
        /// Number of Jobs to run in Steady State
        /// </summary>
        private int totalJobs = 10000;

        /// <summary>
        /// Number of Jobs to run in Warmup
        /// </summary>
        private int warmupJobs = 1000;

        /// <summary>
        /// Number of times to run the simulation
        /// </summary>
        private int trials = 30;

        /// <summary>
        /// Size of printer queue
        /// </summary>
        private const int MAX_PRINTER_JOBS = 10;

        /// <summary>
        /// Min priority queue using heap to organize Jobs based on next event time
        /// </summary>
        private PriorityQueue<double, Job> jobQueue;

        /// <summary>
        /// Simulation clock variable for incrementing
        /// </summary>
        private double simClock = 0;

        /// <summary>
        /// Struct to hold all values once warmup time reached
        /// </summary>
        private WarmUpValues warmUpValues;

        /// <summary>
        /// How many jobs have gone through the system, but not necessarily through every component
        /// </summary>
        private int completedJobs = 0;

        /// <summary>
        /// Used to calculate W
        /// </summary>
        private double totalJobTime = 0;

        /// <summary>
        /// Jobs that have completed all system components
        /// </summary>
        private int jobsDone = 0;

        /// <summary>
        /// Helps determine if job should go straight from NeXT to exit; size of printer queue
        /// </summary>
        private int printerJobs = 0;

        /// <summary>
        /// Used to calculate L
        /// </summary>
        private double totalJobsInSystemArea = 0;

        /// <summary>
        /// Used to calculate L
        /// </summary>
        private int prevJobsInSystem = 0;

        /// <summary>
        /// Used to calculate L
        /// </summary>
        private double prevJobsTime = 0;

        /// <summary>
        /// Timer to trigger next event during animation
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// Timer for event progress bar
        /// </summary>
        private DispatcherTimer timerProg;

        /// <summary>
        /// Number of milliseconds in a second; used for animation
        /// </summary>
        private const int MILLI_PER_SEC = 1000;

        /// <summary>
        /// Whether UI should be updated due to animation
        /// </summary>
        private bool animating = true;

        /// <summary>
        /// If the system has been initialized alread
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// Keep track of number of jobs in system
        /// </summary>
        private int jobsInSystem = 0;

        /// <summary>
        /// Each trial is stored in here
        /// </summary>
        private ObservableCollection<Output> outputs;

        #endregion


        #region Properties (public)

        public double AnimSpeed
        {
            get
            {
                if (SystemSwitch != null && !SystemSwitch.IsOn)
                {
                    return 0;
                }
                else if (SpeedSlider != null)
                {
                    return ConvertSliderVal();
                }
                return 1;
            }
        }

        public static int JobNumber { get; set; }

        public double SimClock
        {
            get 
            { 
                return simClock; 
            }
            set
            {
                simClock = value;
                if (animating)
                {
                    OnPropertyChanged("SimClock");
                }
            }
        }

        public bool TrialCompleted
        {
            get
            {
                return completedJobs >= (warmupJobs + totalJobs);
            }
        }

        #endregion


        #region Constructors
        
        /// <summary>
        /// Sets the data context for this ViewModel
        /// </summary>
        public SimulationPage()
        {
            this.InitializeComponent(); 
            
            outputs = new ObservableCollection<Output>();

            warmUpValues = new WarmUpValues();
            warmUpValues.Init();

            ResetTimers();

            // First random number seed produces bad results, so prime it before app starts
            PseudoRandomGenerator.RandomNumberGenerator();
            
            this.DataContext = this;
        }

        #endregion


        #region Event handlers

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
            if (navigationParameter is Input)
            {
                totalJobs = (navigationParameter as Input).TotalJobs;
                warmupJobs = (navigationParameter as Input).WarmupJobs;
                trials = (navigationParameter as Input).Trials;
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

        /// <summary>
        /// Adjust animation timescale based on slider input
        /// </summary>
        /// <param name="sender">Animation speed slider</param>
        /// <param name="e"></param>
        private void SpeedSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (SpeedSlider != null && AnimSpeed != 0)
            {
                timer.Interval = TimeSpan.FromMilliseconds(MILLI_PER_SEC / AnimSpeed);
                timerProg.Interval = TimeSpan.FromMilliseconds(timer.Interval.TotalMilliseconds / 100);
            }
        }

        /// <summary>
        /// Tick method; every tick fires an simulation event animatino
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, object e)
        {
            if (animating)
            {
                if (!TrialCompleted)
                {
                    if (AnimSpeed != 0)
                    {
                        ComputeSimulationEvent();
                        EventProgressBar.Value = 0;
                    }
                }
                else
                {
                    timer.Stop();
                    initialized = false;
                    SimInitBtn.IsEnabled = true;
                }
            }
            else
            {
                timer.Stop();
            }
        }

        /// <summary>
        /// Event progress bar tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerProg_Tick(object sender, object e)
        {
            if (timerProg.Interval.TotalMilliseconds > 0)
            {
                EventProgressBar.Value += (EventProgressBar.Maximum / timerProg.Interval.TotalMilliseconds);
            }
            else
            {
                EventProgressBar.Value = 100;
            }

            if (!timer.IsEnabled)
            {
                timerProg.Stop();
            }
        }

        /// <summary>
        /// Starts the animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimInitBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            animating = true;
            InitializeSimulation();
            StartTimers();
            SystemSwitch.IsOn = true;
            SimInitBtn.IsEnabled = false;
        }

        /// <summary>
        /// Completes a single trial
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FullSimBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            JobIcon.Visibility = Visibility.Collapsed;
            animating = false;
            ProgressBar.IsIndeterminate = true;

            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync( Windows.UI.Core.CoreDispatcherPriority.Normal, () => { ComputeSimulation(); } );

            // Update the UI with results
            ProgressBar.IsIndeterminate = false;

            double clockDiff = SimClock - warmUpValues.SimClockPostWarmup;

            Output curOutput = new Output(
                1 - ((Mac.Data.TotalTimeIdle - warmUpValues.MacPostWarmup) / clockDiff),
                1 - ((NeXT.Data.TotalTimeIdle - warmUpValues.NeXTPostWarmup) / clockDiff),
                1 - ((Printer.Data.TotalTimeIdle - warmUpValues.PrinterPostWarmup) / clockDiff),
                totalJobTime / totalJobs,
                totalJobsInSystemArea / clockDiff
                );

            RandBox.Text =
                    "Mac util time: " + curOutput.MacUtil
                + "\nNeXT util time: " + curOutput.NextUtil
                + "\nPrinter util time: " + curOutput.PrinterUtil
                + "\nW Avg time per job: " + curOutput.W + " for " + jobsDone + " jobs"
                + "\nL Avg jobs in system: " + curOutput.L;

            outputs.Add(curOutput);

            SimInitBtn.IsEnabled = true;
        }

        /// <summary>
        /// Completes the entire set of trials.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RunTrialsBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            while (outputs.Count < trials)
            {
                JobIcon.Visibility = Visibility.Collapsed;
                animating = false;
                RunTrialsProgressBar.IsIndeterminate = true;

                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { ComputeSimulation(); });


                double clockDiff = SimClock - warmUpValues.SimClockPostWarmup;

                Output curOutput = new Output(
                    1 - ((Mac.Data.TotalTimeIdle - warmUpValues.MacPostWarmup) / clockDiff),
                    1 - ((NeXT.Data.TotalTimeIdle - warmUpValues.NeXTPostWarmup) / clockDiff),
                    1 - ((Printer.Data.TotalTimeIdle - warmUpValues.PrinterPostWarmup) / clockDiff),
                    totalJobTime / totalJobs,
                    totalJobsInSystemArea / clockDiff
                    );

                RandBox.Text =
                        "Mac util time: " + curOutput.MacUtil
                    + "\nNeXT util time: " + curOutput.NextUtil
                    + "\nPrinter util time: " + curOutput.PrinterUtil
                    + "\nW Avg time per job: " + curOutput.W + " for " + jobsDone + " jobs"
                    + "\nL Avg jobs in system: " + curOutput.L;

                outputs.Add(curOutput);
            }

            // Update the UI with results
            RunTrialsProgressBar.IsIndeterminate = false;

            SimInitBtn.IsEnabled = true;

            this.Frame.Navigate(typeof(OutputPage), outputs);
        }

        #endregion


        #region Methods

        /// <summary>
        /// Resets all animation timers.
        /// </summary>
        private void ResetTimers()
        {
            timer = new DispatcherTimer();
            timerProg = new DispatcherTimer();
        }

        /// <summary>
        /// Starts animation timers with proper intervals based on slider.
        /// </summary>
        private void StartTimers()
        {
            timer.Interval = TimeSpan.FromMilliseconds(MILLI_PER_SEC / ConvertSliderVal());
            timer.Tick += timer_Tick;
            timer.Start();

            timerProg.Interval = TimeSpan.FromMilliseconds(timer.Interval.TotalMilliseconds / 100);
            timerProg.Tick += timerProg_Tick;
            timerProg.Start();
        }

        /// <summary>
        /// Initializes the simulation and sets all variables to their start state
        /// TODO: find a better way to do this; probably just create a new data object once Model is made
        /// </summary>
        private void InitializeSimulation()
        {
            JobIcon.Opacity = 1;
            JobIcon.Visibility = Visibility.Visible;
            SimClock = 0;
            completedJobs = 0;
            jobQueue = new PriorityQueue<double, Job>();
            jobsDone = 0;
            warmUpValues.Set(false, 0, 0, 0, 0);
            totalJobTime = 0;
            printerJobs = 0;
            totalJobsInSystemArea = 0;
            prevJobsInSystem = 0;
            prevJobsTime = 0;
            jobsInSystem = 0;
            Mac.Data.TotalTimeIdle = 0;
            Mac.Data.TimeIdleAgain = 0;
            Mac.Data.JobQueue.Clear();
            Mac.Data.NumJobs = 0;
            NeXT.Data.TotalTimeIdle = 0;
            NeXT.Data.TimeIdleAgain = 0;
            NeXT.Data.JobQueue.Clear();
            NeXT.Data.NumJobs = 0;
            Printer.Data.TotalTimeIdle = 0;
            Printer.Data.TimeIdleAgain = 0;
            Printer.Data.JobQueue.Clear();
            Printer.Data.NumJobs = 0;
            ExitSystem.Data.JobQueue.Clear();
            ExitSystem.Data.CompletedJobs = 0;
            ResetTimers();

            // Initializations
            Job uG1Job = UserGroup1.Data.GenerateArrival(SimClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG1Job.ArrivalTime, uG1Job));
            Job uG2Job = UserGroup2.Data.GenerateArrival(SimClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG2Job.ArrivalTime, uG2Job));
            Job uG3Job = UserGroup3.Data.GenerateArrival(SimClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG3Job.ArrivalTime, uG3Job));

            initialized = true;
        }

        /// <summary>
        /// Threaded method to complete a single event in the simulation trial.
        /// Handles both animated and non-animated events.
        /// </summary>
        private async void ComputeSimulationEvent()
        {
            // Pop next event
            KeyValuePair<double, Job> curJob = jobQueue.Dequeue();

            // Advance simulation clock
            SimClock = curJob.Value.ArrivalTime;

            // Set necessary warmup variables once steady state reach
            if (completedJobs == warmupJobs && !warmUpValues.SetPostWarmup)
            {
                warmUpValues.Set(true, SimClock, Mac.Data.TotalTimeIdle, NeXT.Data.TotalTimeIdle, Printer.Data.TotalTimeIdle);

                prevJobsInSystem = jobQueue.Count;
                prevJobsTime = SimClock;
            }

            // Execute it and add new events back to eventQueue
            switch (curJob.Value.EventType)
            {
                case Job.EventTypes.UG_FINISH:
                    if (animating)
                    {
                        Storyboard userGroupScale = (curJob.Value.LocationInSystem as UserGroupData).View.FindName("Triggered") as Storyboard;
                        userGroupScale.SpeedRatio = AnimSpeed;
                        userGroupScale.Begin();

                        // Set up animation
                        Point userGroupPos = (curJob.Value.LocationInSystem as UserGroupData).View.TransformToVisual(Window.Current.Content).TransformPoint(new Point(0, 0));
                        Point macPos = Mac.TransformToVisual(Window.Current.Content).TransformPoint(new Point(0, 0));
                        Storyboard createJob = AnimateJobCreation(userGroupPos, macPos, (curJob.Value.LocationInSystem as UserGroupData).GroupColor);
                        createJob.SpeedRatio = AnimSpeed;
                        // Execute and wait for thread
                        await createJob.BeginAsync();
                    }

                    // Spawn new UserGroup event 
                    Job newUGJob = (curJob.Value.LocationInSystem as UserGroupData).GenerateArrival(SimClock);
                    jobQueue.Add(new KeyValuePair<double, Job>(newUGJob.ArrivalTime, newUGJob));

                    // Send the current job down the system
                    jobQueue.Add(SystemComponentJobProcess(Mac.Data, curJob.Value, true));

                    if (animating)
                    {
                        Mac.Data.JobQueue.Add(curJob.Value);
                        Mac.Data.NumJobs = Mac.Data.JobQueue.Count;
                    }

                    break;
                case Job.EventTypes.MAC_FINISH:
                    if (animating)
                    {
                        Storyboard triggered = Mac.FindName("Triggered") as Storyboard;
                        triggered.SpeedRatio = AnimSpeed;
                        triggered.Begin();
                        Mac.Data.JobQueue.Remove(curJob.Value);
                        Mac.Data.NumJobs = Mac.Data.JobQueue.Count;
                    }

                    curJob = SystemComponentJobProcess(NeXT.Data, curJob.Value, false);
                    jobQueue.Add(curJob);

                    // Contributes to computation of L (average number of jobs in the whole system) once Job enters first component of systems
                    UpdateJobsArea();

                    if (animating)
                    {
                        NeXT.Data.JobQueue.Add(curJob.Value);
                        NeXT.Data.NumJobs = NeXT.Data.JobQueue.Count;
                    }

                    break;
                case Job.EventTypes.NEXT_FINISH:
                    if (animating)
                    {
                        Storyboard triggered2 = NeXT.FindName("Triggered") as Storyboard;
                        triggered2.SpeedRatio = AnimSpeed;
                        triggered2.Begin();
                        NeXT.Data.JobQueue.Remove(curJob.Value);
                        NeXT.Data.NumJobs = NeXT.Data.JobQueue.Count;
                    }

                    if (printerJobs < MAX_PRINTER_JOBS)
                    {
                        // Send job to printer if queue isn't full
                        curJob = SystemComponentJobProcess(Printer.Data, curJob.Value, false);
                        jobQueue.Add(curJob);
                        printerJobs++;

                        if (animating)
                        {
                            Printer.Data.JobQueue.Add(curJob.Value);
                            Printer.Data.NumJobs = Printer.Data.JobQueue.Count;
                        }
                    }
                    else
                    {
                        // Otherwise, just send it out of the system
                        JobExitSystem(curJob, false);

                        if (animating)
                        {
                            ExitSystem.Data.JobQueue.Add(curJob.Value);
                            ExitSystem.Data.CompletedJobs = ExitSystem.Data.JobQueue.Count;
                        }
                    }

                    break;
                case Job.EventTypes.LASERJET_FINISH:
                    if (animating)
                    {
                        Storyboard triggered3 = Printer.FindName("Triggered") as Storyboard;
                        triggered3.SpeedRatio = AnimSpeed;
                        triggered3.Begin();
                        Printer.Data.JobQueue.Remove(curJob.Value);
                        Printer.Data.NumJobs = Printer.Data.JobQueue.Count;
                    }

                    JobExitSystem(curJob, true);
                    printerJobs--;

                    if (animating)
                    {
                        ExitSystem.Data.JobQueue.Add(curJob.Value);
                        ExitSystem.Data.CompletedJobs = ExitSystem.Data.JobQueue.Count;
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Runs the simulation until the current trial is completed
        /// </summary>
        private void ComputeSimulation()
        {
            if (!initialized)
            {
                InitializeSimulation();
            }

            while (!TrialCompleted)
            {                
                ComputeSimulationEvent();
            }

            initialized = false;
        }

        /// <summary>
        /// Updates the values used to calculate L
        /// </summary>
        private void UpdateJobsArea()
        {
            if (completedJobs > warmupJobs)
            {
                double area = prevJobsInSystem * (SimClock - prevJobsTime);
                totalJobsInSystemArea += area;
                
                prevJobsInSystem = jobsInSystem;
                prevJobsTime = SimClock;
            }
        }

        /// <summary>
        /// Called once a Job is ready to exit the system.
        /// Updates variables used to determine L (average number of jobs in system).
        /// Determines how long job spent in system to determine W.
        /// </summary>
        /// <param name="curJob">Job being processed</param>
        /// <param name="includeInMetrics">Those jobs finding 10 or more at the printer will be excluded in the calculation of W.</param>
        private void JobExitSystem(KeyValuePair<double, Job> curJob, bool includeInMetrics)
        {
            completedJobs++;
            jobsInSystem--;

            if (completedJobs > warmupJobs && includeInMetrics)
            {
                curJob.Value.SystemExitTime = SimClock;

                totalJobTime += (curJob.Value.SystemExitTime - curJob.Value.SystemEntryTime);

                jobsDone++;
            }

            // Contributes to computation of L (average number of jobs in the whole system)
            UpdateJobsArea();
        }

        /// <summary>
        /// Called when a system component is processing a Job.
        /// </summary>
        /// <param name="component">Next component to process job</param>
        /// <param name="job">Job being processed</param>
        /// <param name="isMac">If it's the Mac, set SystemEntryTime and increment number of jobs in system for W and L</param>
        /// <returns></returns>
        private KeyValuePair<double, Job> SystemComponentJobProcess(SystemComponentData component, Job job, bool isMac)
        {
            if (isMac)
            {
                job.SystemEntryTime = SimClock;
                jobsInSystem++;
            }

            // BUSY
            if (SimClock < component.TimeIdleAgain)
            {
                job.ArrivalTime = component.TimeIdleAgain + PseudoRandomGenerator.ExponentialRVG(component.ProcessMean);
            }
            // IDLE
            else
            {
                component.TotalTimeIdle += (SimClock - component.TimeIdleAgain);
                job.ArrivalTime += PseudoRandomGenerator.ExponentialRVG(component.ProcessMean);
            }

            component.TimeIdleAgain = job.ArrivalTime;
            job.LocationInSystem = component;

            return new KeyValuePair<double,Job>(job.ArrivalTime, job);
        }

        /// <summary>
        /// Helper method to create animation storyboards to animate jobs firing from
        /// UserGroups to the Mac.
        /// </summary>
        /// <param name="from">Point to fire from</param>
        /// <param name="to">Point to fire at</param>
        /// <param name="color">Color of object to fire</param>
        /// <returns>Storyboard of requested animation</returns>
        private Storyboard AnimateJobCreation(Point from, Point to, Color color)
        {            
            // Initialize a new instance of the CompositeTransform which allows you 
            // apply multiple different transforms to the animated object
            this.JobIcon.Fill = new SolidColorBrush(color);
            this.JobIcon.RenderTransform = new CompositeTransform();

            // Create the timelines
            DoubleAnimationUsingKeyFrames animationX = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames animationY = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames opacity = new DoubleAnimationUsingKeyFrames();

            // Set up Easing functions
            ExponentialEase easingFunction = new ExponentialEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;

            // Add key frames to the timeline
            animationX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = from.X });
            animationX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(MILLI_PER_SEC / 2), Value = to.X, EasingFunction = easingFunction });
            animationY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = from.Y });
            animationY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(MILLI_PER_SEC / 2), Value = to.Y, EasingFunction = easingFunction });
            opacity.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 1 });
            opacity.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds((MILLI_PER_SEC / 2) - (MILLI_PER_SEC / 5)), Value = 1 });
            opacity.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(MILLI_PER_SEC / 2), Value = 0, EasingFunction = easingFunction });

            // Notice the first parameter takes a timeline object not the storyboard itself
            Storyboard.SetTargetProperty(animationX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            Storyboard.SetTargetProperty(animationY, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            Storyboard.SetTargetProperty(opacity, "(UIElement.Opacity)");
            Storyboard.SetTarget(animationX, JobIcon);
            Storyboard.SetTarget(animationY, JobIcon);
            Storyboard.SetTarget(opacity, JobIcon);

            // Create the storyboards
            Storyboard storyboard = new Storyboard() { };
            // Add the timelines to your storyboard
            storyboard.Children.Add(animationX);
            storyboard.Children.Add(animationY);
            storyboard.Children.Add(opacity);

            return storyboard;
        }

        /// <summary>
        /// Converts slider value to fraction instead of using negative numbers
        /// </summary>
        /// <returns>Fractional representation instead of negative representation</returns>
        private double ConvertSliderVal()
        {
            double rounded = Math.Round(SpeedSlider.Value);

            if (SpeedSlider.Value < 0)
            {
                rounded = 1 / Math.Abs(rounded);
            }
            return rounded;
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
