using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;

public class MassSegment : MonoBehaviour
{
    #region Mass Stats Settings
    //Mass Configuration
    public int currentSegments;
    public int minSegments;
    public int maxSegments;
    public float massPerSegment = 2f;
   
    //Base Stats
    public float basemoveSpeed = 5f;
    public float baseJump = 1f;
    public float baseHealth = 100f;
    public float calculatedSpeed;
    public float calculatedJump;

    //Mass Modifiers
    [Range(0f, 5f)]
    public float speedmodifierRange = 2.5f;

    [Range(0f, 5f)]
    public float jumpmodifierRange = 0.5f;

    [Range(0f, 2f)]
    public float healthmodifierRange = 0.5f;

    //Stats Update
    public float TotalMass;
    public float currentMoveSpeed;
    public float currentJumpPower;
    public float currentMaxHealth;

    //Door
    public Door masstoDoor;
    #endregion
 
    public int GetCurrentSegments()
    {
        return currentSegments;
    }
    public int GetMinSegments()
    {
        return minSegments;
    }
    public int GetMaxSegments()
    {
        return maxSegments;
    }

    public float MassRatio()
    {
        //return Mathf.Clamp01((float)(currentSegments - minSegments) / (maxSegments - minSegments));
        return (currentSegments - minSegments) / (maxSegments - minSegments);
    }

    //Modified Stats
    //
    public void Updatemovespeed()
    {
        calculatedSpeed = basemoveSpeed * (1f - MassRatio() * speedmodifierRange);
        currentMoveSpeed = calculatedSpeed;
    }

    public void Updatejumppower()
    {
        //currentJumpPower = baseJump * (1f * MassRatio() * jumpmodifierRange);
        //calculatedJump = baseJump  * (1f +
        //    MassRatio() * jumpmodifierRange);
        //currentJumpPower = calculatedJump;

        calculatedJump = baseJump + jumpmodifierRange * (currentSegments - 1);
        currentJumpPower = calculatedJump;
        //currentJumpPower = Mathf.Clamp(calculatedJump, baseJump, baseJump * (1f + jumpmodifierRange));
    }

    public void Updatemaxhealth()
    {
        currentMaxHealth = baseHealth * (1f + MassRatio() * healthmodifierRange);
    }
    

    public List <Projectile> ProjectileSegments = new List<Projectile> ();

    //Reference to components
    public PlayerMovement playermovement;
    public PlayerStats playerstats;
    private Rigidbody2D rb;
    private Rigidbody2D[] allrigidbodies;
    private AudioManager audioManager;

    public HealthBarUI HealthbarUI;
    public int savedSegment;


