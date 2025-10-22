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

    //Health
    public float maxHealth = 20f;
    private float currentHealth;
    private EnemyHPUI enemyHP;

   

    //spawn
    private Vector3 startPos;
    public void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"[Start] {name} currentHealth: {currentHealth}, maxHealth: {maxHealth}");

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

        //Store Helper's start position.
        startPos = transform.position;

        //Spawn the enemy at the designated spawn point.
        if (enemyPatrol != null)
        {
            enemyPatrol.SpawnAtPointHelperWelper();
        }
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
        if (isHolding) yield break; //This is to prevent double grab.
        isHolding = true;
        grabbedSegments = targetRb;

        // Get player health
        healthstats = targetRb.GetComponentInParent<PlayerStats>();
        if (healthstats != null)
        {
            healthstats.TakeDamage(grabDamage); // Damage immediately when grabbed
            Debug.Log($"Drone dealt {grabDamage} damage to {healthstats.name} at time {Time.time}");
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
        Vector2 throwDirection = new Vector2(throwdirectionX, 2f);
        grabbedSegments.AddForce(throwDirection * (throwForce + 10f), ForceMode2D.Impulse);

        //Clear refernece to prevent double grab.
        grabbedSegments = null;

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
        if (enemyHP != null)
            Destroy(enemyHP.gameObject);
        Destroy(gameObject);
    }
}