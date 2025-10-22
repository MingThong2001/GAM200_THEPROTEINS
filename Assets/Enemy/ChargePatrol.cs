using System.Collections;
using UnityEngine;

public class ChargePatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform enemy;
    [SerializeField] private Transform enemySpawnPos;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolSpeedUpdateInterval = 0.5f;
    [SerializeField] private float idleDuration = 1f;
    [SerializeField] private float maxYjump = 4f;


    [Header("Charge Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float chaseRange = 3f;
    [SerializeField] private float maxChaseRange = 4f;
    [SerializeField] private float chargeSpeed = 5f;
    [SerializeField] private float chargeSpeedUpdateInterval = 0.3f;

    [Header("Obstacle Detection")]
    [SerializeField] private string obstacleTag = "Object";
    [SerializeField] private float returnSpeed = 3f;
    [SerializeField] private float detectionOffset = 0.6f;
    [SerializeField] private Vector2 detectionSize = new Vector2(0.6f, 0.8f);
    [SerializeField] private LayerMask detectionMask = ~0;

    private Vector3 initialScale;
    private enum State { Patrol, Returning, Chasing, Idle }
    private State state = State.Patrol;
    private bool isIdling = false;

    private bool movingLeft = true;
    private float patrolTimer = 0f;
    private float currentPatrolSpeed;

    private float chargeTimer = 0f;
    public Vector3 chargeVelocity = Vector3.zero;
    private bool isCharging = false;

    private void Awake()
    {
        if (enemy == null) enemy = transform;
        initialScale = enemy.localScale;
        currentPatrolSpeed = patrolSpeed;
    }

    private void Update()
    {
        if (enemy == null) return;

        switch (state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Chasing: ChaseUpdate(); break;
        }
    }

    private void PatrolUpdate()
    {
        // --- Check if player is at the patrol point first ---
        CheckPlayerAtPatrolPoint();

        // Skip patrol movement if just started chasing
        if (state == State.Chasing) return;

        int dir = movingLeft ? -1 : 1;

        if (isIdling) return;

        // Obstacle detection
        Vector2 boxCenter = (Vector2)enemy.position + new Vector2(dir * detectionOffset, 0f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, detectionSize, 0f, detectionMask);

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            if (hit.gameObject == enemy.gameObject) continue;
            if (hit.transform.IsChildOf(enemy)) continue;
            if (!hit.CompareTag(obstacleTag)) continue;
            if (!hit.name.Contains("Grab Box")) continue;

            Debug.Log($"[Obstacle DETECTED] {hit.name} — switching to RETURN");
            Vector3 target = movingLeft ? pointB.position : pointA.position;
            state = State.Returning;
            StartCoroutine(ReturnToPointRoutine(target));
            return;
        }

        // Patrol speed erratic
        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolSpeedUpdateInterval)
        {
            currentPatrolSpeed = patrolSpeed * Random.Range(0.5f, 1.5f);
            patrolTimer = 0f;
        }

        // Move
        float randomY = Random.Range(-maxYjump, maxYjump) * Time.deltaTime;
        enemy.position += new Vector3(dir * currentPatrolSpeed * Time.deltaTime, randomY, 0f);
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * dir, initialScale.y, initialScale.z);

        // Patrol endpoints
        if (movingLeft && enemy.position.x <= pointA.position.x + 0.01f)
            StartCoroutine(IdleAndFlip());
        else if (!movingLeft && enemy.position.x >= pointB.position.x - 0.01f)
            StartCoroutine(IdleAndFlip());
    }

    private void CheckPlayerAtPatrolPoint()
    {
        if (player == null) return;

        float checkRadius = 0.3f; // Adjust as needed
        Vector2 patrolPoint = movingLeft ? pointA.position : pointB.position;

        Collider2D hit = Physics2D.OverlapCircle(patrolPoint, checkRadius, LayerMask.GetMask("Player"));
        if (hit != null)
        {
            int dir = (player.position.x > enemy.position.x) ? 1 : -1;
            StartCharge(dir);
            isIdling = false;
            state = State.Chasing;
        }
    }

    private IEnumerator IdleAndFlip()
    {
        if (state == State.Chasing || isIdling)
            yield break;

        isIdling = true;
        state = State.Idle;

        yield return new WaitForSeconds(idleDuration);

        movingLeft = !movingLeft;
        isIdling = false;
        state = State.Patrol;
    }

    private IEnumerator ReturnToPointRoutine(Vector3 target)
    {
        state = State.Returning;

        float targetX = target.x;
        float y = enemy.position.y;

        while (Mathf.Abs(enemy.position.x - targetX) > 0.01f)
        {
            float step = returnSpeed * Time.deltaTime;
            enemy.position = new Vector3(Mathf.MoveTowards(enemy.position.x, targetX, step), y, enemy.position.z);
            yield return null;
        }

        enemy.position = new Vector3(targetX, y, enemy.position.z);

        if (Mathf.Abs(targetX - pointA.position.x) < 0.1f)
        {
            movingLeft = false;
            enemy.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        }
        else
        {
            movingLeft = true;
            enemy.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        }

        enemy.position += new Vector3((movingLeft ? -0.01f : 0.01f), 0f, 0f);
        state = State.Patrol;
    }

    private void StartCharge(int dir)
    {
        if (dir == 0) dir = 1;

        isCharging = true;
        chargeVelocity = new Vector3(dir * chargeSpeed, 0f, 0f);
        chargeTimer = 0f;
        state = State.Chasing;
        Debug.Log("[CHARGE] Enemy started charging!");
    }

    private void ChaseUpdate()
    {
        chargeTimer += Time.deltaTime;
        if (chargeTimer >= chargeSpeedUpdateInterval)
        {
            float dir = chargeVelocity.x > 0 ? 1f : -1f;
            chargeVelocity = new Vector3(dir * chargeSpeed * Random.Range(0.7f, 1.3f), 0f, 0f);
            chargeTimer = 0f;
        }

        enemy.position += chargeVelocity * Time.deltaTime;
        int moveDir = chargeVelocity.x > 0 ? 1 : -1;
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * moveDir, initialScale.y, initialScale.z);

        if (Vector2.Distance(enemy.position, player.position) > maxChaseRange)
        {
            isCharging = false;
            state = State.Patrol;
        }
    }

    public bool PlayerInSight()
    {
        if (!enemy) return false;

        if (player == null) return false;

        float distanceX = Mathf.Abs(player.position.x - enemy.position.x);
        float distanceY = Mathf.Abs(player.position.y - enemy.position.y);

        return distanceX <= chaseRange && distanceY <= 1f;
    }

    public void SpawnAtPointFailedSubject()
    {
        if (enemySpawnPos == null || enemy == null) return;

        enemy.position = enemySpawnPos.position;
        enemy.gameObject.SetActive(true);

        state = State.Patrol;
        isIdling = false;
        movingLeft = true;
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (enemy == null) enemy = transform;
        Gizmos.color = Color.yellow;
        int dir = movingLeft ? -1 : 1;
        Vector2 boxCenter = (Vector2)enemy.position + new Vector2(dir * detectionOffset, 0f);
        Gizmos.DrawWireCube(boxCenter, detectionSize);
    }
}