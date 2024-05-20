using UnityEngine;
using System.Collections;

public class CardAnimation : MonoBehaviour
{
    public GameObject card; // ī�带 ������ ����
    public float animationDuration = 10f; // �ִϸ��̼� ���� �ð�
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

    public void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            Debug.Log($"Starting animation '{animationName}' for {gameObject.name}.");

            // �ִϸ��̼� ���¸� ���� ����
            animator.Play(animationName, 0, 1f);

            // ����� ��� �߰�
            Debug.Log($"Animation '{animationName}' should be playing now.");

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

        // ī�带 Ȱ��ȭ�մϴ�.
        Debug.Log("Animation completed, activating card.");
        if (card != null)
        {
            card.SetActive(true);
        }
    }
}
