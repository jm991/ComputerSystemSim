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
    public sealed partial class SystemComponent : UserControl
    {
        #region Variables (private)

        private SystemComponentData data;

        #endregion


        #region Properties (public)

        public SystemComponentData Data
        {
            get { return data; }
            set { data = value; }
        }

        public double ProcessMean
        {
            get
            {
                double val = 0;
                bool parsed = double.TryParse(GetValue(ProcessProperty).ToString(), out val);
                if (parsed)
                    return val;
                else
                    return -1;  // error val
            }
            set { SetValue(ProcessProperty, value); }
        }

        public string ComponentName
        {
            get { return (string)GetValue(ComponentNameProperty); }
            set { SetValue(ComponentNameProperty, value); }
        }

        #endregion


        #region Dependency Properties

        public static readonly DependencyProperty ComponentNameProperty = DependencyProperty.Register
        (
            "ComponentName",
            typeof(string),
            typeof(UserGroup),
            new PropertyMetadata(0, new PropertyChangedCallback(ChangeComponentName))
        );

        private static void ChangeComponentName(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as SystemComponent).UpdateComponentName(e.NewValue.ToString());
        }

        private void UpdateComponentName(string newText)
        {
            NameBox.Text = newText;
        }

        public static readonly DependencyProperty ProcessProperty = DependencyProperty.Register
        (
            "ProcessMean",
            typeof(double),
            typeof(SystemComponent),
            new PropertyMetadata(0, new PropertyChangedCallback(ChangeProcessMean))
        );

        private static void ChangeProcessMean(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as SystemComponent).UpdateProcessMean(e.NewValue);
        }

        private void UpdateProcessMean(Object newDouble)
        {
            ProcessBox.Text = "" + newDouble.ToString();
        }

        #endregion


        #region Constructors

        public SystemComponent()
        {
            this.InitializeComponent();

            data = new SystemComponentData(this);

            this.DataContext = data;
        }

        #endregion


        #region Methods



        #endregion
    }
}
