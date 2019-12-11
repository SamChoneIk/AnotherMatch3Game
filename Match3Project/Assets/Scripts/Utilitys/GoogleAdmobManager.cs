using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class GoogleAdmobManager : MonoBehaviour
{
    private string appID;
    public string AppID
    {
        get
        {
            appID = "ca-app-pub-4570069551723430~8341763913";
            return appID;
        }
    }

    private string adBannerUnitID;
    public string AdBannerUnitID
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            adBannerUnitID = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_ANDROID
            adBannerUnitID = "ca-app-pub-4570069551723430/8427138795";
#endif
            return adBannerUnitID;
        }
    }

    private string adInterstitialUnitID;
    public string AdInterstitialUnitID
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            adInterstitialUnitID = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_ANDROID
            adInterstitialUnitID = "ca-app-pub-4570069551723430/4093497335";
#endif
            return adInterstitialUnitID;
        }
    }

    public Text logText;
    private BannerView bannerView;
    private InterstitialAd interstitialAd;

    public bool isInitialized = false;

    private static GoogleAdmobManager instance;
    public static GoogleAdmobManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<GoogleAdmobManager>();

            if (instance == null)
                instance = new GameObject(name: "AdmobManager").AddComponent<GoogleAdmobManager>();

            return instance;
        }
    }

    private void Start()
    {
        StartCoroutine(IAPInitializeDelay());
    }

    IEnumerator IAPInitializeDelay()
    {
        while(!IAPManager.Instance.isInitialized)
            yield return null;

        logText.text += "IAP 초기화가 완료되었습니다.\n";
        InitializeAdmob();
    }

    private void InitializeAdmob()
    {
        logText.text += "Admob 초기화를 실행합니다.\n";
        if (StaticVariables.DestroyAd)
        {
            logText.text += "광고가 제거되었습니다.\n";
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
        bannerView = new BannerView(AdBannerUnitID, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();

        bannerView.LoadAd(request);
        bannerView.Show();
        logText.text += "배너광고 초기화가 완료되었습니다.\n";
    }

    private void InitializeInterstitialAd()
    {
        interstitialAd = new InterstitialAd(AdInterstitialUnitID);

        AdRequest request = new AdRequest.Builder().Build();

        interstitialAd.LoadAd(request);
        interstitialAd.OnAdClosed += (sender, e) => logText.text += "광고가 닫힘\n";
        interstitialAd.OnAdLoaded += (sender, e) => logText.text += "광고가 로드됨\n";

        logText.text += "전면광고 초기화가 완료되었습니다.\n";
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
        StaticVariables.DestroyAd = true;
        bannerView.Hide();

        bannerView.Destroy();
        interstitialAd.Destroy();
    }
}