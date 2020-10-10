using System;
using System.Collections.Generic;
using System.Linq;
using Thunder.Sys;
using Thunder.Tool.ObjectPool;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class ListPlane : BaseUI
    {
        protected const string ELEMENT = "element";
        protected Queue<RectTransform> elementContainers = new Queue<RectTransform>();

        protected List<BaseUI> elements = new List<BaseUI>();
        protected RectTransform elementsTrans;
        protected RectTransform maskTrans;
        protected (Scrollbar x, Scrollbar y) scrollbar;
        protected Vector2 scrollRange;

        protected override void Awake()
        {
            base.Awake();
            scrollbar.y = RectTrans.Find("ScrollbarY").GetComponent<Scrollbar>();
            scrollbar.x = RectTrans.Find("ScrollbarX").GetComponent<Scrollbar>();
            maskTrans = RectTrans.Find("Mask").GetComponent<RectTransform>();
            elementsTrans = maskTrans.Find("Elements").GetComponent<RectTransform>();
        }

        public BaseUI[] Init(Parameters arg, List<Action<BaseUI>> inits)
        {
            return Init<BaseUI>(arg, inits);
        }

        public T[] Init<T>(Parameters arg, List<Action<T>> inits) where T : BaseUI
        {
            Clear();
            return CreateElements(arg, inits);
        }

        public void Clear()
        {
            scrollbar.y.size = 1;
            scrollbar.y.value = 0;
            scrollbar.x.size = 1;
            scrollbar.x.value = 0;

            foreach (var item in elements)
                ObjectPool.Ins.Recycle(item);
            elements.Clear();

            foreach (RectTransform item in elementsTrans)
            {
                item.gameObject.SetActive(false);
                elementContainers.Enqueue(item);
            }
        }

        protected T[] CreateElements<T>(Parameters parameters, List<Action<T>> inits) where T : BaseUI
        {
            if (parameters.elementSize.x == 0)
            {
                parameters.elementSize.x =
                    (maskTrans.rect.width - (parameters.rowCount - 1) * parameters.elementInterval.x) /
                    parameters.rowCount;
                parameters.planeSize.x = 0;
            }

            if (parameters.elementSize.y == 0)
                parameters.elementSize.y = parameters.elementSize.x;

            scrollbar.x.gameObject.SetActive(parameters.planeSize.x != 0f);
            scrollbar.y.gameObject.SetActive(parameters.planeSize.y != 0f);

            elementsTrans.anchoredPosition = Vector3.zero;

            var interval = new Vector2(parameters.elementInterval.x + parameters.elementSize.x,
                parameters.elementInterval.y + parameters.elementSize.y);
            var temp = (inits.Count - 1) / parameters.rowCount + 1;
            var planeSize = new Vector2(
                parameters.rowCount * parameters.elementSize.x +
                parameters.elementInterval.x * (parameters.rowCount - 1),
                temp * parameters.elementSize.y + parameters.elementInterval.y * (temp - 1));

            if (scrollbar.x.gameObject.activeSelf)
            {
                RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parameters.planeSize.x);
                scrollRange.x = planeSize.x - maskTrans.rect.width;
                scrollRange.x = scrollRange.x < 0 ? 0 : scrollRange.x;
                scrollbar.y.value = 0;
                scrollbar.y.size = maskTrans.rect.width / planeSize.x;
            }
            else
            {
                RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    planeSize.x + maskTrans.offsetMin.x - maskTrans.offsetMax.x);
            }

            if (scrollbar.y.gameObject.activeSelf)
            {
                RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parameters.planeSize.y);
                scrollRange.y = planeSize.y - maskTrans.rect.height;
                scrollRange.y = scrollRange.y < 0 ? 0 : scrollRange.y;
                scrollbar.y.value = 0;
                scrollbar.y.size = maskTrans.rect.height / planeSize.y;
            }
            else
            {
                RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    planeSize.y + maskTrans.offsetMin.y - maskTrans.offsetMax.y);
            }

            var tempx = parameters.elementSize.x / 2;
            var tempy = parameters.elementSize.y / 2;
            for (var i = 0; i < inits.Count; i++)
            {
                var x = i % parameters.rowCount;
                var y = i / parameters.rowCount;
                var position = new Vector2(x * interval.x + tempx, -y * interval.y - tempy);

                RectTransform elementContainer;
                if (elementContainers.Count != 0)
                {
                    elementContainer = elementContainers.Dequeue();
                    elementContainer.gameObject.SetActive(true);
                }
                else
                {
                    elementContainer = new GameObject(ELEMENT).AddComponent<RectTransform>();
                }

                elementContainer.anchorMax = Vector2.zero;
                elementContainer.anchorMin = Vector2.zero;
                elementContainer.SetParent(elementsTrans);
                elementContainer.anchoredPosition = position;
                elementContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parameters.elementSize.x);
                elementContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parameters.elementSize.y);

                var rectTransform = ObjectPool.Ins
                    .Alloc(null, UISys.DefaultUiBundle, parameters.elementName, inits[i], elementContainer)
                    .GetComponent<RectTransform>();

                elements.Add(rectTransform.GetComponent<BaseUI>());
            }

            return elements.Cast<T>().ToArray();
        }

        public void ScrollBarChangedY()
        {
            elementsTrans.anchoredPosition =
                new Vector2(elementsTrans.anchoredPosition.x, scrollbar.y.value * scrollRange.y);
        }

        public void ScrollBarChangedX()
        {
            elementsTrans.anchoredPosition =
                new Vector2(scrollbar.x.value * scrollRange.x, elementsTrans.anchoredPosition.y);
        }

        [GenerateWrap]
        public struct Parameters
        {
            public int rowCount;
            public Vector2 elementSize;
            public Vector2 elementInterval;
            public string elementName;
            public Vector2 planeSize;

            public Parameters(int rowCount, string elementName, Vector2 elementSize, Vector2 elementInterval,
                Vector2 planeSize)
            {
                this.rowCount = rowCount;
                this.elementSize = elementSize;
                this.elementInterval = elementInterval;
                this.elementName = elementName;
                this.planeSize = planeSize;
            }

            public Parameters(int rowCount, string elementName, (float x, float y) elementSize,
                (float x, float y) elementInterval, (float x, float y) planeSize)
            {
                this.rowCount = rowCount;
                this.elementSize = new Vector2(elementSize.x, elementSize.y);
                this.elementInterval = new Vector2(elementInterval.x, elementInterval.y);
                this.elementName = elementName;
                this.planeSize = new Vector2(planeSize.x, planeSize.y);
            }
        }
    }
}