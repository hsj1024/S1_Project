//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class AnimationController : MonoBehaviour
//{
//    private Animator animator;
//    public GameObject firstAnimationObject;
//    public GameObject secondAnimationObject;
//    public GameObject thirdAnimationObject;

//    private bool isSecondAnimationPlaying = false;
//    private bool isThirdAnimationPlaying = false;

//    void Start()
//    {
//        Debug.Log("Start method called");

//        animator = GetComponent<Animator>();
//        if (animator == null)
//        {
//            Debug.LogError("Animator component is missing from this game object");
//        }

//        // 첫 번째 애니메이션 오브젝트 활성화
//        if (firstAnimationObject != null)
//        {
//            firstAnimationObject.SetActive(true);
//        }

//        // 두 번째, 세 번째 애니메이션 오브젝트 비활성화
//        if (secondAnimationObject != null)
//        {
//            secondAnimationObject.SetActive(false);
//        }

//        if (thirdAnimationObject != null)
//        {
//            thirdAnimationObject.SetActive(false);
//        }
//    }

//    void Update()
//    {
//        // 터치 이벤트 처리
//        if (Input.GetMouseButtonDown(0) && isSecondAnimationPlaying && !isThirdAnimationPlaying)
//        {
//            Debug.Log("Touch detected, setting trigger for OnPlayerTouch");

//            // 두 번째 애니메이션 오브젝트 비활성화
//            if (secondAnimationObject != null)
//            {
//                secondAnimationObject.SetActive(false);
//            }

//            // 세 번째 애니메이션 오브젝트 활성화
//            if (thirdAnimationObject != null)
//            {
//                thirdAnimationObject.SetActive(true);
//                Animator thirdAnimator = thirdAnimationObject.GetComponent<Animator>();
//                if (thirdAnimator != null)
//                {
//                    thirdAnimator.SetTrigger("StartThirdAnimation");
//                    isThirdAnimationPlaying = true;
//                }
//            }

//            animator.SetTrigger("OnPlayerTouch");
//        }
//    }

//    public void OnFirstAnimationEnd()
//    {
//        Debug.Log("First animation ended");

//        // 첫 번째 애니메이션 오브젝트 비활성화
//        if (firstAnimationObject != null)
//        {
//            firstAnimationObject.SetActive(false);
//        }

//        // 두 번째 애니메이션 오브젝트 활성화
//        if (secondAnimationObject != null)
//        {
//            secondAnimationObject.SetActive(true);
//        }

//        isSecondAnimationPlaying = true;
//    }

//    public void OnSecondAnimationStart()
//    {
//        isSecondAnimationPlaying = true;
//        Debug.Log("Second animation started");
//    }

//    public void OnSecondAnimationEnd()
//    {
//        isSecondAnimationPlaying = false;
//        Debug.Log("Second animation ended");
//    }

//    public void OnThirdAnimationEnd()
//    {
//        Debug.Log("Third animation ended, loading main scene");
//        SceneManager.LoadScene("Main/Main");
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationController : MonoBehaviour
{
    public GameObject firstAnimationObject; // 1번 이미지
    public GameObject secondAnimationObject; // 2번 이미지
    public GameObject thirdAnimationObject; // 3번 이미지

    public float fadeDuration = 3f; // 페이드 인 지속 시간
    public float dropDuration = 1f; // 2번 이미지 떨어지는 애니메이션 시간
    public Vector3 offscreenPosition = new Vector3(0, 500, 0); // 2번 이미지 시작 위치
    public Vector3 finalPosition = Vector3.zero; // 2번 이미지 최종 위치
    public float dropOvershoot = 50f; // 최종 위치 위로 약간 떨어지도록 설정

    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 부드러운 커브 적용

    private CanvasGroup firstCanvasGroup;
    private CanvasGroup thirdCanvasGroup;

    void Start()
    {
        // 1번과 3번 이미지를 페이드 설정
        firstCanvasGroup = SetupCanvasGroup(firstAnimationObject);
        thirdCanvasGroup = SetupCanvasGroup(thirdAnimationObject);

        // 2번 이미지를 화면 밖으로 이동
        if (secondAnimationObject != null)
        {
            secondAnimationObject.transform.localPosition = offscreenPosition;
        }

        // 애니메이션 시작
        StartCoroutine(PlayAnimations());
    }

    private void Update()
    {
        // 화면 아무 곳이나 터치하면 Main/Main 씬으로 이동
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("Main/Main");
        }
    }

    private CanvasGroup SetupCanvasGroup(GameObject obj)
    {
        if (obj != null)
        {
            // 오브젝트를 활성화한 상태에서 CanvasGroup 설정
            obj.SetActive(true);
            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = obj.AddComponent<CanvasGroup>();
            }

            // 투명하게 설정
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            return canvasGroup;
        }
        return null;
    }

    private IEnumerator PlayAnimations()
    {
        // 1번과 3번 이미지를 페이드 인
        float timer = 0f;
        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            // AnimationCurve로 부드러운 페이드 조정
            float alpha = fadeCurve.Evaluate(t);

            if (firstCanvasGroup != null)
                firstCanvasGroup.alpha = alpha;

            if (thirdCanvasGroup != null)
                thirdCanvasGroup.alpha = alpha;

            timer += Time.deltaTime;
            yield return null;
        }

        // 최종적으로 완전히 표시
        if (firstCanvasGroup != null)
        {
            firstCanvasGroup.alpha = 1;
            firstCanvasGroup.interactable = true;
            firstCanvasGroup.blocksRaycasts = true;
        }

        if (thirdCanvasGroup != null)
        {
            thirdCanvasGroup.alpha = 1;
            thirdCanvasGroup.interactable = true;
            thirdCanvasGroup.blocksRaycasts = true;
        }

        // 2번 이미지 떨어뜨리기
        if (secondAnimationObject != null)
        {
            StartCoroutine(DropSecondImage());
        }
    }

    private IEnumerator DropSecondImage()
    {
        float timer = 0f;
        Vector3 startPosition = offscreenPosition;

        // 최종 위치 위로 약간 떨어지도록 설정
        Vector3 overshootPosition = finalPosition + new Vector3(0, dropOvershoot, 0);

        // 2번 이미지의 위치를 화면 밖에서 최종 위치 바로 위로 이동
        while (timer < dropDuration)
        {
            float t = timer / dropDuration;
            secondAnimationObject.transform.localPosition = Vector3.Lerp(startPosition, overshootPosition, t);
            timer += Time.deltaTime;
            yield return null;
        }

        // 최종 위치에 배치
        secondAnimationObject.transform.localPosition = finalPosition;

        Debug.Log("Animations complete");
    }
}
