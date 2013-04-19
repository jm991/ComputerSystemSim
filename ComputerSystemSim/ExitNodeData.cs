using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSystemSim
{
    public class ExitNodeData : INotifyPropertyChanged, Updatable
    {
        #region Variables (private)

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

        public ExitNodeData()
        {
            JobQueue = new SortedObservableCollection<Job>(i => i.ArrivalTime);
        }

        #endregion


        #region Methods

        public void Update()
        {
            CompletedJobs = jobQueue.Count;
        }

        #endregion
    }
}
