using System;
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
        private Updatable creator;

        private double arrivalTime;

        private double systemEntryTime = 0;
        private double systemExitTime = 0;

        #endregion


        #region Properties (public)

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
                return creator.EventType;
            }
        }

        public Updatable Creator
        {
            get { return creator; }
            set { creator = value; }
        }

        public string CreatorImageURI
        {
            get
            {
                // TODO: make this more robust; find a substitute for the creator.GetName() by storing the image somewhere
                // TODO: create an abstract class that inherits from UserControl that both SystemComponent, UserGroup inherit from
                // TODO: make Updatable into abstract class instead of interface
                if (creator.Name.Contains("1"))
                    return "UserGroup1.png";
                else if (creator.Name.Contains("2"))
                    return "UserGroup2.png";
                else if (creator.Name.Contains("3"))
                    return "UserGroup3.png";
                else if (creator.Name.Contains("Mac"))
                    return "Mac.png";
                else if (creator.Name.Contains("NeXT"))
                    return "NeXT.png";
                else
                    return "Printer.png";
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

        public Job(double arrivalTime, Updatable creator, double entryTime)
        {
            this.systemEntryTime = entryTime;
            this.arrivalTime = arrivalTime;
            this.creator = creator;
            uniqueID = GenerateId();
        }

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

        private string GenerateId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }

            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
    }
}
