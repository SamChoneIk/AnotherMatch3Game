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

		GameManager.Instance.WriteLog("IAP 초기화가 완료되었습니다.\n");

        InitializeAdmob();
    }

    private void InitializeAdmob()
    {
		if (isInitialized)
		{
			GameManager.Instance.WriteLog("Admob 초기화가 되어있습니다.\n");
			return;
		}

		GameManager.Instance.WriteLog("Admob 초기화를 실행합니다.\n");

        if (StaticVariables.DestroyAd)
        {
			GameManager.Instance.WriteLog("광고가 제거되었습니다.\n");
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

		GameManager.Instance.WriteLog("배너광고 초기화가 완료되었습니다.\n");
    }

    private void InitializeInterstitialAd()
    {
        interstitialAd = new InterstitialAd(adInterstitialUnitID);
        AdRequest request = new AdRequest.Builder().Build();

        interstitialAd.LoadAd(request);
        interstitialAd.OnAdClosed += (sender, e) => GameManager.Instance.WriteLog("광고가 닫힘\n");
        interstitialAd.OnAdLoaded += (sender, e) => GameManager.Instance.WriteLog("광고가 로드됨\n");

		GameManager.Instance.WriteLog("전면광고 초기화가 완료되었습니다.\n");
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