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
        switch (state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Chasing: ChaseUpdate(); break;
        }
    }

    private void PatrolUpdate()
    {
        // Player detected? start charge
        if (PlayerInSight())
        {
            StartCharge();
            state = State.Chasing;
            return;
        }

        int dir = movingLeft ? -1 : 1;

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
            Debug.Log($"[PATROL] New speed: {currentPatrolSpeed:F2}");
        }

        // Move
        enemy.position += new Vector3(dir * currentPatrolSpeed * Time.deltaTime, 0f, 0f);
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * dir, initialScale.y, initialScale.z);
        Debug.Log($"[PATROL] Enemy X: {enemy.position.x:F2}, moving {(movingLeft ? "Left" : "Right")}");

        // Patrol endpoints
        if (movingLeft && enemy.position.x <= pointA.position.x + 0.01f)
            StartCoroutine(IdleAndFlip());
        else if (!movingLeft && enemy.position.x >= pointB.position.x - 0.01f)
            StartCoroutine(IdleAndFlip());
    }

    private IEnumerator IdleAndFlip()
    {
        if (isIdling) yield break;
        isIdling = true;
        state = State.Idle;
        yield return new WaitForSeconds(idleDuration);
        movingLeft = !movingLeft;
        isIdling = false;
        state = State.Patrol;
    }

    private IEnumerator ReturnToPointRoutine(Vector3 target)
    {
        Debug.Log($"[Returning] to {(Vector2.Distance(target, pointA.position) < 0.1f ? "Point A" : "Point B")}");
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
        Debug.Log($"[Return Complete] now movingLeft={movingLeft} -> back to PATROL");
        state = State.Patrol;
    }

    private void StartCharge()
    {
        isCharging = true;
        int dir = (player.position.x > enemy.position.x) ? 1 : -1;
        chargeVelocity = new Vector3(dir * chargeSpeed, 0f, 0f);
        chargeTimer = 0f;
        Debug.Log("[CHARGE] Enemy started charging!");
    }

    private void ChaseUpdate()
    {
        // Erratic charge
        chargeTimer += Time.deltaTime;
        if (chargeTimer >= chargeSpeedUpdateInterval)
        {
            float dir = chargeVelocity.x > 0 ? 1f : -1f;
            chargeVelocity = new Vector3(dir * chargeSpeed * Random.Range(0.7f, 1.3f), 0f, 0f);
            chargeTimer = 0f;
            Debug.Log($"[CHARGE] New erratic speed: {chargeVelocity.x:F2}");
        }

        enemy.position += chargeVelocity * Time.deltaTime;
        int moveDir = chargeVelocity.x > 0 ? 1 : -1;
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * moveDir, initialScale.y, initialScale.z);
        Debug.Log($"[CHARGE] Enemy X: {enemy.position.x:F2}, dir: {moveDir}, speed: {chargeVelocity.x:F2}");

        // Stop charge if player out of range
        if (Vector2.Distance(enemy.position, player.position) > maxChaseRange)
        {
            isCharging = false;
            state = State.Patrol;
            Debug.Log("[CHARGE] Enemy stopped charging, returning to patrol.");
        }
    }

    public bool PlayerInSight()
    {
        float distanceToPlayer = Vector2.Distance(enemy.position, player.position);
        bool inRange = Mathf.Abs(player.position.x - enemy.position.x) <= chaseRange;
        return inRange && distanceToPlayer <= maxChaseRange;
    }
    public void SpawnAtPointFailedSubject()
    {
        if (enemySpawnPos == null || enemy == null) return;

        //Move the enmy to spawn position.
        enemy.position = enemySpawnPos.position;
        enemy.gameObject.SetActive(true);

        // Reset patrol state
        state = State.Patrol;
        isIdling = false;
        movingLeft = true; // or false depending on your design
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