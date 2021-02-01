using Thunder;
using Tool;
// ReSharper disable InconsistentNaming

namespace Framework
{
    public static class Config
    {
        public static string UiFrameworkBaseObjName = "Canvas";
        public static string ConfigXmlPath = "E:\\UnityProjects\\Thunder\\Assets\\Script\\Framework\\Custom\\Config.xml";
        
        public const int CampMapSize = 64;
        public const int CampMaxFriendliness = 100;
        public const int CampNeutralValue = 30;
        public const int AssetIdCacheSize = 20;
        public const int PackageItemInfoBuffer = 10;
        
        public const string PreWeaponKeyName = "SwitchPreWeapon";
        public const string DropWeaponKeyName = "DropWeapon";

        public const string FireKeyName = "Fire1";
        public const string AimScopeKeyName = "AimScope";
        public const string SwitchFireModeKeyName = "SwitchFireMode";
        public const string InteractiveKeyName = "Interactive";

        public const string UnarmedAssetPath = "prefabs/weapon/unarmed";
        public const string DefaultPickupableItemAssetPath = "prefabs/normal/pickupableItem";
        public const string RespawnerAssetPath = "prefabs/normal/respawner";

        public const string BuildingInfoValueAssetPath = "values/normal/building_info";
        public const string ItemInfoValuePath = "values/normal/item_info";
        public const string CampInfoValuePath = "values/normal/camp_info";
        public const string CtrlKeysValuePath = "values/normal/ctrl_keys";
        public const string CombineExpressionValuePath = "values/normal/combine_expression";// todo

        public const string ItemInfoTableName = "database/normal/item_info";
        public const string WeaponInfoTableName = "database/normal/weapon_info";
        public const string TextTableName = "database/normal/text";

        public const int UnarmedId = 1;

        public const string StableLayerName = "Stable";

        public const float DefaultDropForce = 10;

        public const string PlayerCamp = "player";
        public const string NeutralCamp = "neutral";
        public const string EnemyCamp = "enemy";
        public const string DefaultCamp = NeutralCamp;

        public static WeaponType[] WeaponBeltTypes =
        {
            WeaponType.MainWeapon,
            WeaponType.SecondaryWeapon,
            WeaponType.MeleeWeapon,
            WeaponType.ThrowingWeapon,
        };

        /// <summary>
        /// 用于初始化对象
        /// </summary>
        /// <returns>需要加入核心生命周期的继承了IBaseSys的对象</returns>
        public static IBaseSys[] Init()
        {
            // 在这里进行初始化
            RandomRewardGenerator.ResolveDic(DataBaseSys.GetTable("database/normal/reward"));
            CtrlKeys.Init();

            return new IBaseSys[]
            {
                // 添加新的系统
                new LuaSys(), 
                new CampSys(), 
            };
        }
    }


    // 切换按键命名规则：Switch+首字母大写的weapontype，在尾部添加初始为0的数字
    // 重复则+1
    public enum WeaponType
    {
        MainWeapon,
        SecondaryWeapon,
        MeleeWeapon,
        ThrowingWeapon,
    }
}
