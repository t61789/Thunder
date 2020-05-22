public class PlayerManager
{
    private Ship player;

    public Ship SetPlayer(Ship.CreateShipParam param)
    {
        if (player != null)
            UnityEngine.Object.Destroy(player.gameObject);

        player = Ship.CreateShip(param);

        PublicVar.mainCamera.FollowTarget = player.gameObject;

        return player;
    }
}
