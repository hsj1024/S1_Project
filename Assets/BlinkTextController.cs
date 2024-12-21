using TMPro; // TextMeshPro 네임스페이스 추가
using System.Collections;
using UnityEngine;

public class BlinkTextController : MonoBehaviour
{
    public TextMeshProUGUI blinkingText; // TextMeshProUGUI 객체
    public float blinkInterval = 3f; // 깜빡이는 간격 (초)

    private void Start()
    {
        if (blinkingText != null)
        {
            StartCoroutine(BlinkText());
        }
        else
        {
            Debug.LogWarning("Blinking Text GameObject가 설정되지 않았습니다!");
        }
    }

    private IEnumerator BlinkText()
    {
        TextMeshProUGUI textMeshPro = blinkingText.GetComponent<TextMeshProUGUI>();
        if (textMeshPro == null)
        {
            Debug.LogError("blinkingText에 TextMeshProUGUI 컴포넌트가 없습니다.");
            yield break;
        }

        while (true)
        {
            // 텍스트를 켜기
            SetTextAlpha(textMeshPro, 1f);
            yield return new WaitForSeconds(1f); // 3초 동안 켜짐

            // 텍스트를 끄기
            SetTextAlpha(textMeshPro, 0f);
            yield return new WaitForSeconds(1f); // 3초 동안 꺼짐
        }
    }
    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text != null)
        {
            Color color = text.color;
            color.a = alpha; // 알파값 설정
            text.color = color; // 텍스트 색상 업데이트
        }
    }
}
