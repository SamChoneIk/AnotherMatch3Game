using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class GoogleAdmobManager : MonoBehaviour
{
    public string appID;
    public string adBannerUnitID;
    public string adInterstitialUnitID;

    public Text debugBanner;
    public Text debugInterstitial;

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
                instance = new GameObject(name: "AdmobManager").AddComponent<GoogleAdmobManager>();

            return instance;
        }
    }


    private void Start()
    {
        MobileAds.Initialize(appID);

        InitializeBannerView();
        InitializeInterstitialAd();
        Invoke("Show", 10f);
    }

    private void InitializeBannerView()
    {
        bannerView = new BannerView(adBannerUnitID, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();

        bannerView.LoadAd(request);
        bannerView.Show();
        debugBanner.text += "배너 광고\n";
    }

    private void InitializeInterstitialAd()
    {
        interstitialAd = new InterstitialAd(adInterstitialUnitID);

        AdRequest request = new AdRequest.Builder().Build();

        interstitialAd.LoadAd(request);
        interstitialAd.OnAdClosed += (sender, e) => debugInterstitial.text += "광고가 닫힘\n";
        interstitialAd.OnAdLoaded += (sender, e) => debugInterstitial.text += "광고가 로드됨\n";
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
        bannerView.Destroy();
        interstitialAd.Destroy();
    }
}