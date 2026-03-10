using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPCChatLib.Builders;

namespace NPCChatLib.LocalEventArgs
{
    public class ChangeEventArgs<T> : EventArgs
    {
        public T OldValue { get; }
        public T NewValue { get; }
        public ChangeEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public class ChangeEventArgUpdator
    {
        public static void Update<T>(EventHandler<ChangeEventArgs<T>> onChanged, ref T value, T newValue)
        {
            if (value is null && newValue is null) return;
            if (value is not null && newValue is not null && value.Equals(newValue)) return;
            var oldValue = value;
            value = newValue;
            onChanged?.Invoke(null, new ChangeEventArgs<T>(oldValue, newValue));
        }
    }
}
