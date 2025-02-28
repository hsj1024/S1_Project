using UnityEngine;

public class Turret : MonoBehaviour
{
    public Transform target;
    public GameObject objBullet; // �Ѿ� ������
    public Transform firePoint;
    private float fireCountdown = 0f;
    public static Turret Instance { get; private set; }

    void Start()
    {
        Instance = this;

        fireCountdown = Bal.Instance.TurretRt; // �߻� ���� �ʱ�ȭ
        objBullet = Resources.Load<GameObject>("Obj_Bullet");

        InvokeRepeating(nameof(ForceTargetUpdate), 0f, 0.5f); // 0.5�ʸ��� ���� Ÿ�� ���� (�����)
    }

    void Update()
    {
        // �� �����Ӹ��� �� ����� ���� üũ
        UpdateTargetIfCloser();

        if (Bal.Instance.isTurretActive && target != null)
        {
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = Bal.Instance.TurretRt; // TurretRt�� ���� �߻� ���� ����
            }
            fireCountdown -= Time.deltaTime;
        }
    }

    /// <summary>
    /// ���� Ÿ�ٺ��� �� ����� ���Ͱ� ������ Ÿ�� ��ü
    /// </summary>
    void UpdateTargetIfCloser()
    {
        if (target == null) return;

        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        float currentDistance = Vector3.Distance(transform.position, target.position);
        Transform closerTarget = target; // �⺻�� ���� Ÿ�� ����

        foreach (GameObject monster in monsters)
        {
            if (IsInCameraView(monster))
            {
                float distanceToMonster = Vector3.Distance(transform.position, monster.transform.position);
                if (distanceToMonster < currentDistance)
                {
                    currentDistance = distanceToMonster;
                    closerTarget = monster.transform; // �� ����� ���ͷ� ��ü
                }
            }
        }

        target = closerTarget;
    }

    /// ���� Ÿ�� ���� (���� ��� - 0.5�ʸ��� ȣ��)
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
