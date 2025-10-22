using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Drone : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private float range;
    [SerializeField] private float colliderDistance;

    private float cooldownTimer = Mathf.Infinity;
    [SerializeField] private int Damage = 10;

    [SerializeField] private BoxCollider2D boxcollider;
    [SerializeField] private LayerMask defaultlayer;

    private PlayerStats healthstats;
    public EnemyPatrolDrone enemyPatrolDrone;


    //Health
    public float maxHealth = 20f;
    private float currentHealth;
    private EnemyHPUI enemyHP;

    private void Awake()
   {
        enemyPatrolDrone = GetComponentInParent<EnemyPatrolDrone>();
    }


    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"[Start] {name} currentHealth: {currentHealth}, maxHealth: {maxHealth}");

        enemyHP = GetComponentInChildren<EnemyHPUI>();
        if (enemyHP != null)
        {
            enemyHP.SetHealth(currentHealth, maxHealth);
        }

        //Spawn the enemy at the designated spawn point.
        if (enemyPatrolDrone != null)
        {
            enemyPatrolDrone.SpawnAtPointDrone();
        }
    }


    // Update is called once per frame
    void Update()
    {
        cooldownTimer += Time.deltaTime;

        // Check if player is in sight and cache the healthstats
        bool playerDetected = playerinSight();

        if (playerDetected && cooldownTimer >= attackCooldown)
        {
            DamagePlayer(); // uses cached healthstats
            cooldownTimer = 0;
        }

        if (enemyPatrolDrone != null)
        {
            enemyPatrolDrone.enabled = !playerDetected;
        }
    }

    private bool playerinSight()
    {
        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        RaycastHit2D hit = Physics2D.BoxCast(
            boxcollider.bounds.center,
            new Vector2(boxcollider.bounds.size.x * range, boxcollider.bounds.size.y),
            0f,
            direction,
            range * colliderDistance,
            defaultlayer
        );
        if (hit.collider != null)
        {
            healthstats = hit.transform.GetComponentInParent<PlayerStats>();
            return healthstats != null;
        }

        healthstats = null;
        return false;
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
        if (healthstats != null)
        {
            healthstats.TakeDamage(Damage);
            Debug.Log($"Drone dealt {Damage} damage to {healthstats.name} at time {Time.time}");
        }
    }

    //This is to ignroe projectile if the projectile is collectible.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Projectile proj = collision.gameObject.GetComponentInParent<Projectile>();
        if (proj != null && proj.canBeCollected)
        {
            // Ignore physics interactions with this projectile
            Collider2D enemyCollider = GetComponent<Collider2D>();
            Collider2D[] projectileColliders = collision.gameObject.GetComponents<Collider2D>();

            foreach (Collider2D col in projectileColliders)
            {
                Physics2D.IgnoreCollision(col, enemyCollider, true);
            }

            return; // Stop further interaction
        }


    }

    //Health Settings
    public void TakeDamage(float damage)
    {
        Debug.Log($"[DEBUG] {name} currentHealth BEFORE damage: {currentHealth}");
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log($"[DEBUG] {name} took {damage} damage. Remaining health: {currentHealth}");

        if (enemyHP != null)
            enemyHP.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (enemyHP != null)
            Destroy(enemyHP.gameObject);
        Destroy(gameObject);
    }
}
