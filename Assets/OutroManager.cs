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
    private LevelManager levelManager; // LevelManager 참조 추가
    private StatManager statManager; // StatManager 참조 추가



    private string[] outroLines = new string[]
    {
        "Team FOXI",

        "기획 / 디렉팅\n\n김진서\n이은성",

        "프로그래밍\n\n최하정\n황서정",

        "아트\n\n김서영\n이제은",

        "추가 도움\n\n고재현\n성기환",

        "기타 사운드 출처\n\nPixabay\nZapsplat\nFreesound\nSoundeffect-lab\nTaira-komori\nSuno AI",

        "플레이해 주셔서\n감사합니다",

        "다음 회차에서\n더 강한 적들이\n기다립니다..." 
    };

    private void Start()
    {
        skipButton.onClick.AddListener(SkipOutro); // Skip 버튼에 이벤트 연결
        textCanvasGroup.blocksRaycasts = false; // 텍스트의 CanvasGroup이 클릭 이벤트를 막지 않도록 설정

        // LevelManager 인스턴스 찾기
        levelManager = LevelManager.Instance;

        if (levelManager == null)
        {
            Debug.LogError("LevelManager 인스턴스를 찾을 수 없습니다.");
        }

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

        // 모든 텍스트가 끝나면 보너스 스탯 추가 후 메인 씬으로 전환
        AddBonusStats();
        StatManager.Instance.LoadStatsFromPlayerPrefs();
        GameManager.Instance.IncrementRound(); // 회차 증가
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
        AddBonusStats(); // 스킵 시에도 보너스 스탯 추가
        StatManager.Instance.LoadStatsFromPlayerPrefs();

        GameManager.Instance.IncrementRound(); // 회차 증가
        SceneManager.LoadScene("Main"); // 메인 씬으로 바로 전환
    }

    // 보너스 스탯 추가 메서드
    private void AddBonusStats()
    {
        float currentBonusStats = PlayerPrefs.GetFloat("BonusStats", 0);
        currentBonusStats += 10; // 보너스 스탯 10 추가
        PlayerPrefs.SetFloat("BonusStats", currentBonusStats);
        PlayerPrefs.Save(); // 변경 사항 저장

        Debug.Log($"Bonus Stats added: 10. Total Bonus Stats now: {currentBonusStats}");
    }



}
