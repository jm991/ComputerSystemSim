using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSystemSim
{
    /// <summary>
    /// Model data for ExitNode View.
    /// </summary>
    public class ExitNodeData : INotifyPropertyChanged, Updatable
    {
        #region Variables (private)

        /// <summary>
        /// Jobs that have exited the system
        /// </summary>
        private SortedObservableCollection<Job> jobQueue;

        #endregion


        #region Properties

        public Uri IconSource
        {
            get { return new Uri("Exit.png"); }
        }

        public int CompletedJobs
        {
            get { return JobQueue.Count; }
            set
            {
                OnPropertyChanged("CompletedJobs");
            }
        }

        public SortedObservableCollection<Job> JobQueue
        {
            get { return jobQueue; }
            set
            {
                jobQueue = value;
                OnPropertyChanged("EventQueue");
            }
        }

        public string Name
        {
            get
            {
                return "Exit";
            }
        }

        public Job.EventTypes EventType
        {
            get { return Job.EventTypes.EXIT; }
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
        /// Sets the lambda for the ObservableCollection to sort based on Job.ArrivalTime.
        /// </summary>
        public ExitNodeData()
        {
            JobQueue = new SortedObservableCollection<Job>(i => i.ArrivalTime);
        }

        #endregion


        #region Methods

        #endregion
    }
}
