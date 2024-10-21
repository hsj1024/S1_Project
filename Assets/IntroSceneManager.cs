using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour
{
    public TextMeshProUGUI introText; // TextMeshPro 텍스트 UI
    public Button skipButton;         // SKIP 버튼
    public CanvasGroup canvasGroup;   // 페이드 인/아웃을 위한 CanvasGroup

    // 텍스트 리스트
    private string[] introSentences = {
        "종이 왕국의 한 숲 속 마을",
        "평화롭던 어느 날, \n괴물들이 습격을 해왔어.",
        "마을 변두리에 홀로 살던 \n괴짜 대장장이 소녀가\n습격을 예상했다는 듯이",
        "발리스타를 끌고 \n마을을 지키러 나갔어.",
        "이건 우리 마을을 지켰던 \n작은 영웅의 이야기야."
    };

    private bool skipIntro = false; // SKIP 버튼이 눌렸는지 여부

    void Start()
    {
        skipButton.onClick.AddListener(SkipIntro); // SKIP 버튼에 이벤트 추가
        StartCoroutine(PlayIntro());              // 인트로 재생 시작
    }

    // SKIP 버튼이 눌렸을 때 호출되는 함수
    void SkipIntro()
    {
        skipIntro = true; // 인트로 스킵 플래그 설정
        SceneManager.LoadScene("Title"); // 타이틀 씬으로 전환
    }

    // 인트로 텍스트를 순서대로 페이드 인/아웃시키는 코루틴
    IEnumerator PlayIntro()
    {
        foreach (string sentence in introSentences)
        {
            // 텍스트 변경 및 투명도 설정
            introText.text = sentence;
            yield return StartCoroutine(FadeText(1f, 0.3f)); // 페이드 인 (0.3초)
            yield return new WaitForSeconds(3f);             // 텍스트 노출 (1초)
            yield return StartCoroutine(FadeText(0f, 0.3f)); // 페이드 아웃 (0.3초)

            // 만약 SKIP 버튼이 눌렸다면 코루틴 중단
            if (skipIntro)
                yield break;
        }

        // 인트로가 끝나면 타이틀 씬으로 전환
        SceneManager.LoadScene("Title");
    }

    // 텍스트 페이드 인/아웃 코루틴
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
