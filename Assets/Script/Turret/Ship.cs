using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Tool;
using UnityEngine;

/// <summary>
/// 数据库：搭载点
/// 炮塔
/// </summary>
public class Ship : Aircraft
{
    [Serializable]
    public class AttachPoint
    {
        public int index;
        public SerializableVector3 position;
        public SerializableVector3 rotation;
        public string attachType;
        [XmlIgnore]
        public Turret turret;

        public AttachPoint(int index, SerializableVector3 position, SerializableVector3 rotation, string attachType, Turret turret)
        {
            this.index = index;
            this.position = position;
            this.rotation = rotation;
            this.attachType = attachType;
            this.turret = turret;
        }
    }

    protected List<AttachPoint> attachPoints = null;

    protected Turret[] turrets;

    protected override void Awake()
    {
        base.Awake();
        if (attachPoints == null)
            SetAttachPoints();
    }

    private void SetAttachPoints()
    {
        attachPoints = new List<AttachPoint>();
        int index = 0;
        foreach (var item in PublicVar.dataBaseManager["attachPoints"].Select( null, new (string, object)[]{ ("ship_name", AircraftName) }).Rows)
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
    }

    public GameObject AttachTurret<T>(string prefabPath, int pointIndex)
    {
        return AttachTurret(prefabPath, pointIndex, typeof(T).Name);
    }

    public GameObject AttachTurret(string prefabPath, int pointIndex, Type turretType)
    {
        return AttachTurret(prefabPath, pointIndex, turretType.Name);
    }

    public GameObject AttachTurret(string prefabPath, int pointIndex, string turretType)
    {
        GameObject turret = AttachTurret(prefabPath, pointIndex);
        AircraftController.AttachTo(turret, turretType);
        return turret;
    }

    public GameObject AttachTurret(string prefabPath, int pointIndex)
    {
        if (attachPoints[pointIndex].turret != null)
            RemoveTurret(pointIndex);

        GameObject turret = Instantiate(PublicVar.objectPool.GetPrefab(prefabPath));

        attachPoints[pointIndex].turret = turret.GetComponent<Turret>();
        attachPoints[pointIndex].turret.Install(this, attachPoints[pointIndex].position, attachPoints[pointIndex].rotation);

        return turret;
    }

    public void RemoveTurret(int pointIndex)
    {
        attachPoints[pointIndex].turret.Remove();
        attachPoints[pointIndex].turret = null;
    }

    public List<AttachPoint> GetAttachPoints()
    {
        if (attachPoints == null)
            SetAttachPoints();
        return attachPoints;
    }
}
