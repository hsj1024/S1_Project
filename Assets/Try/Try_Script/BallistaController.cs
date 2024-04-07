using UnityEngine;
using System.Collections;
using System.IO.Pipes;

public class BallistaController : MonoBehaviour
{
    public GameObject arrowPrefab; // 화살 프리팹
    public Transform firePoint; // 화살이 발사될 위치
    public GameObject arrowUI; // 발사되기 전에 보여질 화살의 UI
    private Bal playerStats; // 플레이어 스탯 참조
    private float reloadTimer; // 재장전 타이머
    private Vector2 swipeStart; // 스와이프 시작 위치
    private Vector2 swipeEnd; // 스와이프 종료 위치
    public Animator animator; // Animator 컴포넌트 참조

    private enum BallistaState { Idle, ReadyToFire }
    private BallistaState currentState = BallistaState.Idle;
    private float swipeStartTime;
    private float swipeEndTime;
    private float swipeDuration;
    private Vector2 swipeStartPos;
    private Vector2 swipeEndPos;
    private float rotationSpeed = 1f; // 회전 속도 변수 추가

    void Start()
    {
        playerStats = FindObjectOfType<Bal>();
        if (playerStats == null)
        {
            Debug.LogError("Bal 컴포넌트를 찾을 수 없습니다. Bal 컴포넌트가 씬 내에 있는지 확인하세요.");
            return;
        }
        reloadTimer = 2.0f; // 재장전 시간 설정 (예시로 2초로 설정)
        ReloadArrow(); // 게임 시작 시 화살 UI를 활성화
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            animator.ResetTrigger("Fire");
            animator.SetTrigger("Draw");
        }

        if (Input.GetMouseButton(0))
        {
            // 스와이프 거리에 기반하여 DrawStrength 값을 계산
            float currentSwipeStrength = Vector2.Distance(swipeStartPos, Input.mousePosition) / 500.0f; // 정규화 값
            animator.SetFloat("DrawStrength", currentSwipeStrength);

            // 스와이프 동안 발리스타 회전
            swipeEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 swipeDirection = (swipeEndPos - swipeStartPos).normalized;
            RotateBallistaInstantly(swipeDirection);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 swipeEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            animator.SetTrigger("Fire");
            ShootArrow(); // 현재 발리스타의 방향으로 화살 발사
        }
    }


    private void ShootArrow()
    {
        // 화살의 인스턴스를 생성하고, 발리스타의 위치와 회전으로 초기화합니다.
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        arrow.transform.SetParent(null); // 화살을 발리스타의 자식에서 해제합니다.
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // 화살에 중력이 작용하지 않도록 설정합니다.

        // 화살의 초기 속도를 설정합니다. 이는 발리스타의 방향과 플레이어의 스탯에 기반합니다.
        rb.velocity = transform.up * playerStats.ArrowSpeed; // 여기서는 발리스타의 'up' 방향을 사용합니다.

        arrowUI.SetActive(false); // 발사가 완료되면 화살 UI를 비활성화합니다.
        StartCoroutine(ReloadArrowCoroutine(reloadTimer)); // 재장전 로직을 실행합니다.
    }





    private void RotateBallistaInstantly(Vector2 direction)
    {
        // 스와이프 방향으로 발리스타를 즉시 회전시킵니다.
        float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg - 90; // 발리스타의 방향 조정
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // firePoint와 화살 UI의 회전도 발리스타와 동일하게 설정합니다.
        firePoint.rotation = Quaternion.Euler(0, 0, angle); // 화살 발사 지점의 회전도 조정
        arrowUI.transform.rotation = Quaternion.Euler(0, 0, angle); // 화살 UI의 회전도 조정
    }








    private IEnumerator ReloadArrowCoroutine(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime); // 재장전 시간 대기

        ReloadArrow(); // 화살 UI를 다시 활성화

        // 재장전이 완료된 후에 발리스타와 화살 UI의 회전을 초기 상태로 리셋
        ResetRotation();
    }

    private void ReloadArrow()
    {
        arrowUI.SetActive(true); // 화살 UI 활성화
    }

    private bool IsSwipeValid()
    {
        // 스와이프 유효성 검사 로직
        return Vector2.Distance(swipeStart, swipeEnd) > 100; // 예시 조건
    }

    public void OnFireAnimationEnd()
    {
        currentState = BallistaState.Idle; // 애니메이션이 종료되면 Idle 상태로 변경

        // 발리스타와 화살 UI를 초기 회전 상태로 리셋
        ResetRotation();
    }

    // 화살이 날아간 후에 발리스타와 화살 UI의 회전을 리셋하는 Coroutine
    private IEnumerator ResetRotationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간만큼 대기

        // 발리스타와 화살 UI의 회전을 초기 상태로 리셋합니다.
        ResetRotation();
    }
    private void ResetRotation()
    {
        // 발리스타와 화살 UI의 회전을 초기 상태로 리셋합니다. 여기서는 (0, 0, 0)으로 설정했습니다.
        transform.rotation = Quaternion.Euler(0, 0, 0);
        firePoint.rotation = Quaternion.Euler(0, 0, 0);
        arrowUI.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    private void AdjustFirePoint(Vector2 direction)
    {
        // 발사 위치를 조절하여 화살이 원하는 방향으로 발사되도록 합니다.
        // 예를 들어, 현재 구현에서는 발사 위치를 발리스타에서 일정 거리만큼 떨어진 위치로 이동시킵니다.
        float offsetDistance = 1.0f; // 예시로 발사 위치를 발리스타에서 1.0f 만큼 떨어진 위치로 설정합니다.
        firePoint.position = (Vector2)transform.position + direction * offsetDistance;
    }



}
