using UnityEngine.Events;

namespace Thunder.Utility
{
    public class PublicEvents
    {
        public static UnityEvent FlyingSaucerHit = new UnityEvent();
        /// <summary>
        /// 请求true，取消请求false
        /// </summary>
        public static UnityEvent<bool> FlyingSaucerGameRequest = new UnityEvent<bool>();
        public static UnityEvent FlyingSaucerGameStartDelay = new UnityEvent();
        public static UnityEvent FlyingSaucerGameStart = new UnityEvent();
        /// <summary>
        /// 是否是正常结束
        /// </summary>
        public static UnityEvent<bool> FlyingSaucerGameEnd = new UnityEvent<bool>();

        public static UnityEvent GunFire = new UnityEvent();
        /// <summary>
        /// 连发模式，0为无限连发
        /// </summary>
        public static UnityEvent<int> GunFireModeChange = new UnityEvent<int>();
    }
}
