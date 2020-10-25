using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Tool
{
    // 左摇杆Axis1
    // 右摇杆Axis2

    // 遮蔽值小于给定值时不可获取
    // 注入先来先注入

    // 优先在InputManager中定义的操作中查找，再在已注入的词典中查找，Axis映射到x轴上

    public class ControlSys : MonoBehaviour, IBaseSys
    {
        [HideInInspector] public ShieldValue ShieldValue = new ShieldValue();

        private readonly Dictionary<string, ControlInfo> _BufferKey =
            new Dictionary<string, ControlInfo>();

        private readonly Dictionary<string, ControlInfo> _CusKey =
            new Dictionary<string, ControlInfo>();

        private readonly HashSet<string> _InvalidInputManagerKey =
            new HashSet<string>();

        public static ControlSys Ins { get; private set; }

        public void OnSceneEnter(string preScene, string curScene)
        {
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }

        public ControlInfo RequireKey(string key, float shieldValue)
        {
            if (shieldValue < ShieldValue) return ControlInfo.Default;

            var fromInputManager = ControlInfo.Default;
            if (!_InvalidInputManagerKey.Contains(key))
                try
                {
                    fromInputManager = new ControlInfo(new Vector3(Input.GetAxis(key), 0),
                        Input.GetButton(key),
                        Input.GetButtonDown(key),
                        Input.GetButtonUp(key));
                }
                catch (ArgumentException)
                {
                    _InvalidInputManagerKey.Add(key);
                }

            return _CusKey.TryGetValue(key, out var value) ? KeyConflict(fromInputManager, value) : fromInputManager;
        }

        public void InjectValue(string key, ControlInfo info)
        {
            if (_BufferKey.TryGetValue(key, out var value))
                _BufferKey[key] = KeyConflict(value, info);
            else
                _BufferKey.Add(key, info);
        }

        private static ControlInfo KeyConflict(ControlInfo ctr1, ControlInfo ctr2) // ctr1优先
        {
            if (ctr1.Equals(ControlInfo.Default) && !ctr2.Equals(ControlInfo.Default)) return ctr2;
            return ctr1;
        }

        private void Awake()
        {
            Ins = this;
#if UNITY_STANDALONE || UNITY_EDITOR
            if (_LockCursor) _LockCursor = false;
#endif
        }

        private void Update()
        {
            SpecialInject();
        }

        private void FixedUpdate()
        {
            Update();
        }

        private void LateUpdate()
        {
            var keys = _CusKey.Keys.ToArray();
            foreach (var key in keys)
                _CusKey[key] = ControlInfo.Default;

            foreach (var controlKey in _BufferKey)
                _CusKey.AddOrModify(controlKey.Key, controlKey.Value);
            _BufferKey.Clear();
        }

        private void SpecialInject()
        {
#if UNITY_STANDALONE || UNITY_EDITOR

            // wasd轴 Axis1
            Vector2 dir = default;
            var stay = 0;
            for (var i = 0; i < _CodesArr.Length; i++)
            {
                if (!Input.GetKey(_CodesArr[i])) continue;
                stay++;
                dir += _DirsArr[i];
            }

            dir = dir.normalized;

            var up = _CodesArr.Any(Input.GetKeyUp) && stay == 1;
            var down = _CodesArr.Any(Input.GetKeyDown) && stay == 1;

            InjectValue("Axis1", new ControlInfo(dir, stay > 0, down, up));

            // 鼠标 Axis2
            var cursorAxis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            InjectValue("Axis2", new ControlInfo(cursorAxis, true, false, false));

            // 滚轮 Axis3
            InjectValue("Axis3", new ControlInfo(Input.mouseScrollDelta.y * Vector3.right, true, false, false));
#endif
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        private readonly KeyCode[] _CodesArr = {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D};
        private readonly Vector2[] _DirsArr = {Vector2.up, Vector2.left, Vector2.down, Vector2.right};

        public bool LockCursor
        {
            get => _LockCursor;
            set
            {
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                _LockCursor = value;
            }
        }

        private bool _LockCursor;
#endif
    }

    public struct ControlInfo
    {
        public Vector3 Axis;
        public bool Stay;
        public bool Down;
        public bool Up;

        public static readonly ControlInfo Default = new ControlInfo(default, false, false, false);

        public ControlInfo(Vector3 axis, bool stay, bool down, bool up)
        {
            Axis = axis;
            Stay = stay;
            Down = down;
            Up = up;
        }
    }

    public class ShieldValue
    {
        private float _Value;
        private readonly HashSet<string> _Regist = new HashSet<string>();

        public void Request(string key)
        {
            _Regist.Add(key);
            _Value++;
        }

        public void Release(string key)
        {
            _Regist.Remove(key);
            _Value--;
        }

        public static implicit operator float(ShieldValue s)
        {
            return s._Value;
        }
    }
}