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
    public class SystemComponentData : INotifyPropertyChanged, Updatable
    {
        #region Variables (private)

        private SortedObservableCollection<Job> jobQueue;

        private State curState = State.IDLE;

        private double curProcessCooldown = 0;

        private Job curJob;

        private Updatable goal;

        private double timeIdleAgain = 0;

        private double totalTimeIdle = 0;

        private double processMean = 0;

        private Uri iconSource;

        private string name;

        private Job.EventTypes eventType;

        #endregion


        #region Properties (public)

        public double ProcessMean { get { return processMean; } set { processMean = value; } }

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

        public State CurState
        {
            get { return curState; }
            set { curState = value; }
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

        public Job CurJob
        {
            get { return curJob; }
            set 
            {
                curJob = value;
                OnPropertyChanged("CurJob");
            }
        }

        public enum State
        {
            IDLE,
            IN_USE
        };

        public SortedObservableCollection<Job> JobQueue
        {
            get { return jobQueue; }
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

        public SystemComponentData()
        {
            JobQueue = new SortedObservableCollection<Job>(i => i.ArrivalTime);
        }

        #endregion


        #region Methods

        private Job PassCurJob()
        {
            CurJob.ArrivalTime = MainPage.SimClockTicksStatic;

            return CurJob;
        }

        public void Update()
        {
            switch (curState)
            {
                case State.IDLE:
                    LoadNextJob();
                    break;
                case State.IN_USE:
                    CurProcessCooldown -= 1;

                    if (CurProcessCooldown <= 0)
                    {
                        // TODO: make something better for organizing these classes under a subclass
                        // TODO: probably a bug that this doesn't point to the printer
                        if (Goal is SystemComponentData)
                        {
                            if ((Goal as SystemComponentData).JobQueue.Count < 9)
                            {
                                (Goal as SystemComponentData).JobQueue.Add(PassCurJob());
                            }
                            else
                            {
                                ((Goal as SystemComponentData).Goal as ExitNodeData).JobQueue.Add(PassCurJob());
                            }
                        }
                        else if (Goal is ExitNodeData)
                        {
                            (Goal as ExitNodeData).JobQueue.Add(PassCurJob());
                        }

                        LoadNextJob();
                    }
                    break;
                default:
                    break;
            }

            NumJobs = jobQueue.Count;
        }

        private void LoadNextJob()
        {
            if (JobQueue.Count > 0)
            {
                CurJob = JobQueue[0];

                JobQueue.RemoveAt(0);

                curState = State.IN_USE;

                CurProcessCooldown = PseudoRandomGenerator.ExponentialRVG(ProcessMean);
            }
            else
            {
                curState = State.IDLE;
                CurJob = null;
                CurProcessCooldown = 0;
            }
        }

        #endregion
    }
}
