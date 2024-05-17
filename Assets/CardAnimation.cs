using UnityEngine;
using System.Collections;

public class CardAnimation : MonoBehaviour
{
    private Animator animator;
    public float animationDuration = 30f; // 애니메이션 지속 시간

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the card.");
        }
    }

    public void PlayAnimation()
    {
        if (animator != null)
        {
            Debug.Log($"Setting ShowCard trigger on {gameObject.name}.");
            animator.SetTrigger("ShowCard");

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
        // 애니메이션 지속 시간만큼 대기
        Debug.Log("Waiting for animation to complete.");
        yield return new WaitForSeconds(animationDuration);

        // 애니메이션이 종료된 후에 카드를 나타냅니다.
        Debug.Log("Animation completed, activating card.");
        gameObject.SetActive(true);
    }

    // 애니메이션이 종료될 때 호출되는 콜백 메서드
    public void OnAnimationEnd()
    {
        // 애니메이션이 종료된 후에 카드를 나타냅니다.
        Debug.Log("Animation end callback, activating card.");
        gameObject.SetActive(true);
    }
}
