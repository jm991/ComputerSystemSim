using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ComputerSystemSim
{
    /// <summary>
    /// Data class to hold simulation properties set by the user.
    /// </summary>
    public class Input
    {
        #region Variables (private)

        /// <summary>
        /// Jobs to count after warmup period
        /// </summary>
        private int totalJobs;

        /// <summary>
        /// Number of jobs in warmup period
        /// </summary>
        private int warmupJobs;

        /// <summary>
        /// Number of simulation trials to run
        /// </summary>
        private int trials;

        #endregion


        #region Properties (public)

        public int TotalJobs { get { return totalJobs; } }
        public int WarmupJobs { get { return warmupJobs; } }
        public int Trials { get { return trials; } }

        #endregion


        #region Constructors

        /// <summary>
        /// Instantiate a new Input with given params.
        /// </summary>
        /// <param name="totalJobs">Input for totalJobs</param>
        /// <param name="warmupJobs">Input for warmupJobs</param>
        /// <param name="trials">Input for trials</param>
        public Input(int totalJobs, int warmupJobs, int trials)
        {
            this.totalJobs = totalJobs;
            this.warmupJobs = warmupJobs;
            this.trials = trials;
        }

        #endregion
    }

    /// <summary>
    /// Landing page of application; lets user set simulation variables via textboxes.
    /// Serves as both ViewModel/Model since there is no data to store in this page.
    /// 
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class InputPage : ComputerSystemSim.Common.LayoutAwarePage
    {
        #region Properties (public)

        public int TotalJobs
        {
            get
            {
                int val;
                bool parsed = int.TryParse(JobsBox.Text, out val);
                if (parsed)
                {
                    return val;
                }

                return -1;
            }
        }

        public int WarmupJobs
        {
            get
            {
                int val;
                bool parsed = int.TryParse(WarmupBox.Text, out val);
                if (parsed)
                {
                    return val;
                }

                return -1;
            }
        }

        public int Trials
        {
            get
            {
                int val;
                bool parsed = int.TryParse(TrialsBox.Text, out val);
                if (parsed)
                {
                    return val;
                }

                return -1;
            }
        }

        #endregion


        #region Constructors

        public InputPage()
        {
            this.InitializeComponent();
        }

        #endregion


        #region Event Handlers

        /// <summary>
        /// When user clicks OK, navigate to simulation and pass along input params.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SimulationPage), new Input(TotalJobs, WarmupJobs, Trials));
        }

        /// <summary>
        /// Use this handler on textboxes that only can accept numeric input.
        /// </summary>
        /// <param name="sender">TextBox to restrict input on</param>
        /// <param name="e"></param>
        private void TextBox_TextChanged_1(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox senderBox = sender as TextBox;

                senderBox.Text = Regex.Replace(senderBox.Text, @"[^0-9]*", string.Empty, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        #endregion
    }
}
