using System;

namespace Framework
{
    [Flags]
    public enum ItemFlag
    {
        Weapon = 0x1,
        Packageable = 0x2,
        Stackable = 0x4,
        Food = 0x8
    }

    public struct ItemInfo
    {
        public ItemId Id;
        public string Name;
        public ItemFlag Flag;
        public string PickPrefabPath;
        public string WeaponPrefabPath;
        public int MaxStackNum;
        public string PackageCellTexturePath;
    }
}
