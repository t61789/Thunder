﻿using System.Collections.Generic;
using UnityEngine;

public class FactoryTurret : Turret
{
    public string fighterName;
    protected float makeInterval;
    protected float makeIntervalCount;//reset

    protected List<Aircraft> fighters = new List<Aircraft>();

    protected bool MakeFighterControl { get; set; }//reset

    protected void Update()
    {
        if (Time.time - makeIntervalCount >= makeInterval)
        {
            if (MakeFighterControl)
            {

                MakeFighter();
                makeIntervalCount = Time.time;
                MakeFighterControl = false;
            }
        }
    }

    public void MakeFighter()
    {
        Fighter fighter = PublicVar.objectPool.DefaultAlloc<Fighter>(fighterName, x=> {
            x.ObjectPoolInit(trans.position, trans.rotation, null, ship);
            x.OnDestroyed += FighterDestroyed;
            fighters.Add(x);
        });
    }

    public void SetFighter(string fighterName)
    {
        this.fighterName = fighterName;
        makeInterval = PublicVar.objectPool.GetPrefab(fighterName).GetComponent<Fighter>().MakeInterval;
    }

    private void FighterDestroyed(Aircraft aircraft)
    {
        aircraft.OnDestroyed -= FighterDestroyed;
        fighters.Remove(aircraft);
    }
}
