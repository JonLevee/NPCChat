namespace NPCChatLib.Extensions
{
    public static class ValidationExtensions
    {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static string AsString(this object? value)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            return value?.ToString() ?? "(null)";
        }
    }
}
