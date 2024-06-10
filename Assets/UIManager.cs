using UnityEngine;

public class UIManager : MonoBehaviour
{
    public RectTransform[] uiElements; // ���� UI ��Ҹ� �迭�� ó��

    void Start()
    {
        foreach (RectTransform rectTransform in uiElements)
        {
            SetRectTransform(rectTransform);
        }
    }

    void SetRectTransform(RectTransform rectTransform)
    {
        // RectTransform ����
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // �߾ӿ� ��ġ
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero; // �θ� ��ü�� ũ�⿡ �°� Ȯ��
    }
}
