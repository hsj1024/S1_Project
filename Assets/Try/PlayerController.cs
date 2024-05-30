using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Animator animator; // �÷��̾��� Animator ������Ʈ

    // �ִϸ��̼� Ʈ���� �޼���
    public void SetAnimationTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    // �ִϸ��̼� Ʈ���� �ʱ�ȭ �޼���
    public void ResetAnimationTrigger(string triggerName)
    {
        animator.ResetTrigger(triggerName);
    }

    // ��� �ִϸ��̼� Ʈ���� �ʱ�ȭ �޼���
    public void ResetAllAnimationTriggers()
    {
        animator.ResetTrigger("RotateLeft");
        animator.ResetTrigger("RotateRight");
    }

    // �ִϸ��̼� ���� ���� �޼���
    public void SetIsRotatingLeft(bool isRotating)
    {
        animator.SetBool("IsRotatingLeft", isRotating);
    }

    public void SetIsRotatingRight(bool isRotating)
    {
        animator.SetBool("IsRotatingRight", isRotating);
    }
}
