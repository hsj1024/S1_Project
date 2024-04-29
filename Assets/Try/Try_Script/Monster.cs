using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Monster : MonoBehaviour
{

    public static Monster Instance { get; private set; }


    private Bal bal; // 발리스타의 인스턴스를 저장할 변수
    public string monsterName;
    public int hp;
    public int speed;
    public float xp;
    private SpriteRenderer spriteRenderer;
    public GameObject hitPrefab;
    public float fadeOutDuration = 0.4f; // 페이드 아웃 시간
    private MonsterSpawnManager spawnManager;
    public static bool disableGameOver = false;

    public AudioClip hitSound;
    public GameObject hitAnimationPrefab;
    public float animationDuration = 0f;
    public AudioManager audioManager; // AudioManager 참조

    public float xpDrop; // 몬스터가 드랍하는 경험치

    private void Start()
    {
        // AudioManager를 찾아서 할당합니다.
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("AudioManager를 찾을 수 없습니다. AudioManager가 씬 내에 있는지 확인하세요.");
            return;
        }
       
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnManager = FindObjectOfType<MonsterSpawnManager>();
    }

    public void Initialize(string name, int health, int moveSpeed, int experience, GameObject hitEffect, float xpDropAmount, Bal balInstance)
    {
        monsterName = name;
        hp = health;
        speed = moveSpeed;
        xp = experience;
        hitPrefab = hitEffect;
        xpDrop = xpDropAmount; // 몬스터가 드랍하는 경험치 설정
        bal = balInstance; // 발리스타의 인스턴스 저장


    }

    private void Update()
    {
        MoveIfWithinBounds();
        UpdateSortingOrder();
    }

    private void MoveDown()
    {
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
        hp -= damage;
        if (hp > 0)
        {
            StartCoroutine(ShowHitEffect());
        }
        else
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.IncrementMonsterKillCount(); // 몬스터 처치 카운트 증가
            }
            spriteRenderer.enabled = false;
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    public void TakeDamageFromArrow(int damage)
    {

        TakeDamage(damage); 
        StartCoroutine(PlayArrowHitAnimation()); 
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
            audioManager.PlayMonsterHitSound(); // AudioManager에서 화살 발사 소리를 재생하는 메서드 호출
        }

        yield return new WaitForSeconds(animationDuration); 
    }



    private IEnumerator ShowHitEffect()
    {
        spriteRenderer.enabled = false;
        GameObject hitInstance = null;
        if (hitPrefab != null)
        {
            hitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        }
        yield return new WaitForSeconds(0.5f);

        if (hitInstance != null)
        {
            Destroy(hitInstance);
        }

        if (hp > 0)
        {
            spriteRenderer.enabled = true;
        }
    }


    private IEnumerator FadeOutAndDestroy()
    {
        GameObject hitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
        SpriteRenderer hitSpriteRenderer = hitInstance.GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            float t = elapsed / fadeOutDuration;
            hitSpriteRenderer.color = Color.Lerp(hitSpriteRenderer.color, new Color(hitSpriteRenderer.color.r, hitSpriteRenderer.color.g, hitSpriteRenderer.color.b, 0), t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(hitInstance);
        DropExperience(); // 몬스터가 죽었을 때 경험치를 드랍합니다.

        Destroy(gameObject);
    }


    public void DropExperience()
    {
        if (Bal.Instance == null)
        {
            Debug.LogError("Bal instance is null. Cannot drop experience.");
            return;
        }

        float experienceAmount = xpDrop * Bal.Instance.XPM; // 발리스타의 경험치 배수를 곱합니다.
        Bal.Instance.AddExperience(experienceAmount); // 경험치 누적
    }

}
