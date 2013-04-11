using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSystemSim
{
    public class UserGroupData : Updatable, INotifyPropertyChanged
    {
        #region Variables (private)

        private UserGroup view;

        private int curEventCooldown = 0;
        
        private SystemComponentData goal;
        
        #endregion


        #region Properties (public)

        public UserGroup View
        {
            get { return view; }
            set { view = value; }
        }

        public SystemComponentData Goal
        {
            get { return goal; }
            set { goal = value; }
        }

        public int CurEventCooldown
        {
            get { return curEventCooldown; }
            set
            {
                curEventCooldown = value;
                OnPropertyChanged("CurEventCooldown");
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

        public UserGroupData(UserGroup view)
        {
            this.view = view;
        }

        #endregion


        #region Methods

        public Job GenerateArrival()
        {
            CurEventCooldown = (int)PseudoRandomGenerator.ExponentialRVG(view.InterarrivalERVGMean);
            Job newEvent = new Job(CurEventCooldown + MainPage.SimClockTicksStatic, this);

            return newEvent;
        }

        public void Update()
        {
            CurEventCooldown -= 1;

            if (CurEventCooldown <= 0)
            {
                goal.EventQueue.Add(GenerateArrival());
            }
        }

        #endregion
    }
}
