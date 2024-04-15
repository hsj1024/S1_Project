using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public string monsterName;
    public int hp;
    public int speed;
    public double xp;
    private SpriteRenderer spriteRenderer;
    public GameObject hitPrefab;
    public float fadeOutDuration = 0.3f;  // Fade out 속도 조절을 위한 변수 추가

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(string name, int health, int moveSpeed, int experience, GameObject hitEffect)
    {
        monsterName = name;
        hp = health;
        speed = moveSpeed;
        xp = experience;
        hitPrefab = hitEffect;
    }

    void Update()
    {
        if (transform.position.x >= -2 && transform.position.x <= 2.2)
        {
            MoveDown();
        }
        MonsterSpawnManager spawnManager = FindObjectOfType<MonsterSpawnManager>();
        foreach (var otherMonster in spawnManager.activeMonsters)
        {
            if (otherMonster != this && otherMonster.transform.position.y > this.transform.position.y)
            {
                otherMonster.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
            }
        }
        UpdateSortingOrder();
    }

    void MoveDown()
    {
        float speedScale = 0.04f;
        transform.position += Vector3.down * speed * speedScale * Time.deltaTime;
        if (transform.position.y <= -4.5f)
        {
            Destroy(gameObject);
        }
    }

    void UpdateSortingOrder()
    {
        int sortingOrderBase = 5000;
        spriteRenderer.sortingOrder = sortingOrderBase - (int)(transform.position.y * 10);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Arrow"))
        {
            Arr arrowScript = collision.gameObject.GetComponent<Arr>();
            if (arrowScript != null)
            {
                TakeDamage(arrowScript.damage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            InstantiateHitEffect();
            Destroy(gameObject);
        }
    }

    private void InstantiateHitEffect()
    {
        if (hitPrefab != null)
        {
            GameObject hitInstance = Instantiate(hitPrefab, transform.position, Quaternion.identity);
            if (hitInstance != null)
            {
                StartCoroutine(AnimateHitEffect(hitInstance));
            }
        }
    }

    private IEnumerator AnimateHitEffect(GameObject hitInstance)
    {
        Debug.Log("Coroutine started. Waiting 0.5 seconds...");
        yield return new WaitForSeconds(0.5f);

        SpriteRenderer hitSpriteRenderer = hitInstance.GetComponent<SpriteRenderer>();

        Debug.Log("SpriteRenderer found. Starting fade out...");
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            float t = elapsed / fadeOutDuration;
            hitSpriteRenderer.color = Color.Lerp(hitSpriteRenderer.color, new Color(hitSpriteRenderer.color.r, hitSpriteRenderer.color.g, hitSpriteRenderer.color.b, 0), t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Fade out completed. Destroying hit instance...");
        Destroy(hitInstance);  // 오브젝트 제거
    }


}
