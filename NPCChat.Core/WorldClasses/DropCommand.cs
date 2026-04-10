namespace NPCChat.Core.WorldClasses
{
    /// <summary>
    /// Requests that a moveable drops one stack of items onto the tile it occupies.
    /// Processed by the simulation loop on the next tick, inside the world write lock.
    /// </summary>
    public readonly struct DropCommand
    {
        public ObjectHandle DropperHandle { get; init; }
        public string       ItemId        { get; init; }
        public int          Quantity      { get; init; }
    }
}
