using TMPro; // TextMeshPro ���ӽ����̽� �߰�
using System.Collections;
using UnityEngine;

public class BlinkTextController : MonoBehaviour
{
    public TextMeshProUGUI blinkingText; // TextMeshProUGUI ��ü
    public float blinkInterval = 3f; // �����̴� ���� (��)

    private void Start()
    {
        if (blinkingText != null)
        {
            StartCoroutine(BlinkText());
        }
        else
        {
            Debug.LogWarning("Blinking Text GameObject�� �������� �ʾҽ��ϴ�!");
        }
    }

    private IEnumerator BlinkText()
    {
        TextMeshProUGUI textMeshPro = blinkingText.GetComponent<TextMeshProUGUI>();
        if (textMeshPro == null)
        {
            Debug.LogError("blinkingText�� TextMeshProUGUI ������Ʈ�� �����ϴ�.");
            yield break;
        }

        while (true)
        {
            // �ؽ�Ʈ�� �ѱ�
            SetTextAlpha(textMeshPro, 1f);
            yield return new WaitForSeconds(1f); // 3�� ���� ����

            // �ؽ�Ʈ�� ����
            SetTextAlpha(textMeshPro, 0f);
            yield return new WaitForSeconds(1f); // 3�� ���� ����
        }
    }
    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text != null)
        {
            Color color = text.color;
            color.a = alpha; // ���İ� ����
            text.color = color; // �ؽ�Ʈ ���� ������Ʈ
        }
    }
}
