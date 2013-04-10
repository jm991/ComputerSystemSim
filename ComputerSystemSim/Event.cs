using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ComputerSystemSim
{
    public class Event : INotifyPropertyChanged
    {
        #region Variables (private)

        private string uniqueID;
        private UserGroup creator;
        private int arrivalTime;

        #endregion


        #region Properties (public)

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

        public Event()
        {
            this.arrivalTime = 0;
        }

        public Event(int arrivalTime, UserGroup creator)
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
