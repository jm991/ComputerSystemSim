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
        private const int MILLI_PER_SEC = 1000;

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
                    double rounded = Math.Round(SpeedSlider.Value);

                    if (SpeedSlider.Value < 0)
                    {
                        rounded = 1 / Math.Abs(rounded);
                    }

                    return rounded;
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

        #endregion


        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();

            warmUpValues = new WarmUpValues();
            warmUpValues.Init();

            timer = new DispatcherTimer();
            timerProg = new DispatcherTimer();

            // First random number seed produces bad results, so prime it before app starts
            PseudoRandomGenerator.RandomNumberGenerator();
            
            // Should separate the data out into its own object type
            this.DataContext = this;
        }

        #endregion

        #region Event handlers

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
            ComputeSimulationEvent();
            EventProgressBar.Value = 0;

            if (completedJobs >= (WARMUP_JOBS + TOTAL_JOBS))
            {
                timer.Stop();
            }
        }

        private void timerProg_Tick(object sender, object e)
        {
            if (timerProg.Interval.TotalMilliseconds > 0)
            {
                EventProgressBar.Value += (100 / timerProg.Interval.TotalMilliseconds);
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

        private void RandBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RandBox.Text = "rand: " + PseudoRandomGenerator.RandomNumberGenerator() + "\nexp: " + PseudoRandomGenerator.ExponentialRVG(3200);
        }

        private void SimInitBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            InitializeSimulation();
            StartTimer();
            SystemSwitch.IsOn = true;
        }

        private void SimEventBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ComputeSimulationEvent();
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

        private void InitializeSimulation()
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
        }

        private async void ComputeSimulationEvent()
        {
            if (completedJobs < (WARMUP_JOBS + TOTAL_JOBS) && AnimSpeed != 0)
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

                        // Spawn new UserGroup event 
                        Job newUGJob = (curJob.Value.LocationInSystem as UserGroupData).GenerateArrival(simClock);
                        jobQueue.Add(new KeyValuePair<double, Job>(newUGJob.ArrivalTime, newUGJob));

                        // Contributes to computation of L (average number of jobs in the whole system)
                        UpdateJobsArea();

                        // Send the current job down the system
                        jobQueue.Add(SystemComponentJobProcess(Mac.Data, curJob.Value));
                        Mac.Data.JobQueue.Add(curJob.Value);
                        Mac.Data.NumJobs = Mac.Data.JobQueue.Count;

                        break;
                    case Job.EventTypes.MAC_FINISH:
                        Storyboard triggered = Mac.FindName("Triggered") as Storyboard;
                        triggered.SpeedRatio = AnimSpeed;
                        triggered.Begin();
                        Mac.Data.JobQueue.Remove(curJob.Value);
                        Mac.Data.NumJobs = Mac.Data.JobQueue.Count;
                        //await triggered.BeginAsync();

                        jobQueue.Add(SystemComponentJobProcess(NeXT.Data, curJob.Value));
                        NeXT.Data.JobQueue.Add(curJob.Value);
                        NeXT.Data.NumJobs = NeXT.Data.JobQueue.Count;

                        break;
                    case Job.EventTypes.NEXT_FINISH:
                        Storyboard triggered2 = NeXT.FindName("Triggered") as Storyboard;
                        triggered2.SpeedRatio = AnimSpeed;
                        triggered2.Begin();
                        NeXT.Data.JobQueue.Remove(curJob.Value);
                        NeXT.Data.NumJobs = NeXT.Data.JobQueue.Count;
                        //await triggered2.BeginAsync();

                        if (printerJobs < MAX_PRINTER_JOBS)
                        {
                            jobQueue.Add(SystemComponentJobProcess(Printer.Data, curJob.Value));
                            Printer.Data.JobQueue.Add(curJob.Value);
                            Printer.Data.NumJobs = Printer.Data.JobQueue.Count;
                            printerJobs++;
                        }
                        else
                        {
                            JobExitSystem(curJob, false);
                            ExitSystem.Data.JobQueue.Add(curJob.Value);
                            ExitSystem.Data.CompletedJobs = ExitSystem.Data.JobQueue.Count;
                        }

                        break;
                    case Job.EventTypes.LASERJET_FINISH:
                        Storyboard triggered3 = Printer.FindName("Triggered") as Storyboard;
                        triggered3.SpeedRatio = AnimSpeed;
                        triggered3.Begin();
                        Printer.Data.JobQueue.Remove(curJob.Value);
                        Printer.Data.NumJobs = Printer.Data.JobQueue.Count;
                        //await triggered3.BeginAsync();

                        JobExitSystem(curJob, true);
                        ExitSystem.Data.JobQueue.Add(curJob.Value);
                        ExitSystem.Data.CompletedJobs = ExitSystem.Data.JobQueue.Count;
                        printerJobs--;

                        break;
                    default:
                        break;
                }
            }
        }

        private void StartTimer()
        {
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_Tick;
            timer.Start();

            timerProg.Interval = TimeSpan.FromMilliseconds(timer.Interval.TotalMilliseconds / 100);
            timerProg.Tick += timerProg_Tick;
            timerProg.Start();
        }

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
                        Job newUGJob = (curJob.Value.LocationInSystem as UserGroupData).GenerateArrival(simClock);
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
            job.LocationInSystem = component;

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

        #endregion
    }

    public class NegativeToFractionalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                double rounded = Math.Round((double) value);

                if (rounded < 0)
                {
                    rounded = 1 / Math.Abs(rounded);
                }

                return rounded;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                double doubleVal = (double) value;

                if (doubleVal < 0)
                {
                    return 1 / doubleVal;
                }

                return doubleVal;
            }

            return 0;
        }
    } 
}
