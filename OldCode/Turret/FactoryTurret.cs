using System.Collections.Generic;
using UnityEngine;

namespace Thunder.Turret
{
    public class FactoryTurret : Turret
    {
        public string fighterName;
        protected float makeInterval;
        protected float makeIntervalCount;//reset

        protected List<Aircraft> fighters = new List<Aircraft>();

        private const string MAKE_FIGHTER = "MakeFighter";

        protected void Update()
        {
            if (Time.time - makeIntervalCount >= makeInterval)
            {
                if (ControlKeys.GetBool(MAKE_FIGHTER))
                {
                    MakeFighter();
                    makeIntervalCount = Time.time;
                }
            }
        }

        public void MakeFighter()
        {
            Fighter fighter = Sys.Stable.ObjectPool.Alloc<Fighter>(fighterName, x =>
            {
                x.ObjectPoolInit(trans.position, trans.rotation, null, ship);
                x.OnDead += FighterDestroyed;
                fighters.Add(x);
            });
        }

        public void SetFighter(string fighterName)
        {
            this.fighterName = fighterName;
            makeInterval = Sys.Stable.ObjectPool.GetPrefab(fighterName).GetComponent<Fighter>().MakeInterval;
        }

        private void FighterDestroyed(Aircraft aircraft)
        {
            aircraft.OnDead -= FighterDestroyed;
            fighters.Remove(aircraft);
        }
    }
}
