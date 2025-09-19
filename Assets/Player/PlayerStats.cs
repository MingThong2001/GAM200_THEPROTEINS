using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{


    /* Die/Respawn logic not correct, not tied to the checkpoint yet.*/
    public static PlayerStats instance;

    //Health
    public float maxHealth = 100f;

    public int lives = 5;
    private float currentHealth;



    //Mass-Related values
    public int currentSegments = 12;
    public int maxSegments = 12;
    public float healthperSegment = 10f;
    public float massperSegment = 1f;

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
        currentHealth = maxHealth;

        updateStatsbasedonMess();
    }

   

    public void updateStatsbasedonMess()
    {
       
       currentHealth = maxHealth + (currentHealth * healthperSegment);
        if (rb != null)
        { 
            rb.mass = currentSegments * massperSegment;
        }
    }

    //Add a segment to the player ensuring we do not exceed the max segments.
    public void AddSegment()
    { 
        currentSegments = Mathf.Min(currentSegments +1, maxSegments);

        //Give bonus health when gaining a segment
        currentHealth  = Mathf.Min(currentHealth + healthperSegment, maxHealth);
        updateStatsbasedonMess();
    }

    public void MinusSegment()
    {
        currentSegments = Mathf.Max(currentSegments - 1, 1);

        //Reduce health
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        updateStatsbasedonMess();

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
        updateStatsbasedonMess();

    }
    public float GetCurrentHealth()
    { return currentHealth; }

  
    
}
