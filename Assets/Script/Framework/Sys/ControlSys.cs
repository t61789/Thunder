using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Framework
{
    // 左摇杆Axis1
    // 右摇杆Axis2

    // 遮蔽值小于给定值时不可获取
    // 注入先来先注入

    // 优先在InputManager中定义的操作中查找，再在已注入的词典中查找，Axis映射到x轴上

    public class ControlSys : MonoBehaviour, IBaseSys
    {
        public static ShieldValue ShieldValue = new ShieldValue();
        private static readonly Queue<KeyPair> _BufferKey = new Queue<KeyPair>();
        private static readonly Dictionary<string, ControlInfo> _CurControlInfos = new Dictionary<string, ControlInfo>();
        private static readonly HashSet<string> _InvalidInputManagerKey = new HashSet<string>();
        private static ControlSys _Ins;
        private static bool _Updated;

        private static readonly KeyCode[] _CodesArr = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
        private static readonly Vector2[] _DirsArr = { Vector2.up, Vector2.left, Vector2.down, Vector2.right };

        public static bool LockCursor
        {
            get => _LockCursor;
            set
            {
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                _LockCursor = value;
            }
        }
        private static bool _LockCursor;

        private void Awake()
        {
            if (_Ins != null)
                throw new InitDuplicatelyException();
            _Ins = this;

            if (Platform.IsStandalone() && _LockCursor) _LockCursor = false;
        }

        private void Update()
        {
            SpecialInject();

            if (_Updated)
            {
                _Updated = false;
                return;
            }

            UpdateControlInfo();
        }

        private void FixedUpdate()
        {
            _Updated = true;
            UpdateControlInfo();
        }

        /// <summary>
        /// 获取指定key对应的输入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="shieldValue">如果当前遮蔽值大于给定遮蔽值，则会返回空输入</param>
        /// <returns></returns>
        public static ControlInfo RequireKey(string key, float shieldValue)
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

            return _CurControlInfos.TryGetValue(key, out var value) ? KeyConflict(fromInputManager, value) : fromInputManager;
        }
        
        public static void InjectValue(string key, ControlInfo info)
        {
            _BufferKey.Enqueue(new KeyPair(key,info));
        }

        private static void UpdateControlInfo()
        {
            _CurControlInfos.Clear();

            while (_BufferKey.Count != 0)
            {
                var pair = _BufferKey.Dequeue();
                if (_CurControlInfos.TryGetValue(pair.Key,out var info))
                    _CurControlInfos[pair.Key] = KeyConflict(info, pair.ControlInfo);
                else
                    _CurControlInfos.Add(pair.Key, pair.ControlInfo);
            }
        }

        private static ControlInfo KeyConflict(ControlInfo ctr1, ControlInfo ctr2) // ctr1优先
        {
            if (ctr1.Equals(ControlInfo.Default) && !ctr2.Equals(ControlInfo.Default)) return ctr2;
            return ctr1;
        }

        private static void SpecialInject()
        {
            if (Platform.IsStandalone())
            {
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
            }
        }

        public void OnSceneEnter(string preScene, string curScene) { }

        public void OnSceneExit(string curScene) { }

        public void OnApplicationExit() { }

        private readonly struct KeyPair
        {
            public readonly string Key;
            public readonly ControlInfo ControlInfo;

            public KeyPair(string key, ControlInfo controlInfo)
            {
                Key = key;
                ControlInfo = controlInfo;
            }
        }
    }

    public struct ControlInfo
    {
        public Vector3 Axis;
        public bool Stay;
        public bool Down;
        public bool Up;
        public bool Click;
        public bool DoubleClick;

        public static readonly ControlInfo Default = new ControlInfo(default, false, false, false);

        public ControlInfo(Vector3 axis, bool stay, bool down, bool up)
        {
            Axis = axis;
            Stay = stay;
            Down = down;
            Up = up;
            Click = false;
            DoubleClick = false;
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