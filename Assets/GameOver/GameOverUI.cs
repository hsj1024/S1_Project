using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI monstersKilledText;
    public TextMeshProUGUI levelReachedText;
    public TextMeshProUGUI bonusStatsText;
    public TextMeshProUGUI playTimeText; // ���ο� �ؽ�Ʈ ������Ʈ �߰�
    public RectTransform panelRectTransform; // �г��� RectTransform
    public Button returnToMainButton; // �������� ���ư��� ��ư

    public void Initialize()
    {
        int monstersKilled = PlayerPrefs.GetInt("TotalMonstersKilled", 0);
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        float bonusStats = PlayerPrefs.GetFloat("BonusStats", 0f);
        int playTime = PlayerPrefs.GetInt("PlayTime", 0); // �÷���Ÿ�� �������� (�� ����)

        // �÷���Ÿ���� �а� �ʷ� ��ȯ
        int minutes = playTime / 60;
        int seconds = playTime % 60;

        monstersKilledText.text = $"Killed : {monstersKilled}";
        levelReachedText.text = $"Level : {levelReached}";
        bonusStatsText.text = $"Stats : + {bonusStats}";
        playTimeText.text = $"Time : \n{minutes:00}m {seconds:00}s";

        ResetPanelSizeAndPosition();

        // ��ư Ŭ�� �̺�Ʈ ������ �߰�
        if (returnToMainButton != null)
        {
            returnToMainButton.onClick.AddListener(OnReturnToMainButtonClicked);
        }
    }

    private void OnReturnToMainButtonClicked()
    {
        LevelManager.Instance.ReturnToMainScene();
    }

    public void ResetPanelSizeAndPosition()
    {
        if (panelRectTransform != null)
        {
            panelRectTransform.sizeDelta = new Vector2(400, 500);
            panelRectTransform.anchoredPosition = Vector2.zero; // �θ��� �߾ӿ� ��ġ
            panelRectTransform.localScale = Vector3.one; // �������� 1�� ����
        }
    }
}
