using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Transform target;
    public float speed = 30f;
    private Vector3 direction;

    public void Seek(Transform _target)
    {
        target = _target;
        direction = (target.position - transform.position).normalized;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += direction * speed * Time.deltaTime;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(Bal.Instance.TurretDmg);
                //Debug.Log("Dealt damage: " + Bal.Instance.TurretDmg);
            }
            Destroy(gameObject);
        }
    }

}