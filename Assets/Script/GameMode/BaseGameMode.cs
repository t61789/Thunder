using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BaseGameMode:MonoBehaviour
{
    protected const string AIRCRAFT_TABLE_NAME = "difficulty_aircraft";
    protected const string AIRCRAFT_ID = "aircraft_id";
    protected const string MAX = "max";
    protected const string BASELINE_MIN = "baseline_min";
    protected const string BASELINE_MAX = "baseline_max";
    protected const string INTERVAL = "interval";

    protected struct AircraftUnit
    {
        public string aircraftId;
        public int max;
        public float baselineMin;
        public float baselineMax;
        public float interval;

        public int curNum;
        public float intervalCount;
        public bool active;

        public AircraftUnit(string aircraftId, int max, float baselineMin, float baselineMax, float interval) : this()
        {
            this.aircraftId = aircraftId;
            this.max = max;
            this.baselineMin = baselineMin;
            this.baselineMax = baselineMin;
            this.interval = interval;
            active = true;
        }

        public void AircraftDestroyed(Aircraft a)
        {
            curNum--;
        }

        public bool AddAircraft()
        {
            if (Time.time - intervalCount < interval)
                return false;
            else
                intervalCount = Time.time;

            if (curNum == max)
                return false;
            curNum++;
            return true;
        }
    }

    protected float startTime;
    protected float curDiffculty;
    protected bool running;

    protected AircraftUnit[] aircraftUnits;
    protected AircraftUnit[] _aircraftUnits;

    protected Transform target;
    protected float generateRange;

    protected Vector3 centerPos;

    //todo 模式结束
    public delegate void ModeComplete(BaseGameMode mode);
    public event ModeComplete OnModeComplete;

    public abstract void Reset();
    public abstract void Start();
    public abstract void Pause();
    public abstract void UnInstall();
}
