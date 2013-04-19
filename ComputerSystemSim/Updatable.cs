using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSystemSim
{
    public interface Updatable
    {
        string Name { get; }

        Job.EventTypes EventType { get; }                

        void Update();
    }
}
