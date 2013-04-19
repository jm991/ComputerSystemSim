using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSystemSim
{
    // TODO: need to make this abstract class, not interface
    public interface Updatable
    {
        string Name { get; }

        Job.EventTypes EventType { get; }

        Uri IconSource { get; }

        void Update();
    }
}
