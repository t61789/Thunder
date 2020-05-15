using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ControllerInput : MonoBehaviour
{
    private const string TABLE_NAME = "input";
    private const string INPUT_ID = "input_id";
    private const string PROPERTY_FIELD = "property_name";
    private const string CONTROL_TYPE_FIELD = "control_type";
    private const string INJECT_TYPE_FIELD = "inject_type";
    private const string KEY_FIELD = "key";
    private const string REQUEST_TYPE = "playerControl";
    private const string SET = "Set";

    public enum ControlType
    {
        stay,
        down,
        up
    }

    public enum InjectType
    {
        mousePosition
    }

    [Serializable]
    public struct ControlStruct
    {
        public int key;
        public ControlType controlType;
        public Control set;

        public ControlStruct(int key, ControlType controlType, Control set)
        {
            this.key = key;
            this.controlType = controlType;
            this.set = set;
        }
    }

    [Serializable]
    public struct InjectStruct
    {
        public InjectType injectType;
        public Inject inject;

        public InjectStruct(InjectType injectType,Inject inject)
        {
            this.injectType = injectType;
            this.inject = inject ?? throw new ArgumentNullException(nameof(inject));
        }
    }

    public bool InstallWhenStart = false;

    [SerializeField]
    private List<ControlStruct> controlStructs = new List<ControlStruct>();
    [SerializeField]
    private List<InjectStruct> injectStructs = new List<InjectStruct>();

    private Controller controller;

    public delegate void Inject(Vector3 vector3);
    public delegate void Control(bool value);

    private void Start()
    {
        if(InstallWhenStart)
            Install();
    }

    public static void AttachTo(GameObject gameObject)
    {
        gameObject.AddComponent<ControllerInput>().Install();
    }

    public static void RemoveFrom(GameObject gameObject)
    {
        gameObject.GetComponent<ControllerInput>().Remove();
    }

    public void Remove()
    {
        Destroy(this);
    }

    private void Install()
    {
        controlStructs.Clear();
        injectStructs.Clear();

        controller = GetComponent<Controller>();

        var i = PublicVar.dataBase[TABLE_NAME].Select( null, new (string, object)[] { (INPUT_ID, controller.InputId) });
        if (i.IsEmpty)
            Debug.LogWarning("No input named " + name + " in database, input will be invalid");

        foreach (var item in i)
        {
            string controlType = item[CONTROL_TYPE_FIELD] as string;
            if (controlType != string.Empty)
            {
                Control set = (Control)Delegate.CreateDelegate(typeof(Control), controller, SET + item[PROPERTY_FIELD] as string);
                ControlStruct controlStruct = new ControlStruct((int)item[KEY_FIELD], (ControlType)Enum.Parse(typeof(ControlType), (string)item[CONTROL_TYPE_FIELD]), set);
                controlStructs.Add(controlStruct);
            }
            else
            {
                Inject inject = (Inject)Delegate.CreateDelegate(typeof(Inject), controller, SET + item[PROPERTY_FIELD] as string);
                InjectStruct injectStruct = new InjectStruct((InjectType)Enum.Parse(typeof(InjectType), (string)item[INJECT_TYPE_FIELD]), inject);
                injectStructs.Add(injectStruct);
            }
        }
    }

    private void Update()
    {
        if (controller == null)
        {
            Remove();
            return;
        }

        foreach (var item in controlStructs)
        {
            bool control = false;
            switch (item.controlType)
            {
                case ControlType.stay:
                    control = PublicVar.control.RequestStay((KeyCode)item.key, REQUEST_TYPE);
                    break;
                case ControlType.down:
                    control = PublicVar.control.RequestDown((KeyCode)item.key, REQUEST_TYPE);
                    break;
                case ControlType.up:
                    control = PublicVar.control.RequestUp((KeyCode)item.key, REQUEST_TYPE);
                    break;
                default:
                    break;
            }
            if (control)
            {
                item.set(true);
                PublicVar.control.Release((KeyCode)item.key, REQUEST_TYPE);
            }
        }

        foreach (var item in injectStructs)
        {
            switch (item.injectType)
            {
                case InjectType.mousePosition:
                    item.inject(Tool.Tools.GetMousePosition());
                    break;
                default:
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        Update();
    }
}
