using System;
using System.Collections.Generic;
using UnityEngine;

public class ControllerInput : MonoBehaviour
{
    private const string TABLE_NAME = "input";
    private const string INPUT_ID = "input_id";
    private const string PROPERTY_FIELD = "property_name";
    private const string CONTROL_TYPE_FIELD = "control_type";
    private const string KEY_FIELD = "key";
    private const string REQUEST_TYPE = "playerControl";
    private const string SET = "Set";

    public static bool Controlable = true;

    [Serializable]
    public struct ControlStruct
    {
        public int key;
        public string propName;
        public Act act;
    }

    public bool InstallWhenStart = false;

    [SerializeField]
    private List<ControlStruct> controlStructs = new List<ControlStruct>();

    private Controller controller;

    public delegate void Act(ControlStruct c);

    private AimRing aimRing;

    private void Start()
    {
        if (InstallWhenStart)
            Install(false);
    }

    public static void AttachTo(GameObject gameObject, bool aimRing = false)
    {
        gameObject.AddComponent<ControllerInput>().Install(aimRing);
    }

    public static void RemoveFrom(GameObject gameObject)
    {
        gameObject.GetComponent<ControllerInput>().Remove();
    }

    public void Remove()
    {
        if (aimRing != null)
            PublicVar.uiManager.CloseUI(aimRing);
        Destroy(this);
    }

    private void Install(bool aimRing)
    {
        controlStructs.Clear();

        controller = GetComponent<Controller>();

        var i = PublicVar.dataBase[TABLE_NAME].Select(null, new (string, object)[] { (INPUT_ID, controller.InputId) });
        if (i.IsEmpty)
            Debug.LogWarning("No input named " + name + " in database, input will be invalid");

        foreach (var item in i)
        {
            controlStructs.Add(new ControlStruct()
            {
                key = (int)item[KEY_FIELD],
                propName = (string)item[PROPERTY_FIELD],
                act = (Act)Delegate.CreateDelegate(typeof(Act), this, (string)item[CONTROL_TYPE_FIELD])
            });
        }

        if (aimRing)
            this.aimRing = PublicVar.uiManager.OpenUI<AimRing>("aimRing", UIInitAction.CenterParent, x => x.Init(gameObject.GetComponent<Aircraft>()));
    }

    private void Update()
    {
        if (!Controlable) return;

        if (controller == null)
        {
            Remove();
            return;
        }

        foreach (var item in controlStructs)
            item.act(item);
    }

    private void FixedUpdate()
    {
        if (!Controlable) return;

        Update();
    }

    private void KeyStay(ControlStruct c)
    {
        KeyCode k = (KeyCode)c.key;
        if (PublicVar.control.RequestStay(k, REQUEST_TYPE))
        {
            controller.ControlKeys.SetBool(c.propName,true);
            PublicVar.control.Release(k, REQUEST_TYPE);
        }
    }

    private void KeyDown(ControlStruct c)
    {
        KeyCode k = (KeyCode)c.key;
        if (PublicVar.control.RequestDown(k, REQUEST_TYPE))
        {
            controller.ControlKeys.SetBool(c.propName, true);
            PublicVar.control.Release(k, REQUEST_TYPE);
        }
    }

    private void KeyUp(ControlStruct c)
    {
        KeyCode k = (KeyCode)c.key;
        if (PublicVar.control.RequestUp(k, REQUEST_TYPE))
        {
            controller.ControlKeys.SetBool(c.propName, true);
            PublicVar.control.Release(k, REQUEST_TYPE);
        }
    }

    private void MousePosition(ControlStruct c)
    {
        controller.ControlKeys.SetVector(c.propName, Tool.Tools.GetMousePosition());
    }

    private void VJoystick(ControlStruct c)
    {
        controller.ControlKeys.SetVector(c.propName, Joystick.GetValue(c.key).val);
    }

    private void VJoystickClick(ControlStruct c)
    {
        controller.ControlKeys.SetBool(c.propName, Joystick.GetValue(c.key).click);
    }

    private void VJoystickDoubleClick(ControlStruct c, bool clear)
    {
        controller.ControlKeys.SetBool(c.propName, Joystick.GetValue(c.key).doubleClick);
    }

    private void VJoystickHolding(ControlStruct c, bool clear)
    {
        controller.ControlKeys.SetBool(c.propName, Joystick.GetValue(c.key).holding);
    }
}
