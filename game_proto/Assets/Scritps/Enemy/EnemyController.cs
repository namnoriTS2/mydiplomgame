using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnumyState
{
    Patrolling,
    Investigating, 
    Following,
    Attacking
}

public class EnemyController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float stopAtDistance = 0.5f;

    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float losePlayerTime = 3f;

    [SerializeField] private float attackrange = 1.2f;
    [SerializeField] private float attackDuration = 0.8f;

    [SerializeField] private float hearingThreshold = 0.3f;
    [SerializeField] private float hearingRange = 15f;
    [SerializeField] private float investigateTime = 3f;
    [SerializeField] private float maxSoundVolume = 1f;
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 3.5f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float attackHitTime = 0.35f;
    private ZombieAudio zombieAudio;

    private bool attackDamageApplied;

    private NavMeshAgent _agent;

    private int _curentPatrolIndex;
    private bool _isWaiting;

    private EnumyState _state = EnumyState.Patrolling;

    private float _timeSinceLostPlayer;

    private bool _isBiting;
    private float _attackTimer;

    private bool _hasHeardSound;
    private Vector3 _lastHeardPosition;
    private float _investigateTimer;

    // Список всех зомби в сцене
    private static readonly List<EnemyController> enemies = new List<EnemyController>();


    private void Awake()
    {
        zombieAudio = GetComponent<ZombieAudio>();
        _agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (_agent != null)
        {
            _agent.speed = walkSpeed;
        }
    }


    private void OnEnable()
    {
        if (!enemies.Contains(this))
        {
            enemies.Add(this);
        }
    }


    private void OnDisable()
    {
        enemies.Remove(this);
    }


    private void Start()
    {
        GoToNextPatrolpoint();
    }


    
private void Update()
{
    if (!enabled || _agent == null || !_agent.enabled || !_agent.isOnNavMesh)
        return;

    if (player == null)
    {
        UpdateAnimation();
        return;
    }
    if (_agent == null || !_agent.isOnNavMesh)
        return;

    if (player == null)
    {
        UpdateAnimation();
        return;
    }

    float distanceToPlayer =
        Vector3.Distance(player.position, transform.position);

    switch (_state)
    {
        case EnumyState.Patrolling:
            Patrol();

            if (distanceToPlayer <= detectionRange && CanSeePlayer())
            {
                _state = EnumyState.Following;
                _hasHeardSound = false;
            }
            break;

        case EnumyState.Investigating:
            InvestigateSound(distanceToPlayer);
            break;

        case EnumyState.Following:
            FollowPlayer();

            if (distanceToPlayer <= attackrange)
            {
                _state = EnumyState.Attacking;
                StartAttack();
                break;
            }

            if (CanSeePlayer())
            {
                _timeSinceLostPlayer = 0f;
            }
            else
            {
                _timeSinceLostPlayer += Time.deltaTime;

                if (_timeSinceLostPlayer >= losePlayerTime)
                {
                    _state = EnumyState.Patrolling;
                    _timeSinceLostPlayer = 0f;
                    GoToClosestPatrolPoint();
                }
            }
            break;

       case EnumyState.Attacking:

    Attack();

    _attackTimer += Time.deltaTime;

    // Урон в момент удара
    if (!attackDamageApplied && _attackTimer >= attackHitTime)
    {
        ApplyAttackDamage();
        attackDamageApplied = true;
    }

    // Завершение атаки
   if (_attackTimer >= attackCooldown)
{
    _isBiting = false;
    _attackTimer = 0f;

    if (distanceToPlayer <= attackrange)
    {
        StartAttack();
    }
    else
    {
        // Возвращаемся к преследованию
        _state = EnumyState.Following;

        _agent.isStopped = false;
        _agent.speed = runSpeed;

        // Сбрасываем анимацию атаки
        animator.ResetTrigger("Attack");

        // Сразу обновляем анимацию движения
        animator.SetFloat("Speed", 1f);

        _agent.SetDestination(player.position);
    }
}

    break;
    }

    UpdateAnimation();
}

      private void StartAttack()
{
    zombieAudio?.PlayAttackSound();
    _agent.isStopped = true;

    _isBiting = true;
    _attackTimer = 0f;
    attackDamageApplied = false;

    animator.SetTrigger("Attack");
}




