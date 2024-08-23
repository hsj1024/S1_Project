using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossMonster : MonoBehaviour
{
    public float hp = 500f;
    public int speed = 5;
    public float xp = 0f;
    public bool invincible = true; // �ʱ⿡�� ���� ���·� ����
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
    public Sprite originalSprite; // ���� ��������Ʈ
    public Sprite newSprite; // ���ο� ��������Ʈ
    public GameObject hitPrefab;
    public AudioClip hitSound;
    public AudioManager audioManager;
    public GameObject hitAnimationPrefab;
    public float animationDuration = 0f;

    private bool isFadingOut = false;
    private bool hasReachedTargetPosition = false;
    public float targetPositionY; // ��ǥ ��ġ�� Y ��ǥ
    private bool hasShakenCamera = false; // ī�޶� ��鸲�� �� ���� �Ͼ�� �ϴ� �÷���
    public GameObject[] barricades; // �ٸ�����Ʈ�� �μ��� �ٸ�����Ʈ�� ���� �迭

    // ��鸱 ���(��: ���� ������Ʈ��)
    public List<Transform> shakeTargets;

    // ��ġ �Է��� ���� ��������
    public GameObject touchBlocker;

    public MonsterSpawnManager spawnManager;
    private int bossClone2Count = 0; // ���� Ŭ�� 2�� ī��Ʈ
    private int bossClone3Count = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer.sprite; // �ʱ� ��������Ʈ ����
        audioManager = AudioManager.Instance;
        invincible = true;

        // �ٸ�����Ʈ �迭 �ʱ�ȭ
        barricades = GameObject.FindGameObjectsWithTag("Barricade");

        // shakeTargets ����Ʈ�� �ٸ�����Ʈ�� �߰�
        foreach (var barricade in barricades)
        {
            shakeTargets.Add(barricade.transform);
        }

        if (audioManager == null)
        {
            Debug.LogError("AudioManager�� ã�� �� �����ϴ�. AudioManager�� �� ���� �ִ��� Ȯ���ϼ���.");
        }

        // ȭ���� 3���� 1 ������ ����Ͽ� ��ǥ ��ġ�� ����
        Camera mainCamera = Camera.main;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraBottom = mainCamera.transform.position.y - cameraHeight / 2f;
        targetPositionY = cameraBottom + (cameraHeight / 3f) + 4f;

        // ��ġ �Է� �������̸� ��Ȱ��ȭ
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(false);
        }

        // ��ġ �Է��� ��Ȱ��ȭ
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
            // ��ǥ ��ġ�� ������ ��� ���߰� ���� ���� ����
            hasReachedTargetPosition = true;
            invincible = true;

            // ��������Ʈ ��ü�� UI �� ������Ʈ ��鸲 ����
            StartCoroutine(HandleBossArrival());
        }
    }

    IEnumerator HandleBossArrival()
    {
        if (hasShakenCamera) // �̹� ȣ��� ���, �ڷ�ƾ�� ����
        {
            yield break;
        }

        // 1. ��������Ʈ ��ü
        spriteRenderer.sprite = newSprite;

        // 2. 0.5�� ��� �� UI �� ������Ʈ ��鸲 ����
        yield return new WaitForSeconds(0.5f);

        if (!hasShakenCamera) // ��鸲�� ���� �Ͼ�� ���� ��쿡�� ����
        {
            hasShakenCamera = true; // �÷��׸� �����Ͽ� ��鸲�� �� ���� �߻��ϰ� ��
                                    // 3. UI �� ������Ʈ ��鸲 (1.5�� ����)
            if (shakeTargets != null && shakeTargets.Count > 0)
            {
                StartCoroutine(ShakeObjects(shakeTargets, 1.5f, 0.7f));
            }

            DisableBarricades();

            // 4. ��������Ʈ�� ������� ����
            yield return new WaitForSeconds(1.5f); // ��鸲 �ð� ���� ���
            spriteRenderer.sprite = originalSprite;
        }

        // ��鸲�� �Ϸ�Ǹ� �ٽô� ��鸮�� �ʵ��� ����
        hasShakenCamera = false;

        // 5. ��ġ �Է��� �ٽ� Ȱ��ȭ (�������� ��Ȱ��ȭ)
        EnableAllInput();

        if (spawnManager != null)
        {
            Debug.Log("Calling SpawnBossClone1...");
            spawnManager.SpawnBossClone1();
        }
        else
        {
            Debug.LogError("SpawnManager�� �Ҵ���� �ʾҽ��ϴ�!");
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

        // ���� Ŭ�� 2 ���� ����
        spawnManager.SpawnBossClones2();
    }

    public void SpawnBossClones2()
    {
        if (spawnManager != null)
        {
            bossClone2Count = 3; // Ŭ�� 2�� �� ��
            spawnManager.SpawnBossClones2();
        }
        else
        {
            Debug.LogError("SpawnManager�� �Ҵ���� �ʾҽ��ϴ�!");
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
            bossClone3Count = 3; // Ŭ�� 3�� �� ��
            spawnManager.SpawnBossClones3();
        }
        else
        {
            Debug.LogError("SpawnManager�� �Ҵ���� �ʾҽ��ϴ�!");
        }
    }

    private IEnumerator DisableBossInvincibilityAfterClones3()
    {
        invincible = false;
        yield return new WaitForSeconds(10f);
        invincible = true;

        // ���� Ŭ�� 3 ���� ����
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

            // ȭ�� �ϴ� ������ �̵��ϵ��� ����
            if (transform.position.y <= -5.0f) // ���ϴ� ��ġ�� ������ �� �ֽ��ϴ�.
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
        Time.timeScale = 0.5f; // ���ο��� ȿ��
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
        // �� ������Ʈ�� ���� ��ġ�� ����
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

        // ��� ������Ʈ�� ��ġ�� ������� ����
        foreach (var targetTransform in targetTransforms)
        {
            targetTransform.localPosition = originalPositions[targetTransform];
        }
    }

    void EnableAllInput()
    {
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(false); // ��ġ �Է� �������� ��Ȱ��ȭ
        }
    }

    void DisableAllInput()
    {
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(true); // ��ġ �Է� �������� Ȱ��ȭ
        }
    }

    public void TakeDamage(int damage)
    {
        if (!invincible)
        {
            hp -= damage;

            if (hp > 0)
            {
                StartCoroutine(ShowHitEffect(false)); // fire ����Ʈ ���� hit ȿ���� ǥ��
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
            yield break; // �̹� ���̵� �ƿ��� ���� ���̸� �ߴ�
        }

        isFadingOut = true; // ���̵� �ƿ� ����

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
            // ȭ���� �¾��� �� �������� ó���ϴ� �κ�
            Arr arrow = collision.gameObject.GetComponent<Arr>();
            if (arrow != null)
            {
                TakeDamage((int)arrow.damage); // �������� ����
                Destroy(collision.gameObject); // ȭ���� �ı�
            }
        }
    }
}
