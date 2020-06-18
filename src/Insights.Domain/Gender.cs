using System;

namespace Insights.Domain
{
    public class Gender : IEquatable<Gender>
    {
        public Gender(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public bool Equals(Gender other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}
