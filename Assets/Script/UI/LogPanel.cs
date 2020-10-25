using System.Linq;
using TMPro;
using Tool;
using UnityEngine;

namespace Thunder.UI
{
    public class LogPanel : BaseUI
    {
        public static LogPanel Ins;

        private int _LogPointer;
        private CircleQueue<string> _LogQueue;
        private TextMeshProUGUI[] _TextQueue;

        public int MaxBufferSize;

        protected override void Awake()
        {
            base.Awake();
            Ins = this;
            _LogQueue = new CircleQueue<string>(MaxBufferSize);
            var l =
                (from Transform rectTran
                        in RectTrans
                    select rectTran.GetComponent<TextMeshProUGUI>()).ToList();
            l.Reverse();
            _TextQueue = l.ToArray();
            RePaint(0);
        }

        public void MoveView(bool up)
        {
            _LogPointer += up ? 1 : -1;
            var diff = _LogQueue.Count - _TextQueue.Length;
            _LogPointer = _LogPointer.Clamp(0, diff < 0 ? 0 : diff);
            RePaint(_LogPointer);
        }

        public void MoveViewLimit(bool bottom)
        {
            if (bottom)
            {
                _LogPointer = 0;
            }
            else
            {
                var diff = _LogQueue.Count - _TextQueue.Length;
                _LogPointer = _LogQueue.Count.Clamp(0, diff < 0 ? 0 : diff);
            }

            RePaint(_LogPointer);
        }

        private void RePaint(int startPos)
        {
            var i = 0;
            for (; i < _TextQueue.Length && i + startPos < _LogQueue.Count; i++)
                _TextQueue[i].text = _LogQueue[i + startPos, true];
            for (; i < _TextQueue.Length; i++)
                _TextQueue[i].text = null;
        }

        public void Log(string msg)
        {
            _LogQueue.Enqueue(msg);
            if (_LogPointer == 0)
            {
                RePaint(0);
            }
            else
            {
                _LogPointer++;
                if (_LogQueue.Count < _TextQueue.Length)
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