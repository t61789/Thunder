using System.Collections.Generic;
using System.ComponentModel;
using Thunder.Turret;
using Thunder.Utility;

namespace Thunder.GameMode
{
    public abstract class Survival : BaseGameMode
    {
        protected struct Param
        {
            public string diffId;
            [DefaultValue(GlobalSettings.defaultSurvivalGenerateRange)]
            public float generateRange;
        }

        protected float w;//(y2-y1)/(x2-x1)

        protected float generateRange;

        protected Ship player;

        protected List<Aircraft> enemys;

        public override void Init(string arg)
        {
            player = Sys.Stable.player.SetPlayer(Sys.Stable.saveManager.playerShipParam);
            player.OnDead += PlayerDead;
            enemys = new List<Aircraft>();
        }

        protected void PlayerDead(Aircraft player)
        {
            foreach (var item in enemys)
                Sys.Stable.objectPool.Recycle(item);
            Sys.Stable.objectPool.Recycle(player);

            Complete();
        }

        protected void EnemyDead(Aircraft enemy)
        {
            enemys.Remove(enemy);
        }
    }
}
