using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;

namespace ComputerSystemSim
{
    /// <summary>
    /// Reflective methods for Storyboards.
    /// </summary>
    public static class StoryboardExtensions
    {
        /// <summary>
        /// Asynchronous firing of Storyboards using threads.
        /// </summary>
        /// <param name="storyboard">Storyboard to wait on</param>
        /// <returns>Task of the Storyboard playback to wait on</returns>
        public static Task BeginAsync(this Storyboard storyboard)
        {
            System.Threading.Tasks.TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                EventHandler<object> onComplete = null;
                onComplete = (s, e) => {
                    storyboard.Completed -= onComplete; 
                    tcs.SetResult(true); 
                };
                storyboard.Completed += onComplete;
                storyboard.Begin();
            }
            return tcs.Task;
        }
    }
}