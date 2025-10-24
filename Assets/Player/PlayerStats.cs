using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    //Health
    public float baseMaxHealth = 100f;
    public float currentHealth;

    //Checkpoint
    public int currentSegments;

    //References
    private Rigidbody2D rb;
    public PlayerMovement movement;
    public SpriteShape spriteController;
    public HealthBarUI healthBarUI;     


    private void Awake()
    {
        if (instance == null)
        { 
            instance = this;
        }
        rb = GetComponent<Rigidbody2D>();   
        movement = GetComponent<PlayerMovement>();  

        
        if (healthBarUI == null)
        {
            healthBarUI = FindObjectOfType<HealthBarUI>();
        }

    }

    private void Start()
    {
        InitializePlayer();
    }
    public void DeInitializePlayer()
    {

        currentHealth = 0;

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        foreach (Transform child in transform)
        { 
            Renderer playerRenderer = child.GetComponentInChildren<Renderer>();   
            if (playerRenderer != null)
            { playerRenderer.enabled = false; }

            Collider2D playersCols = child.GetComponentInChildren<Collider2D>();
            if (playersCols != null)
            { 
                playersCols.enabled = false;
            }
        }

        // Reset health bar UI
        if (healthBarUI != null)
            healthBarUI.SetHealth(0, baseMaxHealth);

    }
      public void InitializePlayer()
    {
        Debug.Log($"[PlayerStats] === INITIALIZE PLAYER === Health BEFORE: {currentHealth}");
        
        // CRITICAL: Reset health to max
        currentHealth = baseMaxHealth;
        
        Debug.Log($"[PlayerStats] === INITIALIZE PLAYER === Health AFTER: {currentHealth}/{baseMaxHealth}");
        
        // Update health bar UI - FIXED: pass baseMaxHealth, not 1f!
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(currentHealth, baseMaxHealth);
            Debug.Log($"[PlayerStats] Health bar updated to {currentHealth}/{baseMaxHealth}");
        }
        else
        {
            Debug.LogWarning("[PlayerStats] healthBarUI is null!");
        }

        if (movement != null)
        {
            movement.enabled = true;
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }

        foreach (Transform child in transform)
        {
            Renderer playerRenderer = child.GetComponentInChildren<Renderer>();
            if (playerRenderer != null)
            { playerRenderer.enabled = true; }

            SpriteShapeRenderer spriteShapeRenderer = child.GetComponentInChildren<SpriteShapeRenderer>();
            if (spriteShapeRenderer != null)
                spriteShapeRenderer.enabled = true;

            Collider2D playersCols = child.GetComponentInChildren<Collider2D>();
            if (playersCols != null)
            {
                playersCols.enabled = true;
            }
        }

        GameManager.instance.SpawnPlayer();
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
        currentHealth = Mathf.Max(0, currentHealth); //Clamp to minimum 0.
        Debug.Log($"[PlayerStats] Took Damage: {Damage}, Current Health: {currentHealth}/{baseMaxHealth}");
        if (healthBarUI != null)
        { 
            healthBarUI.SetHealth(currentHealth, baseMaxHealth);
        }
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    private void Die()
    {
        Debug.Log("[PlayerStats] Die() called");
        
        //This section is for when a checkpoint is found and active, the player will respawn.
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
            //Respawn from checkpoint
            restorefromCheckpoint(lastCheckPoint);
            return;
        }
        
        //This section is if no checkpoint is found, the player will die.
        
        //Remove Player from scene.
        DeInitializePlayer();

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

        //Trigger death condition
        if (GameManager.instance != null)
        {
            Debug.Log("[PlayerStats] Calling GameManager.endGame(false)");

            GameManager.instance.endGame(false);
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

        //Restore segments.
        MassSegment masssegment = GetComponent<MassSegment>();

        if (masssegment != null)
        {
            masssegment.SetSegmentCount(currentSegments);
        }

        // Re-enable movement
        if (movement != null)
            movement.enabled = true;

        // Unfreeze Rigidbody
        if (rb != null)
            rb.constraints = RigidbodyConstraints2D.None;

        // Re-enable all child renderers and colliders
        foreach (Transform child in transform)
        {
            Renderer rend = child.GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = true;

            Collider2D col = child.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;
        }

        // Update health bar
        if (healthBarUI != null)
            healthBarUI.SetHealth(currentHealth, baseMaxHealth);


    }

    
    public float GetCurrentHealth()
    { return currentHealth; }

    public float GetMaxHealth()
    {
        return baseMaxHealth;
    }

  
    
}
