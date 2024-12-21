

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationController : MonoBehaviour
{
    public GameObject firstImage; // 1�� �̹���
    public GameObject secondImage; // 2�� �̹���
    public GameObject thirdImage; // 3�� �̹���

    public float fadeDuration = 0.5f; // Fade-in ȿ�� �ð�
    public float moveDuration = 3f; // x��ǥ�� �̵��ϴ� �ð�
    public float repeatDuration = 3f; // �ݺ� �ִϸ��̼� �ð�
    public string targetSceneName = "Main/Main"; // �̵��� �� �̸�

    private Vector3 initialPosition1; // 1�� �ʱ� ��ġ
    private Vector3 initialPosition3; // 3�� �ʱ� ��ġ
    private bool isRepeating = true;

    void Start()
    {
        // �ʱ� ��ġ ����
        if (firstImage != null)
            initialPosition1 = firstImage.transform.localPosition;

        if (thirdImage != null)
            initialPosition3 = thirdImage.transform.localPosition;

        // �ִϸ��̼� ����
        StartCoroutine(PlayTitleAnimations());
    }

    private void Update()
    {
        // ȭ�� ��ġ�ϸ� �� �̵�
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("��ġ ����: �� �̵� �غ�");
            isRepeating = false;

            // �� �̵�
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log($"�� �̵�: {targetSceneName}");
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError("�̵��� �� �̸��� �������� �ʾҽ��ϴ�!");
            }
        }
    }

    private IEnumerator PlayTitleAnimations()
    {
        // 1, 2, 3�� �̹��� ���� Fade-in
        yield return StartCoroutine(FadeInImages());

        // 1���� 3�� �̹����� ���ÿ� �̵�
        yield return StartCoroutine(MoveImagesSimultaneously(initialPosition1 + new Vector3(-6f, 0, 0), 8f, firstImage, initialPosition3 + new Vector3(-6f, 0, 0), 8f, thirdImage, moveDuration));

        // �ݺ� �ִϸ��̼� ����
        while (isRepeating)
        {
            yield return StartCoroutine(MoveImagesSimultaneously(initialPosition1 + new Vector3(-6f, 0, 0), 8f, firstImage,
                initialPosition3 + new Vector3(6f, 0, 0), -6f, thirdImage,
                repeatDuration
            ));
        }
    }

    private IEnumerator MoveImagesSimultaneously(Vector3 startPos1, float xOffset1, GameObject obj1,
                                                 Vector3 startPos2, float xOffset2, GameObject obj2,
                                                 float duration)
    {
        if (obj1 == null || obj2 == null) yield break;

        float timer = 0f;

        while (true)
        {
            // PingPong�� ����Ͽ� �ε巴�� �ݺ�
            float t = Mathf.PingPong((timer / duration)*0.5f, 1f); // 0 �� 1 �� 0���� �ε巴�� ��ȯ
            Vector3 targetPos1 = startPos1 + new Vector3(xOffset1 * t, 0, 0);
            Vector3 targetPos2 = startPos2 + new Vector3(xOffset2 * t, 0, 0);

            obj1.transform.localPosition = targetPos1;
            obj2.transform.localPosition = targetPos2;

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeInImages()
    {
        // Sprite �ʱ� ���� (�����ϰ� ����)
        SetupSpriteAlpha(firstImage, 0f);
        SetupSpriteAlpha(secondImage, 0f);
        SetupSpriteAlpha(thirdImage, 0f);

        float timer = 0f;

        while (timer < fadeDuration) // fadeDuration ���� �ݺ�
        {
            float alpha = timer / fadeDuration; // ���İ��� 0 �� 1�� ������ ����
            SetSpriteAlpha(firstImage, alpha);
            SetSpriteAlpha(secondImage, alpha);
            SetSpriteAlpha(thirdImage, alpha);

            timer += Time.deltaTime;
            yield return null; // ���� �����ӱ��� ���
        }

        // ���������� ���İ��� 1�� ����
        SetSpriteAlpha(firstImage, 1f);
        SetSpriteAlpha(secondImage, 1f);
        SetSpriteAlpha(thirdImage, 1f);
    }

    private void SetupSpriteAlpha(GameObject obj, float alpha)
    {
        if (obj != null)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>(); // GameObject���� SpriteRenderer ��������
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = alpha; // ���İ� ����
                renderer.color = color; // ���� ������Ʈ
            }
        }
    }

    private void SetSpriteAlpha(GameObject obj, float alpha)
    {
        if (obj != null)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>(); // GameObject���� SpriteRenderer ��������
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = alpha; // ���İ� ����
                renderer.color = color; // ���� ������Ʈ
            }
        }
    }
}

