using System.Collections.Generic;
using System.Linq;
using Thunder.Tool;
using Thunder.Utility;
using TMPro;
using UnityEngine;

namespace Thunder.UI
{
    public class LogPanel : BaseUI
    {
        #region old code
        //[HideInInspector]
        //public TextMeshProUGUI textMesh;

        //public bool ResizeWithText;
        //public Vector2 Interval;

        //protected override void Awake()
        //{
        //    base.Awake();
        //    textMesh = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        //}

        //private void Resize()
        //{
        //    RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textMesh.rectTransform.rect.width + Interval.x);
        //    RectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textMesh.rectTransform.rect.height + Interval.y);
        //}

        //public string GetText()
        //{
        //    return textMesh.text;
        //}

        //public void SetText(string text)
        //{
        //    textMesh.SetText(text);
        //    Resize();
        //}
        #endregion

        public static LogPanel Instance;

        public int MaxBufferSize;

        private int _LogPointer;
        private CircleBuffer<string> _LogBuffer;
        private CircleBuffer<TextMeshProUGUI> _TextBuffer;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            _LogBuffer = new CircleBuffer<string>(MaxBufferSize);
            var l =
                (from Transform rectTran in RectTrans select rectTran.GetComponent<TextMeshProUGUI>()).ToList();
            l.Reverse();
            _TextBuffer = new CircleBuffer<TextMeshProUGUI>(l.Count);
            foreach (var textMeshProUgui in l)
                _TextBuffer.Insert(textMeshProUgui);
            RePaint(0);
        }

        public void MoveView(bool up)
        {
            _LogPointer += (up ? 1 : -1);
            int diff = _LogBuffer.ElementSize - _TextBuffer.ElementSize;
            _LogPointer = _LogPointer.Clamp(0, diff < 0 ? 0 : diff);
            RePaint(_LogPointer);
        }

        public void MoveViewLimit(bool bottom)
        {
            if (bottom)
                _LogPointer = 0;
            else
            {
                int diff = _LogBuffer.ElementSize - _TextBuffer.ElementSize;
                _LogPointer = _LogBuffer.ElementSize.Clamp(0, diff < 0 ? 0 : diff);
            }
            RePaint(_LogPointer);
        }

        private void RePaint(int startPos)
        {
            int i = 0;
            for (; i < _TextBuffer.ElementSize && (i + startPos) < _LogBuffer.ElementSize; i++)
                _TextBuffer[i].text = _LogBuffer[i + startPos];
            for (; i < _TextBuffer.ElementSize; i++)
                _TextBuffer[i].text = null;
        }

        public void Log(string msg)
        {
            _LogBuffer.Insert(msg, false);
            if (_LogPointer == 0)
                RePaint(0);
            else
            {
                _LogPointer++;
                if (_LogBuffer.ElementSize < _TextBuffer.ElementSize)
                    RePaint(--_LogPointer);
            }
        }

        public void LogSystem(string msg)
        {
            msg = "<color=red>[system]</color>" + msg;
            Log(msg);
        }

        #region guiTest
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(0, 0, 700, 300), "up"))
        //    {
        //        MoveView(true);
        //    }

        //    if (GUI.Button(new Rect(0, 300, 700, 300), "down"))
        //    {
        //        MoveView(false);
        //    }

        //    if (GUI.Button(new Rect(0, 600, 700, 300), "log"))
        //    {
        //        Log();
        //    }
        //}

        //private int count;
        //private void Log()
        //{
        //    Log((count++).ToString());
        //}
        #endregion
    }
}
