using System.Collections;
using UnityEngine;

public class BossClone1 : Monster
{
    public float zigzagAmplitude = 20f; // ������� �̵��� ����
    public float zigzagFrequency = 0.1f; // ������� �̵��� �ֱ�
    public float verticalSpeed = 0.2f; // ���� �̵� �ӵ�
    private bool moveLeftToRight;
    public BossMonster BossMonster;

    public GameObject cloneFireEffectPrefab; // BossClone1 ���� Fire ����Ʈ ������
    public GameObject cloneFireEffectInstance; // BossClone1 ���� Fire ����Ʈ �ν��Ͻ�
    public bool cloneIsOnFire = false; // BossClone1�� Fire ����Ʈ Ȱ��ȭ ����


    void Start()
    {
        base.Start();
        originalHp = hp; // �ʱ� ü���� originalHp�� ����
 
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // ��ο� �������� ����
        moveLeftToRight = transform.position.x < 0; // �ʱ� �̵� ���� ����

        if (BossMonster == null)
        {
            Debug.LogError("BossMonster�� �Ҵ���� �ʾҽ��ϴ�! SetBoss�� ���� �Ҵ��ϼ���.");
        }
    }

    public void SetBoss(BossMonster boss)
    {
        BossMonster = boss;
    }

    void Update()
    {
        base.Update(); // Monster Ŭ������ Update() ȣ��
        MoveDown(); // ��ӹ��� MoveDown() ��� ������� �̵��� ����

        if (cloneFireEffectInstance != null)
        {
            cloneFireEffectInstance.transform.position = transform.position;
        }

        UpdateSortingOrder();
    }


    public override void MoveDown()
    {
        if (isKnockedBack || (hp <= 0)) return;

        // ������� �̵� ó��
        float zigzagDirection = moveLeftToRight ? 1 : -1;
        float zigzag = Mathf.PingPong(Time.time * zigzagFrequency, 1) * 2 - 1;
        Vector3 direction = new Vector3(zigzag * zigzagAmplitude * zigzagDirection, -verticalSpeed, 0);
        transform.position += direction * Time.deltaTime;

        // ȭ�� ��� üũ
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.1f, 0.9f);
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0, 1);
        transform.position = Camera.main.ViewportToWorldPoint(viewportPos);

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
            OnCloneDeath(); // ���� Ŭ�� 1 ��� ó��
        }
    }

    public override void TakeDamageFromArrow(float damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0, bool isAoeHit = false)
    {
        if (hp > 0)
        {
            hp -= damage;

            if (hp <= 0)
            {
                // �˹� ���� �ʱ�ȭ
                isKnockedBack = false;
                rb.velocity = Vector2.zero;

                OnCloneDeath(); // ���� Ŭ�� 1 ��� ó��
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
        //Debug.Log("���� Ŭ�� 1�� ����");

        // �˹� ���� �ʱ�ȭ
        isKnockedBack = false;
        rb.velocity = Vector2.zero; // �˹����� ���� �̵� ����

        if (BossMonster != null)
        {
            BossMonster.OnBossClone1Death();
        }

        // Fire ����Ʈ ����
        if (fireEffectInstance != null)
        {
            Destroy(fireEffectInstance);
            fireEffectInstance = null;
        }

        // Clone Fire ����Ʈ ����
        if (cloneFireEffectInstance != null)
        {
            Destroy(cloneFireEffectInstance);
            cloneFireEffectInstance = null;
        }

        Destroy(gameObject); 
    }

    // DOT �������� ���� Ŭ�п� �����ϴ� �޼���
    public override void ApplyDot(int dotDamage)
    {
        // BossClone1�� ������ �����Ƿ� DOT �������� �ٷ� ����
        if (cloneFireEffectPrefab != null && !cloneIsOnFire)
        {
            cloneFireEffectInstance = Instantiate(cloneFireEffectPrefab, transform.position, Quaternion.identity);
            cloneFireEffectInstance.transform.SetParent(transform);
            var fireSpriteRenderer = cloneFireEffectInstance.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            cloneIsOnFire = true;
        }

        StartCoroutine(ApplyCloneDotDamage(dotDamage));
    }

    private IEnumerator ApplyCloneDotDamage(int dotDamage)
    {
        // ���� ���¿� ������� DOT ������ ���������� ����
        while (hp > 0)
        {
            hp -= dotDamage;

            yield return new WaitForSeconds(1.0f); // DOT �������� 1�ʸ��� �����
        }

        if (hp <= 0)
        {
            if (cloneFireEffectInstance != null)
            {
                Destroy(cloneFireEffectInstance);
            }

            OnCloneDeath(); // DOT �������� ��� �� ȣ��
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
