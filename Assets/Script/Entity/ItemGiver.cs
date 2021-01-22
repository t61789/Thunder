using Framework;
using Thunder.UI;
using UnityEngine;

namespace Thunder
{
    public class ItemGiver:MonoBehaviour,IInteractive
    {
        public int ItemId;
        public int Num;

        public void Interactive(ControlInfo info)
        {
            if (info.Down)
            {
                LogPanel.Ins.Log($"Give player item (id:{ItemId},num:{Num})");
                Player.Ins.ReceiveItem((ItemId,Num));
            }
        }
    }
}
