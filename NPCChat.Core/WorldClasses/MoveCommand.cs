using System;
using System.Drawing;

namespace NPCChat.Core.WorldClasses
{
    /// <summary>
    /// Issued by the UI thread and consumed by the simulation thread.
    /// Instructs the simulation to path the given Mover to the Target grid position.
    /// </summary>
    public readonly struct MoveCommand : IEquatable<MoveCommand>
    {
        public ObjectHandle Mover { get; }
        public Point Target { get; }

        public MoveCommand(ObjectHandle mover, Point target) { Mover = mover; Target = target; }

        public bool Equals(MoveCommand other) => Mover.Equals(other.Mover) && Target == other.Target;
        public override bool Equals(object obj) => obj is MoveCommand other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Mover, Target);
        public static bool operator ==(MoveCommand left, MoveCommand right) => left.Equals(right);
        public static bool operator !=(MoveCommand left, MoveCommand right) => !left.Equals(right);
    }
}
