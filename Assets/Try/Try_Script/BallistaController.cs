using UnityEngine;
using System.Collections;

public class BallistaController : MonoBehaviour
{
    public GameObject arrowPrefab; // ȭ�� ������
    public Transform firePoint; // ȭ���� �߻�� ��ġ
    public GameObject arrowUI; // �߻�Ǳ� ���� ������ ȭ���� UI
    private Bal playerStats; // �÷��̾� ���� ����
    private float reloadTimer; // ������ Ÿ�̸�
    private Vector2 swipeStart; // �������� ���� ��ġ
    private Vector2 swipeEnd; // �������� ���� ��ġ
    public Animator animator; // Animator ������Ʈ ����

    private enum BallistaState { Idle, ReadyToFire }
    private BallistaState currentState = BallistaState.Idle;
    private float swipeStartTime;
    private float swipeEndTime;
    private float swipeDuration;
    private Vector2 swipeStartPos;
    private Vector2 swipeEndPos;

    void Start()
    {
        playerStats = FindObjectOfType<Bal>();
        if (playerStats == null)
        {
            Debug.LogError("Bal ������Ʈ�� ã�� �� �����ϴ�. Bal ������Ʈ�� �� ���� �ִ��� Ȯ���ϼ���.");
            return;
        }
        reloadTimer = 2.0f; // ������ �ð� ���� (���÷� 2�ʷ� ����)
        ReloadArrow(); // ���� ���� �� ȭ�� UI�� Ȱ��ȭ
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // �������� ����
        {
            swipeStartTime = Time.time;
            swipeStartPos = Input.mousePosition;
            animator.ResetTrigger("Fire"); // "Fire" Ʈ���Ÿ� ����
            animator.SetTrigger("Draw"); // "Draw" �ִϸ��̼� ����
        }

        if (Input.GetMouseButton(0)) // �������� ��
        {
            // �������� �Ÿ��� ����Ͽ� DrawStrength ���� ���
            float currentSwipeStrength = Vector2.Distance(swipeStartPos, Input.mousePosition) / 500.0f; // ����ȭ ��
            animator.SetFloat("DrawStrength", currentSwipeStrength);
        }

        if (Input.GetMouseButtonUp(0)) // �������� ��
        {
            swipeEnd = Input.mousePosition;

            //swipeEndTime = Time.time;
            swipeEndPos = Input.mousePosition;
            //swipeDuration = swipeEndTime - swipeStartTime;

            if (IsSwipeValid())
            {
                // ȭ���� �߻�Ǵ� ������ �ִϸ��̼��� ����
                //animator.SetTrigger("Fire"); // "Fire" �ִϸ��̼� ����

                ShootArrow(); // ȭ�� �߻� ����

            }
        }
    }

    public void OnFireAnimationEnd()
    {
        currentState = BallistaState.Idle; // �ִϸ��̼��� ����Ǹ� Idle ���·� ����
    }


    private bool IsSwipeValid()
    {
        // �������� ��ȿ�� �˻� ����
        return Vector2.Distance(swipeStart, swipeEnd) > 100; // ���� ����
    }
    private void ShootArrow()
    {
        animator.SetTrigger("Fire"); // "Fire" �ִϸ��̼� ����


        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.velocity = firePoint.up * playerStats.ArrowSpeed;

        // ȭ�� �߻� �� ȭ�� UI ��Ȱ��ȭ
        arrowUI.SetActive(false);

        StartCoroutine(ReloadArrowCoroutine(reloadTimer)); // ȭ�� �߻� �Ŀ� ������ �ڷ�ƾ ����
    }

    private IEnumerator ReloadArrowCoroutine(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime); // ������ �ð� ���
        ReloadArrow(); // ȭ�� UI�� �ٽ� Ȱ��ȭ
    }

    private void ReloadArrow()
    {
        arrowUI.SetActive(true); // ȭ�� UI Ȱ��ȭ
    }


}
