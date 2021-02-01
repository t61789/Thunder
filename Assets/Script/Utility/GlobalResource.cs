using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Newtonsoft.Json;
using UnityEngine;

namespace Thunder
{
    public class GlobalResource:MonoBehaviour
    {
        public float MaxPower;
        public float MaxPsionic;

        public NumericResource Power;
        public NumericResource Psionic;

        public static GlobalResource Ins { private set; get; }

        private readonly HashSet<Package> _RegisteredPackage = new HashSet<Package>();

        private void Awake()
        {
            Ins = this;
            Power = new NumericResource(0, 0, MaxPower);
            Psionic = new NumericResource(0, 0, MaxPsionic);
        }

        public Transform GetFinalTarget()
        {
            return RadioBaseStation.Ins.Trans;
        }

        public void Cost(GlobalResourceCost cost)
        {
            if (cost == null) return;
            Power.Cost(cost.Power);
            Psionic.Cost(cost.Psionic);
            if (cost.ItemCost == null) return;
            foreach (var i in cost.ItemCost)
            {
                var num = i.Value;
                foreach (var package in _RegisteredPackage)
                {
                    num = package.CostItem(new ItemGroup(i.Key, num)).Remaining.FirstOrDefault().Count;
                    if (num == 0) break;
                }
            }
        }

        public bool CostCheck(GlobalResourceCost cost)
        {
            if (cost == null) return true;
            if (!Power.CostCheck(cost.Power) || !Psionic.CostCheck(cost.Psionic)) return false;

            foreach (var i in cost.ItemCost)
            {
                var num = i.Value;
                foreach (var package in _RegisteredPackage)
                {
                    num -= package.GetItemNum(i.Key);
                    if (num <= 0) break;
                }

                if (num > 0) return false;
            }

            return true;
        }

        public void RegisterPackage(Package package)
        {
            _RegisteredPackage.Add(package);
        }

        public void UnRegisterPackage(Package package)
        {
            _RegisteredPackage.Remove(package);
        }
    }

    public class GlobalResourceCost
    {
        public float Power;
        public float Psionic;
        
        public Dictionary<int, int> ItemCost;
    }
}
