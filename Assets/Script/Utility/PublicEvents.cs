using Framework;
using Thunder.Game.SpotShooting;

using UnityEngine;
using UnityEngine.Events;

namespace Thunder
{
    public class PublicEvents
    {
        /// <summary>
        /// 显示一条信息
        /// </summary>
        public static UnityEvent<string> LogMessage = new UnityEvent<string>();

        /// <summary>
        /// 请求进行游戏<br />请求true，取消请求false
        /// </summary>
        public static UnityEvent<GameType, bool> GameRequest = new UnityEvent<GameType, bool>();

        /// <summary>
        /// 进入等待游戏开始状态
        /// </summary>
        public static UnityEvent<GameType> GameStartDelay = new UnityEvent<GameType>();

        /// <summary>
        /// 游戏开始
        /// </summary>
        public static UnityEvent<GameType> GameStart = new UnityEvent<GameType>();

        /// <summary>
        /// 游戏结束<br />是否是正常结束
        /// </summary>
        public static UnityEvent<GameType, bool> GameEnd = new UnityEvent<GameType, bool>();

        /// <summary>
        /// 击中飞盘
        /// </summary>
        public static UnityEvent FlyingSaucerHit = new UnityEvent();

        /// <summary>
        /// 枪械开火
        /// </summary>
        public static UnityEvent GunFire = new UnityEvent();

        /// <summary>
        /// 枪械切换射击模式<br />连发模式，0为无限连发
        /// </summary>
        public static UnityEvent<int> GunFireModeChange = new UnityEvent<int>();

        /// <summary>
        /// 被击中的靶子
        /// </summary>
        public static UnityEvent<SpotShootingTarget> SpotShootingTargetHit = new UnityEvent<SpotShootingTarget>();

        /// <summary>
        /// 有物品被玩家丢弃<br/>
        /// </summary>
        public static UnityEvent<ItemGroup> DropItem = new UnityEvent<ItemGroup>();

        /// <summary>
        /// 拾取物品
        /// </summary>
        public static UnityEvent<ItemGroup> PickupItem = new UnityEvent<ItemGroup>();

        /// <summary>
        /// 背包物品改变
        /// </summary>
        public static UnityEvent<Package,int[]> PackageItemChanged = new UnityEvent<Package, int[]>();

        /// <summary>
        /// 玩家蹲伏
        /// </summary>
        public static UnityEvent<bool> PlayerSquat = new UnityEvent<bool>();

        /// <summary>
        /// 玩家移动
        /// </summary>
        public static UnityEvent<bool> PlayerMove = new UnityEvent<bool>();

        /// <summary>
        /// 玩家处于悬空状态
        /// </summary>
        public static UnityEvent<bool> PlayerDangling = new UnityEvent<bool>();

        /// <summary>
        /// 开启和关闭了瞄准镜
        /// </summary>
        public static UnityEvent<bool> AimScopeSwitched = new UnityEvent<bool>();

        /// <summary>
        /// 玩家跳跃
        /// </summary>
        public static UnityEvent PlayerJump = new UnityEvent();

        /// <summary>
        /// 产生浮动后坐力
        /// </summary>
        public static UnityEvent<Vector2> RecoilFloat = new UnityEvent<Vector2>();

        /// <summary>
        /// 产生固定后坐力
        /// </summary>
        public static UnityEvent<Vector2> RecoilFixed = new UnityEvent<Vector2>();

        /// <summary>
        /// 取出武器
        /// </summary>
        public static UnityEvent<BaseWeapon> TakeOutWeapon = new UnityEvent<BaseWeapon>();

        /// <summary>
        /// 收回武器
        /// </summary>
        public static UnityEvent<BaseWeapon> PutBackWeapon = new UnityEvent<BaseWeapon>();

        /// <summary>
        /// 玩家死亡
        /// </summary>
        public static UnityEvent PlayerDead = new UnityEvent();

        public static UnityEvent<Process> StartingBuildingMode = new UnityEvent<Process>();

        public static UnityEvent StartBuildingMode = new UnityEvent();

        public static UnityEvent<Process> EndingBuildingMode = new UnityEvent<Process>();

        public static UnityEvent EndBuildingMode = new UnityEvent();
    }
}