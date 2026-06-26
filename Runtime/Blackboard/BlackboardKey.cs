using System;
using Sanctuary.Extensions;

namespace Sanctuary.Blackboard
{
    [Serializable]
    public readonly struct BlackboardKey : IEquatable<BlackboardKey>
    {
        readonly string name;
        readonly int hashedKey;

        private BlackboardKey(string name)
        {
            // Ensure the name is not null or empty
            this.name = name;

            // Compute the hash of the name using FNV-1a hashing algorithm
            hashedKey = name.ComputeFNV1aHash();
        }

        public static BlackboardKey Create(string newName) => new(newName);

        public readonly bool Equals(BlackboardKey other) => hashedKey == other.hashedKey;

        public override readonly bool Equals(object obj) => obj is BlackboardKey other && Equals(other);

        public override readonly int GetHashCode() => hashedKey;

        public override readonly string ToString() => name;

        public static bool operator ==(BlackboardKey lhs, BlackboardKey rhs) => lhs.hashedKey == rhs.hashedKey;

        public static bool operator !=(BlackboardKey lhs, BlackboardKey rhs) => !(lhs == rhs);
    }
}
