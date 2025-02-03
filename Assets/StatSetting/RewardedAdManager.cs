using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;

public class RewardedAdManager : MonoBehaviour
{
    private RewardedAd rewardedAd;
    private const string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // ���� ���� ���� ID�� ����

    public Button rewardAdButton; // ��ư �߰�

    void Start()
    {
        // Google Mobile Ads SDK �ʱ�ȭ
        MobileAds.Initialize(initStatus => { });
        LoadRewardedAd();

        // ��ư �̺�Ʈ ���
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

        // �ֽ� ������� RewardedAd ����
        RewardedAd.Load(adUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"Failed to load rewarded ad: {error?.GetMessage()}");
                return;
            }

           // Debug.Log("Rewarded ad loaded successfully.");
            rewardedAd = ad;

            // ���� �̺�Ʈ ������ �߰�
            rewardedAd.OnAdFullScreenContentClosed += () => LoadRewardedAd(); // ���� ���� �� �ٽ� �ε�
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
