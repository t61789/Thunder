using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;

namespace Thunder
{
    internal class DroppedItem:BaseEntity,IObjectPool
    {
        public ItemId Id;

        public void Init(ItemId id)
        {
            Id = id;
        }

        public AssetId AssetId { get; set; }

        public void OpReset() { }

        public void OpPut() { }

        public void OpDestroy() { }
    }
}
