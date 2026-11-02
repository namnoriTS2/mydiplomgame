
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float health = 100f;

    private bool isDead = false;
    public bool IsDead => isDead;

    private Animator animator;
    private EnemyController enemyController;
    private NavMeshAgent agent;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        enemyController = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
    }

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
        Die();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Отключаем AI
        if (enemyController != null)
            enemyController.enabled = false;

        // Отключаем NavMeshAgent
        if (agent != null)
            agent.enabled = false;

        // Запускаем анимацию смерти
        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }
    }
}
