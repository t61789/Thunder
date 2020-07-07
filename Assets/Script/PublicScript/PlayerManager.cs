using Thunder.Turret;

namespace Thunder.PublicScript
{
    public class PlayerManager
    {
        private Ship player;

        public Ship SetPlayer(Ship.CreateShipParam param)
        {
            if (player != null)
                UnityEngine.Object.Destroy(player.gameObject);

            player = Ship.CreateShip(param);

            Sys.Stable.MainCamera.Target = player.trans;

            return player;
        }
    }
}
