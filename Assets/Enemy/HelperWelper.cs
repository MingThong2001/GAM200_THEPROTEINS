using System.Collections;
using UnityEngine;

public class HelperWelper : MonoBehaviour
{
    //Grab Settings.
    public Transform grabPoint;
    public float grabRange = 2f;
    public float grabDuration = 2f;
    public float throwForce = 20f;
    public Transform playerRoot;
    private bool isHolding = false;
    public float grabDamage = 15f;
    
    //References
    private Rigidbody2D grabbedSegments;
    public EnemyPatrol  enemyPatrol;
    private Collider2D enemyCollider;
    private PlayerStats healthstats;
    private Rigidbody2D rb;

    //Health
    public float maxHealth = 20f;
    private float currentHealth;
    private EnemyHPUI enemyHP;

    //spawn
    private Vector3 startPos;
    private Quaternion startRot;
    private bool isDead = false;
    public void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"[Start] {name} currentHealth: {currentHealth}, maxHealth: {maxHealth}");
        rb = GetComponent<Rigidbody2D>();

        enemyHP = GetComponentInChildren<EnemyHPUI>();
        if (enemyHP != null)
        {
            enemyHP.SetHealth(currentHealth, maxHealth);
        }
    }
    private void Start()
    {
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
        enemyCollider = GetComponent<Collider2D>();
        // Spawn at designated point first
        if (enemyPatrol != null)
        {
            enemyPatrol.SpawnAtPointHelperWelper();
        }

        //Store Helper's start position.
        startPos = transform.position;
        startRot = transform.rotation;  

       
    }
 
    private void OnTriggerStay2D(Collider2D other)
    {
        if (isHolding) return;

        // Skip projectiles
        if (other.GetComponentInParent<Projectile>() != null) return;
        if (!other.transform.root.name.Contains("Player")) return;


        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return; 

        float distance = Vector2.Distance(transform.position, rb.position);
        if (distance <= grabRange)
        { 
            StartCoroutine(GrabHoldThrow(rb));
        }


    }

    private IEnumerator GrabHoldThrow(Rigidbody2D targetRb)
    {
        if (isHolding)
        {
           yield break; 
        }

        isHolding = true;
        grabbedSegments = targetRb;

        // Get player health
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.TakeDamage(grabDamage);
            Debug.Log($"HelperWelper dealt {grabDamage} damage to player at time {Time.time}");
        }
        //Stop enemy Patrol
        if (enemyPatrol != null) enemyPatrol.enabled = false;

        //Freeze enemy so it wont move during grab.
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        RigidbodyType2D originalbodyType = body.bodyType;
        bool wasSimulated = body.simulated;

        body.bodyType = RigidbodyType2D.Kinematic;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.simulated = false;

        //Hold Player as kinematic
        grabbedSegments.bodyType = RigidbodyType2D.Kinematic;

        //Keep player at holdpoint for holdduration.
        float timer = 0f;
        while (timer < grabDuration)
        {
            grabbedSegments.position = grabPoint.position;
            timer += Time.deltaTime;    
            yield return null;

        }
       //Release player.
       grabbedSegments.bodyType = RigidbodyType2D.Dynamic;

        //Apply throw face.
        float throwdirectionX = Mathf.Sign(transform.localScale.x) * 2f ;
        Vector2 throwDirection = new Vector2(throwdirectionX, 3f);
        grabbedSegments.AddForce(throwDirection * (throwForce + 10f), ForceMode2D.Impulse);

        // Debug log to show throw info
        Debug.Log($"[HelperWelper] Throwing player {grabbedSegments.name} from {grabPoint.position} " +
                  $"with direction {throwDirection.normalized}, magnitude {throwForce + 10f}, " +
                  $"final velocity will be approx: {(throwDirection.normalized * (throwForce + 10f))}");

        //Clear refernece to prevent double grab.
        grabbedSegments = null;
        healthstats = null; 

        //Wait for awhile before restoring enemy physics so player is fully thrown.
        yield return new WaitForSeconds(0.5f);

        //Restore enemy physics
        body.simulated = wasSimulated;
        body.bodyType = originalbodyType;

        //Resume patrol
        if (enemyPatrol != null)
        {
            enemyPatrol.enabled = true;
        }

        isHolding = false;  
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
        if (isDead) return;
        isDead = true;

        if (enemyPatrol != null)
            enemyPatrol.enabled = false;

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

    //public void HelperWelperReset()
    //{
    //    Debug.Log($"[HelperWelper] Starting reset for {gameObject.name}");
    //    gameObject.SetActive(true);

    //    isDead = false;

    //    // Stop coroutines and clear references
    //    StopAllCoroutines();
    //    isHolding = false;

    //    // Clear player references to prevent stacked damage
    //    if (grabbedSegments != null)
    //    {
    //        grabbedSegments.bodyType = RigidbodyType2D.Dynamic;
    //        grabbedSegments = null;
    //    }
    //    healthstats = null;

    //    // Reset health
    //    currentHealth = maxHealth;
    //    if (enemyHP != null)
    //    {
    //        enemyHP.gameObject.SetActive(true);
    //        enemyHP.SetHealth(currentHealth, maxHealth);
    //    }

    //    // Reset physics
    //    if (rb != null)
    //    {
    //        rb.constraints = RigidbodyConstraints2D.None;
    //        rb.bodyType = RigidbodyType2D.Dynamic;
    //        rb.simulated = true;
    //        rb.linearVelocity = Vector2.zero;
    //        rb.angularVelocity = 0f;
    //    }

    //    // Reset position (use stored start position)
    //    transform.position = startPos;
    //    transform.rotation = startRot;

    //    // Re-enable collider
    //    if (enemyCollider != null)
    //        enemyCollider.enabled = true;

    //    // Re-enable renderers
    //    foreach (Renderer r in GetComponentsInChildren<Renderer>())
    //        r.enabled = true;

    //    // Re-enable patrol last
    //    if (enemyPatrol != null)
    //        enemyPatrol.enabled = true;


    //    Debug.Log($"[HelperWelper] Reset complete. healthstats is now: {(healthstats == null ? "NULL (GOOD)" : "NOT NULL (BAD!)")}");
    //}
}