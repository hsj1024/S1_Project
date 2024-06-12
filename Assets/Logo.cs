using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Logo : MonoBehaviour
{
    public Image fadeImage;  // ���̵� �ƿ��� ���� �̹���

    void Start()
    {
        // ó���� ���̵� �̹����� �����ϵ��� ����
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
        else
        {
            Debug.LogError("Fade Image is not assigned.");
        }
        // 0.3�� �Ŀ� �� ��ȯ ����
        Invoke("StartFadeOut", 2f);
    }

    void StartFadeOut()
    {
        StartCoroutine(FadeOutAndLoadScene());
    }

    IEnumerator FadeOutAndLoadScene()
    {
        float fadeDuration = 0.5f;  // ���̵� �ƿ� �ð�
        float fadeSpeed = 1.0f / fadeDuration;
        Color fadeColor = fadeImage.color;

        for (float t = 0; t < 1.0f; t += Time.deltaTime * fadeSpeed)
        {
            fadeColor.a = Mathf.Lerp(0, 1, t);
            fadeImage.color = fadeColor;
            yield return null;
        }

        fadeColor.a = 1;
        fadeImage.color = fadeColor;

        // ���̵� �ƿ��� ������ �� ��ȯ
        SceneManager.LoadScene("Title/Title");
    }
}
