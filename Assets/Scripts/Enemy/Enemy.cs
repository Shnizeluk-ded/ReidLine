using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private enum State { Idle, Chase, Attack, Pain }

    private static event Action<Vector3> PlayerShot;
    public static void NotifyPlayerShot(Vector3 shotPosition) => PlayerShot?.Invoke(shotPosition);

    [Header("Health")]
    [SerializeField] private float health = 100f;
    [SerializeField, Range(0f, 1f)] private float painChance = 0.5f;
    [SerializeField] private float painDuration = 0.4f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float repathInterval = 0.2f;

    [Header("Detection")]
    [SerializeField] private Transform target;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float hearingRange = 25f;


    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackWindup = 0.3f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private PlayerHealth playerHealth;


    private NavMeshAgent agent;
    private State state;
    private bool isDead;
    private float stateTimer;
    private float repathTimer;
    private bool attackDone;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.updateRotation = false;  

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void OnEnable() => PlayerShot += OnPlayerShot;
    private void OnDisable() => PlayerShot -= OnPlayerShot;

    private void Start()
    {
        ChangeState(State.Idle);
    }

    private void Update()
    {
        if (isDead) return;

        switch (state)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
            case State.Pain: UpdatePain(); break;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        if (health <= 0f)
        {
            Death();
            return;
        }

        if (UnityEngine.Random.value < painChance)
            ChangeState(State.Pain);        
        else if (state == State.Idle)
            ChangeState(State.Chase);        
    }

    private void Death()
    {
        isDead = true;
        StopAgent();
        Destroy(gameObject);
    }

    // ---------- State machine ----------

    private void ChangeState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.Idle:
                StopAgent();
                break;

            case State.Chase:
                if (agent.isOnNavMesh) agent.isStopped = false;
                repathTimer = 0f;            
                break;

            case State.Attack:
                StopAgent();
                stateTimer = attackWindup;
                attackDone = false;
                break;

            case State.Pain:
                StopAgent();
                stateTimer = painDuration;
                break;
        }
    }

    private void UpdateIdle()
    {
        if (TargetInRange(detectionRange))
            ChangeState(State.Chase);
    }

    private void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.Idle);
            return;
        }

        if (TargetInRange(attackRange))
        {
            ChangeState(State.Attack);
            return;
        }

        if (!agent.isOnNavMesh) return;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            agent.SetDestination(target.position);
            repathTimer = repathInterval;
        }

        FaceDirection(agent.desiredVelocity);
    }


    private void UpdateAttack()
    {
        if (target == null)
        {
            ChangeState(State.Idle);
            return;
        }

        FaceDirection(DirectionToTarget());

        // Если игрок вышел из радиуса атаки
        if (!TargetInRange(attackRange))
        {
            ChangeState(State.Chase);
            return;
        }

        stateTimer -= Time.deltaTime;

        if (!attackDone)
        {
            if (stateTimer <= 0f)
            {
                PerformAttack();
                attackDone = true;
                stateTimer = attackCooldown;
            }
        }
        else if (stateTimer <= 0f)
        {
            ChangeState(State.Chase);
        }
    }


    private void UpdatePain()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
            ChangeState(State.Chase);   
    }


    private void PerformAttack()
    {
        if (playerHealth == null || playerHealth.IsDead)
            return;

        // Проверяем, не убежал ли игрок
        if (!TargetInRange(attackRange))
            return;

        playerHealth.TakeDamage(attackDamage);

        Debug.Log("Enemy attacked player!");
    }


    private void OnPlayerShot(Vector3 shotPosition)
    {
        if (isDead || state != State.Idle) return;

        if (Vector3.Distance(transform.position, shotPosition) <= hearingRange)
            ChangeState(State.Chase);
    }

    private void StopAgent()
    {
        if (!agent.isOnNavMesh) return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private bool TargetInRange(float range)
    {
        return target != null &&
               Vector3.Distance(transform.position, target.position) <= range;
    }

    private Vector3 DirectionToTarget()
    {
        if (target == null) return Vector3.zero;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        return direction.normalized;
    }

    private void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
