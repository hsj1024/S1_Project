    using System.Collections;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class Monster : MonoBehaviour
    {
        public static Monster Instance { get; private set; }

        private Bal bal;
        public string monsterName;
        public int hp;
        public int speed;
        public float xp;
        private SpriteRenderer spriteRenderer;
        public GameObject hitPrefab;
        public float fadeOutDuration = 0.4f;
        private MonsterSpawnManager spawnManager;
        public static bool disableGameOver = false;

        public AudioClip hitSound;
        public GameObject hitAnimationPrefab;
        public float animationDuration = 0f;
        public AudioManager audioManager;

        public bool invincible = false;
        public float invincibleDuration = 0.3f;
        private float lastHitTime;
        public Rigidbody2D rb;

        public float xpDrop;

        public float knockbackForce = 1f;
        public float knockbackDuration = 0.2f;
        private bool isKnockedBack = false;
        private float knockbackTimer = 0f;
        private GameObject currentHitInstance;

        public GameObject fireEffectPrefab;
        private GameObject currentFireEffect;

        private void Start()
        {
            audioManager = AudioManager.Instance;
            if (audioManager == null)
            {
                Debug.LogError("AudioManager를 찾을 수 없습니다. AudioManager가 씬 내에 있는지 확인하세요.");
                return;
            }

            rb = GetComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
        
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spawnManager = FindObjectOfType<MonsterSpawnManager>();
        }

        private void Update()
        {
            if (isKnockedBack)
            {
                knockbackTimer -= Time.deltaTime;

                if (knockbackTimer <= 0)
                {
                    isKnockedBack = false;
                    rb.velocity = Vector2.zero;

                    if (currentHitInstance != null)
                    {
                        Destroy(currentHitInstance);
                        spriteRenderer.enabled = true;
                        RestoreFireEffect(); 
                    }
                }
            }

            if (!isKnockedBack)
            {
                MoveIfWithinBounds();
            }

            UpdateSortingOrder();

            UpdateFireEffectPosition();

            if (currentHitInstance != null)
            {
                currentHitInstance.transform.position = transform.position;
            }
        }



        private void MoveDown()
        {
            if (isKnockedBack || (currentHitInstance != null && hp <= 0)) return;

            float speedScale = 0.04f;
            transform.position += Vector3.down * speed * speedScale * Time.deltaTime;
            if (transform.position.y <= -5.0f)
            {
                if (!disableGameOver)
                {
                    LevelManager.Instance.GameOver();
                    Debug.Log("GameOver");
                }

                Destroy(gameObject);
                Debug.Log("Destroy");
            }
        }

        public static void ToggleInvincibility()
        {
            disableGameOver = !disableGameOver;
        }

        private void UpdateSortingOrder()
        {
            int sortingOrderBase = 3000;
            spriteRenderer.sortingOrder = sortingOrderBase - (int)(transform.position.y * 10);
            if (spawnManager != null)
            {
                foreach (var otherMonster in spawnManager.activeMonsters)
                {
                    if (otherMonster != this && otherMonster.transform.position.y > this.transform.position.y)
                    {
                        otherMonster.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
                    }
                }
            }
        }

        private void MoveIfWithinBounds()
        {
            if (transform.position.x >= -2 && transform.position.x <= 2.2)
            {
                MoveDown();
            }
        }

        public void TakeDamage(int damage)
        {
            if (!invincible)
            {
                hp -= damage;

                if (hp > 0)
                {
                    StartCoroutine(ShowHitEffect());
                }
                else
                {
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.IncrementMonsterKillCount();
                    }
                    StartCoroutine(FadeOutAndDestroy());
                }
                lastHitTime = Time.time;
                invincible = true;
                StartCoroutine(DisableInvincibility());
            }
        }

        private IEnumerator DisableInvincibility()
        {
            yield return new WaitForSeconds(invincibleDuration);
            invincible = false;
        }

        public void TakeDamageFromArrow(int damage, bool knockbackEnabled, Vector2 knockbackDirection, bool applyDot = false, int dotDamage = 0)
        {
            if (hp > 0)
            {
                if (knockbackEnabled && !isKnockedBack && rb != null)
                {
                    ApplyKnockback(knockbackDirection, applyDot);
                }

                TakeDamage(damage);

                if (applyDot && dotDamage > 0)
                {
                    ApplyDot(dotDamage);
                }

                StartCoroutine(PlayArrowHitAnimation());
            }
        }


        public void ApplyDot(int dotDamage)
        {
            CreateFireEffect(); // 몬스터에 화염 효과 적용

            if (currentHitInstance != null)
            {
                CreateFireEffectForHitPrefab(currentHitInstance); // hitPrefab에 화염 효과 적용
            }

            StartCoroutine(DotDamage(dotDamage));
        }

        private void CreateFireEffectForHitPrefab(GameObject hitPrefab)
        {
            GameObject fireEffect = Instantiate(fireEffectPrefab, hitPrefab.transform.position, Quaternion.identity);
            fireEffect.transform.SetParent(hitPrefab.transform);
            fireEffect.transform.localPosition = Vector3.zero;
            fireEffect.transform.position = new Vector3(fireEffect.transform.position.x, fireEffect.transform.position.y, -1);

            SpriteRenderer fireSpriteRenderer = fireEffect.GetComponent<SpriteRenderer>();
            if (fireSpriteRenderer != null)
            {
                fireSpriteRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
                fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            else
            {
                Debug.LogError("Fire effect prefab does not have a SpriteRenderer component.");
            }
        }



        private IEnumerator DotDamage(int dotDamage)
        {
            while (hp > 0)
            {
                hp -= dotDamage;
                yield return new WaitForSeconds(1.0f);
            }

            if (hp <= 0)
            {
                StartCoroutine(FadeOutAndDestroy());
            }
        }


        private void CreateFireEffect()
        {
            if (currentFireEffect == null)
            {
                currentFireEffect = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
                currentFireEffect.transform.SetParent(transform);
                currentFireEffect.transform.localPosition = Vector3.zero;
                currentFireEffect.transform.position = new Vector3(currentFireEffect.transform.position.x, currentFireEffect.transform.position.y, -1);

                SpriteRenderer fireSpriteRenderer = currentFireEffect.GetComponent<SpriteRenderer>();
                if (fireSpriteRenderer != null)
                {
                    fireSpriteRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
                    fireSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
                }
                else
                {
                    Debug.LogError("Fire effect prefab does not have a SpriteRenderer component.");
                }
            }
            else
            {
                currentFireEffect.SetActive(true);
            }
        }


        private void UpdateFireEffectPosition()
        {
            if (currentFireEffect != null)
            {
                currentFireEffect.transform.position = transform.position;
            }
        }

        private void ApplyKnockback(Vector2 knockbackDirection, bool applyDot)
        {
            if (currentHitInstance != null)
            {
                Destroy(currentHitInstance);
            }
            currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);

            if (applyDot)
            {
                CreateFireEffectForHitPrefab(currentHitInstance); // hitPrefab에 화염 효과 추가
            }

            spriteRenderer.enabled = false;
            HideFireEffect();

            rb.velocity = Vector2.zero;
            rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

            isKnockedBack = true;
            knockbackTimer = knockbackDuration;

            if (currentHitInstance != null)
            {
                StartCoroutine(MoveHitPrefabWithKnockback());
            }
        }

        private void HideFireEffect()
        {
            if (currentFireEffect != null)
            {
                currentFireEffect.SetActive(false);
            }
        }

        private void RestoreFireEffect()
        {
            if (currentFireEffect != null)
            {
                currentFireEffect.SetActive(true);
                currentFireEffect.transform.position = transform.position;
            }
        }


        private IEnumerator MoveHitPrefabWithKnockback()
        {
            while (isKnockedBack)
            {
                if (currentHitInstance != null)
                {
                    currentHitInstance.transform.position = transform.position;
                }
                yield return null;
            }

            if (hp > 0)
            {
                spriteRenderer.enabled = true;
                RestoreFireEffect(); 
                Destroy(currentHitInstance);
                currentHitInstance = null;
            }
            else
            {
                StartCoroutine(FadeOutAndDestroy());
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

        private IEnumerator ShowHitEffect()
        {
            spriteRenderer.enabled = false;
            HideFireEffect();

            if (currentHitInstance == null)
            {
                currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.3f);

            if (hp > 0)
            {
                spriteRenderer.enabled = true;
                RestoreFireEffect(); // Ensure fire effect is restored
                Destroy(currentHitInstance);
                currentHitInstance = null;
            }
        }

        public void FadeOut()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutAndDestroy());
        }

        private IEnumerator FadeOutAndDestroy()
        {
            spriteRenderer.enabled = false;

            if (currentHitInstance == null)
            {
                currentHitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
            }

            SpriteRenderer hitSpriteRenderer = currentHitInstance.GetComponent<SpriteRenderer>();

            if (hitSpriteRenderer == null)
            {
                Debug.LogError("hitPrefab does not have a SpriteRenderer component.");
                Destroy(currentHitInstance);
                Destroy(gameObject);
                yield break;
            }

            float elapsed = 0f;
            Color originalColor = hitSpriteRenderer.color;

            while (elapsed < fadeOutDuration)
            {
                float t = elapsed / fadeOutDuration;
                hitSpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(originalColor.a, 0, t));
                elapsed += Time.deltaTime;
                yield return null;
            }

            hitSpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);


            Destroy(currentHitInstance);
            currentHitInstance = null;

            DropExperience();
            Destroy(gameObject);
        }

        public void DropExperience()
        {
            if (Bal.Instance == null)
            {
                Debug.LogError("Bal instance is null. Cannot drop experience.");
                return;
            }

            float experienceAmount = xpDrop * Bal.Instance.XPM;
            Bal.Instance.AddExperience(experienceAmount);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Monster"))
            {
                Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Monster"))
            {
                Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Monster"))
            {
                Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
            }
        }
    }
