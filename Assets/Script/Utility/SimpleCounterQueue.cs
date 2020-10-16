using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Thunder.Utility
{
    public class SimpleCounterQueue
    {
        public event Action<int> OnStageCompleted;

        private readonly float[] _TimeLimitQueue;

        public SimpleCounter Counter { get; }
        public int CurStage { private set; get; }

        public SimpleCounterQueue(MonoBehaviour host,SimpleCounter counter, float[] timeLimitQueue)
        {
            Counter = counter;
            _TimeLimitQueue = timeLimitQueue;
            host.StartCoroutine(Count());
        }

        public void Play(int stage=0)
        {
            CurStage = stage;
            Counter.Recount(_TimeLimitQueue[CurStage]);
        }

        public void Stop()
        {
            CurStage = _TimeLimitQueue.Length;
        }

        private IEnumerator Count()
        {
            while (true)
            {
                yield return null;
                if (CurStage == _TimeLimitQueue.Length || !Counter.Completed) continue;
                OnStageCompleted?.Invoke(CurStage);
                CurStage++;
                if(CurStage == _TimeLimitQueue.Length)continue;
                Counter.Recount(_TimeLimitQueue[CurStage]);
            }
        }
    }
}
