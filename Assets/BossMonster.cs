using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        base.Start();
        originalHp = hp; // 초기 체력을 originalHp로 저장
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

        UpdateSortingOrder();
    }

    // Fire 이펙트 적용 시 무적 여부 확인
    public override void ApplyFireEffect()
    {
        // 보스가 무적일 때는 Fire 이펙트를 적용하지 않음
        if (invincible)
        {
            //Debug.Log("fire이펙트 적용");
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
            //Debug.Log("DOT데미지X");
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
        Debug.Log($"Monster {monsterName} HP: {hp}/{originalHp}");

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

    public override void ApplyKnockback(Vector2 knockbackDirection, bool destroyAfterKnockback = false)
    {
        // 보스가 무적 상태이거나 움직일 수 없는 상태라면 넉백 효과 무시
        if (invincible || !canMoveAgain)
        {
            return;
        }

        if (currentHitInstance != null)
        {
            Destroy(currentHitInstance);
        }
        currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        spriteRenderer.enabled = false;

        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        if (currentHitInstance != null)
        {
            StartCoroutine(MoveHitPrefabWithKnockback(destroyAfterKnockback));
        }

        IgnoreCollisionsWithOtherMonsters(true);
        StartCoroutine(DisableInvincibility());

        // 넉백 후 이동 재개
        StartCoroutine(ResumeMovementAfterKnockback());
    }

    private IEnumerator ResumeMovementAfterKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration); // 넉백 지속시간만큼 기다림

        isKnockedBack = false; // 넉백 상태 해제
        rb.velocity = Vector2.zero; // 현재 힘 제거

        // 다시 아래로 이동 재개
        if (canMoveAgain || !hasReachedTargetPosition)
        {
            MoveTowardsTarget(); // 아래로 이동하는 메서드 호출
        }
    }


    public void OnBossDeath()
    {
        StopAllCoroutines(); // 모든 코루틴 중지
        DisableAllBossEffects();
        StartCoroutine(HandleBossDeathWithEffects());
    }

    private void DisableAllBossEffects()
    {
        isKnockedBack = false;
        isOnFire = false;
        invincible = true;

        if (bossFireEffectInstance != null)
        {
            Destroy(bossFireEffectInstance);
            bossFireEffectInstance = null;
        }

        rb.velocity = Vector2.zero;
    }

    private IEnumerator HandleBossDeathWithEffects()
    {
        // 1. 모든 태그 중 Monster와 ActiveZone만 남기고 나머지 비활성화
        DeactivateByTag("UI");

        // 2. 보스의 스프라이트를 울부짖는 상태로 변경
        animator.SetTrigger("ChangeSprite");
        spriteRenderer.sprite = newSprite;

        // 3. 슬로우 모션 효과
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(0.5f);

        // 4. 보스와 맵 확대
        GameObject map = GameObject.Find("bac_try");
        if (map != null)
        {
            yield return StartCoroutine(ScaleBossAndMap(map.transform, 3.0f, 1.5f)); // 크게 확대 (4배)
        }

        // 5. 화면 페이드아웃
        yield return StartCoroutine(ScreenFadeOut(2.0f));

        // 6. 슬로우 모션 해제 후 엔딩씬으로 이동
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Ending/Ending");
    }

    private void DeactivateByTag(string tagToDeactivate)
    {
        // 현재 씬의 모든 활성화된 오브젝트 가져오기
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // 오브젝트가 활성화되어 있고, 특정 태그를 가진 경우만 비활성화
            if (obj.activeSelf && obj.CompareTag(tagToDeactivate))
            {
                obj.SetActive(false);
            }
        }
    }

    private IEnumerator ScaleBossMapAndCenterCamera(Transform mapTransform, Camera camera, float targetScale, float duration)
    {
        Vector3 initialBossScale = transform.localScale;
        Vector3 initialMapScale = mapTransform.localScale;

        Vector3 targetBossScale = initialBossScale * targetScale;
        Vector3 targetMapScale = initialMapScale * targetScale;

        Vector3 initialCameraPosition = camera.transform.position;
        Vector3 targetCameraPosition = new Vector3(transform.position.x, transform.position.y, initialCameraPosition.z);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;

            // 보스와 맵 스케일을 부드럽게 확대
            transform.localScale = Vector3.Lerp(initialBossScale, targetBossScale, progress);
            mapTransform.localScale = Vector3.Lerp(initialMapScale, targetMapScale, progress);

            // 카메라를 보스 위치로 부드럽게 이동
            camera.transform.position = Vector3.Lerp(initialCameraPosition, targetCameraPosition, progress);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // 최종 상태 설정
        transform.localScale = targetBossScale;
        mapTransform.localScale = targetMapScale;
        camera.transform.position = targetCameraPosition;
    }

    private IEnumerator ScaleBossAndMap(Transform mapTransform, float targetScale, float duration)
    {
        Vector3 initialBossScale = transform.localScale; // 보스의 초기 스케일
        Vector3 initialMapScale = mapTransform.localScale; // 맵의 초기 스케일

        Vector3 targetBossScale = initialBossScale * targetScale; // 보스 목표 스케일
        Vector3 targetMapScale = initialMapScale * targetScale; // 맵 목표 스케일

        Vector3 initialCameraPosition = Camera.main.transform.position; // 초기 카메라 위치
        Vector3 targetCameraPosition = new Vector3(transform.position.x, transform.position.y, initialCameraPosition.z);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;

            // 보스와 맵을 부드럽게 확대
            transform.localScale = Vector3.Lerp(initialBossScale, targetBossScale, progress);
            mapTransform.localScale = Vector3.Lerp(initialMapScale, targetMapScale, progress);

            // 카메라를 보스 중심으로 이동
            Camera.main.transform.position = Vector3.Lerp(initialCameraPosition, targetCameraPosition, progress);

            elapsedTime += Time.unscaledDeltaTime; // 슬로우 모션에서도 정상 작동
            yield return null;
        }

        // 최종 상태 설정
        transform.localScale = targetBossScale;
        mapTransform.localScale = targetMapScale;
        Camera.main.transform.position = targetCameraPosition;
    }


    private IEnumerator ScreenFadeOut(float fadeDuration)
    {
        GameObject fadeOverlay = GameObject.Find("FadeOverlay");

        if (fadeOverlay == null) // FadeOverlay가 없으면 생성
        {
            fadeOverlay = new GameObject("FadeOverlay");

            // 캔버스 추가 및 설정
            Canvas fadeCanvas = fadeOverlay.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // 화면 전체를 덮는 UI

            fadeOverlay.AddComponent<CanvasRenderer>();

            // 검정색 배경 이미지 추가
            Image fadeImage = fadeOverlay.AddComponent<Image>();
            fadeImage.color = new Color(0, 0, 0, 0); // 초기 색상: 투명

            RectTransform rectTransform = fadeOverlay.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero; // 전체 화면으로 확장
        }

        Image overlayImage = fadeOverlay.GetComponent<Image>();
        if (overlayImage == null)
        {
            overlayImage = fadeOverlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0); // 초기 색상: 투명
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            overlayImage.color = new Color(0f, 0f, 0f, alpha);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        overlayImage.color = new Color(0f, 0f, 0f, 1f); // 완전히 검정색
    }

    void MoveTowardsTarget()
    {
        if (canMoveAgain) // 다시 움직이기 시작한 경우
        {
            float speedScale = 0.04f;
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

            if (transform.position.y <= -5.0f)
            {
                if (!disableGameOver)
                {
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.GameOver();
                    }
                    else
                    {
                        Debug.LogError("LevelManager.Instance is null");
                    }
                }

                Destroy(gameObject);
            }
        }
        else if (!hasReachedTargetPosition) // 목표 위치에 도달하기 전 상태
        {
            float speedScale = 0.04f;
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

            if (transform.position.y <= targetPositionY)
            {
                hasReachedTargetPosition = true;
                animator.SetBool("isMoving", false);
                invincible = true;

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

        DisableAllInput();

        // 1. 스프라이트 교체
        animator.SetTrigger("ChangeSprite"); // 애니메이터 트리거 호출
        //Debug.Log("새로운 스프라이트로 변경됨");

        // 2. 0.5초 대기 후 UI 및 오브젝트 흔들림 시작
        yield return new WaitForSeconds(0.5f);

        if (!hasShakenCamera) // 흔들림이 아직 일어나지 않은 경우에만 실행
        {
            hasShakenCamera = true; // 플래그를 설정하여 흔들림이 한 번만 발생하게 함

            // 3. UI 및 오브젝트 흔들림 (1.5초 동안)
            if (shakeTargets != null && shakeTargets.Count > 0)
            {
                StartCoroutine(ShakeObjects(shakeTargets, 1.5f, 0.1f));
            }

            DisableBarricades();

            // 4. 스프라이트를 원래대로 복구 
            yield return new WaitForSeconds(1.5f); // 흔들림 시간 동안 대기
            animator.SetTrigger("RevertSprite");
            //Debug.Log("원래 스프라이트로 복구됨");
        }

        hasShakenCamera = false;

        // 모든 몬스터를 찾아 제거
        DestroyAllMonsters();

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


    // 화면에 보이는 모든 보스가 아닌 몬스터를 파괴하는 메서드
    void DestroyAllMonsters()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");

        foreach (GameObject monster in monsters)
        {
            Monster monsterComponent = monster.GetComponent<Monster>();
            if (monster != this.gameObject && monsterComponent != null && !monsterComponent.isSpecialMonster)
            {
                if (monster.name == "Wolf_Normal1" || monster.name == "Wolf_Normal2")
                    continue;

                monsterComponent.FadeOut(false, false, skipExperienceDrop: true);

                if (spawnManager != null && spawnManager.activeMonsters.Contains(monsterComponent))
                {
                    spawnManager.activeMonsters.Remove(monsterComponent);
                }
            }
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
        //Debug.Log("보스 클론 2 사망, 남은 클론 수: " + bossClone2Count);

        // 모든 클론 2가 죽었을 때만 다음 단계 진행
        if (bossClone2Count <= 0)
        {
            //Debug.Log("보스 클론 2가 죽음. 보스 무적 해제");
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
        //Debug.Log("보스 클론 3 스폰");
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
                LevelManager.Instance.GameOver();
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
                StartCoroutine(FadeOutAndDisable(barricade)); // 페이드아웃 코루틴 호출
            }
        }
    }

    IEnumerator FadeOutAndDisable(GameObject barricade)
    {
        SpriteRenderer renderer = barricade.GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            float duration = 1.0f; // 페이드아웃 시간
            float elapsed = 0f;
            Color originalColor = renderer.color;

            while (elapsed < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration); // 알파값을 1에서 0으로 서서히 변경
                renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }

            renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // 완전히 투명하게 설정
        }

        barricade.SetActive(false); // 바리케이트 비활성화
    }

    IEnumerator ShakeObjects(List<Transform> targetTransforms, float duration, float magnitude)
    {
        Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();

        // 각 오브젝트의 원래 위치 저장
        foreach (var targetTransform in targetTransforms)
        {
            if (targetTransform != null)
            {
                originalPositions[targetTransform] = targetTransform.position; // 월드 기준 위치
            }
        }

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            foreach (var targetTransform in targetTransforms)
            {
                if (targetTransform != null)
                {
                    Vector3 originalPosition = originalPositions[targetTransform];

                    // X축으로만 흔들림, Y값은 유지
                    float xOffset = Random.Range(-1.0f, 1.0f) * magnitude;
                    targetTransform.position = new Vector3(originalPosition.x + xOffset, originalPosition.y, originalPosition.z);

                    // 회전 고정
                    targetTransform.rotation = Quaternion.identity;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원래 위치로 복구
        foreach (var targetTransform in targetTransforms)
        {
            if (targetTransform != null)
            {
                targetTransform.position = originalPositions[targetTransform];
                targetTransform.rotation = Quaternion.identity; // 회전도 초기화
            }
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

    public override void AdjustHp(float multiplier)
    {
        base.AdjustHp(multiplier); // 부모 클래스의 AdjustHp 호출   
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bal_Hitbox")) // Bal_Hitbox와 충돌
        {
            // 몬스터 파괴를 먼저 실행
            Destroy(gameObject);

            // GameOver 처리
            if (!disableGameOver)
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.GameOver();
                }
                else
                {
                    Debug.LogError("LevelManager.Instance is null");
                }
            }
        }
    }
}