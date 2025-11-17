using System.Collections;

using UnityEngine;
using static UnityEngine.UI.Image;

public class EnemyPatrolDrone : MonoBehaviour
{
    [Header("Patrol Points")]
    [SerializeField] public Transform pointA;
    [SerializeField] public Transform pointB;

    //Optional (Will default to the below variable if null).
    [SerializeField] private Transform enemy;
    [SerializeField] private Transform enemySpawnPos; 

    [SerializeField] private float speed = 2f;
    [SerializeField] private float idleDuration = 0.6f;
    private bool movingLeft = true;

    [Header("Chase")]
    [SerializeField] private Transform Player;
    [SerializeField] private float chaseRange = 3f; //Drone start chasing the player within this horizontal distance.
    [SerializeField] private float maxchaseRange = 4f;

    [Header("Obstacle Detection")]
    [SerializeField] private string obstacleTag = "Object";
    [SerializeField] private float returnSpeed = 3f; //Speed when returning after hitting an object.
    [SerializeField] private float detectionOffset = 0.6f;
    [SerializeField] private Vector2 detectionSize = new Vector2(0.6f, 0.8f);
    [SerializeField] private LayerMask detectionMask = ~0; //Detection applies to all layers unless filtered.
    [SerializeField] private Animator animator;


    [Header("Ground Detection")]
    [SerializeField] private Vector2 groundcheckoffSet = new Vector2(1f, 0f); //Horizontal offset for where the ray is cast in front of the drone.
    [SerializeField] private float groundcheckDistance = 1f; //Length of the downward ray.
    [SerializeField] private LayerMask groundMask; //Ground layer which is jumpable.


    //States and flags.
    private Vector3 initialScale;
    private enum State { Patrol, Returning, Chasing, Idle }
    private State state = State.Patrol;
    private bool isIdling = false;

    private void Awake()
    {
        //Set enemy to itself if not assigned and saves the scale which is needed for flipping.
        if (enemy == null) enemy = transform;
        initialScale = enemy.localScale;

    }

    private void Update()
    {
        //Local references.
        if (Player == null && GameManager.PlayerTransform != null)
            Player = GameManager.PlayerTransform; 

        if (Player == null)
            return; //Return if no player exist.


        if (enemy == null) return; //Safety net for enemy.

        //Behaviour state.
        switch (state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Chasing: ChaseUpdate(); break;
            case State.Idle: PlayIdle(); break;
                
        }
    }

   
    private void PatrolUpdate()
    {
        //Switch to chase if player is close enough.
        if (Player != null && PlayerInSight())
        {
            state = State.Chasing;
            return;
        }
        //Set animation.
        animator.SetBool("isChasing", false);
        animator.SetBool("isPatrolling", true);

        //Determine movement directions.
        int dir = movingLeft ? -1 : 1;

        // Detection box in front (ignore self)
        Vector2 boxCenter = (Vector2)enemy.position + new Vector2(dir * detectionOffset, 0f); //The center of your box, put it infront of the drone based on the directions.
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, detectionSize, 0f, detectionMask); //The box will store all the hits, size and the layer that count as obstacles.

        foreach (Collider2D hit in hits) //Loop through all the detected objects.
        {
            if (hit == null) continue;
            // Ignore the enemy itself or its children
            if (hit.gameObject == enemy.gameObject) continue;
            if (hit.transform.IsChildOf(enemy)) continue;

            //Only objects with the correct tag which is tagged as obstacles.
            if (!hit.CompareTag(obstacleTag)) continue;

            //Only count obstacles whose name contains as Grab box.
            if (!hit.name.Contains("Grab Box")) continue;

            Debug.Log($"[Obstacle DETECTED] {hit.name} — switching to RETURN");
            // Start returning to the opposite patrol point
            Vector3 target = movingLeft ? pointB.position : pointA.position; //If moving left, return to point B vise-versa.

            //Switch the state to return.
            state = State.Returning;
            StartCoroutine(ReturnToPointRoutine(target));
            return;
        }

        //Move horitzontal each frame.
        enemy.position += new Vector3(dir * speed * Time.deltaTime, 0f, 0f);

