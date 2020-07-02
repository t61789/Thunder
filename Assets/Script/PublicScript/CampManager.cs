using System.Collections.Generic;
using Assets.Script.Turret;

public class CampManager
{
    public static readonly string PlayerCamp = "player";

    private struct CampUnit
    {
        public string campName;
        public HashSet<string> hostile;
        public HashSet<string> allay;

        public CampUnit(string campName)
        {
            this.campName = campName;
            hostile = new HashSet<string>();
            allay = new HashSet<string>();
        }

        public bool AddHostile(string name)
        {
            return hostile.Add(name);
        }

        public bool AddAlly(string name)
        {
            return allay.Add(name);
        }
    }

    private Dictionary<string, CampUnit> camps = new Dictionary<string, CampUnit>();

    public CampManager()
    {
        foreach (var item in PublicVar.dataBase["camp"].Select(null, null).Rows)
            camps.Add((string)item[0], new CampUnit((string)item[0]));
        PublicVar.dataBase.DeleteTable("camp");

        foreach (var item in PublicVar.dataBase["camp_hostile"].Select(null, null).Rows)
        {
            camps[(string)item[0]].AddHostile((string)item[1]);
            camps[(string)item[1]].AddHostile((string)item[0]);
        }
        PublicVar.dataBase.DeleteTable("camp_hostile");

        foreach (var item in PublicVar.dataBase["camp_ally"].Select(null, null).Rows)
        {
            camps[(string)item[0]].AddAlly((string)item[1]);
            camps[(string)item[1]].AddAlly((string)item[0]);
        }
        PublicVar.dataBase.DeleteTable("camp_ally");
    }

    public bool IsHostile(Aircraft aircraft1, Aircraft aircraft2)
    {
        return camps[aircraft1.Camp].hostile.Contains(aircraft2.Camp);
    }

    public bool IsAllay(Aircraft aircraft1, Aircraft aircraft2)
    {
        return camps[aircraft1.Camp].allay.Contains(aircraft2.Camp);
    }
}
