using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class StealthKill : MonoBehaviour
{
    [Header("Настройки")]
    public float killDistance = 1.5f;
    public float searchRadius = 2f;

    [Range(-1f, 1f)]
    public float behindDot = -0.5f;

    [Header("Ссылки")]
    public Transform playerCamera;
    public TMP_Text promptText;

    [Header("Audio")]
    [SerializeField] private PlayerAudio playerAudio;

    private EnemyHealth target;

    private void Awake()
    {
        if (playerAudio == null)
            playerAudio = GetComponent<PlayerAudio>();
    }

    private void Update()
    {
        FindTarget();

        if (target != null &&
            Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            PerformKill();
        }
    }

    private void FindTarget()
    {
        target = null;
        float closestDistance = killDistance;

        EnemyHealth[] enemies =
            FindObjectsByType<EnemyHealth>(
                FindObjectsSortMode.None
            );

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            Vector3 toPlayer =
                transform.position - enemy.transform.position;

            float distance = toPlayer.magnitude;

            if (distance > killDistance ||
                distance >= closestDistance)
                continue;

            // Игрок должен находиться позади зомби
            Vector3 directionToPlayer = toPlayer.normalized;

            float dot = Vector3.Dot(
                enemy.transform.forward,
                directionToPlayer
            );

            if (dot > behindDot)
                continue;

            // Игрок должен смотреть на зомби
            Vector3 toEnemy =
                enemy.transform.position - playerCamera.position;

            if (toEnemy.sqrMagnitude < 0.001f)
                continue;

            float lookDot = Vector3.Dot(
                playerCamera.forward,
                toEnemy.normalized
            );

            if (lookDot < 0.5f)
                continue;

            target = enemy;
            closestDistance = distance;
        }

        if (promptText != null)
            promptText.gameObject.SetActive(target != null);
    }

    private void PerformKill()
    {
        if (target == null)
            return;

        EnemyHealth enemyToKill = target;
        target = null;

        HidePrompt();

        // Звук скрытого убийства
        playerAudio?.PlayStealthKillSound();

        enemyToKill.StealthKill();
    }

    private void HidePrompt()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    public void OnStealthKill(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PerformKill();
        }
    }
}