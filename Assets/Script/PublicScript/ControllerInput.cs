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

    public enum ControlType
    {
        keyStay,
        keyDown,
        keyUp,
        mousePosition,
        vJoystick,
        vJoystickClick,
        vJoystickDoubleClick,
        vJoystickHolding
    }

    [Serializable]
    public struct ControlStruct
    {
        public int key;
        public ControlType controlType;
        public Control control;
        public Inject inject;
        public Act act;
    }

    public bool InstallWhenStart = false;

    [SerializeField]
    private List<ControlStruct> controlStructs = new List<ControlStruct>();

    private Controller controller;

    public delegate void Inject(Vector3 vector3);
    public delegate void Control(bool value);
    public delegate void Act(ControlStruct c, bool clear);

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
            controlStructs.Add(GetControlStruct((int)item[KEY_FIELD], (ControlType)Enum.Parse(typeof(ControlType), (string)item[CONTROL_TYPE_FIELD]), item[PROPERTY_FIELD] as string));

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
            item.act(item, false);
    }

    private void LateUpdate()
    {
        if (!Controlable) return;

        if (controller == null)
        {
            Remove();
            return;
        }

        foreach (var item in controlStructs)
            item.act(item, true);
    }

    public ControlStruct GetControlStruct(int key, ControlType controlType, string fieldName)
    {
        ControlStruct result = new ControlStruct
        {
            key = key,
            controlType = controlType
        };
        switch (controlType)
        {
            case ControlType.keyStay:
                result.control = (Control)Delegate.CreateDelegate(typeof(Control), controller, SET + fieldName);
                result.act = KeyStay;
                break;
            case ControlType.keyDown:
                result.control = (Control)Delegate.CreateDelegate(typeof(Control), controller, SET + fieldName);
                result.act = KeyDown;
                break;
            case ControlType.keyUp:
                result.control = (Control)Delegate.CreateDelegate(typeof(Control), controller, SET + fieldName);
                result.act = KeyUp;
                break;
            case ControlType.mousePosition:
                result.inject = (Inject)Delegate.CreateDelegate(typeof(Inject), controller, SET + fieldName);
                result.act = MousePosition;
                break;
            case ControlType.vJoystick:
                result.inject = (Inject)Delegate.CreateDelegate(typeof(Inject), controller, SET + fieldName);
                result.act = VJoystick;
                break;
            case ControlType.vJoystickClick:
                result.control = (Control)Delegate.CreateDelegate(typeof(Control), controller, SET + fieldName);
                result.act = VJoystickClick;
                break;
            case ControlType.vJoystickDoubleClick:
                result.control = (Control)Delegate.CreateDelegate(typeof(Control), controller, SET + fieldName);
                result.act = VJoystickDoubleClick;
                break;
            case ControlType.vJoystickHolding:
                result.control = (Control)Delegate.CreateDelegate(typeof(Control), controller, SET + fieldName);
                result.act = VJoystickHolding;
                break;
            default:
                break;
        }
        return result;
    }

    private void FixedUpdate()
    {
        if (!Controlable) return;

        Update();
    }

    private void KeyStay(ControlStruct c, bool clear)
    {
        if (clear)
        {
            c.control(false);
            return;
        }

        KeyCode k = (KeyCode)c.key;
        if (PublicVar.control.RequestStay(k, REQUEST_TYPE))
        {
            c.control(true);
            PublicVar.control.Release(k, REQUEST_TYPE);
        }
    }

    private void KeyDown(ControlStruct c, bool clear)
    {
        if (clear)
        {
            c.control(false);
            return;
        }

        KeyCode k = (KeyCode)c.key;
        if (PublicVar.control.RequestDown(k, REQUEST_TYPE))
        {
            c.control(true);
            PublicVar.control.Release(k, REQUEST_TYPE);
        }
    }

    private void KeyUp(ControlStruct c, bool clear)
    {
        if (clear)
        {
            c.control(false);
            return;
        }

        KeyCode k = (KeyCode)c.key;
        if (PublicVar.control.RequestUp(k, REQUEST_TYPE))
        {
            c.control(true);
            PublicVar.control.Release(k, REQUEST_TYPE);
        }
    }

    private void MousePosition(ControlStruct c, bool clear)
    {
        if (clear)
        {
            c.inject(Vector3.positiveInfinity);
            return;
        }

        c.inject(Tool.Tools.GetMousePosition());
    }

    private void VJoystick(ControlStruct c, bool clear)
    {
        if (clear)
        {
            c.inject(Vector3.positiveInfinity);
            return;
        }

        c.inject(Joystick.GetValue(c.key).val);
    }

    private void VJoystickClick(ControlStruct c, bool clear)
    {
        if (clear)
        {
            c.control(false);
            return;
        }

        c.control(Joystick.GetValue(c.key).click);
    }

    private void VJoystickDoubleClick(ControlStruct c, bool clear)
    {
        if (clear)
        {
            c.control(false);
            return;
        }

        c.control(Joystick.GetValue(c.key).doubleClick);
    }

    private void VJoystickHolding(ControlStruct c, bool clear)
    {
        if (clear)
        {
            c.control(false);
            return;
        }

        c.control(Joystick.GetValue(c.key).holding);
    }
}
