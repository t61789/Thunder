using Assets.Script.Turret;
using UnityEngine;

public abstract class BaseGameMode : MonoBehaviour
{
    protected const string AIRCRAFT_TABLE_NAME = "difficulty";
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
            if (baselineMin == -1)
                this.baselineMin = 0;
            else
                this.baselineMin = baselineMin;
            if (baselineMax == -1)
                this.baselineMax = Mathf.Infinity;
            else
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

    public struct CompleteParam
    {
        public int levelIndex;
    }

    protected float startTime;
    protected float curDiffculty;
    protected bool running;

    protected AircraftUnit[] aircraftUnits;
    protected AircraftUnit[] _aircraftUnits;

    protected Vector3 centerPos;

    public delegate void ModeComplete(BaseGameMode mode, CompleteParam completeParam);
    public event ModeComplete OnModeComplete;

    public abstract void Reset();
    public abstract void Start();
    public abstract void Pause();
    public abstract void BeforeUnInstall();
    public abstract void Init(string arg);
    public virtual void Complete()
    {
        OnModeComplete?.Invoke(this, default);
        Pause();
    }
}
