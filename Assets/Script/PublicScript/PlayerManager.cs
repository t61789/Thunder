using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