        //Flip the enemy sprite when going left or right.
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * dir, initialScale.y, initialScale.z);

        //If drone reach a point, stop and flip. (Add 0.01f to avoid any floating point issues).
        if (movingLeft && enemy.position.x <= pointA.position.x + 0.01f)
            StartCoroutine(IdleAndFlip());
        else if (!movingLeft && enemy.position.x >= pointB.position.x - 0.01f)
            StartCoroutine(IdleAndFlip());
    }

    private IEnumerator IdleAndFlip()
    {
        if (isIdling) yield break; //To prevent duplicate coroutine.

        //Set the state to idle.
        isIdling = true;
        state = State.Idle;

        //Wait for the idle situation.
        yield return new WaitForSeconds(idleDuration);

        //FLip the patrol direction.
        movingLeft = !movingLeft;

        //Afterwards, resume patrol state.
        isIdling = false;
        state = State.Patrol;
    }

    private IEnumerator ReturnToPointRoutine(Vector3 target)
    {
        Debug.Log($"[Returning] to {(Vector2.Distance(target, pointA.position) < 0.1f ? "Point A" : "Point B")}");

        //Set the returning state.
        state = State.Returning;

        //Keep the drone to move only x-axis.
        float targetX = target.x;
        float y = enemy.position.y;

        //Continue looping until the drone’s X position is very close to the target X.
        while (Mathf.Abs(enemy.position.x - targetX) > 0.01f)
        {
            float step = returnSpeed * Time.deltaTime; //Distance to move this frame = return speed * deltaTime.

            //Move the drone horizontaly toward targetX, kepp Y axis constant with Z axis unchanged.
            enemy.position = new Vector3(
                Mathf.MoveTowards(enemy.position.x, targetX, step),
                y,
                enemy.position.z
            );
            yield return null; //Pause until the next frame, for smooth transistion.
        }

        //Force the drone exactly to the target X position.
        enemy.position = new Vector3(targetX, y, enemy.position.z);

        //Set patrol direction after return: if at A -> go right, if at B -> go left.
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

        //After reaching the points, the drone get nudged slightly so it wont retrigger edge detection.
        enemy.position += new Vector3((movingLeft ? -0.01f : 0.01f), 0f, 0f);

        state = State.Patrol;
    }

    private void ChaseUpdate()
    {
        if (!PlayerInSight()) //If the player is not detected, change to patrol state.
        {
            state = State.Patrol;
            return;
        }
        if (!groundahead()) //If the drone is about to fall off the gruond. Stop chasing.
        {
            state = State.Patrol;
            return;
        }

        //Animation state.
        animator.SetBool("isPatrolling", false);
        animator.SetBool("isChasing", true);

        //Flip the direction.
        int dir = (Player.position.x > enemy.position.x) ? 1 : -1;

        //Move horizontally toward the player.
        enemy.position += new Vector3(dir * speed * Time.deltaTime, 0f, 0f);

        //Flip sprite.
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * dir, initialScale.y, initialScale.z);
    }

    public bool PlayerInSight()
    {
        if (!enemy) return false; //If no enemy, return.

        if (Player == null) return false;

        //Measurement horizontal/vertical distance to the player.
        float distanceX = Mathf.Abs(Player.position.x - enemy.position.x);
        float distanceY = Mathf.Abs(Player.position.y - enemy.position.y);

        //Drone can only see player if its within the distance or same height.
        return distanceX <= chaseRange && distanceY <= 1f;
    }

    public void SpawnAtPointDrone()
    {
        if (enemySpawnPos == null || enemy == null) return;

        //Move the enmy to spawn position.
        enemy.position = enemySpawnPos.position;
        enemy.gameObject.SetActive(true);

        //Reset patrol state
        state = State.Patrol;
        isIdling = false;
        movingLeft = true;
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z); //Enemy sprite face right.
    }

    private bool groundahead()
    {
        int dir;

        //If chasing, raycast the direction depends on the player direction.
        if (state == State.Chasing)
            dir = (Player.position.x > enemy.position.x) ? 1 : -1;
        else
            dir = movingLeft ? -1 : 1; //Otherwise,direction depends on movement direction.

        Vector2 originPos = (Vector2)enemy.position + new Vector2(groundcheckoffSet.x * dir, groundcheckoffSet.y); //Ray will be infront of the drone horizontally with an offset vertically.

        RaycastHit2D hit = Physics2D.Raycast(originPos, Vector2.down, groundcheckDistance, groundMask); //Cast the ray downward and chcekc if the ground exists.

        //Debug.DrawLine(originPos, originPos + Vector2.down * groundcheckDistance, Color.red);

        return hit.collider != null; //If the ray hits something, ground exists.


    }
    //// Visual debug for detection box
    //private void OnDrawGizmosSelected()
    //{
    //    if (enemy == null) enemy = transform;
    //    Gizmos.color = Color.yellow;
    //    int dir = movingLeft ? -1 : 1;
    //    Vector2 boxCenter = (Vector2)enemy.position + new Vector2(dir * detectionOffset, 0f);
    //    Gizmos.DrawWireCube(boxCenter, detectionSize);
    //}

    //Idle animation helper method.
    public void PlayIdle()
    {
        animator.SetBool("isPatrolling", false);
    }
}
