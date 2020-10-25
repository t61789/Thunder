using System;
using System.Collections;
using Thunder.Entity;
using Tool;


using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Thunder.UI
{
    public class BaseUI : BaseEntity, IObjectPool
    {
        public RectTransform RectTrans { get; private set; }

        public AssetId AssetId { get; set; }

        protected override void Awake()
        {
            base.Awake();
            RectTrans = transform as RectTransform;
        }

        public virtual void OpRecycle()
        {
        }

        public virtual void OpReset()
        {
        }

        public virtual void OpDestroy()
        {
            Destroy(gameObject);
        }

        public void SetAnchoredPosition(Vector2 pos)
        {
            RectTrans.position = pos;
        }

        public void SetAnchor(Vector2 anchorMax, Vector2 anchorMin)
        {
            RectTrans.anchorMin = anchorMin;
            RectTrans.anchorMax = anchorMax;
        }

        public void SetOffset(Vector2 offsetMax, Vector2 offsetMin)
        {
            RectTrans.offsetMax = offsetMax;
            RectTrans.offsetMin = offsetMin;
        }

        public void InitRect(UiInitType action)
        {
            if (action.HasFlag(UiInitType.MiddleAnchor)) RectTrans.anchorMin = RectTrans.anchorMax = Vector2.one / 2;

            if (action.HasFlag(UiInitType.FillAnchor))
            {
                RectTrans.anchorMax = Vector2.one;
                RectTrans.anchorMin = Vector2.zero;
            }

            if (action.HasFlag(UiInitType.FillSize)) RectTrans.offsetMin = RectTrans.offsetMax = Vector2.zero;

            if (action.HasFlag(UiInitType.PositionMiddleOfAnchor)) RectTrans.anchoredPosition = Vector2.zero;
        }
    }
}