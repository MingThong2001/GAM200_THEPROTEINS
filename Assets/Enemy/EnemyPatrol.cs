using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    //Patrol Points
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [SerializeField] private Transform enemy;

    [SerializeField] private float speed;
    [SerializeField] private float idleDuration;
    private float idleTimer;
    private bool movingLeft;


    //Chase
    [SerializeField] private Transform Player;
    [SerializeField] private float chaseRange = 3f;

    [SerializeField] private float maxchaseRange = 4f;
    private Vector3 spawnPos;


    private Vector3 initialScale;
    private void Awake()
    {
        initialScale = enemy.localScale;
        if (enemy != null)
        { 
            spawnPos = enemy.position; 
        }
    }
    private void Update()
    {
        if (Player != null && PlayerInSight())
        {
            Debug.Log($"Chasing player! Enemy X: {enemy.position.x}, Player X: {Player.position.x}");

            chasePlayer();
        }
        else
        {
            Debug.Log($"Patrolling. Enemy X: {enemy.position.x}");

            if (movingLeft)
            {
                if (enemy.position.x >= pointA.position.x)
                {
                    MoveDirection(-1);

                }
                else
                {
                    DirectionChange();

                }

            }
            else
            {
                if (enemy.position.x <= pointB.position.x)
                {
                    MoveDirection(1);

                }
                else
                {
                    DirectionChange();
                }
            }
        }
    }
    private void chasePlayer()
    {
        int direction = (Player.position.x > enemy.position.x) ? 1 : -1;

        enemy.position = new Vector3(enemy.position.x + direction * speed * Time.deltaTime, enemy.position.y, enemy.position.z);
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
    }

    private bool PlayerInSight()
    {
        float distanceToPlayer = Vector2.Distance(enemy.position, Player.position);

        // Only consider x-axis distance for chasing
        bool inRange = Mathf.Abs(Player.position.x - enemy.position.x) <= chaseRange;

        return inRange && distanceToPlayer <= maxchaseRange;
    }
    private void DirectionChange()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer > idleDuration)
        movingLeft = !movingLeft;
    }
    private void MoveDirection (int direction)
    {
        idleTimer = 0;
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
        enemy.position = new Vector3 (enemy.position.x + Time.deltaTime * direction * speed, enemy.position.y, enemy.position.z);
    }
}
