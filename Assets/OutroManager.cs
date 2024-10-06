using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutroManager : MonoBehaviour
{
    public TextMeshProUGUI outroText; // TextMeshPro �ؽ�Ʈ UI
    public Button skipButton; // Skip ��ư
    public CanvasGroup textCanvasGroup; // �ؽ�Ʈ�� ���̵� ȿ���� ���� CanvasGroup
    public float fadeDuration = 0.3f; // ���̵� ��/�ƿ� �ð�
    public float textDisplayDuration = 3.0f; // �ؽ�Ʈ ���� �ð�
    private LevelManager levelManager; // LevelManager ���� �߰�
    private StatManager statManager; // StatManager ���� �߰�



    private string[] outroLines = new string[]
    {
        "Team FOXI",

        "��ȹ / ����:\n������\n������",

        "���α׷���:\n������\nȲ����",

        "��Ʈ:\n�輭��\n������",

        "�߰� ����:\n������\n����ȯ",

        "��Ÿ ���� ��ó:\nhttps://youtu.be/od0O5D0cPwk?si=RQvmqtrrLbF4gZg0\nhttps://pixabay.com/sound-effects/search/level%20up/\nZapsplat.com\nhttps://freesound.org/\nhttps://soundeffect-lab.info/\nhttps://taira-komori.jpn.org"
    };

    private void Start()
    {
        skipButton.onClick.AddListener(SkipOutro); // Skip ��ư�� �̺�Ʈ ����
        textCanvasGroup.blocksRaycasts = false; // �ؽ�Ʈ�� CanvasGroup�� Ŭ�� �̺�Ʈ�� ���� �ʵ��� ����

        // LevelManager �ν��Ͻ� ã��
        levelManager = LevelManager.Instance;

        if (levelManager == null)
        {
            Debug.LogError("LevelManager �ν��Ͻ��� ã�� �� �����ϴ�.");
        }

        StartCoroutine(PlayOutro()); // �ƿ�Ʈ�� ����
    }

    IEnumerator PlayOutro()
    {
        // �ؽ�Ʈ�� �� ������ ������
        foreach (string line in outroLines)
        {
            // �ؽ�Ʈ�� �����ϰ� ���̵� �� ����
            outroText.text = line;
            yield return StartCoroutine(FadeInText());

            // �ؽ�Ʈ�� 3�� ���� ������
            yield return new WaitForSeconds(textDisplayDuration);

            // ���̵� �ƿ� ����
            yield return StartCoroutine(FadeOutText());
        }

        // ��� �ؽ�Ʈ�� ������ ���ʽ� ���� �߰� �� ���� ������ ��ȯ
        AddBonusStats();
        StatManager.Instance.LoadStatsFromPlayerPrefs();
        SceneManager.LoadScene("Main");
    }

    // �ؽ�Ʈ ���̵� �� ȿ��
    IEnumerator FadeInText()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration); // �ؽ�Ʈ ���̵� �� ȿ��
            yield return null;
        }
        textCanvasGroup.alpha = 1; // ������ ǥ��
    }

    // �ؽ�Ʈ ���̵� �ƿ� ȿ��
    IEnumerator FadeOutText()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration); // �ؽ�Ʈ ���̵� �ƿ� ȿ��
            yield return null;
        }
        textCanvasGroup.alpha = 0; // ������ �����
    }

    // Skip ��ư�� ������ �ƿ�Ʈ�θ� ��ŵ�ϰ� ���� ������ ��ȯ
    public void SkipOutro()
    {
        StopAllCoroutines(); // �ƿ�Ʈ�� ��� ����
        AddBonusStats(); // ��ŵ �ÿ��� ���ʽ� ���� �߰�
        StatManager.Instance.LoadStatsFromPlayerPrefs();


        SceneManager.LoadScene("Main"); // ���� ������ �ٷ� ��ȯ
    }

    // ���ʽ� ���� �߰� �޼���
    private void AddBonusStats()
    {
        float currentBonusStats = PlayerPrefs.GetFloat("BonusStats", 0);
        currentBonusStats += 10; // ���ʽ� ���� 10 �߰�
        PlayerPrefs.SetFloat("BonusStats", currentBonusStats);
        PlayerPrefs.Save(); // ���� ���� ����

        Debug.Log($"Bonus Stats added: 10. Total Bonus Stats now: {currentBonusStats}");
    }


}
