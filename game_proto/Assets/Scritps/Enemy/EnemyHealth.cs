
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
    private ZombieAudio zombieAudio;

    private void Awake()
    {
        zombieAudio = GetComponent<ZombieAudio>();
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
            return;
        }
        zombieAudio?.PlayHitSound();
        // Проигрываем анимацию получения урона
        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.SetTrigger("Hit");

            Debug.Log("Анимация получения урона запущена");
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
        {
            if (agent.enabled && agent.isOnNavMesh)
                agent.isStopped = true;

            agent.enabled = false;
        }

        // Запускаем анимацию смерти
        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            zombieAudio?.PlayDeathSound();
            animator.SetTrigger("Dead");
        }
    }
}
