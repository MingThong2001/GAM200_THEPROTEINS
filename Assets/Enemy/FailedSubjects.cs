using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FailedSubjects : MonoBehaviour
{

    //Erratic Charge
    public bool isCharging = false;
    public float chargeTimer = 0f;
    public float chargeSpeed = 0f;
    public float lastchargeTime = -Mathf.Infinity;
    public float chargeDuration = 0.5f;
    public float chargeCooldown = 2f;
    public float maxAngleOffset = 30f;

    private Vector3 chargeDirection;
    //Knockback
    public float contactDamage = 10f;    // Damage applied
    public float knockbackForce = 10f;   // Force applied
    public float cooldown = 1f;          // Time between knockbacks

    private float lastHitTime = 0f;

    //Health
    public float maxHealth = 20f;
    private float currentHealth;
    private EnemyHPUI enemyHP;


    private Transform player;
    public ChargePatrol chargePatrol;


    //[SerializeField] private Transform spawnPoint;

    //spawn
    private Vector3 startPos;
    private void Awake()
    {
      
    }
    public void Start()
    {
        currentHealth = maxHealth;

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        { 
            player = p.transform;
        }
        chargePatrol = GetComponentInParent<ChargePatrol>();

        startPos = transform.position;

        if (chargePatrol != null)
        {
            chargePatrol.SpawnAtPointFailedSubject();
        }

        enemyHP = GetComponentInChildren<EnemyHPUI>();
        if (enemyHP != null)
        { 
            enemyHP.SetHealth(currentHealth,maxHealth);
        }
    }
    private void Update()
    {
       // if (enemyPatrol.isPaused) return;
      
        if (player == null || chargePatrol == null) return;

        HandleCharge();
    }


    private void HandleCharge()
    {
        if (isCharging)
        {
            //Vector3 direction = (player.position - transform.position).normalized;
            float wobble = Random.Range(-5f, 5f);
            chargeDirection = Quaternion.Euler(0,0,wobble) * chargeDirection;
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            Debug.Log($"[FailedSubjects] Charging towards player. Position: {transform.position}");

            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeDuration)
            {
                isCharging = false;
                chargeTimer = 0f;
                Debug.Log("[FailedSubjects] Charge ended.");
                if (chargePatrol != null)
                {
                    chargePatrol.chargeVelocity = Vector3.zero;
                }
            }

        }
        else
        {
            if (chargePatrol.PlayerInSight() && Time.time - lastchargeTime >= chargeCooldown)
            {
                
                lastchargeTime = Time.time;
                isCharging = true;
                chargeTimer = 0f;

                Vector3 directiontoplayer = (player.position - transform.position).normalized;  
                float angleoffset = Random.Range(-maxAngleOffset, maxAngleOffset);
                chargeDirection = Quaternion.Euler(0,0, angleoffset) * directiontoplayer;

                chargeSpeed = Random.Range(chargeSpeed * 0.7f, chargeSpeed * 1.3f);
                Debug.Log($"[FailedSubjects] Charge started. Speed: {chargeSpeed:F2}, Direction: {chargeDirection}");

                if (chargePatrol != null)
                {
                    chargePatrol.chargeVelocity = chargeDirection * chargeSpeed;

                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        //if (!col.name.Contains("Player")) return;

        if (Time.time - lastHitTime < cooldown) return;
        lastHitTime = Time.time;

        // Damage
        PlayerStats stats = col.GetComponentInParent<PlayerStats>();
        if (stats != null)
            stats.TakeDamage(contactDamage);

        // Knockback
        Rigidbody2D rb = col.GetComponentInParent   <Rigidbody2D>();
        if (rb != null)
        {
            Vector2 knockDir = (col.transform.position - transform.position).normalized;
            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    //This is to ignroe projectile if the projectile is collectible.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Projectile proj = collision.gameObject.GetComponentInParent<Projectile>();
        if (proj != null && proj.canBeCollected)
        {
            // Ignore physics interactions with all body parts
            Collider2D enemyCollider = GetComponent<Collider2D>();

            if (proj.bodyParts != null)
            {
                foreach (Rigidbody2D rb in proj.bodyParts)
                {
                    if (rb == null) continue;

                    Collider2D[] cols = rb.GetComponents<Collider2D>();
                    foreach (Collider2D col in cols)
                    {
                        Physics2D.IgnoreCollision(col, enemyCollider, true);
                    }
                }
            }

            return; // Stop further interaction
        }

//        // Flying projectiles still hit normally
//        if (proj != null && !proj.canBeCollected)
//        {
//<<<<<<< Updated upstream
//            //proj.HandleEnemyHit(collision); //from GetComponent to collision, hope it works
//=======
//            proj.HandleEnemyHit(collision.collider); 
//>>>>>>> Stashed changes
//        }
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
