using System;

namespace Insights.Domain
{
    public class AssetType
    {
        public AssetType(int typeId, string displayName)
        {
            TypeId = typeId;
            DisplayName = !string.IsNullOrEmpty(displayName)
                ? displayName
                : throw new ArgumentException("Display name is empty");
        }

        public int TypeId { get; }
        public string DisplayName { get; private set; }
    }
}