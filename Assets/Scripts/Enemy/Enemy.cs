using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private enum State { Idle, Chase, Attack, Pain }

    public static event Action<Vector3> PlayerShot;
    public static void NotifyPlayerShot(Vector3 pos) => PlayerShot?.Invoke(pos);

    [SerializeField] private float health = 100f;
    [SerializeField, Range(0f, 1f)] private float painChance = 0.5f;
    [SerializeField] private float painDuration = 0.4f;

    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float hearingRange = 25f;

    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackWindup = 0.3f;
    [SerializeField] private float attackCooldown = 0.8f;

    [SerializeField] private Transform target;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Room room;

    [SerializeField] private float flashDuration = 0.2f;

    private State state;
    private bool isDead;
    private float stateTimer;
    private float repathTimer;
    private bool attackDone;
    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        agent.speed = chaseSpeed;
        agent.updateRotation = false;

        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        if (playerHealth == null && target != null)
            playerHealth = target.GetComponent<PlayerHealth>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void OnEnable() => PlayerShot += OnPlayerShot;
    private void OnDisable() => PlayerShot -= OnPlayerShot;

    private void Start() => ChangeState(State.Idle);

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
        PlayDamageFlash();

        if (health <= 0f) { Death(); return; }

        if (UnityEngine.Random.value < painChance) ChangeState(State.Pain);
        else if (state == State.Idle) ChangeState(State.Chase);
    }

    public void SetRoom(Room room) => this.room = room;

    private void UpdateIdle()
    {
        if (TargetInRange(detectionRange)) ChangeState(State.Chase);
    }

    private void UpdateChase()
    {
        if (target == null) { ChangeState(State.Idle); return; }
        if (TargetInRange(attackRange)) { ChangeState(State.Attack); return; }
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
        if (target == null) { ChangeState(State.Idle); return; }

        FaceDirection(DirectionToTarget());

        if (!TargetInRange(attackRange)) { ChangeState(State.Chase); return; }

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
        if (stateTimer <= 0f) ChangeState(State.Chase);
    }

    private void ChangeState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.Idle: StopAgent(); break;
            case State.Chase: ResumeAgent(); repathTimer = 0f; break;
            case State.Attack: StopAgent(); stateTimer = attackWindup; attackDone = false; break;
            case State.Pain: StopAgent(); stateTimer = painDuration; break;
        }
    }

    private void PerformAttack()
    {
        if (playerHealth == null || playerHealth.IsDead) return;
        if (!TargetInRange(attackRange)) return;
        playerHealth.TakeDamage(attackDamage);
    }

    private void OnPlayerShot(Vector3 shotPosition)
    {
        if (isDead || state != State.Idle) return;
        if (Vector3.Distance(transform.position, shotPosition) <= hearingRange)
            ChangeState(State.Chase);
    }

    private void Death()
    {
        isDead = true;
        StopAgent();
        room?.EnemyDied();
        Destroy(gameObject);
    }

    private void StopAgent()
    {
        if (!agent.isOnNavMesh) return;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private void ResumeAgent()
    {
        if (!agent.isOnNavMesh) return;
        agent.isStopped = false;
    }

    private bool TargetInRange(float range) =>
        target != null && Vector3.Distance(transform.position, target.position) <= range;

    private Vector3 DirectionToTarget()
    {
        if (target == null) return Vector3.zero;
        var d = target.position - transform.position;
        d.y = 0f;
        return d.sqrMagnitude < 0.001f ? Vector3.zero : d.normalized;
    }

    private void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;
        var look = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
    }

    private IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private void PlayDamageFlash()
    {
        if (spriteRenderer == null) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(DamageFlash());
    }
}