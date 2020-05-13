using System.Collections.Generic;
using UnityEngine;

public class ControlManager
{
    private struct Unit
    {
        public string name;
        public int priority;
        public bool release;

        public Unit(string name, int priority)
        {
            this.name = name;
            this.priority = priority;
            release = false;
        }

        public Unit Release()
        {
            Unit temp = new Unit(name, priority);
            release = true;

            return temp;
        }
    }

    public bool Controllable = true;

    public bool FollowCursorable = true;

    private Dictionary<string, Unit> units = new Dictionary<string, Unit>();

    private Dictionary<KeyCode, Stack<Unit>> unitStacks = new Dictionary<KeyCode, Stack<Unit>>();
    private Dictionary<KeyCode, List<Unit>> unitLists = new Dictionary<KeyCode, List<Unit>>();

    [Newtonsoft.Json.JsonObject]
    public struct Values
    {
        public string[] priority;
    }

    public ControlManager()
    {
        Values values = PublicVar.valueManager.LoadValueNormal<Values>("input_priority");

        string[] temp = values.priority;
        for (int i = 0; i < temp.Length; i++)
            units.Add(temp[i], new Unit(temp[i], i));
    }

    public bool Request(KeyCode keyCode, string name)
    {
        if (!Controllable) return false;

        Unit unit = units[name];
        if (!unitLists.TryGetValue(keyCode, out List<Unit> list))
        {
            list = new List<Unit>() { unit};

            unitLists.Add(keyCode, list);
            return true;
        }

        if (list.Count != 0)
        {
            Unit top = list[list.Count - 1];
            if (top.priority < unit.priority)
            {
                list.Add(unit);
                return true;
            }
            else if (top.priority > unit.priority)
                return false;
            else
                return true;
        }
        else
        {
            list.Add(unit);
            return true;
        }
    }

    public bool RequestDown(KeyCode keyCode, string name)
    {
        if (!Input.GetKeyDown(keyCode))
            return false;
        return Request(keyCode, name);
    }

    public bool RequestUp(KeyCode keyCode, string name)
    {
        if (!Input.GetKeyUp(keyCode))
            return false;
        return Request(keyCode, name);
    }

    public bool RequestStay(KeyCode keyCode, string name)
    {
        if (!Input.GetKey(keyCode))
            return false;
        return Request(keyCode, name);
    }

    public void Release(KeyCode keyCode, string name)
    {
        Unit unit = units[name];
        List<Unit> list = unitLists[keyCode];

        if (list[list.Count - 1].priority == unit.priority)
        {
            list[list.Count - 1] = list[list.Count - 1].Release();

            while (list.Count != 0 && list[list.Count - 1].release)
            {
                list.RemoveAt(list.Count - 1);
            }
        }
        else
        {
            int cur = list.FindIndex(x => x.priority == unit.priority);
            if (cur != -1)
            {
                list[cur] = list[cur].Release();
            }
            else
            {
                Debug.LogError("You haven't request key " + keyCode.ToString() + ", but you want to release it");
            }
        }
    }

    public bool RequestMouse(string name)
    {
        if (!Controllable) return false;

        Unit unit = units[name];
        for (int i = (int)KeyCode.Mouse0; i < (int)KeyCode.Mouse6; i++)
        {
            if (unit.priority < unitStacks[(KeyCode)i].Peek().priority)
                return false;
        }

        for (int i = (int)KeyCode.Mouse0; i < (int)KeyCode.Mouse6; i++)
        {
            unitStacks[(KeyCode)i].Push(unit);
        }
        return true;
    }

    public void ReleaseMouse(string name)
    {
        Unit unit = units[name];
        for (int i = (int)KeyCode.Mouse0; i < (int)KeyCode.Mouse6; i++)
        {
            if (unit.priority != unitStacks[(KeyCode)i].Peek().priority)
            {
                Debug.LogError(name + " doesn't own priority of every mouse key");
                return;
            }
        }

        for (int i = (int)KeyCode.Mouse0; i < (int)KeyCode.Mouse6; i++)
        {
            unitStacks[(KeyCode)i].Pop();
        }
    }
}
