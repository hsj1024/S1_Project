using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class BannerAdManager : MonoBehaviour
{
    private BannerView bannerView;
    private const string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // 배너 광고 ID

    void Start()
    {
        // Google Mobile Ads SDK 초기화
        MobileAds.Initialize(initStatus => { });
        LoadBannerAd();
    }

    public void LoadBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        Debug.Log("Creating banner view");
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Top);
        var adRequest = new AdRequest();
        bannerView.LoadAd(adRequest);
    }

    public void DestroyBannerAd()
    {
        if (bannerView != null)
        {
            Debug.Log("Destroying banner ad");
            bannerView.Destroy();
            bannerView = null;
        }
    }
}
