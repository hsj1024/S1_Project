using UnityEngine;
using System.Collections;
using System.IO.Pipes;
using System.Collections.Generic;

public class BallistaController : MonoBehaviour
{
    public GameObject arrowPrefab; // ȭ�� ������
    public Transform firePoint; // ȭ���� �߻�� ��ġ
    public GameObject mainArrowUI; // ���� ȭ���� UI
    public GameObject subArrowUI; // ��Ȱ��ȭ�� ȭ���� UI
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
    private List<GameObject> arrows = new List<GameObject>(); // �߻�� ȭ���� ������ ����Ʈ
    public AudioManager audioManager; // AudioManager ����
    private LineRenderer lineRenderer; // LineRenderer ����

    void Start()
    {
        // AudioManager�� ã�Ƽ� �Ҵ��մϴ�.
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManager�� ã�� �� �����ϴ�. AudioManager�� �� ���� �ִ��� Ȯ���ϼ���.");
            return;
        }
        playerStats = FindObjectOfType<Bal>();
        if (playerStats == null)
        {
            Debug.LogError("Bal ������Ʈ�� ã�� �� �����ϴ�. Bal ������Ʈ�� �� ���� �ִ��� Ȯ���ϼ���.");
            return;
        }
        reloadTimer = playerStats.Rt; // �ʱ� ������ �ð� ����
        mainArrowUI.SetActive(false); // ���� ȭ�� UI Ȱ��ȭ
        subArrowUI.SetActive(true); // ��Ȱ��ȭ�� ȭ�� UI ��Ȱ��ȭ
                                    // LineRenderer ������Ʈ�� �߰��մϴ�.
                                    // LineRenderer ������Ʈ�� �߰��ϰ� �߸���Ÿ�� �ڽ����� �����մϴ�.
        GameObject lineRendererObject = new GameObject("LineRendererObject");
        lineRendererObject.transform.SetParent(transform);
        lineRendererObject.transform.localPosition = Vector3.zero; // �θ��� ��ġ�� �°� ����

        lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.enabled = false; // �ʱ⿡�� ��Ȱ��ȭ
    }



    void Update()
    {
        // ������ �Ϸ�� ��쿡�� �������� �Է� ó��
        if (isReloaded)
        {
            // ������ ���� �ƴ� ���� ��ȣ�ۿ� �����ϵ��� ����
            if (!IsReloading())
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

                        // LineRenderer�� Ȱ��ȭ�ϰ� ��θ� �����մϴ�.
                        lineRenderer.enabled = true;
                        // ���� ������ ��ġ ������Ʈ
                        UpdateLineRenderer();
                    }
                }

                if (Input.GetMouseButtonUp(0)) // �������� ��
                {
                    animator.SetTrigger("Fire");
                    ShootArrow(); // ���� �߸���Ÿ�� �������� ȭ�� �߻�

                    // LineRenderer�� ��Ȱ��ȭ�մϴ�.
                    lineRenderer.enabled = false;
                }
            }
        }
    }



    private void ShootArrow()
    {
        if (isReloaded)
        {
            isReloaded = false; // �߻� ������ ���� ����

            // ȭ���� �߻��ϰ� ���� �߻�Ǵ� ȭ���� ����Ʈ�� �߰��մϴ�.
            GameObject newArrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
            newArrow.SetActive(true); // ȭ�� Ȱ��ȭ
            arrows.Add(newArrow);

            // �߻�� ȭ�쿡 Rigidbody2D�� �߰��ϰ� �ʱ� �ӵ��� �����մϴ�.
            Rigidbody2D rb = newArrow.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0; // �߷��� �������� �ʽ��ϴ�.
            rb.velocity = firePoint.up * playerStats.ArrowSpeed; // ȭ���� firePoint�� ���� �������� ���ư����� ����

            // ���� ȭ�� UI�� ��Ȱ��ȭ�ϰ�, ��Ȱ��ȭ�� ȭ�� UI�� Ȱ��ȭ�մϴ�.
            mainArrowUI.SetActive(false);
            subArrowUI.SetActive(false);

            // ȭ�� �߻� �Ҹ��� ����մϴ�.
            if (audioManager != null)
            {
                audioManager.PlayArrowShootSound(); // AudioManager���� ȭ�� �߻� �Ҹ��� ����ϴ� �޼��� ȣ��
            }

            // �߻� �� �������� ���� �ڷ�ƾ�� �����մϴ�.
            StartCoroutine(ReloadArrowCoroutine(reloadTimer));

            // ������ Ÿ�̸Ӱ� ���۵� �� subArrowUI�� ��Ȱ��ȭ�ϵ��� �����մϴ�.
            StartCoroutine(DisableSubArrowUIDelayed());
        }
    }


    private IEnumerator DisableSubArrowUIDelayed()
    {
        yield return new WaitForSeconds(reloadTimer);

        // ������ Ÿ�̸Ӱ� ����Ǹ� subArrowUI�� ��Ȱ��ȭ�մϴ�.
        subArrowUI.SetActive(true);
    }


    private IEnumerator ReloadArrowCoroutine(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime); // ������ �ð� ���

        // ȭ�� UI�� �ٽ� Ȱ��ȭ�մϴ�.
        mainArrowUI.SetActive(true);
        subArrowUI.SetActive(false); // ��Ȱ��ȭ�� ȭ�� UI�� ��Ȱ��ȭ�մϴ�.

        // ������ �߻�� ȭ����� �ı��մϴ�.
        foreach (var arrow in arrows)
        {
            Destroy(arrow);
        }
        arrows.Clear(); // ����Ʈ ����

        isReloaded = true; // ������ �Ϸ� �� �߸���Ÿ�� �ٽ� �߻��� �� �ֵ��� ���� ����

        // �������� �Ϸ�� �Ŀ� �߸���Ÿ�� ȭ�� UI�� ȸ���� �ʱ� ���·� ����
        ResetRotation();
    }



    private void RotateBallistaInstantly(Vector2 direction)
    {
        if (subArrowUI == null) return; // arrowUI�� null�̸� �޼��带 ���������ϴ�.

        // �������� �������� �߸���Ÿ�� ��� ȸ����ŵ�ϴ�.
        float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg - 90; // �߸���Ÿ�� ���� ����
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // firePoint�� ȭ�� UI�� ȸ���� �߸���Ÿ�� �����ϰ� �����մϴ�.
        firePoint.rotation = Quaternion.Euler(0, 0, angle); // ȭ�� �߻� ������ ȸ���� ����
        mainArrowUI.transform.rotation = Quaternion.Euler(0, 0, angle); // ȭ�� UI�� ȸ���� ����
        // ���� ������ ��ġ ������Ʈ
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        // LineRenderer�� ���� ��ġ�� �� ��ġ�� �߸���Ÿ�� ȸ���� �Բ� ������Ʈ�մϴ�.

        lineRenderer.SetPosition(0, firePoint.position);
        //float lineLength = 5f; // ȭ���� ���ư� ������ ���̸� ���� (�ʿ信 ���� ����)

        // LineRenderer�� �� ��ġ�� ȭ�� UI�� ��ġ�� �°� ����
        Vector3 endPosition = firePoint.position + firePoint.up *10f ;
        lineRenderer.SetPosition(1, endPosition);

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            ReloadArrow(); // ȭ���� �����ϰ� ������ ���·� ��ȯ
        }
    }




    private void ReloadArrow()
    {
        isReloaded = true; // �������� �Ϸ�Ǿ� �ٽ� �߻� ���� ���·� ����

        mainArrowUI.SetActive(true); // ȭ�� UI Ȱ��ȭ
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
        subArrowUI.transform.rotation = Quaternion.Euler(0, 0, 0);

        // ���� ������ ��ġ ������Ʈ
        UpdateLineRenderer();
    }
    private void AdjustFirePoint(Vector2 direction)
    {
        // �߻� ��ġ�� �����Ͽ� ȭ���� ���ϴ� �������� �߻�ǵ��� �մϴ�.
        // ���� ���, ���� ���������� �߻� ��ġ�� �߸���Ÿ���� ���� �Ÿ���ŭ ������ ��ġ�� �̵���ŵ�ϴ�.
        float offsetDistance = 1.0f; // ���÷� �߻� ��ġ�� �߸���Ÿ���� 1.0f ��ŭ ������ ��ġ�� �����մϴ�.
        firePoint.position = (Vector2)transform.position + direction * offsetDistance;
    }
    // ������ ������ Ȯ���ϴ� �޼���
    private bool IsReloading()
    {
        return !isReloaded;
    }


}