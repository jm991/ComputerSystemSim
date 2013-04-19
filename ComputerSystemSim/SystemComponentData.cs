using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSystemSim
{
    public class SystemComponentData : INotifyPropertyChanged, Updatable
    {
        #region Variables (private)

        private SystemComponent view;

        private SortedObservableCollection<Job> jobQueue;

        private State curState = State.IDLE;

        private Updatable goal;

        private double curProcessCooldown = 0;

        private Job curJob;

        private double timeIdleAgain = 0;

        private double totalTimeIdle = 0;

        private Job.EventTypes eventType;

        #endregion


        #region Properties (public)

        public string Name
        {
            get
            {
                return view.ComponentName;
            }
        }

        public Job.EventTypes EventType
        {
            get { return view.EventType; }
            set { view.EventType = value; }
        }

        public double TotalTimeIdle
        {
            get { return totalTimeIdle; }
            set { totalTimeIdle = value; }
        }

        public SystemComponent View
        {
            get { return view; }
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

        public SystemComponentData(SystemComponent view)
        {
            JobQueue = new SortedObservableCollection<Job>(i => i.ArrivalTime);

            this.view = view;
        }

        #endregion


        #region Methods

        public Job PassCurJob()
        {
            curJob.ArrivalTime = MainPage.SimClockTicksStatic;

            return curJob;
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
                        if (goal is SystemComponentData)
                        {
                            if ((goal as SystemComponentData).JobQueue.Count < 9)
                            {
                                (goal as SystemComponentData).JobQueue.Add(PassCurJob());
                            }
                            else
                            {
                                ((goal as SystemComponentData).Goal as ExitNodeData).JobQueue.Add(PassCurJob());
                            }
                        }
                        else if (goal is ExitNodeData)
                        {
                            (goal as ExitNodeData).JobQueue.Add(PassCurJob());
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
                curJob = JobQueue[0];
                view.SetCurJobViewer(curJob);

                JobQueue.RemoveAt(0);

                curState = State.IN_USE;

                CurProcessCooldown = PseudoRandomGenerator.ExponentialRVG(view.ProcessMean);
            }
            else
            {
                curState = State.IDLE;
                view.SetCurJobViewer(null);
                CurProcessCooldown = 0;
            }
        }

        #endregion
    }
}
