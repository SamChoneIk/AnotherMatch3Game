using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class GoogleAdmobManager : MonoBehaviour
{
    private string appID = "ca-app-pub-4570069551723430~8341763913";

    private string adBannerUnitID = "ca-app-pub-4570069551723430/8427138795";
    private string adInterstitialUnitID = "ca-app-pub-4570069551723430/4093497335";

    //private string adBannerTestID = "ca-app-pub-3940256099942544/6300978111";
    //private string adInterstitialTestID = "ca-app-pub-3940256099942544/1033173712";

    public bool isInitialized = false;

    private BannerView bannerView;
    private InterstitialAd interstitialAd;

    private static GoogleAdmobManager instance;
    public static GoogleAdmobManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<GoogleAdmobManager>();

            if (instance == null)
                instance = new GameObject(name: "GoogleAdmobManager").AddComponent<GoogleAdmobManager>();

            return instance;
        }
    }

    public void Awake()
    {
        var findAdmobManager = FindObjectOfType<GoogleAdmobManager>();
        if (findAdmobManager != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void IAPInitializeDelay(bool success)
    {
        if (!success)
        {
            InitializeAdmob();
            return;
        }

        StartCoroutine(WaitIAPInitialize());
    }

    IEnumerator WaitIAPInitialize()
    {
        while (!IAPManager.Instance.isInitialized)
            yield return null;

        InitializeAdmob();
    }

    private void InitializeAdmob()
    {
		if (isInitialized)
			return;

        if (StaticVariables.DestroyAd)
        {
            isInitialized = true;
			return;
        }

        MobileAds.Initialize(appID);

        InitializeBannerView();
        InitializeInterstitialAd();

        isInitialized = true;
    }

    private void InitializeBannerView()
    {
        bannerView = new BannerView(adBannerUnitID, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();

        bannerView.LoadAd(request);
        bannerView.Show();
    }

    private void InitializeInterstitialAd()
    {
        interstitialAd = new InterstitialAd(adInterstitialUnitID);
        AdRequest request = new AdRequest.Builder().Build();

        interstitialAd.LoadAd(request);
        //interstitialAd.OnAdClosed += (sender, e) => GameManager.Instance.WriteLog("광고가 닫힘\n");
        //interstitialAd.OnAdLoaded += (sender, e) => GameManager.Instance.WriteLog("광고가 로드됨\n");
    }

    public void Show()
    {
        StartCoroutine(ShowInterstitialAd());
    }

    private IEnumerator ShowInterstitialAd()
    {
        while (!interstitialAd.IsLoaded())
            yield return null;

        interstitialAd.Show();
    }

    public void DestroyAd()
    {
		if (!StaticVariables.DestroyAd)
			StaticVariables.DestroyAd = true;

        bannerView.Hide();

        bannerView.Destroy();
        interstitialAd.Destroy();
    }
}