using System.Collections;
using UnityEngine;

public class BossClone2 : Monster
{
    private bool hasReachedCenter = false;
    public BossMonster BossMonster;
    private bool isDead = false; // Ŭ���� �̹� �׾����� ���θ� ����

    public GameObject cloneFireEffectPrefab; // BossClone2 ���� Fire ����Ʈ ������
    public GameObject cloneFireEffectInstance; // BossClone2 ���� Fire ����Ʈ �ν��Ͻ�
    public bool cloneIsOnFire = false; // BossClone2�� Fire ����Ʈ Ȱ��ȭ ����

    void Start()
    {
        base.Start();
        originalHp = hp; // �ʱ� ü���� originalHp�� ����

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // ��ο� �������� ����
        audioManager = AudioManager.Instance;

        invincible = false; // Ŭ���� ������ �ƴ�
    }

    public void SetBoss(BossMonster boss)
    {
        BossMonster = boss;
    }

    void Update()
    {
        if (!isKnockedBack && !hasReachedCenter)
        {
            MoveDown(); // �߾ӿ� �������� �ʾ��� ���� �̵�
        }

        if (hasReachedCenter)
        {
            rb.velocity = Vector2.zero; // �߾ӿ� �����ϸ� ����
        }

        if (cloneFireEffectInstance != null)
        {
            cloneFireEffectInstance.transform.position = transform.position;
        }
        UpdateSortingOrder();
    }



    public override void MoveDown()
    {
        if (isKnockedBack || (hp <= 0)) return;

        float speedScale = 0.02f;
        transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float cameraCenterY = mainCamera.transform.position.y;

            if (transform.position.y <= cameraCenterY && !hasReachedCenter)
            {
                hasReachedCenter = true;
                rb.velocity = Vector2.zero; // �߾ӿ� �������� �� ����
            }
        }

        if (transform.position.y <= -5.0f)
        {
            if (!disableGameOver)
            {
                LevelManager.Instance?.GameOver();
            }

            Destroy(gameObject);
        }
    }

    public void ApplyCloneFireEffect()
    {
        if (cloneFireEffectPrefab != null && !cloneIsOnFire)
        {
            cloneFireEffectInstance = Instantiate(cloneFireEffectPrefab, transform.position, Quaternion.identity);
            cloneFireEffectInstance.transform.SetParent(transform); // Ŭ�п� ���̱�
            cloneIsOnFire = true;
        }
    }

    public override void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp > 0)
        {
            // ���� �������
        }
        else if (hp <= 0)
        {
            OnCloneDeath(); // ���� Ŭ�� 2 ��� ó��
        }
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            hp -= damage;

            if (hp <= 0)
            {
                OnCloneDeath(); // ��� ó��
                return;
            }

            if (knockbackEnabled && !isKnockedBack && rb != null && !isAoeHit)
            {
                // �˹� ���� �� �ٽ� �Ʒ��� �̵��ϵ��� �ڷ�ƾ ȣ��
                StartCoroutine(HandleKnockbackAndResume(knockbackDirection));
            }

            if (applyDot && dotDamage > 0)
            {
                ApplyDot(dotDamage);
            }
        }

        StartCoroutine(PlayArrowHitAnimation());
    }

    private IEnumerator HandleKnockbackAndResume(Vector2 knockbackDirection)
    {
        // �˹� ó��
        isKnockedBack = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration); // �˹� ���� �ð� ���

        // �˹� ���� ���� �� �ӵ� �ʱ�ȭ
        isKnockedBack = false;
        rb.velocity = Vector2.zero;

        // �߾ӿ� �������� �ʾҰ� ������� ���� ��� �Ʒ��� �̵� �簳
        if (!hasReachedCenter && hp > 0)
        {
            MoveDown();
        }
    }


    private void OnCloneDeath()
    {
        // Debug.Log("���� Ŭ�� 2 ���");

        // ���� �ʱ�ȭ
        isKnockedBack = false; // �˹� ���� �ʱ�ȭ
        rb.velocity = Vector2.zero; // ���� ���� �ӵ� ����

        // ������ ����� ���� ó��
        if (BossMonster != null)
        {
            BossMonster.OnBossClone2Death();
        }

        // ȿ�� ����
        if (fireEffectInstance != null)
        {
            Destroy(fireEffectInstance);
            fireEffectInstance = null;
        }

        if (cloneFireEffectInstance != null)
        {
            Destroy(cloneFireEffectInstance);
            cloneFireEffectInstance = null;
        }

        Destroy(gameObject); // ��ü ����
    }

    // DOT �������� ���� Ŭ�п� �����ϴ� �޼���
    public override void ApplyDot(int dotDamage)
    {
        // BossClone2�� ������ �����Ƿ� DOT �������� �ٷ� ����
        if (cloneFireEffectPrefab != null && !cloneIsOnFire)
        {
            cloneFireEffectInstance = Instantiate(cloneFireEffectPrefab, transform.position, Quaternion.identity);
            cloneFireEffectInstance.transform.SetParent(transform); // Ŭ�п� ���̱�
            var fireSpriteRenderer = cloneFireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            cloneIsOnFire = true;
        }

        // DOT ������ ���������� ����
        StartCoroutine(ApplyCloneDotDamage(dotDamage));
    }

    private IEnumerator ApplyCloneDotDamage(int dotDamage)
    {
        // BossClone2�� ������ �����Ƿ� �� ƽ���� DOT �������� �ٷ� ����
        while (hp > 0)
        {
            hp -= dotDamage;

            yield return new WaitForSeconds(1.0f); // DOT �������� 1�ʸ��� ����
        }

        if (hp <= 0)
        {
            if (cloneFireEffectInstance != null)
            {
                Destroy(cloneFireEffectInstance);
            }

            // DOT �������� ��� �� OnCloneDeath ȣ��
            OnCloneDeath();
        }
    }

    private IEnumerator PlayArrowHitAnimation()
    {
        if (hitAnimationPrefab != null)
        {
            Vector3 animationPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
            GameObject animationInstance = Instantiate(hitAnimationPrefab, animationPosition, Quaternion.identity);
            Destroy(animationInstance, animationDuration);
        }

        if (audioManager != null)
        {
            audioManager.PlayMonsterHitSound();
        }

        yield return new WaitForSeconds(animationDuration);
    }

    public override void AdjustHp(float multiplier)
    {
        base.AdjustHp(multiplier); // �θ� Ŭ������ AdjustHp ȣ��   
    }
}