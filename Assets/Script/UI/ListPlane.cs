using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.PublicScript;
using Assets.Script.System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.UI
{
    public class ListPlane : BaseUi
    {
        public struct Parameters<T> where T : BaseUi
        {
            public int rowCount;
            public Vector2 elementSize;
            public Vector2 elementInterval;
            public string elementName;
            public Vector2 planeSize;
            public List<Action<T>> inits;

            public Parameters(int rowCount, string elementName, (float x, float y) elementSize, (float x, float y) elementInterval, (float x, float y) planeSize, List<Action<T>> inits)
            {
                this.rowCount = rowCount;
                this.elementSize = new Vector2(elementSize.x, elementSize.y);
                this.elementInterval = new Vector2(elementInterval.x, elementInterval.y);
                this.elementName = elementName;
                this.planeSize = new Vector2(planeSize.x, planeSize.y);
                this.inits = inits;
            }
        }

        protected const string ELEMENT = "element";
        protected Vector2 scrollRange;
        protected (Scrollbar x, Scrollbar y) scrollbar;
        protected RectTransform maskTrans;
        protected RectTransform elementsTrans;

        protected List<BaseUi> elements = new List<BaseUi>();
        protected Queue<RectTransform> elementContainers = new Queue<RectTransform>();

        protected override void Awake()
        {
            base.Awake();
            scrollbar.y = RectTrans.Find("ScrollbarY").GetComponent<Scrollbar>();
            scrollbar.x = RectTrans.Find("ScrollbarX").GetComponent<Scrollbar>();
            maskTrans = RectTrans.Find("Mask").GetComponent<RectTransform>();
            elementsTrans = maskTrans.Find("Elements").GetComponent<RectTransform>();
        }

        public T[] Init<T>(Parameters<T> arg) where T : BaseUi
        {
            Clear();
            return CreateElements(arg);
        }

        public void Clear()
        {
            scrollbar.y.size = 1;
            scrollbar.y.value = 0;
            scrollbar.x.size = 1;
            scrollbar.x.value = 0;

            foreach (var item in elements)
                System.System.objectPool.Recycle(item);
            elements.Clear();

            foreach (RectTransform item in elementsTrans)
            {
                item.gameObject.SetActive(false);
                elementContainers.Enqueue(item);
            }
        }

        protected T[] CreateElements<T>(Parameters<T> parameters) where T : BaseUi
        {
            if (parameters.elementSize.x == 0)
            {
                parameters.elementSize.x = (maskTrans.rect.width - (parameters.rowCount - 1) * parameters.elementInterval.x) / parameters.rowCount;
                parameters.planeSize.x = 0;
            }
            if (parameters.elementSize.y == 0)
                parameters.elementSize.y = parameters.elementSize.x;

            scrollbar.x.gameObject.SetActive(parameters.planeSize.x != 0f);
            scrollbar.y.gameObject.SetActive(parameters.planeSize.y != 0f);

            elementsTrans.anchoredPosition = Vector3.zero;

            Vector2 interval = new Vector2(parameters.elementInterval.x + parameters.elementSize.x, parameters.elementInterval.y + parameters.elementSize.y);
            int temp = (parameters.inits.Count - 1) / parameters.rowCount + 1;
            Vector2 planeSize = new Vector2(parameters.rowCount * parameters.elementSize.x + parameters.elementInterval.x * (parameters.rowCount - 1),
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
                RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, planeSize.x + maskTrans.offsetMin.x - maskTrans.offsetMax.x);

            if (scrollbar.y.gameObject.activeSelf)
            {
                RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parameters.planeSize.y);
                scrollRange.y = planeSize.y - maskTrans.rect.height;
                scrollRange.y = scrollRange.y < 0 ? 0 : scrollRange.y;
                scrollbar.y.value = 0;
                scrollbar.y.size = maskTrans.rect.height / planeSize.y;
            }
            else
                RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, planeSize.y + maskTrans.offsetMin.y - maskTrans.offsetMax.y);

            float tempx = parameters.elementSize.x / 2;
            float tempy = parameters.elementSize.y / 2;
            for (int i = 0; i < parameters.inits.Count; i++)
            {
                int x = i % parameters.rowCount;
                int y = i / parameters.rowCount;
                Vector2 position = new Vector2(x * interval.x + tempx, -y * interval.y - tempy);

                RectTransform elementContainer;
                if (elementContainers.Count != 0)
                {
                    elementContainer = elementContainers.Dequeue();
                    elementContainer.gameObject.SetActive(true);
                }
                else
                    elementContainer = new GameObject(ELEMENT).AddComponent<RectTransform>();

                elementContainer.anchorMax = Vector2.zero;
                elementContainer.anchorMin = Vector2.zero;
                elementContainer.SetParent(elementsTrans);
                elementContainer.anchoredPosition = position;
                elementContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parameters.elementSize.x);
                elementContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parameters.elementSize.y);

                RectTransform rectTransform = System.System.objectPool.Alloc(null,UiSys.DefaultUiBundle, parameters.elementName, parameters.inits[i], elementContainer).GetComponent<RectTransform>();

                elements.Add(rectTransform.GetComponent<BaseUi>());
            }

            return elements.Cast<T>().ToArray();
        }

        public void ScrollBarChangedY()
        {
            elementsTrans.anchoredPosition = new Vector2(elementsTrans.anchoredPosition.x, scrollbar.y.value * scrollRange.y);
        }

        public void ScrollBarChangedX()
        {
            elementsTrans.anchoredPosition = new Vector2(scrollbar.x.value * scrollRange.x, elementsTrans.anchoredPosition.y);
        }
    }
}
