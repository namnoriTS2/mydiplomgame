using System.Collections;
using System.Runtime.CompilerServices;
using TMPro.Examples;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnumyState
{
    Patrolling, 
    Following, 
    Attacking
}

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float stopAtDistance = 0.5f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float losePlayerTime = 3f;
    [SerializeField] private float attackrange = 1.2f;


    private NavMeshAgent _agent;
    //private Animator _animator;
    private int _curentPatrolIndex;
    private bool _isWaiting;
    private EnumyState _state = EnumyState.Patrolling;

    private float _timeSinceLostPlayer;

    private bool _isBiting;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
       // _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GoToNextPatrolpoint();
    }

    private void Update()
    {
        var distanceToPlayer = Vector3.Distance(player.position, transform.position);
        switch (_state)
        {
            case EnumyState.Patrolling:
                Patrol();
                if(distanceToPlayer <= detectionRange && CanSeePlayer())
                {
                    _state = EnumyState.Following;
                }
                break;
            case EnumyState.Following:
                FollowPlayer();
                if(distanceToPlayer <= attackrange){
                    _state = EnumyState.Attacking;
                    StartAttack();
                }
                if (!CanSeePlayer())
                {
                    _timeSinceLostPlayer += Time.deltaTime;
                    if(_timeSinceLostPlayer >= losePlayerTime)
                    {
                        _state = EnumyState.Patrolling;
                        GoToClosestPatrolPoint();
                    }
                }
                else
                {
                    _timeSinceLostPlayer = 0f;
                }
                break;
                case EnumyState.Attacking:
                Attack();
                if (!_isBiting && distanceToPlayer > attackrange)
                {
                    _state = EnumyState.Following;
                    _agent.isStopped = false;
                }
                break;
        }
        // UpdateAnimations();

    }

    private void StartAttack()
    {
        _agent.isStopped = true;
        _isBiting = true;
        //_animator.SetTrigger("Bite", Bite)
    }

    private void Attack()
    {
        _agent.isStopped = true;
        var direction = (player.position - transform.position).normalized;
        direction.y = 0f;
        if(direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnBiteAnimationEnd()
    {
        _isBiting = false;
    }

    private void FollowPlayer()
    {
        _agent.SetDestination(player.position);
    }

    private void Patrol()
    {
        if (_isWaiting)
        {
            return;
        }
        if(!_agent.pathPending && _agent.remainingDistance <= stopAtDistance)
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
        if(patrolPoints.Length == 0)
        {
            return;
        }
        var closestIndex = 0;
        var closestDistance = float.MaxValue;

        for(var i = 0; i<patrolPoints.Length; i++)
        {
            var distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        _curentPatrolIndex = closestIndex;
        _agent.SetDestination(patrolPoints[_curentPatrolIndex].position);

    }

    private void GoToNextPatrolpoint()
    {
        if(patrolPoints.Length == 0)
        {
            return;
        }
        _agent.SetDestination(patrolPoints[_curentPatrolIndex].position);
        _curentPatrolIndex = (_curentPatrolIndex+1)%patrolPoints.Length;

    }
    //когда анимации будут, добавить потом это все
    // private void UpdateAnimations()
    // {
    //     var isMoving = _agent.velocity.sqrMagnitude > 0.01f;
    //     _animator.SetBool("IsWalking", isMoving);
    // }

    private bool CanSeePlayer()
    {
        return IsFacingPlayer() && HasClearPathToPlayer();
    }

    private bool IsFacingPlayer()
    {
        var dirToPlayer = (player.position - transform.position).normalized;
        var angle = Vector3.Angle(transform.forward, dirToPlayer);
        return angle <= viewAngle / 2f;
    }

    private bool HasClearPathToPlayer()
    {
        var dirToPlayer = player.position - transform.position;
        if(Physics.Raycast(transform.position, dirToPlayer.normalized, out RaycastHit hit, dirToPlayer.magnitude))
        {
            return hit.transform == player;
        }
        return true;
    }
}
