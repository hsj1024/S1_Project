using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    public GameObject firstAnimationObject; // ù ��° �ִϸ��̼� ������Ʈ
    public GameObject secondAnimationObject; // �� ��° �ִϸ��̼� ������Ʈ
    public GameObject thirdAnimationObject; // �� ��° �ִϸ��̼� ������Ʈ

    void Start()
    {
        // Animator ������Ʈ�� �ڵ����� �Ҵ�
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component is missing from this game object");
        }

        // �� ��° �ִϸ��̼� ������Ʈ ��Ȱ��ȭ
        if (thirdAnimationObject != null)
        {
            thirdAnimationObject.SetActive(false);
        }
    }

    void Update()
    {
        // ��ġ �̺�Ʈ ó��
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Touch detected, setting trigger for OnPlayerTouch");

            // ù ��° �ִϸ��̼� ������Ʈ ��Ȱ��ȭ
            if (firstAnimationObject != null)
            {
                firstAnimationObject.SetActive(false);
            }

            // �� ��° �ִϸ��̼� ������Ʈ ��Ȱ��ȭ
            if (secondAnimationObject != null)
            {
                secondAnimationObject.SetActive(false);
            }

            // �� ��° �ִϸ��̼� ������Ʈ Ȱ��ȭ
            if (thirdAnimationObject != null)
            {
                thirdAnimationObject.SetActive(true);
                // �� ��° �ִϸ��̼� ������Ʈ�� �ִϸ����� ������Ʈ�� �����ͼ� �ִϸ��̼� ����
                Animator thirdAnimator = thirdAnimationObject.GetComponent<Animator>();
                if (thirdAnimator != null)
                {
                    thirdAnimator.SetTrigger("StartThirdAnimation");
                }
            }

            // ���� �ִϸ����Ϳ��� Ʈ���� ����
            animator.SetTrigger("OnPlayerTouch");

            // ���� �ִϸ����� ���� Ȯ��
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log("Current Animator State: " + stateInfo.fullPathHash);
            Debug.Log("Animation Time: " + stateInfo.normalizedTime);
        }
    }

    // �� ��° �ִϸ��̼��� ���� �� ���� ȭ�� ������ ��ȯ
    public void OnThirdAnimationEnd()
    {
        Debug.Log("Third animation ended, loading main scene");
        SceneManager.LoadScene("Main/Main");
    }
}
