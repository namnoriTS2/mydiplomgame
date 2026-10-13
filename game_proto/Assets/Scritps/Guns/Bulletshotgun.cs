
using UnityEngine;

public class Bulletshotgun : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;
    public float damage = 25f;

    private Rigidbody rb;
    private Collider bulletCollider;

void Start()
{
    rb = GetComponent<Rigidbody>();
    bulletCollider = GetComponent<Collider>();

    if (rb == null)
    {
        Debug.LogError("У дробинки отсутствует Rigidbody!");
        return;
    }

    // Дробинки не сталкиваются друг с другом
    Bulletshotgun[] allBullets =
        FindObjectsByType<Bulletshotgun>(FindObjectsSortMode.None);

    foreach (Bulletshotgun otherBullet in allBullets)
    {
        if (otherBullet == this)
            continue;

        Collider otherCollider = otherBullet.GetComponent<Collider>();

        if (bulletCollider != null && otherCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, otherCollider);
        }
    }

    rb.linearVelocity = transform.forward * speed;
    Destroy(gameObject, lifetime);
}


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Пуля столкнулась с: " + collision.gameObject.name);

        EnemyHealth enemyHealth =
            collision.gameObject.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.ApplyDamage(damage);
        }

        Destroy(gameObject);
    }
}
