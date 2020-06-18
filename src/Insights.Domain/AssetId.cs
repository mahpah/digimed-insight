using System;

namespace Insights.Domain
{
    public class AssetId : IEquatable<AssetId>
    {
        public AssetId(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public bool Equals(AssetId other)
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