using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Logo : MonoBehaviour
{
    public Image fadeImage;  // 페이드 아웃을 위한 이미지

    void Start()
    {
        // 처음에 페이드 이미지가 투명하도록 설정
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
        else
        {
            Debug.LogError("Fade Image is not assigned.");
        }
        // 0.3초 후에 씬 전환 시작
        Invoke("StartFadeOut", 2f);
    }

    void StartFadeOut()
    {
        StartCoroutine(FadeOutAndLoadScene());
    }

    IEnumerator FadeOutAndLoadScene()
    {
        float fadeDuration = 0.5f;  // 페이드 아웃 시간
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

        // 페이드 아웃이 끝나면 씬 전환
        SceneManager.LoadScene("Title/Title");
    }
}
