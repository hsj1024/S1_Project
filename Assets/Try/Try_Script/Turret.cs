using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform target;
    public GameObject objBullet; // 총알 프리팹
    public Transform firePoint;
    private float fireCountdown = 0f;
    public static Turret Instance { get; private set; }

    void Start()
    {
        Instance = this;

        fireCountdown = Bal.Instance.TurretRt; // 발사 간격을 TurretRt로 설정 (초 단위)
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        Debug.Log("Turret Damage from Bal: " + Bal.Instance.TurretDmg);
        Debug.Log("Turret Reload Time (seconds): " + Bal.Instance.TurretRt);
    }

    void Update()
    {
        if (Bal.Instance.isTurretActive && target != null)
        {
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = Bal.Instance.TurretRt; // TurretRt에 따라 발사 간격 설정
            }
            fireCountdown -= Time.deltaTime; // 타이머 감소
        }
    }

    void Awake()
    {
        objBullet = Resources.Load<GameObject>("Obj_Bullet");
    }

    void UpdateTarget()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestMonster = null;

        foreach (GameObject monster in monsters)
        {
            if (IsInCameraView(monster))
            {
                float distanceToMonster = Vector3.Distance(transform.position, monster.transform.position);
                if (distanceToMonster < shortestDistance)
                {
                    shortestDistance = distanceToMonster;
                    nearestMonster = monster;
                }
            }
        }

        if (nearestMonster != null)
        {
            target = nearestMonster.transform;
        }
        else
        {
            target = null;
        }
    }

    bool IsInCameraView(GameObject monster)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(monster.transform.position);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }

    public void Shoot()
    {
        if (objBullet == null)
        {
            Debug.LogError("Bullet prefab is not assigned!");
            return;
        }
        GameObject bulletGO = Instantiate(objBullet, firePoint.position, firePoint.rotation);
        Collider2D collider = bulletGO.GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError("The cloned bullet does not have a Collider2D component.");
        }
        bulletGO.GetComponent<Bullet>().Seek(target);
    }
}
