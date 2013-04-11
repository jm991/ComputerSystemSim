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

        private SortedObservableCollection<Job> eventQueue;

        #endregion


        #region Properties (public)

        public SortedObservableCollection<Job> EventQueue
        {
            get { return eventQueue; }
            set
            {
                eventQueue = value;
                OnPropertyChanged("EventQueue");
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
            EventQueue = new SortedObservableCollection<Job>(i => i.ArrivalTime);

            this.view = view;
        }

        #endregion


        #region Methods

        public void Update()
        {

        }

        #endregion
    }
}
