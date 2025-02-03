using UnityEngine;
using TMPro;

public class RoundText : MonoBehaviour
{
    public TextMeshProUGUI roundText; // TextMeshPro를 연결

    void Start()
    {
        // PlayerPrefs에서 현재 회차 정보를 가져옴
        int currentRound = PlayerPrefs.GetInt("CurrentRound", 1); // 기본값 1
        roundText.text = $"{currentRound} 회차"; // 텍스트 설정
    }
}
