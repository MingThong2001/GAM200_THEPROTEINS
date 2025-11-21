using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HelperWelper : MonoBehaviour
{
    [Header("Grab Settings")]
    public Transform grabPoint; 
    public float grabRange = 2f;
    public float grabDuration = 2f;
    public float throwForce = 20f;
    private bool isHolding = false;
    public float grabDamage = 15f;

    [Header("References Settings")]
    private Rigidbody2D grabbedPointA;
    private Rigidbody2D grabbedPointB;
    public EnemyPatrol enemyPatrol;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;

    [Header("Health Settings")]
    public float maxHealth = 20f;
    private float currentHealth;
    private EnemyHPUI enemyHP;

    [Header("Spawn Settings")]
    private Vector3 startPos;
    private Quaternion startRot;
    private bool isDead = false;


   // [SerializeField] private GameObject deathVFX;
 //   [SerializeField] private GameObject damageVFX;
  //  [SerializeField] private Animator animator;
    public ParticleSystem impactVFX;
    private ParticleSystem activeImpactVFX;

    public void Awake()
    {
        //Initialization of relevant component.
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        //Get the enemy HP and set it.
        enemyHP = GetComponentInChildren<EnemyHPUI>();
        if (enemyHP != null)
        {
            enemyHP.SetHealth(currentHealth, maxHealth);
        }
    }

    private void Start()
    {

        //Get patrol script and collider.
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
        enemyCollider = GetComponent<Collider2D>();

        if (enemyPatrol != null)
        {
            enemyPatrol.SpawnAtPointHelperWelper();
        }

        startPos = transform.position; //Store the starting position of the enemy.
        startRot = transform.rotation; //Store the starting rotation of the enemy.
    }

    private void OnTriggerStay2D(Collider2D other)
    {

        if (isHolding) //If the enmy already holding the player skip this mehtod.
        {
            return;
        }

        //Skip if the object is projectile. Projectile has the same tag as player.
        if (other.GetComponentInParent<Projectile>() != null)
        {
            return;
        }

        //Skip if the root object does not contain the string "Player".
        if (!other.transform.root.name.Contains("Player"))
        {
            return;
        }

        Rigidbody2D rb = other.attachedRigidbody; //Get the rigidbody attached to the player.
        if (rb == null) //If no rigidbody found, skip.
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, rb.position); //Calcykate teh distance form enemy to th eplayer.
        Debug.Log($"Distance: {distance}, grabRange: {grabRange}");

        if (distance <= grabRange) // If the player is whint the grab range.
        {
            Debug.Log(" STARTING GRAB!");
            StartCoroutine(GrabHoldThrow(rb)); //Start the grab coroutine.
        }
    }

    private IEnumerator GrabHoldThrow(Rigidbody2D detectedRb)
    {
        Debug.Log("=== GrabHoldThrow STARTED ===");

        if (isHolding) //If is already holding, exit teh coroutine.
        {
            yield break;
        }

        if (grabPoint == null) //If no grab point, exit.
        {
            yield break;
        }

        isHolding = true; //Set the grab state flag to true.

        //Find ALL player rigidbodies because player has multiple rb.
        Transform playerRoot = detectedRb.transform.root;
        Rigidbody2D[] allPlayerSegments = playerRoot.GetComponentsInChildren<Rigidbody2D>();


        //Take all the player segments, calculate how far each one is from the grab point, sort them from closest to farthest, and put the result into a list.
        var sortedByDistance = allPlayerSegments
            .OrderBy(segment => Vector2.Distance(grabPoint.position, segment.position))
            .ToList();

        if (sortedByDistance.Count < 2) //If there are fewer than 2 points, retturn.
        {
            isHolding = false;
            yield break;
        }

        grabbedPointA = sortedByDistance[0]; //Closest
        grabbedPointB = sortedByDistance[1]; //Second closest

       

        //Deal damage to th eplayer.
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.TakeDamage(grabDamage);
            //Debug.Log($"HelperWelper dealt {grabDamage} damage to player at time {Time.time}");
        }

        //Stop enemy patrol.
        if (enemyPatrol != null) enemyPatrol.enabled = false;

        //Freeze enemy so it does not move while holding the player.
        RigidbodyType2D originalbodyType = rb.bodyType;
        bool wasSimulated = rb.simulated;
        rb.bodyType = RigidbodyType2D.Kinematic; //Stop all physics.
        rb.linearVelocity = Vector2.zero; //REmove moving..
        rb.angularVelocity = 0f; //Remove rotation.
        rb.simulated = false; //Disbale simulation

        //Store original states of the two grab points.
        RigidbodyType2D originalTypeA = grabbedPointA.bodyType;
        RigidbodyType2D originalTypeB = grabbedPointB.bodyType;

        //Calculate distance between the two grabbed points to maintain spacing.
        float originalDistance = Vector2.Distance(grabbedPointA.position, grabbedPointB.position);
        Vector2 originalDirection = (grabbedPointB.position - grabbedPointA.position).normalized;

        //Freeze ONLY the two grabbed points.
        grabbedPointA.bodyType = RigidbodyType2D.Kinematic;
        grabbedPointA.linearVelocity = Vector2.zero;
        grabbedPointA.angularVelocity = 0f;

        grabbedPointB.bodyType = RigidbodyType2D.Kinematic;
        grabbedPointB.linearVelocity = Vector2.zero;
        grabbedPointB.angularVelocity = 0f;

        //Debug.Log($"Holding for {grabDuration} seconds...");

        // Hold the player for the grab duration, maintaining the distance and direction.
        float timer = 0f;
        
        //While loop yippie.
        while (timer < grabDuration)
        {
            // Keep point A at the grab point position.
            grabbedPointA.position = grabPoint.position;

            // Keep point B relative to A
            grabbedPointB.position = (Vector2)grabbedPointA.position + originalDirection * originalDistance;

            timer += Time.deltaTime; //Increment the timer.
            yield return null;
        }

        //Debug.Log("Hold complete, releasing and throwing...");

        //Restore the points bodytybe to dynamic.
        grabbedPointA.bodyType = originalTypeA;
        grabbedPointB.bodyType = originalTypeB;

        //Apply throw force to detected rigidbody (usually player root)
        float throwdirectionX = Mathf.Sign(transform.localScale.x) * 2f; //Determine whether the enemy is facing left and right then multiply by 2. JUST IN CASE, CUZ IT CANT FLIP SOMETIMES. IDK WHY.
        Vector2 throwDirection = new Vector2(throwdirectionX, 3f); //Create a direction vector for the throw. 3f is a vertical lift.
        detectedRb.AddForce(throwDirection * (throwForce + 10f), ForceMode2D.Impulse); //Applies instant force to the player rb.

        //Debug.Log($"[HelperWelper] Threw player with force {throwDirection * (throwForce + 10f)}");

        //Clear references to prepare the next grab.
        grabbedPointA = null;
        grabbedPointB = null;

        //Wait before restoring enemy physics.
        yield return new WaitForSeconds(0.5f);

        //Restore enemy physics.
        rb.simulated = wasSimulated;
        rb.bodyType = originalbodyType;

        //Resume patrol
        if (enemyPatrol != null)
            enemyPatrol.enabled = true;

        isHolding = false;
        //Debug.Log("=== GrabHoldThrow COMPLETE ===");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore ground collisions entirely
        if (collision.gameObject.CompareTag("groundLayer"))
        {
            return;
        }

        Projectile proj = collision.gameObject.GetComponentInParent<Projectile>();
        if (proj == null) return;

        // Check if projectile is moving fast enough (actively flying) for VFX
        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
        bool isFastMoving = (projRb != null && projRb.linearVelocity.magnitude >= 2f);

        // Spawn VFX only for fast-moving projectiles
        if (isFastMoving && impactVFX != null)
        {
            Vector3 hitPoint = collision.GetContact(0).point;
            ParticleSystem vfx = Instantiate(impactVFX, hitPoint, Quaternion.identity);
            vfx.Play();
            Destroy(vfx.gameObject, vfx.main.duration + vfx.main.startLifetime.constantMax);
        }

        // ALWAYS ignore collision physics for collectible projectiles (regardless of velocity)
        if (proj.canBeCollected)
        {
            Collider2D enemyCollider = GetComponent<Collider2D>();
            Collider2D[] projectileColliders = collision.gameObject.GetComponents<Collider2D>();
            foreach (Collider2D col in projectileColliders)
            {
                Physics2D.IgnoreCollision(col, enemyCollider, true);
            }
            return;
        }
    }
    public void TakeDamage(float damage)
    {
        //Debug.Log($"[DEBUG] {name} currentHealth BEFORE damage: {currentHealth}");
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        //Debug.Log($"[DEBUG] {name} took {damage} damage. Remaining health: {currentHealth}");

        if (enemyHP != null)
            enemyHP.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
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
}