using UnityEngine;
using System.Collections;
using System.IO.Pipes;

public class BallistaController : MonoBehaviour
{
    public GameObject arrowPrefab; // ȭ�� ������
    public Transform firePoint; // ȭ���� �߻�� ��ġ
    public GameObject arrowUI; // �߻�Ǳ� ���� ������ ȭ���� UI
    private Bal playerStats; // �÷��̾� ���� ����
    private Vector2 swipeStart; // �������� ���� ��ġ
    private Vector2 swipeEnd; // �������� ���� ��ġ
    public Animator animator; // Animator ������Ʈ ����

    private enum BallistaState { Idle, ReadyToFire }
    private BallistaState currentState = BallistaState.Idle;
    private float swipeStartTime;
    private float swipeEndTime;
    private float swipeDuration;
    private Vector2 swipeStartPos; // �������� ���� ����
    private Vector2 swipeEndPos;
    private float rotationSpeed = 1f; // ȸ�� �ӵ� ���� �߰�

    private bool isReloaded = true; // ������ ������ ����
    private float reloadTimer; // ������ Ÿ�̸� (�������� ������Ʈ)
    void Start()
    {
       
        playerStats = FindObjectOfType<Bal>();
        if (playerStats == null)
        {
            Debug.LogError("Bal ������Ʈ�� ã�� �� �����ϴ�. Bal ������Ʈ�� �� ���� �ִ��� Ȯ���ϼ���.");
            return;
        }
        reloadTimer = playerStats.BallistaReloadTime; // �ʱ� ������ �ð� ����
        ReloadArrow(); // ���� ���� �� ȭ�� UI�� Ȱ��ȭ
    }


    void Update()
    {
        // ������ �Ϸ�� ��쿡�� �������� �Է� ó��
        if (isReloaded)
        {
            if (Input.GetMouseButtonDown(0)) // �������� ����
            {
                swipeStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                swipeStartTime = Time.time; // �������� ���� �ð� ���

                animator.ResetTrigger("Fire");
                animator.SetTrigger("Draw");
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 swipeDirection = (currentPos - swipeStartPos).normalized; // �������� ���� ���

                float swipeDistance = Vector2.Distance(swipeStartPos, currentPos);
                if (swipeDistance > 0.1f)
                {
                    RotateBallistaInstantly(swipeDirection);
                    float swipeDuration = Time.time - swipeStartTime;
                    float drawStrength = Mathf.Clamp(swipeDuration, 0f, 1f); // ���⼭�� �ð� ������� DrawStrength�� ����մϴ�. �ʿ信 ���� ���� ����
                    animator.SetFloat("DrawStrength", drawStrength);
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && isReloaded) // �������� ��
        {
            

            animator.SetTrigger("Fire");
            ShootArrow(); // ���� �߸���Ÿ�� �������� ȭ�� �߻�
        }
    }



    private void ShootArrow()
    {
        isReloaded = false; // ������ ���¸� false�� �����Ͽ� �߰� �߻縦 ����

        // ȭ���� �ν��Ͻ��� �����ϰ�, �߸���Ÿ�� ��ġ�� ȸ������ �ʱ�ȭ�մϴ�.
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        arrow.transform.SetParent(null); // ȭ���� �߸���Ÿ�� �ڽĿ��� �����մϴ�.
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // ȭ�쿡 �߷��� �ۿ����� �ʵ��� �����մϴ�.

        // ȭ���� �ʱ� �ӵ��� �����մϴ�. �̴� �߸���Ÿ�� ����� �÷��̾��� ���ȿ� ����մϴ�.
        rb.velocity = transform.up * playerStats.ArrowSpeed; // ���⼭�� �߸���Ÿ�� 'up' ������ ����մϴ�.

        arrowUI.SetActive(false); // �߻簡 �Ϸ�Ǹ� ȭ�� UI�� ��Ȱ��ȭ�մϴ�.
        Destroy(arrow, 5);
        StartCoroutine(ReloadArrowCoroutine(reloadTimer)); // ������ ������ �����մϴ�.
    }


    private void RotateBallistaInstantly(Vector2 direction)
    {
        // �������� �������� �߸���Ÿ�� ��� ȸ����ŵ�ϴ�.
        float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg - 90; // �߸���Ÿ�� ���� ����
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // firePoint�� ȭ�� UI�� ȸ���� �߸���Ÿ�� �����ϰ� �����մϴ�.
        firePoint.rotation = Quaternion.Euler(0, 0, angle); // ȭ�� �߻� ������ ȸ���� ����
        arrowUI.transform.rotation = Quaternion.Euler(0, 0, angle); // ȭ�� UI�� ȸ���� ����
    }


    private IEnumerator ReloadArrowCoroutine(float reloadTime)
    {
        yield return new WaitForSeconds(playerStats.BallistaReloadTime); // ������ �ð� ���

        ReloadArrow(); // ȭ�� UI�� �ٽ� Ȱ��ȭ

        // �������� �Ϸ�� �Ŀ� �߸���Ÿ�� ȭ�� UI�� ȸ���� �ʱ� ���·� ����
        ResetRotation();
    }

    private void ReloadArrow()
    {
        isReloaded = true; // �������� �Ϸ�Ǿ� �ٽ� �߻� ���� ���·� ����

        arrowUI.SetActive(true); // ȭ�� UI Ȱ��ȭ
    }

    private bool IsSwipeValid()
    {
        // �������� ��ȿ�� �˻� ����
        return Vector2.Distance(swipeStart, swipeEnd) > 100; // ���� ����
    }

    public void OnFireAnimationEnd()
    {
        currentState = BallistaState.Idle; // �ִϸ��̼��� ����Ǹ� Idle ���·� ����

        // �߸���Ÿ�� ȭ�� UI�� �ʱ� ȸ�� ���·� ����
        ResetRotation();
    }

    // ȭ���� ���ư� �Ŀ� �߸���Ÿ�� ȭ�� UI�� ȸ���� �����ϴ� Coroutine
    private IEnumerator ResetRotationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // ������ �ð���ŭ ���

        // �߸���Ÿ�� ȭ�� UI�� ȸ���� �ʱ� ���·� �����մϴ�.
        ResetRotation();
    }
    private void ResetRotation()
    {
        // �߸���Ÿ�� ȭ�� UI�� ȸ���� �ʱ� ���·� �����մϴ�. ���⼭�� (0, 0, 0)���� �����߽��ϴ�.
        transform.rotation = Quaternion.Euler(0, 0, 0);
        firePoint.rotation = Quaternion.Euler(0, 0, 0);
        arrowUI.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    private void AdjustFirePoint(Vector2 direction)
    {
        // �߻� ��ġ�� �����Ͽ� ȭ���� ���ϴ� �������� �߻�ǵ��� �մϴ�.
        // ���� ���, ���� ���������� �߻� ��ġ�� �߸���Ÿ���� ���� �Ÿ���ŭ ������ ��ġ�� �̵���ŵ�ϴ�.
        float offsetDistance = 1.0f; // ���÷� �߻� ��ġ�� �߸���Ÿ���� 1.0f ��ŭ ������ ��ġ�� �����մϴ�.
        firePoint.position = (Vector2)transform.position + direction * offsetDistance;
    }



}