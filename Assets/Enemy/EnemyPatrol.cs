using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    [SerializeField] public Transform pointA;
    [SerializeField] public Transform pointB;
    [SerializeField] private Transform enemy; // optional, will default to this.transform if null
    [SerializeField] private Transform enemySpawnPos; // optional, will default to this.transform if null

    [SerializeField] private float speed = 2f;
    [SerializeField] private float idleDuration = 0.6f;
    private bool movingLeft = true;

    [Header("Chase")]
    [SerializeField] private Transform Player;
    [SerializeField] private float chaseRange = 3f;
    [SerializeField] private float maxchaseRange = 4f;

    [Header("Obstacle Detection")]
    [SerializeField] private string obstacleTag = "Object";
    [SerializeField] private float returnSpeed = 3f;
    [SerializeField] private float detectionOffset = 0.6f;
    [SerializeField] private Vector2 detectionSize = new Vector2(0.6f, 0.8f);
    [SerializeField] private LayerMask detectionMask = ~0; // set to specific layers if needed

    private Vector3 initialScale;
    private enum State { Patrol, Returning, Chasing, Idle }
    private State state = State.Patrol;
    private bool isIdling = false;

    private void Awake()
    {
        if (enemy == null) enemy = transform;
        initialScale = enemy.localScale;

    }

    private void Update()
    {
        switch (state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Chasing: ChaseUpdate(); break;
                // Returning handled by coroutine; Idle handled by coroutine
        }
    }

    private void PatrolUpdate()
    {
        if (Player != null && PlayerInSight())
        {
            state = State.Chasing;
            return;
        }

        int dir = movingLeft ? -1 : 1;

        // Detection box in front (ignore self)
        Vector2 boxCenter = (Vector2)enemy.position + new Vector2(dir * detectionOffset, 0f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, detectionSize, 0f, detectionMask);

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            // Ignore the enemy itself or its children
            if (hit.gameObject == enemy.gameObject) continue;
            if (hit.transform.IsChildOf(enemy)) continue;
            if (!hit.CompareTag(obstacleTag)) continue;
            if (!hit.name.Contains("Grab Box")) continue;

            Debug.Log($"[Obstacle DETECTED] {hit.name} — switching to RETURN");
            // Start returning to the opposite patrol point
            Vector3 target = movingLeft ? pointB.position : pointA.position;
            state = State.Returning;
            StartCoroutine(ReturnToPointRoutine(target));
            return;
        }

        // Move
        enemy.position += new Vector3(dir * speed * Time.deltaTime, 0f, 0f);
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * dir, initialScale.y, initialScale.z);

        // Reached endpoint? start idle/turn coroutine (use small epsilon)
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

        // Keep Y constant (patrol plane)
        float targetX = target.x;
        float y = enemy.position.y;

        while (Mathf.Abs(enemy.position.x - targetX) > 0.01f)
        {
            float step = returnSpeed * Time.deltaTime;
            enemy.position = new Vector3(
                Mathf.MoveTowards(enemy.position.x, targetX, step),
                y,
                enemy.position.z
            );
            yield return null;
        }

        // Snap
        enemy.position = new Vector3(targetX, y, enemy.position.z);

        // Set patrol direction after return: if at A -> go right, if at B -> go left
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

        // Tiny nudge to avoid boundary stall
        enemy.position += new Vector3((movingLeft ? -0.01f : 0.01f), 0f, 0f);

        Debug.Log($"[Return Complete] now movingLeft={movingLeft} -> back to PATROL");
        state = State.Patrol;
    }

    private void ChaseUpdate()
    {
        if (!PlayerInSight())
        {
            state = State.Patrol;
            return;
        }

        int dir = (Player.position.x > enemy.position.x) ? 1 : -1;
        enemy.position += new Vector3(dir * speed * Time.deltaTime, 0f, 0f);
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * dir, initialScale.y, initialScale.z);
    }

    private bool PlayerInSight()
    {
        if (Player == null) return false;
        float distanceToPlayer = Vector2.Distance(enemy.position, Player.position);
        bool inRange = Mathf.Abs(Player.position.x - enemy.position.x) <= chaseRange;
        return inRange && distanceToPlayer <= maxchaseRange;
    }

    public void SpawnAtPoint()
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


    // Visual debug for detection box
    private void OnDrawGizmosSelected()
    {
        if (enemy == null) enemy = transform;
        Gizmos.color = Color.yellow;
        int dir = movingLeft ? -1 : 1;
        Vector2 boxCenter = (Vector2)enemy.position + new Vector2(dir * detectionOffset, 0f);
        Gizmos.DrawWireCube(boxCenter, detectionSize);
    }
}