private void UpdateAnimation()
{
    if (animator == null || _agent == null)
        return;

    if (_state == EnumyState.Attacking)
    {
        animator.SetFloat("Speed", 0f);
        zombieAudio?.StopMovementSound();
        return;
    }

    float currentSpeed = _agent.velocity.magnitude;

    if (currentSpeed < 0.1f)
    {
        animator.SetFloat("Speed", 0f);
        zombieAudio?.StopMovementSound();
    }
    else if (_state == EnumyState.Following)
    {
        animator.SetFloat("Speed", 1f);
        zombieAudio?.PlayMovementSound(true);
    }
    else
    {
        animator.SetFloat("Speed", 0.5f);
        zombieAudio?.PlayMovementSound(false);
    }
}


    private void Attack()
    {
        _agent.isStopped = true;

        var direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Здесь позже можно наносить урон игроку
    }


    // Этот метод можно будет вызывать Animation Event
    private void OnBiteAnimationEnd()
    {
        _isBiting = false;
    }

    private void FollowPlayer()
{
    if (_agent == null || player == null)
        return;

    _agent.isStopped = false;
    _agent.speed = runSpeed;
    _agent.SetDestination(player.position);
}

    private void InvestigateSound(float distanceToPlayer)
{
    // Если увидел игрока — начинает преследование
    if (distanceToPlayer <= detectionRange && CanSeePlayer())
    {
        _state = EnumyState.Following;
        _hasHeardSound = false;
        _investigateTimer = 0f;
        return;
    }

    // Если ещё идёт к месту звука — продолжаем движение
    if (!_agent.pathPending &&
        _agent.remainingDistance <= stopAtDistance)
    {
        _agent.isStopped = true;

        _investigateTimer += Time.deltaTime;

        if (_investigateTimer >= investigateTime)
        {
            _hasHeardSound = false;
            _investigateTimer = 0f;

            _agent.isStopped = false;
            _state = EnumyState.Patrolling;

            GoToClosestPatrolPoint();
        }
    }
}


    // Этот метод вызывается, когда где-то произошёл звук
    public void HearSound(Vector3 soundPosition, float volume)
{
    if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
        return;

    if (volume < hearingThreshold)
        return;

    float distance = Vector3.Distance(transform.position, soundPosition);

    float soundRadius =
        hearingRange * Mathf.Clamp01(volume / maxSoundVolume);

    if (distance > soundRadius)
        return;

    if (_state == EnumyState.Attacking)
        return;

    Debug.Log($"{gameObject.name} услышал звук! Расстояние: {distance:F1}, радиус: {soundRadius:F1}");

    _lastHeardPosition = soundPosition;
    _hasHeardSound = true;

    _investigateTimer = 0f;
    _timeSinceLostPlayer = 0f;

    _state = EnumyState.Investigating;

    _agent.isStopped = false;
    _agent.speed = walkSpeed;
    _agent.SetDestination(_lastHeardPosition);
}


    // Этот метод вызывается игроком
    public static void MakeSound(Vector3 soundPosition, float volume)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null)
            {
                enemies[i].HearSound(soundPosition, volume);
            }
        }
    }

    private void Patrol()
    {
        _agent.speed = walkSpeed;
        if (_isWaiting)
        {
            return;
        }

        if (!_agent.pathPending &&
            _agent.remainingDistance <= stopAtDistance)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }


    private IEnumerator WaitAtPatrolPoint()
{
    if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
        yield break;

    _isWaiting = true;

    _agent.isStopped = true;

    yield return new WaitForSeconds(patrolWaitTime);

    // Проверяем, жив ли зомби и активен ли агент
    if (!enabled || _agent == null || !_agent.enabled || !_agent.isOnNavMesh)
        yield break;

    _isWaiting = false;

    if (_state == EnumyState.Patrolling)
    {
        _agent.isStopped = false;
        GoToNextPatrolpoint();
    }
}


   private void GoToClosestPatrolPoint()
{
    if (!enabled || _agent == null || !_agent.enabled || !_agent.isOnNavMesh)
        return;

    if (patrolPoints == null || patrolPoints.Length == 0)
        return;

    var closestIndex = 0;
    var closestDistance = float.MaxValue;

    for (var i = 0; i < patrolPoints.Length; i++)
    {
        if (patrolPoints[i] == null)
            continue;

        var distance = Vector3.Distance(
            transform.position,
            patrolPoints[i].position
        );

        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestIndex = i;
        }
    }

    _curentPatrolIndex = closestIndex;

    _agent.isStopped = false;

    _agent.SetDestination(
        patrolPoints[_curentPatrolIndex].position
    );
}


    private void GoToNextPatrolpoint()
{
    if (!enabled || _agent == null || !_agent.enabled || !_agent.isOnNavMesh)
        return;

    if (patrolPoints == null || patrolPoints.Length == 0)
        return;

    _agent.isStopped = false;

    _agent.SetDestination(
        patrolPoints[_curentPatrolIndex].position
    );

    _curentPatrolIndex =
        (_curentPatrolIndex + 1) % patrolPoints.Length;
}
    private bool CanSeePlayer()
    {
        return IsFacingPlayer() && HasClearPathToPlayer();
    }


    private bool IsFacingPlayer()
    {
        var dirToPlayer =
            (player.position - transform.position).normalized;

        var angle =
            Vector3.Angle(transform.forward, dirToPlayer);

        return angle <= viewAngle / 2f;
    }
    private bool HasClearPathToPlayer()
    {
        var dirToPlayer =
            player.position - transform.position;

        if (Physics.Raycast(
            transform.position,
            dirToPlayer.normalized,
            out RaycastHit hit,
            dirToPlayer.magnitude))
        {
            return hit.transform == player;
        }

        return true;
    }
    private void ApplyAttackDamage()
{
    if (player == null)
        return;

    Health playerHealth = player.GetComponent<Health>();

    if (playerHealth != null)
    {
        playerHealth.TakeDmage();
    }
}
}