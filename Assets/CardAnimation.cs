using UnityEngine;
using System.Collections;

public class CardAnimation : MonoBehaviour
{
    private Animator animator;
    public float animationDuration = 30f; // �ִϸ��̼� ���� �ð�

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

            // �ִϸ��̼� �Ϸ� �� ī�带 Ȱ��ȭ�ϴ� �ڷ�ƾ ����
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
        // �ִϸ��̼� ���� �ð���ŭ ���
        Debug.Log("Waiting for animation to complete.");
        yield return new WaitForSeconds(animationDuration);

        // �ִϸ��̼��� ����� �Ŀ� ī�带 ��Ÿ���ϴ�.
        Debug.Log("Animation completed, activating card.");
        gameObject.SetActive(true);
    }

    // �ִϸ��̼��� ����� �� ȣ��Ǵ� �ݹ� �޼���
    public void OnAnimationEnd()
    {
        // �ִϸ��̼��� ����� �Ŀ� ī�带 ��Ÿ���ϴ�.
        Debug.Log("Animation end callback, activating card.");
        gameObject.SetActive(true);
    }
}
