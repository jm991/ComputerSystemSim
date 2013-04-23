using System;
using System.Collections.Generic;
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
        private const int MILLI_PER_SEC = 1000;

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

        private DispatcherTimer timer;
        private DispatcherTimer timerProg;
        private bool animating = true;
        private bool initialized = false;

        // TODO: move animation onto the same system as the full simulation
        private static int simClockTicks = 0;

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

        public bool TrialCompleted
        {
            get
            {
                return completedJobs >= (WARMUP_JOBS + TOTAL_JOBS);
            }
        }

        #endregion


        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();

            warmUpValues = new WarmUpValues();
            warmUpValues.Init();

            ResetTimers();

            // First random number seed produces bad results, so prime it before app starts
            PseudoRandomGenerator.RandomNumberGenerator();
            
            // Should separate the data out into its own object type
            this.DataContext = this;
        }

        #endregion

        #region Event handlers

        private void TextBox_TextChanged_1(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox senderBox = sender as TextBox;

                senderBox.Text = Regex.Replace(senderBox.Text, @"[^0-9]*", string.Empty, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            }
        }

        private void SpeedSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (SpeedSlider != null && AnimSpeed != 0)
            {
                timer.Interval = TimeSpan.FromMilliseconds(MILLI_PER_SEC / AnimSpeed);
                timerProg.Interval = TimeSpan.FromMilliseconds(timer.Interval.TotalMilliseconds / 100);
            }
        }

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

        private void SimInitBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            animating = true;
            InitializeSimulation();
            StartTimers();
            SystemSwitch.IsOn = true;
            SimInitBtn.IsEnabled = false;
        }

        private async void FullSimBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            animating = false;
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

            SimInitBtn.IsEnabled = true;
        }

        #endregion


        #region Methods

        private void InitializeSimulation()
        {
            simClock = 0;
            completedJobs = 0;
            jobQueue = new PriorityQueue<double, Job>();
            jobsDone = 0;
            warmUpValues.Set(false, 0, 0, 0, 0);
            totalJobTime = 0;
            printerJobs = 0;
            totalJobsInSystemArea = 0;
            prevJobsInSystemArea = 0;
            prevJobsTime = 0;
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
            Job uG1Job = UserGroup1.Data.GenerateArrival(simClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG1Job.ArrivalTime, uG1Job));
            Job uG2Job = UserGroup2.Data.GenerateArrival(simClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG2Job.ArrivalTime, uG2Job));
            Job uG3Job = UserGroup3.Data.GenerateArrival(simClock);
            jobQueue.Add(new KeyValuePair<double, Job>(uG3Job.ArrivalTime, uG3Job));

            initialized = true;
        }

        private void ResetTimers()
        {
            timer = new DispatcherTimer();
            timerProg = new DispatcherTimer();
        }

        private void StartTimers()
        {
            timer.Interval = TimeSpan.FromMilliseconds(MILLI_PER_SEC / ConvertSliderVal());
            timer.Tick += timer_Tick;
            timer.Start();

            timerProg.Interval = TimeSpan.FromMilliseconds(timer.Interval.TotalMilliseconds / 100);
            timerProg.Tick += timerProg_Tick;
            timerProg.Start();
        }

        private async void ComputeSimulationEvent()
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
                        await createJob.BeginAsync();
                        // createJob.Begin();
                    }
                    // Spawn new UserGroup event 
                    Job newUGJob = (curJob.Value.LocationInSystem as UserGroupData).GenerateArrival(simClock);
                    jobQueue.Add(new KeyValuePair<double, Job>(newUGJob.ArrivalTime, newUGJob));

                    // Contributes to computation of L (average number of jobs in the whole system)
                    UpdateJobsArea();

                    // Send the current job down the system
                    jobQueue.Add(SystemComponentJobProcess(Mac.Data, curJob.Value));
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
                        //await triggered.BeginAsync();
                    }
                    
                    curJob = SystemComponentJobProcess(NeXT.Data, curJob.Value);
                    jobQueue.Add(curJob);

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
                        //await triggered2.BeginAsync();
                    }

                    if (printerJobs < MAX_PRINTER_JOBS)
                    {
                        curJob = SystemComponentJobProcess(Printer.Data, curJob.Value);
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
                        //await triggered3.BeginAsync();
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
            job.LocationInSystem = component;

            return new KeyValuePair<double,Job>(job.ArrivalTime, job);
        }

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
            opacity.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 100 });
            opacity.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds((MILLI_PER_SEC / 2) - (MILLI_PER_SEC / 10)), Value = 100 });
            opacity.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(MILLI_PER_SEC / 2), Value = 0, EasingFunction = easingFunction });

            // Notice the first parameter takes a timeline object not the storyboard itself
            Storyboard.SetTargetProperty(animationX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            Storyboard.SetTargetProperty(animationY, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            Storyboard.SetTargetProperty(opacity, "UIElement.Opacity");
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
    }
}
