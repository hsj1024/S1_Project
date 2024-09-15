using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutroManager : MonoBehaviour
{
    public TextMeshProUGUI outroText; // TextMeshPro 텍스트 UI
    public Button skipButton; // Skip 버튼
    public CanvasGroup textCanvasGroup; // 텍스트만 페이드 효과를 위한 CanvasGroup
    public float fadeDuration = 0.3f; // 페이드 인/아웃 시간
    public float textDisplayDuration = 3.0f; // 텍스트 노출 시간

    private string[] outroLines = new string[]
    {
        "Team FOXI",

        "기획 / 디렉팅:\n김진서\n이은성",

        "프로그래밍:\n최하정\n황서정",

        "아트:\n김서영\n이제은",

        "추가 도움:\n고재현\n성기환",

        "기타 사운드 출처:\nhttps://youtu.be/od0O5D0cPwk?si=RQvmqtrrLbF4gZg0\nhttps://pixabay.com/sound-effects/search/level%20up/\nZapsplat.com\nhttps://freesound.org/\nhttps://soundeffect-lab.info/\nhttps://taira-komori.jpn.org"
    };

    private void Start()
    {
        skipButton.onClick.AddListener(SkipOutro); // Skip 버튼에 이벤트 연결
        textCanvasGroup.blocksRaycasts = false; // 텍스트의 CanvasGroup이 클릭 이벤트를 막지 않도록 설정
        StartCoroutine(PlayOutro()); // 아웃트로 시작
    }

    IEnumerator PlayOutro()
    {
        // 텍스트를 한 묶음씩 보여줌
        foreach (string line in outroLines)
        {
            // 텍스트를 설정하고 페이드 인 시작
            outroText.text = line;
            yield return StartCoroutine(FadeInText());

            // 텍스트가 3초 동안 유지됨
            yield return new WaitForSeconds(textDisplayDuration);

            // 페이드 아웃 시작
            yield return StartCoroutine(FadeOutText());
        }

        // 모든 텍스트가 끝나면 메인 씬으로 전환
        SceneManager.LoadScene("Main");
    }

    // 텍스트 페이드 인 효과
    IEnumerator FadeInText()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration); // 텍스트 페이드 인 효과
            yield return null;
        }
        textCanvasGroup.alpha = 1; // 완전히 표시
    }

    // 텍스트 페이드 아웃 효과
    IEnumerator FadeOutText()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration); // 텍스트 페이드 아웃 효과
            yield return null;
        }
        textCanvasGroup.alpha = 0; // 완전히 사라짐
    }

    // Skip 버튼을 누르면 아웃트로를 스킵하고 메인 씬으로 전환
    public void SkipOutro()
    {
        StopAllCoroutines(); // 아웃트로 재생 중지
        SceneManager.LoadScene("Main"); // 메인 씬으로 바로 전환
    }
}
