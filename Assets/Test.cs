using DG.Tweening;
using Thunder.Tool;
using Thunder.UI;
using UnityEngine;

public class Test : BaseUI
{
    private bool _Set;

    private float nope;
    private Sequence se;

    private void Start()
    {
        Sequence se = DOTween.Sequence();
        se.SetAutoKill(false);
        Tweener t = RectTrans.DOFixedSize(new Vector2(500, 200), 1);
        se.Append(t);
        t = RectTrans.DOFixedSize(new Vector2(0, 0), 5);
        se.Append(t);

        se.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_Set && Time.time > 3)
        {
            //se.();
            //se.Rewind();
            //se.Play();
            _Set = true;
        }
    }
}
