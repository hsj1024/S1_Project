using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI monstersKilledText;
    public TextMeshProUGUI levelReachedText;
    public TextMeshProUGUI bonusStatsText;
    public TextMeshProUGUI playTimeText; // ���ο� �ؽ�Ʈ ������Ʈ �߰�
    public RectTransform panelRectTransform; // �г��� RectTransform

    public void Initialize()
    {
        int monstersKilled = PlayerPrefs.GetInt("TotalMonstersKilled", 0);
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        float bonusStats = PlayerPrefs.GetFloat("BonusStats", 0f);
        int playTime = PlayerPrefs.GetInt("PlayTime", 0); // �÷���Ÿ�� �������� (�� ����)

        // �÷���Ÿ���� �а� �ʷ� ��ȯ
        int minutes = playTime / 60;
        int seconds = playTime % 60;

        monstersKilledText.text = $"   Killed : {monstersKilled}";
        levelReachedText.text = $"   Level : {levelReached}";
        bonusStatsText.text = $"   Stats : + {bonusStats}";
        playTimeText.text = $"PlayTime :\n{minutes:00}m {seconds:00}s";

        // �г� ũ�� ����
        if (panelRectTransform != null)
        {
            panelRectTransform.sizeDelta = new Vector2(400, 600);
        }
    }
}
