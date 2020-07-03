using System.Collections;
using UnityEngine;

namespace Assets.Script.Skill
{
    public abstract class Skill : MonoBehaviour
    {
        public float SkillCd;
        public abstract void SkillInit(Hashtable arg);
        public abstract void SkillRemove();
    }
}
