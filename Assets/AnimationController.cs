

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationController : MonoBehaviour
{
    public GameObject firstImage; // 1번 이미지
    public GameObject secondImage; // 2번 이미지
    public GameObject thirdImage; // 3번 이미지

    public float fadeDuration = 0.5f; // Fade-in 효과 시간
    public float moveDuration = 3f; // x좌표로 이동하는 시간
    public float repeatDuration = 3f; // 반복 애니메이션 시간
    public string targetSceneName = "Main/Main"; // 이동할 씬 이름

    private Vector3 initialPosition1; // 1번 초기 위치
    private Vector3 initialPosition3; // 3번 초기 위치
    private bool isRepeating = true;

    void Start()
    {
        // 초기 위치 저장
        if (firstImage != null)
            initialPosition1 = firstImage.transform.localPosition;

        if (thirdImage != null)
            initialPosition3 = thirdImage.transform.localPosition;

        // 애니메이션 시작
        StartCoroutine(PlayTitleAnimations());
    }

    private void Update()
    {
        // 화면 터치하면 씬 이동
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("터치 감지: 씬 이동 준비");
            isRepeating = false;

            // 씬 이동
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log($"씬 이동: {targetSceneName}");
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError("이동할 씬 이름이 설정되지 않았습니다!");
            }
        }
    }

    private IEnumerator PlayTitleAnimations()
    {
        // 1, 2, 3번 이미지 동시 Fade-in
        yield return StartCoroutine(FadeInImages());

        // 1번과 3번 이미지를 동시에 이동
        yield return StartCoroutine(MoveImagesSimultaneously(initialPosition1 + new Vector3(-6f, 0, 0), 8f, firstImage, initialPosition3 + new Vector3(-6f, 0, 0), 8f, thirdImage, moveDuration));

        // 반복 애니메이션 실행
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
            // PingPong을 사용하여 부드럽게 반복
            float t = Mathf.PingPong((timer / duration)*0.5f, 1f); // 0 → 1 → 0으로 부드럽게 변환
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
        // Sprite 초기 설정 (투명하게 시작)
        SetupSpriteAlpha(firstImage, 0f);
        SetupSpriteAlpha(secondImage, 0f);
        SetupSpriteAlpha(thirdImage, 0f);

        float timer = 0f;

        while (timer < fadeDuration) // fadeDuration 동안 반복
        {
            float alpha = timer / fadeDuration; // 알파값을 0 → 1로 서서히 증가
            SetSpriteAlpha(firstImage, alpha);
            SetSpriteAlpha(secondImage, alpha);
            SetSpriteAlpha(thirdImage, alpha);

            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 최종적으로 알파값을 1로 설정
        SetSpriteAlpha(firstImage, 1f);
        SetSpriteAlpha(secondImage, 1f);
        SetSpriteAlpha(thirdImage, 1f);
    }

    private void SetupSpriteAlpha(GameObject obj, float alpha)
    {
        if (obj != null)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>(); // GameObject에서 SpriteRenderer 가져오기
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = alpha; // 알파값 설정
                renderer.color = color; // 색상 업데이트
            }
        }
    }

    private void SetSpriteAlpha(GameObject obj, float alpha)
    {
        if (obj != null)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>(); // GameObject에서 SpriteRenderer 가져오기
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = alpha; // 알파값 설정
                renderer.color = color; // 색상 업데이트
            }
        }
    }
}

