//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class AnimationController : MonoBehaviour
//{
//    private Animator animator;
//    public GameObject firstAnimationObject;
//    public GameObject secondAnimationObject;
//    public GameObject thirdAnimationObject;

//    private bool isSecondAnimationPlaying = false;
//    private bool isThirdAnimationPlaying = false;

//    void Start()
//    {
//        Debug.Log("Start method called");

//        animator = GetComponent<Animator>();
//        if (animator == null)
//        {
//            Debug.LogError("Animator component is missing from this game object");
//        }

//        // ù ��° �ִϸ��̼� ������Ʈ Ȱ��ȭ
//        if (firstAnimationObject != null)
//        {
//            firstAnimationObject.SetActive(true);
//        }

//        // �� ��°, �� ��° �ִϸ��̼� ������Ʈ ��Ȱ��ȭ
//        if (secondAnimationObject != null)
//        {
//            secondAnimationObject.SetActive(false);
//        }

//        if (thirdAnimationObject != null)
//        {
//            thirdAnimationObject.SetActive(false);
//        }
//    }

//    void Update()
//    {
//        // ��ġ �̺�Ʈ ó��
//        if (Input.GetMouseButtonDown(0) && isSecondAnimationPlaying && !isThirdAnimationPlaying)
//        {
//            Debug.Log("Touch detected, setting trigger for OnPlayerTouch");

//            // �� ��° �ִϸ��̼� ������Ʈ ��Ȱ��ȭ
//            if (secondAnimationObject != null)
//            {
//                secondAnimationObject.SetActive(false);
//            }

//            // �� ��° �ִϸ��̼� ������Ʈ Ȱ��ȭ
//            if (thirdAnimationObject != null)
//            {
//                thirdAnimationObject.SetActive(true);
//                Animator thirdAnimator = thirdAnimationObject.GetComponent<Animator>();
//                if (thirdAnimator != null)
//                {
//                    thirdAnimator.SetTrigger("StartThirdAnimation");
//                    isThirdAnimationPlaying = true;
//                }
//            }

//            animator.SetTrigger("OnPlayerTouch");
//        }
//    }

//    public void OnFirstAnimationEnd()
//    {
//        Debug.Log("First animation ended");

//        // ù ��° �ִϸ��̼� ������Ʈ ��Ȱ��ȭ
//        if (firstAnimationObject != null)
//        {
//            firstAnimationObject.SetActive(false);
//        }

//        // �� ��° �ִϸ��̼� ������Ʈ Ȱ��ȭ
//        if (secondAnimationObject != null)
//        {
//            secondAnimationObject.SetActive(true);
//        }

//        isSecondAnimationPlaying = true;
//    }

//    public void OnSecondAnimationStart()
//    {
//        isSecondAnimationPlaying = true;
//        Debug.Log("Second animation started");
//    }

//    public void OnSecondAnimationEnd()
//    {
//        isSecondAnimationPlaying = false;
//        Debug.Log("Second animation ended");
//    }

//    public void OnThirdAnimationEnd()
//    {
//        Debug.Log("Third animation ended, loading main scene");
//        SceneManager.LoadScene("Main/Main");
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationController : MonoBehaviour
{
    public GameObject firstAnimationObject; // 1�� �̹���
    public GameObject secondAnimationObject; // 2�� �̹���
    public GameObject thirdAnimationObject; // 3�� �̹���

    public float fadeDuration = 3f; // ���̵� �� ���� �ð�
    public float dropDuration = 1f; // 2�� �̹��� �������� �ִϸ��̼� �ð�
    public Vector3 offscreenPosition = new Vector3(0, 500, 0); // 2�� �̹��� ���� ��ġ
    public Vector3 finalPosition = Vector3.zero; // 2�� �̹��� ���� ��ġ
    public float dropOvershoot = 50f; // ���� ��ġ ���� �ణ ���������� ����

    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // �ε巯�� Ŀ�� ����

    private CanvasGroup firstCanvasGroup;
    private CanvasGroup thirdCanvasGroup;

    void Start()
    {
        // 1���� 3�� �̹����� ���̵� ����
        firstCanvasGroup = SetupCanvasGroup(firstAnimationObject);
        thirdCanvasGroup = SetupCanvasGroup(thirdAnimationObject);

        // 2�� �̹����� ȭ�� ������ �̵�
        if (secondAnimationObject != null)
        {
            secondAnimationObject.transform.localPosition = offscreenPosition;
        }

        // �ִϸ��̼� ����
        StartCoroutine(PlayAnimations());
    }

    private void Update()
    {
        // ȭ�� �ƹ� ���̳� ��ġ�ϸ� Main/Main ������ �̵�
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("Main/Main");
        }
    }

    private CanvasGroup SetupCanvasGroup(GameObject obj)
    {
        if (obj != null)
        {
            // ������Ʈ�� Ȱ��ȭ�� ���¿��� CanvasGroup ����
            obj.SetActive(true);
            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = obj.AddComponent<CanvasGroup>();
            }

            // �����ϰ� ����
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            return canvasGroup;
        }
        return null;
    }

    private IEnumerator PlayAnimations()
    {
        // 1���� 3�� �̹����� ���̵� ��
        float timer = 0f;
        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            // AnimationCurve�� �ε巯�� ���̵� ����
            float alpha = fadeCurve.Evaluate(t);

            if (firstCanvasGroup != null)
                firstCanvasGroup.alpha = alpha;

            if (thirdCanvasGroup != null)
                thirdCanvasGroup.alpha = alpha;

            timer += Time.deltaTime;
            yield return null;
        }

        // ���������� ������ ǥ��
        if (firstCanvasGroup != null)
        {
            firstCanvasGroup.alpha = 1;
            firstCanvasGroup.interactable = true;
            firstCanvasGroup.blocksRaycasts = true;
        }

        if (thirdCanvasGroup != null)
        {
            thirdCanvasGroup.alpha = 1;
            thirdCanvasGroup.interactable = true;
            thirdCanvasGroup.blocksRaycasts = true;
        }

        // 2�� �̹��� ����߸���
        if (secondAnimationObject != null)
        {
            StartCoroutine(DropSecondImage());
        }
    }

    private IEnumerator DropSecondImage()
    {
        float timer = 0f;
        Vector3 startPosition = offscreenPosition;

        // ���� ��ġ ���� �ణ ���������� ����
        Vector3 overshootPosition = finalPosition + new Vector3(0, dropOvershoot, 0);

        // 2�� �̹����� ��ġ�� ȭ�� �ۿ��� ���� ��ġ �ٷ� ���� �̵�
        while (timer < dropDuration)
        {
            float t = timer / dropDuration;
            secondAnimationObject.transform.localPosition = Vector3.Lerp(startPosition, overshootPosition, t);
            timer += Time.deltaTime;
            yield return null;
        }

        // ���� ��ġ�� ��ġ
        secondAnimationObject.transform.localPosition = finalPosition;

        Debug.Log("Animations complete");
    }
}
