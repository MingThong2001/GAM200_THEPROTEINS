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

    private Transform player;
    private ChargePatrol chargePatrol;


    public void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        { 
            player = p.transform;
        }

        chargePatrol = GetComponentInParent<ChargePatrol>();
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
            Debug.Log($"[HelperWelper] Charging towards player. Position: {transform.position}");

            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeDuration)
            {
                isCharging = false;
                chargeTimer = 0f;
                Debug.Log("[HelperWelper] Charge ended.");
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

        if (Time.time - lastHitTime < cooldown) return;
        lastHitTime = Time.time;

        // Damage
        PlayerStats stats = col.GetComponentInParent<PlayerStats>();
        if (stats != null)
            stats.TakeDamage(contactDamage);

        // Knockback
        Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 knockDir = (col.transform.position - transform.position).normalized;
            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
