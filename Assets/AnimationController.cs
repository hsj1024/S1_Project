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

        // ù ��° �ִϸ��̼� ������Ʈ Ȱ��ȭ
        if (firstAnimationObject != null)
        {
            firstAnimationObject.SetActive(true);
        }

        // �� ��°, �� ��° �ִϸ��̼� ������Ʈ ��Ȱ��ȭ
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
        // ��ġ �̺�Ʈ ó��
        if (Input.GetMouseButtonDown(0) && isSecondAnimationPlaying && !isThirdAnimationPlaying)
        {
            Debug.Log("Touch detected, setting trigger for OnPlayerTouch");

          
            // �� ��° �ִϸ��̼� ������Ʈ ��Ȱ��ȭ
            if (secondAnimationObject != null)
            {
                secondAnimationObject.SetActive(false);
            }

            // �� ��° �ִϸ��̼� ������Ʈ Ȱ��ȭ
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

        // ù ��° �ִϸ��̼� ������Ʈ ��Ȱ��ȭ
        if (firstAnimationObject != null)
        {
            firstAnimationObject.SetActive(false);
        }

        // �� ��° �ִϸ��̼� ������Ʈ Ȱ��ȭ
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
