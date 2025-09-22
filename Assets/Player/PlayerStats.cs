using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{


    /* Die/Respawn logic not correct, not tied to the checkpoint yet.*/
    public static PlayerStats instance;

    //Health
    public float baseMaxHealth = 100f;
    public int lives = 5;
    private float currentHealth;

    //Checkpoint
    public int currentSegments;
    //References
    private Rigidbody2D rb;
    public PlayerMovement movement;
    


    private void Awake()
    {
        if (instance == null)
        { 
            instance = this;
        }
        rb = GetComponent<Rigidbody2D>();   
        movement = GetComponent<PlayerMovement>();  
        currentHealth = baseMaxHealth;

      
    }

  
    public void SetHealth(float maxHealth, float currentHealthPercent = -1f)
    {
        if (currentHealthPercent < 0)
        {
            currentHealthPercent = baseMaxHealth > 0 ? currentHealth / baseMaxHealth : 1f;
        }

        baseMaxHealth = maxHealth;  
        currentHealth = baseMaxHealth * Mathf.Clamp01(currentHealthPercent);
    }

    
    public void TakeDamage(float Damage)
    { 
        currentHealth -= Damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    private void Die()
    {
        CheckPoints lastCheckPoint = null;

        for (int i = 0; i < CheckPoints.allCheckPoints.Count; i++)
        {
            if (CheckPoints.allCheckPoints[i].isActivated)
            { 
                lastCheckPoint = CheckPoints.allCheckPoints[i];
                break;
            }
            
        }

        if (lastCheckPoint != null)
        {
            restorefromCheckpoint(lastCheckPoint);
        }
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

        }
        PlayerMovement playermovement = GetComponent<PlayerMovement>();
        if (playermovement != null)
        {
            playermovement.enabled = false;
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        AudioSource audiosource = GetComponent<AudioSource>();
        if (audiosource != null)
        {
            // audiosource.PlayOneShot(deathSound);
        }
    }

    public void restorefromCheckpoint(CheckPoints checkpoint)
    {
        if (checkpoint == null)
        {
            return;
        }

        currentHealth = checkpoint.savedHealth;
        currentSegments = checkpoint.savedSegment;
        transform.position = checkpoint.savedPosition;

        MassSegment masssegment = GetComponent<MassSegment>();

        if (masssegment != null)
        {
            masssegment.SetSegmentCount(currentSegments);
        }

    }
    public float GetCurrentHealth()
    { return currentHealth; }

    public float GetMaxHealth()
    {
        return baseMaxHealth;
    }

  
    
}
