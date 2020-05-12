using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AircraftController : MonoBehaviour
{
    private const string TABLE_NAME = "input";
    private const string CLASS_NAME_FIELD = "class_name";
    private const string PROPERTY_FIELD = "property_name";
    private const string CONTROL_TYPE_FIELD = "control_type";
    private const string INJECT_TYPE_FIELD = "inject_type";
    private const string KEY_FIELD = "key";
    private const string REQUEST_TYPE = "playerControl";

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
        public string propertyName;
        public Set set;

        public ControlStruct(int key, ControlType controlType, string propertyName, Set set)
        {
            this.key = key;
            this.controlType = controlType;
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            this.set = set ?? throw new ArgumentNullException(nameof(set));
        }
    }

    [Serializable]
    public struct InjectStruct
    {
        public InjectType injectType;
        public string propertyName;
        public Inject inject;

        public InjectStruct(InjectType injectType, string propertyName, Inject inject)
        {
            this.injectType = injectType;
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            this.inject = inject ?? throw new ArgumentNullException(nameof(inject));
        }
    }

    [SerializeField]
    private string ClassName = null;

    [SerializeField]
    private List<ControlStruct> controlStructs = new List<ControlStruct>();
    [SerializeField]
    private List<InjectStruct> injectStructs = new List<InjectStruct>();

    private object attachComponent;

    public delegate void Inject(Vector3 vector3);
    public delegate void Set(bool value);

    private void Start()
    {
        if (ClassName != null)
            Install(ClassName);
    }

    public static void AttachTo<T>(GameObject gameObject)
    {
        gameObject.AddComponent<AircraftController>().Install(typeof(T).Name);
    }

    public static void AttachTo(GameObject gameObject, Type t)
    {
        gameObject.AddComponent<AircraftController>().Install(t.Name);
    }

    public static void AttachTo(GameObject gameObject, string t)
    {
        gameObject.AddComponent<AircraftController>().Install(t);
    }

    public static void RemoveFrom(GameObject gameObject)
    {
        gameObject.GetComponent<AircraftController>().Remove();
    }

    public void Remove()
    {
        Destroy(this);
    }

    private void Install(string className)
    {
        controlStructs.Clear();
        injectStructs.Clear();

        Type type = Type.GetType(className);
        attachComponent = Convert.ChangeType(GetComponent(type), type);

        var i = PublicVar.dataBaseManager[TABLE_NAME].Select( null, new (string, object)[] { (CLASS_NAME_FIELD, className) });
        if (i.Rows.Length == 0)
            Debug.LogWarning("No class named " + className + " in database, controller will be unused");

        foreach (var item in i.Rows)
        {
            PropertyInfo info = type.GetProperty((string)item[PROPERTY_FIELD], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (item[CONTROL_TYPE_FIELD] != null)
            {
                Set set = (Set)Delegate.CreateDelegate(typeof(Set), attachComponent, info.SetMethod);
                ControlStruct controlStruct = new ControlStruct((int)item[KEY_FIELD], (ControlType)Enum.Parse(typeof(ControlType), (string)item[CONTROL_TYPE_FIELD]), (string)item[PROPERTY_FIELD], set);
                controlStructs.Add(controlStruct);
            }
            else
            {
                Inject inject = (Inject)Delegate.CreateDelegate(typeof(Inject), attachComponent, info.SetMethod);
                InjectStruct injectStruct = new InjectStruct((InjectType)Enum.Parse(typeof(InjectType), (string)item[INJECT_TYPE_FIELD]), (string)item[PROPERTY_FIELD], inject);
                injectStructs.Add(injectStruct);
            }
        }
    }

    private void Update()
    {
        if (attachComponent == null)
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
                    control = PublicVar.controlManager.RequestStay((KeyCode)item.key, REQUEST_TYPE);
                    break;
                case ControlType.down:
                    control = PublicVar.controlManager.RequestDown((KeyCode)item.key, REQUEST_TYPE);
                    break;
                case ControlType.up:
                    control = PublicVar.controlManager.RequestUp((KeyCode)item.key, REQUEST_TYPE);
                    break;
                default:
                    break;
            }
            if (control)
            {
                item.set(true);
                PublicVar.controlManager.Release((KeyCode)item.key, REQUEST_TYPE);
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

    //private void LateUpdate()
    //{
    //	foreach (var item in controlStructs)
    //		item.set(false);
    //	foreach (var item in injectStructs)
    //	{
    //		switch (item.injectType)
    //		{
    //			case InjectType.mousePosition:
    //				item.inject(Vector3.positiveInfinity);
    //				break;
    //			default:
    //				break;
    //		}
    //	}
    //}
}
