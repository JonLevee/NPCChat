using System.Drawing;

namespace NPCChat.Core.WorldClasses
{
    /// <summary>
    /// Issued by the UI thread and consumed by the simulation thread.
    /// Instructs the simulation to path the given Mover to the Target grid position.
    /// </summary>
    public readonly record struct MoveCommand(ObjectHandle Mover, Point Target);
}
