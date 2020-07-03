using System.Collections;
using Assets.Script.PublicScript;
using Assets.Script.System;
using Assets.Script.Tool.BuffData;
using Assets.Script.Tool.SerializableStruct;
using UnityEngine;

namespace Assets.Script.Skill
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Charge : Skill
    {
        private GameObject protonSword;
        private float damageCoeffient;
        private BuffData sharpness;
        private float buffPriority;
        private int buffOperator;
        //todo 重写技能系统
        private bool insideObject;
        private bool preInsideObject;
        private Collider2D c2d;
        private Rigidbody2D rb2d;

        public struct Values
        {
            public SerializableVector3 protonSwordPosition;
            public string protonSwordPrefab;
            public float damageCoeffient;
            public float sharpness;
            public int buffOperator;
            public float buffPriority;
        }

        private void Awake()
        {
            c2d = GetComponent<Collider2D>();
            rb2d = GetComponent<Rigidbody2D>();
            //playerController = GetComponent<PlayerController>();
            insideObject = false;

            Values values = PublicVar.value.LoadValue<Values>("skill" + Paths.Div + "charge");

            damageCoeffient = values.damageCoeffient;
            protonSword = Instantiate(PublicVar.bundle.GetAsset<GameObject>(values.protonSwordPrefab));
            protonSword.transform.SetParent(transform);
            protonSword.transform.localRotation = Quaternion.AngleAxis(0, Vector3.forward);
            protonSword.transform.localPosition = values.protonSwordPosition;
            sharpness = values.sharpness;
            buffOperator = values.buffOperator;
            buffPriority = values.buffPriority;

            protonSword.SetActive(false);
        }

        private void Update()
        {
            if (PublicVar.control.RequestUp(KeyCode.Space, "playerControl"))
            {
                if (!insideObject)
                    EndSkill();

                PublicVar.control.Release(KeyCode.Space, "playerControl");
            }

            if (PublicVar.control.RequestDown(KeyCode.Space, "playerControl"))
            {
                if (!insideObject)
                    StartSkill();

                PublicVar.control.Release(KeyCode.Space, "playerControl");
            }
        }

        private void FixedUpdate()
        {

            if (preInsideObject && !insideObject)
            {
                //playerController.dragCoefficient.RemoveBuff("charge");

                if (!Input.GetKey(KeyCode.Space))
                {
                    protonSword.SetActive(false);
                    c2d.isTrigger = false;
                }
            }
            preInsideObject = insideObject;
            insideObject = false;
        }

        private void StartSkill()
        {
            protonSword.SetActive(true);
            c2d.isTrigger = true;
        }

        private void StaySkill()
        {
            //playerController.dragCoefficient.AddBuff(buffOperator, sharpness, buffPriority, "charge");
        }

        private void EndSkill()
        {
            protonSword.SetActive(false);
            c2d.isTrigger = false;
            //playerController.dragCoefficient.RemoveBuff("charge");
        }

        public override void SkillInit(Hashtable arg)
        {

        }

        public override void SkillRemove()
        {
            //playerController.dragCoefficient.RemoveBuff("charge");
            Destroy(protonSword);
        }

        //private void OnTriggerEnter2D(Collider2D collision)
        //{
        //    EnemyController enemyController = collision.gameObject.GetComponent<EnemyController>();
        //    if (enemyController != null)
        //    {
        //        float damage = (collision.attachedRigidbody.velocity - rb2d.velocity).sqrMagnitude * damageCoeffient;
        //        enemyController.GetDamage(rb2d.velocity.sqrMagnitude * damageCoeffient);
        //        if (!insideObject)
        //            StartSkill();
        //    }
        //}

        //private void OnTriggerStay2D(Collider2D collision)
        //{
        //    EnemyController enemyController = collision.gameObject.GetComponent<EnemyController>();
        //    if (enemyController != null)
        //    {
        //        Debug.Log(1);
        //        StaySkill();
        //        insideObject = true;
        //    }
        //}
    }
}

