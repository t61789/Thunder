using Assets.Script.Turret;

namespace Assets.Script.PublicScript
{
    public class PlayerManager
    {
        private Ship player;

        public Ship SetPlayer(Ship.CreateShipParam param)
        {
            if (player != null)
                UnityEngine.Object.Destroy(player.gameObject);

            player = Ship.CreateShip(param);

            System.System.mainCamera.FollowTarget = player;

            return player;
        }
    }
}
