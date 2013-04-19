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

        private double curEventCooldown = 0;
        
        private Updatable goal;
        
        #endregion


        #region Properties (public)

        public string Name
        {
            get
            {
                return view.GroupName;
            }
        }

        public Job.EventTypes EventType
        {
            get
            {
                return Job.EventTypes.UG_FINISH;
            }
        }

        public UserGroup View
        {
            get { return view; }
            set { view = value; }
        }

        public Updatable Goal
        {
            get { return goal; }
            set { goal = value; }
        }

        public double CurEventCooldown
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
            CurEventCooldown = PseudoRandomGenerator.ExponentialRVG(view.InterarrivalERVGMean);
            Job newJob = new Job(CurEventCooldown + MainPage.SimClockTicksStatic, this, MainPage.SimClockTicksStatic);

            return newJob;
        }

        public Job GenerateArrival(double simulationClock)
        {
            CurEventCooldown = PseudoRandomGenerator.ExponentialRVG(view.InterarrivalERVGMean);
            Job newJob = new Job(CurEventCooldown + simulationClock, this, simulationClock);

            return newJob;
        }

        public void Update()
        {
            CurEventCooldown -= 1;

            if (CurEventCooldown <= 0 && goal is SystemComponentData)
            {
                (goal as SystemComponentData).JobQueue.Add(GenerateArrival());
            }
        }

        #endregion
    }
}
