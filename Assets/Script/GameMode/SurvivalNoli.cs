﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurvivalNoli : BaseGameMode
{
    private const string TABLE_NAME = "difficulty_noli";
    private const string DIFF_ID = "diff_id";
    private const string UI_NAME = "survivalNoli";

    private (Vector2 left, Vector2 right) curNodes;
    private (Vector2 left, Vector2 right) _curNodes;
    private float risingCoefficient;
    private float interval;

    private float w;

    protected SurvivalNoliUI ui;

    public void Init(Transform target, string diffId, float generateRange)
    {
        this.target = target;
        this.generateRange = generateRange;

        DataTable.Row row = PublicVar.dataBaseManager[TABLE_NAME].Select( null, new (string, object)[] { (DIFF_ID, diffId) }).Rows.FirstOrDefault();
        risingCoefficient = (float)row["rising_coefficient"];
        interval = (float)row["time_interval"];

        float temp = (float)row["base_baseline"];
        _curNodes = (new Vector2(0, temp), new Vector2(interval, temp));

        List<AircraftUnit> units = new List<AircraftUnit>();
        foreach (var item in PublicVar.dataBaseManager[AIRCRAFT_TABLE_NAME].Select( null, new (string, object)[] { ("diff_id", diffId) }).Rows)
            units.Add(new AircraftUnit((string)item[AIRCRAFT_ID], (int)item[MAX], (float)item[BASELINE_MIN], (float)item[BASELINE_MAX], (float)item[INTERVAL]));
        _aircraftUnits = units.ToArray();

        ui = PublicVar.uiManager.OpenUI<SurvivalNoliUI>(UI_NAME);

        Reset();
    }

    private void NextNode()
    {
        Vector2 temp = curNodes.right - curNodes.left;

        float k = temp.y / temp.x + risingCoefficient;
        curNodes.left = curNodes.right;
        curNodes.right = curNodes.left + new Vector2(interval, k * interval);
    }

    private void FixedUpdate()
    {
        if (running)
        {
            float x = Time.time - startTime;
            if (x >= curNodes.right.x)
            {
                NextNode();
                CalculateW();
            }
            curDiffculty = (x - curNodes.left.x) * w + curNodes.left.y;

            for (int i = 0; i < aircraftUnits.Length; i++)
            {
                if (!aircraftUnits[i].active)
                    continue;

                if (curDiffculty > aircraftUnits[i].baselineMax)
                {
                    aircraftUnits[i].active = false;
                    continue;
                }

                if (curDiffculty < aircraftUnits[i].baselineMin)
                    continue;

                if (aircraftUnits[i].AddAircraft())
                {
                    centerPos = target ? target.position : centerPos;
                    Vector2 temp = Tool.Tools.RandomVectorInCircle(1).normalized * generateRange + centerPos;

                    Aircraft a = PublicVar.objectPool.DefaultAlloc<Aircraft>(aircraftUnits[i].aircraftId, item =>
                    {
                        item.ObjectPoolInit(temp, Quaternion.identity, null, null, "enemy");
                    });

                    a.OnDestroyed += aircraftUnits[i].AircraftDestroyed;
                }
            }

            ui.Refresh(x,0);
        }
    }

    private void CalculateW()
    {
        w = (curNodes.right.y - curNodes.left.y) / (curNodes.right.x - curNodes.left.x);
    }

    public override void Reset()
    {
        aircraftUnits = (AircraftUnit[])_aircraftUnits.Clone();
        curNodes = _curNodes;
        curDiffculty = curNodes.left.y;
        CalculateW();
    }

    public override void Start()
    {
        startTime = Time.time;
        running = true;
    }

    public override void Pause()
    {
        running = false;
    }

    public override void UnInstall()
    {
        PublicVar.uiManager.CloseUI(ui);
        ui = null;
    }
}
