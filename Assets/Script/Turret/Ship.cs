using System.Collections.Generic;
using Thunder.Tool;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.Turret
{
    public class Ship : Aircraft
    {
        public struct AttachPoint
        {
            public int index;
            public Vector3 position;
            public Vector3 rotation;
            public string type;
            public Turret turret;

            public AttachPoint(int index, Vector3 position, Vector3 rotation, string type, Turret turret)
            {
                this.index = index;
                this.position = position;
                this.rotation = rotation;
                this.type = type;
                this.turret = turret;
            }
        }

        public AttachPoint[] attachPoints;

        protected override void Awake()
        {
            base.Awake();
            attachPoints = GetAttachPoints(ControllerName);
        }

        public static AttachPoint[] GetAttachPoints(string shipId)
        {
            List<AttachPoint> attachPoints = new List<AttachPoint>();
            int index = 0;
            foreach (var item in Sys.Stable.DataBase["ship_attach_points"].Select(null, new (string, object)[] { ("ship_id", shipId) }))
            {
                AttachPoint a = new AttachPoint(
                    index,
                    Tools.StringToVector3((string)item["position"]),
                    Tools.StringToVector3((string)item["rotation"]),
                    (string)item["attach_type"],
                    null);
                attachPoints.Add(a);
                index++;
            }
            return attachPoints.ToArray();
        }

        public GameObject AttachTurret(string turretPath, int pointIndex, bool controlable)
        {
            GameObject turret = AttachTurret(turretPath, pointIndex);
            if (controlable)
                ControllerInput.AttachTo(turret);
            return turret;
        }

        public GameObject AttachTurret(string turretPath, int pointIndex)
        {
            if (attachPoints[pointIndex].turret != null)
                RemoveTurret(pointIndex);

            GameObject turret = Instantiate(Sys.Stable.ObjectPool.GetPrefab(turretPath));

            attachPoints[pointIndex].turret = turret.GetComponent<Turret>();
            attachPoints[pointIndex].turret.Install(this, attachPoints[pointIndex].position, attachPoints[pointIndex].rotation);

            return turret;
        }

        public void RemoveTurret(int pointIndex)
        {
            attachPoints[pointIndex].turret.Remove();
            attachPoints[pointIndex].turret = null;
        }

        public struct CreateShipParam
        {
            public string shipId;
            public string camp;
            public string[] turrets;
            public bool controlable;

            public CreateShipParam(string shipId, string camp, string[] turrets, bool controlable)
            {
                this.shipId = shipId;
                this.camp = camp;
                this.turrets = turrets;
                this.controlable = controlable;
            }
        }

        public static Ship CreateShip(CreateShipParam param)
        {
            Ship ship = Sys.Stable.ObjectPool.Alloc<Ship>(param.shipId);
            ship.Camp = param.camp;
            param.turrets = param.turrets ?? new string[0];

            for (int i = 0; i < param.turrets.Length; i++)
            {
                if (param.turrets[i] == null) continue;
                ship.AttachTurret(param.turrets[i], i, param.controlable);
            }

            if (param.controlable)
                ControllerInput.AttachTo(ship.gameObject, true);

            return ship;
        }
    }
}
