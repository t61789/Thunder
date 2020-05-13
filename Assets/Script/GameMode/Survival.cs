using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Survival:BaseGameMode
{
    protected float w;//(y2-y1)/(x2-x1)

    public virtual void Init(Transform target, string diffId, float generateRange)
    {
        this.target = target;
        this.generateRange = generateRange;
    }
}
