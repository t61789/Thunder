namespace Thunder
{
    public class GlobalSettings
    {
        public const string DefaultCamp = "Unknown";
        public const int CampMapSize = 64;
        public const int CampMaxFriendliness = 100;
        public const int CampNeutralValue = 30;
        public const int AssetIdCacheSize = 20;
        public const int PackageItemInfoBuffer = 10;

        public const string MainWeaponType = "mainWeapon";
        public const string SecondaryWeaponType = "secondaryWeapon";
        public const string MeleeWeaponType = "meleeWeapon";
        public const string ThrowingWeaponType = "throwingWeapon";
        public const string PreWeaponKeyName = "SwitchPreWeapon";
        public const string DropWeaponKeyName = "DropWeapon";

        public const string FireKeyName = "Fire1";
        public const string AimScopeKeyName = "AimScope";
        public const string SwitchFireModeKeyName = "SwitchFireMode";
        public const string InteractiveKeyName = "Interactive";

        public const string CrossbowArrowAssetPath = "crossbowArrow";
        public const string UnarmedAssetPath = "unarmed";
        public const string MeleeAttackAreaAssetPath = "meleeAttackArea";
        public const string PackageCellPrefabAssetPath = "packageCell";

        public const string ItemInfoTableName = "item_info";

        public const int UnarmedId = 1;

        public static string[] WeaponTypes =
        {
            MainWeaponType,
            SecondaryWeaponType,
            MeleeWeaponType,
            ThrowingWeaponType
        };

        // 切换按键命名规则：Switch+首字母大写的weapontype，在尾部添加初始为0的数字
        // 重复则+1
        public static string[] WeaponBeltCellTypes =
        {
            MainWeaponType,
            MainWeaponType,
            SecondaryWeaponType,
            MeleeWeaponType,
            ThrowingWeaponType
        };
    }
}