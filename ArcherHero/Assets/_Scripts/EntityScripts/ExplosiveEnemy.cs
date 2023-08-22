using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class ExplosiveEnemy : Enemy
{   
    [Header("Movement Settings")]
    [SerializeField] private float _timeToChangeDirection = 5f;
    [SerializeField] private float _movementBoundsX = 10f;
    [SerializeField] private float _movementBoundsZ = 5f;
    [SerializeField] private float _attackTriggerDistance = 2f;
    [SerializeField] private float _boostedSpeed = 50f;

    [SerializeField] private float _attackDuration = 5f;
    [SerializeField] private float _explosionRadius = 3f;
    
    private const string _animationMove = "Move";
    private const string _animationAttack = "Attack";
    private const string _animationDead = "Dead";

    private Animator _animator;
    private bool isAttack = false;
    private float _distanceToTarget;
    private CancellationTokenSource _cancellationTokenAttack;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        StartRandomMovement().Forget();
    }
    
    private async UniTaskVoid StartRandomMovement()
    {   
        _cancellationToken = new CancellationTokenSource();
        while (!_cancellationToken.IsCancellationRequested)
        {
            _targetMovePosition = new Vector3(Random.Range(-_movementBoundsX, _movementBoundsX), 0f, Random.Range(-_movementBoundsZ, _movementBoundsZ));
            
            _agent.SetDestination(_targetMovePosition);
            
            await UniTask.Delay(TimeSpan.FromSeconds(_timeToChangeDirection), cancellationToken: _cancellationToken.Token). SuppressCancellationThrow();
        }
    }

    private void OnDisable()
    {
        _cancellationToken?.Cancel();
        _cancellationTokenAttack?.Cancel();
    }

    private async UniTaskVoid AttackPlayer()
    {   
        _cancellationTokenAttack = new CancellationTokenSource();
        while (!_cancellationTokenAttack.IsCancellationRequested)
        {   
            Debug.Log("preparation for attack");
            Vector3 directionToPlayer = _targetAttack.position - transform.position;
            Quaternion rotationToPlayer = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            transform.rotation = rotationToPlayer;
            
            await UniTask.Delay(TimeSpan.FromSeconds(_attackDuration));
        
            _agent.speed = _boostedSpeed;
            _agent.SetDestination(_targetAttack.position);
            
            await UniTask.WaitUntil(() => _distanceToTarget  <= 1f);
            
            ExplosiveAttack();
        }
    }

    private void ExplosiveAttack()
    {
        if (_distanceToTarget <= _explosionRadius)
        {   
            Debug.Log("Boom");
            IDamageable damageable = _targetAttack.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(typeDamage, damage);
            }
        }
        Die();
    }
    
    private void Update()
    {
        _distanceToTarget = Vector3.Distance(transform.position , _targetAttack.position);
        
        if (!isAttack)
        {
            if (_distanceToTarget <= _attackTriggerDistance)
            {
                isAttack = true;
                _cancellationToken.Cancel();
                _agent.ResetPath();
                AttackPlayer().Forget();
            }
        }
    }
    
    
    protected override void Die()
    {
        _cancellationTokenAttack.Cancel();
        base.Die();
        Destroy(gameObject);
    }
}