    //Reference
    private Projectile projectilePrefab;
    public void Awake()
    {
      
   
        if (playerstats == null)
        {
            playerstats = GetComponent<PlayerStats>();
        }
        if (playermovement == null)
        {
            playermovement = GetComponent<PlayerMovement>();
        }
        rb = GetComponent<Rigidbody2D>();
        allrigidbodies = GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rb in allrigidbodies)
        {
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            }

        }
        UpdateAllStats();
    }

    private void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    
    }
    public void handlepickupCollision(Collider2D coll)
    {

        Debug.Log("handlepickupCollision triggered!");

        gloopickup pickup = coll.GetComponentInParent<gloopickup>();
        //if (pickup == null || pickup.isBeingProcessed) return;
        //pickup.isBeingProcessed = true;
        Debug.Log("Pickup is: " + pickup);
        Debug.Log("Is Pickup Being Processed? " + pickup?.isBeingProcessed);

        
        bool processed = false;

            Debug.Log("Pickup is not being processed, continuing.");

            Debug.Log("Can add segment: " + canaddSegment());
            Debug.Log("Can remove segment: " + canremoveSegment());
            if (pickup.isAdditive)
            {
                if (canaddSegment() && processed == false)
                {
                    Debug.Log("Add Segments.");
                    processed = true;
                    AddSegment(pickup.segmentAmount);
                    pickup.CollectPickup();
                    

                }
              
            }

    }
    #region Segment Count

    public bool canaddSegment()
    {
        if (currentSegments < maxSegments)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool canremoveSegment()
    {
        if (currentSegments > minSegments)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddSegment(int amount = 1)
    {
        int oldSegment = currentSegments;

        Debug.Log($"[AddSegment] Current: {oldSegment}, Adding: {amount}"); 
        int newsegmentCount = Mathf.Min(maxSegments, oldSegment + amount);
        if (newsegmentCount != oldSegment)
        {
            currentSegments = newsegmentCount;
            playerstats.currentSegments = newsegmentCount;

            savedSegment = currentSegments;
            Debug.Log($"Segment Added! New segment count: {currentSegments}");

            UpdateAllStats();
            LogStats("AddSegment");

            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.pickup);
            }
        }
        ChangeVolume ch = GetComponent<ChangeVolume>();

        ch.AdjustFirePointDistance(1.5f);
        ch.Change(1.2f);


  }
    public void RemoveSegment(int amount)
    {
        int oldSegment = currentSegments;
        Debug.Log($"[RemoveSegment] Current: {oldSegment}, Removing: {amount}");

        int newsegmentCount = Mathf.Max(minSegments, oldSegment - amount);
        if (newsegmentCount != oldSegment)
        {
            currentSegments = newsegmentCount;
            playerstats.currentSegments = newsegmentCount;

            savedSegment = currentSegments ;
            Debug.Log($"Segment Added! New segment count: {currentSegments}");
            UpdateAllStats();
            LogStats("RemoveSegment");
        }
        ChangeVolume ch = GetComponent<ChangeVolume>();
        ch.Change(0.8f);
        ch.AdjustFirePointDistance(0.5f);
        //CheckdoorMass();
    }
    public void SetSegmentCount(int count)
    {
        int newsegmentCount = Mathf.Clamp(count, minSegments, maxSegments);
        if (newsegmentCount != currentSegments)
        {
            currentSegments = newsegmentCount;
            UpdateAllStats();
        }
    }
    #endregion

    public void CollectProjectile(Projectile projectilePrefab)
    {

        if (projectilePrefab == null) return;
        //projectile.returntoprojectilePool();
        AddSegment(projectilePrefab.segmentAmount);
        Destroy(projectilePrefab.gameObject); 
    }


    #region Stats Update
    public void UpdateAllStats()
    {
        Updatemaxhealth();
        Updatejumppower();
        Updatemovespeed();

        UpdatemovementStats();
        UpdatehealthStats();
        UpdatephysicsMass();
        UpdateUi();

        TotalMass = GetTotalMass();
    }

    private void UpdatemovementStats()
    {
        if (playermovement != null)
        {
            playermovement.normalmovementSpeed = currentMoveSpeed;
            playermovement.puddlemovementSpeed = currentMoveSpeed * 0.4f;
            //playermovement.jumpforce = currentJumpPower;
        }
    }

    private void UpdatehealthStats()
    {
        if (playerstats != null)
        {
            float healthPercentage = playerstats.GetMaxHealth() > 0 ? playerstats.GetCurrentHealth() / playerstats.GetMaxHealth() : 1f;
            playerstats.SetHealth(currentMaxHealth, healthPercentage);

            if (HealthbarUI != null)
            {
                HealthbarUI.SetHealth(playerstats.GetCurrentHealth(), playerstats.GetMaxHealth());
            }

        }

    }
  
    public void UpdatephysicsMass()
    {
        foreach (Rigidbody2D rb in allrigidbodies)
        {
            if (rb != null)
            {
                rb.mass = massPerSegment;

            }
        }
    }

    public void UpdateUi()
    {
        if (HealthbarUI != null && playerstats != null)
        {
            HealthbarUI.SetHealth(playerstats.GetCurrentHealth(), playerstats.GetMaxHealth());
        }

    }

    public float GetTotalMass()
    {
        return currentSegments * massPerSegment;
    }

    //private void UpdateCheckpoint(int newsegmentCount)
    //{
    //    CheckPoints checkpoint = GameObject.FindGameObjectWithTag("CheckPoints").GetComponent<CheckPoints>();
    //    if (checkpoint != null && checkpoint.isActivated)
    //    {
    //        checkpoint.UpdateCheckPointSegments(newsegmentCount);

    //    }

    //}


        #endregion


        //    #region Pickup
        //    private void OnTriggerEnter2D(Collider2D slimepickup)
        //    {
        //        Debug.Log("Trigger Enter this: " + slimepickup.gameObject.name);
        //        if (slimepickup.CompareTag("GlooSegment"))
        //        {
        //            gloopickup pickup = slimepickup.GetComponent<gloopickup>();
        //            if (pickup != null)
        //            { 
        //                pickupProcess(pickup);
        //            }
        //        }
        //    }

        //    private void pickupProcess(gloopickup pickup)
        //    {
        //        if (pickup.isAdditive && canaddSegment())
        //        {
        //            AddSegment(pickup.segmentAmount);
        //            pickup.CollectPickup();
        //        }
        //        else if (!pickup.isAdditive && canremoveSegment())
        //        {
        //            RemoveSegment(pickup.segmentAmount );
        //            pickup.CollectPickup(); 

        //        }
        //        Destroy(gameObject);

        //    }



        //#endregion

        //}
    private void LogStats(string action)
    {
        //Debug.Log($"[{ action}] Segments: {currentSegments}, " + $"MaxHealth: {currentMaxHealth}, " + 
            //$"PlayerHealth: {playerstats.GetCurrentHealth()}, " + $"MoveSpeed: {playermovement.normalmovementSpeed}, " + $"JumpForce: {playermovement.jumpforce}");
    
    }

    private void UpdateCheckpoint(int newSegmentCount)
    {
        // Find the active checkpoint and update its segment count
        CheckPoints checkpoint = GameObject.FindGameObjectWithTag("CheckPoints")?.GetComponent<CheckPoints>();
        if (checkpoint != null && checkpoint.isActivated)
        {
            checkpoint.UpdateCheckPointSegments(newSegmentCount);
            Debug.Log($"Checkpoint segment count updated to {newSegmentCount}");
        }
        else
        {
            Debug.LogWarning("No active checkpoint found or checkpoint is not activated.");
        }
    }
    public void CheckdoorMass() 
    {
        if (masstoDoor != null && !masstoDoor.isUnlocked)
        {
            float totalMass = GetTotalMass();
            if (totalMass >= masstoDoor.minMassThreshold && totalMass <= masstoDoor.maxMassThreshold)
            {
                masstoDoor.UnlockedDoor();
            }
        }

    }
}
