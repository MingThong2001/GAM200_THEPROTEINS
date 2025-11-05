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
    [SerializeField] private float pushForce = 10f;

    [SerializeField] private BoxCollider2D boxcollider;
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private Animator animator;

    [SerializeField] private LayerMask defaultlayer;

    private PlayerStats healthstats;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;
    public EnemyPatrolDrone enemyPatrolDrone;


    //Health
    public float maxHealth = 20f;
    private float currentHealth;
    private EnemyHPUI enemyHP;

    //spawn
    private Vector3 startPos;
    private Quaternion startRot;
    private bool isDead = false;
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

        bool playerDetected = playerinSight();

        if (playerDetected)
        {
            ApplyContinuousPush(); //

            if (cooldownTimer >= attackCooldown)
            {
                DamagePlayer(); // periodic damage only
                cooldownTimer = 0;
            }
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

    private void ApplyContinuousPush()
    {

        Rigidbody2D[] allBodies = healthstats.GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D body in allBodies)
        {
            if (body == null) continue;
            Vector2 pushDirection = ((Vector2)body.position - (Vector2)transform.position).normalized;
            float continousforce = pushForce * 0.5f;
            body.AddForce(pushDirection * continousforce * Time.deltaTime * 50f, ForceMode2D.Force);
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
        playdamagevfx();
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    private void playdamagevfx()
    {
        if (damageVFX != null)
        {
            GameObject vfx = Instantiate(damageVFX, transform.position, Quaternion.identity);
            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Play();
            
            Destroy(vfx, ps != null ? ps.main.duration + 0.1f : 1f);


        }
    }
   private IEnumerator Die()
    {
        if (isDead) yield return null;
        isDead = true;
        animator.SetBool("isAlive", false);
        yield return new WaitForSeconds(0.7f);
        // Spawn VFX prefab at drone's position
        if (deathVFX != null)
        {
            GameObject vfx = Instantiate(deathVFX, transform.position, Quaternion.identity);
            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(vfx, ps.main.duration + 0.5f);
        }
        if (enemyPatrolDrone != null)
            enemyPatrolDrone.enabled = false;

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (enemyHP != null)
            enemyHP.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    //public void droneReset()
    //{
    //    isDead = false;

    //    // Reset health
    //    currentHealth = maxHealth;
    //    if (enemyHP != null)
    //    {
    //        enemyHP.gameObject.SetActive(true);
    //        enemyHP.SetHealth(currentHealth, maxHealth);
    //    }

    //    // Reset position and rotation
    //    transform.position = startPos;
    //    transform.rotation = startRot;

    //    // Re-enable patrol and collider
    //    if (enemyPatrolDrone != null)
    //        enemyPatrolDrone.enabled = true;

    //    if (enemyCollider != null)
    //        enemyCollider.enabled = true;

    //    // Re-enable renderers
    //    foreach (Renderer r in GetComponentsInChildren<Renderer>())
    //        r.enabled = true;

    //    // Remove Rigidbody constraints and zero velocity
    //    if (rb != null)
    //    {
    //        rb.constraints = RigidbodyConstraints2D.None;
    //        rb.linearVelocity = Vector2.zero;
    //        rb.angularVelocity = 0f;
    //        rb.simulated = true;
    //    }

    //    // Reactivate the whole GameObject
    //    gameObject.SetActive(true);
    //}
}
