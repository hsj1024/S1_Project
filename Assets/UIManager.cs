using UnityEngine;

public class UIManager : MonoBehaviour
{
    public RectTransform[] uiElements; // 여러 UI 요소를 배열로 처리

    void Start()
    {
        foreach (RectTransform rectTransform in uiElements)
        {
            SetRectTransform(rectTransform);
        }
    }

    void SetRectTransform(RectTransform rectTransform)
    {
        // RectTransform 설정
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 중앙에 배치
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero; // 부모 객체의 크기에 맞게 확장
    }
}
