using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class IAPManager : MonoBehaviour, IStoreListener
{
    // 프로젝트 내의 사용될 ID
    public const string productAd = "DestroyAd"; // NonConsumable

    // 개발자 콘솔 내에 등록된 ID
    private const string _android_AdID = "addestroy01";

    private IStoreController storeController; // 구매 과정을 제어하는 함수를 제공
    private IExtensionProvider storeExtensionProvider; // 여러 플랫폼을 위한 확장 처리를 제공

    public bool isInitialized => storeController != null && storeExtensionProvider != null;

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

	private void Awake()
	{
		var findIAPManager = FindObjectOfType<IAPManager>();
		if (findIAPManager != this)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);
	}

	public void GoogleLogin(bool success)
    {
		if (success)
            InitUnityIAP();

        GoogleAdmobManager.Instance.IAPInitializeDelay(success);
    }

    private void InitUnityIAP()
    {
        if (isInitialized)
            return;

		//GameManager.Instance.WriteLog("IAP 초기화를 실행합니다.\n");

        // 인앱결제 설정을 빌드할 수있는 설정       // 유니티가 제공하는 스토어 설정
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        // 광고 제거 상품
        builder.AddProduct
            (productAd, ProductType.NonConsumable, new IDs()
            {
                {_android_AdID, GooglePlay.Name}
            });

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
		//GameManager.Instance.WriteLog("유니티 IAP 초기화 성공\n");
        Debug.Log("유니티 IAP 초기화 성공");

        storeController = controller;
        storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
		//GameManager.Instance.WriteLog($"유니티 IAP 초기화 실패{error}\n");
        Debug.LogError($"유니티 IAP 초기화 실패{error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
       //Debug.Log($"구매 성공 - ID : {e.purchasedProduct.definition.id}"); // 구매한 상품의 아이디

        if(e.purchasedProduct.definition.id == productAd)
        {
            GoogleAdmobManager.Instance.DestroyAd();
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        //Debug.LogWarning($"구매 실패 - {i.definition.id}, {p}");
    }

    public void Purchase(string productId)
    {
        if (!isInitialized)
            return;

        // 해당 상품의 id를 반환
        var product = storeController.products.WithID(productId);
        
        if(product != null && product.availableToPurchase)
        {
            //Debug.Log($"구매 시도 - {product.definition.id}");
            storeController.InitiatePurchase(product);
        }

        else
        {
			//GameManager.Instance.WriteLog($"구매 시도 불가 - {productId}\n");
            //Debug.Log($"구매 시도 불가 - {productId}");
        }
    }

    public void RestorePurchase()
    {
        if (!isInitialized)
            return;

        if (Application.platform == RuntimePlatform.Android)
        {
            storeExtensionProvider.GetExtension<IGooglePlayStoreExtensions>().RestoreTransactions(result =>
            {
				if (result)
				{
					if (HadPurchased(productAd))
					{
						GoogleAdmobManager.Instance.DestroyAd();
					}
				}
            });
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
