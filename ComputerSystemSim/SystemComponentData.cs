using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ComputerSystemSim
{
    /// <summary>
    /// Model data for the SystemComponent View.
    /// </summary>
    public class SystemComponentData : INotifyPropertyChanged, Updatable
    {
        #region Variables (private)

        /// <summary>
        /// Jobs in the Component's queue; for UI only
        /// </summary>
        private SortedObservableCollection<Job> jobQueue;

        /// <summary>
        /// Cooldown based on sim clock
        /// </summary>
        private double curProcessCooldown = 0;

        /// <summary>
        /// Next item in system that this component feeds to
        /// </summary>
        private Updatable goal;

        /// <summary>
        /// Next time this component will be idle
        /// </summary>
        private double timeIdleAgain = 0;

        /// <summary>
        /// Time spent idle during simulation
        /// </summary>
        private double totalTimeIdle = 0;

        /// <summary>
        /// Process mean used in random number generation
        /// </summary>
        private double processMean = 0;

        /// <summary>
        /// Image of the component
        /// </summary>
        private Uri iconSource;

        /// <summary>
        /// Name of component
        /// </summary>
        private string name;

        /// <summary>
        /// Activity is based on what EventType is handled by this component
        /// </summary>
        private Job.EventTypes eventType;

        #endregion


        #region Properties (public)

        public double ProcessMean
        {
            get { return processMean; }
            set
            {
                processMean = value;
                OnPropertyChanged("ProcessMean");
            }
        }

        public Uri IconSource { get { return iconSource; } set { iconSource = value; } }

        public string Name { get { return name; } set { name = value; } }

        public Job.EventTypes EventType
        {
            get { return eventType; }
            set { eventType = value; }
        }

        public double TotalTimeIdle
        {
            get { return totalTimeIdle; }
            set { totalTimeIdle = value; }
        }

        public double TimeIdleAgain
        {
            get { return timeIdleAgain; }
            set { timeIdleAgain = value; }
        }

        public Updatable Goal
        {
            get { return goal; }
            set { goal = value; }
        }

        public double CurProcessCooldown
        {
            get { return curProcessCooldown; }
            set
            {
                curProcessCooldown = value;
                OnPropertyChanged("CurProcessCooldown");
            }
        }

        public SortedObservableCollection<Job> JobQueue
        {
            get
            {
                return jobQueue;
            }
            set
            {
                jobQueue = value;
                OnPropertyChanged("JobQueue");
            }
        }

        public int NumJobs
        {
            get { return JobQueue.Count; }
            set
            {
                OnPropertyChanged("NumJobs");
            }
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


        #region Constructors

        /// <summary>
        /// Instantiates a new Model
        /// </summary>
        public SystemComponentData()
        {
            JobQueue = new SortedObservableCollection<Job>(i => i.ArrivalTime);
        }

        #endregion


        #region Methods

        #endregion
    }
}
