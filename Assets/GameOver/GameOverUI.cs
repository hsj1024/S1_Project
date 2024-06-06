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

        // �ؽ�Ʈ ���� ���� (����׿�)
        monstersKilledText.color = Color.black;
        levelReachedText.color = Color.black;
        bonusStatsText.color = Color.black;

        // �ؽ�Ʈ�� Canvas�� Sorting Order ����
        Canvas textCanvas = monstersKilledText.GetComponentInParent<Canvas>();
        if (textCanvas != null)
        {
            textCanvas.sortingOrder = 2; // GameOver �г��� Canvas���� ���� ����
        }

        // GameOver �г��� Image ���� ����
        Image gameOverImage = GetComponent<Image>();
        if (gameOverImage != null)
        {
            Color color = gameOverImage.color;
            color.a = 0.5f; // ������ ���� ������ ����
            gameOverImage.color = color;
        }
    }
}
