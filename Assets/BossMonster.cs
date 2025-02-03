using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossMonster : Monster
{
    private bool hasReachedTargetPosition = false; // ������ ��ǥ ��ġ�� �����ߴ��� ����
    public float targetPositionY; // ��ǥ ��ġ�� Y ��ǥ
    private bool hasShakenCamera = false; // ī�޶� ��鸲�� �� ���� �Ͼ�� �ϴ� �÷���
    public GameObject[] barricades; // �ٸ�����Ʈ�� �μ��� �ٸ�����Ʈ�� ���� �迭
    public List<Transform> shakeTargets; // ��鸱 ���
    public GameObject touchBlocker; // ��ġ �Է��� ���� ��������
    public Sprite originalSprite; // ���� ��������Ʈ
    public Sprite newSprite; // ���ο� ��������Ʈ

    private int bossClone2Count = 0; // ���� Ŭ�� 2�� ī��Ʈ
    private int bossClone3Count = 0; // ���� Ŭ�� 3�� ī��Ʈ
    private bool canMoveAgain = false; // ������ �ٽ� �̵��� �� �ִ��� ����
    private bool isBossInvincible = true; // ������ �������� ����
    private Animator animator; // �ִϸ����� ���� �߰�
    public GameObject bossFireEffectPrefab; // ���� ���� Fire ����Ʈ ������
    public GameObject bossFireEffectInstance; // ���� ���� Fire ����Ʈ �ν��Ͻ�
    public bool bossIsOnFire = false; // Boss�� Fire ����Ʈ Ȱ��ȭ ����


    void Start()
    {
        base.Start();
        originalHp = hp; // �ʱ� ü���� originalHp�� ����
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        invincible = true; // ���� ���� �� ���� ����
        animator.SetBool("isMoving", true);

        // �ٸ�����Ʈ �迭 �ʱ�ȭ
        barricades = GameObject.FindGameObjectsWithTag("Barricade");

        // shakeTargets ����Ʈ�� �ٸ�����Ʈ�� �߰�
        foreach (var barricade in barricades)
        {
            shakeTargets.Add(barricade.transform);
        }

        // ȭ���� 3���� 1 ������ ����Ͽ� ��ǥ ��ġ�� ����
        Camera mainCamera = Camera.main;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraBottom = mainCamera.transform.position.y - cameraHeight / 2f;
        targetPositionY = cameraBottom + (cameraHeight / 3f) + 4f;

        // ��ġ �Է� �������� ��Ȱ��ȭ
        if (touchBlocker != null)
        {
            touchBlocker.SetActive(false);
        }
    }

    void Update()
    {
        // ������ �̵� ���� �� �ִϸ��̼� ���� ����
        if (!isKnockedBack && (canMoveAgain || !hasReachedTargetPosition))
        {
            MoveTowardsTarget();
            animator.SetBool("isMoving", true); // �̵� ���� �� "isMoving" true�� ����
        }
        else
        {
            animator.SetBool("isMoving", false); // ������ �� "isMoving" false�� ����
        }

        if (bossFireEffectInstance != null)
        {
            bossFireEffectInstance.transform.position = transform.position;
        }

        UpdateSortingOrder();
    }

    // Fire ����Ʈ ���� �� ���� ���� Ȯ��
    public override void ApplyFireEffect()
    {
        // ������ ������ ���� Fire ����Ʈ�� �������� ����
        if (invincible)
        {
            //Debug.Log("fire����Ʈ ����");
            return;
        }

        // ������ ������ �ƴ� ��쿡�� Fire ����Ʈ�� ����
        if (bossFireEffectPrefab != null && !bossIsOnFire)
        {
            bossFireEffectInstance = Instantiate(bossFireEffectPrefab, transform.position, Quaternion.identity);
            bossFireEffectInstance.transform.SetParent(transform);
            bossIsOnFire = true;
        }
    }

    // DOT �������� ������ �����ϴ� �޼���
    public override void ApplyDot(int dotDamage)
    {
        // ������ ���� ������ �� DOT �������� �������� ����
        if (invincible)
        {
            //Debug.Log("DOT������X");
            return;
        }

        // Fire ����Ʈ�� Boss�� �߰�
        if (bossFireEffectPrefab != null && !bossIsOnFire)
        {
            bossFireEffectInstance = Instantiate(bossFireEffectPrefab, transform.position, Quaternion.identity);
            bossFireEffectInstance.transform.SetParent(transform); // ������ ���̱�
            var fireSpriteRenderer = bossFireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            bossIsOnFire = true;
        }

        // DOT ������ ���������� ����
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

            yield return new WaitForSeconds(1.0f); // DOT �������� 1�ʸ��� �����
        }

        // ������ Fire ����Ʈ�� ���� �׾��� �� ó��
        if (hp <= 0)
        {
            if (bossFireEffectInstance != null)
            {
                Destroy(bossFireEffectInstance); // Fire ����Ʈ�� ����
            }

            OnBossDeath(); // ������ �׾��� ���� FadeOut ��� OnBossDeath ȣ��
        }
    }

    // ������ ȭ�쿡 �¾��� ���� ������ ���� �ʵ��� ����
    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        // ���� ������ ���� �������� ���� ����
        if (invincible)
        {
            return; // ���� ������ ��� �ƹ� ó���� ���� ����
        }

        // ������ �ƴ� �� ������ ó��
        hp -= damage;

        if (hp <= 0)
        {
            OnBossDeath(); // ������ �׾��� �� ó��
        }

        // �˹� ó��
        if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
        {
            ApplyKnockback(knockbackDirection);
        }

        // DOT ������ ó��
        if (applyDot && dotDamage > 0)
        {
            ApplyDot(dotDamage);
        }

        StartCoroutine(PlayArrowHitAnimation());
    }

    public override void ApplyKnockback(Vector2 knockbackDirection, bool destroyAfterKnockback = false)
    {
        // ������ ���� �����̰ų� ������ �� ���� ���¶�� �˹� ȿ�� ����
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

        // �˹� �� �̵� �簳
        StartCoroutine(ResumeMovementAfterKnockback());
    }

    private IEnumerator ResumeMovementAfterKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration); // �˹� ���ӽð���ŭ ��ٸ�

        isKnockedBack = false; // �˹� ���� ����
        rb.velocity = Vector2.zero; // ���� �� ����

        // �ٽ� �Ʒ��� �̵� �簳
        if (canMoveAgain || !hasReachedTargetPosition)
        {
            MoveTowardsTarget(); // �Ʒ��� �̵��ϴ� �޼��� ȣ��
        }
    }


    public void OnBossDeath()
    {
        StopAllCoroutines(); // ��� �ڷ�ƾ ����
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
        // 1. ��� �±� �� Monster�� ActiveZone�� ����� ������ ��Ȱ��ȭ
        DeactivateByTag("UI");

        // 2. ������ ��������Ʈ�� ���¢�� ���·� ����
        animator.SetTrigger("ChangeSprite");
        spriteRenderer.sprite = newSprite;

        // 3. ���ο� ��� ȿ��
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(0.5f);

        // 4. ������ �� Ȯ��
        GameObject map = GameObject.Find("bac_try");
        if (map != null)
        {
            yield return StartCoroutine(ScaleBossAndMap(map.transform, 3.0f, 1.5f)); // ũ�� Ȯ�� (4��)
        }

        // 5. ȭ�� ���̵�ƿ�
        yield return StartCoroutine(ScreenFadeOut(2.0f));

        // 6. ���ο� ��� ���� �� ���������� �̵�
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Ending/Ending");
    }

    private void DeactivateByTag(string tagToDeactivate)
    {
        // ���� ���� ��� Ȱ��ȭ�� ������Ʈ ��������
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // ������Ʈ�� Ȱ��ȭ�Ǿ� �ְ�, Ư�� �±׸� ���� ��츸 ��Ȱ��ȭ
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

            // ������ �� �������� �ε巴�� Ȯ��
            transform.localScale = Vector3.Lerp(initialBossScale, targetBossScale, progress);
            mapTransform.localScale = Vector3.Lerp(initialMapScale, targetMapScale, progress);

            // ī�޶� ���� ��ġ�� �ε巴�� �̵�
            camera.transform.position = Vector3.Lerp(initialCameraPosition, targetCameraPosition, progress);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // ���� ���� ����
        transform.localScale = targetBossScale;
        mapTransform.localScale = targetMapScale;
        camera.transform.position = targetCameraPosition;
    }

    private IEnumerator ScaleBossAndMap(Transform mapTransform, float targetScale, float duration)
    {
        Vector3 initialBossScale = transform.localScale; // ������ �ʱ� ������
        Vector3 initialMapScale = mapTransform.localScale; // ���� �ʱ� ������

        Vector3 targetBossScale = initialBossScale * targetScale; // ���� ��ǥ ������
        Vector3 targetMapScale = initialMapScale * targetScale; // �� ��ǥ ������

        Vector3 initialCameraPosition = Camera.main.transform.position; // �ʱ� ī�޶� ��ġ
        Vector3 targetCameraPosition = new Vector3(transform.position.x, transform.position.y, initialCameraPosition.z);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;

            // ������ ���� �ε巴�� Ȯ��
            transform.localScale = Vector3.Lerp(initialBossScale, targetBossScale, progress);
            mapTransform.localScale = Vector3.Lerp(initialMapScale, targetMapScale, progress);

            // ī�޶� ���� �߽����� �̵�
            Camera.main.transform.position = Vector3.Lerp(initialCameraPosition, targetCameraPosition, progress);

            elapsedTime += Time.unscaledDeltaTime; // ���ο� ��ǿ����� ���� �۵�
            yield return null;
        }

        // ���� ���� ����
        transform.localScale = targetBossScale;
        mapTransform.localScale = targetMapScale;
        Camera.main.transform.position = targetCameraPosition;
    }


    private IEnumerator ScreenFadeOut(float fadeDuration)
    {
        GameObject fadeOverlay = GameObject.Find("FadeOverlay");

        if (fadeOverlay == null) // FadeOverlay�� ������ ����
        {
            fadeOverlay = new GameObject("FadeOverlay");

            // ĵ���� �߰� �� ����
            Canvas fadeCanvas = fadeOverlay.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // ȭ�� ��ü�� ���� UI

            fadeOverlay.AddComponent<CanvasRenderer>();

            // ������ ��� �̹��� �߰�
            Image fadeImage = fadeOverlay.AddComponent<Image>();
            fadeImage.color = new Color(0, 0, 0, 0); // �ʱ� ����: ����

            RectTransform rectTransform = fadeOverlay.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero; // ��ü ȭ������ Ȯ��
        }

        Image overlayImage = fadeOverlay.GetComponent<Image>();
        if (overlayImage == null)
        {
            overlayImage = fadeOverlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0); // �ʱ� ����: ����
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            overlayImage.color = new Color(0f, 0f, 0f, alpha);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        overlayImage.color = new Color(0f, 0f, 0f, 1f); // ������ ������
    }

    void MoveTowardsTarget()
    {
        if (canMoveAgain) // �ٽ� �����̱� ������ ���
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
        else if (!hasReachedTargetPosition) // ��ǥ ��ġ�� �����ϱ� �� ����
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
        if (hasShakenCamera) // �̹� ȣ��� ���, �ڷ�ƾ�� ����
        {
            yield break;
        }

        DisableAllInput();

        // 1. ��������Ʈ ��ü
        animator.SetTrigger("ChangeSprite"); // �ִϸ����� Ʈ���� ȣ��
        //Debug.Log("���ο� ��������Ʈ�� �����");

        // 2. 0.5�� ��� �� UI �� ������Ʈ ��鸲 ����
        yield return new WaitForSeconds(0.5f);

        if (!hasShakenCamera) // ��鸲�� ���� �Ͼ�� ���� ��쿡�� ����
        {
            hasShakenCamera = true; // �÷��׸� �����Ͽ� ��鸲�� �� ���� �߻��ϰ� ��

            // 3. UI �� ������Ʈ ��鸲 (1.5�� ����)
            if (shakeTargets != null && shakeTargets.Count > 0)
            {
                StartCoroutine(ShakeObjects(shakeTargets, 1.5f, 0.1f));
            }

            DisableBarricades();

            // 4. ��������Ʈ�� ������� ���� 
            yield return new WaitForSeconds(1.5f); // ��鸲 �ð� ���� ���
            animator.SetTrigger("RevertSprite");
            //Debug.Log("���� ��������Ʈ�� ������");
        }

        hasShakenCamera = false;

        // ��� ���͸� ã�� ����
        DestroyAllMonsters();

        // 5. �߰��� 1�� ��� �� ���� Ŭ�� 1 ����
        yield return new WaitForSeconds(1.0f); // ī�޶� ��鸲�� ���� �� ���� �ð� ���

        // 6. ��ġ �Է��� �ٽ� Ȱ��ȭ (�������� ��Ȱ��ȭ)
        EnableAllInput();

        // ���� Ŭ�� 1�� ����
        if (spawnManager != null)
        {
            spawnManager.SpawnBossClone1();
        }
        else
        {
            Debug.LogError("SpawnManager�� �Ҵ���� �ʾҽ��ϴ�!");
        }
    }



    // ȭ�鿡 ���̴� ��� ������ �ƴ� ���͸� �ı��ϴ� �޼���
    void DestroyAllMonsters()
    {
        // "Monster" �±װ� �ִ� ��� Ȱ��ȭ�� ���͸� ã���ϴ�.
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");

        foreach (GameObject monster in monsters)
        {
            // Monster ������Ʈ�� �����Ͽ� ����� ���� ���θ� Ȯ��
            Monster monsterComponent = monster.GetComponent<Monster>();
            if (monster != this.gameObject && monsterComponent != null && !monsterComponent.isSpecialMonster)
            {
                // ����ġ�� ������� �ʰ� FadeOut ó��
                monsterComponent.FadeOut(false, false, skipExperienceDrop: true);
            }
        }

        // Debug.Log("������ ����� ���͸� ������ ��� ���͸� fadeout.");
    }


    public void OnBossClone1Death()
    {
        //Debug.Log("�������� ���� Ŭ�� 1�� ����");

        StartCoroutine(TemporarilyDisableBossInvincibilityThenSpawnClones2());
    }

    private IEnumerator TemporarilyDisableBossInvincibilityThenSpawnClones2()
    {
        // ���� ����
        invincible = false;
        // Debug.Log("���� ���� ���� - Ŭ��1 ���");

        yield return new WaitForSeconds(10f); // 10�� �� ���� ����
        invincible = true;
        //Debug.Log("���� ���� ����");

        // ���� Ŭ�� 2 ����
        spawnManager.SpawnBossClones2();
        //Debug.Log("���� Ŭ�� 2 ����");
    }
    public void OnBossClone2Death()
    {
        bossClone2Count--;

        // ����� �α� �߰� - ���� ���� bossClone2Count ���
        //Debug.Log("���� Ŭ�� 2 ���, ���� Ŭ�� ��: " + bossClone2Count);

        // ��� Ŭ�� 2�� �׾��� ���� ���� �ܰ� ����
        if (bossClone2Count <= 0)
        {
            //Debug.Log("���� Ŭ�� 2�� ����. ���� ���� ����");
            StartCoroutine(TemporarilyDisableBossInvincibilityThenSpawnClones3());
        }
    }

    public void SetBossClone2Count(int count)
    {
        bossClone2Count = count;
        //Debug.Log("BossMonster���� bossClone2Count ����: " + bossClone2Count);
    }


    private IEnumerator TemporarilyDisableBossInvincibilityThenSpawnClones3()
    {
        // ���� ����
        invincible = false;
        //Debug.Log("���� ���� ���� - Ŭ��2 ���");

        yield return new WaitForSeconds(10f); // 10�� �� ���� ����
        invincible = true;
        //Debug.Log("���� ���� ����");

        // ���� Ŭ�� 3 ����
        spawnManager.SpawnBossClones3();
        //Debug.Log("���� Ŭ�� 3 ����");
    }

    public void SetBossClone3Count(int count)
    {
        bossClone3Count = count;
        //Debug.Log("BossMonster���� bossClone3Count ����: " + bossClone3Count);
    }


    // ���� Ŭ�� 3�� ������ ȣ���
    public void OnBossClone3Death()
    {
        bossClone3Count--;

        // ����� �α� �߰� - ���� ���� bossClone3Count ���
        //Debug.Log("���� Ŭ�� 3 ���, ���� Ŭ�� ��: " + bossClone3Count);

        // ��� Ŭ�� 3�� �׾��� ���� ���� ����
        if (bossClone3Count <= 0)
        {
            //Debug.Log("��� ���� Ŭ�� 3�� �׾����ϴ�. ���� ���� ����!");
            StartCoroutine(DisableBossInvincibilityAfterClones3());
        }
    }


    // ���� Ŭ�� 3�� ���� �� ���� ����
    private IEnumerator DisableBossInvincibilityAfterClones3()
    {
        invincible = false; // ���� ����       

        yield break; // ��� ��ο��� ��ȯ
    }

    // ���� Ŭ��3�� �߾ӿ� �������� �� ȣ��Ǵ� �޼���
    public void EnableBossMovementAgain()
    {
        canMoveAgain = true; // �ٽ� �̵� �����ϰ� ����
        //Debug.Log("����������");
        invincible = true;   // ���� ����
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
                StartCoroutine(FadeOutAndDisable(barricade)); // ���̵�ƿ� �ڷ�ƾ ȣ��
            }
        }
    }

    IEnumerator FadeOutAndDisable(GameObject barricade)
    {
        SpriteRenderer renderer = barricade.GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            float duration = 1.0f; // ���̵�ƿ� �ð�
            float elapsed = 0f;
            Color originalColor = renderer.color;

            while (elapsed < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration); // ���İ��� 1���� 0���� ������ ����
                renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }

            renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // ������ �����ϰ� ����
        }

        barricade.SetActive(false); // �ٸ�����Ʈ ��Ȱ��ȭ
    }

    IEnumerator ShakeObjects(List<Transform> targetTransforms, float duration, float magnitude)
    {
        Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();

        // �� ������Ʈ�� ���� ��ġ ����
        foreach (var targetTransform in targetTransforms)
        {
            if (targetTransform != null)
            {
                originalPositions[targetTransform] = targetTransform.position; // ���� ���� ��ġ
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

                    // X�����θ� ��鸲, Y���� ����
                    float xOffset = Random.Range(-1.0f, 1.0f) * magnitude;
                    targetTransform.position = new Vector3(originalPosition.x + xOffset, originalPosition.y, originalPosition.z);

                    // ȸ�� ����
                    targetTransform.rotation = Quaternion.identity;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ���� ��ġ�� ����
        foreach (var targetTransform in targetTransforms)
        {
            if (targetTransform != null)
            {
                targetTransform.position = originalPositions[targetTransform];
                targetTransform.rotation = Quaternion.identity; // ȸ���� �ʱ�ȭ
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
        base.AdjustHp(multiplier); // �θ� Ŭ������ AdjustHp ȣ��   
    }

}