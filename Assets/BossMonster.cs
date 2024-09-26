using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossMonster : Monster
{
    private bool hasReachedTargetPosition = false; // 보스가 목표 위치에 도달했는지 여부
    public float targetPositionY; // 목표 위치의 Y 좌표
    private bool hasShakenCamera = false; // 카메라 흔들림이 한 번만 일어나게 하는 플래그
    public GameObject[] barricades; // 바리케이트와 부서진 바리케이트를 담을 배열
    public List<Transform> shakeTargets; // 흔들릴 대상
    public GameObject touchBlocker; // 터치 입력을 막는 오버레이
    public Sprite originalSprite; // 원래 스프라이트
    public Sprite newSprite; // 새로운 스프라이트

    private int bossClone2Count = 0; // 보스 클론 2의 카운트
    private int bossClone3Count = 0; // 보스 클론 3의 카운트
    private bool canMoveAgain = false; // 보스가 다시 이동할 수 있는지 여부
    private bool isBossInvincible = true; // 보스가 무적인지 여부
    private Animator animator; // 애니메이터 변수 추가
    public GameObject bossFireEffectPrefab; // 보스 전용 Fire 이펙트 프리팹
    public GameObject bossFireEffectInstance; // 보스 전용 Fire 이펙트 인스턴스
    public bool bossIsOnFire = false; // Boss의 Fire 이펙트 활성화 여부

    void Start()
    {
        base.Start(); // Monster 클래스의 Start() 호출
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        invincible = true; // 보스 시작 시 무적 상태
        animator.SetBool("isMoving", true);

        // 바리케이트 배열 초기화
        barricades = GameObject.FindGameObjectsWithTag("Barricade");

        // shakeTargets 리스트에 바리케이트를 추가
        foreach (var barricade in barricades)
        {
            shakeTargets.Add(barricade.transform);
        }

        // 화면의 3분의 1 지점을 계산하여 목표 위치로 설정
        Camera mainCamera = Camera.main;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraBottom = mainCamera.transform.position.y - cameraHeight / 2f;
        targetPositionY = cameraBottom + (cameraHeight / 3f) + 4f;

        // 터치 입력 오버레이 비활성화
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(false);
        }

        DisableAllInput();
    }

    void Update()
    {
        // 보스가 이동 중일 때 애니메이션 상태 변경
        if (!isKnockedBack && (canMoveAgain || !hasReachedTargetPosition))
        {
            MoveTowardsTarget();
            animator.SetBool("isMoving", true); // 이동 중일 때 "isMoving" true로 설정
        }
        else
        {
            animator.SetBool("isMoving", false); // 멈췄을 때 "isMoving" false로 설정
        }

        if (bossFireEffectInstance != null)
        {
            bossFireEffectInstance.transform.position = transform.position;
        }
    }

    // Fire 이펙트 적용 시 무적 여부 확인
    public override void ApplyFireEffect()
    {
        // 보스가 무적일 때는 Fire 이펙트를 적용하지 않음
        if (invincible)
        {
            Debug.Log("보스가 무적 상태이므로 Fire 이펙트를 적용하지 않습니다.");
            return;
        }

        // 보스가 무적이 아닐 경우에만 Fire 이펙트를 적용
        if (bossFireEffectPrefab != null && !bossIsOnFire)
        {
            bossFireEffectInstance = Instantiate(bossFireEffectPrefab, transform.position, Quaternion.identity);
            bossFireEffectInstance.transform.SetParent(transform);
            bossIsOnFire = true;
        }
    }

    // DOT 데미지를 보스에 적용하는 메서드
    public override void ApplyDot(int dotDamage)
    {
        // 보스가 무적 상태일 때 DOT 데미지를 적용하지 않음
        if (invincible)
        {
            Debug.Log("보스는 무적 상태이므로 DOT 데미지가 적용되지 않습니다.");
            return;
        }

        // Fire 이펙트를 Boss에 추가
        if (bossFireEffectPrefab != null && !bossIsOnFire)
        {
            bossFireEffectInstance = Instantiate(bossFireEffectPrefab, transform.position, Quaternion.identity);
            bossFireEffectInstance.transform.SetParent(transform); // 보스에 붙이기
            var fireSpriteRenderer = bossFireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            bossIsOnFire = true;
        }

        // DOT 데미지 지속적으로 적용
        StartCoroutine(ApplyBossDotDamage(dotDamage));
    }

    private IEnumerator ApplyBossDotDamage(int dotDamage)
    {
        while (hp > 0)
        {
            if (!invincible)
            {
                hp -= dotDamage;
            }

            yield return new WaitForSeconds(1.0f); // DOT 데미지가 1초마다 적용됨
        }

        // 보스가 Fire 이펙트로 인해 죽었을 때 처리
        if (hp <= 0)
        {
            if (bossFireEffectInstance != null)
            {
                Destroy(bossFireEffectInstance); // Fire 이펙트를 제거
            }

            OnBossDeath(); // 보스가 죽었을 때는 FadeOut 대신 OnBossDeath 호출
        }
    }

    // 보스가 화살에 맞았을 때도 데미지 받지 않도록 수정
    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        // 무적 상태일 때는 데미지를 받지 않음
        if (invincible)
        {
            return; // 무적 상태일 경우 아무 처리를 하지 않음
        }

        // 무적이 아닐 때 데미지 처리
        hp -= damage;

        if (hp <= 0)
        {
            OnBossDeath(); // 보스가 죽었을 때 처리
        }

        // 넉백 처리
        if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
        {
            ApplyKnockback(knockbackDirection);
        }

        // DOT 데미지 처리
        if (applyDot && dotDamage > 0)
        {
            ApplyDot(dotDamage);
        }

        StartCoroutine(PlayArrowHitAnimation());
    }

    public void OnBossDeath()
    {
        StartCoroutine(HandleBossDeath());
    }

    private IEnumerator HandleBossDeath()
    {
        animator.SetTrigger("ChangeSprite"); // 애니메이터 트리거 호출

        Time.timeScale = 0.5f; // 슬로우모션 효과
        yield return new WaitForSeconds(3f * Time.timeScale);

        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Ending/Ending");
    }

    void MoveTowardsTarget()
    {
        if (canMoveAgain)
        {
            // canMoveAgain이 true인 경우 보스가 다시 이동을 시작
            float speedScale = 0.04f;
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

            // 화면 아래로 사라지면 보스를 파괴
            if (transform.position.y <= -5.0f)
            {
                Destroy(gameObject);
            }
        }
        else if (!hasReachedTargetPosition)
        {
            // 목표 위치로 이동하는 동안
            float speedScale = 0.04f;
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

            if (transform.position.y <= targetPositionY)
            {
                hasReachedTargetPosition = true;
                animator.SetBool("isMoving", false); // 이동 중이 아니므로 false 설정
                invincible = true; // 보스는 목표 위치에 도달하면 무적 상태 유지

                // 도착 처리 로직 실행
                StartCoroutine(HandleBossArrival());
            }
        }
    }

    IEnumerator HandleBossArrival()
    {
        if (hasShakenCamera) // 이미 호출된 경우, 코루틴을 종료
        {
            yield break;
        }

        // 1. 스프라이트 교체
        animator.SetTrigger("ChangeSprite"); // 애니메이터 트리거 호출
        Debug.Log("새로운 스프라이트로 변경됨");

        // 2. 0.5초 대기 후 UI 및 오브젝트 흔들림 시작
        yield return new WaitForSeconds(0.5f);

        if (!hasShakenCamera) // 흔들림이 아직 일어나지 않은 경우에만 실행
        {
            hasShakenCamera = true; // 플래그를 설정하여 흔들림이 한 번만 발생하게 함

            // 3. UI 및 오브젝트 흔들림 (1.5초 동안)
            if (shakeTargets != null && shakeTargets.Count > 0)
            {
                StartCoroutine(ShakeObjects(shakeTargets, 1.5f, 0.7f));
            }

            DisableBarricades();

            // 4. 스프라이트를 원래대로 복구 
            yield return new WaitForSeconds(1.5f); // 흔들림 시간 동안 대기
            animator.SetTrigger("RevertSprite");
            Debug.Log("원래 스프라이트로 복구됨");
        }

        hasShakenCamera = false;

        // 5. 추가로 1초 대기 후 보스 클론 1 스폰
        yield return new WaitForSeconds(1.0f); // 카메라 흔들림이 끝난 후 일정 시간 대기

        // 6. 터치 입력을 다시 활성화 (오버레이 비활성화)
        EnableAllInput();

        // 보스 클론 1을 스폰
        if (spawnManager != null)
        {
            spawnManager.SpawnBossClone1();
        }
        else
        {
            Debug.LogError("SpawnManager가 할당되지 않았습니다!");
        }
    }

    public void OnBossClone1Death()
    {
        //Debug.Log("보스몬스터 보스 클론 1이 죽음");

        StartCoroutine(TemporarilyDisableBossInvincibilityThenSpawnClones2());
    }

    private IEnumerator TemporarilyDisableBossInvincibilityThenSpawnClones2()
    {
        // 무적 해제
        invincible = false;
        // Debug.Log("보스 무적 해제 - 클론1 사망");

        yield return new WaitForSeconds(10f); // 10초 후 무적 복원
        invincible = true;
        //Debug.Log("보스 무적 복원");

        // 보스 클론 2 스폰
        spawnManager.SpawnBossClones2();
        //Debug.Log("보스 클론 2 스폰");
    }
    public void OnBossClone2Death()
    {
        bossClone2Count--;

        // 디버그 로그 추가 - 현재 남은 bossClone2Count 출력
        Debug.Log("보스 클론 2 사망, 남은 클론 수: " + bossClone2Count);

        // 모든 클론 2가 죽었을 때만 다음 단계 진행
        if (bossClone2Count <= 0)
        {
            Debug.Log("모든 보스 클론 2가 죽었습니다. 보스 무적 해제!");
            StartCoroutine(TemporarilyDisableBossInvincibilityThenSpawnClones3());
        }
    }

    public void SetBossClone2Count(int count)
    {
        bossClone2Count = count;
        //Debug.Log("BossMonster에서 bossClone2Count 설정: " + bossClone2Count);
    }


    private IEnumerator TemporarilyDisableBossInvincibilityThenSpawnClones3()
    {
        // 무적 해제
        invincible = false;
        //Debug.Log("보스 무적 해제 - 클론2 사망");

        yield return new WaitForSeconds(10f); // 10초 후 무적 복원
        invincible = true;
        //Debug.Log("보스 무적 복원");

        // 보스 클론 3 스폰
        spawnManager.SpawnBossClones3();
        Debug.Log("보스 클론 3 스폰");
    }

    public void SetBossClone3Count(int count)
    {
        bossClone3Count = count;
        //Debug.Log("BossMonster에서 bossClone3Count 설정: " + bossClone3Count);
    }


    // 보스 클론 3이 죽으면 호출됨
    public void OnBossClone3Death()
    {
        bossClone3Count--;

        // 디버그 로그 추가 - 현재 남은 bossClone3Count 출력
        //Debug.Log("보스 클론 3 사망, 남은 클론 수: " + bossClone3Count);

        // 모든 클론 3이 죽었을 때만 무적 해제
        if (bossClone3Count <= 0)
        {
            //Debug.Log("모든 보스 클론 3이 죽었습니다. 보스 무적 해제!");
            StartCoroutine(DisableBossInvincibilityAfterClones3());
        }
    }



    // 보스 클론 3이 죽은 후 무적 해제
    private IEnumerator DisableBossInvincibilityAfterClones3()
    {
        invincible = false; // 무적 해제       

        yield break; // 모든 경로에서 반환
    }

    // 보스 클론3가 중앙에 도달했을 때 호출되는 메서드
    public void EnableBossMovementAgain()
    {
        canMoveAgain = true; // 다시 이동 가능하게 설정
        //Debug.Log("보스움직임");
        invincible = true;   // 무적 유지
    }

    public IEnumerator MoveBossToCenter()
    {
        float speedScale = 0.02f;
        while (true)
        {
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

            if (transform.position.y <= -5.0f)
            {
                Destroy(gameObject);
                yield break;
            }
        }
    }


    void DisableBarricades()
    {
        foreach (var barricade in barricades)
        {
            if (barricade != null)
            {
                barricade.SetActive(false);
            }
        }
    }

    IEnumerator ShakeObjects(List<Transform> targetTransforms, float duration, float magnitude)
    {
        Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
        foreach (var targetTransform in targetTransforms)
        {
            originalPositions[targetTransform] = targetTransform.localPosition;
        }

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            foreach (var targetTransform in targetTransforms)
            {
                Vector3 originalPosition = originalPositions[targetTransform];
                float xOffset = Random.Range(-5f, 5f) * magnitude;
                float yOffset = Random.Range(-1f, 1f) * magnitude;
                targetTransform.localPosition = new Vector3(originalPosition.x + xOffset, originalPosition.y + yOffset, originalPosition.z);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var targetTransform in targetTransforms)
        {
            targetTransform.localPosition = originalPositions[targetTransform];
        }
    }

    void EnableAllInput()
    {
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(false);
        }
    }

    void DisableAllInput()
    {
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(true);
        }
    }
}