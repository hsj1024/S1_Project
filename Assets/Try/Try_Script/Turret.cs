using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform target;
    public GameObject objBullet; // 이 변수는 총알 프리팹을 참조해야 합니다.
    public Transform firePoint;
    private float fireCountdown = 0f;

    void Start()
    {

        fireCountdown = 1f / Bal.Instance.TurretRt;
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        //Debug.Log("Turret Damage from Bal: " + Bal.Instance.TurretDmg);
        //Debug.Log("Turret Reload Time from Bal: " + Bal.Instance.TurretRt);
    }

    void Update()
    {
        gameObject.SetActive(Bal.Instance.isTurretActive); // 터렛의 가시성을 Bal의 isTurretActive 값에 따라 설정

        if (target != null)
        {
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / Bal.Instance.TurretRt;
            }
            fireCountdown -= Time.deltaTime;
        }
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

    void Shoot()
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
