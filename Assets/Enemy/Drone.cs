using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Drone : MonoBehaviour
{
    [Header("Drone Settings")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float range;
    [SerializeField] private float colliderDistance;

    private float cooldownTimer = Mathf.Infinity;
    [SerializeField] private int Damage = 10;
    [SerializeField] private float pushForce = 10f;

    [Header("Visualization Settings")]
    [SerializeField] private BoxCollider2D boxcollider;
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private Animator animator;

    [SerializeField] private LayerMask defaultlayer;

    [Header("Player Settings")]
    private PlayerStats healthstats;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;
    public EnemyPatrolDrone enemyPatrolDrone;


    [Header("Drone Health Settings")]
    public float maxHealth = 20f;
    private float currentHealth;
    private EnemyHPUI enemyHP;

    //Spawn Settings (Not In Use).
    private Vector3 startPos;
    private Quaternion startRot;
    private bool isDead = false;

    
    private void Awake()
   {
        enemyPatrolDrone = GetComponentInParent<EnemyPatrolDrone>();
    }

   //Initialization.
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


    void Update()
    {
        cooldownTimer += Time.deltaTime; //Increase attack cd timer.

        bool playerDetected = playerinSight(); //Check if the player is in sight.

        if (playerDetected)
        {
            ApplyContinuousPush(); //Push players continously.

            if (cooldownTimer >= attackCooldown)
            {
                DamagePlayer(); // periodic damage only (To prevent stacked damage).
                cooldownTimer = 0;
            }
        }
        if (enemyPatrolDrone != null) //Disable patrol state if player found.
        {
            enemyPatrolDrone.enabled = !playerDetected;
        }
    }

    //When player is in sight.
    private bool playerinSight()
    {
        //Get the direction they are facing.
        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);
        /*Perfrom a box cast taht the width is multiplied by the range and cast it in facing direction. Length is dertermined by colliderditance and only detect it in default layer.*/
        RaycastHit2D hit = Physics2D.BoxCast(
            boxcollider.bounds.center,
            new Vector2(boxcollider.bounds.size.x * range, boxcollider.bounds.size.y),
            0f,
            direction,
            range * colliderDistance,
            defaultlayer
        );

        if (hit.collider != null) //If the raycast hit something within the designated bounds, check if it’s the player by finding PlayerStats.
        {
            healthstats = hit.transform.GetComponentInParent<PlayerStats>();
            return healthstats != null;
        }

        //if nothing is detected, return.
        healthstats = null;
        return false;
    }


    ////For debug purposes.
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(
    //        boxcollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
    //        new Vector3(boxcollider.bounds.size.x * range, boxcollider.bounds.size.y, boxcollider.bounds.size.z)
    //    );
    //}

    //To daamage player.
    private void DamagePlayer()
    {
        if (healthstats != null)
        {
            healthstats.TakeDamage(Damage); //Call take damage logic form player stats.
            Debug.Log($"Drone dealt {Damage} damage to {healthstats.name} at time {Time.time}");
        }

    }
    
    //For continous push.
    private void ApplyContinuousPush()
    {

        Rigidbody2D[] allBodies = healthstats.GetComponentsInChildren<Rigidbody2D>(); //Get all the rigidbody in the players because we have multiple childs.
        foreach (Rigidbody2D body in allBodies)
        {
            if (body == null) continue;
            Vector2 pushDirection = ((Vector2)body.position - (Vector2)transform.position).normalized; //Push direction is from drone to the player.
            float continousforce = pushForce * 0.5f;
            body.AddForce(pushDirection * continousforce * Time.deltaTime * 50f, ForceMode2D.Force); //Apply smooth push force for every frame.
        }

    }

    //This is to ignroe projectile if the projectile is collectible.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Projectile proj = collision.gameObject.GetComponentInParent<Projectile>(); //Check if the colided object has projectile component
        if (proj != null && proj.canBeCollected) //If the object is projectile and is marked as collectible.
        {
            // Ignore physics interactions with this projectile.
            Collider2D enemyCollider = GetComponent<Collider2D>();
            Collider2D[] projectileColliders = collision.gameObject.GetComponents<Collider2D>(); //Get all the projectile collider because it has the same body qualities as the player.

            foreach (Collider2D col in projectileColliders)
            {
                Physics2D.IgnoreCollision(col, enemyCollider, true);
            }

            return; // Stop further interaction.
        }


    }

    //Take damage from player or any possible interactions.
    public void TakeDamage(float damage)
    {
        Debug.Log($"[DEBUG] {name} currentHealth BEFORE damage: {currentHealth}");
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); //Clamp health so it never drop below 0.
        Debug.Log($"[DEBUG] {name} took {damage} damage. Remaining health: {currentHealth}");

        if (enemyHP != null)
            enemyHP.SetHealth(currentHealth, maxHealth); //Update the health bar.

        //VFX animation.
        playdamagevfx();

        //Start coroutine to handle the death.
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    //VFX Logic when taking damage. 
    private void playdamagevfx()
    {
        if (damageVFX != null)
        {
            GameObject vfx = Instantiate(damageVFX, transform.position, Quaternion.identity); //Spawn the VFX at the drone position.
            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Play();
            
            Destroy(vfx, ps != null ? ps.main.duration + 0.1f : 1f); //If the particile system exists, destroy the particle duration. If not destroy after 1 sec.


        }
    }
   private IEnumerator Die()
    {
        if (isDead) yield return null; //Prevent duplicate death

        isDead = true;
        animator.SetBool("isAlive", false);
        yield return new WaitForSeconds(0.7f); //Wait for 0.7 sec for the death animation to finish.
        // Spawn VFX prefab at drone's position
        if (deathVFX != null)
        {
            GameObject vfx = Instantiate(deathVFX, transform.position, Quaternion.identity); //Spawn VFX atthe drone location.
            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(vfx, ps.main.duration + 0.5f); //Destroy after the animation finishes.
        }

        //Disbale patrol.
        if (enemyPatrolDrone != null)
            enemyPatrolDrone.enabled = false;

        //Disable collider.
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        //Disable anyform of renderer.
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        //Dsiable its movemenets.
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        //Hide HP
        if (enemyHP != null)
            enemyHP.gameObject.SetActive(false);

        //Deactivaete the drone.
        gameObject.SetActive(false);
    }

    
}
