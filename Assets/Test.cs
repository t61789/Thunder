using System;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private VerticalLayoutGroup _LayoutGroup;
    private void Start()
    {
        _LayoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Debug.Log(_LayoutGroup.preferredWidth);
        Debug.Log(_LayoutGroup.flexibleWidth);
    }
}
