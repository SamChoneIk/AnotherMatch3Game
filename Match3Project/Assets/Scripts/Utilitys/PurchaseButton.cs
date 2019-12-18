using System.Collections;
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
                GameManager.Instance.WriteLog("이미 구매한 상품\n");
				GoogleAdmobManager.Instance.DestroyAd();
				return;
            }
        }

        IAPManager.Instance.Purchase(targetProductId);
    }
}