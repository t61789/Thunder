using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Newtonsoft.Json;
using UnityEngine;

namespace Thunder
{
    public class RespawnerCenter:MonoBehaviour
    {
        public Transform RespawnerContainer;
        public float Difficulty = 1;

        public static RespawnerCenter Ins { private set; get; }

        private Respawner[] _Respawners;

        private void Awake()
        {
            Ins = this;

            var sel = 
                from Transform trans in RespawnerContainer 
                select trans.GetComponent<Respawner>();
            _Respawners = sel.ToArray();
            foreach (var respawner in _Respawners)
            {
                respawner.Init(() => Difficulty);
                respawner.enabled = false;
            }
        }

        private void OnDestroy()
        {
            Ins = null;
        }

        public void Enable(bool enable)
        {
            foreach (var respawner in _Respawners)
                respawner.enabled = enable;
        }

        //public void BuildRespawner(string respawnerTable)
        //{

        //    const string prefabPathField = "prefab_path";
        //    const string respawnTimeField = "respawn_time";
        //    const string x = "x";
        //    const string y = "y";
        //    const string z = "z";

        //    foreach (var row in DataBaseSys.GetTable(respawnerTable))
        //    {
        //        var respawner = GameObjectPool.Get<Respawner>(Config.RespawnerAssetPath);
        //        respawner.PrefabPath = row[prefabPathField];
        //        respawner.RespawnTime = row[respawnTimeField];
        //        respawner.Trans.position = new Vector3();
        //    }
        //}

        //private class RespawnerJsonReceiver
        //{
        //    public float RespawnTime;
        //    public string PrefabPath;
        //}
    }
}
