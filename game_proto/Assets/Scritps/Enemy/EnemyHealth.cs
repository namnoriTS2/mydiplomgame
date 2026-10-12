
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float health = 100f;

    private bool isDead = false;

    public void ApplyDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        health -= damage;

        Debug.Log("Зомби получил урон: " + damage);
        Debug.Log("Осталось здоровья: " + Mathf.Max(health, 0f));

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Зомби умер!");
        Destroy(gameObject);
    }
}
