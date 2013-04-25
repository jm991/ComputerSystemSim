using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ComputerSystemSim
{
    /// <summary>
    /// Model data class that represents a Job in the system.
    /// </summary>
    public class Job : INotifyPropertyChanged
    {
        #region Variables (private)

        /// <summary>
        /// Job's identification in system; mostly for UI purposes
        /// </summary>
        private string uniqueID;

        /// <summary>
        /// Indicates which SystemComponent the Job is currently being processed by or waiting for
        /// </summary>
        private Updatable locationInSystem;

        /// <summary>
        /// UserGroup that created this Job
        /// </summary>
        private Updatable creator;

        /// <summary>
        /// Arrival time to <paramref name="locationInSystem"/>
        /// </summary>
        private double arrivalTime;

        /// <summary>
        /// Time when job enters into the Mac processing queue
        /// </summary>
        private double systemEntryTime = 0;

        /// <summary>
        /// Time that job exited system
        /// </summary>
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

        /// <summary>
        /// Instantiate a Job
        /// </summary>
        /// <param name="arrivalTime">Next time event will fire for this Job in system</param>
        /// <param name="creator">UserGroup that spawned this Job</param>
        /// <param name="entryTime">Entry time into system</param>
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

        /// <summary>
        /// Generate a unique identifier based off a static, incremented value.
        /// </summary>
        /// <returns>A unique ID number</returns>
        private string GenerateId()
        {
            SimulationPage.JobNumber++;

            return "" + SimulationPage.JobNumber;
        }

        #endregion
    }
}
