using UnityEngine;
using System.Collections;
using System.IO.Pipes;
using System.Collections.Generic;

public class BallistaController : MonoBehaviour
{
    public GameObject arrowPrefab; // 화살 프리팹
    public Transform firePoint; // 화살이 발사될 위치
    public GameObject mainArrowUI; // 메인 화살의 UI
    public GameObject subArrowUI; // 비활성화될 화살의 UI
    private Bal playerStats; // 플레이어 스탯 참조
    private Vector2 swipeStart; // 스와이프 시작 위치
    private Vector2 swipeEnd; // 스와이프 종료 위치
    public Animator animator; // Animator 컴포넌트 참조

    private enum BallistaState { Idle, ReadyToFire }
    private BallistaState currentState = BallistaState.Idle;
    private float swipeStartTime;
    private float swipeEndTime;
    private float swipeDuration;
    private Vector2 swipeStartPos; // 스와이프 시작 지점
    private Vector2 swipeEndPos;
    private float rotationSpeed = 1f; // 회전 속도 변수 추가

    private bool isReloaded = true; // 재장전 중인지 여부
    private float reloadTimer; // 재장전 타이머 (동적으로 업데이트)
    private List<GameObject> arrows = new List<GameObject>(); // 발사된 화살을 저장할 리스트
    public AudioManager audioManager; // AudioManager 참조
    private LineRenderer lineRenderer; // LineRenderer 참조

    private PlayerController playerController; // 플레이어 컨트롤러 참조
    private Vector2 previousPosition; // 이전 프레임의 마우스 위치
    private const float centralThreshold = 0.1f; // 중앙으로 간주되는 영역의 크기
    private bool isLineRendererEnabled = false; // LineRenderer 활성화 상태를 추적하는 플래그

    //하정 추가
    public int numberOfArrows;


    void Start()
    {
        // AudioManager를 찾아서 할당합니다.
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManager를 찾을 수 없습니다. AudioManager가 씬 내에 있는지 확인하세요.");
            return;
        }
        playerStats = FindObjectOfType<Bal>();
        if (playerStats == null)
        {
            Debug.LogError("Bal 컴포넌트를 찾을 수 없습니다. Bal 컴포넌트가 씬 내에 있는지 확인하세요.");
            return;
        }

