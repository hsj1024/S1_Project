using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Animator animator; // 플레이어의 Animator 컴포넌트

    // 애니메이션 트리거 메서드
    public void SetAnimationTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    // 애니메이션 트리거 초기화 메서드
    public void ResetAnimationTrigger(string triggerName)
    {
        animator.ResetTrigger(triggerName);
    }

    // 모든 애니메이션 트리거 초기화 메서드
    public void ResetAllAnimationTriggers()
    {
        animator.ResetTrigger("RotateLeft");
        animator.ResetTrigger("RotateRight");
    }

    // 애니메이션 상태 설정 메서드
    public void SetIsRotatingLeft(bool isRotating)
    {
        animator.SetBool("IsRotatingLeft", isRotating);
    }

    public void SetIsRotatingRight(bool isRotating)
    {
        animator.SetBool("IsRotatingRight", isRotating);
    }
}
