using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    public GameObject firstAnimationObject;
    public GameObject secondAnimationObject;
    public GameObject thirdAnimationObject;

    private bool isSecondAnimationPlaying = false;
    private bool isThirdAnimationPlaying = false;

    void Start()
    {
        Debug.Log("Start method called");

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing from this game object");
        }

        // 첫 번째 애니메이션 오브젝트 활성화
        if (firstAnimationObject != null)
        {
            firstAnimationObject.SetActive(true);
        }

        // 두 번째, 세 번째 애니메이션 오브젝트 비활성화
        if (secondAnimationObject != null)
        {
            secondAnimationObject.SetActive(false);
        }

        if (thirdAnimationObject != null)
        {
            thirdAnimationObject.SetActive(false);
        }
    }

    void Update()
    {
        // 터치 이벤트 처리
        if (Input.GetMouseButtonDown(0) && isSecondAnimationPlaying && !isThirdAnimationPlaying)
        {
            Debug.Log("Touch detected, setting trigger for OnPlayerTouch");

          
            // 두 번째 애니메이션 오브젝트 비활성화
            if (secondAnimationObject != null)
            {
                secondAnimationObject.SetActive(false);
            }

            // 세 번째 애니메이션 오브젝트 활성화
            if (thirdAnimationObject != null)
            {
                thirdAnimationObject.SetActive(true);
                Animator thirdAnimator = thirdAnimationObject.GetComponent<Animator>();
                if (thirdAnimator != null)
                {
                    thirdAnimator.SetTrigger("StartThirdAnimation");
                    isThirdAnimationPlaying = true;
                }
            }

            animator.SetTrigger("OnPlayerTouch");
        }
    }

    public void OnFirstAnimationEnd()
    {
        Debug.Log("First animation ended");

        // 첫 번째 애니메이션 오브젝트 비활성화
        if (firstAnimationObject != null)
        {
            firstAnimationObject.SetActive(false);
        }

        // 두 번째 애니메이션 오브젝트 활성화
        if (secondAnimationObject != null)
        {
            secondAnimationObject.SetActive(true);
        }

        isSecondAnimationPlaying = true;
    }

    public void OnSecondAnimationStart()
    {
        isSecondAnimationPlaying = true;
        Debug.Log("Second animation started");
    }

    public void OnSecondAnimationEnd()
    {
        isSecondAnimationPlaying = false;
        Debug.Log("Second animation ended");
    }

    public void OnThirdAnimationEnd()
    {
        Debug.Log("Third animation ended, loading main scene");
        SceneManager.LoadScene("Main/Main");
    }
}
