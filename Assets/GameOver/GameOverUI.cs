using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI monstersKilledText;
    public TextMeshProUGUI levelReachedText;
    public TextMeshProUGUI bonusStatsText;
    public TextMeshProUGUI playTimeText; // 새로운 텍스트 컴포넌트 추가
    public RectTransform panelRectTransform; // 패널의 RectTransform

    public void Initialize()
    {
        int monstersKilled = PlayerPrefs.GetInt("TotalMonstersKilled", 0);
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        float bonusStats = PlayerPrefs.GetFloat("BonusStats", 0f);
        int playTime = PlayerPrefs.GetInt("PlayTime", 0); // 플레이타임 가져오기 (초 단위)

        // 플레이타임을 분과 초로 변환
        int minutes = playTime / 60;
        int seconds = playTime % 60;

        monstersKilledText.text = $"   Killed : {monstersKilled}";
        levelReachedText.text = $"   Level : {levelReached}";
        bonusStatsText.text = $"   Stats : + {bonusStats}";
        playTimeText.text = $"PlayTime :\n{minutes:00}m {seconds:00}s";

        // 패널 크기 설정
        if (panelRectTransform != null)
        {
            panelRectTransform.sizeDelta = new Vector2(400, 600);
        }
    }
}
