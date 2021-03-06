﻿using System;
using System.Collections.Generic;
using Thunder.Sys;
using Thunder.Turret;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class BuildShipPanel : BaseUi
    {
        private InputField shipFrameInput;
        private Transform attachPointsTrans;
        private GameObject attachPoint;
        private Image frameImage;

        private ListPlane listPlane;
        private string shipId;
        private BaseUi selectedAttachPoint;
        private readonly Dictionary<BaseUi, Ship.AttachPoint> attachPointInfo = new Dictionary<BaseUi, Ship.AttachPoint>();

        private string[] attachResult;
        private Vector2 imageScale;

        public Ship.CreateShipParam buildResult;

        public delegate void BuildShipCompleteDel(BuildShipPanel buildShipPanel);
        public event BuildShipCompleteDel OnBuildShipComplete;

        protected override void Awake()
        {
            base.Awake();

            shipFrameInput = transform.Find("ShipFrameInput").GetComponent<InputField>();
            frameImage = transform.Find("FrameImage").GetComponent<Image>();
            attachPointsTrans = transform.Find("AttachPoints");
            attachPoint = attachPointsTrans.Find("AttachPoint").gameObject;
            listPlane = transform.Find("listPlane").GetComponent<ListPlane>();
        }

        public override void AfterOpen()
        {
            if (shipFrameInput.text == "")
                return;
            ShowShipFrame();
        }

        public void ShowShipFrame()
        {
            Clear();

            shipId = shipFrameInput.text;

            Ship.AttachPoint[] attachPoints = Ship.GetAttachPoints(shipId);
            Sprite frameSprite = Sys.Stable.ObjectPool.GetPrefab(shipId).GetComponent<SpriteRenderer>().sprite;

            Vector2 baseSize = frameSprite.bounds.size;

            frameImage.sprite = frameSprite;

            Rect rect = frameImage.rectTransform.rect;

            imageScale.x = rect.width / baseSize.x;
            imageScale.y = rect.height / baseSize.y;

            foreach (var item in attachPoints)
            {
                GameObject newAttachPoint = Instantiate(attachPoint);
                newAttachPoint.SetActive(true);
                newAttachPoint.transform.SetParent(attachPointsTrans);
                RectTransform naTrans = (newAttachPoint.transform as RectTransform);
                naTrans.anchoredPosition = item.position * imageScale;

                naTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageScale.x);
                naTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageScale.y);

                BaseUi b = newAttachPoint.GetComponent<BaseUi>();
                b.PointerClick += AttachPointClick;
                attachPointInfo.Add(b, item);
            }

            attachResult = new string[attachPoints.Length];
        }

        private void ClearCurAttachPoints()
        {
            foreach (var item in attachPointInfo.Keys)
                Destroy(item.gameObject);
            attachPointInfo.Clear();
            frameImage.sprite = null;
        }

        public override bool BeforeClose()
        {
            bool result = base.BeforeClose();
            if (result)
            {
                Clear();
                shipFrameInput.text = "";
                shipId = "";
            }
            return result;
        }

        private void Clear()
        {
            listPlane.Clear();
            ClearCurAttachPoints();
            selectedAttachPoint = null;
            frameImage.sprite = null;
        }

        private void AttachPointClick(BaseUi baseUI, PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                baseUI.GetComponent<Image>().sprite = Sys.Stable.ObjectPool.GetPrefab(null, BundleSys.UIBundle, "emptyUI").GetComponent<Image>().sprite;

                baseUI.RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageScale.x);
                baseUI.RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageScale.y);

                attachResult[attachPointInfo[selectedAttachPoint].index] = null;

                return;
            }
            else if (selectedAttachPoint == baseUI)
                return;

            listPlane.Clear();

            selectedAttachPoint = baseUI;
            Ship.AttachPoint info = attachPointInfo[baseUI];
            DataTable select = Sys.Stable.DataBase["turret"].Select(new string[] { "name" }, new (string, object)[] { ("type", info.type) });
            List<Action<BaseUi>> inits = new List<Action<BaseUi>>();

            foreach (var item in select.Rows)
            {
                string turretName = (string)item["name"];

                GameObject prefab = Sys.Stable.ObjectPool.GetPrefab(turretName);
                Sprite sprite = prefab.GetComponent<SpriteRenderer>().sprite;
                inits.Add(x =>
                {
                    x.GetComponent<Image>().sprite = sprite;
                    x.InitRect(UiInitType.FillParent);
                    x.GetComponent<Button>().onClick.AddListener(() => ElementClick(turretName, sprite));
                });
            }

            listPlane.Init(new ListPlane.Parameters(3, "normalButton", (0, 0), (10, 10), (0, RectTrans.rect.height)),inits);
        }

        private void ElementClick(string turretName, Sprite sprite)
        {
            Image i = selectedAttachPoint.GetComponent<Image>();
            Ship.AttachPoint info = attachPointInfo[selectedAttachPoint];
            attachResult[info.index] = turretName;
            i.sprite = sprite;
            i.color = Color.white;

            i.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sprite.bounds.size.x * imageScale.x);
            i.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sprite.bounds.size.y * imageScale.y);
        }

        public void CompleteBuildShip()
        {
            buildResult = new Ship.CreateShipParam(shipId, "player", attachResult, true);

            OnBuildShipComplete?.Invoke(this);

            Sys.Stable.Ui.CloseUi(name);
        }
    }
}
