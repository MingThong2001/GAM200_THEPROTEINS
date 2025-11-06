using JetBrains.Annotations;
using System.Collections;
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
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private GameObject deathVFX;

    //Checkpoint Storage.
    public int currentSegments;

    //References
    private Rigidbody2D rb;
    public PlayerMovement movement;
    public SpriteShape spriteController;
    public HealthBarUI healthBarUI;     


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        rb = GetComponent<Rigidbody2D>();   
        movement = GetComponent<PlayerMovement>();  

        
        if (healthBarUI == null)
        {
            healthBarUI = FindObjectOfType<HealthBarUI>();
        }

    }

    private void Start()
    {
        InitializePlayer(); //Create player.
    }
    public void DeInitializePlayer() //Destroy Player.
    {
        //Update health value.
        currentHealth = 0;
        if (healthBarUI != null)
        {
            Debug.Log($"[PlayerStats] DeInitializePlayer() forcing HP bar to 0. CurrentHealth={currentHealth}");
            healthBarUI.SetHealth(0, baseMaxHealth);
            Debug.Log($"[PlayerStats] HealthBarUI now showing {healthBarUI.GetCurrentFill()} (if you can expose one)");
        }

        //Disable Movement.
        if (movement != null)
        {
            movement.enabled = false;
        }

        //Freeze any rb.
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        //Disable all the child, colliders components.
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
            Transform tentaclepoint = transform.Find("TentaPoint");
            if (tentaclepoint != null)
                tentaclepoint.gameObject.SetActive(false);
        }

       

    }

    //Create player.
      public void InitializePlayer()
    {
        Debug.Log($"[PlayerStats] === INITIALIZE PLAYER === Health BEFORE: {currentHealth}");
        
        //Reset health to max.
        currentHealth = baseMaxHealth;
        
        Debug.Log($"[PlayerStats] === INITIALIZE PLAYER === Health AFTER: {currentHealth}/{baseMaxHealth}");
        
        //Update HealthbarUI.
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(currentHealth, baseMaxHealth);
            Debug.Log($"[PlayerStats] Health bar updated to {currentHealth}/{baseMaxHealth}");
        }
        else
        {
            Debug.LogWarning("[PlayerStats] healthBarUI is null!");
        }

        //Enable movement.
        if (movement != null)
        {
            movement.enabled = true;
        }

        //Enable RBs.
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }

        //Enable all the childs
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

    }

    //Set health value.
    public void SetHealth(float maxHealth, float currentHealthPercent = -1f) //-1f is use as a flag to get player current health percentage relative to the new max health. 
    {
        if (currentHealthPercent < 0)
        {
            currentHealthPercent = baseMaxHealth > 0 ? currentHealth / baseMaxHealth : 1f;
        }

        baseMaxHealth = maxHealth;  
        currentHealth = baseMaxHealth * Mathf.Clamp01(currentHealthPercent);
    }

   //Take damage value.
    public void TakeDamage(float Damage)
    { 
        currentHealth -= Damage;
        currentHealth = Mathf.Max(0, currentHealth); //Clamp to minimum 0.
        Debug.Log($"[PlayerStats] Took Damage: {Damage}, Current Health: {currentHealth}/{baseMaxHealth}");
        if (healthBarUI != null)
        { 
            healthBarUI.SetHealth(currentHealth, baseMaxHealth);
        }
        playdamagevfx();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            Debug.Log("[PlayerStats] Die() called! CurrentHealth: " + currentHealth);

        }
    }


    //Play VFX.
    private void playdamagevfx()
    {
        if (damageVFX != null)
        {
            GameObject vfx = Instantiate(damageVFX, transform.position, Quaternion.identity);
            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Play();

            Destroy(vfx, ps != null ? ps.main.duration + 0.1f : 1f);


        }
    }

    //Die Logic.
    private void Die()
    {
        
        Debug.Log("[PlayerStats] Die() called");
        //For now no checkpoint is created or implemented

        ////This section is for when a checkpoint is found and active, the player will respawn.
        //CheckPoints lastCheckPoint = null;

        //for (int i = 0; i < CheckPoints.allCheckPoints.Count; i++)
        //{
        //    if (CheckPoints.allCheckPoints[i].isActivated)
        //    {
        //        lastCheckPoint = CheckPoints.allCheckPoints[i];
        //        break;
        //    }

        //}

        //if (lastCheckPoint != null)
        //{
        //    //Respawn from checkpoint
        //    restorefromCheckpoint(lastCheckPoint);
        //    return;
        //}

        //Remove Player from scene.
        DeInitializePlayer();
        StartCoroutine(DeathSequence());
    }

    //Unlock death sequence.
    private IEnumerator DeathSequence()
    {
        //Disable player control.
        if (movement != null)
        {
            movement.enabled = false;
        }

       
        //How long the vfx duration.
        float vfxDuration = 1f;
        if (deathVFX != null)
        {
            //Create VFX.
            GameObject dVFX = Instantiate(deathVFX, transform.position, Quaternion.identity);
            ParticleSystem ps = dVFX.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                vfxDuration = ps.main.duration;
            }

            Destroy(dVFX, vfxDuration + 0.1f);

            yield return new WaitForSeconds(vfxDuration);


            //Trigger death condition.
            if (GameManager.instance != null)
            {
                Debug.Log("[PlayerStats] Calling GameManager.endGame(false)");

                GameManager.instance.endGame(false);
            }



        }



    }

//Checkpoint Logic.
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


    //Health Settings.
    public float GetCurrentHealth()
    { return currentHealth; }

    public float GetMaxHealth()
    {
        return baseMaxHealth;
    }

  
    
}
