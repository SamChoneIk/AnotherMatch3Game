using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseWindow : UImenu
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void OnFocus()
    {
        firstSelected = gameObject;
        base.OnFocus();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }
}
