using System;

namespace NPCChat.Core.Exceptions
{
    public class WorldGenerationException : Exception
    {
        public WorldGenerationException(string message) : base(message)
        {
        }
    }
}
