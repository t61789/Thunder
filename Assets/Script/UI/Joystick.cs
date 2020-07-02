using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Script.UI
{
    public class Joystick : BaseUI
    {
        public struct Value
        {
            public Vector3 val;
            public bool click;
            public bool doubleClick;
            public bool holding;
        }

        private static Value[] values = new Value[0];

        public bool StablePanel;
        public int Index;
        public float Radius = 203.6f;
        public float CapRadius = 59.21f;
        public float ShowTime = 0.1f;
        public float HideTime = 1f;
        public float StayTime = 0.7f;

        private BaseUI cap;
        private BaseUI panel;
        private Image panelImage;
        private bool dragging;
        private float curAlpha;
        private float alphaCount;
        private float fadeCount;
        private Material imageMaterial;

        protected void Start()
        {
            List<Value> valueList = new List<Value>(values);
            for (int i = valueList.Count; i <= Index; i++)
                valueList.Add(new Value());
            values = valueList.ToArray();

            cap = transform.Find("Cap").GetComponent<BaseUI>();
            panel = transform.Find("Panel").GetComponent<BaseUI>();
            panelImage = panel.GetComponent<Image>();

            cap.DragStart += CapDragStart;
            cap.DragEnd += CapDragEnd;
            cap.Dragging += CapDragging;
            cap.PointerClick += CapClick;
            cap.PointerDown += CapDown;
            cap.PointerUp += CapUp;

            imageMaterial = new Material(panelImage.material);
            panelImage.material = imageMaterial;
        }

        private void Update()
        {
            if (!StablePanel)
            {
                if (!dragging && curAlpha > 0 && Time.time - fadeCount >= StayTime)
                {
                    curAlpha = alphaCount - (Time.time - fadeCount - StayTime) / HideTime;

                    if (curAlpha <= 0)
                        values[Index].val = Vector3.zero;
                    imageMaterial.SetFloat("_GlobalAlpha", curAlpha);
                }
                else if (dragging && curAlpha < 1)
                {
                    curAlpha = alphaCount + (Time.time - fadeCount) / ShowTime;
                    imageMaterial.SetFloat("_GlobalAlpha", curAlpha);
                }
            }
        }

        private void LateUpdate()
        {
            values[Index].click = false;
            values[Index].doubleClick = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, CapRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

        private void CapDragStart(BaseUI baseUI, PointerEventData eventData)
        {
            Vector2 pos = eventData.position;
            if ((pos - (Vector2)cap.rectTrans.position).magnitude < CapRadius)
            {
                dragging = true;
                if (!StablePanel)
                {
                    fadeCount = Time.time;
                    alphaCount = curAlpha;

                    if (curAlpha <= 0)
                        panel.rectTrans.position = cap.rectTrans.position;
                }
            }
        }

        private void CapDragEnd(BaseUI baseUI, PointerEventData eventData)
        {
            if (dragging)
            {
                dragging = false;
                values[Index].val = Vector3.zero;

                if (StablePanel)
                {
                    cap.rectTrans.position = panel.rectTrans.position;
                }
                else
                {
                    fadeCount = Time.time;
                    alphaCount = curAlpha;
                }
            }
        }

        private void CapDragging(BaseUI baseUI, PointerEventData eventData)
        {
            if (dragging)
            {
                Vector3 temp = (Vector3)eventData.position - panel.rectTrans.position;
                if (StablePanel)
                {
                    if (temp.magnitude > Radius)
                        temp = temp.normalized * Radius;
                    cap.rectTrans.position = panel.rectTrans.position + temp;
                }
                else
                {
                    cap.rectTrans.position = eventData.position;
                    if (temp.magnitude > Radius)
                    {
                        temp = temp.normalized * Radius;
                        panel.rectTrans.position = -temp.normalized * Radius + cap.rectTrans.position;
                    }
                }
                values[Index].val = temp.normalized * (temp.magnitude / Radius);
            }
        }

        private void CapClick(BaseUI baseUI, PointerEventData eventData)
        {
            if (dragging) return;

            if (eventData.clickCount == 1)
                values[Index].click = true;
            else
                values[Index].doubleClick = true;
        }

        private void CapDown(BaseUI baseUI, PointerEventData eventData)
        {
            values[Index].holding = true;
        }

        private void CapUp(BaseUI baseUI, PointerEventData eventData)
        {
            values[Index].holding = false;
        }

        public static Value GetValue(int index)
        {
            if (index < 0 || index >= values.Length)
                return new Value();
            return values[index];
        }
    }
}
