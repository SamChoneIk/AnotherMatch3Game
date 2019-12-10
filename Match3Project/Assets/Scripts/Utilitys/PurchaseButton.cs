﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseButton : MonoBehaviour
{
    public string targetProductId;

    public Text logText;

    public void HandleClick()
    {
        if(targetProductId == IAPManager.productAd)
        {
            if (IAPManager.Instance.HadPurchased(targetProductId))
            {
                logText.text += "이미 구매한 상품\n";

                if (!StaticVariables.isDestroyAd)
                    StaticVariables.isDestroyAd = true;

                return;
            }
        }

        IAPManager.Instance.Purchase(targetProductId);
    }
}