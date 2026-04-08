using System;

namespace NPCChat.Core.Exceptions
{
    /// <summary>
    /// Thrown by the simulation loop when a command channel exceeds its
    /// configured threshold, indicating the simulation cannot keep up with
    /// the rate of incoming commands.
    /// </summary>
    public class SimulationOverflowException : Exception
    {
        public SimulationOverflowException(string message) : base(message)
        {
        }
    }
}
