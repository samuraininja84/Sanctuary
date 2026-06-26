using System;

namespace Sanctuary.Blackboard
{
    [Serializable]
    public class BlackboardEntry<T> : IEquatable<T>
    {
        public BlackboardKey Key { get; }
        public T Value { get; }
        public Type ValueType { get; }

        private BlackboardEntry(BlackboardKey key, T value)
        {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }

        public static BlackboardEntry<T> Create(BlackboardKey key, T value) => new(key, value);

        // Implicit operator to convert BlackboardEntry<T> to T
        public static implicit operator T(BlackboardEntry<T> entry) => entry.Value;

        // Equality comparison based on the hash code of the Value property
        public static bool operator ==(BlackboardEntry<T> left, BlackboardEntry<T> right) => left.Equals(right);
        public static bool operator !=(BlackboardEntry<T> left, BlackboardEntry<T> right) => !left.Equals(right);

        public bool Equals(T other) => Value.GetHashCode() == other.GetHashCode();

        public override bool Equals(object obj) => obj is BlackboardEntry<T> other && other.Key == Key;

        public override int GetHashCode() => Key.GetHashCode();
    }
}
