
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float health = 100f;

    private bool isDead = false;

    public bool IsDead => isDead;

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

    public void StealthKill()
    {
        if (isDead)
            return;

        health = 0f;

        Debug.Log("Зомби убит скрытно!");

        Die();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Отключаем AI зомби
        EnemyController enemy = GetComponent<EnemyController>();

        if (enemy != null)
            enemy.enabled = false;

        Debug.Log("Зомби умер!");

        Destroy(gameObject);
    }
}
