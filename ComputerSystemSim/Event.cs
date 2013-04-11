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
        private UserGroupData creator;
        private int arrivalTime;

        #endregion


        #region Properties (public)

        public string Creator
        {
            get
            {
                string creatorName = creator.View.GroupName;
                if (creatorName.Contains("1"))
                    return "UserGroup1.png";
                else if (creatorName.Contains("2"))
                    return "UserGroup2.png";
                else
                    return "UserGroup3.png";
            }
            set
            {
                OnPropertyChanged("Creator");
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

        public int ArrivalTime
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

        public Job()
        {
            this.arrivalTime = 0;
        }

        public Job(int arrivalTime, UserGroupData creator)
        {
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
