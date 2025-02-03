using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;

public class RewardedAdManager : MonoBehaviour
{
    private RewardedAd rewardedAd;
    private const string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // 실제 광고 단위 ID로 변경

    public Button rewardAdButton; // 버튼 추가

    void Start()
    {
        // Google Mobile Ads SDK 초기화
        MobileAds.Initialize(initStatus => { });
        LoadRewardedAd();

        // 버튼 이벤트 등록
        if (rewardAdButton != null)
        {
            rewardAdButton.onClick.AddListener(ShowRewardedAd);
        }
    }

    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        // 최신 방식으로 RewardedAd 생성
        RewardedAd.Load(adUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"Failed to load rewarded ad: {error?.GetMessage()}");
                return;
            }

           // Debug.Log("Rewarded ad loaded successfully.");
            rewardedAd = ad;

            // 광고 이벤트 리스너 추가
            rewardedAd.OnAdFullScreenContentClosed += () => LoadRewardedAd(); // 광고 종료 후 다시 로드
        });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show(reward =>
            {
                Debug.Log($"User rewarded with {reward.Amount} {reward.Type}");
                HandleUserEarnedReward();
            });
        }
        else
        {
            Debug.Log("Rewarded ad is not ready yet.");
        }
    }

    private void HandleUserEarnedReward()
    {
      //  Debug.Log("User earned the reward!");
        FindObjectOfType<StatManager>()?.ResetStatsAndRefundPoints();
    }
}
