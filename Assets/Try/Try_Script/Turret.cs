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

        fireCountdown = Bal.Instance.TurretRt; // 발사 간격 초기화
        objBullet = Resources.Load<GameObject>("Obj_Bullet");

        InvokeRepeating(nameof(ForceTargetUpdate), 0f, 0.5f); // 0.5초마다 강제 타겟 갱신 (백업용)
    }

    void Update()
    {
        // 매 프레임마다 더 가까운 몬스터 체크
        UpdateTargetIfCloser();

        if (Bal.Instance.isTurretActive && target != null)
        {
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = Bal.Instance.TurretRt; // TurretRt에 따라 발사 간격 설정
            }
            fireCountdown -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 기존 타겟보다 더 가까운 몬스터가 있으면 타겟 교체
    /// </summary>
    void UpdateTargetIfCloser()
    {
        if (target == null) return;

        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        float currentDistance = Vector3.Distance(transform.position, target.position);
        Transform closerTarget = target; // 기본은 기존 타겟 유지

        foreach (GameObject monster in monsters)
        {
            if (IsInCameraView(monster))
            {
                float distanceToMonster = Vector3.Distance(transform.position, monster.transform.position);
                if (distanceToMonster < currentDistance)
                {
                    currentDistance = distanceToMonster;
                    closerTarget = monster.transform; // 더 가까운 몬스터로 교체
                }
            }
        }

        target = closerTarget;
    }

    /// 강제 타겟 갱신 (기존 방식 - 0.5초마다 호출)
    void ForceTargetUpdate()
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

        target = nearestMonster != null ? nearestMonster.transform : null;
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
