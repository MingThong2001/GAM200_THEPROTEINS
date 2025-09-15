using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    public float maxHealth = 100f;
    private float currentHealth;

    //Mass-Related values
    public int currentSegments = 12;
    public int maxSegments = 12;
    public float healthperSegment = 1f;

    public float massperSegment = 1f;
    private Rigidbody2D rb;

    private void Awake()
    {
        if (instance == null)
        { 
            instance = this;
        }

        rb = GetComponent<Rigidbody2D>();
        updateStatsbasedonMess();
    }


    public void updateStatsbasedonMess()
    {
       //Health scales with number of segments.
       currentHealth = maxHealth + (currentHealth * healthperSegment);
        if (rb != null)
        { 
            rb.mass = currentSegments * massperSegment;
        }
    }

    public void AddSegment()
    { 
        currentSegments = Mathf.Min(currentSegments +1, maxSegments);
        updateStatsbasedonMess();
    }

    public void MinusSegment()
    {
        currentSegments = Mathf.Max(currentSegments - 1, 1);
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
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
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

    public float GetCurrentHealth()
    { return currentHealth; }

  
    
}
