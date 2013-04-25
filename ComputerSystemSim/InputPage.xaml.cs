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
    public class Input
    {
        private int totalJobs;
        private int warmupJobs;
        private int trials;

        public int TotalJobs { get { return totalJobs; } }
        public int WarmupJobs { get { return warmupJobs; } }
        public int Trials { get { return trials; } }

        public Input(int totalJobs, int warmupJobs, int trials)
        {
            this.totalJobs = totalJobs;
            this.warmupJobs = warmupJobs;
            this.trials = trials;
        }
    }

    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class InputPage : ComputerSystemSim.Common.LayoutAwarePage
    {
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

        private void OkBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SimulationPage), new Input(TotalJobs, WarmupJobs, Trials));
        }

        private void TextBox_TextChanged_1(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox senderBox = sender as TextBox;

                senderBox.Text = Regex.Replace(senderBox.Text, @"[^0-9]*", string.Empty, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            }
        }

        public InputPage()
        {
            this.InitializeComponent();
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
    }
}
