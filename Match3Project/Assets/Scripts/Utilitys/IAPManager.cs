using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public const string productAd = "addestroy01"; // NonConsumable
    //public const string productSubscription = "premium_subscription"; // Subscription

    private const string _android_AdID = "com.selab.candyexcutematch.addestroy01";
    //private const string _android_PremiumSubID = "com.studio.app.sub";

    private IStoreController storeController; // 구매 과정을 제어하는 함수를 제공
    private IExtensionProvider storeExtensionProvider; // 여러 플랫폼을 위한 확장 처리를 제공

    public bool isInitialized => storeController != null && storeExtensionProvider != null;

    public Text debugText;

    private static IAPManager instance;
    public static IAPManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<IAPManager>();

            if (instance == null)
                instance = new GameObject(name: "IAPManager").AddComponent<IAPManager>();

            return instance;
        }
    }

    private void Start()
    {
        InitUnityIAP();
    }

    private void InitUnityIAP()
    {
        if (isInitialized)
            return;

        // 인앱결제 설정을 빌드할 수있는 설정       // 유니티가 제공하는 스토어 설정
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        // 광고 제거 상품
        builder.AddProduct(productAd, ProductType.NonConsumable,
            new IDs()
            {
                {_android_AdID, GooglePlay.Name}
            }
            );

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        debugText.text = "유니티 IAP 초기화 성공";
        Debug.Log("유니티 IAP 초기화 성공");

        storeController = controller;
        storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        debugText.text = $"유니티 IAP 초기화 실패{error}";
        Debug.LogError($"유니티 IAP 초기화 실패{error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        debugText.text = $"구매 성공 - ID : {e.purchasedProduct.definition.id}";
        Debug.Log($"구매 성공 - ID : {e.purchasedProduct.definition.id}"); // 구매한 상품의 아이디

        if(e.purchasedProduct.definition.id == productAd)
        {
            debugText.text = "광고 제거";
            Debug.Log("광고 제거");
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        debugText.text = $"구매 실패 - {i.definition.id}, {p}";
        Debug.LogWarning($"구매 실패 - {i.definition.id}, {p}");
    }

    public void Purchase(string productId)
    {
        if (!isInitialized)
            return;

        // 해당 상품의 id를 반환
        var product = storeController.products.WithID(productId);
        
        if(product != null && product.availableToPurchase)
        {
            debugText.text = $"구매 시도 - {product.definition.id}";
            Debug.Log($"구매 시도 - {product.definition.id}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            debugText.text = $"구매 시도 불가 - {productId}";
            Debug.Log($"구매 시도 불가 - {productId}");
        }
    }

    public void RestorePurchase()
    {
        if (!isInitialized)
            return;

        if(Application.platform == RuntimePlatform.Android)
        {
            debugText.text = "구매 복구 시도";
            Debug.Log("구매 복구 시도");
        }
    }

    // 구매복구시도
    public bool HadPurchased(string productId)
    {
        if (!isInitialized)
            return false;

        var product = storeController.products.WithID(productId);

        if(product != null)
        {
            // 영수증이 있다.
            return product.hasReceipt;
        }

        return false;
    }
}
