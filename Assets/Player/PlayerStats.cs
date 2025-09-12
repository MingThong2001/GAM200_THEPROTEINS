/*using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    public float maxHealth = 100f;
    private float currentHealth;

    public float baseSpeed = 5f;
    public float baseJumpHeight = 5f;

    //Mass-Related values
    public int maxSegments 

    public void Start()
    {
       currentHealth = basemaxHealth;   
       rb = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
        updatestatsonMass();
        updateSpring
    }


    private void chargeSquishyEnergy()
    {
        if (currentSquishyEnergy < maxSquishyEnergy)
        {
            currentSquishyEnergy += squishyEnergyRechargerate * Time.deltaTime;
            if (currentSquishyEnergy > maxSquishyEnergy)
            { 
                currentSquishyEnergy = maxSquishyEnergy;
            }
        }
    }
    
  public  void UpdateHealth()
    {
    }
    public void TakeDamage(float Damage)
    {
       currentHealth -= Damage;
       if (currentHealth <= 0f)
        { 
            currentHealth = 0f;
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


        public void resetStats()
        {
//currentHealth = maxHealth;
            currentSquishyEnergy = 0;

        }

       
    
}*/
