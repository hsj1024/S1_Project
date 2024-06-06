using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI monstersKilledText;
    public TextMeshProUGUI levelReachedText;
    public TextMeshProUGUI bonusStatsText;

    private void Start()
    {
        Time.timeScale = 0f;
        int monstersKilled = PlayerPrefs.GetInt("TotalMonstersKilled", 0);
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        float bonusStats = PlayerPrefs.GetFloat("BonusStats", 0f);

        monstersKilledText.text = $"   Killed : {monstersKilled}";
        levelReachedText.text = $"   Level : {levelReached}";
        bonusStatsText.text = $"   Stats : + {bonusStats}";

        // 텍스트 색상 설정 (디버그용)
        monstersKilledText.color = Color.black;
        levelReachedText.color = Color.black;
        bonusStatsText.color = Color.black;

        // 텍스트의 Canvas와 Sorting Order 설정
        Canvas textCanvas = monstersKilledText.GetComponentInParent<Canvas>();
        if (textCanvas != null)
        {
            textCanvas.sortingOrder = 2; // GameOver 패널의 Canvas보다 높게 설정
        }

        // GameOver 패널의 Image 투명도 설정
        Image gameOverImage = GetComponent<Image>();
        if (gameOverImage != null)
        {
            Color color = gameOverImage.color;
            color.a = 0.5f; // 적절한 투명도 값으로 설정
            gameOverImage.color = color;
        }
    }
}
