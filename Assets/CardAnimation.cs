using UnityEngine;
using System.Collections;

public class CardAnimation : MonoBehaviour
{
    public GameObject card; // 카드를 참조할 변수
    public float animationDuration = 2.5f; // 애니메이션 지속 시간
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the card.");
        }

        if (card == null)
        {
            Debug.LogError("Card object not assigned.");
        }
    }

    public void PlayAnimation(string animationName, float speed = 1.0f)
    {
        if (animator != null)
        {
            Debug.Log($"Starting animation '{animationName}' for {gameObject.name}.");

            // 애니메이션 속도 설정
            animator.speed = speed;

            // 애니메이션 상태를 직접 설정
            animator.Play(animationName, 0, 0f);

            // 디버깅 출력 추가
            Debug.Log($"Animation '{animationName}' should be playing now.");

            // 애니메이션 완료 후 카드를 활성화하는 코루틴 실행
            Debug.Log("Starting ActivateCardAfterAnimation coroutine.");
            StartCoroutine(ActivateCardAfterAnimation());
        }
        else
        {
            Debug.LogError("Animator component is missing, cannot play animation.");
        }
    }

    private IEnumerator ActivateCardAfterAnimation()
    {
        // 애니메이션 지속 시간만큼 대기 (Unscaled Time)
        Debug.Log("Waiting for animation to complete.");
        yield return new WaitForSecondsRealtime(animationDuration);

        // 애니메이션 속도를 원래대로 복원
        if (animator != null)
        {
            animator.speed = 0.5f;
        }

        // 카드를 활성화합니다.
        Debug.Log("Animation completed, activating card.");
        if (card != null)
        {
            card.SetActive(true);
        }
    }
}
