using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionWindow : UImenu
{
    public Slider bgmVolume;
    public Slider seVolume;

    public void Update()
    {
        if (manager.currentMenuID == Menus.Option)
            GameManager.Instance.VolumeControl(bgmVolume.value, seVolume.value);
    }

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