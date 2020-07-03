using System.Collections.Generic;
using Assets.Script.PublicScript;
using Assets.Script.Turret;
using Assets.Script.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Script.GameMode
{
    public class SurvivalLi : Survival
    {
        private const string TABLE_NAME = "difficulty_li";
        private const string UI_NAME = "survivalLi";

        private Vector2[] difficultyList;
        private Vector2[] _difficultyList;

        private int leftDiffIndex;

        protected SurvivalLiUI ui;

        public override void Init(string arg)
        {
            base.Init(arg);

            Param tempArg = JsonConvert.DeserializeObject<Param>(arg, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate });
            string diffId = tempArg.diffId;
            generateRange = tempArg.generateRange;

            List<Vector2> difficultyList = new List<Vector2>();
            string time = "time";
            string baseline = "baseline";
            foreach (var item in System.System.dataBase[TABLE_NAME].Select(null, new (string, object)[] { ("diff_id", diffId) }).Rows)
                difficultyList.Add(new Vector2((float)item[time], (float)item[baseline]));
            _difficultyList = difficultyList.ToArray();

            List<AircraftUnit> units = new List<AircraftUnit>();
            foreach (var item in System.System.dataBase[AIRCRAFT_TABLE_NAME].Select(null, new (string, object)[] { ("diff_id", diffId) }).Rows)
                units.Add(new AircraftUnit((string)item[AIRCRAFT_ID], (int)item[MAX], (float)item[BASELINE_MIN], (float)item[BASELINE_MAX], (float)item[INTERVAL]));
            _aircraftUnits = units.ToArray();

            ui = System.System.UiSys.OpenUi<SurvivalLiUI>(UI_NAME, 0, x => x.Init(_difficultyList[_difficultyList.Length - 1].x));

            Reset();
        }

        public override void Reset()
        {
            curDiffculty = 0;
            leftDiffIndex = 0;
            difficultyList = (Vector2[])_difficultyList.Clone();
            aircraftUnits = (AircraftUnit[])_aircraftUnits.Clone();
            CalculateW(leftDiffIndex);
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

        private void FixedUpdate()
        {
            if (running)
            {
                float curTime = Time.time - startTime;

                if (leftDiffIndex != difficultyList.Length - 1)
                {
                    for (int i = leftDiffIndex; i < difficultyList.Length; i++)
                    {
                        if (curTime >= difficultyList[i].x)
                        {
                            if (leftDiffIndex != i)
                            {
                                CalculateW(i);
                                leftDiffIndex = i;
                            }
                            break;
                        }
                    }
                    if (leftDiffIndex != difficultyList.Length - 1)
                        curDiffculty = (curTime - difficultyList[leftDiffIndex].x) * w + difficultyList[leftDiffIndex].y;
                    else
                        curDiffculty = difficultyList[leftDiffIndex].y;
                }

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
                        centerPos = player ? player.trans.position : centerPos;
                        Vector2 temp = Tool.Tools.RandomVectorInCircle(1).normalized * generateRange + centerPos;

                        Aircraft a = System.System.objectPool.Alloc<Aircraft>(aircraftUnits[i].aircraftId, x =>
                        {
                            x.ObjectPoolInit(temp, Quaternion.identity, null, null, "enemy");
                        });

                        enemys.Add(a);
                        a.OnDead += EnemyDead;
                        a.OnDead += aircraftUnits[i].AircraftDestroyed;
                    }
                }

                ui.Refresh(curTime, 0);
            }
        }

        private void CalculateW(int leftIndex)
        {
            if (leftIndex == difficultyList.Length - 1)
                return;
            w = (difficultyList[leftIndex + 1].y - difficultyList[leftIndex].y) / (difficultyList[leftIndex + 1].x - difficultyList[leftIndex].x);
        }

        public override void BeforeUnInstall()
        {
            System.System.UiSys.CloseUi(ui);
            ui = null;
        }
    }
}
