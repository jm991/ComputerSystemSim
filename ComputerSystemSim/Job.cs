﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ComputerSystemSim
{
    public class Job : INotifyPropertyChanged
    {
        #region Variables (private)

        private string uniqueID;
        private Updatable locationInSystem;
        private Updatable creator;
        private double arrivalTime;
        private double systemEntryTime = 0;
        private double systemExitTime = 0;

        #endregion


        #region Properties and enums (public)

        public enum EventTypes
        {
            UG_FINISH,
            MAC_FINISH,
            NEXT_FINISH,
            LASERJET_FINISH,
            EXIT
        };

        public double SystemEntryTime
        {
            get { return systemEntryTime; }
            set { systemEntryTime = value; }
        }

        public double SystemExitTime
        {
            get { return systemExitTime; }
            set { systemExitTime = value; }
        }

        public EventTypes EventType
        {
            get
            {
                return locationInSystem.EventType;
            }
        }

        public Updatable LocationInSystem
        {
            get { return locationInSystem; }
            set { locationInSystem = value; }
        }

        public Updatable Creator
        {
            get { return creator; }
        }

        public string CreatorImageURI
        {
            get
            {
                if (creator != null && creator.IconSource != null)
                {
                    return creator.IconSource.OriginalString;
                }
                return "";
            }
            set
            {
                OnPropertyChanged("CreatorImageURI");
            }
        }

        public string JobTitle
        {
            get
            {
                return "Job " + uniqueID;
            }
            set
            {
                OnPropertyChanged("JobTitle");
            }
        }

        public double ArrivalTime
        {
            get 
            { 
                return arrivalTime; 
            }
            set 
            { 
                arrivalTime = value;
                OnPropertyChanged("ArrivalTime");
            }
        }

        #endregion


        #region Constructors 

        public Job(double arrivalTime, Updatable creator, double entryTime)
        {
            this.systemEntryTime = entryTime;
            this.arrivalTime = arrivalTime;
            this.locationInSystem = creator;
            this.creator = creator;
            uniqueID = GenerateId();
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


        #region Methods

        private string GenerateId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }

            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        #endregion
    }
}
