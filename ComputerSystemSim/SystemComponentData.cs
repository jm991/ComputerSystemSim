﻿using System;
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

        public SystemComponentData()
        {
            JobQueue = new SortedObservableCollection<Job>(i => i.ArrivalTime);
        }

        #endregion


        #region Methods

        #endregion
    }
}
