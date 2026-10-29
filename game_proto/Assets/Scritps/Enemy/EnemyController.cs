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

            if (_attackTimer >= attackDuration)
            {
                _isBiting = false;
                _attackTimer = 0f;
            }

            if (!_isBiting && distanceToPlayer > attackrange)
            {
                _state = EnumyState.Following;
                _agent.isStopped = false;
            }
            break;
    }

    UpdateAnimation();
}

        private void StartAttack()
    {
        _agent.isStopped = true;

        _isBiting = true;
        _attackTimer = 0f;

        // Здесь позже можно запустить анимацию:
        // _animator.SetTrigger("Bite");
    }



private void UpdateAnimation()
{
    if (animator == null || _agent == null)
        return;

    if (_state == EnumyState.Attacking)
    {
        animator.SetFloat("Speed", 0f);
        return;
    }

    if (_agent.velocity.magnitude < 0.1f)
    {
        animator.SetFloat("Speed", 0f);
    }
    else if (_state == EnumyState.Following)
    {
        animator.SetFloat("Speed", 1f);
    }
    else
    {
        animator.SetFloat("Speed", 0.5f);
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
        // Если во время поиска зомби увидел игрока
        if (distanceToPlayer <= detectionRange && CanSeePlayer())
        {
            _state = EnumyState.Following;
            _hasHeardSound = false;
            _investigateTimer = 0f;
            return;
        }
        _agent.speed = walkSpeed;
        _agent.isStopped = false;
        _agent.SetDestination(_lastHeardPosition);

        // Если дошли до места звука
        if (!_agent.pathPending &&
            _agent.remainingDistance <= stopAtDistance)
        {
            _agent.isStopped = true;

            _investigateTimer += Time.deltaTime;

            // Некоторое время "ищем" игрока
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
        // Слишком тихий звук
        if (volume < hearingThreshold)
            return;

        // Расстояние до источника звука
        float distance = Vector3.Distance(transform.position, soundPosition);

        // Чем громче звук, тем дальше его слышно
        float soundRadius =
            hearingRange * Mathf.Clamp01(volume / maxSoundVolume);

        // Звук слишком далеко
        if (distance > soundRadius)
            return;

        // Если уже атакует - звук игнорируем
        if (_state == EnumyState.Attacking)
            return;

        _lastHeardPosition = soundPosition;
        _hasHeardSound = true;

        _investigateTimer = 0f;
        _timeSinceLostPlayer = 0f;

        _state = EnumyState.Investigating;

        _agent.isStopped = false;
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
        _isWaiting = true;

        _agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        _agent.isStopped = false;

        GoToNextPatrolpoint();

        _isWaiting = false;
    }


    private void GoToClosestPatrolPoint()
    {
        if (patrolPoints.Length == 0)
        {
            return;
        }

        var closestIndex = 0;
        var closestDistance = float.MaxValue;

        for (var i = 0; i < patrolPoints.Length; i++)
        {
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
        if (patrolPoints.Length == 0)
        {
            return;
        }

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
}