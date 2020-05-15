using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Survival:BaseGameMode
{
    protected float w;//(y2-y1)/(x2-x1)

    protected float generateRange;

    public override void Init(Transform target, string arg)
    {
        this.target = target;
    }
}
