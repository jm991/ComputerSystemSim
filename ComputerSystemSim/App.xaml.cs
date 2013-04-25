using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace ComputerSystemSim
{
    /// <summary>
    /// Computer System Simulation Visualization
    /// Windows 8 Modern App
    /// 
    /// <c>John McElmurray</c>
    /// <c>PID: panta@vt.edu</c>
    /// <value>2013.04.25</value>
    /// 
    /// HOW TO RUN THIS PROGRAM:
    /// - Must be run on Windows 8 operating system
    /// - Open the ComputerSystemSim solution file in Visual Studio 2012 or Expression Blend 5 for Visual Studio
    /// - Press F5 or deploy to Local Machine button
    /// - On the first screen enter the values for the trial #, warmup jobs, and steady state jobs (defaults to correct values)
    /// - Hit OK
    /// - Press the Play button to watch animations (slider changes speed)
    /// - Press Run Trial to run current trial to completion
    /// - Press Run Simulation to run current trial and all remaining trials and go to the Output screen (suggested)
    /// - View the data
    /// 
    /// CONCEPTUAL FRAMEWORK DESIGN:
    /// This solution employs a modified version of Event Scheduling for Object Oriented programming using M-V-VM
    /// (Model-View-ViewModel) paradigm. Event Scheduling was an ideal choice due to the characterization of the system
    /// we were given. The combination of UserGroups that create events at interarrival times and queuing at system
    /// components lends itself to this approach.
    /// This implementation employs the usage of a warmup period to ensure values during the steady state are reliable.
    /// The events were broken down as such:
    /// Event	    Description	
    /// E_1	        Job arrival to Mac
    /// E_2	        Start of Mac processing
    /// E_3	        Job arrival to NeXT
    /// E_4	        Start of execution on NeXT
    /// E_5	        Job arrival to the Printer
    /// E_6	        Start of printing
    /// E_7	        Job departure from the computer system
    /// And into activities:
    /// Activity	Description	            Starting Event	Ending Event
    /// A_1	        Waiting for Mac	        E_1	            E_2
    /// A_2	        Execution of Mac	    E_2	            E_3
    /// A_3	        Waiting for NeXT	    E_3	            E_4
    /// A_4	        Execution on NeXT	    E_4	            if (Printer.Queue.Size less 10) E_5, else E_7
    /// A_5	        Waiting for Printer	    E_5	            E_6
    /// A_6	        Execution on Printer	E_6	            E_7
    /// My programming approch took advantage of Activities and performs the necessary logic for each one. See "EVENT SCANNING."
    /// For more information on M-V-VM, <see cref="http://en.wikipedia.org/wiki/Model_View_ViewModel"/>.
    /// 
    /// INITIALIZATIONS:
    /// The logic for initialization is in the SimulationPage.xaml class in the method <see cref="InitializeSimulation"/>.
    /// Here, all values used in the simulation are reset to 0 ensure the system starts from a blank slate.
    /// The initialization is completed by creating one Job from each UserGroup and adding it to the jobQueue.
    /// 
    /// DATA STRUCTURE USAGE:
    /// I used a variable in each component to track the next time the system component's queue would be empty and was
    /// therefore able to use a single a min priority queue implemented using a heap for the job queue. I chose a 
    /// min-binary heap because the "heapify" method's ability to keep the data sorted efficiently for add and remove.
    /// Add: O(log n)
    /// Remove: O(log n)
    /// Build: O(n) - not applicable, since we build using additions only (starts empty)
    /// 
    /// TIME FLOW MECHANISM:
    /// In the event based approach, the simulation clock was updated to the event's execution time in my 
    /// event scanning function, SimulationPage.xaml's <see cref="ComputeSimulationEvent"/> method. I chose to abstract
    /// away from increasing time at a constant rate because this approach provided a much faster simulation. 
    /// Advancing the clock by events meant a less realistic animation, since all events execute in the same amount of time.
    /// The logic to disregard warmup time is also in this method.
    /// 
    /// EVENT SCANNING PER EVENT:
    /// The event scanning function in SimulationPage.xaml's <see cref="ComputeSimulationEvent"/> method works by
    /// removing the min value from the jobQueue (the next event to arrive) and checking its EventType parameter, which
    /// indicates which component in the system it is currently being processed by. Using this value as the pivot for
    /// a switch statement, processing the event and executing the proper activity is trivial and only requires 4
    /// cases:
    ///     UG_FINISH
    ///         This indicates an event created by the UserGroup, i.e. interarrival time between events.
    ///         Create a new event, add it to the jobQueue, and send the current job to the Mac where it's entry 
    ///         time will be set and the number of jobs in the system incremented.
    ///         The arrival time of the job is updated to the next time the Mac is idle plus the processing time required to
    ///         send it to the NeXT station. 
    ///         The jobs area is also updated since the number of jobs in the system changed.
    ///     MAC_FINISH
    ///         This indicates the end of the Mac's processing of a job. The job is sent to the NeXTStation and assigned
    ///         a new arrival time of the next time the NeXTStation is idle plus the processing time required to send it
    ///         to the printer.
    ///     NEXT_FINISH
    ///         There are two options here:
    ///         Send to Printer
    ///             If the printer queue is not full, the job is sent to the printer and its arrival time is updated.
    ///             The printer queue is incremented.
    ///         Bypass printer
    ///             If the printer queue is full, the job goes straight out of the system, the number of jobs decremented
    ///             and the completed jobs incremented. However, it is not counted in the W calculation.
    ///             The jobs area is also updated since the number of jobs in the system changed.
    ///     LASERJET_FINISH
    ///         When the printer finishes a job, its queue is decremented and the job exits the system, contributing its
    ///         values to W and L.
    /// 
    /// RANDOM NUMBER GENERATOR:
    /// I used the random number generator given in class with some slight modifications to enable it to work statically with C# style
    /// pass by reference rather than pointers. For more information on this class, please <see cref="PseudoRandomGenerator.cs"/>.
    /// 
    /// EXPONENTIAL RANDOM VARIATE GENERATOR
    /// Also held within <see cref="PseudoRandomGenerator.cs"/> as a static method, ExponentialRVG takes a mean value
    /// and computes random numbers around that value. This method uses the RandomNumberGenerator method and is primarily
    /// for calculating interarrival times (UserGroups) and process times (SystemComponents).
    /// 
    /// OUTPUT:
    /// My <see cref="Output"/> class stores all of the 5 variables (p_Mac, p_NeXT, p_LaserJet, W, L) returned by the simulation.
    /// An instance of the class is created for every trial run and stored in an ObservableCollection which is databound to a ListBox
    /// in the UI for easy viewing by the user. For more information, <seealso cref="OutputPage.xaml.cs"/>.
    /// 
    /// 
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(InputPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
