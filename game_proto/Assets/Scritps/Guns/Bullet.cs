using UnityEngine;

public class Bullet : MonoBehaviour
{
   public float speed = 15f;
   public float lifetime = 3f;
    public float damage = 25f;

   private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);

    }

    private void OnCollisionEnter(Collision collision)
{
    EnemyHealth enemyHealth =
        collision.gameObject.GetComponentInParent<EnemyHealth>();

    if (enemyHealth != null)
    {
        enemyHealth.ApplyDamage(damage);
    }

    Destroy(gameObject);
}
}
