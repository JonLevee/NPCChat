using System;


namespace NPCChat.Core.WorldClasses
{

    /// <summary>
    /// 32-bit packed handle:
    /// low 24 bits = slot id
    /// high 8 bits = generation
    /// </summary>
    public readonly struct ObjectHandle : IEquatable<ObjectHandle>
    {
        public static readonly ObjectHandle None = new();

        private const uint IdMask = 0x00FFFFFF;
        private readonly uint _value;

        public int Id => (int)(_value & IdMask);
        public byte Generation => (byte)(_value >> 24);

        public bool IsDefault => _value == 0;

        public ObjectHandle(int id, byte generation)
        {
            if ((uint)id > IdMask)
                throw new ArgumentOutOfRangeException(nameof(id), "Id must fit in 24 bits.");

            _value = ((uint)generation << 24) | (uint)id;
        }

        private ObjectHandle(uint rawValue)
        {
            _value = rawValue;
        }

        public static implicit operator uint(ObjectHandle handle) => handle._value;
        public static implicit operator ObjectHandle(uint rawValue) => new ObjectHandle(rawValue);

        public bool Equals(ObjectHandle other) => _value == other._value;
        public override bool Equals(object obj) => obj is ObjectHandle other && Equals(other);
        public override int GetHashCode() => (int)_value;

        public static bool operator ==(ObjectHandle left, ObjectHandle right) => left._value == right._value;
        public static bool operator !=(ObjectHandle left, ObjectHandle right) => left._value != right._value;

        public override string ToString() => $"Handle(Id={Id}, Gen={Generation})";
    }
}