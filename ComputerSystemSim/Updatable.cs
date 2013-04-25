using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSystemSim
{
    /// <summary>
    /// Interface for any time sensitive object in the Simulation
    /// </summary>
    public interface Updatable
    {
        string Name { get; }

        Job.EventTypes EventType { get; }

        Uri IconSource { get; }
    }
}