        // PlayerController를 찾아서 할당합니다.
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController 컴포넌트를 찾을 수 없습니다. PlayerController가 씬 내에 있는지 확인하세요.");
            return;
        }
        reloadTimer = playerStats.Rt; // 초기 재장전 시간 설정
        mainArrowUI.SetActive(false); // 메인 화살 UI 활성화
        subArrowUI.SetActive(true); // 비활성화될 화살 UI 비활성화
                                    // LineRenderer 컴포넌트를 추가하고 발리스타의 자식으로 설정

        GameObject lineRendererObject = new GameObject("LineRendererObject");
        lineRendererObject.transform.SetParent(transform);
        lineRendererObject.transform.localPosition = Vector3.zero; // 부모의 위치에 맞게 설정

        lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.enabled = false; // 초기에는 비활성화
    }



    void Update()
    {
        // 재장전 완료된 경우에만 스와이프 입력 처리
        if (isReloaded)
        {

            // 재장전 중이 아닐 때만 상호작용 가능하도록 설정
            if (!IsReloading())
            {
                if (Input.GetMouseButtonDown(0)) // 스와이프 시작
                {
                    swipeStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    swipeStartTime = Time.time; // 스와이프 시작 시간 기록

                    animator.ResetTrigger("Fire");
                    animator.SetTrigger("Draw");

                    playerController.SetIsRotatingLeft(false);
                    playerController.SetIsRotatingRight(false);
                    previousPosition = swipeStartPos; // 초기 위치 설정

                }

                if (Input.GetMouseButton(0))
                {
                    Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 swipeDirection = (currentPos - swipeStartPos).normalized; // 스와이프 방향 계산

                    float swipeDistance = Vector2.Distance(swipeStartPos, currentPos);
                    if (swipeDistance > 0.1f)
                    {
                        RotateBallistaInstantly(swipeDirection);
                        float swipeDuration = Time.time - swipeStartTime;
                        float drawStrength = Mathf.Clamp(swipeDuration, 0.5f, 0.6f); // 여기서는 시간 기반으로 DrawStrength를 계산, 필요에 따라 조정 가능.
                        animator.SetFloat("DrawStrength", drawStrength);
                        // LineRenderer를 활성화합니다.
                        if (isLineRendererEnabled)
                        {
                            lineRenderer.enabled = true;
                            UpdateLineRenderer();
                        }

                        // 애니메이션 설정
                        if (Mathf.Abs(currentPos.x - swipeStartPos.x) < centralThreshold)
                        {
                            // 중앙에 가까운 경우 Idle 상태 유지
                            playerController.SetIsRotatingLeft(false);
                            playerController.SetIsRotatingRight(false);
                        }
                        else if (currentPos.x > swipeStartPos.x)
                        {
                            playerController.SetIsRotatingRight(false);
                            playerController.SetIsRotatingLeft(true);
                        }
                        else if (currentPos.x < swipeStartPos.x)
                        {
                            playerController.SetIsRotatingLeft(false);
                            playerController.SetIsRotatingRight(true);
                        }

                        previousPosition = currentPos; // 이전 위치 업데이트

                    }
                }

                if (Input.GetMouseButtonUp(0)) // 스와이프 끝
                {
                    animator.SetTrigger("Fire");
                    ShootArrow(); // 현재 발리스타의 방향으로 화살 발사
                                  // 스와이프가 끝난 후 시위가 원위치로 돌아가도록 설정
                    StartCoroutine(ResetRotationAfterDelay(0.1f)); // 애니메이션 후 잠시 대기 후 회전 리셋

                    // LineRenderer를 비활성화
                    lineRenderer.enabled = false;

                    // 애니메이션 상태 초기화
                    playerController.SetIsRotatingLeft(false);
                    playerController.SetIsRotatingRight(false);
                }
            }
        }
    }


    // LineRenderer를 활성화/비활성화하는 메서드
    public void SetLineRendererEnabled(bool enabled)
    {
        if (lineRenderer != null)
        {
            isLineRendererEnabled = enabled;
            lineRenderer.enabled = enabled;
        }
    }
    private void ShootArrow()
    {
        if (isReloaded)
        {
            isReloaded = false; // 발사 중으로 상태 변경

            int arrowCount = numberOfArrows; // 기존 playerStats.numberOfArrows에서 numberOfArrows로 변경

            // 화살이 여러 개일 때 부채꼴 모양으로 발사
            float angleStep = 5f; // 각도 조절을 위한 변수
            float angleStart = -(arrowCount - 1) * angleStep / 2f; // 중앙을 기준으로 양쪽으로 퍼지도록 설정

            for (int i = 0; i < arrowCount; i++)
            {
                float angle = angleStart + i * angleStep;
                FireArrow(angle);
            }

            // 메인 화살 UI를 비활성화하고, 비활성화될 화살 UI를 활성화
            mainArrowUI.SetActive(false);
            subArrowUI.SetActive(false);

            // 화살 발사 소리를 재생합니다.
            if (audioManager != null)
            {
                audioManager.PlayArrowShootSound(); // AudioManager에서 화살 발사 소리를 재생하는 메서드 호출
            }

            // 발사 후 재장전을 위해 코루틴을 시작합니다.
            StartCoroutine(ReloadArrowCoroutine(reloadTimer));

            // 재장전 타이머가 시작될 때 subArrowUI를 비활성화하도록 설정
            StartCoroutine(DisableSubArrowUIDelayed());
        }
    }


    private void FireArrow(float angle)
    {
        GameObject newArrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, angle));
        newArrow.SetActive(true); // 화살 활성화
        arrows.Add(newArrow);

        // 발사된 화살에 Rigidbody2D를 추가하고 초기 속도를 설정
        Rigidbody2D rb = newArrow.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // 
        rb.velocity = newArrow.transform.up * playerStats.ArrowSpeed; // 화살이 firePoint의 위쪽 방향으로 날아가도록 설정
    }

    public void IncreaseArrowCount()
    {
        numberOfArrows++;
        //Debug.Log("화살증가"+numberOfArrows);
    }


    private IEnumerator DisableSubArrowUIDelayed()
    {
        yield return new WaitForSeconds(reloadTimer);

        // 재장전 타이머가 종료되면 subArrowUI를 비활성화
        subArrowUI.SetActive(true);
    }


    private IEnumerator ReloadArrowCoroutine(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime); // 재장전 시간 대기

        // 화살 UI를 다시 활성화합니다.
        mainArrowUI.SetActive(true);
        subArrowUI.SetActive(false); // 비활성화될 화살 UI를 비활성화

        // 이전에 발사된 화살들을 파괴합니다.
        foreach (var arrow in arrows)
        {
            Destroy(arrow);
        }
        arrows.Clear(); // 리스트 비우기

        isReloaded = true; // 재장전 완료 후 발리스타가 다시 발사할 수 있도록 상태 변경

        // 재장전이 완료된 후에 발리스타와 화살 UI의 회전을 초기 상태로 리셋
        ResetRotation();
    }



    private void RotateBallistaInstantly(Vector2 direction)
    {
        if (subArrowUI == null) return; // arrowUI가 null이면 return

        // 스와이프 방향으로 발리스타를 즉시 회전
        //float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg - 50; // 발리스타의 방향 조정
        //transform.rotation = Quaternion.Euler(0, 0, angle);



        // 중앙 방향으로 조준 시작점이 설정되도록 초기 각도를 조정
        float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;

        // 중앙을 기준으로 맞추기 위해 기본 각도를 90도 더해줍니다.
        angle -= 90f;

        transform.rotation = Quaternion.Euler(0, 0, angle);



        // firePoint와 화살 UI의 회전도 발리스타와 동일하게 설정
        firePoint.rotation = Quaternion.Euler(0, 0, angle); // 화살 발사 지점의 회전도 조정
        mainArrowUI.transform.rotation = Quaternion.Euler(0, 0, angle); // 화살 UI의 회전도 조정
        // 라인 렌더러 위치 업데이트
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        if (isLineRendererEnabled)
        {
            lineRenderer.positionCount = numberOfArrows * 2;
            float angleStep = 5f; // 각도 조절을 위한 변수
            float angleStart = -(numberOfArrows - 1) * angleStep / 2f; // 중앙을 기준으로 양쪽으로 퍼지도록 설정

            for (int i = 0; i < numberOfArrows; i++)
            {
                float angle = angleStart + i * angleStep;
                Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, angle);
                Vector3 lineStartPosition = firePoint.position;
                Vector3 lineEndPosition = firePoint.position + rotation * Vector3.up * 10f;

                lineRenderer.SetPosition(i * 2, lineStartPosition);
                lineRenderer.SetPosition(i * 2 + 1, lineEndPosition);
            }
        }
        else
        {
            lineRenderer.positionCount = 0; // 라인 렌더러를 비활성화
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            ReloadArrow(); // 화살을 제거하고 재장전 상태로 전환
        }
    }

    private void ReloadArrow()
    {
        isReloaded = true; // 재장전이 완료되어 다시 발사 가능 상태로 변경

        mainArrowUI.SetActive(true); // 화살 UI 활성화
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

        // 발리스타와 화살 UI의 회전을 초기 상태로 리셋.
        ResetRotation();
    }
    private void ResetRotation()
    {
        // 발리스타와 화살 UI의 회전을 초기 상태로 리셋. 여기서는 (0, 0, 0)으로 설정
        transform.rotation = Quaternion.Euler(0, 0, 0);
        firePoint.rotation = Quaternion.Euler(0, 0, 0);
        subArrowUI.transform.rotation = Quaternion.Euler(0, 0, 0);

        // 라인 렌더러 위치 업데이트
        UpdateLineRenderer();
    }
    private void AdjustFirePoint(Vector2 direction)
    {
        // 발사 위치를 조절하여 화살이 원하는 방향으로 발사
        // 예를 들어, 현재 구현에서는 발사 위치를 발리스타에서 일정 거리만큼 떨어진 위치로 이동

        float offsetDistance = 1.0f; // 예시로 발사 위치를 발리스타에서 1.0f 만큼 떨어진 위치로 설정
        firePoint.position = (Vector2)transform.position + direction * offsetDistance;
    }
    // 재장전 중인지 확인하는 메서드
    private bool IsReloading()
    {
        return !isReloaded;
    }

}