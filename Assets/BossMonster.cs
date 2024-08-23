using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossMonster : MonoBehaviour
{
    public float hp = 500f;
    public int speed = 5;
    public float xp = 0f;
    public bool invincible = true; // 초기에는 무적 상태로 시작
    public float invincibleDuration = 0.3f;
    public float lastHitTime;
    public Rigidbody2D rb;
    public float knockbackForce = 0.5f;
    public float knockbackDuration = 0.2f;
    public bool isKnockedBack = false;
    public float knockbackTimer = 0f;
    public GameObject fireEffectPrefab;
    public GameObject fireEffectInstance;
    public bool isOnFire = false;
    public SpriteRenderer spriteRenderer;
    public Sprite originalSprite; // 원래 스프라이트
    public Sprite newSprite; // 새로운 스프라이트
    public GameObject hitPrefab;
    public AudioClip hitSound;
    public AudioManager audioManager;
    public GameObject hitAnimationPrefab;
    public float animationDuration = 0f;

    private bool isFadingOut = false;
    private bool hasReachedTargetPosition = false;
    public float targetPositionY; // 목표 위치의 Y 좌표
    private bool hasShakenCamera = false; // 카메라 흔들림이 한 번만 일어나게 하는 플래그
    public GameObject[] barricades; // 바리케이트와 부서진 바리케이트를 담을 배열

    // 흔들릴 대상(예: 여러 오브젝트들)
    public List<Transform> shakeTargets;

    // 터치 입력을 막는 오버레이
    public GameObject touchBlocker;

    public MonsterSpawnManager spawnManager;
    private int bossClone2Count = 0; // 보스 클론 2의 카운트
    private int bossClone3Count = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer.sprite; // 초기 스프라이트 저장
        audioManager = AudioManager.Instance;
        invincible = true;

        // 바리케이트 배열 초기화
        barricades = GameObject.FindGameObjectsWithTag("Barricade");

        // shakeTargets 리스트에 바리케이트를 추가
        foreach (var barricade in barricades)
        {
            shakeTargets.Add(barricade.transform);
        }

        if (audioManager == null)
        {
            Debug.LogError("AudioManager를 찾을 수 없습니다. AudioManager가 씬 내에 있는지 확인하세요.");
        }

        // 화면의 3분의 1 지점을 계산하여 목표 위치로 설정
        Camera mainCamera = Camera.main;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraBottom = mainCamera.transform.position.y - cameraHeight / 2f;
        targetPositionY = cameraBottom + (cameraHeight / 3f) + 4f;

        // 터치 입력 오버레이를 비활성화
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(false);
        }

        // 터치 입력을 비활성화
        DisableAllInput();
    }

    void Update()
    {
        if (!isKnockedBack && !hasReachedTargetPosition)
        {
            MoveTowardsTarget();
        }

        if (fireEffectInstance != null)
        {
            fireEffectInstance.transform.position = transform.position;

            var fireSpriteRenderer = fireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
        }
    }

    void MoveTowardsTarget()
    {
        if (transform.position.y > targetPositionY)
        {
            float speedScale = 0.04f;
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;
        }
        else
        {
            // 목표 위치에 도달한 경우 멈추고 무적 상태 유지
            hasReachedTargetPosition = true;
            invincible = true;

            // 스프라이트 교체와 UI 및 오브젝트 흔들림 시작
            StartCoroutine(HandleBossArrival());
        }
    }

    IEnumerator HandleBossArrival()
    {
        if (hasShakenCamera) // 이미 호출된 경우, 코루틴을 종료
        {
            yield break;
        }

        // 1. 스프라이트 교체
        spriteRenderer.sprite = newSprite;

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
            spriteRenderer.sprite = originalSprite;
        }

        // 흔들림이 완료되면 다시는 흔들리지 않도록 설정
        hasShakenCamera = false;

        // 5. 터치 입력을 다시 활성화 (오버레이 비활성화)
        EnableAllInput();

        if (spawnManager != null)
        {
            Debug.Log("Calling SpawnBossClone1...");
            spawnManager.SpawnBossClone1();
        }
        else
        {
            Debug.LogError("SpawnManager가 할당되지 않았습니다!");
        }
    }

    public void OnBossClone2Death()
    {
        bossClone2Count--;

        if (bossClone2Count <= 0)
        {
            StartCoroutine(TemporarilyDisableBossInvincibility());
        }
    }

    private IEnumerator TemporarilyDisableBossInvincibility()
    {
        invincible = false;
        yield return new WaitForSeconds(10f);
        invincible = true;

        // 보스 클론 2 스폰 시작
        spawnManager.SpawnBossClones2();
    }

    public void SpawnBossClones2()
    {
        if (spawnManager != null)
        {
            bossClone2Count = 3; // 클론 2의 총 수
            spawnManager.SpawnBossClones2();
        }
        else
        {
            Debug.LogError("SpawnManager가 할당되지 않았습니다!");
        }
    }

    public void OnBossClone3Death()
    {
        bossClone3Count--;

        if (bossClone3Count <= 0)
        {
            StartCoroutine(DisableBossInvincibilityAfterClones3());
        }
    }

    public void SpawnBossClones3()
    {
        if (spawnManager != null)
        {
            bossClone3Count = 3; // 클론 3의 총 수
            spawnManager.SpawnBossClones3();
        }
        else
        {
            Debug.LogError("SpawnManager가 할당되지 않았습니다!");
        }
    }

    private IEnumerator DisableBossInvincibilityAfterClones3()
    {
        invincible = false;
        yield return new WaitForSeconds(10f);
        invincible = true;

        // 보스 클론 3 스폰 시작
        SpawnBossClones3();
    }

    public void MoveToCenter()
    {
        StartCoroutine(MoveBossToCenter());
    }

    private IEnumerator MoveBossToCenter()
    {
        float speedScale = 0.02f;

        while (true)
        {
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

            // 화면 하단 끝까지 이동하도록 수정
            if (transform.position.y <= -5.0f) // 원하는 위치로 수정할 수 있습니다.
            {
                Destroy(gameObject); 
                yield break;
            }
        }
    }

    public void OnBossDeath()
    {
        StartCoroutine(HandleBossDeath());
    }

    private IEnumerator HandleBossDeath()
    {
        Time.timeScale = 0.5f; // 슬로우모션 효과
        yield return new WaitForSeconds(3f * Time.timeScale);

        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Ending/Ending");
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
        // 각 오브젝트의 원래 위치를 저장
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

        // 모든 오브젝트의 위치를 원래대로 복구
        foreach (var targetTransform in targetTransforms)
        {
            targetTransform.localPosition = originalPositions[targetTransform];
        }
    }

    void EnableAllInput()
    {
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(false); // 터치 입력 오버레이 비활성화
        }
    }

    void DisableAllInput()
    {
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(true); // 터치 입력 오버레이 활성화
        }
    }

    public void TakeDamage(int damage)
    {
        if (!invincible)
        {
            hp -= damage;

            if (hp > 0)
            {
                StartCoroutine(ShowHitEffect(false)); // fire 이펙트 없이 hit 효과만 표시
            }
            else if (hp <= 0 && !isFadingOut) // Ensure this condition
            {
                OnBossDeath();
            }
            lastHitTime = Time.time;
            ActivateInvincibility();
        }
    }

    private void ActivateInvincibility()
    {
        invincible = true;
        StartCoroutine(DisableInvincibility());
    }

    private IEnumerator DisableInvincibility()
    {
        yield return new WaitForSeconds(invincibleDuration);
        invincible = false;
    }

    private IEnumerator ShowHitEffect(bool showFireEffect = true)
    {
        spriteRenderer.enabled = false;

        GameObject currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.3f);

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
            Destroy(currentHitInstance);
        }
        else
        {
            StartCoroutine(FadeOutAndDestroy(showFireEffect, false));
        }
    }

    private IEnumerator FadeOutAndDestroy(bool showFireEffect, bool applyDot)
    {
        if (isFadingOut)
        {
            yield break; // 이미 페이드 아웃이 진행 중이면 중단
        }

        isFadingOut = true; // 페이드 아웃 시작

        spriteRenderer.enabled = false;

        GameObject currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        var hitInstanceSpriteRenderer = currentHitInstance?.GetComponent<SpriteRenderer>();

        float fadeOutDuration = 0.4f;
        float elapsed = 0f;
        Color originalColor = hitInstanceSpriteRenderer.color;

        while (elapsed < fadeOutDuration)
        {
            if (hitInstanceSpriteRenderer != null)
            {
                float t = elapsed / fadeOutDuration;
                hitInstanceSpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(originalColor.a, 0, t));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (hitInstanceSpriteRenderer != null)
        {
            hitInstanceSpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        }

        if (fireEffectInstance != null)
        {
            Destroy(fireEffectInstance);
        }

        if (currentHitInstance != null)
        {
            Destroy(currentHitInstance);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Arrow"))
        {
            // 화살이 맞았을 때 데미지를 처리하는 부분
            Arr arrow = collision.gameObject.GetComponent<Arr>();
            if (arrow != null)
            {
                TakeDamage((int)arrow.damage); // 데미지를 받음
                Destroy(collision.gameObject); // 화살을 파괴
            }
        }
    }
}
