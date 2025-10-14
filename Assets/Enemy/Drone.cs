using UnityEngine;

public class Drone : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private float range;
    [SerializeField] private float colliderDistance;

    private float cooldownTimer = Mathf.Infinity;
    [SerializeField] private int Damage;

    [SerializeField] private BoxCollider2D boxcollider;
    [SerializeField] private LayerMask defaultlayer;

    private PlayerStats healthstats;
    public EnemyPatrol enemypatrol;

    private void Awake()
   {
        enemypatrol = GetComponentInParent<EnemyPatrol>();
    }


    private void Start()
    {
        //Spawn the enemy at the designated spawn point.
        if (enemypatrol != null)
        {
            enemypatrol.SpawnAtPointDrone();
        }
    }


    // Update is called once per frame
    void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (playerinSight())
        {
            if (cooldownTimer >= attackCooldown)
            {
                DamagePlayer();
                cooldownTimer = 0;
            }
        }

        if (enemypatrol != null)
        {
            enemypatrol.enabled = !playerinSight();
        }
    }

    private bool playerinSight()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
       boxcollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
       new Vector2(boxcollider.bounds.size.x * range, boxcollider.bounds.size.y),
       0f,
       Vector2.left,
       0f,
       defaultlayer
       );
        if (hit.collider != null)
        {
             healthstats = hit.transform.GetComponentInParent<PlayerStats>();

        }
        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            boxcollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxcollider.bounds.size.x * range, boxcollider.bounds.size.y, boxcollider.bounds.size.z)
        );
    }

    private void DamagePlayer()
    {
        if (playerinSight())
        {
            healthstats.TakeDamage(Damage);
            Debug.Log($"Drone dealt {Damage} damage to {healthstats.name} at time {Time.time}");

        }
    }
}
