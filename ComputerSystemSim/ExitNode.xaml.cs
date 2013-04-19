using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ComputerSystemSim
{
    public sealed partial class ExitNode : UserControl
    {
        #region Variables (private)

        private ExitNodeData data;

        #endregion


        #region Properties

        public ExitNodeData Data
        {
            get { return data; }
            set { data = value; }
        }

        #endregion


        #region Constructors

        public ExitNode()
        {
            this.InitializeComponent();

            data = new ExitNodeData();

            this.DataContext = data;
        }

        #endregion

    }
}
