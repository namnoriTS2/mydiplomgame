// using System.Collections;
// using System.Runtime.CompilerServices;
// using TMPro.Examples;
// using UnityEditor.Experimental.GraphView;
// using UnityEngine;
// using UnityEngine.AI;
// using UnityEngine.UI;

// public enum EnumyState
// {
//     Patrolling, 
//     Following, 
//     Attacking
// }

// public class EnemyController : MonoBehaviour
// {
//     [Header("References")]
//     [SerializeField] private Transform[] patrolPoints;
//     [SerializeField] private Transform player;

//     [Header("Settings")]
//     [SerializeField] private float patrolWaitTime = 2f;
//     [SerializeField] private float stopAtDistance = 0.5f;
//     [SerializeField] private float detectionRange = 5f;
//     [SerializeField] private float viewAngle = 90f;
//     [SerializeField] private float losePlayerTime = 3f;
//     [SerializeField] private float attackrange = 1.2f;


//     private NavMeshAgent _agent;
//     //private Animator _animator;
//     private int _curentPatrolIndex;
//     private bool _isWaiting;
//     private EnumyState _state = EnumyState.Patrolling;

//     private float _timeSinceLostPlayer;

//     private bool _isBiting;

//     private void Awake()
//     {
//         _agent = GetComponent<NavMeshAgent>();
//        // _animator = GetComponent<Animator>();
//     }

//     private void Start()
//     {
//         GoToNextPatrolpoint();
//     }

//     private void Update()
//     {
//         var distanceToPlayer = Vector3.Distance(player.position, transform.position);
//         switch (_state)
//         {
//             case EnumyState.Patrolling:
//                 Patrol();
//                 if(distanceToPlayer <= detectionRange && CanSeePlayer())
//                 {
//                     _state = EnumyState.Following;
//                 }
//                 break;
//             case EnumyState.Following:
//                 FollowPlayer();
//                 if(distanceToPlayer <= attackrange){
//                     _state = EnumyState.Attacking;
//                     StartAttack();
//                 }
//                 if (!CanSeePlayer())
//                 {
//                     _timeSinceLostPlayer += Time.deltaTime;
//                     if(_timeSinceLostPlayer >= losePlayerTime)
//                     {
//                         _state = EnumyState.Patrolling;
//                         GoToClosestPatrolPoint();
//                     }
//                 }
//                 else
//                 {
//                     _timeSinceLostPlayer = 0f;
//                 }
//                 break;
//                 case EnumyState.Attacking:
//                 Attack();
//                 if (!_isBiting && distanceToPlayer > attackrange)
//                 {
//                     _state = EnumyState.Following;
//                     _agent.isStopped = false;
//                 }
//                 break;
//         }
//         // UpdateAnimations();

//     }

//     private void StartAttack()
//     {
//         _agent.isStopped = true;
//         _isBiting = true;
//         //_animator.SetTrigger("Bite", Bite)
//     }

//     private void Attack()
//     {
//         _agent.isStopped = true;
//         var direction = (player.position - transform.position).normalized;
//         direction.y = 0f;
//         if(direction != Vector3.zero)
//         {
//             transform.rotation = Quaternion.LookRotation(direction);
//         }
//     }

//     private void OnBiteAnimationEnd()
//     {
//         _isBiting = false;
//     }

//     private void FollowPlayer()
//     {
//         _agent.SetDestination(player.position);
//     }

//     private void Patrol()
//     {
//         if (_isWaiting)
//         {
//             return;
//         }
//         if(!_agent.pathPending && _agent.remainingDistance <= stopAtDistance)
//         {
//             StartCoroutine(WaitAtPatrolPoint());
//         }

//     }

//     private IEnumerator WaitAtPatrolPoint()
//     {
//         _isWaiting = true;
//         _agent.isStopped = true;

//         yield return new WaitForSeconds(patrolWaitTime);

//         _agent.isStopped = false;

//         GoToNextPatrolpoint();
//         _isWaiting = false;
//     }

//     private void GoToClosestPatrolPoint()
//     {
//         if(patrolPoints.Length == 0)
//         {
//             return;
//         }
//         var closestIndex = 0;
//         var closestDistance = float.MaxValue;

//         for(var i = 0; i<patrolPoints.Length; i++)
//         {
//             var distance = Vector3.Distance(transform.position, patrolPoints[i].position);
//             if(distance < closestDistance)
//             {
//                 closestDistance = distance;
//                 closestIndex = i;
//             }
//         }

//         _curentPatrolIndex = closestIndex;
//         _agent.SetDestination(patrolPoints[_curentPatrolIndex].position);

//     }

//     private void GoToNextPatrolpoint()
//     {
//         if(patrolPoints.Length == 0)
//         {
//             return;
//         }
//         _agent.SetDestination(patrolPoints[_curentPatrolIndex].position);
//         _curentPatrolIndex = (_curentPatrolIndex+1)%patrolPoints.Length;

//     }
//     //когда анимации будут, добавить потом это все
//     // private void UpdateAnimations()
//     // {
//     //     var isMoving = _agent.velocity.sqrMagnitude > 0.01f;
//     //     _animator.SetBool("IsWalking", isMoving);
//     // }

//     private bool CanSeePlayer()
//     {
//         return IsFacingPlayer() && HasClearPathToPlayer();
//     }

//     private bool IsFacingPlayer()
//     {
//         var dirToPlayer = (player.position - transform.position).normalized;
//         var angle = Vector3.Angle(transform.forward, dirToPlayer);
//         return angle <= viewAngle / 2f;
//     }

    
// private bool HasClearPathToPlayer()
// {
//     Vector3 origin = transform.position + Vector3.up * 1.5f;
//     Vector3 target = player.position + Vector3.up * 1f;

//     Vector3 direction = target - origin;
//     float distance = direction.magnitude;

//     if (Physics.Raycast(
//         origin,
//         direction.normalized,
//         out RaycastHit hit,
//         distance))
//     {
//         // Проверяем сам объект игрока и его дочерние объекты
//         return hit.transform == player ||
//                hit.transform.IsChildOf(player);
//     }

//     return true;
// }

// }
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
        if (player == null)
            return;

        var distanceToPlayer = Vector3.Distance(player.position, transform.position);

        switch (_state)
        {
            case EnumyState.Patrolling:

                Patrol();

                // Зомби увидел игрока
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

                // Игрок вошёл в радиус атаки
                if (distanceToPlayer <= attackrange)
                {
                    _state = EnumyState.Attacking;
                    StartAttack();
                    break;
                }

                // Если игрок виден
                if (CanSeePlayer())
                {
                    _timeSinceLostPlayer = 0f;
                }
                else
                {
                    // Начинаем отсчитывать время потери игрока
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

                // Заканчиваем укус
                if (_attackTimer >= attackDuration)
                {
                    _isBiting = false;
                    _attackTimer = 0f;
                }

                // Если игрок отошёл - снова преследуем
                if (!_isBiting && distanceToPlayer > attackrange)
                {
                    _state = EnumyState.Following;
                    _agent.isStopped = false;
                }

                break;
        }
    }
        private void StartAttack()
    {
        _agent.isStopped = true;

        _isBiting = true;
        _attackTimer = 0f;

        // Здесь позже можно запустить анимацию:
        // _animator.SetTrigger("Bite");
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
        _agent.isStopped = false;
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