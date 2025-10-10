using System.Runtime.CompilerServices;
using UnityEngine;

public class ChargePatrol : MonoBehaviour
{
    //Patrol Points
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [SerializeField] private Transform enemy;
    [SerializeField] private float speedBurstInterval = 0.3f; 

    [SerializeField] private float speed;
    [SerializeField] private float idleDuration;
    private float idleTimer;
    private bool movingLeft;
    private float currentSpeed;
    private float speedTimer = 0f;

    //Chase
    [SerializeField] private Transform Player;
    [SerializeField] private float chaseRange = 3f;
    public Vector3 extravelocity = Vector3.zero;
    [SerializeField] private float maxchaseRange = 4f;
    private Vector3 spawnPos;


    private Vector3 initialScale;

    public Vector3 chargeVelocity = Vector3.zero;
    public bool isCharging = false;
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
        UpdateSpeedBurst();
        if (isCharging)
        {
            Debug.Log($"Chasing player! Enemy X: {enemy.position.x}, Player X: {Player.position.x}");
            enemy.position += chargeVelocity * Time.deltaTime;
        }
        else if (PlayerInSight())
        {
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

    private void UpdateSpeedBurst()
    {
        speedTimer += Time.deltaTime;
        if (speedTimer >= speedBurstInterval)
        {
            currentSpeed = speed * Random.Range(0.5f, 2f);
            speedTimer = 0f;
        }
    }
    private void chasePlayer()
    {
        int direction = (Player.position.x > enemy.position.x) ? 1 : -1;
        Debug.Log($"[Chase] Current Speed: {currentSpeed:F2}");

        enemy.position = new Vector3(
        enemy.position.x + direction * currentSpeed * Time.deltaTime + extravelocity.x * Time.deltaTime,
        enemy.position.y + extravelocity.y * Time.deltaTime,
        enemy.position.z + extravelocity.z * Time.deltaTime);

        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
    }

    public bool PlayerInSight()
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
    private void MoveDirection(int direction)
    {
        idleTimer = 0;
        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
        Debug.Log($"[Patrol] Current Speed: {currentSpeed:F2}");

        enemy.position = new Vector3(
      enemy.position.x + Time.deltaTime * direction * currentSpeed + extravelocity.x * Time.deltaTime,
      enemy.position.y + extravelocity.y * Time.deltaTime,
      enemy.position.z + extravelocity.z * Time.deltaTime);
    }
}
