using System.Collections;
using UnityEngine;

public class BossClone3 : Monster
{
    public GameObject healEffectPrefab; // �� ����Ʈ ������ �߰�
    private GameObject healEffectInstance; // �� ����Ʈ �ν��Ͻ�
    private bool hasReachedCenter = false;
    private bool isHealing = false;
    public BossMonster BossMonster;

    public GameObject cloneFireEffectPrefab; // BossClone3 ���� Fire ����Ʈ ������
    public GameObject cloneFireEffectInstance; // BossClone3 ���� Fire ����Ʈ �ν��Ͻ�
    public bool cloneIsOnFire = false; // BossClone3�� Fire ����Ʈ Ȱ��ȭ ����

    void Start()
    {
        base.Start(); // Monster Ŭ������ Start() ȣ��
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // ��ο� �������� ����
        audioManager = AudioManager.Instance;
        StartCoroutine(HealOverTime());
        invincible = false; // Ŭ���� ������ �ƴ�
    }

    public void SetBoss(BossMonster boss)
    {
        BossMonster = boss;
    }

    void Update()
    {
        base.Update(); // Monster Ŭ������ Update() ȣ��

        if (!isKnockedBack && !hasReachedCenter)
        {
            MoveDown();
        }

        if (healEffectInstance != null)
        {
            healEffectInstance.transform.position = transform.position; // �� ����Ʈ�� ��������� ��ġ ����
        }

        if (cloneFireEffectInstance != null)
        {
            cloneFireEffectInstance.transform.position = transform.position;
        }

        UpdateSortingOrder();
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

    public override void MoveDown()
    {
        if (isKnockedBack || hp <= 0) return;

        float speedScale = 0.02f;
        transform.position += Vector3.down * speed * speedScale * Time.deltaTime;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float cameraCenterY = mainCamera.transform.position.y;

            if (transform.position.y <= cameraCenterY && !hasReachedCenter)
            {
                hasReachedCenter = true;
                rb.velocity = Vector2.zero;

                // ������ ��ȣ�ۿ�: ������ �ٽ� ������ �� �ְ� ��
                if (BossMonster != null)
                {
                    BossMonster.EnableBossMovementAgain();
                }
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

    private IEnumerator HealOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.0f);

            if (hp < 300)
            {
                Heal(5);
            }
        }
    }

    private void Heal(float amount)
    {
        hp += amount;
        if (hp > 300)
        {
            hp = 300;
        }

        StartCoroutine(ShowHealEffect());
    }

    private IEnumerator ShowHealEffect()
    {
        if (!isHealing)
        {
            isHealing = true;

            // �� ����Ʈ �������� ȣ���Ͽ� �ð��� ȿ���� �߰�
            if (healEffectPrefab != null)
            {
                // �� ����Ʈ�� �������� �ʴ� ��쿡�� ����
                if (healEffectInstance == null)
                {
                    healEffectInstance = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
                    healEffectInstance.transform.SetParent(transform); // BossClone3�� ���̱�
                }

                // �� ����Ʈ�� sortingOrder ����
                SpriteRenderer monsterRenderer = GetComponent<SpriteRenderer>();
                SpriteRenderer healEffectRenderer = healEffectInstance.GetComponent<SpriteRenderer>();

                if (healEffectRenderer != null && monsterRenderer != null)
                {
                    healEffectRenderer.sortingLayerName = monsterRenderer.sortingLayerName;
                    healEffectRenderer.sortingOrder = monsterRenderer.sortingOrder + 1;
                }

                // ���� �ð� �� �� ����Ʈ ����
                yield return new WaitForSeconds(1.0f);
                Destroy(healEffectInstance);
                healEffectInstance = null;
            }
            else
            {
                Debug.LogWarning("Heal effect prefab is not assigned!");
            }

            isHealing = false;
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
            OnCloneDeath(); // ���� Ŭ�� 3 ��� ó��
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
                ApplyKnockback(knockbackDirection);
            }

            if (applyDot && dotDamage > 0)
            {
                ApplyDot(dotDamage);
            }
        }

        StartCoroutine(PlayArrowHitAnimation());
    }


    private void OnCloneDeath()
    {
        //Debug.Log("���� Ŭ�� 3 ���");

        // ���� �ʱ�ȭ
        isKnockedBack = false; // �˹� ���� ����
        rb.velocity = Vector2.zero; // ���� ���� �ӵ� ����

        // ������ ����� ���� ó��
        if (BossMonster != null)
        {
            BossMonster.OnBossClone3Death();
        }

        // ����Ʈ ����
        if (cloneFireEffectInstance != null)
        {
            Destroy(cloneFireEffectInstance);
            cloneFireEffectInstance = null;
        }

        if (healEffectInstance != null)
        {
            Destroy(healEffectInstance);
            healEffectInstance = null;
        }

        // ��� �ڷ�ƾ ����
        StopAllCoroutines();

        Destroy(gameObject);
    }


    // DOT �������� ���� Ŭ�п� �����ϴ� �޼���
    public override void ApplyDot(int dotDamage)
    {
        // BossClone3�� ������ �����Ƿ� DOT �������� �ٷ� ����
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
        // BossClone3�� ������ �����Ƿ� �� ƽ���� DOT �������� �ٷ� ����
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
}
