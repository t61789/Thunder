using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 与ui管理器通信
///		显示
///		隐藏
/// 与自身组件通信
///		可用元素列表
///		搭载点
///		框架图片
/// 创建ship
///		TODO:重复创建飞船
///		调用
/// </summary>
public class BuildShipPanel : BaseUI
{
    private Button setFrameButton;
    private Button confirmButton;
    private Button cancelButton;
    private InputField shipFrameInput;
    private Image frameImage;
    private Transform attachPoints;
    private GameObject attachPoint;
    private ListPlane listPlane;

    private string frameName;

    private BaseUI selectedAttachPoint;
    private List<GameObject> listElements = new List<GameObject>();
    private Dictionary<BaseUI, Ship.AttachPoint> attachPointInfo = new Dictionary<BaseUI, Ship.AttachPoint>();

    private string[] attachResult;

    public override void Awake()
    {
        base.Awake();

        setFrameButton = transform.Find("SetFrameButton").GetComponent<Button>();
        confirmButton = transform.Find("ConfirmButton").GetComponent<Button>();
        cancelButton = transform.Find("CancelButton").GetComponent<Button>();
        shipFrameInput = transform.Find("ShipFrameInput").GetComponent<InputField>();
        frameImage = transform.Find("FrameImage").GetComponent<Image>();
        attachPoints = transform.Find("AttachPoints");
        attachPoint = attachPoints.Find("AttachPoint").gameObject;
        listPlane = transform.Find("ListPlane").GetComponent<ListPlane>();
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

        frameName = shipFrameInput.text;
        Ship frame = PublicVar.objectPool.GetPrefab(frameName).GetComponent<Ship>();
        Sprite tempSprite = frame.GetComponent<SpriteRenderer>().sprite;

        Vector3 extents = tempSprite.bounds.extents;

        frameImage.sprite = tempSprite;
        frameImage.SetNativeSize();

        Rect rect = frameImage.rectTransform.rect;

        float i = (rect.width / 2) / extents.x;
        float j = (rect.height / 2) / extents.y;

        foreach (var item in frame.GetAttachPoints())
        {
            GameObject newAttachPoint = Instantiate(attachPoint);
            newAttachPoint.SetActive(true);
            newAttachPoint.transform.SetParent(attachPoints);

            Vector3 temp = item.position;
            newAttachPoint.GetComponent<RectTransform>().localPosition = new Vector2(temp.x * i, temp.y * j);

            BaseUI b = newAttachPoint.GetComponent<BaseUI>();
            b.PointerClick += AttachPointClick;
            attachPointInfo.Add(b, item);
        }

        attachResult = new string[frame.GetAttachPoints().Count];
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
            frameName = "";
        }
        return result;
    }

    private void Clear()
    {
        listPlane.ClearElements();
        ClearCurAttachPoints();
        listElements.Clear();
        selectedAttachPoint = null;
        frameImage.sprite = null;
        attachResult = null;
    }

    private void AttachPointClick(BaseUI baseUI, PointerEventData eventData)
    {
        selectedAttachPoint = baseUI;
        Ship.AttachPoint info = attachPointInfo[baseUI];
        DataTable select = PublicVar.dataBaseManager["turret"].Select( new string[] { "name" }, new (string, object)[] { ( "type", info.attachType ) });
        List<Action<BaseUI>> inits = new List<Action<BaseUI>>();
        listElements.Clear();
        foreach (var item in select.Rows)
        {
            GameObject prefab = PublicVar.objectPool.GetPrefab((string)item["name"]);
            listElements.Add(prefab);
            Sprite sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            inits.Add(x => x.GetComponent<Image>().sprite = sprite);
        }

        listPlane.Init(
            new ListPlane.Parameters<BaseUI>(
                3,
                "listElement",
                (67,67),
                (10,10),
                (0, rectTrans.rect.height),
                inits));
    }

    private void ElementClick(int index)
    {
        Image i = selectedAttachPoint.GetComponent<Image>();
        Ship.AttachPoint info = attachPointInfo[selectedAttachPoint];
        attachResult[info.index] = listElements[index].name;
        i.sprite = listElements[index].GetComponent<SpriteRenderer>().sprite;
        i.color = Color.white;
        i.SetNativeSize();
    }

    public void CreateShip()
    {
        Ship ship = PublicVar.objectPool.DefaultAlloc<Ship>(frameName);

        for (int i = 0; i < attachResult.Length; i++)
        {
            if (attachResult[i] == null) continue;
            ship.AttachTurret(attachResult[i], i, (string)PublicVar.dataBaseManager.SelectOnce("turret",  "control_class" , new (string, string)[] { ("name", attachResult[i]) }));
        }

        //PublicVar.SetCurPlayer(ship.gameObject);

        AircraftController.AttachTo<Ship>(ship.gameObject);

        PublicVar.uiManager.CloseUI(name);
    }
}
