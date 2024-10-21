using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour
{
    public TextMeshProUGUI introText; // TextMeshPro �ؽ�Ʈ UI
    public Button skipButton;         // SKIP ��ư
    public CanvasGroup canvasGroup;   // ���̵� ��/�ƿ��� ���� CanvasGroup

    // �ؽ�Ʈ ����Ʈ
    private string[] introSentences = {
        "���� �ձ��� �� �� �� ����",
        "��ȭ�Ӵ� ��� ��, \n�������� ������ �ؿԾ�.",
        "���� ���θ��� Ȧ�� ��� \n��¥ �������� �ҳడ\n������ �����ߴٴ� ����",
        "�߸���Ÿ�� ���� \n������ ��Ű�� ������.",
        "�̰� �츮 ������ ���״� \n���� ������ �̾߱��."
    };

    private bool skipIntro = false; // SKIP ��ư�� ���ȴ��� ����

    void Start()
    {
        skipButton.onClick.AddListener(SkipIntro); // SKIP ��ư�� �̺�Ʈ �߰�
        StartCoroutine(PlayIntro());              // ��Ʈ�� ��� ����
    }

    // SKIP ��ư�� ������ �� ȣ��Ǵ� �Լ�
    void SkipIntro()
    {
        skipIntro = true; // ��Ʈ�� ��ŵ �÷��� ����
        SceneManager.LoadScene("Title"); // Ÿ��Ʋ ������ ��ȯ
    }

    // ��Ʈ�� �ؽ�Ʈ�� ������� ���̵� ��/�ƿ���Ű�� �ڷ�ƾ
    IEnumerator PlayIntro()
    {
        foreach (string sentence in introSentences)
        {
            // �ؽ�Ʈ ���� �� ���� ����
            introText.text = sentence;
            yield return StartCoroutine(FadeText(1f, 0.3f)); // ���̵� �� (0.3��)
            yield return new WaitForSeconds(3f);             // �ؽ�Ʈ ���� (1��)
            yield return StartCoroutine(FadeText(0f, 0.3f)); // ���̵� �ƿ� (0.3��)

            // ���� SKIP ��ư�� ���ȴٸ� �ڷ�ƾ �ߴ�
            if (skipIntro)
                yield break;
        }

        // ��Ʈ�ΰ� ������ Ÿ��Ʋ ������ ��ȯ
        SceneManager.LoadScene("Title");
    }

    // �ؽ�Ʈ ���̵� ��/�ƿ� �ڷ�ƾ
    IEnumerator FadeText(float targetAlpha, float duration)
    {
        float startAlpha = introText.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            introText.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        introText.alpha = targetAlpha;
    }
}
