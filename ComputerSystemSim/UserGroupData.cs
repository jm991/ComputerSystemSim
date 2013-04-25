using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ComputerSystemSim
{
    /// <summary>
    /// Model data for the UserGroup View.
    /// </summary>
    public class UserGroupData : Updatable, INotifyPropertyChanged
    {
        #region Variables (private)

        /// <summary>
        /// TODO: remove this coupling; bad design choice
        /// </summary>
        private UserGroup view;

        /// <summary>
        /// Cooldown based on sim clock till next event fire
        /// </summary>
        private double curEventCooldown = 0;

        /// <summary>
        /// Next item in system that this component feeds to
        /// </summary>
        private Updatable goal;

        /// <summary>
        /// Representative color of this group in UI
        /// </summary>
        private Color groupColor = Color.FromArgb(0, 0, 0, 0);

        /// <summary>
        /// Interarrival mean used in random number generation
        /// </summary>
        private double interarrivalERVGMean = 0;
        
        #endregion


        #region Properties (public)

        public double InterarrivalERVGMean
        {
            get { return interarrivalERVGMean; }
            set
            {
                interarrivalERVGMean = value;
                OnPropertyChanged("InterarrivalERVGMean");
            }
        }

        public Uri IconSource
        {
            get
            {
                return view.IconSource;
            }
        }

        public Color GroupColor
        {
            get { return groupColor; }
            set { groupColor = value; }
        }

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

        /// <summary>
        /// Instantiates a new Model
        /// </summary>
        /// <param name="view">TODO remove this view coupling</param>
        public UserGroupData(UserGroup view)
        {
            this.view = view;
        }

        #endregion


        #region Methods

        /// <summary>
        /// Generates a new event based on this UserGroups random interarrival mean.
        /// </summary>
        /// <param name="simulationClock">Simulation clock for offsetting time to enter system</param>
        /// <returns></returns>
        public Job GenerateArrival(double simulationClock)
        {
            CurEventCooldown = PseudoRandomGenerator.ExponentialRVG(view.InterarrivalERVGMean);
            Job newJob = new Job(CurEventCooldown + simulationClock, this, simulationClock);

            return newJob;
        }

        #endregion
    }
}
