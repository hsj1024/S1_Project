using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    public GameObject firstAnimationObject; // 첫 번째 애니메이션 오브젝트
    public GameObject secondAnimationObject; // 두 번째 애니메이션 오브젝트
    public GameObject thirdAnimationObject; // 세 번째 애니메이션 오브젝트

    void Start()
    {
        // Animator 컴포넌트를 자동으로 할당
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component is missing from this game object");
        }

        // 세 번째 애니메이션 오브젝트 비활성화
        if (thirdAnimationObject != null)
        {
            thirdAnimationObject.SetActive(false);
        }
    }

    void Update()
    {
        // 터치 이벤트 처리
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Touch detected, setting trigger for OnPlayerTouch");

            // 첫 번째 애니메이션 오브젝트 비활성화
            if (firstAnimationObject != null)
            {
                firstAnimationObject.SetActive(false);
            }

            // 두 번째 애니메이션 오브젝트 비활성화
            if (secondAnimationObject != null)
            {
                secondAnimationObject.SetActive(false);
            }

            // 세 번째 애니메이션 오브젝트 활성화
            if (thirdAnimationObject != null)
            {
                thirdAnimationObject.SetActive(true);
                // 세 번째 애니메이션 오브젝트의 애니메이터 컴포넌트를 가져와서 애니메이션 시작
                Animator thirdAnimator = thirdAnimationObject.GetComponent<Animator>();
                if (thirdAnimator != null)
                {
                    thirdAnimator.SetTrigger("StartThirdAnimation");
                }
            }

            // 메인 애니메이터에서 트리거 설정
            animator.SetTrigger("OnPlayerTouch");

            // 현재 애니메이터 상태 확인
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("Current Animator State: " + stateInfo.fullPathHash);
            Debug.Log("Animation Time: " + stateInfo.normalizedTime);
        }
    }

    // 세 번째 애니메이션이 끝난 후 메인 화면 씬으로 전환
    public void OnThirdAnimationEnd()
    {
        Debug.Log("Third animation ended, loading main scene");
        SceneManager.LoadScene("Main/Main");
    }
}
