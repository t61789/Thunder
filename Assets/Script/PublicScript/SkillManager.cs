using System;
using System.Collections;
using System.Collections.Generic;
using Thunder.Skill;
using Thunder.Utility;
using UnityEngine;

namespace Thunder.PublicScript
{
    [DontGenerateWrap]
    public class SkillManager
    {
        private Dictionary<string, Type> skills = new Dictionary<string, Type>();

        private string[] skillBar = new string[5];

        public SkillManager()
        {
            //skills.Add("charge", typeof(Charge));
            //skills.Add("circleShoot", typeof(CircleShoot));

            skillBar[0] = "charge";
            skillBar[1] = "circleShoot";
        }

        public bool UseSkill<T>(T targetScript, KeyCode key, Hashtable arg = null) where T : MonoBehaviour, ISkillManager
        {
            int index = (int)key - 49;
            if (index < 0 || index >= skillBar.Length)
                return false;

            if (skills.TryGetValue(skillBar[index], out Type type))
            {
                ISkillManager interfac = targetScript as ISkillManager;
                GameObject go = (targetScript as MonoBehaviour).gameObject;
                Skill.Skill curSkill = interfac.GetCurSkill();
                if (curSkill != null)
                {
                    if (curSkill.GetType() == type)
                        return false;

                    curSkill.SkillRemove();
                    UnityEngine.Object.Destroy(go.GetComponent(curSkill.GetType()));
                }

                Skill.Skill skill = go.AddComponent(type) as Skill.Skill;
                skill.SkillInit(arg);
                interfac.SetSkill(skill);
                return true;
            }
            return false;
        }
    }
}
