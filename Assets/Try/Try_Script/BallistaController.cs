using UnityEngine;
using System.Collections;

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
        if (Input.GetMouseButtonDown(0)) // 스와이프 시작
        {
            swipeStartTime = Time.time;
            swipeStartPos = Input.mousePosition;
            animator.ResetTrigger("Fire"); // "Fire" 트리거를 리셋
            animator.SetTrigger("Draw"); // "Draw" 애니메이션 실행
        }

        if (Input.GetMouseButton(0)) // 스와이프 중
        {
            // 스와이프 거리에 기반하여 DrawStrength 값을 계산
            float currentSwipeStrength = Vector2.Distance(swipeStartPos, Input.mousePosition) / 500.0f; // 정규화 값
            animator.SetFloat("DrawStrength", currentSwipeStrength);
        }

        if (Input.GetMouseButtonUp(0)) // 스와이프 끝
        {
            swipeEnd = Input.mousePosition;

            //swipeEndTime = Time.time;
            swipeEndPos = Input.mousePosition;
            //swipeDuration = swipeEndTime - swipeStartTime;

            if (IsSwipeValid())
            {
                // 화살이 발사되는 시점에 애니메이션을 실행
                //animator.SetTrigger("Fire"); // "Fire" 애니메이션 실행

                ShootArrow(); // 화살 발사 로직

            }
        }
    }

    public void OnFireAnimationEnd()
    {
        currentState = BallistaState.Idle; // 애니메이션이 종료되면 Idle 상태로 변경
    }


    private bool IsSwipeValid()
    {
        // 스와이프 유효성 검사 로직
        return Vector2.Distance(swipeStart, swipeEnd) > 100; // 예시 조건
    }
    private void ShootArrow()
    {
        animator.SetTrigger("Fire"); // "Fire" 애니메이션 실행


        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.velocity = firePoint.up * playerStats.ArrowSpeed;

        // 화살 발사 후 화살 UI 비활성화
        arrowUI.SetActive(false);

        StartCoroutine(ReloadArrowCoroutine(reloadTimer)); // 화살 발사 후에 재장전 코루틴 시작
    }

    private IEnumerator ReloadArrowCoroutine(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime); // 재장전 시간 대기
        ReloadArrow(); // 화살 UI를 다시 활성화
    }

    private void ReloadArrow()
    {
        arrowUI.SetActive(true); // 화살 UI 활성화
    }


}
