namespace NPCChatLib.WorldClasses
{
    public struct ObjectSlot
    {
        public static readonly ObjectSlot None = new();
        public WorldObject Object;
        public byte Generation;
        public bool IsOccupied;
    }
}