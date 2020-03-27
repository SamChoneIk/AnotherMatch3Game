using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseButton : MonoBehaviour
{
    public string targetProductId;

    public void HandleClick()
	{
        if(targetProductId == IAPManager.productAd)
        {
            if (GameManager.Instance.iapMgr.HadPurchased(targetProductId))
            {
                GameManager.Instance.admobMgr.DestroyAd();
				return;
            }
        }

        GameManager.Instance.iapMgr.Purchase(targetProductId);
    }

	public void RestorePurchase()
	{
        GameManager.Instance.iapMgr.RestorePurchase();
	}
}