using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        base.Start(); // Monster Ŭ������ Start() ȣ��
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

        DisableAllInput();
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
    }

    // Fire ����Ʈ ���� �� ���� ���� Ȯ��
    public override void ApplyFireEffect()
    {
        // ������ ������ ���� Fire ����Ʈ�� �������� ����
        if (invincible)
        {
            Debug.Log("������ ���� �����̹Ƿ� Fire ����Ʈ�� �������� �ʽ��ϴ�.");
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
            Debug.Log("������ ���� �����̹Ƿ� DOT �������� ������� �ʽ��ϴ�.");
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

    public void OnBossDeath()
    {
        StartCoroutine(HandleBossDeath());
    }

    private IEnumerator HandleBossDeath()
    {
        animator.SetTrigger("ChangeSprite"); // �ִϸ����� Ʈ���� ȣ��

        Time.timeScale = 0.5f; // ���ο��� ȿ��
        yield return new WaitForSeconds(3f * Time.timeScale);

        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Ending/Ending");
    }

    void MoveTowardsTarget()
    {
        if (canMoveAgain)
        {
            // canMoveAgain�� true�� ��� ������ �ٽ� �̵��� ����
            float speedScale = 0.04f;
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

            // ȭ�� �Ʒ��� ������� ������ �ı�
            if (transform.position.y <= -5.0f)
            {
                Destroy(gameObject);
            }
        }
        else if (!hasReachedTargetPosition)
        {
            // ��ǥ ��ġ�� �̵��ϴ� ����
            float speedScale = 0.04f;
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

            if (transform.position.y <= targetPositionY)
            {
                hasReachedTargetPosition = true;
                animator.SetBool("isMoving", false); // �̵� ���� �ƴϹǷ� false ����
                invincible = true; // ������ ��ǥ ��ġ�� �����ϸ� ���� ���� ����

                // ���� ó�� ���� ����
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

        // 1. ��������Ʈ ��ü
        animator.SetTrigger("ChangeSprite"); // �ִϸ����� Ʈ���� ȣ��
        Debug.Log("���ο� ��������Ʈ�� �����");

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
            animator.SetTrigger("RevertSprite");
            Debug.Log("���� ��������Ʈ�� ������");
        }

        hasShakenCamera = false;

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
        Debug.Log("���� Ŭ�� 2 ���, ���� Ŭ�� ��: " + bossClone2Count);

        // ��� Ŭ�� 2�� �׾��� ���� ���� �ܰ� ����
        if (bossClone2Count <= 0)
        {
            Debug.Log("��� ���� Ŭ�� 2�� �׾����ϴ�. ���� ���� ����!");
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
        Debug.Log("���� Ŭ�� 3 ����");
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